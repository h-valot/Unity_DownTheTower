using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Rope : Permanent
{
	#region REFERENCES

	[FoldoutGroup("Internal references")][SerializeField] private BoxCollider m_boxCollider;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_ropeAttach;
	[FoldoutGroup("Internal references")][SerializeField] public Transform RaycastTarget;
	[FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_previewMeshRendered;
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_previewGameObject;
	[FoldoutGroup("Internal references")][SerializeField] private ConfigurableJoint m_joint;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterLastPosition m_rsoCharacterLastPosition;

	#endregion

	#region VARIABLES

	private bool m_isPlaced;
	[ShowInInspector] private float m_holdLength;
	private List<Vector3> m_folds = new List<Vector3>();
	private List<Vector3> m_foldRaycastPositions = new List<Vector3>();
	private Vector3[] m_foldValidPositions = new Vector3[] { };
	private Rigidbody m_characterRigidbody;
	private Transform m_characterHarness;
	private SoftJointLimit m_linearLimit;

	[ShowInInspector] public bool IsConstrained;
	public Action OnAttached;
	public Action OnDetached;
	public Action OnLimitReached;
	public bool IsConnected => m_characterRigidbody;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
	public Rigidbody CharacterRigidbody => m_characterRigidbody;
	public List<Vector3> Folds => m_folds;
	public Vector3 CharacterPosition
	{
		get
		{
			if (m_characterHarness) return m_characterHarness.position;
			else return CurrentFold;
		}
	}
	public Vector3 CurrentFold 
	{
		get 
		{
			if (m_folds.Count >= 1) return m_folds[^1];
			else return Vector3.zero;
		}
	}
	public Vector3 LastFold 
	{ 
		get 
		{ 
			if (m_folds.Count >= 2) return m_folds[^2];
			else return CurrentFold;
		}
	}

	#endregion

	#region MONOBEHAVIOR

	private void Update()
	{
		// Assertions
		if (!IsConnected) return;
		if (!m_isPlaced) return;

		AddFolds();
		RemoveFolds();
		HandleJoint();
	}

	private void OnEnable()
	{
		m_rsoRopes.value.Add(this);
	}

	private void OnDisable()
	{
		m_rsoRopes.value.Remove(this);
	}

	#endregion

	#region PERMANENT

	public override void InitializePreview()
	{
		m_previewGameObject.SetActive(true);
		m_previewGameObject.transform.rotation = Quaternion.identity;
	}

	public override void PreviewThrow(Transform cameraTransform)
	{
		if (Physics.Raycast(
			cameraTransform.position, 
			GetPositionRayDirection(cameraTransform, m_ssoRope.CameraOffsetAngle, m_ssoRope.MaxCameraDownwardClamp), 
			out var hitInfo, 
			m_ssoRope.MaxDistFromCamera, 
			~m_ssoRope.NoRaycastLayer))
		{
			if (!m_previewGameObject.activeInHierarchy)
			{
				m_previewGameObject.SetActive(true);
			}

			// Update preview position
			m_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + m_ssoRope.HeightLimit / 2, hitInfo.point.z);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ssoRope.HeightLimit, ~m_ssoRope.NoCollisionNoRaycastLayer) 
				&& !IsSpaceAround(hitInfo, cameraTransform, m_ssoRope.MinRadiusAround, ~m_ssoRope.NoCollisionNoRaycastLayer)
			);
		}
		else
		{
			UpdateColor(isDeployable: false);

			if (m_previewGameObject.activeInHierarchy) 
			{
				m_previewGameObject.SetActive(false);
			}
		}
    }


    private void UpdateColor(bool isDeployable)
	{
		m_previewMeshRendered.material.SetFloat("_colorSwitch", isDeployable ? 0f : 1f);
	}

	public override bool Throw(Transform cameraTransform)
	{
		m_previewGameObject.SetActive(false);

		if (Physics.Raycast(
			cameraTransform.position,
			GetPositionRayDirection(cameraTransform, m_ssoRope.CameraOffsetAngle, m_ssoRope.MaxCameraDownwardClamp),
			out var hitInfo,
			m_ssoRope.MaxDistFromCamera,
			~m_ssoRope.NoRaycastLayer))
		{
			if (IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ssoRope.HeightLimit, ~m_ssoRope.NoCollisionNoRaycastLayer) 
				&& !IsSpaceAround(hitInfo, cameraTransform, m_ssoRope.MinRadiusAround, ~m_ssoRope.NoCollisionNoRaycastLayer))
			{
				transform.SetParent(null, true);
				Deploy(cameraTransform, hitInfo.point);
				return true;
			}
		}
		
		return false;
	}
    private void Deploy(Transform cameraTransform, Vector3 deployPoint)
	{
		// Disable hold length constraint to avoid the character to be snapped to the rope when placed
		SetHoldLength(9999);

		transform.eulerAngles = new Vector3(0, cameraTransform.rotation.eulerAngles.y, 0);
		transform.DOJump(deployPoint, 1f, 0, 0.3f).OnComplete(() =>
		{
			// Rope custom initialization commands
			m_boxCollider.enabled = true;
			m_folds = new List<Vector3>() { m_ropeAttach.position.CutDigits(2) };
			m_isPlaced = true;
		});
	}

	#endregion

	#region ROPE

	public void Attach(Transform attach, Rigidbody rigidbody)
	{
		m_characterHarness = attach;
		m_characterRigidbody = rigidbody;
		m_joint.connectedBody = rigidbody;

		if (m_isPlaced) UpdateHoldLength();

		OnAttached?.Invoke();
	}

	public void Reattach(CharacterInteract characterInteract)
	{
		// Attach the character to the rope
		var characterMotor = characterInteract.GetComponent<CharacterMotor>();
		Attach(characterMotor.Attach, characterMotor.Rigidbody);
		characterMotor.Equip(this);

		// Update folds
		for (int i = m_folds.Count - 1; i >= 0; i--)
		{
			// Assert: an object is obstructing the way from the fold towards the character.
			if (!Physics.Linecast(m_characterHarness.position, m_folds[i], out var hit, m_ssoRope.FoldLayerToInclude)) break;

			m_folds.Remove(m_folds[i]);
		}
		UpdateHoldLength();
	}

	public void Detach()
	{
		// Assertion
		if (!IsConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterHarness.position.CutDigits(2));

		m_joint.connectedBody = null;
		m_characterRigidbody = null;
		m_characterHarness = null;

		OnDetached?.Invoke();
	}
 
	/// <summary>
	/// Add fold if a collider stands between the character and the last fold.
	/// </summary>
	private void AddFolds()
	{
		Vector3 startPosition = m_characterHarness.position;
		Vector3 endPosition = m_rsoCharacterLastPosition.value + (m_characterHarness.position - m_characterRigidbody.position);

		// Assert: Folds can't be added if the character isn't moving.
		if (startPosition == endPosition) return;

		// - Store every possible raycast starting positions based on the FoldingPrecision entered in SSO_Rope -
		Vector3 direction = (startPosition - endPosition).normalized;
		float distance = (startPosition - endPosition).magnitude;
		m_foldRaycastPositions = new List<Vector3> { startPosition };
		for (int i = 0; i < m_ssoRope.FoldingPrecision; i++)
		{
			distance /= 2;
			Vector3 newPosition = endPosition + direction * distance;
			m_foldRaycastPositions.Add(newPosition);
		}

		// - Get the possible contact point (fold) closest to the edge of the collider -
		bool isPointAdded = false;
		m_foldValidPositions = new Vector3[m_foldRaycastPositions.Count];
		for (int i = 0; i < m_foldRaycastPositions.Count; i++)
		{
			if (Physics.Linecast(m_foldRaycastPositions[i], CurrentFold, out var hit, m_ssoRope.FoldLayerToInclude))
			{
				// Assert: The distance between the two last folds is less than the minimum threshold.
				if (Folds.Count >= 2
				&& (CurrentFold - LastFold).magnitude < m_ssoRope.MinFoldDistance)
				{
					return;
				}

				// Store the valid position
				m_foldValidPositions[i] = (hit.point + hit.normal * m_ssoRope.FoldOffset).CutDigits(2);
			}

			// Adds the last valid folding position into the folds list.
			if (i > 0										// Avoid index outside the bounds of the array
			&& m_foldValidPositions[i] == Vector3.zero     	// AND Current position isn't valid
			&& m_foldValidPositions[i - 1] != Vector3.zero) // AND Last position is valid
			{
				m_folds.AddUnique(
					m_foldValidPositions[i - 1],
					callback: added => { if (added) IncreaseHoldLength(-(LastFold - CurrentFold).magnitude); }
				);
				isPointAdded = true;
				break;
			}
		}

		// Handle cases where only the start position is valid.
		if (isPointAdded = false					// Precise point haven't been added 
		&& m_foldValidPositions[0] == Vector3.zero)	// AND Start position is valid
		{
			m_folds.AddUnique(
				m_foldValidPositions[0],
				callback: added => { if (added) IncreaseHoldLength(-(LastFold - CurrentFold).magnitude); }
			);
		}
	}

	/// <summary>
	/// Remove the last fold from the list if there is no collider 
	/// standing between the character and the previous last fold.
	/// </summary>
	private void RemoveFolds()
	{
		if (Folds.Count >= 2
		&& !Physics.Linecast(m_characterHarness.position, LastFold, out var removeHit, m_ssoRope.FoldLayerToInclude))
		{
			IncreaseHoldLength((LastFold - CurrentFold).magnitude);
			m_folds.Remove(CurrentFold);
		}
	}

	/// <summary>
	/// Update hold rope radius to be the distance between the character rope attach 
	/// position and the last fold of the rope. Only if allowed (used by callbacks).
	/// </summary>
	/// <param name="isAllowed">Is it allowed to update hold rope radius.</param>
	public void UpdateHoldLength(bool isAllowed = true)
	{
		// Assertions
		if (!isAllowed) return;
		if (!IsConstrained) return;

		m_holdLength = GetCurrentFoldCharacterDistance();
		HandleJoint();
	}

	public void IncreaseHoldLength(float amount)
	{
		m_holdLength += amount;
		HandleJoint();
	}

	public void SetHoldLength(float length)
	{
		m_holdLength = length;
		HandleJoint();
	}

	private void HandleJoint()
	{
		m_joint.transform.position = CurrentFold;
		m_linearLimit.limit = m_holdLength;
		m_joint.linearLimit = m_linearLimit;
	}

	/// <summary>
	/// Current distance between the character's position and the base of the rope.
	/// </summary>
	public float GetTotalLength()
	{
		float output = 0;
		for (int i = 0; i < m_folds.Count - 1; i++)
		{
			Vector3 nextPosition = i + 1 >= m_folds.Count - 1 && IsConnected
				? m_characterRigidbody.position
				: m_folds[i + 1];

			output += (m_folds[i] - nextPosition).magnitude;
		}
		return output;
	}

	public float GetFixedLength()
	{
		float output = 0;
		for (int i = 0; i < m_folds.Count - 1; i++)
		{
			if (i + 1 > m_folds.Count - 1) continue;

			output += (m_folds[i + 1] - m_folds[i]).magnitude;
		}
		return output;
	}

	public float GetCurrentFoldCharacterDistance()
	{
		// Assertion
		if (!IsConnected) return -1;

		// Note that we do not connect the current fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		// Only the graphics and folds raycasts are connected to harness.
		return (CurrentFold - m_characterRigidbody.position).magnitude;
	}

	#endregion
}
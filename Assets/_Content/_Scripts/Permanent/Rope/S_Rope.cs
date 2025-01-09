using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Rope : Permanent
{
	#region REFERENCES

	[Title("Internal references")]
	[SerializeField] private BoxCollider m_boxCollider;
	[SerializeField] private Transform m_ropeAttach;
	[SerializeField] public Transform RaycastTarget;
	[SerializeField] private MeshRenderer m_previewMeshRendered;
	[SerializeField] private GameObject m_previewGameObject;
	[SerializeField] private ConfigurableJoint m_joint;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;

	#endregion

	#region VARIABLES

	private bool m_isConnected;
	private bool m_isPlaced;
	private float m_holdLength;
	private List<Vector3> m_folds = new List<Vector3>();
	private Rigidbody m_characterRigidbody;
	private Transform m_characterHarness;
	private SoftJointLimit m_linearLimit;

	[HideInInspector] public bool IsConstrained;
	public Action OnAttached;
	public Action OnDetached;
	public bool IsConnected => m_isConnected;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
	public Rigidbody CharacterRigidbody => m_characterRigidbody;
	public Vector3 CharacterPosition
	{
		get
		{
			if (m_characterRigidbody) return m_characterRigidbody.position;
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
		if (!m_isConnected) return;
		if (!m_isPlaced) return;

		HandleFolds();
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
			~m_ssoRope.DeployLayersToIgnore))
		{
			if (!m_previewGameObject.activeInHierarchy)
			{
				m_previewGameObject.SetActive(true);
			}

			// Update preview position
			m_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + m_ssoRope.HeightLimit / 2, hitInfo.point.z);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ssoRope.HeightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ssoRope.MinDistanceFromWall)
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
			~m_ssoRope.DeployLayersToIgnore))
		{
			if (IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ssoRope.HeightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ssoRope.MinDistanceFromWall))
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
		m_isConnected = true;

		if (m_isPlaced) UpdateHoldLength();

		OnAttached?.Invoke();
	}

	public void Reattach(CharacterInteract characterInteract)
	{
		// Attach the character to the rope
		var characterMotor = characterInteract.GetComponent<CharacterMotor>();
		Attach(characterMotor.Harness, characterMotor.Rigidbody);
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
		if (!m_isConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterHarness.position.CutDigits(2));

		m_isConnected = false;
		m_joint.connectedBody = null;
		m_characterRigidbody = null;
		m_characterHarness = null;

		OnDetached?.Invoke();
	}

	/// <summary>
	/// Check rope folding using raycasts.
	/// </summary>
	public void HandleFolds()
	{
		// Add fold if a collider stands between the character and the last fold
		if (Physics.Linecast(m_characterHarness.position, CurrentFold, out var addHit, m_ssoRope.FoldLayerToInclude))
		{
			Vector3 offsetPoint = addHit.point + addHit.normal * m_ssoRope.FoldOffset;
			Vector3 approximatePoint = offsetPoint.CutDigits(2);

			if (m_folds.Count >= 2)
			{
				// Minimal distance between two fold point to be register
				if ((CurrentFold - LastFold).magnitude >= m_ssoRope.MinFoldDistance)
				{
					m_folds.AddUnique(approximatePoint, callback: added => {
						if (added) IncreaseHoldLength(-(LastFold - CurrentFold).magnitude);
					});
				}
			}
			else
			{
				
				m_folds.AddUnique(approximatePoint, callback: added => {
					if (added) IncreaseHoldLength(-(LastFold - CurrentFold).magnitude);
				});
			}
		}

		// Remove the last fold from the list if there is no collider 
		// that stands between the character and the previous last fold.
		if (m_folds.Count >= 2
		&& !Physics.Linecast(m_characterHarness.position, LastFold, out var removeHit, m_ssoRope.FoldLayerToInclude))
		{
			IncreaseHoldLength((LastFold - CurrentFold).magnitude);
			m_folds.Remove(CurrentFold);
		}
	}

	/// <summary>
	/// Update hold rope radius to be the distance between the character rope attach 
	/// position and the last fold of the rope. Only if allowed.
	/// </summary>
	/// <param name="isAllowed">Is it allowed to update hold rope radius</param>
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
		// Assertion
		if (!m_isConnected) return 0;

		float output = 0;
		for (int i = 0; i < m_folds.Count; i++)
		{
			Vector3 nextPosition = i + 1 >= m_folds.Count
				? m_characterRigidbody.position
				: m_folds[i + 1];

			output += (m_folds[i] - nextPosition).magnitude;
		}
		return output;
	}

	public float GetCurrentFoldCharacterDistance()
	{
		// Assertion
		if (!m_isConnected) return -1;

		// Note that we do not connect the current fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		// Only the graphics are connected to harness.
		return (CurrentFold - m_characterRigidbody.position).magnitude;
	}

	#endregion
}
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
	[SerializeField] private MeshRenderer m_previewMeshRendered;
	[SerializeField] private GameObject m_previewGameObject;
	[SerializeField] private ConfigurableJoint m_joint;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	#endregion

	#region VARIABLES

	private bool m_isConnected;
	private bool m_isPlaced;
	private float m_holdLength;
	private List<Vector3> m_folds = new List<Vector3>();
	private List<RopeLine> m_ropeLines = new List<RopeLine>();
	private List<Interactable> m_interactables = new List<Interactable>();
	private Transform m_characterHarness;
	private SoftJointLimit m_linearLimit;

	[HideInInspector] public bool IsConstrained;
	public bool IsConnected => m_isConnected;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
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
		DrawLines();
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
		transform.eulerAngles = new Vector3(0, cameraTransform.rotation.eulerAngles.y, 0);
		transform.DOJump(deployPoint, 1f, 0, 0.3f).OnComplete(() =>
		{
			// Rope custom initialization commands
			m_boxCollider.enabled = true;
			m_folds = new List<Vector3>() { m_ropeAttach.position.CutDigits(2) };
			if (m_characterHarness) SetHoldLength((m_ropeAttach.position - m_characterHarness.position).magnitude);
			m_isPlaced = true;
		});
	}

	#endregion

	#region ROPE

	public void Attach(Transform harness, Rigidbody rigidbody)
	{
		m_characterHarness = harness;
		m_joint.connectedBody = rigidbody;
		m_isConnected = true;
	}

	public void Detach()
	{
		// Assertions
		if (m_characterHarness == null || !m_isConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterHarness.position.CutDigits(2));
		SpawnInteractables();

		m_isConnected = false;
		m_joint.connectedBody = null;
		m_characterHarness = null;
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
					m_folds.AddUnique(approximatePoint, UpdateHoldLength);
				}
			}
			else
			{
				m_folds.AddUnique(approximatePoint, UpdateHoldLength);
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

	public void SpawnInteractables()
	{
		float sphereDiameter = m_ssoRope.PfRopeInteractible.GetComponent<SphereCollider>().radius * 2f;

		for (int i = 0; i < m_ropeLines.Count; i++)
		{
			Vector3 lineDirection = (m_ropeLines[i].Positions[0] - m_ropeLines[i].Positions[1]).normalized;
			float lineLength = (m_ropeLines[i].Positions[0] - m_ropeLines[i].Positions[1]).magnitude;
			int sphereAmount = Mathf.FloorToInt(lineLength / sphereDiameter);

			for (int j = 0; j < sphereAmount; j++)
			{
				var newInteractable = Instantiate(m_ssoRope.PfRopeInteractible, transform);
				newInteractable.OnInteractedWithRef += Reattach;
				newInteractable.transform.rotation = Quaternion.LookRotation(lineDirection);
				newInteractable.transform.position = m_ropeLines[i].Positions[1] + lineDirection * sphereDiameter * j;
				m_interactables.Add(newInteractable);
			}
		}
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

		// Delete interactable spheres
		for (int i = m_interactables.Count - 1; i >= 0; i--)
		{
			Destroy(m_interactables[i].gameObject);
		}
		m_interactables.Clear();
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

		m_holdLength = GetLastFoldHarnessDistance();
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
				? m_characterHarness.position
				: m_folds[i + 1];

			output += (m_folds[i] - nextPosition).magnitude;
		}
		return output;
	}

	public float GetLastFoldHarnessDistance()
	{
		// Assertion
		if (!m_isConnected) return -1;

		// Note that we do not connect the last fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		return (CurrentFold - m_rsoCharacterPosition.value).magnitude;
	}

	private void DrawLines()
	{
		// Assertion
		if (!m_characterHarness) return;

		// Clear lists
		if (m_ropeLines.Count >= 1)
		{
			for (int i = m_ropeLines.Count - 1; i >= 0; i--)
			{
				Destroy(m_ropeLines[i].gameObject);
			}
			m_ropeLines = new List<RopeLine>();
		}

		// Get material based in the total distance
		Material material = m_ssoRope.DangerMaterial;
		if (GetTotalLength() <= m_ssoRope.MaxLength / 2f)
		{
			material = m_ssoRope.SafeMaterial;
		}
		else if (GetTotalLength() <= 3 * (m_ssoRope.MaxLength / 4f))
		{
			material = m_ssoRope.MidMaterial;
		}

		// Draw lines 
		for (int i = 0; i < m_folds.Count; i++)
		{
			RopeLine newRopeLine = Instantiate(m_ssoRope.PfRopeLine, transform);
			newRopeLine.SetPositions(m_folds[i], i + 1 >= m_folds.Count ? m_characterHarness.position : m_folds[i + 1]);
			newRopeLine.SetColor(material);
			m_ropeLines.Add(newRopeLine);
		}
	}

	#endregion
}
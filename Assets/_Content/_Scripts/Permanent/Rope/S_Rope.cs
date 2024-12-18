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
	[SerializeField] private Interactable m_baseInteractable;
	[SerializeField] private ConfigurableJoint m_joint;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;

	#endregion

	#region VARIABLES

	private bool m_isConnected;
	private bool m_isPlaced;
	private float m_holdLength;
	private List<Vector3> m_folds = new List<Vector3>();
	private RopeLine m_ropeLine;
	private List<Interactable> m_interactables = new List<Interactable>();
	private Rigidbody m_characterRigidbody;
	private Transform m_characterAttach;
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

	private void Start()
	{
		m_baseInteractable.gameObject.SetActive(false);
	}

	private void Update()
	{
		// Assertions
		if (!m_isConnected) return;
		if (!m_isPlaced) return;

		HandleFolds();
		HandleJoint();
		DrawLines();
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
		m_characterAttach = attach;
		m_characterRigidbody = rigidbody;
		m_joint.connectedBody = rigidbody;
		m_isConnected = true;

		if (m_isPlaced) UpdateHoldLength();
	}

	public void Detach()
	{
		// Assertion
		if (!m_isConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterAttach.position.CutDigits(2));
		SpawnInteractables();

		m_isConnected = false;
		m_joint.connectedBody = null;
		m_characterRigidbody = null;
		m_characterAttach = null;
	}

	/// <summary>
	/// Check rope folding using raycasts.
	/// </summary>
	public void HandleFolds()
	{
		// Add fold if a collider stands between the character and the last fold
		if (Physics.Linecast(m_characterAttach.position, CurrentFold, out var addHit, m_ssoRope.FoldLayerToInclude))
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
		&& !Physics.Linecast(m_characterAttach.position, LastFold, out var removeHit, m_ssoRope.FoldLayerToInclude))
		{
			IncreaseHoldLength((LastFold - CurrentFold).magnitude);
			m_folds.Remove(CurrentFold);
		}
	}

	public void SpawnInteractables()
	{
		// Update the interactable component at the base of the rope.
		m_baseInteractable.GetComponent<SphereCollider>().radius = m_ssoRope.InteractableSphereRadius;
		m_baseInteractable.gameObject.SetActive(true);
		m_baseInteractable.OnInteractedWithRef += Reattach;

		m_ssoRope.PfRopeInteractible.GetComponent<SphereCollider>().radius = m_ssoRope.InteractableSphereRadius;
		float sphereDiameter = m_ssoRope.PfRopeInteractible.GetComponent<SphereCollider>().radius * 2f;

		for (int i = 1; i < m_ropeLine.Positions.Length; i++)
		{
			Vector3 lineDirection = (m_ropeLine.Positions[i-1] - m_ropeLine.Positions[i]).normalized;
			float lineLength = (m_ropeLine.Positions[i-1] - m_ropeLine.Positions[i]).magnitude;
			int sphereAmount = Mathf.FloorToInt(lineLength / sphereDiameter) + 1;

			for (int j = 0; j < sphereAmount; j++)
			{
				var newInteractable = Instantiate(m_ssoRope.PfRopeInteractible, transform);
				newInteractable.OnInteractedWithRef += Reattach;
				newInteractable.transform.rotation = Quaternion.LookRotation(lineDirection);
				newInteractable.transform.position = m_ropeLine.Positions[i] + lineDirection * sphereDiameter * j;
				m_interactables.Add(newInteractable);
			}
		}
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
			if (!Physics.Linecast(m_characterAttach.position, m_folds[i], out var hit, m_ssoRope.FoldLayerToInclude)) break;
			
			m_folds.Remove(m_folds[i]);
		}
		UpdateHoldLength();

		// Remove all rope interactables from the character interact
		characterInteract.Remove(m_baseInteractable);
		foreach (var interactable in m_interactables)
		{
			characterInteract.Remove(interactable);
		}

		// Delete interactables
		m_baseInteractable.gameObject.SetActive(false);
		m_baseInteractable.OnInteractedWithRef -= Reattach;
		for (int i = m_interactables.Count - 1; i >= 0; i--)
		{
			m_interactables[i].OnInteractedWithRef -= Reattach;
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

	private void DrawLines()
	{
		// Assertion
		if (!m_isConnected) return;

		if (m_ropeLine == null)
		{
			m_ropeLine = Instantiate(m_ssoRope.PfRopeLine, transform);
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

		List<Vector3> positions = new List<Vector3>();
		// Draw lines 
		foreach (Vector3 fold in m_folds)
		{
			positions.Add(fold);
		}
        positions.Add(m_characterAttach.position.CutDigits(2));
        
        m_ropeLine.SetPositions(positions.ToArray());
        m_ropeLine.SetColor(material);
    }

	#endregion
}
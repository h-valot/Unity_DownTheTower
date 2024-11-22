using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Rope : Permanent
{
	#region REFERENCES

	[Header("Internal references")]
	[SerializeField] private Transform m_ropeAttach;
	[SerializeField] private MeshRenderer m_previewMeshRendered;
	[SerializeField] private GameObject m_previewGameObject;
	[SerializeField] private ConfigurableJoint m_joint;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig m_ropeConfig;
	[SerializeField] private RSE_SetCharacterPosition m_rseSetCharacterPosition;
	[SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	#endregion

	#region VARIABLES

	private bool m_isConnected;
	private bool m_isPlaced;
	public float m_holdLength;
	public List<Vector3> m_folds = new List<Vector3>();
	private List<RopeLine> m_ropeLines = new List<RopeLine>();
	private List<Interactable> m_interactibles = new List<Interactable>();
	private Transform m_characterHarness;
	private SoftJointLimit m_linearLimit;

	public bool IsConnected => m_isConnected;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
	public Vector3 CurrentFold => m_folds[^1];
	public Vector3 LastFold 
	{ 
		get { 
			if (m_folds.Count >= 2) return m_folds[^2];
			else return Vector3.zero;
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
		HandleInteractibles();
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
			GetPositionRayDirection(cameraTransform, m_ropeConfig.CameraOffsetAngle, m_ropeConfig.MaxCameraDownwardClamp), 
			out var hitInfo, 
			m_ropeConfig.MaxDistFromCamera, 
			~m_ropeConfig.DeployLayersToIgnore))
		{
			if (!m_previewGameObject.activeInHierarchy)
			{
				m_previewGameObject.SetActive(true);
			}

			// Update preview position
			m_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + m_ropeConfig.HeightLimit / 2, hitInfo.point.z);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, m_ropeConfig.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ropeConfig.HeightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ropeConfig.MinDistanceFromWall)
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
		m_previewMeshRendered.material.SetFloat("_colorSwitch", isDeployable ? 1f : 0f);
	}

	public override bool Throw(Transform cameraTransform)
	{
		m_previewGameObject.SetActive(false);

		if (Physics.Raycast(
			cameraTransform.position,
			GetPositionRayDirection(cameraTransform, m_ropeConfig.CameraOffsetAngle, m_ropeConfig.MaxCameraDownwardClamp),
			out var hitInfo,
			m_ropeConfig.MaxDistFromCamera,
			~m_ropeConfig.DeployLayersToIgnore))
		{
			if (IsGroundFlat(hitInfo, m_ropeConfig.MaxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ropeConfig.HeightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ropeConfig.MinDistanceFromWall))
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
			m_folds = new List<Vector3>() { m_ropeAttach.position.CutDigits(2) };
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
		// Assertion
		if (m_characterHarness == null || !m_isConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterHarness.position.CutDigits(2));
		HandleInteractibles();

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
		if (Physics.Linecast(m_characterHarness.position, CurrentFold, out var addHit, ~m_ropeConfig.FoldLayerToInclude))
		{
			Vector3 approximatePoint = addHit.point.CutDigits(2);

			if (m_folds.Count >= 2)
			{
				// Minimal distance between two fold point to be register
				if ((CurrentFold - LastFold).magnitude >= m_ropeConfig.MinFoldDistance)
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
		&& !Physics.Linecast(m_characterHarness.position, LastFold, out var removeHit, ~m_ropeConfig.FoldLayerToInclude))
		{
			ChangeHoldLength((LastFold - CurrentFold).magnitude);
			m_folds.Remove(CurrentFold);
		}
	}

	public void HandleInteractibles()
	{
		for (int i = m_interactibles.Count - 1; i >= m_folds.Count - 1; i--)
		{
			m_interactibles[i].OnInteracted -= Teleport;
			Destroy(m_interactibles[i].gameObject);
			m_interactibles.RemoveAt(i);
		}

		for (int i = 0; i < m_folds.Count; i++)
		{
			if (m_interactibles.Count - 1 < i) 
			{
				Interactable newInteractible = Instantiate(m_ropeConfig.PfRopeInteractible, m_folds[i], Quaternion.identity, transform);
				newInteractible.OnInteracted += Teleport;
				m_interactibles.Add(newInteractible);
				continue;
			}

			m_interactibles[i].transform.position = m_folds[i];
		}
	}

	public void Teleport()
	{
		m_rseSetCharacterPosition.Call(m_ropeAttach.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// Update hold rope radius to be the distance between the character rope attach 
	/// position and the last fold of the rope. Only if allowed.
	/// </summary>
	/// <param name="isAllowed">Is it allowed to update hold rope radius</param>
	public void UpdateHoldLength(bool isAllowed = true)
	{
		// Assert: is it not allowed
		if (!isAllowed) return;

		m_holdLength = GetLastFoldHarnessDistance();
		HandleJoint();
	}

	public void ChangeHoldLength(float amount)
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
		Material material = m_ropeConfig.DangerMaterial;
		if (GetTotalLength() <= m_ropeConfig.MaxLength / 2f)
		{
			material = m_ropeConfig.SafeMaterial;
		}
		else if (GetTotalLength() <= 3 * (m_ropeConfig.MaxLength / 4f))
		{
			material = m_ropeConfig.MidMaterial;
		}

		// Draw lines 
		for (int i = 0; i < m_folds.Count; i++)
		{
			RopeLine newRopeLine = Instantiate(m_ropeConfig.PfRopeLine);
			newRopeLine.SetPositions(m_folds[i], i + 1 >= m_folds.Count ? m_characterHarness.position : m_folds[i + 1]);
			newRopeLine.SetColor(material);
			m_ropeLines.Add(newRopeLine);
		}
	}

	#endregion
}
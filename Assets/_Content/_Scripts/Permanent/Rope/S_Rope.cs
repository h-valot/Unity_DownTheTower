using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Rope : Permanent
{
	[Header("Internal references")]
	[SerializeField] private Transform m_ropeAttach;
	[SerializeField] private MeshRenderer m_previewMeshRendered;
	[SerializeField] private GameObject m_previewGameObject;
	[SerializeField] private ConfigurableJoint m_joint;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig m_ropeConfig;
	[SerializeField] private RSE_SetCharacterPosition m_rseSetCharacterPosition;
	[SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	private bool m_isConnected;
	private bool m_isPlaced;
	private float m_holdLength;
	private List<Vector3> m_folds = new List<Vector3>();
	private List<RopeLine> m_ropeLines = new List<RopeLine>();
	private List<Interactable> m_interactibles = new List<Interactable>();
	private Rigidbody m_characterRigidbody;
	private SoftJointLimit m_linearLimit;

	public bool IsConnected => m_isConnected;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
	public List<Vector3> Folds => m_folds;

	#region MONOBEHAVIOR

	private void Update()
	{
		// Assertions
		if (!m_isConnected) return;
		if (!m_isPlaced) return;

		HandleFolds();
		HandleInteractibles();
		HandleEnd();
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
			GetPositionRayDirection(cameraTransform, m_ropeConfig.cameraOffsetAngle, m_ropeConfig.maxCameraDownwardClamp), 
			out var hitInfo, 
			m_ropeConfig.maxDistFromCamera, 
			~m_ropeConfig.layersToIgnore))
		{
			if (!m_previewGameObject.activeInHierarchy)
			{
				m_previewGameObject.SetActive(true);
			}

			// Update preview position
			m_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + m_ropeConfig.heightLimit / 2, hitInfo.point.z);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, m_ropeConfig.maxGroundAngle) 
				&& !IsCeiling(hitInfo, m_ropeConfig.heightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ropeConfig.minDistanceFromWall)
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
			GetPositionRayDirection(cameraTransform, m_ropeConfig.cameraOffsetAngle, m_ropeConfig.maxCameraDownwardClamp),
			out var hitInfo,
			m_ropeConfig.maxDistFromCamera,
			~m_ropeConfig.layersToIgnore))
		{
			if (IsGroundFlat(hitInfo, m_ropeConfig.cameraOffsetAngle) 
				&& !IsCeiling(hitInfo, m_ropeConfig.heightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, m_ropeConfig.minDistanceFromWall))
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

	public void Attach(Rigidbody rigidbody)
	{
		m_characterRigidbody = rigidbody;
		m_joint.connectedBody = rigidbody;
		m_isConnected = true;
	}

	public void Detach()
	{
		// Assertion
		if (m_characterRigidbody == null || !m_isConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(m_characterRigidbody.position.CutDigits(2));
		HandleInteractibles();

		m_characterRigidbody = null;
		m_isConnected = false;
	}

	/// <summary>
	/// Check rope folding using raycasts.
	/// </summary>
	public void HandleFolds()
	{
		// Assert: character ref null
		if (m_characterRigidbody == null) return;

		// Add fold if a collider stands between the character and the last fold
		if (Physics.Linecast(m_characterRigidbody.position, m_folds[^1], out var addHit, ~m_ropeConfig.foldLayerToIgnore))
		{
			Vector3 approximatePoint = addHit.point.CutDigits(2);

			if (m_folds.Count >= 2)
			{
				// Minimal distance between two fold point to be register
				if ((m_folds[^1] - m_folds[^2]).magnitude >= m_ropeConfig.minFoldDistance)
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
		&& !Physics.Linecast(m_characterRigidbody.position, m_folds[^2], out var removeHit, ~m_ropeConfig.foldLayerToIgnore))
		{
			m_holdLength = GetLastFoldHarnessDistance() + (m_folds[^2] - m_folds[^1]).magnitude;
			m_folds.Remove(m_folds[^1]);
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
				Interactable newInteractible = Instantiate(m_ropeConfig.pfRopeInteractible, m_folds[i], Quaternion.identity, transform);
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
	/// Detach the rope from the player if its total length is greater than the limit.
	/// </summary>
	private void HandleEnd()
	{
		// Assert: total rope length is smaller than the max length
		if (GetTotalLength() <= m_ropeConfig.maxLength) return;

		Detach();
	}

	/// <summary>
	/// Update configurable joint position to match the last fold position.
	/// Update the linear limit to constraint the character in the sphere.
	/// </summary>
	private void HandleJoint()
	{
		m_joint.transform.position = m_folds[^1];
		m_linearLimit.limit = m_holdLength;
		m_joint.linearLimit = m_linearLimit;
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
	}

	/// <summary>
	/// Current distance between the character's position and the base of the rope.
	/// </summary>
	public float GetTotalLength()
	{
		// Assertions
		if (!m_isPlaced) return 0;
		if (m_characterRigidbody == null) return 0;

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

	public float GetLastFoldHarnessDistance()
	{
		// Assertions
		if (!m_isPlaced) return -1;
		if (m_characterRigidbody == null) return -1;

		// Note that we do not connect the last fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		return (m_folds[^1] - m_rsoCharacterPosition.value).magnitude;
	}
	
	public void ChangeHoldLength(float amount)
	{
		m_holdLength += amount;
	}

	private void DrawLines()
	{
		// Assertions
		if (!m_isPlaced) return;
		if (m_characterRigidbody == null) return;

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
		Material material = m_ropeConfig.dangerMaterial;
		if (GetTotalLength() <= m_ropeConfig.maxLength / 2f)
		{
			material = m_ropeConfig.safeMaterial;
		}
		else if (GetTotalLength() <= 3 * (m_ropeConfig.maxLength / 4f))
		{
			material = m_ropeConfig.midMaterial;
		}

		// Draw lines 
		for (int i = 0; i < m_folds.Count; i++)
		{
			RopeLine newRopeLine = Instantiate(m_ropeConfig.pfRopeLine);
			newRopeLine.SetPositions(m_folds[i], i + 1 >= m_folds.Count ? m_characterRigidbody.position : m_folds[i + 1]);
			newRopeLine.SetColor(material);
			m_ropeLines.Add(newRopeLine);
		}
	}

	#endregion
}
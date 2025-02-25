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
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_topMeshRenderer;
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_baseMeshRenderer;
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_detail0MeshRenderer;
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_detail1MeshRenderer;
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_detail2MeshRenderer;
    [FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_previewMeshRendered;
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_previewGameObject;
	[FoldoutGroup("Internal references")][SerializeField] private ConfigurableJoint m_joint;

    [FoldoutGroup("Internal references")][SerializeField] private Transform m_topTransform;
    [FoldoutGroup("Internal references")][SerializeField] private Transform m_detail0Transform;
    [FoldoutGroup("Internal references")][SerializeField] private Transform m_detail1Transform;
    [FoldoutGroup("Internal references")][SerializeField] private Transform m_detail2Transform;

    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_HarnessPosition m_rsoHarnessPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterLastPosition m_rsoCharacterLastPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DebugLog m_rseDebugLog;

	#endregion

	#region VARIABLES

	private bool m_isPlaced;
	private float m_holdLength;
	public List<Fold> m_folds = new List<Fold>();
	private Rigidbody m_characterRigidbody;
	private Transform m_characterHarness;
	private SoftJointLimit m_linearLimit;

	public bool IsConstrained;
	public Action OnAttached;
	public Action OnDetached;
	public Action OnLimitReached;
	public bool IsConnected => m_characterRigidbody;
	public bool IsPlaced => m_isPlaced;
	public float HoldLength => m_holdLength;
	public Rigidbody CharacterRigidbody => m_characterRigidbody;
	public List<Fold> Folds => m_folds;
	public Vector3 HarnessPosition
	{
		get 
		{
			if (m_characterHarness) return m_characterHarness.position;
			else return CurrentFold.Position;
		}
	}
	public Fold CurrentFold 
	{
		get 
		{
			if (m_folds.Count >= 1) return m_folds[^1];
			else return new Fold(m_ropeAttach.position, Vector3.zero);
		}
	}
	public Fold LastFold 
	{ 
		get 
		{ 
			if (m_folds.Count >= 2) return m_folds[^2];
			else return CurrentFold;
		}
	}

	// Graphics
	private MaterialPropertyBlock m_materialPropertyBlock;

    #endregion

    #region MONOBEHAVIOR

    private void Awake()
    {
        m_materialPropertyBlock = new MaterialPropertyBlock();
    }

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
			if (!m_previewGameObject.activeInHierarchy) m_previewGameObject.SetActive(true);

			// Update preview position
			m_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
			m_previewGameObject.transform.rotation = Quaternion.Euler(0, m_previewGameObject.transform.eulerAngles.y, 0);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsSpaceAbove(hitInfo, m_ssoRope.HeightLimit, ~m_ssoRope.NoCollisionNoRaycastLayer) 
				&& !IsSpaceAround(hitInfo, m_ssoRope.MinRadiusAround, ~m_ssoRope.NoCollisionNoRaycastLayer)
				&& !IsSpaceBetween(hitInfo.point + (m_ropeAttach.position - transform.position).magnitude * Vector3.up, m_rsoHarnessPosition.value, m_ssoRope.FoldLayerToInclude)
			);
		}
		else
		{
			if (m_previewGameObject.activeInHierarchy) m_previewGameObject.SetActive(false);

			UpdateColor(isDeployable: false);
		}
    }

	public override void DisablePreview()
    {
        m_previewGameObject.SetActive(false);
    }

    private void UpdateColor(bool isDeployable)
	{
		m_previewMeshRendered.material.SetFloat("_colorSwitch", isDeployable ? 0f : 1f);
	}

	public override bool Throw(Transform cameraTransform)
	{
		DisablePreview();

		if (Physics.Raycast(
			cameraTransform.position,
			GetPositionRayDirection(cameraTransform, m_ssoRope.CameraOffsetAngle, m_ssoRope.MaxCameraDownwardClamp),
			out var hitInfo,
			m_ssoRope.MaxDistFromCamera,
			~m_ssoRope.NoRaycastLayer))
		{
			if (IsGroundFlat(hitInfo, m_ssoRope.MaxGroundAngle) 
				&& !IsSpaceAbove(hitInfo, m_ssoRope.HeightLimit, ~m_ssoRope.NoCollisionNoRaycastLayer) 
				&& !IsSpaceAround(hitInfo, m_ssoRope.MinRadiusAround, ~m_ssoRope.NoCollisionNoRaycastLayer)
				&& !IsSpaceBetween(hitInfo.point + (m_ropeAttach.position - transform.position).magnitude * Vector3.up, m_rsoHarnessPosition.value, m_ssoRope.FoldLayerToInclude))
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
		SetHoldLength(m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset - GetFixedLength());

		transform.eulerAngles = new Vector3(0, cameraTransform.rotation.eulerAngles.y, 0);
		transform.DOScale(1f, 0.3f);
		transform.DOJump(deployPoint, 1f, 0, 0.3f).OnComplete(() =>
		{
			// Rope custom initialization commands
			m_boxCollider.enabled = true;
			m_folds = new List<Fold>() { new Fold(m_ropeAttach.position.CutDigits(2), Vector3.zero) };
			m_isPlaced = true;
            m_topMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            m_baseMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            m_detail0MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            m_detail1MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            m_detail2MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			OpenAnchor();
        });
	}

	private void OpenAnchor()
	{
        m_topTransform.DOLocalMoveY(0, m_ssoRope.DeployDuration);
        m_detail0Transform.DOLocalRotate(new Vector3(0, m_detail0Transform.localEulerAngles.y, 0), m_ssoRope.DeployDuration);
        m_detail1Transform.DOLocalRotate(new Vector3(0, m_detail1Transform.localEulerAngles.y, 0), m_ssoRope.DeployDuration);
        m_detail2Transform.DOLocalRotate(new Vector3(0, m_detail2Transform.localEulerAngles.y, 0), m_ssoRope.DeployDuration);

		m_materialPropertyBlock.SetFloat("_deployed", 1f);
        m_topMeshRenderer.SetPropertyBlock(m_materialPropertyBlock);
        m_baseMeshRenderer.SetPropertyBlock(m_materialPropertyBlock);
    }

	#endregion

	#region ROPE

	public void Attach(Transform harness, Rigidbody rigidbody)
	{
		m_characterHarness = harness;
		m_characterRigidbody = rigidbody;
		m_joint.connectedBody = rigidbody;

		if (m_isPlaced) UpdateHoldLength(); 

		OnAttached?.Invoke();
	}

	public void Reattach(CharacterInteract characterInteract)
	{
		// Attach the character to the rope
		var characterMotor = characterInteract.GetComponent<CharacterMotor>();
		Attach(characterMotor.Harness, characterMotor.Rigidbody);
		characterMotor.Equip(this);

		// Remove the current fold used to spawn interactables
		if (!Physics.Linecast(m_characterHarness.position, CurrentFold.Position, out var hit, m_ssoRope.FoldLayerToInclude))
		{
			m_folds.Remove(CurrentFold);
			UpdateHoldLength();
		}
	}

	public void Detach()
	{
		// Assertion
		if (!IsConnected) return;

		// Add a final fold to spawn an interactible on it.
		m_folds.Add(new Fold(m_characterHarness.position.CutDigits(2), Vector3.zero));

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
		Vector3 charaDir = (CurrentFold.Position - m_characterHarness.position).normalized;
		float maxDistance = (CurrentFold.Position - m_characterHarness.position).magnitude;
		if (Physics.Raycast(m_characterHarness.position, charaDir, out var charaHit, maxDistance,  m_ssoRope.FoldLayerToInclude)
		&& Physics.Raycast(CurrentFold.Position, -charaDir, out var foldHit, maxDistance, m_ssoRope.FoldLayerToInclude))
		{
			// Get the plane around the fold and the vector from the character to intersect with that plane
			Plane foldPlane = new Plane(foldHit.normal, foldHit.point);
			Vector3 charaInner = Vector3.Cross(charaDir, foldHit.normal);
			Vector3 charaCross = Vector3.Cross(-charaInner, charaHit.normal);
			Vector3 intersectionPoint = foldPlane.ClosestPointOnPlane(charaHit.point + charaCross);

			// Get the offset direction applied to the intersection plane point
			Vector3 offsetDir = foldHit.normal + charaHit.normal;

			// Get the next fold position out of the intersection plane point and the offset
			Fold nextFold =  new Fold(
				newPosition: intersectionPoint + offsetDir * m_ssoRope.FoldOffset, 
				newNormal: offsetDir
			);

			// Assertion: the next fold is too close to the previous one
			if ((nextFold.Position - CurrentFold.Position).magnitude > m_ssoRope.MinFoldDistance)
			{
				// Update the fold list and the rope length
				m_folds.Add(nextFold);
				IncreaseHoldLength(-(LastFold.Position - CurrentFold.Position).magnitude);
			}
		}
	}

	/// <summary>
	/// Remove the last fold from the list if there is no collider standing between the character and the previous last fold.
	/// </summary>
	private void RemoveFolds()
	{
		// Assertion
		if (Folds.Count < 2) return;

		// Get the plane of the fold
		Vector3 foldDir = (CurrentFold.Position - LastFold.Position).normalized;
		Vector3 edgeCross = Vector3.Cross(foldDir, CurrentFold.Normal);
		Plane edgePlane = new Plane(edgeCross, CurrentFold.Position);

		// Get both ray towards plane direction
		Ray downCharaRay = new Ray(m_characterHarness.position, -edgeCross);
		Ray upCharaRay = new Ray(m_characterHarness.position, edgeCross);

		// Get the position of the hit point on the plane
		Vector3 planedPoint = Vector3.zero;
		if (edgePlane.Raycast(downCharaRay, out var enter)) planedPoint = downCharaRay.GetPoint(enter);
		if (edgePlane.Raycast(upCharaRay, out enter)) planedPoint = upCharaRay.GetPoint(enter);

		if (planedPoint != Vector3.zero) 
		{
			// Remove fold if the angle between the direction from the harness towards the fold
			// and the direction of the plane is superior to 90 degrees.
			Vector3 charaPlanedDir = (CurrentFold.Position - planedPoint).normalized;
			if (Vector3.SignedAngle(charaPlanedDir, foldDir, edgeCross) > 90)
			{
				IncreaseHoldLength((LastFold.Position - CurrentFold.Position).magnitude);
				m_folds.Remove(CurrentFold);
			}
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
		m_joint.transform.position = CurrentFold.Position;
		m_linearLimit.limit = m_holdLength;
		m_joint.linearLimit = m_linearLimit;
	}

	public float GetTotalLength()
	{
		return GetFixedLength() + (IsConnected ? (CurrentFold.Position - m_characterRigidbody.position).magnitude : 0);
	}

	public float GetFixedLength()
	{
		float output = 0;
		for (int i = 0; i < m_folds.Count - 1; i++)
		{
			if (i + 1 > m_folds.Count - 1) continue;

			output += (m_folds[i + 1].Position - m_folds[i].Position).magnitude;
		}
		return output;
	}

	public float GetHeight()
	{
		return (transform.position - m_ropeAttach.position).magnitude;
	}

	public float GetCurrentFoldCharacterDistance()
	{
		// Assertion
		if (!IsConnected) return -1;

		// Note that we do not connect the current fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		// Only the graphics and folds raycasts are connected to harness.
		return (CurrentFold.Position - m_characterRigidbody.position).magnitude;
	}

	#endregion
}
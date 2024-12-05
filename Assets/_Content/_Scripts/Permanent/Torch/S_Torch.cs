using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class Torch : Permanent
{
    [Title("Internal references")]
    [SerializeField] private Light m_light;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private LineRenderer m_aimPreview;
    [SerializeField] private Transform m_torchTop;
    [SerializeField] private Transform m_pointLightBase;
    [SerializeField] private SphereCollider m_lightCollider;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	// ----- PUBLIC VARIABLES -----
	[HideInInspector] public bool IsLit;
    [HideInInspector] public bool IsInHand;

	// ----- PRIVATE VARIABLES -----
	private Vector3 m_lastPosition;
    private LayerMask m_layerMask;

    private MaterialPropertyBlock m_propertyBlock;

    private bool m_hasChangedColor;
    private bool m_hasPlayedHitSound;
    private bool m_isDeactivate;

    private float m_lightPercent;
    private Color m_emitColor;

    #region MONOBEHAVIOR

    private void Awake()
    {
        IsInHand = true;
        m_isDeactivate = false;
        m_hasChangedColor = false;
        m_hasPlayedHitSound = false;

        // Collisions
        m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        m_lightCollider.radius = m_ssoTorch.LightOffsetDistance;
        m_layerMask |= (1 << LayerMask.NameToLayer("Default"));
        m_layerMask |= (1 << LayerMask.NameToLayer("Collision_NoRaycast"));

        // Preview
        m_aimPreview.useWorldSpace = true;

        // Visual
        m_propertyBlock = new MaterialPropertyBlock();
        m_light.color = m_ssoTorch.LightColor;
        m_light.intensity = m_ssoTorch.LightIntensity;
        m_propertyBlock.SetColor("_lightColor", m_ssoTorch.LightColor);
        m_meshRenderer.SetPropertyBlock(m_propertyBlock);
    }

    private void Start()
    {
        if (m_ssoTorch.IsStartingLit)
        {
            IsLit = true;
            m_lightPercent = 1f;
            m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
            m_meshRenderer.SetPropertyBlock(m_propertyBlock);
            m_torchTop.transform.localPosition = new Vector3(m_torchTop.transform.localPosition.x, m_ssoTorch.TopTorchOffsetDistance, m_torchTop.transform.localPosition.z);
        }

        m_rsoTorchManager.value.Add(this);
        m_rsoCharacterPosition.OnChanged += UpdateTorchFeedback;
    }

    private void Update()
    {
		// Assertion
        if (IsInHand || !IsLit) return;

		if (HasMoved()) UpdateTorchFeedback();
	}

    private void LateUpdate()
    {
		// TODO - Prevent the penetration test to fire if the torch is immobile.
		// TODO - Use trigger enter and exit to prevent penetration test when there is no collider in range.

		// Assertion
		if (IsInHand || !IsLit) return;

		Vector3 _lightOffset = Vector3.zero;
		Collider[] _hitColliders = Physics.OverlapSphere(m_pointLightBase.position, m_ssoTorch.LightOffsetDistance, m_layerMask);
		if (_hitColliders.Length > 0)
		{
			foreach (Collider _otherCollider in _hitColliders)
			{
				Vector3 otherPosition = _otherCollider.gameObject.transform.position;
				Quaternion otherRotation = _otherCollider.gameObject.transform.rotation;

				Vector3 direction;
				float distance;

				bool overlapped = Physics.ComputePenetration(
					m_lightCollider, transform.position, transform.rotation,
					_otherCollider, otherPosition, otherRotation,
					out direction, out distance
				);

				if (overlapped)
				{
					_lightOffset += direction * distance;
				}

				_lightOffset = Vector3.ClampMagnitude(_lightOffset, m_ssoTorch.LightOffsetDistance);
				m_light.transform.position = m_pointLightBase.position + _lightOffset;
			}
		}
		else
		{
			m_light.transform.position = m_pointLightBase.position;
		}
    }   

    private void OnCollisionEnter(Collision collision)
    {
		// Assertion
        if (IsInHand) return;

		// If it collide with a flat surface it increase drag to prevent the torch from rolling for eternity
		if (Vector3.Dot(collision.contacts[0].normal, new Vector3(0, 1, 0)) >= 0.8)
		{
			m_rigidbody.drag = 1f;
			m_rigidbody.angularDrag = 1f;
		}

		if (!m_hasPlayedHitSound 
		&& m_rigidbody.velocity.magnitude > m_ssoTorch.MinSpeedForHitSound)
		{
			m_hasPlayedHitSound = true;
			if (m_isDeactivate)
			{
				Instantiate(m_ssoTorch.TorchBreakSFX, transform.position, Quaternion.identity);
				DOTween.Sequence().AppendInterval(m_ssoTorch.DesactivatingTime).SetId(gameObject.GetInstanceID()).OnComplete(() => { m_hasPlayedHitSound = false; });
			}
			else
			{
				Instantiate(m_ssoTorch.TorchHitSFX, transform.position, Quaternion.identity);
				DOTween.Sequence().AppendInterval(m_ssoTorch.TimeBetweenHitSound).SetId(gameObject.GetInstanceID()).OnComplete(() => { m_hasPlayedHitSound = false; });
			}
		}
    }

    #endregion

    #region LIGHT

    /// <summary> 
	/// Activate or deactivate light on the torch.
	/// </summary>
    public override void ToggleHandEffect()
    {
        m_light.enabled = false;

		// Assertion
        if (!IsInHand) return;

		if (IsLit) 
		{
            IsLit = false;
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 0f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
                .OnUpdate(() =>
                {
                    m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
                    m_meshRenderer.SetPropertyBlock(m_propertyBlock);
                });
            m_light.DOIntensity(0f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
            m_torchTop.DOLocalMoveY(0f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light").OnComplete(() => { m_light.enabled = false; });
        }
		else
		{
            IsLit = true;
            m_light.enabled = true;
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 1f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
               .OnUpdate(() =>
               {
                   m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
                   m_meshRenderer.SetPropertyBlock(m_propertyBlock);
               });
            m_light.DOIntensity(m_ssoTorch.LightIntensity, m_ssoTorch.LitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
            m_torchTop.DOLocalMoveY(m_ssoTorch.TopTorchOffsetDistance, m_ssoTorch.LitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
        }
    }

    private bool HasMoved()
    {
        if (m_lastPosition != transform.position)
        {
            m_lastPosition = transform.position;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region PREVIEW

    /// <summary> 
	/// Torch previsualisation with the camera's transform for the direction.
	/// </summary>
    public override void PreviewThrow(Transform _cameraTransform)
    {
        m_aimPreview.enabled = true;
        m_aimPreview.positionCount = Mathf.CeilToInt(m_ssoTorch.PreviewLength / m_ssoTorch.PreviewSmoothing) + 1;

        // set up starting point and velocity
        Vector3 startPosition = transform.position;
        Vector3 startVelocity = Quaternion.AngleAxis(-CalculateThrowAngleOffset(_cameraTransform), _cameraTransform.right) * _cameraTransform.forward * CalculateLaunchForce(_cameraTransform);

        // placing points along the line renderer
        int i = 0;
        m_aimPreview.SetPosition(i, startPosition);
        for (float time = 0; time < m_ssoTorch.PreviewLength; time += m_ssoTorch.PreviewSmoothing)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            // defines placement over time using gravity as an accelerator
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            m_aimPreview.SetPosition(i, point);

            if (CheckEndOfPreview(i, point)) return;
        }
    }


    /// <summary> 
	/// Stop the curve of the previsualisation if it collides with an object.
	/// </summary>
    private bool CheckEndOfPreview(int pointNb, Vector3 pointPos)
    {
        Vector3 lastPosition = m_aimPreview.GetPosition(pointNb - 1);
        if (Physics.Raycast(lastPosition, (pointPos - lastPosition).normalized, out var hit, (pointPos - lastPosition).magnitude, ~(m_ssoTorch.PreviewLayersToIgnore)))
        {
            m_aimPreview.SetPosition(pointNb, hit.point);
            m_aimPreview.positionCount = pointNb + 1;
            return true;
        }
        return false;
    }

    #endregion

    #region THROW
	
    public override bool Throw(Transform _cameraTransform)
    {
		// Assertion
        if (!IsInHand || !m_ssoTorch.CanThrow) return false;

		if (!IsLit)
		{
			ToggleHandEffect();
		}

        m_aimPreview.enabled = false;

        m_lastPosition = transform.position;
        gameObject.transform.parent = null;
		m_rigidbody.constraints = RigidbodyConstraints.None;
		m_rigidbody.velocity = Quaternion.AngleAxis(-CalculateThrowAngleOffset(_cameraTransform), _cameraTransform.right) * _cameraTransform.forward * CalculateLaunchForce(_cameraTransform);
		IsInHand = false;

		StartCoroutine(WaitAndDeactivateTorch(m_ssoTorch.GroundedLightDuration));
        return true;
    }


    private float CalculateThrowAngleOffset(Transform _cameraTransform)
    {
        float cameraAngle = SetUpCameraAngle(_cameraTransform);
        return (-(cameraAngle * cameraAngle) + m_ssoTorch.LaunchCameraAngle.Max * cameraAngle) / 200;
    }

    private float SetUpCameraAngle(Transform _cameraTransform)
    {
        // Setting up the camera angle from just the eulerAngle from a value going from 0 to the difference between min and max camera angle
        float cameraAngle = _cameraTransform.rotation.eulerAngles.x + 60;
        if (cameraAngle > 250) cameraAngle = cameraAngle - 360;
		
        // Setting the inverse since we want the launch force to be highest when the camera is at its lowest
        return m_ssoTorch.LaunchCameraAngle.Max - cameraAngle;
    }

    private float CalculateLaunchForce(Transform _cameraTransform)
    {
        return m_ssoTorch.LaunchForce.Min +
            (Mathf.Clamp(SetUpCameraAngle(_cameraTransform), 0, m_ssoTorch.LaunchCameraAngle.Max / 2) - m_ssoTorch.LaunchCameraAngle.Min) *
            (m_ssoTorch.LaunchForce.Max - m_ssoTorch.LaunchForce.Min) /
            (m_ssoTorch.LaunchCameraAngle.Max / 2 - m_ssoTorch.LaunchCameraAngle.Min);
    }

    private IEnumerator WaitAndDeactivateTorch(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_rsoTorchManager.value.Remove(this);
    }

    public override bool StateInHand()
    {
        return IsInHand;
    }

    #endregion

    #region HEIGHT FEEDBACK

    /// <summary>
    /// Calculate the difference in height between player and torch and update torch state based on that.
    /// </summary>
    private void UpdateTorchFeedback()
    {
		// Assertions
        if (IsInHand) return;
		if (gameObject == null) return;

		if (m_rsoCharacterPosition.value.y - transform.position.y > m_ssoCharacter.LethalHeight)
		{
			if (!m_hasChangedColor)
			{
				DOTween.Kill(gameObject.GetInstanceID() + "feedback");
				m_light.DOColor(m_ssoTorch.DeathColor, 0.5f).SetId(gameObject.GetInstanceID() + "feedback");
				DOTween.To(() => m_emitColor, x => m_emitColor = x, m_ssoTorch.DeathColor, 0.5f).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "feedback")
					.OnUpdate(() => {
						m_propertyBlock.SetColor("_lightColor", m_emitColor);
						m_meshRenderer.SetPropertyBlock(m_propertyBlock);
					});
				m_hasChangedColor = true;
			}
		}
		else
		{
			if (m_hasChangedColor)
			{
				DOTween.Kill(gameObject.GetInstanceID() + "feedback");
				m_light.DOColor(m_ssoTorch.LightColor, 0.5f).SetId(gameObject.GetInstanceID() + "feedback");
				DOTween.To(() => m_emitColor, x => m_emitColor = x, m_ssoTorch.LightColor, 0.5f).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "feedback")
					.OnUpdate(() => {
						m_propertyBlock.SetColor("_lightColor", m_emitColor);
						m_meshRenderer.SetPropertyBlock(m_propertyBlock);
					});
				m_hasChangedColor = false;
			}
		}

		if (m_rsoCharacterPosition.value.y - transform.position.y > m_ssoCharacter.LethalHeight + m_ssoRope.MaxLength)
		{
			m_rsoTorchManager.value.Remove(this);
		}
    }

    /// <summary>
    /// Make torch flicker and desappear.
    /// </summary>
    public void Deactivate()
    {
		if (m_isDeactivate) return;

		if (!m_ssoTorch.ActivateBreakAnim)
		{
			DestroyTorch();
			return;
		}

		m_isDeactivate = true;
		m_rsoCharacterPosition.OnChanged -= UpdateTorchFeedback;

		Sequence _deactivatingSequence = DOTween.Sequence().Pause();
		_deactivatingSequence.AppendInterval(0.03f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = false; });
		_deactivatingSequence.AppendInterval(0.08f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = true; });
		_deactivatingSequence.AppendInterval(0.03f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = false; });
		_deactivatingSequence.AppendInterval(0.03f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = true; });
		_deactivatingSequence.AppendInterval(0.03f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = false; });
		_deactivatingSequence.AppendInterval(0.03f);
		_deactivatingSequence.AppendCallback(() => { m_light.enabled = true; });
		_deactivatingSequence.Insert(0f, m_light.DOIntensity(0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear));
		_deactivatingSequence.Insert(0f, DOTween.To(() => m_light.range, x => m_light.range = x, 0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear));
		_deactivatingSequence.Insert(0f, DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear)
														.OnUpdate(() => {
															m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
															m_meshRenderer.SetPropertyBlock(m_propertyBlock);
														}));
		_deactivatingSequence.SetId(gameObject.GetInstanceID());

		_deactivatingSequence.Play().OnComplete(() => { DestroyTorch(); });
    }

    public void DestroyTorch()
    {
        DOTween.Kill(gameObject.GetInstanceID() + "light");
        DOTween.Kill(gameObject.GetInstanceID() + "feedback");
        DOTween.Kill(gameObject.GetInstanceID());
        m_rsoCharacterPosition.OnChanged -= UpdateTorchFeedback;
        Destroy(gameObject);
    }

    #endregion
}
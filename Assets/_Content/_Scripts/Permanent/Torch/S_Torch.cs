using DG.Tweening;
using EasyCurvedLine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : Permanent
{
    [Title("Internal references")]
    [SerializeField] public Light m_light;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] public MeshRenderer m_meshRenderer;
    [SerializeField] private LineRenderer m_aimLineRenderer;
    [SerializeField] private Transform m_pointLightBase;
    [SerializeField] private SphereCollider m_lightCollider;
	[SerializeField] public Transform RaycastTarget;
	[SerializeField] private Interactable m_interactable;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ssoSoundHit;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ssoSoundBreak;

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraTransform m_rsoCameraTransform;

    [Title("Variables")]
    [SerializeField] private bool m_isSpawned;

    // ----- PUBLIC VARIABLES -----
    [HideInInspector] public bool IsLit;
    [HideInInspector] public bool IsInHand;

    // ----- PRIVATE VARIABLES -----
    private Vector3 m_lastPosition;

    private Vector3 m_throwSpeed;

    private MaterialPropertyBlock m_propertyBlock;

    private bool m_hasChangedColor;
    private bool m_hasPlayedHitSound;
    private bool m_isDeactivate;

    private Coroutine m_thrownCoroutine;

    private float m_lightPercent;
    private Color m_emitColor;

    #region MONOBEHAVIOR

    private void Awake()
    {
        if (!m_isSpawned)
        {
            IsInHand = true;
            m_rigidbody.isKinematic = true;
        }
        m_isDeactivate = false;
        m_hasChangedColor = false;
        m_hasPlayedHitSound = false;

        // Collisions
        m_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        m_lightCollider.radius = m_ssoTorch.LightOffsetDistance;

        if (m_isSpawned) m_rigidbody.constraints = RigidbodyConstraints.None;

        // Preview
        m_aimLineRenderer.useWorldSpace = true;

        // Visual
        m_propertyBlock = new MaterialPropertyBlock();
        m_light.color = m_ssoTorch.LightColor;
        m_light.intensity = m_ssoTorch.LightIntensity;
		m_light.range = m_ssoTorch.LightRange;
        m_propertyBlock.SetColor("_lightColor", m_ssoTorch.LightColor);
		m_meshRenderer.SetPropertyBlock(m_propertyBlock);
    }

    private void Start()
    {
        if (m_ssoTorch.IsStartingLit)
        {
            IsLit = true;
            Shader.SetGlobalFloat("_IS_TORCH_LIT", 1f);
            m_lightPercent = 1f;
            m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
            m_meshRenderer.SetPropertyBlock(m_propertyBlock);
        }

        if(!m_isSpawned) m_rsoTorchManager.value.Add(this);
    }

    private void OnEnable()
    {
		m_interactable.OnInteracted += OnInteracted;
		m_rsoCharacterPosition.OnChanged += UpdateTorchFeedback;
    }

    private void OnDisable()
    {
		m_interactable.OnInteracted -= OnInteracted;
        m_rsoCharacterPosition.OnChanged -= UpdateTorchFeedback;
    }

    private void Update()
    {
        // Assertion
        if (!IsLit) return;

        UpdateLightFlicker();

        // Assertion
        if (IsInHand) return;

		if (HasMoved()) UpdateTorchFeedback();
	}

    private void LateUpdate()
    {
		// TODO - Prevent the penetration test to fire if the torch is immobile.
		// TODO - Use trigger enter and exit to prevent penetration test when there is no collider in range.

		// Assertion
		if (!IsLit) return;

		var lightOffset = Vector3.zero;
		Collider[] hitColliders = Physics.OverlapSphere(m_pointLightBase.position, m_ssoTorch.LightOffsetDistance, m_ssoTorch.LayerLightOffsetToInclude);

		if (hitColliders.Length <= 0)
		{
			m_light.transform.position = m_pointLightBase.position;
			return;
		}

		foreach (Collider otherCollider in hitColliders)
		{
			Vector3 otherPosition = otherCollider.gameObject.transform.position;
			Quaternion otherRotation = otherCollider.gameObject.transform.rotation;

			Vector3 direction;
			float distance;

			bool overlapped = Physics.ComputePenetration(
				m_lightCollider, transform.position, transform.rotation,
				otherCollider, otherPosition, otherRotation,
				out direction, out distance
			);

			if (overlapped)
			{
				lightOffset += direction * distance;
			}

			lightOffset = Vector3.ClampMagnitude(lightOffset, m_ssoTorch.LightOffsetDistance);
			m_light.transform.position = m_pointLightBase.position + lightOffset;
		}
	}   

    private void OnCollisionEnter(Collision collision)
    {
		// Assertion
        if (IsInHand) return;

        m_rigidbody.excludeLayers = m_ssoTorch.LayerToIgnoreAfterHit;

        // If it collide with a flat surface it increase drag to prevent the torch from rolling for eternity
        if (Vector3.Dot(collision.contacts[0].normal, new Vector3(0, 1, 0)) >= 0.8)
		{
			m_rigidbody.linearDamping = 1f;
			m_rigidbody.angularDamping = 1f;
		}

		if (!m_hasPlayedHitSound 
		&& m_rigidbody.linearVelocity.magnitude > m_ssoTorch.MinSpeedForHitSound)
		{
			m_hasPlayedHitSound = true;
			if (m_isDeactivate)
			{
                m_rsePlaySoundAt.Call(m_ssoSoundBreak, transform.position);
				DOTween.Sequence().AppendInterval(m_ssoTorch.DesactivatingTime).SetId(gameObject.GetInstanceID()).OnComplete(() => { m_hasPlayedHitSound = false; });
			}
			else
			{
                m_rsePlaySoundAt.Call(m_ssoSoundHit, transform.position);
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
            Shader.SetGlobalFloat("_IS_TORCH_LIT", 0f);
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 0f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
                .OnUpdate(() =>
                {
                    m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
                    m_meshRenderer.SetPropertyBlock(m_propertyBlock);
                });
            m_light.DOIntensity(0f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
        }
		else
		{
            IsLit = true;
            Shader.SetGlobalFloat("_IS_TORCH_LIT", 1f);
            m_light.enabled = true;
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 1f, m_ssoTorch.UnlitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
               .OnUpdate(() =>
               {
                   m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
                   m_meshRenderer.SetPropertyBlock(m_propertyBlock);
               });
            m_light.DOIntensity(m_ssoTorch.LightIntensity, m_ssoTorch.LitDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
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

    private void UpdateLightFlicker()
    {
        float bigWaveFreq = 2.6f;
        float bigWaveAmp = 0.11f;
        float midWaveFreq = -3.8f;
        float midWaveAmp = 0.05f;
        float smallWaveFreq = -9.4f;
        float smallWaveAmp = 0.03f;

        float flickerFactor = 1 - bigWaveAmp - bigWaveAmp*Mathf.Sin(Time.time*bigWaveFreq) - midWaveAmp*Mathf.Sin(Time.time*midWaveFreq) - smallWaveAmp*Mathf.Sin(Time.time*smallWaveFreq);
        m_light.intensity = m_ssoTorch.LightIntensity * flickerFactor;
    }

    #endregion

    #region PREVIEW

    /// <summary> 
	/// Torch previsualisation with the camera's transform for the direction.
	/// </summary>
    public override void PreviewThrow(Transform _cameraTransform)
    {
        m_aimLineRenderer.enabled = true;

        List<Vector3> trajectoryPoints = new List<Vector3> ();
        trajectoryPoints.Add(transform.position);

        float trajectoryDistance = 0;
        Vector3 targetPoint;

        if (Physics.Raycast(m_rsoCameraTransform.value.position, m_rsoCameraTransform.value.forward, out RaycastHit hit, m_ssoTorch.ThrowRange, ~m_ssoTorch.PreviewLayersToIgnore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = transform.position + m_rsoCameraTransform.value.forward * m_ssoTorch.ThrowRange;
        }

        float throwDuration = (targetPoint - transform.position).magnitude / (m_ssoTorch.ThrowMaxSpeed * m_ssoTorch.SpeedRangeCurve.Evaluate((targetPoint - transform.position).magnitude / m_ssoTorch.ThrowRange));

        m_throwSpeed = (targetPoint - transform.position) / throwDuration - (Physics.gravity * throwDuration);

        for (int i = 0; i < m_ssoTorch.PreviewPhysicAccuracy; i++)
        {
            if(Physics.Linecast(transform.position + (m_throwSpeed + Physics.gravity * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * i)) * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * i),
                transform.position + (m_throwSpeed + Physics.gravity * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * (i + 1))) * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * (i + 1)),
                out RaycastHit hitCurve,
                ~m_ssoTorch.PreviewLayersToIgnore
                ))
            {
                trajectoryPoints.Add(hitCurve.point);
                trajectoryDistance += (trajectoryPoints[^1] - trajectoryPoints[^2]).magnitude;
                break;
            }
            else
            {
                trajectoryPoints.Add(transform.position + (m_throwSpeed + Physics.gravity * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * (i + 1))) * (throwDuration / m_ssoTorch.PreviewPhysicAccuracy * (i + 1)));
                trajectoryDistance += (trajectoryPoints[^1] - trajectoryPoints[^2]).magnitude;
            }
        }

        Vector3[] smoothedPoints = LineSmoother.SmoothLine(trajectoryPoints, m_ssoTorch.LineSegmentSize);

        // set line settings
        m_aimLineRenderer.positionCount = smoothedPoints.Length;
        m_aimLineRenderer.SetPositions(smoothedPoints);
        m_aimLineRenderer.startWidth = m_ssoTorch.LineWidth;
        m_aimLineRenderer.endWidth = m_ssoTorch.LineWidth;

        float fadeInDistancePercent = (m_ssoTorch.FadeInDistance < trajectoryDistance * 0.25f) ? (m_ssoTorch.FadeInDistance / trajectoryDistance) : 0.25f;

        Gradient gradient = new Gradient();

        // Set color
        GradientColorKey[] colors = new GradientColorKey[3];
        colors[0] = new GradientColorKey(new Color(255f, 229f, 0), 0.0f);
        colors[1] = new GradientColorKey(new Color(255f, 229f, 0), fadeInDistancePercent);
        colors[2] = new GradientColorKey(new Color(255f, 229f, 0), 1.0f);

        // Blend alpha from alpha at 0% to opaque at fade in distance to transparent at 100%
        GradientAlphaKey[] alphas = new GradientAlphaKey[3];
        alphas[0] = new GradientAlphaKey(0.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, fadeInDistancePercent);
        alphas[2] = new GradientAlphaKey(0.0f, 1.0f);

        gradient.SetKeys(colors, alphas);

        m_aimLineRenderer.colorGradient = gradient;
    }

    public override void DisablePreview()
    {
        m_aimLineRenderer.enabled = false;
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

		m_rigidbody.isKinematic = false;

        DisablePreview();

        m_lastPosition = transform.position;
        gameObject.transform.parent = null;
        m_rigidbody.constraints = RigidbodyConstraints.None;
        m_rigidbody.AddForce(m_throwSpeed, ForceMode.Impulse);
        IsInHand = false;

        m_thrownCoroutine = StartCoroutine(WaitAndDeactivateTorch(m_ssoTorch.GroundedLightDuration));
        return true;
    }

    public IEnumerator WaitAndDeactivateTorch(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_rsoTorchManager.value.Remove(this, true);
    }

    public override bool StateInHand()
    {
        return IsInHand;
    }

    public void DetachAndCancelDeactivation()
    {
        if (m_thrownCoroutine == null) return;
        StopCoroutine(m_thrownCoroutine);
        m_rsoTorchManager.value.Remove(this, false);
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
			m_rsoTorchManager.value.Remove(this, true);
		}
    }

	private void OnInteracted()
	{
		m_rsoTorchManager.value.Remove(this, true);
	}

	/// <summary>
	/// Make torch flicker and desappear.
	/// </summary>
	public void Desactivate()
    {
		if (m_isDeactivate) return;

		if (!m_ssoTorch.ActivateBreakAnim)
		{
			DestroyTorch();
			return;
		}

		m_isDeactivate = true;
		m_rsoCharacterPosition.OnChanged -= UpdateTorchFeedback;

		Sequence flickeringSequence = DOTween.Sequence().Pause();
		flickeringSequence.AppendInterval(0.03f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = false; });
		flickeringSequence.AppendInterval(0.08f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = true; });
		flickeringSequence.AppendInterval(0.03f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = false; });
		flickeringSequence.AppendInterval(0.03f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = true; });
		flickeringSequence.AppendInterval(0.03f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = false; });
		flickeringSequence.AppendInterval(0.03f);
		flickeringSequence.AppendCallback(() => { m_light.enabled = true; });
		flickeringSequence.Insert(0f, m_light.DOIntensity(0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear));
		flickeringSequence.Insert(0f, DOTween.To(() => m_light.range, x => m_light.range = x, 0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear));
		flickeringSequence.Insert(0f, DOTween.To(() => m_lightPercent, x => m_lightPercent = x, 0f, m_ssoTorch.DesactivatingTime).SetEase(Ease.Linear)
							.OnUpdate(() => {
								m_propertyBlock.SetFloat("_lightPercent", m_lightPercent);
								m_meshRenderer.SetPropertyBlock(m_propertyBlock);
							}));

		if (gameObject != null) 
		{
			flickeringSequence.SetId(gameObject.GetHashCode()); // Unsafe version of GetInstanceID()
			flickeringSequence.Play().OnComplete(() => { DestroyTorch(); });
		}
		else 
		{
			DestroyTorch();
		}
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
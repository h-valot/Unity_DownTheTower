using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Interactable
{
    [Title("External references")]
    [SerializeField] private List<Switchable> m_switchables = new List<Switchable>();
    [SerializeField] private RSE_PlaySound m_rsePlaySound;
    [SerializeField] private RSE_PlayAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoLeverActivate;
    [SerializeField] private SSO_Sound m_ssoLeverDeactivate;

    [Title("Tweakable values")]
    [SerializeField] private bool m_isActivated = false;

    [Title("Graphic elements")]
    [SerializeField] private GameObject m_handleOrigin;
    [SerializeField] private MeshRenderer m_meshRendererBase;
    [SerializeField] private float EmitStrengthActive;
    [SerializeField] private float EmitStrengthInactive;

    private MaterialPropertyBlock m_propertyBlock;
    private float m_currentEmitStrength;

    private void Awake()
    {
        m_propertyBlock = new MaterialPropertyBlock();
        m_currentEmitStrength = EmitStrengthActive;
        m_propertyBlock.SetFloat("EmitStrength", m_currentEmitStrength);
        m_meshRendererBase.SetPropertyBlock(m_propertyBlock);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    public override void InteractionTrigger()
    {
        m_isActivated = !m_isActivated;

        foreach (var switchable in m_switchables) 
		{
			switchable.SwitchBehavior(m_isActivated);
		}

        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        if (m_isActivated)
        {
            m_handleOrigin.transform.DOLocalRotate(new Vector3(180, 0, 0), 0.5f).SetTarget(this);
            DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, EmitStrengthInactive, 0.5f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererBase.SetPropertyBlock(m_propertyBlock);
                            });
            m_rsePlayAt.Call(m_ssoLeverActivate, this.transform.position);
        }
        else
        {
            m_handleOrigin.transform.DOLocalRotate(new Vector3(90, 0, 0), 0.5f).SetTarget(this);
            DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, EmitStrengthActive, 0.5f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererBase.SetPropertyBlock(m_propertyBlock);
                            });
            m_rsePlayAt.Call(m_ssoLeverDeactivate, this.transform.position);
        }
    }
}

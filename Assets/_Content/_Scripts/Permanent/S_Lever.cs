using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Lever : Interactable
{
    [Title("External references")]
    [SerializeField] private List<Switchable> m_switchables = new List<Switchable>();
    [SerializeField] private RSE_PlaySound m_rsePlaySound;
    [SerializeField] private RSE_PlaySoundAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoLeverActivate;
    [SerializeField] private SSO_Sound m_ssoLeverDeactivate;
    [SerializeField] private GameObject m_navLink1;
    [SerializeField] private GameObject m_navLink2;

    [Title("Tweakable values")]
    [SerializeField] private bool m_isActivated = false;

    [Title("Graphic elements")]
    [SerializeField] private GameObject m_handleOrigin;
    [SerializeField] private MeshRenderer m_meshRendererBase;
    [SerializeField] private float EmitStrengthActive;
    [SerializeField] private float EmitStrengthInactive;

    private MaterialPropertyBlock m_propertyBlock;
    private float m_currentEmitStrength;
    private bool m_Link;

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
        if (m_navLink1 != null)
        {
            if (m_Link == false)
            {
                m_navLink1.SetActive(true);
                m_navLink2.SetActive(true);
                m_Link = true;
            }
            else
            {
                m_navLink1.SetActive(false);
                m_navLink2.SetActive(false);
                m_Link = false;
            }
        }

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

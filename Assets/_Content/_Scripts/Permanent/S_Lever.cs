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

    [Title("TEMP: graphic elements")]
    [SerializeField] private GameObject m_handleOrigin;
    [SerializeField] private GameObject m_gauge;

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
            m_handleOrigin.transform.DOLocalRotate(new Vector3(90, 0, 0), 0.5f);
            m_gauge.transform.DOScaleY(0.8f, 0.5f);
            m_rsePlayAt.Call(m_ssoLeverActivate, this.transform.position);
        }
        else
        {
            m_handleOrigin.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f);
            m_gauge.transform.DOScaleY(0.1f, 0.5f);
            m_rsePlayAt.Call(m_ssoLeverDeactivate, this.transform.position);
        }
    }
}

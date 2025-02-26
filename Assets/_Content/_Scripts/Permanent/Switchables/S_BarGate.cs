using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class BarGate : Switchable
{
    [Title("References")]
    [SerializeField] private RSE_PlayAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoDoorOpen;
    [SerializeField] private SSO_Sound m_ssoDoorClose;

    [Title("Internal References")]
    [SerializeField] private GameObject m_bar;

    [Title("Tweakable values")]
    [SerializeField] private float m_maxScale = 1f;
    [SerializeField] private float m_minScale = 0.1f;
    [SerializeField] private float m_animLength = 1f;

    protected override void ActivateMechanism()
    {
        m_bar.transform.DOScaleY(m_minScale, m_animLength);
        m_rsePlayAt.Call(m_ssoDoorOpen, this.transform.position);
    }

    protected override void DeactivateMechanism()
    {
        m_bar.transform.DOScaleY(m_maxScale, m_animLength);
        m_rsePlayAt.Call(m_ssoDoorClose, this.transform.position);
    }
}

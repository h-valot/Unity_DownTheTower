using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gate : Switchable
{
    [Title("References")]
    [SerializeField] private RSE_PlayAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoDoorOpen;
    [SerializeField] private SSO_Sound m_ssoDoorClose;

    [Title("Tweakable values")]
    [SerializeField] private float m_maxHeight = 1;
    [SerializeField] private float m_animLength = 1;

    protected override void ActivateMechanism()
    {
        transform.DOScaleY(0.01f, 1);
        m_rsePlayAt.Call(m_ssoDoorOpen, this.transform.position);
    }

    protected override void DeactivateMechanism()
    {
        transform.DOScaleY(m_maxHeight, m_animLength);
        m_rsePlayAt.Call(m_ssoDoorClose, this.transform.position);
    }
}

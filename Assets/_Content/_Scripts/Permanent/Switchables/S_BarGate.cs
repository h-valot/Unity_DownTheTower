using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class BarGate : Switchable
{
    [Title("Internal References")]
    [SerializeField] private GameObject m_bar;

    [Title("Tweakable values")]
    [SerializeField] private float m_maxScale = 1f;
    [SerializeField] private float m_minScale = 0.1f;
    [SerializeField] private float m_animLength = 1f;

    protected override void ActivateMechanism()
    {
        m_bar.transform.DOScaleY(m_minScale, m_animLength);
    }

    protected override void DeactivateMechanism()
    {
        m_bar.transform.DOScaleY(m_maxScale, m_animLength);
    }
}

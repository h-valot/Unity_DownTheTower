using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gate : Switchable
{
    [Title("Tweakable values")]
    [SerializeField] private float m_maxHeight = 1;
    [SerializeField] private float m_animLength = 1;

    protected override void ActivateMechanism()
    {
        transform.DOScaleY(0.01f, m_animLength);
    }

    protected override void DeactivateMechanism()
    {
        transform.DOScaleY(m_maxHeight, m_animLength);
    }
}

using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : Switchable
{
    [Title("Tweakable values")]
    [SerializeField] private Vector3 m_maxRotation;
    [SerializeField] private float m_animLength = 1;

    protected override void ActivateMechanism()
    {
        transform.DOLocalRotate(m_maxRotation, m_animLength);
    }

    protected override void DeactivateMechanism()
    {
        // Deactivation is not working, it need to be link with the starting rotation of the prefab
        transform.DOLocalRotate(m_maxRotation, m_animLength);
    }
}

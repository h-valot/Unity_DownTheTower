using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDoor : LightReciever
{
    [Title("Tweakable values")]
    [SerializeField] private float m_animLength = 1;

    private float m_height;

    private void OnEnable()
    {
        m_height = transform.localScale.y;
    }

    protected override void ActivateMechanism()
    {
        transform.DOMoveY(transform.position.y - m_height - 0.5f, m_animLength);
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : Switchable
{
    [Header("Tweakable Variables")]
    [SerializeField] private float _maxHeight = 1;
    [SerializeField] private float _animLength = 1;

    protected override void ActivateMechanism()
    {
        transform.DOScaleY(0.01f, 1);
    }

    protected override void DeactivateMechanism()
    {
        transform.DOScaleY(_maxHeight, _animLength);
    }
}

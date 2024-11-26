using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : Interactable
{
    [Header("External References")]
    [SerializeField] private List<Switchable> _switchableList;

    [Header("Tweakable Variables")]
    [SerializeField] private bool _isActivated = false;

    [Header("Graphic Elements (TEMP)")]
    [SerializeField] private GameObject _handleOrigin;
    [SerializeField] private GameObject _gauge;

    public override void InteractionTrigger()
    {
        _isActivated = !_isActivated;

        foreach (Switchable switchable in _switchableList) switchable.SwitchBehavior(_isActivated);

        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        if (_isActivated)
        {
            _handleOrigin.transform.DORotate(new Vector3(0, 0, 30), 0.5f);
            _gauge.transform.DOScaleY(0.8f, 0.5f);
        }
        else
        {
            _handleOrigin.transform.DORotate(new Vector3(0, 0, 150), 0.5f);
            _gauge.transform.DOScaleY(0.1f, 0.5f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switchable : MonoBehaviour
{
    [Header("Switchable")]
    [SerializeField] protected bool _isReversed = false;

    public void SwitchBehavior(bool isLeverActive)
    {
        // In case there is a need to do the "on" behavior when the lever is "off"
        if (_isReversed) isLeverActive = !isLeverActive;

        if (isLeverActive) ActivateMechanism();
        else DeactivateMechanism();
    }

    protected virtual void ActivateMechanism()
    {

    }

    protected virtual void DeactivateMechanism()
    {

    }
}

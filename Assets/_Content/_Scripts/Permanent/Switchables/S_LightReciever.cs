using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : Switchable
{
    [Header("Properties")]
    [SerializeField] private int _lightsToActivate;

    // PRIVATE VARIABLES
    private int _currentLights = 0;
    private bool _isActivated = false;

    public void AddToLightCounter()
    {
        _currentLights++;
        if(_currentLights >= _lightsToActivate && !_isActivated)
        {
            ActivateMechanism();
            _isActivated = true;
        }
    }

}

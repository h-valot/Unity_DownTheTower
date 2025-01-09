using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDoor : Switchable
{
    [Header("Properties")]
    [SerializeField] private int _lightsToActivate;
    [SerializeField] private List<LightBasket> _sources;

    // PRIVATE VARIABLES
    private int _currentLights;
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

    protected override void ActivateMechanism()
    {

    }
}

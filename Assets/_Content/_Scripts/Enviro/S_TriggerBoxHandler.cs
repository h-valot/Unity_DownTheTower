using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class S_TriggerBoxHandler : MonoBehaviour
{
    [SerializeField] private ToxicGas _toxicGas;

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _toxicGas.CharacterHasEnter(_character);
        }

        if (other.TryGetComponent<Torch>(out var _torch))
        {
            _toxicGas.TorchHasEnter(_torch);
        }
    }
}

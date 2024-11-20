using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private Guardian _GuardianRef;

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _GuardianRef.RemovePotentialTargets(_playerCheckRef.gameObject);
            //StartCoroutine(_GuardianRef.KillPlayer());
            //_playerCheckRef.HandleDeath();
        }

        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            if (_torchCheckRef!=null)

                if (!_torchCheckRef.StateInHand())
                {
                    {
                        _GuardianRef.RemovePotentialTargets(_torchCheckRef.gameObject);
                        Debug.Log("Je vire la ref");
                    }
                    _GuardianRef.destroyTorchCoroutine = StartCoroutine(_GuardianRef.DestroyTorchTime(_torchCheckRef.gameObject));
                }
        }

        
    }
}

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
            StartCoroutine(_GuardianRef.KillPlayer());
            //_playerCheckRef.HandleDeath();
        }

        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            if (_torchCheckRef!=null)
            {
                Destroy(_torchCheckRef.gameObject);
            }
            StartCoroutine(_GuardianRef.DestroyTorchTime());
        }

        
    }
}

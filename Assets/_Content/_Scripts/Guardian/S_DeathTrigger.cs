using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private Guardian _GuardianRef;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.ToString());
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _playerCheckRef.HandleDeath();
            _GuardianRef.DestroyedTarget();
        }

        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            if (_torchCheckRef!=null)
            {
                Destroy(_torchCheckRef.gameObject);
                _GuardianRef.DestroyedTarget();
            }
        }

        
    }
}

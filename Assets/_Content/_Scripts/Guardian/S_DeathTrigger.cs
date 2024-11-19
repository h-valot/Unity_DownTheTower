using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.ToString());
        if (other.TryGetComponent<OldCharacterMotor>(out var _playerCheckRef))
        {
            _playerCheckRef.HandleDeath();
        }

        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            if (_torchCheckRef!=null)
            {
                Destroy(_torchCheckRef.gameObject);
            }
        }

        
    }
}

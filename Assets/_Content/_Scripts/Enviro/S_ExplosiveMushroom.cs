using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMushroom : MonoBehaviour
{



    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.HandleDeath();
        }
        
        if (other.TryGetComponent<Torch>(out var _torch))
        {
            Debug.Log(this.ToString());
            Destroy(_torch.gameObject);
            Destroy(gameObject);
        }
    }

}

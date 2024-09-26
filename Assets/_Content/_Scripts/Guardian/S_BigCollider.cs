using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;


    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _guardianRef.MakePLayerRef(_playerCheckRef);
            _guardianRef.PlayerStayIn();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _guardianRef.PlayerExit();
        }
    }
}

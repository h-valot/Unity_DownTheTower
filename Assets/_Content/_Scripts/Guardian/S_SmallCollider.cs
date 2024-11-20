using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;


    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _playerCheckRef))
        {
            if (_playerCheckRef.CraftInHand && _playerCheckRef.CraftInRobot) { }
            _guardianRef.MakePLayerRef(_playerCheckRef);
            _guardianRef.TargetStayIn();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _playerCheckRef) )
        {
            _guardianRef.TargetExit();
        }
    }
}

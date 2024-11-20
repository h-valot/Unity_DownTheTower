using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            if (_playerCheckRef._craftInHand && _playerCheckRef._craftInRobot) { }
            //_guardianRef.MakePLayerRef(_playerCheckRef);
            //_guardianRef.TargetStayIn();
            _guardianRef.AddToPotentialTargets(_playerCheckRef.gameObject);
        }
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            //_guardianRef.MakeTorchRef(_torchCheckRef);
            //_guardianRef.TargetStayIn();
            _guardianRef.AddToPotentialTargets(_torchCheckRef.gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            //_guardianRef.TargetExit();
            _guardianRef.RemovePotentialTargets(_torchCheckRef.gameObject);
        }

        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef) )
        {
            //_guardianRef.TargetExit();
            _guardianRef.RemovePotentialTargets(_playerCheckRef.gameObject);
        }
    }
}

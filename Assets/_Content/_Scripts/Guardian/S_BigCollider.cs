using UnityEngine;
using static CharacterMotor;

public class BigCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            Debug.Log("torche en vue");
            //_guardianRef.MakeTorchRef(_torchCheckRef);
            //_guardianRef.TargetStayIn();
            _guardianRef.AddToPotentialTargets(_torchCheckRef.gameObject);
        }

        else if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            Debug.Log("joueur");
            if ((_playerCheckRef._craftInHand._craftType == CraftType.TORCH && _playerCheckRef._craftInHand.StateInHand())
                || (_playerCheckRef._craftInRobot._craftType == CraftType.TORCH && _playerCheckRef._craftInRobot.StateInHand()))
            {
                Debug.Log("detecte torchonplayer");
                //_guardianRef.MakePLayerRef(_playerCheckRef);
                //_guardianRef.TargetStayIn();
                _guardianRef.AddToPotentialTargets(_playerCheckRef.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            Debug.Log("Torche plus dans la range");
            //_guardianRef.TargetExit();
            _guardianRef.RemovePotentialTargets(_torchCheckRef.gameObject);
        }

        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            Debug.Log("plus dans la range");
            _guardianRef.RemovePotentialTargets(_playerCheckRef.gameObject);
        }
    }
}

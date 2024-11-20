using UnityEngine;
using static OldCharacterMotor;

public class BigCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;


    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            Debug.Log("torche en vue");
            _guardianRef.MakeTorchRef(_torchCheckRef);
            _guardianRef.TargetStayIn();
        }

        else if (other.TryGetComponent<OldCharacterMotor>(out var _playerCheckRef))
        {
            Debug.Log("joueur");
            if ((_playerCheckRef.CraftInHand.Type == CraftType.TORCH && _playerCheckRef.CraftInHand.StateInHand())
                || (_playerCheckRef.CraftInRobot.Type == CraftType.TORCH && _playerCheckRef.CraftInRobot.StateInHand()))
            {
                Debug.Log("detecte torchonplayer");
                _guardianRef.MakePLayerRef(_playerCheckRef);
                _guardianRef.TargetStayIn();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _playerCheckRef) || other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            Debug.Log("plus dans la range");
            _guardianRef.TargetExit();
        }
    }
}

using UnityEngine;
using static CharacterMotor;

public class BigCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            _guardianRef.AddToPotentialTargets(_torchCheckRef.gameObject);
        }

        else if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            if (_playerCheckRef._craftInHand != null)
            {
                if ((_playerCheckRef._craftInHand._craftType == CraftType.TORCH && _playerCheckRef._craftInHand.StateInHand())
                || (_playerCheckRef._craftInRobot._craftType == CraftType.TORCH && _playerCheckRef._craftInRobot.StateInHand()))
                {
                    _guardianRef.AddToPotentialTargets(_playerCheckRef.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            _guardianRef.RemovePotentialTargets(_torchCheckRef.gameObject);
        }

        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _guardianRef.RemovePotentialTargets(_playerCheckRef.gameObject);

        }
    }
}

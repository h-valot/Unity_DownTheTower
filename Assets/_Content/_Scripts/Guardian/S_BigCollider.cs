using UnityEngine;

public class BigCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            _guardianRef.AddToPotentialTargets(_torchCheckRef.gameObject);
        }

        else if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            if (character.HandObject != null)
            {
                if ((character.HandObject.Type == CraftType.TORCH && character.HandObject.StateInHand())
                || (character.RobotObject.Type == CraftType.TORCH && character.RobotObject.StateInHand()))
                {
                    _guardianRef.AddToPotentialTargets(character.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var torch))
        {
            _guardianRef.RemovePotentialTargets(torch.gameObject);
        }

        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _guardianRef.RemovePotentialTargets(character.gameObject);

        }
    }
}

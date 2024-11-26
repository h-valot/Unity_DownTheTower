using UnityEngine;

public class SmallCollider : MonoBehaviour
{
    [SerializeField] private Guardian _guardianRef;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            if (character.HandObject && character.RobotObject) { }
            _guardianRef.AddToPotentialTargets(character.gameObject);
        }
        if (other.TryGetComponent<Torch>(out var _torchCheckRef))
        {
            _guardianRef.AddToPotentialTargets(_torchCheckRef.gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Torch>(out var torch))
        {
            _guardianRef.RemovePotentialTargets(torch.gameObject);
        }

        if (other.TryGetComponent<CharacterMotor>(out var character) )
        {
            _guardianRef.RemovePotentialTargets(character.gameObject);
        }
    }
}

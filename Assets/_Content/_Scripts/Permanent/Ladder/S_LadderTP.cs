using UnityEngine;

public class LadderTP : Interactible
{
    public Vector3 _teleportTo;
    private CharacterMotor _character;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out _character))
        {
            _character.AddToInteractList(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out _character))
        {
            _character.RemoveFromInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
        _character.transform.position = _teleportTo;

        Physics.SyncTransforms();
    }

    public void SetVariables(Vector3 _origin, Vector3 _destination)
    {
        transform.position = _origin;
        _teleportTo = _destination;
    }
}

using UnityEngine;

public class LadderTP : Interactible
{
	[Header("Scriptable references")]
	[SerializeField] private RSE_SetCharacterPosition _rseSetCharacterPosition;

    [HideInInspector] public Vector3 _teleportTo;
    private CharacterMotor _character;

    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out _character))
        {
            _character.AddToInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
		_rseSetCharacterPosition.Call(_teleportTo, Quaternion.identity);
    }

    public void SetVariables(Vector3 _origin, Vector3 _destination)
    {
        transform.position = _origin;
        _teleportTo = _destination;
    }
}
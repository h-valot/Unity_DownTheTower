using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorInteract : Interactible
{
    [Header("Internal Variables")]
    [SerializeField] private Elevator _elevator;

    // --- PRIVATE VARIABLES ---
    private CharacterMotor _character;

    public override void InteractionTrigger()
    {
        _elevator.StartElevator();
        _character.RemoveFromInteractList(this);
    }


    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _character = character;
            character.AddToInteractList(this);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            character.RemoveFromInteractList(this);
            _character = null;
        }
    }

}

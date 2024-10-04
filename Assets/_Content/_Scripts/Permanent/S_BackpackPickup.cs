using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BackpackPickup : Interactible
{
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
        _character.ToggleCraftInput(true);
        _character.RemoveFromInteractList(this);
        Destroy(this.gameObject);
    }
}

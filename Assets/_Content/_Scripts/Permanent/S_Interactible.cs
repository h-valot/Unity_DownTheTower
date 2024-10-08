using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
   
    public virtual void InteractionTrigger()
    {

    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.AddToInteractList(this);
        }
    }
    public virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.RemoveFromInteractList(this);
        }
    }

}

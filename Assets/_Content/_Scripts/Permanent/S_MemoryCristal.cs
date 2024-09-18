using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MemoryCristal : Interactible
{

    [SerializeField] private GameObject _door;

    private bool _doorOpen;
    private Animation _animDoor;


    private void Start()
    {
        _animDoor = _door.GetComponent<Animation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out CharacterMotor _player))
        {
            _player.AddToInteractList(this);
            Debug.Log(this.name);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out CharacterMotor _player))
        {
            _player.RemoveFromInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
        if (_doorOpen == false)
        {
            _animDoor.Play();
            _doorOpen = true;
        }

        if (_doorOpen == true )
        {
            _animDoor.Rewind();
            _doorOpen = false;
        }
    }
}

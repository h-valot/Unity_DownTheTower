using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MemoryCristal : Interactible
{

    [SerializeField] private GameObject _door;
    private GameObject _self;
    private bool _doorOpen;
    private Animation _animDoor;


    private void Start()
    {
        _animDoor = _door.GetComponent<Animation>();
        _self = this.GetComponent<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out CharacterMotor _player))
        {
            _player._interactibleObject = _self;

              
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out CharacterMotor _player))
        {
            _player._interactibleObject = null;


        }
    }

    public override void InteractionTrigger()
    {
        _animDoor.Play();
    }
}

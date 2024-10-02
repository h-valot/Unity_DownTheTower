using DG.Tweening;
using UnityEngine;

public class MemoryCristal : Interactible
{

    [SerializeField] private GameObject _door;
    [SerializeField] private Vector3 _openvector;

    private bool _doorOpen;


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.AddToInteractList(this);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.RemoveFromInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
        if (_doorOpen == false)
        {
            _door.transform.DOMove(_door.transform.position + _openvector, 3f).SetId("Door");
            _doorOpen = true;
        }
    }

    private void Animation()
    {
        
    }
}

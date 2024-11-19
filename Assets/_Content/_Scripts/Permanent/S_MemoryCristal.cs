using DG.Tweening;
using UnityEngine;

public class MemoryCristal : Interactible
{
    [SerializeField] private GameObject _firstDoor;
    [SerializeField] private GameObject _secondDoor;
    [SerializeField] private Vector3 _openvector;

    private bool _doorOpen;
    private void Start()
    {
        _firstDoor.transform.position += _openvector;
    }

    public override void InteractionTrigger()
    {
        if (!_doorOpen)
        {
            MoveDoor(true, -_openvector);
            MoveDoor(false, _openvector);
            _doorOpen = true;
        }
    }

    public void MoveDoor(bool isFirst, Vector3 direction)
    {
        if (isFirst) _firstDoor.transform.DOMove(_firstDoor.transform.position + direction, 3f);
        else _secondDoor.transform.DOMove(_secondDoor.transform.position + direction, 3f);
    }

    private void Animation()
    {
        
    }
}
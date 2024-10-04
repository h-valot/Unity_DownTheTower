using DG.Tweening;
using UnityEngine;

public class MemoryCristal : Interactible
{

    [SerializeField] private GameObject _door;
    [SerializeField] private Vector3 _openvector;

    private bool _doorOpen;

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

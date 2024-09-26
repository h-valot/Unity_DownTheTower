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
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            Debug.Log(this.name);
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
            _doorOpen = true;
            Debug.Log("Animation lancée");
            _animDoor.Play();
        }
    }
}

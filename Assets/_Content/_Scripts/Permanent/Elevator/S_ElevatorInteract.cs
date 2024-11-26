using UnityEngine;

public class ElevatorInteract : Interactable
{
    [Header("Internal Variables")]
    [SerializeField] private Elevator _elevator;

    // --- PRIVATE VARIABLES ---
    private CharacterInteract m_characterInteract;

    public override void InteractionTrigger()
    {
        _elevator.StartElevator();
        m_characterInteract.Remove(this);
    }


    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterInteract>(out var character))
        {
            m_characterInteract = character;
            character.Add(this);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterInteract>(out var character))
        {
            character.Remove(this);
            m_characterInteract = null;
        }
    }

}

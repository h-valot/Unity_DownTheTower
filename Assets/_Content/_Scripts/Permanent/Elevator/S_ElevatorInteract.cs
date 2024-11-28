using Sirenix.OdinInspector;
using UnityEngine;

public class ElevatorInteract : Interactable
{
    [Title("Internal references")]
    [SerializeField] private Elevator m_elevator;

    private CharacterInteract m_characterInteract;

    public override void InteractionTrigger()
    {
        m_elevator.StartElevator();
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
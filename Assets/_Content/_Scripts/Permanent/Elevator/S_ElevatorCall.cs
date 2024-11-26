using UnityEngine;

public class ElevatorCall : Interactable
{
    [Header("External Variables")]
    [SerializeField] private Elevator _elevator;

    [Header("Tweakable Variables")]
    [SerializeField] private bool callToTop = false;

    public override void InteractionTrigger()
    {
        if(callToTop && !_elevator.isUp) _elevator.Ascend();
        if(!callToTop && _elevator.isUp) _elevator.Descend();
        UpdateGraphics();
    }

    private void UpdateGraphics()
    {

    }
}
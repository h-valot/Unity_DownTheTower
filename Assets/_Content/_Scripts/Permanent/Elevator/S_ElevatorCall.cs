using Sirenix.OdinInspector;
using UnityEngine;

public class ElevatorCall : Interactable
{
    [Title("Tweakable references")]
    [SerializeField] private bool m_callToTop = false;

    [Title("External references")]
    [SerializeField] private Elevator m_elevator;

    public override void InteractionTrigger()
    {
        if (m_callToTop && !m_elevator.IsUp) m_elevator.Ascend();
        if (!m_callToTop && m_elevator.IsUp) m_elevator.Descend();
        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
		// TODO
    }
}
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ElevatorButton : Interactable
{
    [Title("Tweakable values")]
    [SerializeField] private bool m_isUpButton;

    [Title("External references")]
    [SerializeField] private Elevator m_elevator;

    [Title("Internal references")]
    [SerializeField] private GameObject m_button;

    private void OnDisable()
    {
        DOTween.Kill(GetInstanceID());
    }

    public override void InteractionTrigger()
    {
        if(m_elevator.IsUp && !m_isUpButton)
        {
            m_elevator.Descend();
        }
        else if(!m_elevator.IsUp && m_isUpButton)
        {
            m_elevator.Ascend(); 
        }

        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        m_button.transform.DOLocalMoveX(0.4f, 0.2f).SetId(GetInstanceID()).OnComplete(() => { m_button.transform.DOLocalMoveX(0.16f, 0.2f).SetId(GetInstanceID()); });
    }
}

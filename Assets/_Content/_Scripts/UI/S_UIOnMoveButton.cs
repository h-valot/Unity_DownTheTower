using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class UIOnMoveButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [FoldoutGroup("Tweakable values")]
    [SerializeField] private bool moveToRight = false;
    [SerializeField] private float moveAmount;

    [FoldoutGroup("Internal references")]
    [InfoBox("It will be moved to the chosen direction slightly", InfoMessageType.None)]
    [SerializeField] private GameObject m_graphicsParent;

    [FoldoutGroup("External references")]
    [InfoBox("It will be moved to the chosen direction slightly", InfoMessageType.None)]
    [SerializeField] private RSO_CurrentControls m_rsoCurrentControls;

    private float m_baseLocalPosX;

    private void Start()
    {
        m_baseLocalPosX = m_graphicsParent.transform.localPosition.x;
    }

    public void OnSelect(BaseEventData eventData)
    {
        print("selected: " + this.gameObject.name);
        if (m_rsoCurrentControls.value != ControlType.GAMEPAD) return;
        ToggleMove(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ToggleMove(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("highlighted: " + this.gameObject.name);
        if (m_rsoCurrentControls.value != ControlType.KEYBOARDMOUSE) return;
        ToggleMove(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleMove(false);
    }

    private void ToggleMove(bool value)
    {
        if(value) m_graphicsParent.transform.DOLocalMoveX(m_baseLocalPosX + (moveToRight ? 1 : -1) * moveAmount, 0.5f).SetUpdate(true).SetEase(Ease.OutQuint);
        else m_graphicsParent.transform.DOLocalMoveX(m_baseLocalPosX, 0.5f).SetUpdate(true).SetEase(Ease.OutQuint);
    }
}

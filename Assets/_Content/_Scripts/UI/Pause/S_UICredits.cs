using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICredits : UIWindow
{
    [FoldoutGroup("Tweakable values")][SerializeField] protected bool m_isMainMenu;

    [FoldoutGroup("Internal references")][SerializeField] private S_MenuManager m_mainMenu;

    public override void Return(bool isPressed)
    {
        // Assertions
        if (!m_toggleReturn) return;
        if (!IsActive) return;
        if (m_rsoCancelPriority.value != m_requiredState) return;
        if (!m_rsoCancelConsumable.value) return;

        m_rsoCancelConsumable.value = false;
        Hide();
        if (m_previousUISelect != null && m_rsoCurrentControls.value == ControlType.GAMEPAD) EventSystem.current.SetSelectedGameObject(m_previousUISelect);
        m_rsoCancelPriority.value = m_previousState;

        if (m_isMainMenu) m_mainMenu.ShowMenu();
    }
}
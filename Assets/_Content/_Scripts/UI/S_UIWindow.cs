using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIWindow : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleCursor = true;
	[FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleReturn;
    [FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleSelectable = false;
    [ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected CancelState m_requiredState;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected CancelState m_previousState;
    [ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected GameObject m_defaultSelect;
    [ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected GameObject m_previousUISelect;

	[FoldoutGroup("Internal references")][SerializeField] protected GameObject m_graphicsParent;
	
	[ShowIf("m_toggleCursor")][FoldoutGroup("Scriptable")][SerializeField] protected RSE_ToggleCursor m_rseToggleCursor;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSO_CancelConsumable m_rsoCancelConsumable;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSO_CancelPriority m_rsoCancelPriority;
    [ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSO_CurrentControls m_rsoCurrentControls;
    [ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSE_Return m_rseReturn;

	public bool IsActive => m_graphicsParent.activeInHierarchy;

	protected virtual void OnEnable()
	{
		if (m_toggleReturn) m_rseReturn.action += Return;
		m_rsoCurrentControls.OnChanged += UpdateSelection;

    }

	protected virtual void OnDisable()
	{
		if (m_toggleReturn) m_rseReturn.action -= Return;
        m_rsoCurrentControls.OnChanged -= UpdateSelection;
    }

	public virtual void Start()
	{
		Hide();
	}

	public virtual void Toggle()
	{
		if (IsActive)
		{
			Hide();
		}
		else 
		{
			Show();
		}
	}

	public virtual void Hide()
	{
		m_graphicsParent.SetActive(false);
		if (m_toggleCursor) m_rseToggleCursor.Call(false);
	}

	public virtual void Show()
	{
		m_graphicsParent.SetActive(true);
		if (m_toggleReturn) m_rsoCancelPriority.value = m_requiredState;
        UpdateSelection();
    }

	public virtual void Return(bool isPressed)
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
	}

	protected virtual void UpdateSelection()
    {
        if (!m_toggleSelectable) return;
        if (m_rsoCancelPriority.value != m_requiredState) return;

		switch (m_rsoCurrentControls.value)
		{
			case ControlType.GAMEPAD:
                m_rseToggleCursor.Call(false);
                if (m_defaultSelect != null) EventSystem.current.SetSelectedGameObject(m_defaultSelect);
                else EventSystem.current.SetSelectedGameObject(null);
                break;
			case ControlType.KEYBOARDMOUSE:
                m_rseToggleCursor.Call(true);
                EventSystem.current.SetSelectedGameObject(null);
                break;
		}
	}

}
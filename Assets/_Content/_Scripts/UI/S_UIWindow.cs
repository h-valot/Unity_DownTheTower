using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIWindow : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleCursor = true;
	[FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleReturn;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected CancelState m_requiredState;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected CancelState m_previousState;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Tweakable values")][SerializeField] protected GameObject m_previousUISelect;

	[FoldoutGroup("Internal references")][SerializeField] protected GameObject m_graphicsParent;

	[ShowIf("m_toggleCursor")][FoldoutGroup("Scriptable")][SerializeField] protected RSE_ToggleCursor m_rseToggleCursor;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSO_CancelConsumable m_rsoCancelConsumable;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSO_CancelPriority m_rsoCancelPriority;
	[ShowIf("m_toggleReturn")][FoldoutGroup("Scriptable")][SerializeField] protected RSE_Cancel m_rseCancel;

	public bool IsActive => m_graphicsParent.activeInHierarchy;

	protected virtual void OnEnable()
	{
		if (m_toggleReturn) m_rseCancel.action += Return;
	}

	protected virtual void OnDisable()
	{
		if (m_toggleReturn) m_rseCancel.action -= Return;
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
		if (m_toggleCursor) m_rseToggleCursor.Call(true);
		if (m_toggleReturn) m_rsoCancelPriority.value = m_requiredState;
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
		if (m_previousUISelect != null) EventSystem.current.SetSelectedGameObject(m_previousUISelect);
		m_rsoCancelPriority.value = m_previousState;
	}
}
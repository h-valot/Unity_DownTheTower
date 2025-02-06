using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIWindow : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] protected bool m_toggleCursor = true;
	
	[FoldoutGroup("Internal references")][SerializeField] protected GameObject m_graphicsParent;
    [FoldoutGroup("Internal references")][SerializeField] protected GameObject m_previousUISelect;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

    public bool IsActive => m_graphicsParent.activeInHierarchy;

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
	}

	public virtual void Return()
	{
		if (m_previousUISelect != null) EventSystem.current.SetSelectedGameObject(m_previousUISelect);
		Hide();
	}
}
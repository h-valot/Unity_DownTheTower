using Sirenix.OdinInspector;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
	[Title("UI Window")]
	[SerializeField] protected GameObject m_graphicsParent;
	
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
		m_rseToggleCursor.Call(false);
	}

	public virtual void Show()
	{
		m_graphicsParent.SetActive(true);
		m_rseToggleCursor.Call(true);
	}
}
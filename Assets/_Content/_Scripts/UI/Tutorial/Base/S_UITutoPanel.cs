using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoPanel : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] protected int m_index;

	[FoldoutGroup("Internal references")][SerializeField] protected GameObject m_graphicsParent;

	[FoldoutGroup("Scriptables")][SerializeField] protected RSO_CurrentTutoIndex m_rsoCurrentTutoIndex;

	protected bool m_isCompleted;

	public bool IsActive => m_graphicsParent.activeInHierarchy;

	protected virtual void OnEnable()
	{
		m_rsoCurrentTutoIndex.OnChanged += OnTutoChanged;
	}

	protected virtual void OnDisable()
	{
		m_rsoCurrentTutoIndex.OnChanged -= OnTutoChanged;
	}

	protected virtual void Start()
	{
		m_isCompleted = false;
		Hide();
	}

	private void OnTutoChanged()
	{
		if (m_rsoCurrentTutoIndex.value != m_index)
		{
			Hide();
		}
	}

	protected virtual void Complete()
	{
		m_isCompleted = true;
		m_graphicsParent.SetActive(false);
	}

	protected virtual void Hide()
	{
		m_graphicsParent.SetActive(false);
	}

	protected virtual void Show()
	{
		m_graphicsParent.SetActive(true);
		m_rsoCurrentTutoIndex.value = m_index;
	}

}
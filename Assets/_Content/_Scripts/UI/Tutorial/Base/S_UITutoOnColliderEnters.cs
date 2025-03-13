using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoOnColliderEnters : UITutoPanel
{
	[FoldoutGroup("Tweakable values")][SerializeField] private TutoColliderType m_type;

	[FoldoutGroup("Scriptables")][SerializeField] private RSE_TutoColliderEnters m_rseTutoColliderEnters;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseTutoColliderEnters.action += OnColliderEnters;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseTutoColliderEnters.action -= OnColliderEnters;
	}

	private void OnColliderEnters(TutoColliderType type)
	{
		// Assertions
		if (m_isCompleted) return;
		if (m_type != type) return;

		Show();
	}
}
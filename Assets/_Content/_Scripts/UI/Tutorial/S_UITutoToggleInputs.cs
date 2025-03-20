using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoToggleInputs : UITutoOnColliderEnters
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_duration;

	private float m_timer;

	private void Update()
	{
		// Assertions
		if (!IsActive) return;
		if (m_isCompleted) return;

		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			Complete();
		}
	}

	protected override void Show()
	{
		base.Show();
		m_timer = m_duration;
	}
}
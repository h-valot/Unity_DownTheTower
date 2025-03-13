using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoThrowTorch : UITutoOnColliderEnters
{
	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_ThrowTorch m_rseThrowTorch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseThrowTorch.action += OnThrowTorch;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseThrowTorch.action -= OnThrowTorch;
	}

	private void OnThrowTorch(bool isInputPressed)
	{
		if (!IsActive) return;
		if (isInputPressed) return;

		Complete();
	}
}
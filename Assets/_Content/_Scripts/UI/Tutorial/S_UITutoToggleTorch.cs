using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoToggleTorch : UITutoOnColliderEnters
{
	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_ToggleHandObject m_rseToggleHandObject;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseToggleHandObject.action += OnToggleTorch;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseToggleHandObject.action -= OnToggleTorch;
	}

	private void OnToggleTorch(bool isInputPressed)
	{
		Complete();
	}
}
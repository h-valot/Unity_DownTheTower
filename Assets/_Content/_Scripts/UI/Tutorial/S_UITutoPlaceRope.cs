using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoPlaceRope : UITutoOnColliderEnters
{
	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_ThrowRope m_rseThrowRope;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseThrowRope.action += OnThrowRope;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseThrowRope.action -= OnThrowRope;
	}

	private void OnThrowRope(bool isInputPressed)
	{
		if (isInputPressed) return;
		if (m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;

		Complete();
	}
}
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoClimbAbseil : UITutoPanel
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_showDelay;

	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Climb m_rseClimb;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_ThrowRope m_rseThrowRope;

	private bool m_hasClimp;
	private bool m_hasAbseil;

	protected override void OnEnable()
	{
		base.OnEnable();
		
		m_rsoCharacterState.OnChanged += OnStateChanged;

		m_rseClimb.action += OnClimb;
		m_rseThrowRope.action += OnAbseil;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		
		m_rsoCharacterState.OnChanged -= OnStateChanged;

		m_rseClimb.action -= OnClimb;
		m_rseThrowRope.action -= OnAbseil;
	}

	private void OnStateChanged()
	{
		if (m_rsoCharacterState.value == BehaviorState.ROPE
		&& !m_isCompleted)
		{
			StartCoroutine(ShowAfterDelay(m_showDelay));
		}

		if (IsActive
		&& m_rsoCharacterState.value != BehaviorState.ROPE)
		{
			// Skip extra verifications
			base.Complete();
		}
	}

	private IEnumerator ShowAfterDelay(float duration)
	{
		yield return new WaitForSeconds(duration);

		if (m_isCompleted) 
		{
			yield return null;
		}
		
		Show();
	}

	protected override void Show()
	{
		// Assert: Inputs are already known
		if (m_hasClimp && m_hasAbseil) return;

		base.Show();
	}

	private void OnClimb(bool isInputPressed)
	{
		if (m_rsoCharacterState.value == BehaviorState.ROPE)
		{
			m_hasClimp = true;
			Complete();
		}
	}

	private void OnAbseil(bool isInputPressed)
	{
		if (m_rsoCharacterState.value == BehaviorState.ROPE)
		{
			m_hasAbseil = true;
			Complete();
		}
	}

	protected override void Complete()
	{
		// Assertion
		if (!IsActive) return;
		if (!m_hasClimp || !m_hasAbseil) return;
		if (m_rsoCurrentTutoIndex.value > m_index) return;

		base.Complete();
	}
}
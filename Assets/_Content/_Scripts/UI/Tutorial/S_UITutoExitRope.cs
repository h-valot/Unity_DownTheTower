using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoExitRope : UITutoPanel
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_showDelay;

	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Cancel m_rseCancel;

	private bool m_wasOnRope;
	private bool m_hasCancelled;

	protected override void OnEnable()
	{
		base.OnEnable();

		m_rsoCharacterState.OnChanged += OnStateChanged;

		m_rseCancel.action += OnCancel;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		m_rsoCharacterState.OnChanged -= OnStateChanged;

		m_rseCancel.action -= OnCancel;
	}

	private void OnStateChanged()
	{
		if (m_rsoCharacterState.value == BehaviorState.ROPE)
		{
			m_wasOnRope = true;
		}
		
		if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION
		&& m_wasOnRope
		&& !m_isCompleted)
		{
			StartCoroutine(ShowAfterDelay(m_showDelay));
		}
	}

	private IEnumerator ShowAfterDelay(float duration)
	{
		yield return new WaitForSeconds(duration);

		// Exit if the rope has been detach during the delay.
		if (m_hasCancelled) 
		{
			yield return null;
		}

		Show();
	}

	protected override void Show()
	{
		// Assert: The rope has already been detached
		if (m_hasCancelled && m_wasOnRope) return;

		base.Show();
	}

	private void OnCancel(bool isInputPressed)
	{
		m_hasCancelled = true;
		Complete();
	}

	protected override void Complete()
	{
		if (!m_wasOnRope) return;
		if (m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;
		if (m_rsoCurrentTutoIndex.value > m_index) return;

		base.Complete();
	}
}
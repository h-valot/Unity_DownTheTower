using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoRopeJump : UITutoOnColliderEnters
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_inputHeldDuration;

	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Jump m_rseJump;

	private float m_timer;
	private bool m_isJumping;

	private void Update()
	{
		// Assertions
		if (!m_isJumping) return;
		if (m_isCompleted) return;
		if (!IsActive) return;
		if (m_rsoCharacterState.value != BehaviorState.ROPE) return;

		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			Complete();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rsoCharacterState.OnChanged += OnStateChanged;
		m_rseJump.action += OnJump;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rsoCharacterState.OnChanged -= OnStateChanged;
		m_rseJump.action -= OnJump;
	}

	private void OnStateChanged()
	{
		if (IsActive
		&& m_rsoCharacterState.value != BehaviorState.ROPE)
		{
			Complete();
		}
	}

	protected override void Show()
	{
		m_timer = m_inputHeldDuration;
		base.Show();
	}

	private void OnJump(bool isInputPressed)
	{
		m_isJumping = isInputPressed;
	}
}
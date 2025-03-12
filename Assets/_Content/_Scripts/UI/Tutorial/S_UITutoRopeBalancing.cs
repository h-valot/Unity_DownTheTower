using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoRopeBalancing : UITutoOnColliderEnters
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_inputHeldDuration;

	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Move m_rseMove;

	private float m_timer;
	private bool m_isMoving;

	private void Update()
	{
		// Assertions
		if (!m_isMoving) return;
		if (m_isCompleted) return;
		if (m_rsoCharacterState.value != BehaviorState.ROPE) return;
		if (!IsActive) return;

		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			print("completed");
			Complete();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseMove.action += OnMove;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseMove.action -= OnMove;
	}

	protected override void Show()
	{
		m_timer = m_inputHeldDuration;
		base.Show();
	}

	private void OnMove(Vector2 vector)
	{
		m_isMoving = vector.magnitude > 0;
	}
}
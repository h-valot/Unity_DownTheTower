using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class UITutoGameStarts : UITutoPanel
{
	[FoldoutGroup("Tweakable values")][SerializeField] private float m_delayFromStart;

	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Move m_rseMove;
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_Look m_rseLook;

	private bool m_hasMoved;
	private bool m_hasLooked;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseMove.action += OnMove;
		m_rseLook.action += OnLook;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rseMove.action -= OnMove;
		m_rseLook.action -= OnLook;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(OnStart());
	}

	private IEnumerator OnStart()
	{
		yield return new WaitForSeconds(m_delayFromStart);
		Show();
	}

	private void OnMove(Vector2 vector)
	{
		if (vector.magnitude > 0f)
		{
			m_hasMoved = true;
			Hide();
		}
	}

	private void OnLook(Vector2 vector)
	{
		if (vector.magnitude > 0f)
		{
			m_hasLooked = true;
			Hide();
		}
	}

	protected override void Show()
	{
		// Assert: Inputs are already known
		if (m_hasMoved && m_hasLooked) return;

		base.Show();
	}

	protected override void Hide()
	{
		// Assertion
		if (!m_hasMoved || !m_hasLooked) return;
		if (m_rsoCurrentTutoIndex.value > m_index) return;

		Complete();
	}

}
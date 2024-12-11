using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UILogManager : UIWindow
{
	[FoldoutGroup("Static variables")][SerializeField] private TextMeshProUGUI m_tmpHeader;
	[FoldoutGroup("Static variables")][SerializeField] private TextMeshProUGUI m_tmpBody;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_LogContent m_rseLogContent;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	private void OnEnable()
	{
		m_rseLogContent.action += Show;
		m_rseCancel.action += Hide;
		m_rsePause.action += OnGamePaused;
	}

	private void OnDisable()
	{
		m_rseLogContent.action -= Show;
		m_rseCancel.action -= Hide;
		m_rsePause.action -= OnGamePaused;
	}

	private void OnGamePaused()
	{
		Hide();
	}

	private void Show(string header, string body)
	{
		m_tmpHeader.text = header;
		m_tmpBody.text = body;

		TogglePauseGame(true);
		base.Show();
	}

	private void Hide(bool isHide = true)
	{
		// Assertion
		if (!IsActive) return;

		TogglePauseGame(false);
		base.Hide();
	}

	private void TogglePauseGame(bool isPaused)
	{
		m_rsoGamePaused.value = isPaused;
		Time.timeScale = isPaused ? 0f : 1f;
		m_rseToggleInputs.Call();
	}
}
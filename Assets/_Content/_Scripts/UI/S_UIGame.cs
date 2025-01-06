using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UIGame : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlPause;
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlLog;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpLogHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpLogBody;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpVersion;
	[FoldoutGroup("Internal references")][SerializeField] private List<UIWindow> m_subwindows = new List<UIWindow>();
	
	[FoldoutGroup("External references")][SerializeField] private UILogDisplayer m_uiLogDisplayer;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	private void Start()
	{
		Hide();
	}

    private void OnEnable()
    {
        m_rsePause.action += TogglePausePanel;
	}

    private void OnDisable()
    {
        m_rsePause.action -= TogglePausePanel;
    }

    private void TogglePausePanel()
	{
		SetPausePanel(!m_pnlPause.activeInHierarchy);
	}

	public void SetPausePanel(bool doEnabled)
	{
		m_tmpVersion.text = $"version: {m_ssoGame.Version} {m_ssoGame.BuildType.ToString().ToLower()}";

		if (doEnabled)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		m_pnlPause.SetActive(true);
		TogglePauseGame(true);
		m_rseToggleCursor.Call(true);
		HideSubwindows();
	}

	public void Hide()
	{
		m_pnlPause.SetActive(false);
		TogglePauseGame(false);
		m_rseToggleCursor.Call(false);
		HideSubwindows();
	}

	private void HideSubwindows()
	{
		foreach (var subwindow in m_subwindows)
		{
			subwindow.Hide();
		}
	}

	public void Exit()
	{
		Application.Quit();
	}

	private void TogglePauseGame(bool isPaused)
    {
        m_rsoGamePaused.value = isPaused;
        m_rseToggleInputs.Call();
    }
}
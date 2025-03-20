using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIGame : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlPause;
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlLog;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_defaultSelect;
    [FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpLogHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpLogBody;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpVersion;
	[FoldoutGroup("Internal references")][SerializeField] private List<UIWindow> m_subwindows = new List<UIWindow>();
	
	[FoldoutGroup("External references")][SerializeField] private UILogDisplayer m_uiLogDisplayer;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_Return m_rseReturn;

    [FoldoutGroup("Scriptable")][SerializeField] protected RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CancelPriority m_rsoCancelPriority;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CancelConsumable m_rsoCancelConsumable;
    [FoldoutGroup("Scriptable")][SerializeField] protected RSO_CurrentControls m_rsoCurrentControls;
    [FoldoutGroup("Scriptable")][SerializeField] protected RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;

	private void Start()
	{
		Hide();
	}

    private void OnEnable()
    {
		m_rsoPause.OnChanged += TogglePausePanel;
		m_rseReturn.action += CheckResume;
        m_rsoCurrentControls.OnChanged += UpdateSelection;

    }

    private void OnDisable()
    {
		m_rsoPause.OnChanged -= TogglePausePanel;
        m_rseReturn.action -= CheckResume;
        m_rsoCurrentControls.OnChanged -= UpdateSelection;
    }

    private void TogglePausePanel()
	{
		if (!m_rsoGameStarted.value) return;

        SetPausePanel(m_rsoPause.value);
    }

	public void SetPausePanel(bool doEnabled)
	{
		m_tmpVersion.text = $"version: {m_ssoGame.Version} {m_ssoGame.BuildType.ToString().ToLower()}";

		if (doEnabled)
        {
			if (m_rsoPause.value) m_rsoCurrentScheme.value = InputScheme.PAUSE;
            m_rsoCancelPriority.value = CancelState.UI_PAUSE;
            Show();
		}
		else
        {
            if (!m_rsoPause.value) 
			{
				m_rsoCurrentScheme.value = InputScheme.GAME;
				m_rsoCancelPriority.value = CancelState.IN_GAME;
			}
            Hide();
		}
	}

	public void ShowPausePanelFromLogs()
    {
        m_rsoCancelPriority.value = CancelState.UI_PAUSE;
        Show();
    }

	private void Show()
	{
		m_pnlPause.SetActive(true);
        UpdateSelection();
        HideSubwindows();
	}

	public void Hide()
	{
		m_pnlPause.SetActive(false);
		m_rseToggleCursor.Call(false);
		HideSubwindows();
	}

	public void CheckResume(bool isPressed)
	{
		if (!m_rsoGameStarted.value) return;
		if (m_rsoCancelPriority.value != CancelState.UI_PAUSE) return;
        if (!m_rsoCancelConsumable.value) return;
		if (!isPressed) return;

		m_rsoCancelConsumable.value = false;

		foreach (var subwindow in m_subwindows)
        {
			if (subwindow.IsActive) return;
        }

		m_rsoPause.value = false;
    }

    public void CancelPause()
    {
        m_rsoCancelConsumable.value = false;

        foreach (var subwindow in m_subwindows)
        {
            if (subwindow.IsActive) return;
        }

        m_rsoPause.value = false;
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

    protected virtual void UpdateSelection()
    {
        if (m_rsoCancelPriority.value != CancelState.UI_PAUSE) return;

        switch (m_rsoCurrentControls.value)
        {
            case ControlType.GAMEPAD:
                m_rseToggleCursor.Call(false);
				if (m_defaultSelect != null) EventSystem.current.SetSelectedGameObject(m_defaultSelect);
				else EventSystem.current.SetSelectedGameObject(null);
                break;
            case ControlType.KEYBOARDMOUSE:
                m_rseToggleCursor.Call(true);
                EventSystem.current.SetSelectedGameObject(null);
                break;
        }
    }
}
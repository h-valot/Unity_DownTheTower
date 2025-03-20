using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILogDisplayer : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpBody;

	[FoldoutGroup("External references")][SerializeField] private UILogCollection m_uiLogCollection;
	[FoldoutGroup("External references")][SerializeField] private UIGame m_uiGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ssoSoundLogOpen;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ssoSoundLogClose;

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdvisorDisplayed m_rsoInputAdviceDisplayed;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;

    private bool m_logCollectionDisplayed;
	private GameObject selectedLog;

	protected override void OnEnable()
	{
		m_rseDisplayLog.action += Display;
		m_rseReturn.action += Hide;
		m_rsoPause.OnChanged += Hide;
	}

	protected override void OnDisable()
	{
		m_rseDisplayLog.action -= Display;
		m_rseReturn.action -= Hide;
		m_rsoPause.OnChanged -= Hide;
	}

	private void Display(SSO_Log ssoLog)
	{
		m_tmpHeader.text = ssoLog.Header;
		m_tmpBody.text = ssoLog.Body;

		if (m_uiLogCollection.IsActive)
		{
			selectedLog = EventSystem.current.currentSelectedGameObject;
			m_logCollectionDisplayed = true;
			m_uiGame.SetPausePanel(false);
		}
        else
        {
            m_logCollectionDisplayed = false;
        }

        m_rsoInputsLocked.value = true;
		m_rsoInputAdviceDisplayed.value = false;
		base.Show();
        if (!m_rsoPause.value) m_rsoCurrentScheme.value = InputScheme.PAUSE;
		m_rsoCancelPriority.value = CancelState.UI_LOG;
        m_rsePlaySound.Call(m_ssoSoundLogOpen);
    }

	private void Hide(bool isHidden)
	{
		if (!IsActive) return;
		if (m_rsoCancelPriority.value != CancelState.UI_LOG) return;
		if (!m_rsoCancelConsumable.value) return;

		m_rsoCancelConsumable.value = false;
		base.Hide();
        if (!m_rsoPause.value) m_rsoCurrentScheme.value = InputScheme.GAME;
        m_rsePlaySound.Call(m_ssoSoundLogClose);

		if (m_logCollectionDisplayed)
        {
            m_uiGame.ShowPausePanelFromLogs();
            m_uiLogCollection.Show();
			if(m_rsoCurrentControls.value == ControlType.GAMEPAD) EventSystem.current.SetSelectedGameObject(selectedLog);
		}
		else
		{
			m_rsoInputsLocked.value = false;
			m_rsoInputAdviceDisplayed.value = true;
			m_rsoCancelPriority.value = CancelState.IN_GAME;
		}

		selectedLog = null;
	}
}
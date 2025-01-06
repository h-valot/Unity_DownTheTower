using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UILogDisplayer : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpBody;

	[FoldoutGroup("External references")][SerializeField] private UILogCollection m_uiLogCollection;
	[FoldoutGroup("External references")][SerializeField] private UIGame m_uiGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleInputs m_rseToggleInputs;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdviceDisplayed m_rsoInputAdviceDisplayed;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	private bool m_logCollectionDisplayed;

	private void OnEnable()
	{
		m_rseDisplayLog.action += Display;
		m_rseCancel.action += Hide;
		m_rsePause.action += Hide;
	}

	private void OnDisable()
	{
		m_rseDisplayLog.action -= Display;
		m_rseCancel.action -= Hide;
		m_rsePause.action -= Hide;
	}

	private void Display(SSO_Log ssoLog)
	{
		m_tmpHeader.text = ssoLog.Header;
		m_tmpBody.text = ssoLog.Body;

		if (m_uiLogCollection.IsActive)
		{
			m_logCollectionDisplayed = true;
			m_uiGame.SetPausePanel(false);
		}

		m_rsoInputAdviceDisplayed.value = false;
		m_rsoGamePaused.value = true;
		base.Show();
	}

	private void Hide(bool isHidden)
	{
		base.Hide();

		if (m_logCollectionDisplayed)
		{
			m_uiGame.SetPausePanel(true);
			m_uiLogCollection.Show();
		}
		else
		{
			m_rsoInputAdviceDisplayed.value = true;
			m_rsoGamePaused.value = false;
		}
	}
}
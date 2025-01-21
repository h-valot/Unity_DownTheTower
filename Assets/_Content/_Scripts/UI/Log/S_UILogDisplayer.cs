using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UILogDisplayer : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpBody;

	[FoldoutGroup("External references")][SerializeField] private UILogCollection m_uiLogCollection;
	[FoldoutGroup("External references")][SerializeField] private UIGame m_uiGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdviceDisplayed m_rsoInputAdviceDisplayed;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;

	private bool m_logCollectionDisplayed;

	private void OnEnable()
	{
		m_rseDisplayLog.action += Display;
		m_rseCancel.action += Hide;
		m_rsoPause.OnChanged += Hide;
	}

	private void OnDisable()
	{
		m_rseDisplayLog.action -= Display;
		m_rseCancel.action -= Hide;
		m_rsoPause.OnChanged -= Hide;
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

		m_rsoInputsLocked.value = true;
		m_rsoInputAdviceDisplayed.value = false;
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
			m_rsoInputsLocked.value = false;
			m_rsoInputAdviceDisplayed.value = true;
		}
	}
}
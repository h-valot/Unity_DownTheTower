using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UILogDisplayer : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpHeader;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpBody;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

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

		base.Show();
	}

	private void Hide(bool isHidden)
	{
		base.Hide();
	}
}
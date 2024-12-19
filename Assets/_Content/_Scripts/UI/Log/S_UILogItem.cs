using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UILogItem : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpTitle;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;
	
	private SSO_Log m_ssoLog;

	public void Initialize(SSO_Log ssoLog)
	{
		m_ssoLog = ssoLog;
	}

	public void Press()
	{
		// Assertion
		if (!m_ssoLog.IsDiscovered) return;

		m_rseDisplayLog.Call(m_ssoLog);
	}

	public void Toggle(bool isDiscovered)
	{
		if (isDiscovered)
		{
			Highlight();
		}
		else
		{
			Withdraw();
		}
	}

	public void Highlight()
	{
		m_tmpTitle.text = m_ssoLog.Header;
	}

	public void Withdraw()
	{
		m_tmpTitle.text = "???";
	}
}
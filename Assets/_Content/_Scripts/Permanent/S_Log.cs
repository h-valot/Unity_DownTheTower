using Sirenix.OdinInspector;
using UnityEngine;

public class Log : Interactable
{

	[SerializeField] private SSO_Log m_ssoLog;


    [FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayLog m_rseDisplayLog;

    public override void InteractionTrigger()
    {
		// Assertion
		if (!m_ssoLog) return;

		m_ssoLog.IsDiscovered = true;
		m_rseDisplayLog.Call(m_ssoLog);

    }
}
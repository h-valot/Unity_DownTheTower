using Sirenix.OdinInspector;
using UnityEngine;

public class Log : Interactable
{
	[Title("Title")]
	[HideLabel]
	[MultiLineProperty(2)]
	[SerializeField] private string m_header;

	[Title("Flavor")]
	[HideLabel]
	[MultiLineProperty(7)]
	[SerializeField] private string m_body;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_LogContent m_rseLogContent;

    public override void InteractionTrigger()
    {
        m_rseLogContent.Call(m_header, m_body);
    }
}
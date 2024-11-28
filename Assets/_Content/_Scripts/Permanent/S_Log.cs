using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Log : Interactable
{
    [Title("Tweakable Values")]
    [SerializeField] private string m_logHeader;
    [SerializeField] private List<string> m_logBody = new List<string>();

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_LogContent m_rseLogContent;

    public override void InteractionTrigger()
    {
        m_rseLogContent.Call(m_logHeader, m_logBody);
    }
}
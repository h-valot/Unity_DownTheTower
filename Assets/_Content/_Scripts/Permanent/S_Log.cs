using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Log : Interactible
{
    [Header("External Variables")]
    [SerializeField] private RSE_LogContent _rseLogContent;

    [Header("Tweakable Values")]
    [SerializeField] private string _logHeader;
    [SerializeField] private List<string> _logBody;

    public override void InteractionTrigger()
    {
        _rseLogContent.Call(_logHeader, _logBody);
    }
}

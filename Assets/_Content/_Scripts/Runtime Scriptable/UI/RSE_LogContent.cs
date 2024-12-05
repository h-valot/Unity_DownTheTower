using RuntimeScriptables;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_LogContent", menuName = "Runtime Scriptable/UI/Log content")]
public class RSE_LogContent : RuntimeScriptableEvent<string, List<string>> { }
using RuntimeScriptables;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_LogContent", menuName = "RSE/UI/LogContent")]
public class RSE_LogContent : RuntimeScriptableEvent<string, List<string>> { }
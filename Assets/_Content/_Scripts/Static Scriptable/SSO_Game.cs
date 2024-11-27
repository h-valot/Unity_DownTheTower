using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Game", menuName = "Static Scriptable/Game")]
public class SSO_Game : ScriptableObject
{
	[Title("Debug")]
	[InfoBox("If true, the console will gather debug logs. Note: high cost, keep it desable for now.", InfoMessageType.None)]
	/// <summary> If true, the console will gather debug logs. </summary>
	public bool enableConsoleLogging;
}	
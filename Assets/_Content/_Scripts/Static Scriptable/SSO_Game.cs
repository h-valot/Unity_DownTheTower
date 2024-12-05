using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Game", menuName = "Static Scriptable/Game")]
public class SSO_Game : ScriptableObject
{
	#region BUILD

	[Title("Build")]
	public string Version;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The type of build the game will be. DEBUG: Activate all debug fonctionalities. RELEASE: Disable all debug fonctionalities.", InfoMessageType.None)]
	/// <summary> The type of build the game will be. DEBUG: Activate all debug fonctionalities. RELEASE: Disable all debug fonctionalities. </summary>
	public BuildType BuildType;


	#endregion

	#region DEBUG

	[Title("Debug")]
	[InfoBox("If true, the console will gather debug logs. Note: high cost, keep it desable for now.", InfoMessageType.None)]
	/// <summary> If true, the console will gather debug logs. </summary>
	public bool enableConsoleLogging;

	#endregion
}
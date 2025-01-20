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
	public BuildType BuildType;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 15)]
	[InfoBox("Enable the chrono debugging  system.", InfoMessageType.None)]
	public bool EnableChrono;

	#endregion

	#region WORLD

	[FoldoutGroup("World")]
	[InfoBox("Default light intensity of scene global point lights.", InfoMessageType.None)]
	public float GlobalLightIntensity;

	#endregion

	#region RUNTIME VALUE MODIFIER

	[FoldoutGroup("Runtime value modifier")]
	[InfoBox("The following values are used in the build tool to run AB testing.")]
	[InfoBox("@\"Base value : \" + m_ssoTorch.LightRange.ToString()", InfoMessageType.None)]
	public float NewTorchLightRange;

	[FoldoutGroup("Runtime value modifier")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("@\"Base value : \" + GlobalLightIntensity.ToString()", InfoMessageType.None)]
	public float NewGlobalLightIntensity;

	[FoldoutGroup("Runtime value modifier")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("@\"Base value : \" + m_ssoCharacter.RunSpeed.ToString()", InfoMessageType.None)]
	public float NewCharacterRunSpeed;

	#endregion

	#region DATA

	[FoldoutGroup("Data")]
	[InfoBox("If true, reset all game data on game start.", InfoMessageType.None)]
	public bool ResetData;

	#endregion

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;
}
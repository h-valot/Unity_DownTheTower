using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Inputs", menuName = "Static Scriptable/Inputs")]
public class SSO_Inputs : ScriptableObject
{
	#region LOOK

	[Title("Mouse")]
	[InfoBox("Scalar for mouse sensibility on X.", InfoMessageType.None)]
	/// <summary> Scalar for mouse sensibility on X. </summary>
	public float MouseSensibilityX;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar for mouse sensibility on Y.", InfoMessageType.None)]
	/// <summary> Scalar for mouse sensibility on Y. </summary>
	public float MouseSensibilityY;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Is mouse Y inverted.", InfoMessageType.None)]
	/// <summary> Is mouse Y inverted. </summary>
	public bool InvertMouseY;


	[Title("Gamepad")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar for gamepad sensibility on X.", InfoMessageType.None)]
	/// <summary> Scalar for gamepad sensibility on X. </summary>
	public float GamepadSensibilityX;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar for gamepad sensibility on Y.", InfoMessageType.None)]
	/// <summary> Scalar for gamepad sensibility on Y. </summary>
	public float GamepadSensibilityY;

	#endregion
}
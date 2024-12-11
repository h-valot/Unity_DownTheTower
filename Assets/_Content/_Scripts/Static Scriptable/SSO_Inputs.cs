using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Inputs", menuName = "Static Scriptable/Inputs")]
public class SSO_Inputs : ScriptableObject
{
	#region LOOK

	[Title("Mouse")]
	[InfoBox("Scalar for mouse sensibility on X.", InfoMessageType.None)]
	public float MouseSensibilityX;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar for mouse sensibility on Y.", InfoMessageType.None)]
	public float MouseSensibilityY;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Is mouse Y inverted.", InfoMessageType.None)]
	public bool InvertMouseY;


	[Title("Gamepad")]
	[InfoBox("Scalar for gamepad sensibility on X.", InfoMessageType.None)]
	public float GamepadSensibilityX;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar for gamepad sensibility on Y.", InfoMessageType.None)]
	public float GamepadSensibilityY;

	#endregion
}
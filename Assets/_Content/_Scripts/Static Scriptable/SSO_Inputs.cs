using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Inputs", menuName = "Static Scriptable/Inputs")]
public class SSO_Inputs : ScriptableObject
{
    [Header("Look - Gamepad")]
    [Tooltip("Scalar for mouse sensibility on X")]
    public float mouseSensibilityX;
    [Tooltip("Scalar for mouse sensibility on Y")]
    public float mouseSensibilityY;
    [Tooltip("Is mouse Y inverted")]
    public bool InvertMouseY;


    [Space(5)]
	[Header("Look - Mouse")]
	[Tooltip("Scalar for gamepad sensibility on X")]
    public float gamepadSensibilityX;
    [Tooltip("Scalar for gamepad sensibility on Y")]
    public float gamepadSensibilityY;
}
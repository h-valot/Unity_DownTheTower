using UnityEngine;

[CreateAssetMenu(fileName = "HPPC_InputsConfig", menuName = "Configs/HPPC Inputs")]
public class HPPC_InputsConfig : ScriptableObject
{
    [Header("Look")]
    [Tooltip("Scalar for mouse sensibility on X")]
    public float mouseSensibilityX;
    [Tooltip("Scalar for mouse sensibility on Y")]
    public float mouseSensibilityY;
    [Tooltip("Is mouse Y inverted")]
    public bool InvertMouseY;


    [Space(5)]
    [Tooltip("Scalar for gamepad sensibility on X")]
    public float gamepadSensibilityX;
    [Tooltip("Scalar for gamepad sensibility on Y")]
    public float gamepadSensibilityY;
}
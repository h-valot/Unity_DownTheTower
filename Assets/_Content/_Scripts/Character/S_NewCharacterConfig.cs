using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Configs/New character")]
public class NewCharacterConfig : ScriptableObject
{
	[Header("Move")]
	public float walkSpeed;
	public float sprintSpeed;

	[Header("Ground")]
	public float groundedRaycastLength = 1.3f;
	public float gravity = -9.8f;

	[Header("Jump")]
	public float jumpHeight;
	public float jumpCooldown;
	[Range(0f, 1f)] public float airControlModifier;

	[Header("CAMERA")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float topClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float bottomClamp = -60.0f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float cameraAngleOverride = 0.0f;


	public CameraStyle startingStyle;
	public float rotationSpeed = 7;
}
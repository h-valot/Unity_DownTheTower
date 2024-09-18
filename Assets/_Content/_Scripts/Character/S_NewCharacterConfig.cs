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
	[Tooltip("0.25 + 0.08 (sphereCastRadius + CC skin width)")]
	public float groundCheckY = 0.33f;
	[Tooltip("Radius of area to detect the ground")]
	public float sphereCastRadius = 0.25f;
	[Tooltip("How far spherecast moves down from origin point")]
	public float sphereCastDistance = 0.75f;

	[Header("Jump")]
	public float jumpHeight;
	public float jumpCooldown;
	[Range(0f, 1f)] public float airControlModifier;

	[Header("Camera")]
	public float topClamp = 70.0f;
	public float bottomClamp = -60.0f;
	public CameraStyle startingStyle;
	public float rotationSpeed = 7;
}
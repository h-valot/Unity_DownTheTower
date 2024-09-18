using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Configs/New character")]
public class NewCharacterConfig : ScriptableObject
{
	[Header("Move")]
	public float walkSpeed;
	public float sprintSpeed;


	[Header("Ground")]
	public float groundedRaycastLength = 1.3f;
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


	[Header("Fall")]
	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -9.8f;

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float fallDelay = 0.15f;

	[Tooltip("If the distance on the y-axis travelled while falling is greater or equal to this value, the character dies")]
	public float lethalHeight = 7;

	[Tooltip("If the distance on the y-axis travelled while falling is greater or equal to this value, the character is stunned on land")]
	public float stunHeight = 3;

	[Tooltip("Duration of the stun based on the distance travelled")]
	public AnimationCurve stunDuration;

	[Tooltip("If the distance on the y-axis travelled while falling is greater or equal to this value, the character is slowed on land. Below this threshold, the character does not suffer any harmful effect")]
	public float slowHeight = 1;

	[Tooltip("Duration of the slow based on the distance travelled")]
	public AnimationCurve slowDuration;

	[Tooltip("Percentage of the target speed reduction while slowed")]
	public AnimationCurve slowPercentage;


	[Header("Camera")]
	public float topClamp = 70.0f;
	public float bottomClamp = -60.0f;
	public CameraStyle startingStyle;
	public float rotationSpeed = 7;
}
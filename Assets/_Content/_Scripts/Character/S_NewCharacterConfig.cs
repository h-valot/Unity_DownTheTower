using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Configs/New character")]
public class NewCharacterConfig : ScriptableObject
{
	[Header("Move")]
	[Tooltip("")]
	public float walkSpeed;
	[Tooltip("")]
	public float sprintSpeed;
	[Tooltip("")]
	public float speedChangeRate = 10.0f;

	[Header("Slope")]
	[Tooltip("")]
	public int slopeLimit = 45;

	[Tooltip("Percentage of character speed when ascending")]
	public AnimationCurve uphillDeceleration;

	[Tooltip("Percentage of character speed when descending")]
	public AnimationCurve downhillAcceleration;


	[Header("Edge")]
	[Tooltip("")]
	public float noSlipDistance = 0.5f;
	[Tooltip("")]
	public float edgeFallFactor = 1f;


	[Header("Ground")]
	[Tooltip("")]
	public float groundedRaycastLength = 1.3f;

	[Tooltip("0.25 + 0.08 (sphereCastRadius + CC skin width)")]
	public float groundCheckY = 0.33f;

	[Tooltip("Radius of area to detect the ground")]
	public float sphereCastRadius = 0.25f;

	[Tooltip("How far raycast moves down from origin point")]
	public float raycastLength = 0.75f;


	[Header("Jump")]
	[Tooltip("The height the player can jump")]
	public float jumpHeight;

	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpDelay = 0.50f;


	[Header("Gravity")]
	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -9.8f;
	[Tooltip("")]
	[Range(0f, 1f)] public float airControlModifier;


	[Header("Fall")]
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
	[Tooltip("")]
	public float topClamp = 70.0f;
	[Tooltip("")]
	public float bottomClamp = -60.0f;
	[Tooltip("")]
	public CameraStyle startingStyle;
	[Tooltip("")]
	public float rotationSpeed = 7;
}
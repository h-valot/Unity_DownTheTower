using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/character")]
public class CharacterConfig : ScriptableObject
{
	[Header("Move")]
	[Tooltip("Walk speed of the character")]
	public float walkSpeed;
	
	[Tooltip("Sprint speed of the character")]
	public float sprintSpeed;
	
	[Tooltip("Acceleration and deceleration rate. The current move speed of the character increases and decreases times this value times time.deltatime.")]
	public float speedChangeRate = 10.0f;


	[Header("Slope")]
	[Tooltip("Percentage of character speed when ascending. The current move speed is reduced by this value based on the slope angle")]
	public AnimationCurve uphillDeceleration;

	[Tooltip("Percentage of character speed when descending. The current move speed is increased by this value based on the slope angle")]
	public AnimationCurve downhillAcceleration;


	[Header("Edge")]
	[Tooltip("")]
	public float noSlipDistance = 0.5f;
	[Tooltip("Speed scalar when the character is on an edge.")]
	public float edgeFallFactor = 1f;


	[Header("Ground")]
	[Tooltip("Ground check raycast is cast from character's position plus this value times up vector")]
	public float groundCheckY = 0.33f;

	[Tooltip("How far raycast moves down from origin point calculate from the groundCheckY value")]
	public float groundRaycastLength = 0.75f;


	[Header("Jump")]
	[Tooltip("When jumping, the character reaches this height")]
	public float jumpHeight;

	[Tooltip("After touching the ground, this value is the time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpCooldown = 0.50f;


	[Header("Gravity")]
	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -15f;
	[Tooltip("Multiply this value by the player's directional inputs while in the air.")]
	[Range(0f, 1f)] public float airControlModifier;


	[Header("Fall")]
	[Tooltip("Whenever the character leaves a plateform, the coyote time counter starts. During this periode of time, the character can still jump.")]
	public float coyoteTime = 0.25f;

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


	[Header("Rope")]
	[Tooltip("HOLD_TO_STOP: stop the character from getting any further away from the rope base when pressing the corresponding input. HOLD_TO_LET_GO: letting the character getting further from the rope base when pressing the corresponding input.")]
	public RopeHolding ropeHoldingMethod;

	[Tooltip("Against wall raycast will include only referenced layers.")]
	public LayerMask againstWallLayerToInclude;

	[Tooltip("Scalar that multiply the direction towards the attraction point while on the partial rope suspension (eg. character against a wall)")]
	public float partialSphericalAttractiveForce = 2.0f;

	[Tooltip("Scalar that multiply the player's input direction while on the partial rope suspension (eg. character against a wall).")]
	public float partialSuspensionSpeed = 3.0f;

	[Tooltip("Scalar that multiply the direction towards the attraction point while on the complete rope suspension (eg. character in the void).")]
	public float completeSphericalAttractiveForce = 3.0f;

	[Tooltip("Scalar that multiply the player's input direction while on the complete rope suspension (eg. character in the void).")]
	public float completeSuspensionSpeed = 4.0f;

	[Tooltip("Start facing the center when the distance between the character and the hold rope radius is less than this value.")]
	public float facingCenterThreshold = 0.25f;


	[Header("Camera")]
	[Tooltip("Clamp the camera rotation to this angle when looking down")]
	public float topClamp = 70.0f;
	
	[Tooltip("Clamp the camera rotation to this angle when looking up")]
	public float bottomClamp = -60.0f;
	
	[Tooltip("Starting style of the third person character's camera. (1) BASIC is a free camera and (2) AIMING locks the character's facing direction towards the player's look inputs")]
	public CameraStyle startingStyle;
	
	[Tooltip("Slerp the character's graphics to the character's moving direction at this value times time.deltatime")]
	public float rotationSpeed = 7;
}
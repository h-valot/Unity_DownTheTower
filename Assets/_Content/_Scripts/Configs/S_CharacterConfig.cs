using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/character")]
public class CharacterConfig : ScriptableObject
{
	/// <summary>
	/// 	Unity in-built editor function called whenever the scriptable object is updated.
	/// </summary>
	public void OnValidate() => OnHierarchyChanged?.Invoke();
	public Action OnHierarchyChanged;


	[Header("Move")]
	[Tooltip("Walk speed of the character")]
	public float walkSpeed;
	
	[Tooltip("Sprint speed of the character")]
	public float runSpeed;
	
	[Tooltip("Acceleration rate.")]
	public float groundAcceleration = 10.0f;

	[Tooltip("Deceleration rate.")]
	public float groundDecceleration = 20.0f;

	[Tooltip("Percentage of character speed when on a slope. The current move speed is multiply by this value based on the slope angle.")]
	public AnimationCurve slopeSpeedModifier;

	[Tooltip("Used to snap the character to the floor when going down stairs and slopes.")]
	public float SnapGravity = -2f;


	[Header("Edge")]
	[Tooltip("")]
	public float noSlipDistance = 0.5f;

	[Tooltip("Speed scalar when the character is on an edge.")]
	public float edgeFallFactor = 1f;

    [Tooltip("Edge height that the character climb into.")]
    public float edgeMaxClimbingHeight = 1.5f;


    [Header("Ground")]
	[Tooltip("How far raycast moves down from origin point multiply by the capsule radius")]
	public float groundCheckYFactor = 2.5f;

	[Tooltip("Used to enlarge character capsule to approximate ground detection")]
	public float skinWidth = 0.1f;
	

	[Header("Jump")]
	[Tooltip("Speed added when using the jump button.")]
	public float jumpMinimalPlanarVelocity = 1.5f;

	[Tooltip("When jumping, the character reaches this height")]
	public float jumpHeight;

	[Tooltip("After touching the ground, this value is the time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpCooldown = 0.50f;


	[Header("Gravity")]
	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -9.8f;

	[Tooltip("Acceleration remove from character speed per second while falling.")]
	public float dragDecceleration = 1f;

	[Tooltip("Angular speed per second the character can turn while falling. Will be effective if character has a speed.")]
	public float airControlAngularSpeed = 30f;

	[Tooltip("Transform the dot product between character forward and input to a factor that multiply the air control angular speed.")]
	public AnimationCurve airControlInputFactor;


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

	[Tooltip("Max duration for slow, multiply the sowduration curve.")]
	public float maxSlowTime = 3f;

	[Tooltip("Percentage of the target speed reduction while slowed")]
	public AnimationCurve slowPercentage;

	[Tooltip("Duration of the slow after a stun.")]
	public float slowTimePostStun = 1f;


	[Header("Rope")]
	[Tooltip("HOLD_TO_STOP: stop the character from getting any further away from the rope base when pressing the corresponding input. HOLD_TO_LET_GO: letting the character getting further from the rope base when pressing the corresponding input.")]
	public RopeHolding ropeHoldingMethod;

	[Tooltip("Against wall raycasts will include only referenced layers.")]
	public LayerMask againstWallLayerToInclude;

	[Tooltip("Against wall raycasts will be this length.")]
	public float againstWallRayCastLength = 1f;

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

	[Tooltip("")]
	public float maxSideAngle = 45f;

	[Tooltip("")]
	public float ropeOffsetAngle = 5f;

	[Tooltip("The mass of the character. Used for rope pendulum effect calculations")]
	public float mass = 1f;

	[Tooltip("")]
	public float drag;

	[Tooltip("")]
	public float jumpOffWallForce;


	[Header("Camera")]
	[Tooltip("Clamp the camera rotation to this angle when looking down")]
	public float topClamp = 70.0f;
	
	[Tooltip("Clamp the camera rotation to this angle when looking up")]
	public float bottomClamp = -60.0f;
	
	[Tooltip("Starting style of the third person character's camera. (1) BASIC is a free camera and (2) AIMING locks the character's facing direction towards the player's look inputs")]
	public CameraStyle startingStyle;
	
	[Tooltip("Slerp the character's graphics to the character's moving direction at this value times time.deltatime")]
	public float rotationSpeed = 7;


	[Header("Self-glow")]
	[Tooltip("Radius of the self glow capsule")]
	public float glowRadius = 2f;

	[Tooltip("Height of the self glow capsule")]
	public float glowHeight = 1f;

	[Tooltip("Strength of the glow")]
	public float glowStrength = 0.1f;

	[Tooltip("Tint of the glow")]
	public Color glowColor = new Color(1f, 0.8196079f, 0.6666667f, 1f);


	[Header("Debug")]
	[Tooltip("Show debug ray used to determine grounded state")]
	public bool showGroundedDebug = false;
	public bool startWithBag = false;
}
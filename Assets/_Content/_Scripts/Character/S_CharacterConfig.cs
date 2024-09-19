using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/Character")]
public class CharacterConfig : ScriptableObject
{
	//Trigerred when a value is changed
	public Action OnValueChanged;
    public void OnValidate()
    {
		OnValueChanged?.Invoke();
    }

    [Header("MOTOR")]

	[Header("Speed")]
	[Tooltip("Move speed of the character")]
	public float moveSpeed = 3.0f;

	[Tooltip("Sprint speed of the character")]
	public float sprintSpeed = 6.0f;

	[Header("Air control")]
	[Tooltip("Speed of the character in air")]
	public float airControlSpeed = 1.0f;

	[Tooltip("While initializing a jump with a currentSpeed below this threshold, the airControlSpeed is increased to enhancedAirControlSpeed.")]
	public float enhancedAirControlThreshold = 0.2f;

	[Tooltip("Speed of the character in air with the speed boost.")]
	public float enhancedAirControlSpeed = 3.0f;


	[Header("Acceleration and deceletation")]
	public float speedChangeRate = 10.0f;


	[Header("Slope")]
	[Tooltip("Percentage of character speed when ascending")]
	public AnimationCurve uphillDeceleration;

	[Tooltip("Percentage of character speed when descending")]
	public AnimationCurve downhillAcceleration;


	[Header("Rotation")]
	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;


    [Tooltip("Time to craft the torch")]
    public int timeToCraft;

	[Header("Jump")]
	[Tooltip("The height the player can jump")]
	public float jumpHeight = 1.2f;

	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpDelay = 0.50f;


	[Header("Edging")]
	[Tooltip("If the character moves below this move speed and gathers others edging properties, snap the character on the edge.")]
	public float edgingSpeedThreshold = 0.50f;

	[Tooltip("The forward offset scalar of the down ray check.")]
	public float edgingForwardOffsetScalar = 1.5f;

	[Tooltip("Snap the character to the face if the dot product of the normal of the face touched by the down ray and the down vector of the character is less or equal to this value (vectors dot returns: (1) -1 if they point in completely opposite directions; (2) 0 if the vectors are perpendicular which means a angle of 90 degrees; (3) 1 if they point in exactly the same direction which means a angle of 0 degrees.")]
	[Range(-0.001f, -1f)] public float edgingDotProductThreshold = -0.5f;


	[Header("Fall")]
	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -15.0f;

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



	[Header("GROUNDED")]
	[Tooltip("Useful for rough ground")]
	public float groundedOffset = -0.14f;

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float groundedRadius = 0.28f;

	[Tooltip("What layers the character uses as ground")]
	public LayerMask groundLayers;

	[Header("Player Spawn")]
	[Tooltip("Spawn the player with a torch already in hand")]
	public bool torchInHand;

	[Header("Cinemachine")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float topClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float bottomClamp = -30.0f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float cameraAngleOverride = 0.0f;

	[Tooltip("For locking the camera position on all axis")]
	public bool lockCameraPosition = false;

	[Header("SELF GLOW")]
    [Tooltip("Radius of the self glow capsule")]
    public float glowRadius = 2f;
    [Tooltip("Height of the self glow capsule")]
    public float glowHeight = 1f;
    [Tooltip("Strength of the glow")]
    public float glowStrength = 0.1f;
    [Tooltip("Tint of the glow")]
    public Color glowColor = new Color(1f, 0.8196079f, 0.6666667f, 1f);

	[Header("AUDIO")]
	[Range(0, 1)] public float audioVolume = 0.5f;
	public AudioClip landingAudioClip;
	public AudioClip[] footstepAudioClips;
}
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/Character")]
public class CharacterConfig : ScriptableObject
{
	[Header("MOTOR")]

	[Header("Speed")]
	[Tooltip("Move speed of the character")]
	public float moveSpeed = 3.0f;

	[Tooltip("Sprint speed of the character")]
	public float sprintSpeed = 6.0f;

	[Tooltip("Speed of the character in air")]
	[Range(0f, 1f)] public float airSpeed = 0.6f;


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


	[Header("Jump")]
	[Tooltip("The height the player can jump")]
	public float jumpHeight = 1.2f;

	[Tooltip("The character uses its own gravity value. The engine default is -9.8f")]
	public float gravity = -15.0f;

	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpDelay = 0.50f;


	[Header("Fall")]
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



	[Header("CINEMACHINE")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float topClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float bottomClamp = -30.0f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float cameraAngleOverride = 0.0f;

	[Tooltip("For locking the camera position on all axis")]
	public bool lockCameraPosition = false;



	[Header("AUDIO")]
	[Range(0, 1)] public float audioVolume = 0.5f;
	public AudioClip landingAudioClip;
	public AudioClip[] footstepAudioClips;
}
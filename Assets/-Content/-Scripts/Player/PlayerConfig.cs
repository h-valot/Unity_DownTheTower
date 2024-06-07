using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player")]
public class PlayerConfig : ScriptableObject
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float moveSpeed = 3.0f;

	[Tooltip("Sprint speed of the character in m/s")]
	public float sprintSpeed = 6f;

	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;

	[Tooltip("Acceleration and deceleration")]
	public float speedChangeRate = 10.0f;

	public AudioClip landingAudioClip;

	[Range(0, 1)] public float footstepAudioVolume = 0.5f;
	public AudioClip[] footstepAudioClips;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float jumpHeight = 1.2f;

	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float gravity = -15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpTimeout = 0.50f;

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float fallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool grounded = true;

	[Tooltip("Useful for rough ground")]
	public float groundedOffset = -0.14f;

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float groundedRadius = 0.28f;

	[Tooltip("What layers the character uses as ground")]
	public LayerMask groundLayers;

	[Header("Cinemachine")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float topClamp = 70.0f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float bottomClamp = -30.0f;

	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float cameraAngleOverride = 0.0f;

	[Tooltip("For locking the camera position on all axis")]
	public bool lockCameraPosition = false;
}

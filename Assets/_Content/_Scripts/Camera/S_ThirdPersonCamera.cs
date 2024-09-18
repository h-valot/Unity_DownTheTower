using NaughtyAttributes;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Look _rseLook;

	[Header("External references")]
	[SerializeField] private Transform _orientation;
	[SerializeField] private Transform _character;
	[SerializeField] private Transform _characterGraphics;
	[SerializeField] private Transform _aimingLookAt;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private GameObject _thirdPersonCamera;
	[SerializeField] private GameObject _aimingCamera;
	[SerializeField] private Transform _cinemachineCameraTarget;

	[Header("debug")]
	[ReadOnly] public Vector2 _lookInput;
	[ReadOnly] public CameraStyle _currentStyle;

	// ----- PRIVATE VARIABLES -----
	// - cinemachine - 
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;

	// ----- CONST -----
	private const float _LOOK_THRESHOLD = 0.01f;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		HandleInputs();
	}

	private void LateUpdate()
	{
		HandleCamera();
	}

	private void OnEnable()
	{
		_rseLook.action += Look;
	}

	private void OnDisable()
	{
		_rseLook.action -= Look;
	}

	private void Initialize()
	{
		SwitchCameraStyle(_characterConfig.startingStyle);
		_cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
	}

	private void HandleInputs()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.BASIC);
		if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.AIMING);
	}

	private void HandleCamera()
	{
		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = Matha.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = Matha.ClampAngle(_cinemachineTargetPitch, _characterConfig.bottomClamp, _characterConfig.topClamp);

		// stops the camera if the character is dead
		if (_rsoPlayerDeath.value) return;

		// cinemachine will follow this target
		_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _characterConfig.cameraAngleOverride, _cinemachineTargetYaw, 0.0f);

		// rotate orientation
		Vector3 viewDirection = _character.position - new Vector3(transform.position.x, _character.position.y, transform.position.z);
		if (viewDirection != Vector3.zero)
		{
			_orientation.forward = viewDirection.normalized;
		}

		// rotate player object
		if (_currentStyle == CameraStyle.BASIC)
		{
			Vector3 inputDirection = _orientation.forward * _lookInput.y + _orientation.right * _lookInput.x;

			if (inputDirection != Vector3.zero)
			{
				_characterGraphics.forward = Vector3.Slerp(_characterGraphics.forward, inputDirection.normalized, Time.deltaTime * _characterConfig.rotationSpeed);
			}
		}

		else if (_currentStyle == CameraStyle.AIMING)
		{
			Vector3 directionToAimingLookAt = _aimingLookAt.position - new Vector3(_character.position.x, _aimingLookAt.position.y, _character.position.z);
			_orientation.forward = directionToAimingLookAt.normalized;

			_characterGraphics.forward = directionToAimingLookAt.normalized;
		}
	}

	private void SwitchCameraStyle(CameraStyle newStyle)
	{
		_aimingCamera.SetActive(false);
		_thirdPersonCamera.SetActive(false);

		if (newStyle == CameraStyle.BASIC) _thirdPersonCamera.SetActive(true);
		if (newStyle == CameraStyle.AIMING) _aimingCamera.SetActive(true);

		_currentStyle = newStyle;
	}

	private void Look(Vector2 input)
	{
		// exit, if there is no inputs
		if (input.sqrMagnitude < _LOOK_THRESHOLD)
		{
			return;
		}

		_lookInput = input;

		// don't multiply mouse input by Time.deltaTime;
		float deltaTimeMultiplier = _rsoControlScheme.value == "KeyboardMouse" ? 1.0f : Time.deltaTime;

		_cinemachineTargetYaw += input.x * deltaTimeMultiplier;
		_cinemachineTargetPitch += input.y * deltaTimeMultiplier;
	}
}

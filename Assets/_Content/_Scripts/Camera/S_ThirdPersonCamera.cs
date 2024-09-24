using NaughtyAttributes;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;

	[Header("External references")]
	[SerializeField] private Transform _cameraDirection;
	[SerializeField] private Transform _graphicsDirection;
	[SerializeField] private Transform _characterMotor;
	[SerializeField] private Transform _aimingLookAt;
	[SerializeField] private GameObject _thirdPersonCamera;
	[SerializeField] private GameObject _aimingCamera;
	[SerializeField] private Transform _cinemachineCameraTarget;
	[SerializeField] private Transform _lockedCameraTarget;

	[Header("debug")]
	[ReadOnly] public Vector2 _lookInput;
	[ReadOnly] public Vector2 _moveInput;
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
		_rseMove.action += Move;
		_rsoPlayerDeath.OnChanged += HandleDeath;
	}

	private void OnDisable()
	{
		_rseLook.action -= Look;
		_rseMove.action -= Move;
		_rsoPlayerDeath.OnChanged -= HandleDeath;
	}


	private void Initialize()
	{
		SwitchCameraStyle(_characterConfig.startingStyle);
		_cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
	}

	private void HandleInputs()
	{
		// temp
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
		_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);

        // rotate orientation
        Vector3 viewDirection = transform.forward - new Vector3(0, transform.forward.y, 0);
        if (viewDirection != Vector3.zero)
		{
			_cameraDirection.forward = viewDirection.normalized;
			_aimingLookAt.position = transform.position + transform.forward * 5.5f;
		}

		// rotate player object
		if (_currentStyle == CameraStyle.BASIC)
		{
			// character is facing the movement direction
			// but not is the moveInput is null or equals to zero
			Vector3 moveDirection = _cameraDirection.forward * _moveInput.y + _cameraDirection.right * _moveInput.x;
			if (_moveInput != Vector2.zero)
			{
				_graphicsDirection.forward = Vector3.Slerp(_graphicsDirection.forward, moveDirection.normalized, Time.deltaTime * _characterConfig.rotationSpeed);
			}
		}

		else if (_currentStyle == CameraStyle.AIMING)
		{
            _graphicsDirection.forward = _aimingLookAt.position - new Vector3(_cameraDirection.transform.position.x, _aimingLookAt.position.y, _cameraDirection.transform.position.z);
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

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void HandleDeath()
	{
		if (!_rsoPlayerDeath.value) return;

		_cinemachineCameraTarget.transform.SetParent(_lockedCameraTarget.transform);
	}
}

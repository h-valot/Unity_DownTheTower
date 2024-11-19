using Cinemachine;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
	[Header("External references")]
	[SerializeField] private CinemachineVirtualCamera _aimingCamera;
	[SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;

	[Header("Scriptable references")]
	[SerializeField] private OldCharacterConfig _characterConfig;
	[Space(5)]
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[Space(5)]
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSO_CameraStyle _rsoCameraStyle;

	// - Private variables -
	private Vector2 _lookInput;
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;
	private Transform _cameraTarget;

	// - Proprieties -
	public Vector3 PlanarForward { get; private set; }
	public Vector3 PlanarRight { get; private set; }

	private void LateUpdate()
	{
		HandleRotation();
		CalculatePlanarVectors();
	}

	private void OnEnable()
	{
		_rseLook.action += UpdateLookInput;
		_rsoCameraStyle.OnChanged += SwitchStyle;
		_rsoPlayerDeath.OnChanged += HandleDeath;
	}

	private void OnDisable()
	{
		_rseLook.action -= UpdateLookInput;
		_rsoCameraStyle.OnChanged -= SwitchStyle;
		_rsoPlayerDeath.OnChanged -= HandleDeath;
	}

	public void Initialize(Transform aimingLookAt, Transform cameraTarget)
	{
		_cameraTarget = cameraTarget;
		_aimingCamera.Follow = cameraTarget;
		_aimingCamera.LookAt = aimingLookAt;
		_thirdPersonCamera.Follow = cameraTarget;
		_thirdPersonCamera.LookAt = cameraTarget;

		PlanarForward = new Vector3(transform.forward.x, 0, transform.forward.z);
		PlanarRight = new Vector3(transform.right.x, 0, transform.right.z);

		_rsoCameraStyle.value = _characterConfig.startingStyle;
		_cinemachineTargetYaw = _cameraTarget.rotation.eulerAngles.y;
	}

	private void HandleRotation()
	{
		// Clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = Matha.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = Matha.ClampAngle(_cinemachineTargetPitch, _characterConfig.bottomClamp, _characterConfig.topClamp);

		// Stops the camera if the character is dead
		if (_rsoPlayerDeath.value) return;

		// Cinemachine will follow this target
		_cameraTarget.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
	}

	public void SwitchStyle()
	{
		_aimingCamera.gameObject.SetActive(false);
		_thirdPersonCamera.gameObject.SetActive(false);

		if (_rsoCameraStyle.value == CameraStyle.BASIC) _thirdPersonCamera.gameObject.SetActive(true);
		if (_rsoCameraStyle.value == CameraStyle.AIMING) _thirdPersonCamera.gameObject.SetActive(true);
	}

	public void CalculatePlanarVectors()
	{
		if (_lookInput == Vector2.zero) return;

		PlanarForward = new Vector3(transform.forward.x, 0, transform.forward.z);
		PlanarRight = new Vector3(transform.right.x, 0, transform.right.z);
	}

	private void UpdateLookInput(Vector2 input)
	{
		_lookInput = input;

		_cinemachineTargetYaw += input.x;
		_cinemachineTargetPitch += input.y;
    }

	private void HandleDeath()
	{
		if (!_rsoPlayerDeath.value) return;

		// Set parent as scene root 
		_cameraTarget.transform.parent = null;
	}
}

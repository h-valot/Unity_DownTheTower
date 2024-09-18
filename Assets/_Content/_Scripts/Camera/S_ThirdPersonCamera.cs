using NaughtyAttributes;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private CameraStyle _currentStyle;
	[SerializeField] private float _rotationSpeed;

	[Header("Scriptable references")]
	[SerializeField] private RSE_Look _rseLook;

	[Header("External references")]
	[SerializeField] private Transform _orientation;
	[SerializeField] private Transform _character;
	[SerializeField] private Transform _characterGraphics;
	[SerializeField] private Transform _aimingLookAt;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private GameObject _thirdPersonCamera;
	[SerializeField] private GameObject _aimingCamera;

	[Header("debug: look")]
	[ReadOnly] public Vector2 _lookInput;

	private void Update()
	{
		HandleInputs();
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

	private void HandleInputs()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.BASIC);
		if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.AIMING);
	}

	private void HandleCamera()
	{
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
				_characterGraphics.forward = Vector3.Slerp(_characterGraphics.forward, inputDirection.normalized, Time.deltaTime * _rotationSpeed);
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
		_lookInput = input;
	}
}

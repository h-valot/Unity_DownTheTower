using UnityEngine;
using NaughtyAttributes;

public class ControllerMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private CharacterController _controller;
	[SerializeField] private Animator _animator;
	[SerializeField] private GameObject _cinemachineCameraTarget;

	[Header("External references")]
	[SerializeField] private PlayerConfig _playerConfig;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;

	// cinemachine
	[ShowNonSerializedField] private float _cinemachineTargetYaw;
	[ShowNonSerializedField] private float _cinemachineTargetPitch;

	// player
	[ShowNonSerializedField] private Vector2 _moveInput;
	[ShowNonSerializedField] private bool _isGrounded;
	[ShowNonSerializedField] private bool _isSprinting;
	[ShowNonSerializedField] private float _speed;
	[ShowNonSerializedField] private float _targetSpeed;
	[ShowNonSerializedField] private float _animationBlend;
	[ShowNonSerializedField] private float _targetRotation = 0.0f;
	[ShowNonSerializedField] private float _rotationVelocity;
	[ShowNonSerializedField] private float _verticalVelocity;
	private const float _TERMINAL_VELOCITY = 53.0f;
	private const float _LOOK_THRESHOLD = 0.01f;

	// delay timer
	[ShowNonSerializedField] private float _fallDelayTimer;
	[ShowNonSerializedField] private float _jumpDelayTimer;

	// animations params
	private int _animSpeed;
	private int _animGrounded;
	private int _animJump;
	private int _animFreeFall;
	private int _animMotionSpeed;

	// private references
	private GameObject _mainCamera;


	private void Awake()
	{
		// get a reference to our main camera if null
		_mainCamera ??= GameObject.FindGameObjectWithTag("MainCamera");
	}

	private void Start()
	{
		_cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
		
		// assign animations params
		_animSpeed = Animator.StringToHash("Speed");
		_animJump = Animator.StringToHash("Jump");
		_animGrounded = Animator.StringToHash("Grounded");
		_animFreeFall = Animator.StringToHash("FreeFall");
		_animMotionSpeed = Animator.StringToHash("MotionSpeed");

		// reset our timeouts on start
		_fallDelayTimer = _playerConfig.fallDelay;
		_jumpDelayTimer = _playerConfig.jumpDelay;
	}

	private void Update()
	{
		CheckGrounded();
		HandleMove();
		ApplyGravity();
	}

	private void LateUpdate()
	{
		HandleCamera();
	}

	private void CheckGrounded()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _playerConfig.groundedOffset, transform.position.z);
		_isGrounded = Physics.CheckSphere(spherePosition, _playerConfig.groundedRadius, _playerConfig.groundLayers, QueryTriggerInteraction.Ignore);

		// update animator
		_animator.SetBool(_animGrounded, _isGrounded);
	}

	private void HandleMove()
	{
		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// set target speed based on move speed, sprint speed and if sprint is pressed
		_targetSpeed = _isSprinting ? _playerConfig.sprintSpeed : _playerConfig.moveSpeed;

		// if there is no input, set the target speed to 0
		// Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		if (_moveInput == Vector2.zero) 
		{
			_targetSpeed = 0.0f;
		}

		// a reference to the players current horizontal velocity
		float currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = 1f;

		// accelerate or decelerate to target speed
		if (currentSpeed < _targetSpeed - speedOffset
		|| currentSpeed > _targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentSpeed, _targetSpeed * inputMagnitude, Time.deltaTime * _playerConfig.speedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = _targetSpeed;
		}

		_animationBlend = Mathf.Lerp(_animationBlend, _targetSpeed, Time.deltaTime * _playerConfig.speedChangeRate);
		if (_animationBlend < 0.01f) _animationBlend = 0f;

		// normalise input direction
		Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

		// if there is a move input rotate player when the player is moving
		// Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		if (_moveInput != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _playerConfig.rotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		// move the player
		_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		// update animator if using character
		_animator.SetFloat(_animSpeed, _animationBlend);
		_animator.SetFloat(_animMotionSpeed, inputMagnitude);
	}

	private void ApplyGravity()
	{
		if (_isGrounded)
		{
			// reset the fall delay timer
			_fallDelayTimer = _playerConfig.fallDelay;

			_animator.SetBool(_animFreeFall, false);

			// the jump function can be called at any moment
			// to prevent the following line to cancel the jump animation
			// it is commented and the animation param bool have been switch to a trigger
			// _animator.SetBool(_animJump, false);

			// stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
			{
				_verticalVelocity = -2f;
			}

			// jump delay
			if (_jumpDelayTimer >= 0.0f)
			{
				_jumpDelayTimer -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump delay timer
			_jumpDelayTimer = _playerConfig.jumpDelay;

			// fall delay
			if (_fallDelayTimer >= 0.0f)
			{
				_fallDelayTimer -= Time.deltaTime;
			}
			else
			{
				_animator.SetBool(_animFreeFall, true);
			}
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _TERMINAL_VELOCITY)
		{
			_verticalVelocity += _playerConfig.gravity * Time.deltaTime;
		}
	}

	private void HandleCamera()
	{
		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = Matha.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = Matha.ClampAngle(_cinemachineTargetPitch, _playerConfig.bottomClamp, _playerConfig.topClamp);

		// cinemachine will follow this target
		_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _playerConfig.cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseLook.action += Look;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
	}

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseLook.action -= Look;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
	}

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void Look(Vector2 input)
	{
		// exit, if there is no inputs
		if (input.sqrMagnitude < _LOOK_THRESHOLD)
		{
			return;
		}
		
		// don't multiply mouse input by Time.deltaTime;
		float deltaTimeMultiplier = _rsoControlScheme.value == "KeyboardMouse" ? 1.0f : Time.deltaTime;

		_cinemachineTargetYaw += input.x * deltaTimeMultiplier;
		_cinemachineTargetPitch += input.y * deltaTimeMultiplier;
	}

	private void Jump()
	{
		if (!_isGrounded)
		{
			return;
		}

		if (_jumpDelayTimer >= 0)
		{
			return;
		}

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_verticalVelocity = Mathf.Sqrt(_playerConfig.jumpHeight * -2f * _playerConfig.gravity);

		// this function can be called at any moment
		// to prevent the jump animation to be cancelled, the animation param bool have been switch to a trigger
		_animator.SetTrigger(_animJump);
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

	private void OnFootstep(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			if (_playerConfig.footstepAudioClips.Length > 0)
			{
				var index = Random.Range(0, _playerConfig.footstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(_playerConfig.footstepAudioClips[index], transform.TransformPoint(_controller.center), _playerConfig.audioVolume);
			}
		}
	}

	private void OnLand(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			AudioSource.PlayClipAtPoint(_playerConfig.landingAudioClip, transform.TransformPoint(_controller.center), _playerConfig.audioVolume);
		}
	}
}
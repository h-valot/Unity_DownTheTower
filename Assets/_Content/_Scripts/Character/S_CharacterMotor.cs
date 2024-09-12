using NaughtyAttributes;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private CharacterController _controller;
	[SerializeField] private Animator _animator;
	[SerializeField] private GameObject _cinemachineCameraTarget;

	[Header("External references")]
	[SerializeField] private CharacterConfig _characterConfig;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private RSO_PlayerTransform _rsoPlayerTranform;

	// ----- CINEMACHINE -----
	[ShowNonSerializedField] private float _cinemachineTargetYaw;
	[ShowNonSerializedField] private float _cinemachineTargetPitch;

	// ----- CHARACTER -----
	// speed
	[ShowNonSerializedField] private bool _isSprinting;
	[ShowNonSerializedField] private Vector2 _moveInput;
	[ShowNonSerializedField] private float _speed;	
	[ShowNonSerializedField] private float _targetSpeed;
	[ShowNonSerializedField] private float _animationBlend;
	[ShowNonSerializedField] private float _slopePercentage;
	// falling
	[ShowNonSerializedField] private bool _isGrounded;
	[ShowNonSerializedField] private Vector3 _lastGroundedDirection;
	[ShowNonSerializedField] private float _lastGroundedSpeed;
	// rotation
	[ShowNonSerializedField] private float _targetRotation = 0.0f;
	[ShowNonSerializedField] private float _rotationVelocity;
	[ShowNonSerializedField] private float _verticalVelocity;
	// const
	private const float _TERMINAL_VELOCITY = 53.0f;
	private const float _LOOK_THRESHOLD = 0.01f;

	// ----- DELAY TIMER -----
	[ShowNonSerializedField] private float _fallDelayTimer;
	[ShowNonSerializedField] private float _jumpDelayTimer;

	// ----- ANIMATIONS PARAMS -----
	private int _animSpeed;
	private int _animGrounded;
	private int _animJump;
	private int _animFreeFall;
	private int _animMotionSpeed;

	// ----- PRIVATE VARIABLES -----
	private GameObject _mainCamera;
	private bool _groundedCheckLocked;


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
		_fallDelayTimer = _characterConfig.fallDelay;
		_jumpDelayTimer = _characterConfig.jumpDelay;
	}

	private void Update()
	{
		CheckGrounded();
		Accelerate();
		Move();
		ApplyGravity();
	}

	private void LateUpdate()
	{
		HandleCamera();
	}

	private void OnDrawGizmos()
	{
		// down vector
		Gizmos.color = Color.red;
		Gizmos.DrawLine(
			new Vector3(transform.position.x, transform.position.y - _characterConfig.groundedOffset, transform.position.z), 
			transform.position + (transform.TransformDirection(Vector3.down).normalized * _characterConfig.groundedRadius)
		);
	}

	private RaycastHit _result;

	private void CheckGrounded()
	{
		// set ray with offset
		Vector3 rayPosition = new Vector3(transform.position.x, transform.position.y - _characterConfig.groundedOffset, transform.position.z);
		Vector3 downVector = transform.TransformDirection(Vector3.down);
		Vector3 forwardVector = transform.TransformDirection(Vector3.forward);
		_isGrounded = false;

		if (Physics.Raycast(new Ray(rayPosition, downVector), out var result, _characterConfig.groundedRadius, _characterConfig.groundLayers, QueryTriggerInteraction.Ignore))
		{
			_isGrounded = true;

			// slope acceleration and deceleration
			Vector3 groundNormal = result.normal;

			// for normalized vectors dot returns 
			// • -1 if they point in completely opposite directions
			// • 0 if the vectors are perpendicular which means a angle of 90 degrees
			// • 1 if they point in exactly the same direction which means a angle of 0 degrees
			// so, 0.5 means a angle of 45 degrees
			float groundDotValue = 1 - Vector3.Dot(groundNormal, -downVector);

			// cross product to get the delta 
			// clamp it to avoid negative dot values and they are not needed
			int slopeAngle = (int)Mathf.Clamp(groundDotValue * 90f, 0, _controller.slopeLimit);
			_slopePercentage = (float)slopeAngle / (float)_controller.slopeLimit;

			// check the direction of the character based on the slope
			if (Vector3.Dot(groundNormal, forwardVector) > 0)
			{
				_slopePercentage *= -1;
			}
			
		}

		// update animator
		_animator.SetBool(_animGrounded, _isGrounded);

		if (!_isGrounded
			&& !_groundedCheckLocked)
		{
			// save last grounded momentum
			_lastGroundedSpeed = _speed;
			_lastGroundedDirection = transform.forward;

			_groundedCheckLocked = true;
		}

		if (_isGrounded 
			&& _groundedCheckLocked)
		{
			_groundedCheckLocked = false;
		}
	}

	private void Accelerate()
	{
		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// set target speed based on move speed, sprint speed and if sprint is pressed
		_targetSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.moveSpeed;

		// on slope acceleration and deceleration
		if (_slopePercentage > 0)
		{
			_targetSpeed *= 1 - _characterConfig.uphillDeceleration.Evaluate(_slopePercentage);
		}
		else if (_slopePercentage < 0)
		{
			_targetSpeed *= 1 + _characterConfig.downhillAcceleration.Evaluate(-_slopePercentage);
		}

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
			_speed = Mathf.Lerp(currentSpeed, _targetSpeed * inputMagnitude, Time.deltaTime * _characterConfig.speedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = _targetSpeed;
		}

		_animationBlend = Mathf.Lerp(_animationBlend, _targetSpeed, Time.deltaTime * _characterConfig.speedChangeRate);
		if (_animationBlend < 0.01f) _animationBlend = 0f;

		// update animator if using character
		_animator.SetFloat(_animSpeed, _animationBlend);
		_animator.SetFloat(_animMotionSpeed, inputMagnitude);
	}

	private void Move()
	{
		// normalise input direction
		Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

		// if there is a move input rotate player when the player is moving
		// Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		if (_moveInput != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _characterConfig.rotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
		float currentSpeed = _speed;

		// handle air control
		if (!_isGrounded) 
		{
			targetDirection = _lastGroundedDirection + (targetDirection * _characterConfig.airSpeed);
			currentSpeed = _lastGroundedSpeed;
		}

		// move the player
		_controller.Move(targetDirection.normalized * (currentSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        _rsoPlayerTranform.value = transform;

    }

	private void ApplyGravity()
	{
		if (_isGrounded)
		{
			// reset the fall delay timer
			_fallDelayTimer = _characterConfig.fallDelay;

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
			_jumpDelayTimer = _characterConfig.jumpDelay;

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
			_verticalVelocity += _characterConfig.gravity * Time.deltaTime;
		}
	}

	private void HandleCamera()
	{
		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = Matha.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = Matha.ClampAngle(_cinemachineTargetPitch, _characterConfig.bottomClamp, _characterConfig.topClamp);

		// cinemachine will follow this target
		_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _characterConfig.cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
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
			Debug.LogWarning("CHARACTER_MOTOR: can't jump, the character isn't grounded");
			return;
		}

		if (_jumpDelayTimer >= 0)
		{
			Debug.LogWarning("CHARACTER_MOTOR: can't jump, the jump delay timer isn't ready");
			return;
		}

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_verticalVelocity = Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity);

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
			if (_characterConfig.footstepAudioClips.Length > 0)
			{
				var index = UnityEngine.Random.Range(0, _characterConfig.footstepAudioClips.Length);
				AudioSource.PlayClipAtPoint(_characterConfig.footstepAudioClips[index], transform.TransformPoint(_controller.center), _characterConfig.audioVolume);
			}
		}
	}

	private void OnLand(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			AudioSource.PlayClipAtPoint(_characterConfig.landingAudioClip, transform.TransformPoint(_controller.center), _characterConfig.audioVolume);
		}
	}
}
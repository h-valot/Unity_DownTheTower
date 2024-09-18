using System;
using NaughtyAttributes;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private CharacterController _controller;
	[SerializeField] private Animator _animator;
	[SerializeField] private GameObject _cinemachineCameraTarget;

	[Header("Scriptable references")]
	[SerializeField] private CharacterConfig _characterConfig;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Throw _rseThrow;
	[SerializeField] private RSE_Lit_Unlit _rseLit_Unlit;
	[SerializeField] private RSE_CraftTorch _rseCraftTorch;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private GameObject _torchPrefab;
	[SerializeField] private GameObject _torchSpawner;
	[SerializeField] private RSO_PlayerTransform _rsoPlayerTranform;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

	[Header("External references")]
	[SerializeField] private GameObject _lockedCameraTarget;
	[SerializeField] private Camera _mainCamera;

	// ----- DEBUG -----
	[Header("debug: speed")]
	[ReadOnly] public bool _isSprinting;
	[ReadOnly] public float _targetSpeed;
	[ReadOnly] public float _currentSpeed;

	[Header("debug: gravity")]
	[ReadOnly] public Vector3 _gravityModifier;

	[Header("debug: ground & slope")]
	[ReadOnly] public float _slopePercentage;
	[ReadOnly] public float _slopeAngle;
	[ReadOnly] public Vector3 _slopeDirection;

	[Header("debug: fall")]
	[ReadOnly] public bool _isGrounded;
	[ReadOnly] public bool _isStunned;
	[ReadOnly] public bool _isSlowed;
	[ReadOnly] public bool _isJumpEnhanced;

	// ----- PUBLIC VARIABLES -----
	[HideInInspector] public bool torchInHand;

	// ----- PRIVATE VARIABLES -----
	// - move -
	private Vector2 _moveInput;
	private float _fallDelayTimer;
	private float _jumpDelayTimer;

	// - rotation -
	private float _targetRotation;
	private float _rotationVelocity;

	// - fall -
	private bool _groundedCheckLocked;
	private float _lastGroundedSpeed;
	private Vector3 _lastGroundedPosition;
	private Vector3 _lastGroundedDirection;
	private float _lastDistanceTravelled;
	private float _stunTimer;
	private float _slowTimer;
	private float _slowModifier;

	// - cinemachine - 
 	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;

	// - const -
	private const float _TERMINAL_VELOCITY = 53.0f;
	private const float _LOOK_THRESHOLD = 0.01f;

	// ----- ADDITIONAL SETTINGS -----
	// the starting position of the _isGrounded spherecast. set to the sphereCastRadius plus the CC Skin Width. enable showGizmos to visualize.
	// this should be just above the base of the cc, in the amount of the skin width (in case the cc sinks in)
	private float _groundCheckY = 0.33f;               // 0.25 + 0.08 (sphereCastRadius + CC skin width)
	private float _sphereCastRadius = 0.25f;           // radius of area to detect for ground
	private float _sphereCastDistance = 0.75f;         // How far spherecast moves down from origin point
	private float _raycastLength = 0.75f;              // secondary raycasts (match to sphereCastDistance)

	private Vector3 _rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
	private Vector3 _rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		CheckGrounded();
		ApplyGravity();
		Move();
	}

	private void LateUpdate()
	{
		HandleCamera();
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseLook.action += Look;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
		_rseThrow.action += Throw;
		_rseLit_Unlit.action += Lit_Unlit;
		_rseCraftTorch.action += CraftTorch;
	}

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseLook.action -= Look;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
		_rseThrow.action -= Throw;
		_rseLit_Unlit.action -= Lit_Unlit;
		_rseCraftTorch.action -= CraftTorch;
	}

	private void Initialize()
	{
		_cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;

		// reset our timeouts on start
		_fallDelayTimer = _characterConfig.fallDelay;
		_jumpDelayTimer = _characterConfig.jumpDelay;

		// update last grounded position to avoid instant death on spawn
		_lastGroundedPosition = transform.position;
	}

	private void CheckGrounded()
	{
		_isGrounded = false;

		// check lethal death
		_lastDistanceTravelled = Math.Abs(transform.position.y - _lastGroundedPosition.y);
		if (_lastDistanceTravelled >= _characterConfig.lethalHeight)
		{
			// stops camera movements
			_cinemachineCameraTarget.transform.SetParent(_lockedCameraTarget.transform);
		}

		Vector3 origin = new Vector3(transform.position.x, transform.position.y + _groundCheckY, transform.position.z);
		if (Physics.SphereCast(origin, _sphereCastRadius, Vector3.down, out var result, _sphereCastDistance))
		{
			_isGrounded = true;
			_lastGroundedPosition = transform.position;

			// // for normalized vectors dot returns 
			// // • -1 if they point in completely opposite directions
			// // • 0 if the vectors are perpendicular which means a angle of 90 degrees
			// // • 1 if they point in exactly the same direction which means a angle of 0 degrees
			// // so, 0.5 means a angle of 45 degrees
			// float groundDotValue = 1 - Vector3.Dot(result.normal, Vector3.up);

			// // cross product to get the delta 
			// // clamp it to avoid negative dot values and they are not needed
			// int slopeAngle = (int)Mathf.Clamp(groundDotValue * 90f, 0, _controller.slopeLimit);
			// _slopePercentage = (float)slopeAngle / (float)_controller.slopeLimit;

			// // check the direction of the character based on the slope
			// if (Vector3.Dot(result.normal, Vector3.forward) > 0)
			// {
			// 	_slopePercentage *= -1;
			// }

			// // angle of our slope (between these two vectors). 
			// // a hit normal is at a 90 degree angle from the surface that is collided with (at the point of collision).
			// // e.g. on a flat surface, both vectors are facing straight up, so the angle is 0.
			// _slopeAngle = Vector3.Angle(result.normal, Vector3.up);

			// // find the vector that represents our slope as well. 
			// // temp: basically, finds vector moving across hit surface 
			// Vector3 temp = Vector3.Cross(result.normal, Vector3.down);

			// // now use this vector and the hit normal, to find the other vector moving up and down the hit surface
			// _slopeDirection = Vector3.Cross(temp, result.normal);
		}

		// - when the character leaves the ground -
		if (!_isGrounded
			&& !_groundedCheckLocked)
		{
			_groundedCheckLocked = true;

			// save last grounded momentum
			_lastGroundedSpeed = _currentSpeed;
			_lastGroundedDirection = transform.forward;
			_lastGroundedPosition = transform.position;
		}

		// - when the character touches the ground -
		if (_isGrounded
			&& _groundedCheckLocked)
		{
			_groundedCheckLocked = false;

			// disable enhanced jump
			_isJumpEnhanced = false;
		}

		// we get angle values that we don't want. to correct for this, let's do some raycasts.
		// Vector3 originRay = new Vector3(transform.position.x, transform.position.y + _groundCheckY, transform.position.z);
		// if (Physics.Raycast(originRay + _rayOriginOffset1, Vector3.down, out var slopeHit1, _raycastLength))
		// {
		// 	// get angle of slope on hit normal
		// 	float angleOne = Vector3.Angle(slopeHit1.normal, Vector3.up);

		// 	if (Physics.Raycast(originRay + _rayOriginOffset2, Vector3.down, out var slopeHit2, _raycastLength))
		// 	{
		// 		// get angle of slope of these two hit points.
		// 		float angleTwo = Vector3.Angle(slopeHit2.normal, Vector3.up);

		// 		// 3 collision points: Take the MEDIAN by sorting array and grabbing middle.
		// 		float[] angles = new float[] { _slopeAngle, angleOne, angleTwo };
		// 		System.Array.Sort(angles);
		// 		_slopeAngle = angles[1];
		// 	}
		// 	else
		// 	{
		// 		// 2 collision points (sphere and first raycast): AVERAGE the two
		// 		float average = (_slopeAngle + angleOne) / 2;
		// 		_slopeAngle = average;
		// 	}
		// }
	}

	private void Move()
	{
		// - variables -
		Vector3 input = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
		Vector3 inputDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
		_currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
		Vector3 otherDirection = Vector3.zero;
		Vector3 mainDirection = Vector3.zero;
		float speedOffset = 0.1f;

		// set target speed based on move speed, sprint speed and if sprint is pressed
		_targetSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.moveSpeed;

		// - handle speed acceleration and deceleration on slope -
		if (_slopePercentage > 0)
		{
			_targetSpeed *= 1 - _characterConfig.uphillDeceleration.Evaluate(_slopePercentage);
		}
		else if (_slopePercentage < 0)
		{
			_targetSpeed *= 1 + _characterConfig.downhillAcceleration.Evaluate(-_slopePercentage);
		}

		if (_isSlowed)
		{
			_targetSpeed *= 1 - _slowModifier;
		}

		if (_isStunned)
		{
			_targetSpeed = 0;
		}

		if (_moveInput == Vector2.zero)
		{
			_targetSpeed = 0;
		}

		// if there is a move input rotate player when the player is moving
		if (_moveInput != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _characterConfig.rotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}

		// - handle speed acceleration and deceleration on move input pressed -
		if (_currentSpeed < _targetSpeed - speedOffset
		|| _currentSpeed > _targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// t in lerp is clamped, so we don't need to clamp our speed
			_currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * _characterConfig.speedChangeRate);

			// round speed to 3 decimal places
			_currentSpeed = Mathf.Round(_currentSpeed * 1000f) / 1000f;
		}
		else
		{
			_currentSpeed = _targetSpeed;
		}

		if (_isGrounded)
		{
			if (_slopeAngle > _controller.slopeLimit)
			{
				// - grounded on slope -
				// movement left/right while slipping down
				// player rotation to slope
				Vector3 slopeRight = Quaternion.LookRotation(Vector3.right) * _slopeDirection;
				float dot = Vector3.Dot(slopeRight, transform.right);

				// move on X axis, with Y rotation relative to slopeDir
				inputDirection = slopeRight * (dot > 0 ? transform.forward.normalized.x : -transform.forward.normalized.x);

				// increase angular gravity
				Vector3 rotationAngle = Vector3.Slerp(_gravityModifier, _slopeDirection * _characterConfig.sprintSpeed, 4f * Time.deltaTime);
				_gravityModifier = rotationAngle.normalized * _gravityModifier.magnitude;
			}
			else
			{
				// - grounded -
				// reset angular gravity mod movement
				_gravityModifier.x = 0;
				_gravityModifier.z = 0;

				// constant grounded gravity
				if (_gravityModifier.y < 0)
				{
					_gravityModifier.y = Mathf.Lerp(_gravityModifier.y, -1f, 4f * Time.deltaTime);
				}
			}

			// grounded movement forces
			mainDirection = inputDirection.normalized * _currentSpeed;
			otherDirection = Vector2.zero;
		}
		else
		{
			// - handle air control -
			float airSpeed = _isJumpEnhanced
				? _characterConfig.enhancedAirControlSpeed
				: _characterConfig.airControlSpeed;

			// clamp the airSpeed to the target speed
			if (_lastGroundedSpeed >= _targetSpeed)
			{
				_lastGroundedSpeed = _targetSpeed;
				airSpeed = 0;
			}
			else if (_lastGroundedSpeed + airSpeed >= _targetSpeed)
			{
				airSpeed -= _targetSpeed - (_lastGroundedSpeed + airSpeed); 
			}

			// nullify the direction if input's magnitude are smaller than enhanced jump threshol
			if (_moveInput.magnitude <= _characterConfig.enhancedAirControlThreshold)
			{
				inputDirection = Vector3.zero;
			}

			// air movement forces
			mainDirection = _lastGroundedDirection.normalized * _lastGroundedSpeed;
			otherDirection = inputDirection * airSpeed;
		}

		// - move -
		_controller.Move(Time.deltaTime * (mainDirection + otherDirection + _gravityModifier));

		// update rso character transform data
		if (_rsoPlayerTranform.value != transform) _rsoPlayerTranform.value = transform;
	}

	private void ApplyGravity()
	{
		if (_isGrounded)
		{
			// reset the fall delay timer
			_fallDelayTimer = _characterConfig.fallDelay;

			// stop our velocity dropping infinitely when grounded
			if (_gravityModifier.y < 0.0f)
			{
				_gravityModifier.y = -2f;
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
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_gravityModifier.y < _TERMINAL_VELOCITY)
		{
			_gravityModifier.y += _characterConfig.gravity * Time.deltaTime;
		}
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

		// sometimes, the character is not able to climb on mid/low ground with impulsion
		// add speed or increase the air control sensibility
		_isJumpEnhanced = _currentSpeed <= _characterConfig.enhancedAirControlThreshold;

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_gravityModifier.y = Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity);
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

	private void Throw()
	{
		Ray r = _mainCamera.ScreenPointToRay(Input.mousePosition);

		Vector3 dir = r.GetPoint(1) - r.GetPoint(0);
		GetComponentInChildren<Torch>().ThrowTorch(dir);
	}

	private void Lit_Unlit()
	{
		GetComponentInChildren<Torch>().ChangeLightState();
    }

	private void CraftTorch()
	{
		if (torchInHand == true)
		{
			GameObject _newTorch = Instantiate(_torchPrefab, _torchSpawner.transform);
			_newTorch.transform.position = _torchSpawner.transform.position;
			_newTorch.transform.rotation = _torchSpawner.transform.rotation;
		}
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

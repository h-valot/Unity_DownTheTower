using System;
using NaughtyAttributes;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _orientation;
	[SerializeField] private Transform _graphicsParent;
	[SerializeField] private CharacterController _controller;

	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private RSO_PlayerTransform _rsoPlayerTransform;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;

	[Header("debug: move")]
	[ReadOnly] public Vector2 _moveInput;
	[ReadOnly] public float _targetSpeed;
	[ReadOnly] public float _currentSpeed;
	[ReadOnly] public float _moveSpeed;
	[ReadOnly] public bool _isSprinting;

	[Header("debug: gravity")]
	[ReadOnly] public Vector3 _gravityModifier;

	[Header("debug: slope")]
	[ReadOnly] public float _slopePercentage;
	[ReadOnly] public float _slopeAngle;
	[ReadOnly] public Vector3 _slopeDirection;

	[Header("debug: fall")]
	[ReadOnly] public bool _isGrounded;
	[ReadOnly] public bool _isStunned;
	[ReadOnly] public bool _isSlowed;
	[ReadOnly] public bool _inAir;

	[Header("debug: momentum")]
	[ReadOnly] public float _lastGroundedSpeed;
	[ReadOnly] public Vector3 _lastGroundedPosition;
	[ReadOnly] public Vector3 _lastGroundedDirection;
	[ReadOnly] public float _lastDistanceTravelled;

	// ----- PRIVATE VARIABLES -----
	private bool _groundedCheckLocked;
	private float _stunTimer;
	private float _slowTimer;
	private float _slowModifier;
	private RaycastHit _groundHit;
	private float _jumpDelayTimer;

	// ----- CONST -----
	private const float _TERMINAL_VELOCITY = 53.0f;

	private void Update()
	{
		CheckGround();
		HandleStun();
		HandleSlow();
		Accelerate();
		ApplyGravity();
		HandleMovement();
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
	}

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
	}

	private void CheckGround()
	{
		_isGrounded = false;
		
		_lastDistanceTravelled = Math.Abs(transform.position.y - _lastGroundedPosition.y);

		Vector3 origin = new Vector3(transform.position.x, transform.position.y + _characterConfig.groundCheckY, transform.position.z);
		if (Physics.Raycast(origin, Vector3.down, out _groundHit, _characterConfig.sphereCastDistance))
		{
			_isGrounded = true;

			_slopeAngle = Vector3.Angle(_groundHit.normal, Vector3.up);
			_slopePercentage = _slopeAngle / _characterConfig.slopeLimit;

			// check the direction of the character based on the slope
			if (Vector3.Dot(_groundHit.normal, _graphicsParent.forward) > 0)
			{
				_slopePercentage *= -1;
			}
		}

		// - when the character leaves the ground -
		if (!_isGrounded && !_groundedCheckLocked)
		{
			_groundedCheckLocked = true;

			// save last grounded momentum
			_lastGroundedSpeed = _moveSpeed;
			_lastGroundedDirection = _graphicsParent.forward;
			_lastGroundedPosition = transform.position;
		}

		// - when the character touches the ground -
		if (_isGrounded && _groundedCheckLocked)
		{
			_groundedCheckLocked = false;

			if (_lastDistanceTravelled >= _characterConfig.lethalHeight)
			{
				HandleDeath();
			}
			else if (_lastDistanceTravelled >= _characterConfig.stunHeight)
			{
				// stun the character for x secondes
				_isStunned = true;

				// cross product to get the stun mitiged value on a 0-1 scale
				float stunMitigedValue = (_lastDistanceTravelled - _characterConfig.stunHeight) / (_characterConfig.lethalHeight - _characterConfig.stunHeight);
				_stunTimer = _characterConfig.stunDuration.Evaluate(stunMitigedValue);
			}
			else if (_lastDistanceTravelled >= _characterConfig.slowHeight)
			{
				// slow the character for x secondes by y percent
				_isSlowed = true;

				// cross product to get the slow mitiged value on a 0-1 scale
				float slowMitigedValue = (_lastDistanceTravelled - _characterConfig.slowHeight) / (_characterConfig.stunHeight - _characterConfig.slowHeight);
				_slowTimer = _characterConfig.slowDuration.Evaluate(slowMitigedValue);
				_slowModifier = _characterConfig.slowPercentage.Evaluate(slowMitigedValue);
			}
		}
	}

	private void HandleDeath()
	{
		_rsoPlayerDeath.value = true;
		Destroy(gameObject);
	}

	private void HandleStun()
	{
		if (!_isStunned) return;

		_stunTimer -= Time.deltaTime;
		if (_stunTimer <= 0)
		{
			_isStunned = false;

			// slows the character using the stun and lethal height
			_isSlowed = true;
			float slowMitigedValue = (_lastDistanceTravelled - _characterConfig.stunHeight) / (_characterConfig.lethalHeight - _characterConfig.stunHeight);
			_slowTimer = _characterConfig.slowDuration.Evaluate(slowMitigedValue);
			_slowModifier = _characterConfig.slowPercentage.Evaluate(slowMitigedValue);
		}
	}

	private void HandleSlow()
	{
		if (!_isSlowed) return;

		_slowTimer -= Time.deltaTime;
		_isSlowed = _slowTimer > 0;
	}

	private void Accelerate()
	{
		// - variables -
		_targetSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.walkSpeed;
		_currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
		float speedOffset = 0.1f;

		// slope modifications
		Vector3 origin = 
			transform.position 
			+ _graphicsParent.forward.normalized * 0.5f
			+ Vector3.up.normalized * 0.5f;

		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1f) && _isGrounded)
		{
			UnityEngine.Debug.DrawRay(origin, Vector3.down, Color.red);
			if (_slopePercentage > 0)
			{
				_targetSpeed *= 1 - _characterConfig.uphillDeceleration.Evaluate(_slopePercentage);
			}
			else if (_slopePercentage < 0)
			{
				_targetSpeed *= 1 + _characterConfig.downhillAcceleration.Evaluate(-_slopePercentage);
			}
		}

		// apply status effects
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
			_targetSpeed = 0.0f;
		}

		// accelerate or decelerate to target speed
		if (_currentSpeed < _targetSpeed - speedOffset
		|| _currentSpeed > _targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// T in Lerp is clamped, so we don't need to clamp our speed
			_moveSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * _characterConfig.speedChangeRate);

			// round speed to 3 decimal places
			_moveSpeed = Mathf.Round(_moveSpeed * 1000f) / 1000f;
		}
		else
		{
			_moveSpeed = _targetSpeed;
		}
	}

	private void ApplyGravity()
	{
		if (_isGrounded)
		{
			// stop our velocity dropping infinitely when grounded
			if (_gravityModifier.y < 0.0f) _gravityModifier.y = -2.0f;

			// runs prevent jump timer
			if (_jumpDelayTimer >= 0.0f) _jumpDelayTimer -= Time.deltaTime;
			else _inAir = false;
		}
		else 
		{
			_jumpDelayTimer = _characterConfig.jumpDelay;
			_inAir = true;
		}

		// apply gravity over time if under terminal
		// multiply by delta time twice to linearly speed up over time
		if (_gravityModifier.y < _TERMINAL_VELOCITY)
		{
			_gravityModifier.y += _characterConfig.gravity * Time.deltaTime;
		}
	}

	private void HandleMovement()
	{
		// - variables -
		Vector3 direction = _orientation.forward * _moveInput.y + _orientation.right * _moveInput.x;

		// - handle slope sliding -
		if (Physics.SphereCast(transform.position + _controller.center, _controller.radius - _controller.skinWidth, Vector3.down, out var hitInfo, _controller.height * 0.7f))
		{
			Vector3 relativeHitPoint = hitInfo.point - (transform.position + _controller.center);
			relativeHitPoint.y = 0;

			if (relativeHitPoint.magnitude > _characterConfig.noSlipDistance)
			{
				Vector3 edgeFallMovement = transform.position - hitInfo.point;
				edgeFallMovement.y = 0;
				direction += edgeFallMovement * Time.deltaTime * _characterConfig.edgeFallFactor - _gravityModifier;
			}
		}

		// - grounded -
		if (_isGrounded)
		{
			_controller.Move(Time.deltaTime * (
				direction.normalized * _moveSpeed
				+ _gravityModifier
			));
		}

		// - in air -
		else
		{
			_controller.Move(Time.deltaTime * (
				// last ground direction and speed to keep the inertia going on
				_lastGroundedDirection.normalized * _lastGroundedSpeed
				// current direction and speed reduced by the air control modifier to slightly moves while in air
				+ direction * _targetSpeed * _characterConfig.airControlModifier
				+ _gravityModifier
			));
		}

		// keep the character grounded
		if (_isGrounded && !_inAir && _gravityModifier.y <= 2f)
		{
			Vector3 extraGravity = new Vector3(
				_controller.velocity.x,
				-_controller.stepOffset / Time.deltaTime,
				_controller.velocity.z
			);

			_controller.Move(Time.deltaTime * extraGravity);
		}

		// - update variables -
		if (_rsoPlayerTransform.value != transform) _rsoPlayerTransform.value = transform;
	}

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void Jump()
	{
		if (!_isGrounded || _jumpDelayTimer > 0.0f)
		{
			return;
		}

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_gravityModifier.y = Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity);
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}
}
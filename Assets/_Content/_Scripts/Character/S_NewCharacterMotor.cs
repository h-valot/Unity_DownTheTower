using System;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _orientation;
	[SerializeField] private Rigidbody _rigidbody;

	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private RSO_PlayerTransform _rsoPlayerTransform;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;

	[Header("debug: move")]
	[ReadOnly] public float _moveSpeed;
	[ReadOnly] public bool _isSprinting;
	[ReadOnly] public Vector3 _moveInput;

	[Header("debug: fall")]
	[ReadOnly] public bool _isGrounded;
	[ReadOnly] public bool _isStunned;
	[ReadOnly] public bool _isSlowed;
	[ReadOnly] public bool _isJumping;

	// ----- PRIVATE VARIABLES -----
	private bool _groundedCheckLocked;
	private Vector3 _lastGroundedPosition;
	private Vector3 _lastGroundedDirection;
	private float _lastGroundedSpeed;
	private float _lastDistanceTravelled;
	private float _stunTimer;
	private float _slowTimer;
	private float _slowModifier;

	// ----- CONST -----
	private const float _RIGIDBODY_FORCE_MODIFIER = 10f;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		HandleStun();
		HandleSlow();

		CheckGround();
		SpeedControl();
	}

	private void FixedUpdate()
	{
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

	private void Initialize()
	{
		// update last grounded position to avoid instant death on spawn
		_lastGroundedPosition = transform.position;
	}

	private void CheckGround()
	{
		_isGrounded = false;
		
		_lastDistanceTravelled = Math.Abs(transform.position.y - _lastGroundedPosition.y);

		Vector3 origin = new Vector3(transform.position.x, transform.position.y + _characterConfig.groundCheckY, transform.position.z);
		if (Physics.SphereCast(origin, _characterConfig.sphereCastRadius, Vector3.down, out var result, _characterConfig.sphereCastDistance))
		{
			_isGrounded = true;
		}

		// - when the character leaves the ground -
		if (!_isGrounded && !_groundedCheckLocked)
		{
			_groundedCheckLocked = true;

			// save last grounded momentum
			_lastGroundedSpeed = _moveSpeed;
			_lastGroundedDirection = _orientation.forward;
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

	private void SpeedControl()
	{
		// - variables -
		Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
		_moveSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.walkSpeed;

		// - apply status effects -
		if (_isSlowed) _moveSpeed *= 1 - _slowModifier;
		if (_isStunned) _moveSpeed = 0;

		// limit velocity if needed
		if (flatVelocity.magnitude > _moveSpeed)
		{
			Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
			_rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
		}
	}

	private void HandleMovement()
	{
		// - variables -
		Vector3 direction = _orientation.forward * _moveInput.y + _orientation.right * _moveInput.x;

		// - grounded -
		if (_isGrounded)
		{
			_rigidbody.AddForce(
				direction.normalized * _moveSpeed * _RIGIDBODY_FORCE_MODIFIER,
				ForceMode.Force
			);
		}

		// - in air -
		else
		{
			_rigidbody.AddForce(
				// last ground direction and speed to keep the inertia going on
				_lastGroundedDirection.normalized * _lastGroundedSpeed * _RIGIDBODY_FORCE_MODIFIER
				// current direction and speed reduced by the air control modifier to slightly moves while in air
				+ direction * _moveSpeed * _characterConfig.airControlModifier * _RIGIDBODY_FORCE_MODIFIER,
				ForceMode.Force
			);
		}

		// update rso character transform data
		if (_rsoPlayerTransform.value != transform) _rsoPlayerTransform.value = transform;
	}

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void Jump()
	{
		if (!_isGrounded
			|| _isJumping)
		{
			return;
		}

		_isJumping = true;

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_rigidbody.velocity = new Vector3(
			_rigidbody.velocity.x,
			Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity),   
			_rigidbody.velocity.z
		);

		_isJumping = false;
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}
}
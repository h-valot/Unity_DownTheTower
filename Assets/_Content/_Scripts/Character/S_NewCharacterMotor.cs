using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _cameraDirection;
	[SerializeField] private Transform _characterDirection;
	[SerializeField] private Transform _torchParent;
	[SerializeField] private CharacterController _controller;

	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private LadderConfig _ladderConfig;
	[SerializeField] private TorchConfig _torchConfig;
	[SerializeField] private RSO_PlayeGraphicsDirection _rsoPlayerTransform;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Throw _rseThrow;
	[SerializeField] private RSE_ToggleLight _rseToggleLight;
	[SerializeField] private RSE_CraftTorch _rseCraftTorch;

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
	// - status -
	private float _stunTimer;
	private float _slowTimer;
	private float _slowModifier;

	// - ground -
	private bool _groundedCheckLocked;
	private RaycastHit _groundHit;

	// - jump -
	private float _jumpDelayTimer;

	// - permanent -
	private PreLadder _currentPreLadder;
	private Torch _currentTorch;
	private bool _isCrafting;

	// ----- CONST -----
	private const float _TERMINAL_VELOCITY = 53.0f;

	private void Update()
	{
		// temp
		HandleInputs();

		CheckGround();
		HandleSlope();
		HandleStun();
		HandleSlow();
		Accelerate();
		ApplyGravity();
		HandleMovement();
	}

	private void HandleInputs()
	{
		// temp
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			_currentPreLadder = Instantiate(_ladderConfig.pfPreLadder);
			_currentPreLadder.Initialize(_cameraDirection);
		}

		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			_currentPreLadder.InstantiateLadder();
			if (_currentPreLadder != null)
			{
				Destroy(_currentPreLadder.gameObject);
				_currentPreLadder = null;
			}
		}
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
		_rseThrow.action += Throw;
		_rseCraftTorch.action += CraftTorch;
		_rseToggleLight.action += ToggleLight;
	}

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
		_rseThrow.action -= Throw;
		_rseCraftTorch.action -= CraftTorch;
		_rseToggleLight.action -= ToggleLight;
	}

	private void CheckGround()
	{
		Vector3 origin = new Vector3(transform.position.x, transform.position.y + _characterConfig.groundCheckY, transform.position.z);
		_isGrounded = Physics.Raycast(origin, Vector3.down, out _groundHit, _characterConfig.raycastLength);

		// - when the character leaves the ground after being grounded-
		if (!_isGrounded && !_groundedCheckLocked)
		{
			_groundedCheckLocked = true;

			// save last grounded momentum
			_lastGroundedSpeed = _moveSpeed;
			_lastGroundedDirection = _characterDirection.forward;
			_lastGroundedPosition = transform.position;
		}

		// - when the character touches the ground after being in the air -
		if (_isGrounded && _groundedCheckLocked)
		{
			_groundedCheckLocked = false;

			_lastDistanceTravelled = Math.Abs(transform.position.y - _lastGroundedPosition.y);
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

	private void HandleSlope()
	{
		if (!_isGrounded) return;

		float middleAngle = Vector3.Angle(_groundHit.normal, Vector3.up);

		// we get angle values that we don't want. to correct for this, let's do some raycasts.
		Vector3 originForward =
			transform.position
			+ Vector3.up * _characterConfig.groundCheckY
			+ _characterDirection.forward * 0.5f;

		if (Physics.Raycast(originForward, Vector3.down, out var slopeHitForward, _characterConfig.raycastLength))
		{
			UnityEngine.Debug.DrawRay(originForward, Vector3.down, Color.white);

			// get angle of slope on hit normal
			float angleForward = Vector3.Angle(slopeHitForward.normal, Vector3.up);

			Vector3 originBackward =
				transform.position
				+ Vector3.up * _characterConfig.groundCheckY
				- _characterDirection.forward * 0.5f;

			if (Physics.Raycast(originBackward, Vector3.down, out var slopeHitBackward, _characterConfig.raycastLength))
			{
				UnityEngine.Debug.DrawRay(originBackward, Vector3.down, Color.white);

				// get angle of slope of these two hit points.
				float angleBackward = Vector3.Angle(slopeHitBackward.normal, Vector3.up);

				// 3 collision points: Take the MEDIAN by sorting array and grabbing middle.
				float[] angles = new float[] { angleForward, middleAngle, angleBackward };
				System.Array.Sort(angles);
				_slopeAngle = angles[1];
			}
			else
			{
				// 2 collision points (sphere and first raycast): MINIMUM the two
				float minimum = Mathf.Min(angleForward, middleAngle);
				_slopeAngle = minimum;
			}
		}

		// get the slope percentage to calculate slows later in the movement function
		_slopePercentage = _slopeAngle / _controller.slopeLimit;

		// check the direction of the character based on the slope
		if (Vector3.Dot(_groundHit.normal, _characterDirection.forward) > 0)
		{
			_slopePercentage *= -1;
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
		float speedOffset = 0.1f;

		// slope modifications
		Vector3 origin = 
			transform.position 
			+ _characterDirection.forward * 0.5f
			+ Vector3.up * 0.5f;

		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1f) 
			&& _isGrounded
			&& _slopeAngle <= _controller.slopeLimit)
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
			_moveSpeed += Time.deltaTime * _characterConfig.speedChangeRate;
			_moveSpeed = Mathf.Clamp(_moveSpeed, 0, _targetSpeed);
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
		Vector3 direction = _cameraDirection.forward * _moveInput.y + _cameraDirection.right * _moveInput.x;

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

		// keep the character grounded for stairs and downhill slopes
		if (_isGrounded 
			&& !_inAir 
			&& _gravityModifier.y <= 2f)
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

	private void Throw()
	{
		// the following code works only with the torch
		// this will change as soon of throw and craft component are ready to use

		if (_currentTorch == null) return;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 direction = ray.GetPoint(1) - ray.GetPoint(0);
		_currentTorch?.Throw(direction);

		_currentTorch = null;
	}

	private void ToggleLight()
	{
		_currentTorch?.ToggleLight();
	}

	private void CraftTorch()
	{
		if (_currentTorch == null 
			&& !_isCrafting)
		{
			StartCoroutine(SpawnTorch());
		}
	}

	private IEnumerator SpawnTorch()
	{
		_isCrafting = true;

		// wait the crafting duration
		yield return new WaitForSeconds(_torchConfig.craftingDuration);

		// instantiate the torch in the character's hand
		_currentTorch = Instantiate(_torchConfig.pfTorch, _torchParent.transform);
		_currentTorch.transform.position = _torchParent.transform.position;

		_isCrafting = false;
	}
}
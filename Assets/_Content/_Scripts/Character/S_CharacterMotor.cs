using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Obi;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
	#region exposed variables

	[Header("Internal references")]
	[SerializeField] private Transform _cameraDirection;
	[SerializeField] private Transform _characterDirection;
	[SerializeField] private Transform _torchParent;
	[SerializeField] private Transform _ropeAttach;
	[SerializeField] private CharacterController _controller;
	public ObiCollider obiCollider;

	[Header("Scriptable references")]
	[SerializeField] private CharacterConfig _characterConfig;
	[SerializeField] private LadderConfig _ladderConfig;
	[SerializeField] private RopeConfig _ropeConfig;
	[SerializeField] private TorchConfig _torchConfig;
    [SerializeField] private RSO_CharacterForward _rsoCharacterForward;
	[SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;
    [SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Throw _rseThrow;
	[SerializeField] private RSE_ToggleLight _rseToggleLight;
	[SerializeField] private RSE_CraftTorch _rseCraftTorch;
	[SerializeField] private RSE_Interact _rseInteract;
    [SerializeField] private RSE_CancelAction _rseCancelAction;
    [SerializeField] private RSE_CraftLadder _rseCraftLadder;
    [SerializeField] private RSE_CraftRope _rseCraftRope;

    #endregion

    #region runtime variables

    [Header("debug: animation")]
	[ReadOnly] public AnimationState _currentState;

	[Header("debug: move")]
	[ReadOnly] public Vector2 _moveInput;
	[ReadOnly] public Vector3 _velocity;
	[ReadOnly] public float _targetSpeed;
	[ReadOnly] public float _currentSpeed;
	[ReadOnly] public float _moveSpeed;
	[ReadOnly] public bool _isSprinting;
	[ReadOnly] public float _coyoteTimer;

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

	[Header("debug: permanent")]
	[ReadOnly] public float ropeLength;

	// ----- PRIVATE VARIABLES -----
	// - status -
	private float _stunTimer;
	private float _slowTimer;
	private float _slowModifier;

	// - ground -
	private bool _groundedCheckLocked;
	private RaycastHit _groundHit;

	// - jump -
	private float _jumpTimer;

    // - interact -
    private List<Interactible> _interactables;
    private Interactible _nearestInteractible;

    // - permanent -
    private PreLadder _currentPreLadder;
	private PreRope _currentPreRope;
	private Torch _currentTorch;
	private bool _isCrafting;

	// ----- CONST -----
	private const float _TERMINAL_VELOCITY = 53.0f;

	#endregion

	#region monobehaviour functions

	private void Start()
	{
		SwitchState(AnimationState.LOCOMOTION);

        // creation of the interaction list
        _interactables = new List<Interactible>();
    }

	private void Update()
	{
		UpdateCurrentState();
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
		_rseThrow.action += Throw;
		_rseCraftTorch.action += CraftTorch;
		_rseToggleLight.action += ToggleLight;
		_rseCancelAction.action += CancelAction;
		_rseCraftRope.action += CraftRope;
		_rseCraftLadder.action += CraftLadder;
        _rseInteract.action += Interact;
    }

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
		_rseThrow.action -= Throw;
		_rseCraftTorch.action -= CraftTorch;
		_rseToggleLight.action -= ToggleLight;
        _rseCancelAction.action -= CancelAction;
        _rseCraftRope.action -= CraftRope;
        _rseCraftLadder.action -= CraftLadder;
        _rseInteract.action -= Interact;
    }

	#endregion

	#region animation state

	/// <summary>
	/// 	call the update function of the current state.
	/// </summary>
	private void UpdateCurrentState()
	{
		switch (_currentState)
		{
			case AnimationState.LOCOMOTION:
				UpdateLocomotionState();
				break;

			case AnimationState.JUMP:
				UpdateJumpState();
				break;

			case AnimationState.FALL:
				UpdateFallState();
				break;

			case AnimationState.CRAFT:
				UpdateCraftState();
				break;

			case AnimationState.ROPE:
				UpdateRopeState();
				break;

			case AnimationState.LADDER:
				UpdateLadderState();
				break;
		}
	}

	/// <summary>
	/// 	exit current state and enter the given state.
	/// </summary>
	/// <param name="newState">state to enter into</param>
	private void SwitchState(AnimationState newState)
	{
		ExitCurrentState();
		EnterState(newState);
	}

	/// <summary>
	/// 	call the exit function of the current state.
	/// </summary>
	private void ExitCurrentState()
	{
		switch (_currentState)
		{
			case AnimationState.LOCOMOTION:
				ExitLocomotionState();
				break;
			
			case AnimationState.JUMP:
				ExitJumpState();
				break;
			
			case AnimationState.FALL:
				ExitFallState();
				break;
			
			case AnimationState.CRAFT:
				ExitCraftState();
				break;
			
			case AnimationState.ROPE:
				ExitRopeState();
				break;
			
			case AnimationState.LADDER:
				ExitLadderState();
				break;
		}
	}

	/// <summary>
	/// 	call the enter function of the given state.
	/// </summary>
	/// <param name="newState">state to enter into</param>
	private void EnterState(AnimationState newState)
	{
		switch (newState)
		{
			case AnimationState.LOCOMOTION:
				EnterLocomotionState();
				break;

			case AnimationState.JUMP:
				EnterJumpState();
				break;

			case AnimationState.FALL:
				EnterFallState();
				break;

			case AnimationState.CRAFT:
				EnterCraftState();
				break;

			case AnimationState.ROPE:
				EnterRopeState();
				break;

			case AnimationState.LADDER:
				EnterLadderState();
				break;
		}
	}

	#endregion

	#region ground checks

	/// <summary>
	/// 	use raycasting to check if the character has a collider below it.
	/// 	save last grounded variables when the character leaves the ground.
	/// 	handle falling when the character touches the ground.
	/// </summary>
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
			_isJumping = false;

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

	/// <summary>
	/// 	set the slope angle to the smallest angle value amoung 3 raycasts.
	/// 	try to get the slope deceleration or acceleration percentage based on the slope angle.
	/// </summary>
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

				// 3 collision points: Take the MINIMUM by sorting array and grabbing middle.
				float[] angles = new float[] { angleForward, middleAngle, angleBackward };
				System.Array.Sort(angles);
				_slopeAngle = Mathf.Min(angles);
			}
			else
			{
				// 2 collision points (sphere and first raycast): MINIMUM the two
				_slopeAngle = Mathf.Min(angleForward, middleAngle);
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

	#endregion

	#region character status

	/// <summary>
	/// 	kill the character
	/// </summary>
	private void HandleDeath()
	{
		_rsoPlayerDeath.value = true;
		Destroy(gameObject);
	}

	/// <summary>
	/// 	set the character as stunned for the stun timer duration,
	/// 	then set the character as slowed.
	/// </summary>
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

	/// <summary>
	/// 	set the character as slowed for the slow timer duration.
	/// </summary>
	private void HandleSlow()
	{
		if (!_isSlowed) return;

		_slowTimer -= Time.deltaTime;
		_isSlowed = _slowTimer > 0;
	}

	#endregion

	#region movement

	/// <summary>
	/// 	lerp the current speed to the target speed with several modifiers: 
	/// 	(1) slope acceleration or deceleration,
	/// 	(2) slow status, 
	/// 	(3) stun status,
	/// 	(4) player's input magnitude - stops the character if the player don't command it to
	/// </summary>
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

	/// <summary>
	/// 	handle gravity modifier, and jump delay
	/// </summary>
	private void ApplyGravity()
	{
		if (_isGrounded)
		{
			// stop our velocity dropping infinitely when grounded
			if (_gravityModifier.y < 0.0f) _gravityModifier.y = -2.0f;

			// runs prevent jump timer
			if (_jumpTimer >= 0.0f) _jumpTimer -= Time.deltaTime;
			else _inAir = false;

			// reset the coyote timer
			_coyoteTimer = _characterConfig.coyoteTime;
		}
		else 
		{
			// runs the coyote timer
			if (_coyoteTimer >= 0.0f) _coyoteTimer -= Time.deltaTime;

			// reset the jump delay timer
			_jumpTimer = _characterConfig.jumpCooldown;

			// set the character as in the air
			_inAir = true;
		}

		// apply gravity over time if under terminal
		// multiply by delta time twice to linearly speed up over time
		if (_gravityModifier.y < _TERMINAL_VELOCITY)
		{
			_gravityModifier.y += _characterConfig.gravity * Time.deltaTime;
		}
	}

	/// <summary>
	/// 	moves the character towards the input directions 
	/// </summary>
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
		if (_rsoCharacterPosition.value != _characterDirection.position) { _rsoCharacterPosition.value = _characterDirection.position; }
		if (_rsoCharacterForward.value != _characterDirection.forward) { _rsoCharacterForward.value = _characterDirection.forward; }
    }

	#endregion

	#region inputs

	private Rope _lastInstantiatedRope;
	private Rope _equippedRope;

	/// <summary>
	/// 	temporary function to handle ladder and rope placement.
	/// </summary>
	private void HandleInputs()
	{
		// TODO - whenever one of the crafting input are pressed, switch to craft state
		// reduce the movement, switch to aim camera, disable sprinting, disable jumping
		// regroup preladder with prerope to make one modular component that instantiate
		// either rope or ladder based on player's input

		// - ladder -
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			_currentPreLadder = Instantiate(_ladderConfig.pfPreLadder);
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

		// - rope -
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			_currentPreRope = Instantiate(_ropeConfig.pfPreRope);
		}

		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			_lastInstantiatedRope = _currentPreRope.InstantiateRope();
			if (_currentPreRope != null)
			{
				Destroy(_currentPreRope.gameObject);
				_currentPreRope = null;
			}
		}

		if (Input.GetKeyDown(KeyCode.K)
			&& _lastInstantiatedRope != null)
		{
			_lastInstantiatedRope.Interact(this);
			_equippedRope = _lastInstantiatedRope;
		}

		if (Input.GetKeyDown(KeyCode.L)
			&& _equippedRope != null)
		{
			_equippedRope.Cancel();
			_equippedRope = null;
		}
	}

	/// <summary>
	/// 	update the movement input when pressed
	/// </summary>
	/// <param name="input">input direction value</param>
	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private bool _isJumping;

	/// <summary>
	/// 	add vertical velocity to the gravity modifier to make it jump
	/// </summary>
	private void Jump()
	{
		// exit, if the character is already jumping
		if (_isJumping) 
		{
			return;
		}

		// exit, if the coyote time is exhaused 
		// or character is grounded but the jump delay is not over
		if ((_coyoteTimer <= 0.0f || _isGrounded)
			&& (!_isGrounded || _jumpTimer >= 0.0f))
		{
			return;
		}
		
		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_gravityModifier.y = Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity);

		_isJumping = true;
	}

	/// <summary>
	/// 	update the sprint input value
	/// </summary>
	/// <param name="isSprinting">is the input pressed</param>
	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

	/// <summary>
	/// 	(temp) throw the torch towards the camera.
	/// 	this will change as soon of throw and craft component are ready to use.
	/// </summary>
	private void Throw()
	{
		if (_currentTorch == null) return;

		Ray ray = new Ray(_cameraDirection.position, _cameraDirection.forward);
		Vector3 direction = ray.GetPoint(1) - ray.GetPoint(0);
		_currentTorch?.Throw(direction);

		_currentTorch = null;
	}

	/// <summary>
	/// 	lit and unlit the currently equipped torch
	/// </summary>
	private void ToggleLight()
	{
		_currentTorch?.ToggleLight();
	}

	private void CancelAction()
	{

	}


	#endregion

	/// <summary>
	/// 	instantiate the torch prefab after the fixed duration.
	/// </summary>
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

	#region locomotion state

	private void EnterLocomotionState()
	{

	}

	private void UpdateLocomotionState()
	{
		// temp
		HandleInputs();

		CheckGround();

		// speed calculations
		HandleSlope();
		HandleStun();
		HandleSlow();
		Accelerate();

		// velocity calculations
		ApplyGravity();
		HandleMovement();

		// HandleRopeLength();
	}

	private void ExitLocomotionState()
	{

	}

	#endregion

	#region jump state

	private void EnterJumpState()
	{

	}

	private void UpdateJumpState()
	{

	}

	private void ExitJumpState()
	{

	}

	#endregion

	#region fall state

	private void EnterFallState()
	{

	}

	private void UpdateFallState()
	{

	}

	private void ExitFallState()
	{

	}

	#endregion

	#region craft state

	private void EnterCraftState()
	{

	}

	private void UpdateCraftState()
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

    /// <summary>
    /// 	start the spawn torch coroutine if (1) there is no torch equiped, (2) another permanent is being crafted
    /// </summary>
    private void CraftTorch()
    {
        if (_currentTorch == null
            && !_isCrafting)
        {
            StartCoroutine(SpawnTorch());
        }
    }

    private void CraftRope()
	{

	}

    private void CraftLadder()
    {

    }

    private void ExitCraftState()
	{

	}

	#endregion

	#region rope state

	private void EnterRopeState()
	{

	}

	private void UpdateRopeState()
	{

	}

	private void HandleRopeLength()
	{
		// exit, if there is no rope equipped
		if (_equippedRope == null) return;

		if (Physics.Linecast(_ropeAttach.transform.position, _equippedRope.folds[^1], out var addHit, ~_ropeConfig.foldLayer))
		{
			_equippedRope.folds.Add(addHit.point);
		}

		if (_equippedRope.folds.Count >= 2
			&& !Physics.Linecast(_ropeAttach.transform.position, _equippedRope.folds[^2], out var removeHit, ~_ropeConfig.foldLayer))
		{
			_equippedRope.folds.Remove(_equippedRope.folds[^1]);
		}

		ropeLength = _equippedRope.GetLength();
		if (ropeLength >= _ropeConfig.maxLength)
		{
			// reposition the player within the rope radius
			if (Vector3.Dot(_characterDirection.forward, (_ropeAttach.transform.position - _equippedRope.folds[^1]).normalized) >= 0)
			{
				_velocity = Vector3.zero;
			}
		}
	}

	private void ExitRopeState()
	{

	}

	#endregion

	#region ladder state

	private void EnterLadderState()
	{

	}

	private void UpdateLadderState()
	{

	}

	private void ExitLadderState()
	{

	}

    #endregion

   
	#region interaction

    private void Interact()
    {
		if (_interactables.Count >= 1)
		{
            for (int i = 0; i < _interactables.Count; i++)
            {
                float distance = (_interactables[i].transform.position - this.transform.position).sqrMagnitude;

                if (_nearestInteractible == null)
                {
                    _nearestInteractible = _interactables[i];
                }

                else if (distance < (_nearestInteractible.transform.position - this.transform.position).sqrMagnitude)
                {
                    _nearestInteractible = _interactables[i];
                }
            }
            Debug.Log("Try to interact");
            _nearestInteractible.InteractionTrigger();
        }
    }

    public void AddToInteractList(Interactible _interactibleObject)
    {
		Debug.Log("ajoute");
        _interactables.Add(_interactibleObject);
    }

    public void RemoveFromInteractList(Interactible _interactibleObject)
    {
        _interactables.Remove(_interactibleObject);
    }

    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
	#region exposed variables

	[Header("Internal references")]
	[SerializeField] private Transform _cameraTransform;
	[SerializeField] private Transform _characterDirection;
	[SerializeField] private Transform _handSocket;
	[SerializeField] private Transform _robotHandSocket;
	[SerializeField] private Transform _harness;
	[SerializeField] private CharacterController _controller;

	[Space(5)]
    [Header("External references")]
	[SerializeField] private ThirdPersonCamera _thirdPersonCamera;

    [Space(5)]
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
	[SerializeField] private RSE_ToggleInHand _rseToggleInHand;
	[SerializeField] private RSE_Craft _rseCraft;
	[SerializeField] private RSE_Interact _rseInteract;
    [SerializeField] private RSE_CancelAction _rseCancelAction;
	[SerializeField] private RSE_CanInteract _rseCanInteract;
	[SerializeField] private RSE_Holding _rseHolding;
    [SerializeField] private RSE_ToggleInputs _rseToggleInputs;
	[SerializeField] private RSE_KillCharacter _rseKillCharacter;
	[SerializeField] private RSO_GamePaused _rsoGamePaused;

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
	[ReadOnly] public bool _isJumping;

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

	[Header("debug: craft")]
	[ReadOnly] public Permanent _craftInHand;
    [ReadOnly] public Permanent _craftInRobot;

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

	// - craft -
	private Coroutine _craftCoroutine;

	// ----- CONST -----
	private const float _TERMINAL_VELOCITY = 53.0f;
	private const float FIXED_GRAVITY = -2.0f;

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

    private void LateUpdate()
    {
        LateUpdateCurrentState();
    }

    private void OnEnable()
	{
        _rseToggleInputs.action += ToggleInputs;
        SubscribeInputs();

        // debug
        if (_characterConfig.startWithBag) return;

        _rseCraft.action -= ToggleCraft;
    }

	private void OnDisable()
	{
		UnsubscribeInputs();
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

            case AnimationState.AIM:
                UpdateAimState();
                break;
        }
	}

    /// <summary>
    /// 	call the late update function of the current state.
    /// </summary>
    private void LateUpdateCurrentState()
	{
        switch (_currentState)
        {
            case AnimationState.LOCOMOTION:
                LateUpdateLocomotionState();
                break;

            case AnimationState.JUMP:
                LateUpdateJumpState();
                break;

            case AnimationState.FALL:
                LateUpdateFallState();
                break;

            case AnimationState.CRAFT:
                LateUpdateCraftState();
                break;

            case AnimationState.ROPE:
                LateUpdateRopeState();
                break;

            case AnimationState.LADDER:
                LateUpdateLadderState();
                break;

            case AnimationState.AIM:
                LateUpdateAimState();
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

            case AnimationState.AIM:
                ExitAimState();
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

            case AnimationState.AIM:
                EnterAimState();
                break;
        }

		_currentState = newState;
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
		_isGrounded = Physics.Raycast(origin, Vector3.down, out _groundHit, _characterConfig.groundRaycastLength);

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

		if (Physics.Raycast(originForward, Vector3.down, out var slopeHitForward, _characterConfig.groundRaycastLength))
		{
			UnityEngine.Debug.DrawRay(originForward, Vector3.down, Color.white);

			// get angle of slope on hit normal
			float angleForward = Vector3.Angle(slopeHitForward.normal, Vector3.up);

			Vector3 originBackward =
				transform.position
				+ Vector3.up * _characterConfig.groundCheckY
				- _characterDirection.forward * 0.5f;

			if (Physics.Raycast(originBackward, Vector3.down, out var slopeHitBackward, _characterConfig.groundRaycastLength))
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
	public void HandleDeath()
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
	/// 

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
	/// 	handle gravity modifier, and jump delay.
	/// 	by default the gravity modifier accelerate over time.
	///		if we won't it to accelerate (eg. rope suspension gravity) set the param to false.
	/// </summary>
	private void ApplyGravity(bool doAccelerate = true)
	{
		if (_isGrounded)
		{
			// stop our velocity dropping infinitely when grounded
			if (_gravityModifier.y < 0.0f) _gravityModifier.y = FIXED_GRAVITY;

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
			if (doAccelerate)
			{
				// cumulative gravity acceleration
				_gravityModifier.y += _characterConfig.gravity * Time.deltaTime;
			}
			else 
			{
				// lerp towards the constant gravity modifier
				_gravityModifier.y = Mathf.Lerp(_gravityModifier.y, FIXED_GRAVITY, Time.deltaTime);
			}
		}
	}

	/// <summary>
	/// 	moves the character towards the input directions 
	/// </summary>
	private void HandleMovement()
	{
		// - variables -
		Vector3 direction = _cameraTransform.forward * _moveInput.y + _cameraTransform.right * _moveInput.x;

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

    /// <summary>
    /// 	add character behavior to player inputs
    /// </summary>
    private void SubscribeInputs()
    {
        _rseMove.action += Move;
        _rseJump.action += Jump;
        _rseSprint.action += Sprint;
        _rseThrow.action += ToggleAim;
        _rseCraft.action += ToggleCraft;
        _rseToggleInHand.action += ToggleInHand;
        _rseCancelAction.action += CancelAction;
        _rseInteract.action += Interact;
		_rseKillCharacter.action += HandleDeath;
		_rseHolding.action += Holding;
    }

    /// <summary>
    /// 	remove character behavior from player inputs
    /// </summary>
    private void UnsubscribeInputs()
    {
        _rseMove.action -= Move;
        _rseJump.action -= Jump;
        _rseSprint.action -= Sprint;
        _rseThrow.action -= ToggleAim;
        _rseCraft.action -= ToggleCraft;
        _rseToggleInHand.action -= ToggleInHand;
        _rseCancelAction.action -= CancelAction;
        _rseInteract.action -= Interact;
		_rseKillCharacter.action -= HandleDeath;
		_rseHolding.action -= Holding;
    }

	public void ToggleCraftInput(bool isActive)
	{
		if(isActive) _rseCraft.action += ToggleCraft;
		else _rseCraft.action -= ToggleCraft;
	}

	private void ToggleInputs()
	{
		if (_rsoGamePaused.value)
		{
			CancelAction();
			Sprint(false);
			UnsubscribeInputs();
		}
		else
		{
			SubscribeInputs();
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
	/// 	update the sprint input value.
	/// </summary>
	/// <param name="isSprinting">is the input pressed</param>
	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

	/// <summary>
	/// 	update the holding rope input value.
	/// </summary>
	/// <param name="isHolding">is the input pressed</param>
	private void Holding(bool isHolding)
	{
		if (_rope == null)
		{
			_isHolding = false;
			return;
		}

		_isHolding = isHolding;

		// handle both hold methods
		switch (_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				if (_isHolding)
				{
					_rope.UpdateHoldLength();
				}
				else
				{
					// reset the gravity velocity
					_gravityModifier = Vector3.zero;
				}
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				if (_isHolding)
				{
					// reset the gravity velocity
					_gravityModifier = Vector3.zero;
				}
				else
				{
					_rope.UpdateHoldLength();
				}
				break;
		}
	}

	/// <summary>
	/// 	lit and unlit the currently equipped torch
	/// </summary>
	private void ToggleInHand()
	{
		_craftInHand?.ToggleInHand();
	}

	private void CancelAction()
	{
		// the cancel action is contextual
		// do various things based on the context

		// rope context
		if (_rope != null)
		{
			_rope.Detach();
			_rope = null;
			_isHolding = false;
		}
	}

	#endregion

	#region locomotion state

	private void EnterLocomotionState()
	{

	}

	private void UpdateLocomotionState()
	{
		// checks
		CheckGround();

		// speed calculations
		HandleSlope();
		HandleStun();
		HandleSlow();
		Accelerate();

		// velocity calculations
		ApplyGravity();
		HandleMovement();

		// exit locomotion state
		if (_rope != null 
			&& _rope.isPlaced
			&& _rope.isConnected)
		{
			SwitchState(AnimationState.ROPE);
		}
	}

	private void LateUpdateLocomotionState()
	{

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

	private void LateUpdateJumpState()
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

	private void LateUpdateFallState()
	{

	}


    private void ExitFallState()
	{

	}

    #endregion

    #region craft state

    private void ToggleCraft(CraftType _craftName, bool _isInputPressed)
    {
        //Prevent switching to craft state if not in locomotion or crafting state or already crafting another item
        if ((_currentState != AnimationState.LOCOMOTION && _currentState != AnimationState.CRAFT) || _craftCoroutine != null)
        {
            return;
        }

        //If craft button is pressed
        if (_isInputPressed)
        {
            SwitchState(AnimationState.CRAFT);

            switch (_craftName)
            {
                case CraftType.None:
                    SwitchState(AnimationState.LOCOMOTION);
                    break;

                case CraftType.Torch:
                    if (_craftInHand != null)
                    {
                        if (_craftInHand._craftType != CraftType.Torch && _craftInRobot?._craftType != CraftType.Torch)
                        {
                            Destroy(_craftInHand.gameObject);
                            _craftCoroutine = StartCoroutine(Craft(CraftType.Torch, _torchConfig.craftingDuration));
                        }
                    }
                    else
                    {
                        _craftCoroutine = StartCoroutine(Craft(CraftType.Torch, _torchConfig.craftingDuration));
                    }
                    break;

                case CraftType.Ladder:
                    if (_craftInHand != null)
                    {
                        if (_craftInHand._craftType == CraftType.Torch)
                        {
							_craftInHand.transform.SetParent(_robotHandSocket, false);
							_craftInRobot = _craftInHand;
                            _craftInRobot.transform.rotation = _robotHandSocket.rotation;
                            _craftInHand = null;
                            _craftCoroutine = StartCoroutine(Craft(CraftType.Ladder, _torchConfig.craftingDuration));
                        }
						else if (_craftInHand._craftType != CraftType.Ladder)
						{
                            Destroy(_craftInHand.gameObject);
                            _craftCoroutine = StartCoroutine(Craft(CraftType.Ladder, _torchConfig.craftingDuration));
                        }
                    }
                    else
                    {
                        _craftCoroutine = StartCoroutine(Craft(CraftType.Ladder, _torchConfig.craftingDuration));
                    }
                    break;

                case CraftType.Rope:
					if (_craftInHand != null)
					{
						if (_craftInHand._craftType == CraftType.Torch)
						{
							_craftInHand.transform.SetParent(_robotHandSocket, false);
							_craftInRobot = _craftInHand;
							_craftInHand = null;
							_craftCoroutine = StartCoroutine(Craft(CraftType.Rope, _ropeConfig.craftingDuration));
						}
						else if (_craftInHand._craftType != CraftType.Rope)
						{
							Destroy(_craftInHand.gameObject);
							_craftCoroutine = StartCoroutine(Craft(CraftType.Rope, _ropeConfig.craftingDuration));
						}
					}
					else
					{
						_craftCoroutine = StartCoroutine(Craft(CraftType.Rope, _ropeConfig.craftingDuration));
					}
					break;
            }
        }
        else // if craft button is released
        {
            if (_craftCoroutine != null)
            {
                StopCoroutine(_craftCoroutine);
				_craftCoroutine = null;
            }
            if (_currentState == AnimationState.CRAFT)
            {
                SwitchState(AnimationState.LOCOMOTION);
            }
        }
    }

	private void EnterCraftState()
	{

	}

	private void UpdateCraftState()
	{
		CheckGround();
		HandleSlope();
		HandleStun();
		HandleSlow();
		Accelerate();
		ApplyGravity();
		HandleMovement();
	}
	
	private void LateUpdateCraftState()
	{

	}

    /// <summary>
    /// 	instantiate the torch prefab after the fixed duration.
    /// </summary>
    private IEnumerator Craft(CraftType _objectToCraft, float _craftDuration)
    {
        // wait the crafting duration
        yield return new WaitForSeconds(_craftDuration);

		// instantiate the crafted object
		switch (_objectToCraft)
		{
            case CraftType.None:
                break;

            case CraftType.Torch:
                _craftInHand = Instantiate(_torchConfig.pfTorch, _handSocket.transform);
                break;

			case CraftType.Ladder:
				_craftInHand = Instantiate(_ladderConfig.PF_Ladder, _handSocket.transform);
                break;

			case CraftType.Rope:
				_craftInHand = Instantiate(_ropeConfig.pfRope, _handSocket.transform);
				break;
		}

		_craftInHand.transform.position = _handSocket.transform.position;

		_craftCoroutine = null;
        SwitchState(AnimationState.LOCOMOTION);
    }

    private void ExitCraftState()
	{

	}

	public enum CraftType
	{
		None,
		Torch,
		Ladder,
		Rope,
	}

    #endregion

    #region rope state

	#region variables

	public enum RopeState
	{
		GROUNDED = 0,
		PARTIAL_SUSPENSION,
		COMPLETE_SUSPENSION,
	}

	[Header("debug: rope")]
	[ReadOnly] public RopeState _ropeState;
	[ReadOnly] public bool _isHolding;
	[ReadOnly] public bool _isAgainstWall;

	private Rope _rope;

	#endregion

	#region animation-state-related functions

	private void EnterRopeState()
	{

	}

	private void UpdateRopeState()
	{
		// checks
		CheckGround();
		CheckWall();

		// exit rope state
		if (_rope == null
			|| _rope != null && !_rope.isConnected)
		{
			SwitchState(AnimationState.LOCOMOTION);
			return;
		}

		// state machine update rope state
		HandleRopeState();
		switch (_ropeState)
		{
			case RopeState.GROUNDED:
				UpdateRopeGroundedState();
				break;

			case RopeState.PARTIAL_SUSPENSION:
				UpdateRopePartialSuspensionState();
				break;

			case RopeState.COMPLETE_SUSPENSION:
				UpdateRopeCompleteSuspensionState();
				break;
		}

		// apply rope holding constraint after the input movements
		// this allow to avoid glitchy movements
		HandleRopeHolding();
	}

	private void LateUpdateRopeState()
	{

	}

	private void ExitRopeState()
	{
		_rope = null;
	}

	#endregion

	#region rope-state-related functions

	/// <summary>
	/// 	update the current rope state to match the last checks.
	/// </summary>
	private void HandleRopeState()
	{
		if (_isGrounded)
		{
			_ropeState = RopeState.GROUNDED;
		}
		else
		{
			if (_isAgainstWall)
			{
				_ropeState = RopeState.PARTIAL_SUSPENSION;
			}
			else
			{
				_ropeState = RopeState.COMPLETE_SUSPENSION;
			}
		}
	}

	/// <summary>
	/// 	same as the base locomotion update with the rope limitation extra-layer.
	/// 	this state changes if the characters is no more grounded but still attach to a rope.
	/// </summary>
	private void UpdateRopeGroundedState()
	{
		// speed calculations
		HandleSlope();
		HandleStun();
		HandleSlow();
		Accelerate();

		// velocity calculations
		ApplyGravity();
		HandleMovement();
	}

	/// <summary>
	/// 	handle movement related to the front wall. 
	/// 	left / right, jump, go down the rope movement.
	/// 	jumping and falling off the wall on an edge, change from partial to complete suspension state.
	/// 	touching the ground, change from partial to grounded state.
	/// </summary>
	private void UpdateRopePartialSuspensionState()
	{
		HandleRopeMovement();
		FaceFoldCenter();
	}

	/// <summary>
	/// 	handle movement in the void suspended to the rope. 
	/// 	left / right, forward / backward, go down the rope movement.
	/// 	gain support against a wall, change from complete to partial suspension state.
	/// 	touching the ground, change from complete to grounded state.
	/// </summary>
	private void UpdateRopeCompleteSuspensionState()
	{
		HandleRopeMovement();
		FaceFoldCenter();
	}

	#endregion

	#region rope functions

	/// <summary>
	/// 	check if there is a collider in front of the character using a raycast.
	/// </summary>
	private void CheckWall()
	{
		_isAgainstWall = Physics.SphereCast(
			_controller.center,
			_controller.height / 2f,
			_characterDirection.forward,
			out var hitInfo,
			_controller.height / 2f + 0.05f,
			_characterConfig.againstWallLayerToInclude
		);

		if (_isAgainstWall)
		{
			Debug.Log("CHARACTER_MOTOR: wall touched");
			Vector3 hitPoint = new Vector3(hitInfo.point.x, transform.position.y, hitInfo.point.z);
			Vector3 touchedDirection = hitPoint - transform.position;
			_characterDirection.forward = touchedDirection.normalized;
		}
	}

	/// <summary>
	/// 	add spherical locomotion constraint to the character movement. 
	/// </summary>
	private void HandleRopeHolding()
	{
		// assert: there is no equipped rope 
		if (_rope == null) return;

		// assert: holding input method
		switch (_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				// while hold to stop, we don't constraint the character if the player IS NOT holding the button
				if (!_isHolding) return;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				// while hold to let go, we don't constraint the character if the player IS holding the button
				if (_isHolding) return;
				break;
		}

		// get the distance between the current character's position and the position of the last fold
		Vector3 towardCharacter = transform.position - _rope.folds[^1];

		// re-snap the character's position within the spherical constraint
		if (towardCharacter.magnitude > _rope.holdLength)
		{
			transform.position = _rope.folds[^1] + towardCharacter.normalized * _rope.holdLength;

			// transform position of the character controller has been modified outside the movement function
			// call this unity function to synchronize transform to avoid glitchy movement effects
			Physics.SyncTransforms();
		}
	}

	/// <summary>
	/// 	make the character facing center of the last fold (not with the y-axis).
	/// </summary>
	private void FaceFoldCenter()
	{
		// assert: holding input method
		switch (_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				// while hold to stop, we don't constraint the character if the player IS NOT holding the button
				if (!_isHolding) return;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				// while hold to let go, we don't constraint the character if the player IS holding the button
				if (_isHolding) return;
				break;
		}

		// assert: character is touching a wall
		if (_isAgainstWall) return;
 
		// assert: center-character distance is greater than the threshold 
		if (Mathf.Abs(_rope.holdLength - (_rope.folds[^1] - transform.position).magnitude) > _characterConfig.facingCenterThreshold) return;

		Vector3 towardsCenter = transform.position - new Vector3(_rope.folds[^1].x, transform.position.y, _rope.folds[^1].z);
		_characterDirection.forward = -towardsCenter.normalized;
	}

	private void HandleRopeMovement()
	{
		// assert: there is no equipped rope 
		if (_rope == null) return;

		// player direction inputs
		Vector3 inputDirectionGrounded = _cameraTransform.forward * _moveInput.y + _cameraTransform.right * _moveInput.x;
		Vector3 inputDirectionWall = _characterDirection.right * _moveInput.x;

		// do the character fall based on the rope holding method
		bool doFall = false;
		switch (_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				if (_isHolding) doFall = false;
				else doFall = true;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				if (_isHolding) doFall = true;
				else doFall = false;
				break;
		}

		// - character is not holding the rope -

		if (doFall)
		{	
			// apply grounded and falling like forces
			ApplyGravity();

			// move the character with air control scalar
			_controller.Move(Time.deltaTime * (
				// last ground direction and speed to keep the inertia going on
				_lastGroundedDirection.normalized * _lastGroundedSpeed
				// current direction and speed reduced by the air control modifier to slightly moves while in air
				+ inputDirectionGrounded * _targetSpeed * _characterConfig.airControlModifier
				+ _gravityModifier
			));

			return;
		}

		// - character is holding the rope -

		// from the attraction point (which is the lowest point on the sphere)
		// get the normalized direction towards this point with a down offset
		// so the direction is more likely to be tangent to the sphere
		Vector3 attractionPoint = _rope.folds[^1] + Vector3.down * _rope.holdLength;
		Vector3 sphereCharaSnapPoint = _rope.folds[^1] + (_rope.folds[^1] - _rsoCharacterPosition.value).normalized * _rope.holdLength;
		Vector3 offsetAttractionPoint = _rope.folds[^1] + Vector3.down * (attractionPoint - sphereCharaSnapPoint).magnitude;
		Vector3 attractionDirection = (offsetAttractionPoint - sphereCharaSnapPoint).normalized;

		// TODO:
		// (1) acceleration movement while against the wall
		// (2) reduce the character speed to 0 when approching the limit angles of the balancier effect
		// (3) jump off the wall logic
		// (4) lerp the speed acceleration when starting going down the rope 

		// apply movements
		// partial rope suspension
		if (_isAgainstWall)
		{
			_controller.Move(Time.deltaTime * (
				// player's inputs
				inputDirectionGrounded * _characterConfig.partialSuspensionSpeed
				// attraction direction is a custom gravity force applied while on the rope
				+ attractionDirection * _characterConfig.partialSphericalAttractiveForce
			));
		}

		// complete rope suspension
		else
		{
			_controller.Move(Time.deltaTime * (
				// player's inputs
				inputDirectionGrounded * _characterConfig.completeSuspensionSpeed
				// attraction direction is a custom gravity force applied while on the rope
				+ attractionDirection * _characterConfig.partialSphericalAttractiveForce
			));
		}

		// update variables
		if (_rsoCharacterPosition.value != _characterDirection.position) { _rsoCharacterPosition.value = _characterDirection.position; }
		if (_rsoCharacterForward.value != _characterDirection.forward) { _rsoCharacterForward.value = _characterDirection.forward; }
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		// assert: rope ref is null
		if (_rope is null) return;

		// assert: rope isn't placed yet
		if (!_rope.isPlaced) return;

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(_rope.folds[^1], _rope.holdLength);
	}
#endif

	#endregion

	#endregion

	#region ladder state

	private void EnterLadderState()
	{

	}

	private void UpdateLadderState()
	{

    }

	private void LateUpdateLadderState()
	{

	}


    private void ExitLadderState()
	{

	}

    #endregion

    #region aim state

	private void ToggleAim(bool _isPressed)
	{
        //Prevent switching to aim state if not in locomotion or no craft in hand
        if ((_currentState != AnimationState.LOCOMOTION && _currentState != AnimationState.AIM) || _craftInHand == null)
        {
            return;
        }

        if (_isPressed)
		{
			SwitchState(AnimationState.AIM);
            _thirdPersonCamera.SwitchCameraStyle(CameraStyle.AIMING);
			_craftInHand.InitializePreview();
        }
		else
		{
            if (_currentState == AnimationState.AIM)
			{
                if (_craftInHand.Throw(_thirdPersonCamera.transform))
				{
					// rope attachment exception
					if ((Rope)_craftInHand != null) _rope = (Rope)_craftInHand;
					_rope?.Attach(_harness);

					_craftInHand = null;
					if (_craftInRobot != null)
					{
                        _craftInRobot.transform.SetParent(_handSocket, false);
						_craftInHand = _craftInRobot;
                        _craftInHand.transform.rotation = _handSocket.transform.rotation;
                        _craftInRobot = null;
                    }
                }

                _thirdPersonCamera.SwitchCameraStyle(CameraStyle.BASIC);
                SwitchState(AnimationState.LOCOMOTION);
            }
        }
	}

    private void EnterAimState()
    {

    }

    private void UpdateAimState()
    {
        CheckGround();
        HandleSlope();
        HandleStun();
        HandleSlow();
        Accelerate();
        ApplyGravity();
        HandleMovement();
    }

	private void LateUpdateAimState()
	{
        _craftInHand.PreviewThrow(_thirdPersonCamera.transform);
    }

    private void ExitAimState()
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
            _nearestInteractible.InteractionTrigger();
        }
    }

    public void AddToInteractList(Interactible _interactibleObject)
    {
        _interactables.Add(_interactibleObject);
		CheckShowInteract();
    }

    public void RemoveFromInteractList(Interactible _interactibleObject)
    {
        _interactables.Remove(_interactibleObject);
		CheckShowInteract();
    }

	private void CheckShowInteract()
	{
		_rseCanInteract.Call(_interactables.Count > 0);
    }

	#endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
	[SerializeField] private RSE_Run _rseRun;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Throw _rseThrow;
	[SerializeField] private RSE_ToggleInHand _rseToggleInHand;
	[SerializeField] private RSE_Craft _rseCraft;
	[SerializeField] private RSE_Interact _rseInteract;
    [SerializeField] private RSE_CancelAction _rseCancelAction;
	[SerializeField] private RSE_CanInteract _rseCanInteract;
	[SerializeField] private RSE_CanRecycle _rseCanRecycle;
	[SerializeField] private RSE_Holding _rseHolding;
	[SerializeField] private RSE_Recycle _rseRecycle;
    [SerializeField] private RSE_ToggleInputs _rseToggleInputs;
	[SerializeField] private RSE_KillCharacter _rseKillCharacter;
	[SerializeField] private RSO_GamePaused _rsoGamePaused;

	#endregion

	#region runtime variables

    [Header("debug: animation")]
	[HideInInspector] public AnimationState _currentState;

	[Header("debug: move")]
	private Vector2 _moveInput;
    [HideInInspector] public float _planarSpeed;
	private float _targetPlanarSpeed;
	private float _gravitySpeed;
	private Vector3 _movement;
	private bool _isRunning;
	private float _coyoteTime;

	[Header("debug: slope")]
	private float _slopePercentage;

	[Header("debug: fall")]
    [HideInInspector] public bool _isGrounded;
    private bool[] _groundChecks = new bool[5];
	private bool _isStunned = false;
	private bool _isSlowed = false;
	private bool _isSlowedPostStun = false;

	[Header("debug: momentum")]
	private Vector3 _positionStartFall;
	private Vector3 _lastGroundedPlanarForward;
	private float _fallHeight;

	[Header("debug: permanent")]
	[HideInInspector] public bool _hasBackpack;
	[ReadOnly] public float ropeLength;
	[HideInInspector] public Permanent _craftInHand;
    [HideInInspector] public Permanent _craftInRobot;

    // ----- PRIVATE VARIABLES -----
    // - status -
    private float _stunTimer;
	private float _slowTimer;

	// - ground -
	private Vector3 _origin;
    private Vector3 _planarForward;
    private Vector3 _planarRight;
	private bool _isGroundedLastFrame = true;
	private LayerMask _raycastLayerMask;
    private RaycastHit[] _groundHits = new RaycastHit[5];
	private float _discriminantForward;
    private float _discriminantRight;

    // - jump -
    private bool _isJumping;
	private float _jumpTimer;
	private RaycastHit[] _edgeHits;
	private RaycastHit _edgeHit;

    // - interact -
    private List<Interactible> _interactables;
	private List<Interactible> _validInteractibles;

	// - craft -
	private Coroutine _craftCoroutine;

	// ----- CONST -----
	private const float _TERMINAL_VERTICAL_VELOCITY = 53.0f;
	private const float FIXED_GRAVITY = -2.0f;

	#endregion

	#region monobehaviour functions

	private void Start()
    {
        // creation of the interaction list
        _interactables = new List<Interactible>();
        _validInteractibles = new List<Interactible>();

		_raycastLayerMask |= (1 << LayerMask.NameToLayer("Default"));
		_raycastLayerMask |= (1 << LayerMask.NameToLayer("Collision_NoRaycast"));

		SwitchState(AnimationState.LOCOMOTION);
    }

	private void Update()
	{
		CalculateOriginForwardRight();
		CheckGround();
		CheckCoyoteTime();
		UpdateStatus();

		VerifyState();

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
		if (_characterConfig.startWithBag)
		{
            _hasBackpack = true;
			ToggleCraftInput(_hasBackpack);
        }
    }

	private void OnDisable()
	{
		UnsubscribeInputs();
    }

	#if UNITY_EDITOR
	private void OnDrawGizmos()
    {
        if (_characterConfig.showGroundedDebug)
        {
            if (_discriminantForward > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_groundHits[1].point + (_groundHits[2].point - _groundHits[1].point).normalized *
                    (-Vector3.Dot((_groundHits[2].point - _groundHits[1].point).normalized, _groundHits[1].point - _origin) + Mathf.Sqrt(_discriminantForward))
                    , 0.05f);
                Gizmos.DrawSphere(_groundHits[1].point + (_groundHits[2].point - _groundHits[1].point).normalized *
                    (-Vector3.Dot((_groundHits[2].point - _groundHits[1].point).normalized, _groundHits[1].point - _origin) - Mathf.Sqrt(_discriminantForward))
                    , 0.05f);
            }
            if (_discriminantRight > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_groundHits[3].point + (_groundHits[4].point - _groundHits[3].point).normalized *
                    (-Vector3.Dot((_groundHits[4].point - _groundHits[3].point).normalized, _groundHits[3].point - _origin) + Mathf.Sqrt(_discriminantRight))
                    , 0.05f);
                Gizmos.DrawSphere(_groundHits[3].point + (_groundHits[4].point - _groundHits[3].point).normalized *
                    (-Vector3.Dot((_groundHits[4].point - _groundHits[3].point).normalized, _groundHits[3].point - _origin) - Mathf.Sqrt(_discriminantRight))
                    , 0.05f);
            }
        }
		if (_edgeHits != null && (_currentState == AnimationState.FALL || _currentState == AnimationState.JUMP))
		{
            Gizmos.color = Color.cyan;
            foreach (RaycastHit _hit in _edgeHits)
            {
				Gizmos.DrawSphere(_hit.point, 0.05f);
            }
        }

        if (_rope)
        {
            if (_rope.isPlaced)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_rope.folds[^1], _rope.holdLength);
            }
        }
    }
	#endif

	#endregion

	#region animation state switch
	/// <summary>
	/// 	exit current state and enter the given state.
	/// </summary>
	/// <param name="newState">state to enter into</param>
	private void SwitchState(AnimationState newState)
	{
		ExitCurrentState();
		Debug.Log(newState.ToString());
		EnterState(newState);
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

		CheckShowInteract();
		CheckShowRecycle(false);
	}

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
	#endregion

    #region misc

	/// <summary>
	/// 	kill the character
	/// </summary>
    public void HandleDeath()
	{
		_rsoPlayerDeath.value = true;
		Destroy(gameObject);
	}

	public void PickupBackpack()
	{
        _hasBackpack = true;
        ToggleCraftInput(_hasBackpack);
    }

	#endregion

	#region movement

	/// <summary>
	/// 	Determine origin forward and right vectors based on character position, camera and inputs.	
	/// </summary>
	private void CalculateOriginForwardRight()
	{
		_origin = new Vector3(transform.position.x, transform.position.y + _controller.radius, transform.position.z);

        // Check if ground on 5 points align with player inputs or character direction if no inputs
        if (_moveInput != Vector2.zero)
		{
			//Calculate input forward and right on character plane
            _planarForward = (new Vector3(_thirdPersonCamera.transform.forward.x, 0, _thirdPersonCamera.transform.forward.z) * _moveInput.y + new Vector3(_thirdPersonCamera.transform.right.x, 0, _thirdPersonCamera.transform.right.z) * _moveInput.x).normalized;
            _planarRight = new Vector3(-_planarForward.z, 0, _planarForward.x);
        }
		else
		{
            //Calculate character graphic forward and right on character plane
            _planarForward = new Vector3(_characterDirection.forward.x, 0, _characterDirection.forward.z).normalized;
            _planarRight = new Vector3(-_planarForward.z, 0, _planarForward.x);
        }

	}

	/// <summary>
	/// 	Use 5 raycasts to check if the character has a collider below it.
	/// </summary>
	private void CheckGround()
    {
		//Debug Line
		if (_characterConfig.showGroundedDebug)
		{
			UnityEngine.Debug.DrawLine(_origin, new Vector3(_origin.x, _origin.y - _controller.radius * _characterConfig.groundCheckYFactor, _origin.z), Color.red);
			UnityEngine.Debug.DrawLine(_origin + _planarForward * _controller.radius, new Vector3(_origin.x + _planarForward.x * _controller.radius, _origin.y - _controller.radius * _characterConfig.groundCheckYFactor, _origin.z + _planarForward.z * _controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(_origin - _planarForward * _controller.radius, new Vector3(_origin.x - _planarForward.x * _controller.radius, _origin.y - _controller.radius * _characterConfig.groundCheckYFactor, _origin.z - _planarForward.z * _controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(_origin + _planarRight * _controller.radius, new Vector3(_origin.x + _planarRight.x * _controller.radius, _origin.y - _controller.radius * _characterConfig.groundCheckYFactor, _origin.z + _planarRight.z * _controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(_origin - _planarRight * _controller.radius, new Vector3(_origin.x - _planarRight.x * _controller.radius, _origin.y - _controller.radius * _characterConfig.groundCheckYFactor, _origin.z - _planarRight.z * _controller.radius), Color.red);
		}

        //Raycast
        _groundChecks[0] = Physics.Raycast(_origin, Vector3.down, out _groundHits[0], _controller.radius * _characterConfig.groundCheckYFactor, _raycastLayerMask);
        _groundChecks[1] = Physics.Raycast(_origin + _planarForward * _controller.radius, Vector3.down, out _groundHits[1], _controller.radius * _characterConfig.groundCheckYFactor, _raycastLayerMask);
        _groundChecks[2] = Physics.Raycast(_origin - _planarForward * _controller.radius, Vector3.down, out _groundHits[2], _controller.radius * _characterConfig.groundCheckYFactor, _raycastLayerMask);
        _groundChecks[3] = Physics.Raycast(_origin + _planarRight * _controller.radius, Vector3.down, out _groundHits[3], _controller.radius * _characterConfig.groundCheckYFactor, _raycastLayerMask);
        _groundChecks[4] = Physics.Raycast(_origin - _planarRight * _controller.radius, Vector3.down, out _groundHits[4], _controller.radius * _characterConfig.groundCheckYFactor, _raycastLayerMask);

		_isGroundedLastFrame = _isGrounded;
		_isGrounded = false;
		_discriminantForward = -1f;
		_discriminantRight = -1f;

		//Check if raycast directly below the character hit a surface near enough to consider grounded
		if (_groundChecks[0])
		{
			if ((_groundHits[0].point - _origin).magnitude <= _controller.radius + _characterConfig.skinWidth)
			{
				_isGrounded = true;
			}
		}
		//Prevent unnecessary check if we already know the character is grounded
		if (!_isGrounded)
		{
            //Check if the raycast hits Forward/Backward make a line that cross player capsule+skin, which mean the player is grounded
            if (_groundChecks[1] && _groundChecks[2])
            {
                //Debug Line
                if (_characterConfig.showGroundedDebug)
                {
                    UnityEngine.Debug.DrawLine(_groundHits[1].point, _groundHits[2].point, Color.yellow);
                }

                //discriminant of the equation between the sphere (centered on _origin and radius of _controller.radius+skinWidth) and the line resulting of the hits of the raycasts
                _discriminantForward = Mathf.Pow(Vector3.Dot((_groundHits[2].point - _groundHits[1].point).normalized, _groundHits[1].point - _origin), 2) - ((_groundHits[1].point - _origin).sqrMagnitude - Mathf.Pow(_controller.radius + _characterConfig.skinWidth, 2));
                //discriminant > 0 means that the line cross the sphere in at least 2 points (no tangent)
                if (_discriminantForward > 0)
                {
                    _isGrounded = true;
                }
            }
            //Check if the raycast hits Right/Left make a line that cross player capsule+skin, which mean the player is grounded
            if (_groundChecks[3] && _groundChecks[4])
            {
                //Debug Line
                if (_characterConfig.showGroundedDebug)
                {
                    UnityEngine.Debug.DrawLine(_groundHits[3].point, _groundHits[4].point, Color.yellow);
                }
                //discriminant of the equation between the sphere (centered on _origin and radius of _controller.radius+skinWidth) and the line resulting of the hits of the raycasts
                _discriminantRight = Mathf.Pow(Vector3.Dot((_groundHits[4].point - _groundHits[3].point).normalized, _groundHits[3].point - _origin), 2) - ((_groundHits[3].point - _origin).sqrMagnitude - Mathf.Pow(_controller.radius + _characterConfig.skinWidth, 2));
                //discriminant > 0 means that the line cross the sphere in at least 2 points (no tangent)
                if (_discriminantRight > 0)
                {
                    _isGrounded = true;
                }
            }
        }
    }

    /// <summary>
    /// 	Check variable of player to determine new player state.
    /// </summary>
    private void VerifyState()
	{
		if (_isJumping && (_isGrounded || _coyoteTime > 0f) && _currentState != AnimationState.JUMP)
        {
            SwitchState(AnimationState.JUMP);
        }
        else if (!_isGrounded && _currentState != AnimationState.FALL && _gravitySpeed <= 0)
		{
			if(_isGroundedLastFrame) _coyoteTime = _characterConfig.coyoteTime; 
            SwitchState(AnimationState.FALL);
        }
		else if (_isGrounded && !_isGroundedLastFrame)
		{
			ApplyFallHeight();
            SwitchState(AnimationState.LOCOMOTION); 
		}

		//Reset Jump if it is not possible to jump
		_isJumping = false;
	}

	/// <summary>
	///		Check fall height and kill/stun/slow player if necessary
	/// </summary>
	private void ApplyFallHeight()
	{
        _fallHeight = Math.Abs(transform.position.y - _positionStartFall.y);
        if (_fallHeight >= _characterConfig.lethalHeight)
        {
            HandleDeath();
        }
        else if (_fallHeight >= _characterConfig.stunHeight)
        {
            // stun the character for x secondes
            _isStunned = true;

            // cross product to get the stun mitiged value on a 0-1 scale
            float stunMitigedValue = (_fallHeight - _characterConfig.stunHeight) / (_characterConfig.lethalHeight - _characterConfig.stunHeight);
            _stunTimer = _characterConfig.stunDuration.Evaluate(stunMitigedValue);
        }
        else if (_fallHeight >= _characterConfig.slowHeight)
        {
            // slow the character for x secondes by y percent
            _isSlowed = true;

            // cross product to get the slow mitiged value on a 0-1 scale
            float slowMitigedValue = (_fallHeight - _characterConfig.slowHeight) / (_characterConfig.stunHeight - _characterConfig.slowHeight);
            _slowTimer = _characterConfig.slowDuration.Evaluate(slowMitigedValue);
        }
    }

	/// <summary>
	/// 	Update coyote time
	/// </summary>
	private void CheckCoyoteTime()
	{
		if (_coyoteTime > 0f)
		{
			_coyoteTime -= Time.deltaTime;
		}
	}

	/// <summary>
	/// 	Set _targetSpeed based on player running input.
	/// </summary>
	private void CheckWalkRun()
	{
		if(_isRunning)
		{
			_targetPlanarSpeed = _characterConfig.runSpeed;
		}
		else
		{
			_targetPlanarSpeed = _characterConfig.walkSpeed;
		}
	}

	/// <summary>
	/// 	Set the slope angle to the mean angle value amoung 5 raycasts.
	/// 	Get the slope deceleration or acceleration percentage based on the slope angle.
	/// </summary>
	private void ApplySlope()
	{
		if (!_isGrounded) return;

		Vector3 _hitsNormalSum = Vector3.zero;
		int _hitCount = 0;

		for (int _indexHit = 0; _indexHit < 5; _indexHit++)
		{
			if (_groundChecks[_indexHit])
			{
                _hitsNormalSum += _groundHits[_indexHit].normal;
				_hitCount++;
            }
		}

        // get the slope percentage to calculate slows later in the movement function
        _slopePercentage = Vector3.Angle(_hitsNormalSum/_hitCount, Vector3.up) / _controller.slopeLimit;

		// signed and scaled percent based on player input direction and mean normal
		_slopePercentage *= -Vector3.Dot(new Vector3((_hitsNormalSum/_hitCount).x, 0, (_hitsNormalSum/_hitCount).z).normalized, _planarForward)*2;

		_targetPlanarSpeed *= _characterConfig.slopeSpeedModifier.Evaluate(_slopePercentage);
	}

	/// <summary>
	/// 	Increase _planarSpeed by minimal jup speed and clamp it to max walk/run speed 
	/// </summary>
	private void ApplyJumpImpulsePlanarSpeed()
	{
        if (_isRunning)
		{
			_planarSpeed = Mathf.Clamp(_planarSpeed+_characterConfig.jumpMinimalPlanarVelocity, 0, _characterConfig.runSpeed);
		}
		else
		{
			_planarSpeed = Mathf.Clamp(_planarSpeed+_characterConfig.jumpMinimalPlanarVelocity, 0, _characterConfig.walkSpeed);
		}
	}

    /// <summary>
    /// 	Multiply target speed by input magnitude.
    /// </summary>
	private void ApplyInputs()
	{
        //multiply by input magnitude
        _targetPlanarSpeed *= Mathf.Clamp(_moveInput.magnitude, 0, 1);
        // TO DO: remap input magnitude from 0:1 to deadzone:1
    }

    /// <summary>
    /// 	Add acceleration or decceleration and clamp it.
    /// </summary>
    private void ApplyAcceleration()
	{
		// accelerate or decelerate to target speed
		if (_planarSpeed <= _targetPlanarSpeed)
		{
			_planarSpeed = Mathf.Clamp(_planarSpeed + _characterConfig.groundAcceleration*Time.deltaTime, 0, _targetPlanarSpeed);
		}
		else
		{
			_planarSpeed = Mathf.Clamp(_planarSpeed - _characterConfig.groundDecceleration*Time.deltaTime, _targetPlanarSpeed, _characterConfig.runSpeed);
		}
	}

    /// <summary>
    /// 	Calculate _movement with _planarSpeed and _planarForward.
    /// </summary>
    private void CreateMovement()
	{
        //calculate _movement to apply to CharacterController
        _movement = _planarSpeed * _planarForward;
    }

    /// <summary>
    /// 	Add positive vertical speed to make character jump
    /// </summary>
    private void ApplyJumpImpulseVerticalSpeed()
	{
        _gravitySpeed = Mathf.Sqrt(_characterConfig.jumpHeight * -3f * _characterConfig.gravity) + _characterConfig.gravity * Time.deltaTime;
	}

	/// <summary>
	///  Call to update timer and status without applying movement modif
	/// </summary>
	private void UpdateStatus()
	{
		if(_isStunned)
		{
			_stunTimer -= Time.deltaTime;

			if (_stunTimer <= 0)
			{
				_isStunned = false;
				_isSlowed = true;
				_isSlowedPostStun = true;
				_slowTimer = _characterConfig.slowTimePostStun;
			}
		}
		if(_isSlowed)
		{
			_slowTimer -= Time.deltaTime;
			
			if (_slowTimer <= 0)
			{
				_isSlowed = false;
				_isSlowedPostStun = false;
			}
		}
	}

	/// <summary>
	/// Call to update timer and status and applying movement modif
	/// </summary>
	private void ApplyStatus()
	{
		if(_isStunned)
		{
			_movement = Vector3.zero;
		}
		else if(_isSlowed)
		{
			if(!_isSlowedPostStun)
			{
				_movement *= _characterConfig.slowPercentage.Evaluate((_characterConfig.maxSlowTime - _slowTimer)/_characterConfig.maxSlowTime);
			}
			else
			{
				_movement *= _characterConfig.slowDuration.Evaluate((_characterConfig.slowTimePostStun - _slowTimer)/_characterConfig.slowTimePostStun);
			}
		}
	}

	/// <summary>
	/// 	Add fake gravity to snap the character to the floor while going down stairs and slopes.
	/// </summary>
	private void ApplySnapGravity()
	{
		_movement = new Vector3(_movement.x, _characterConfig.SnapGravity, _movement.z);
	}

	/// <summary>
	/// 	Allow the player to slighty turn during falling.
	/// </summary>
	private void ApplyAirControl()
	{
		//Angle to add based on time since last frame
		float _airControlAngle = _characterConfig.airControlAngularSpeed * Time.deltaTime;
		//Factor it based on difference between input and character forward
		_airControlAngle *= _characterConfig.airControlInputFactor.Evaluate(Vector3.Dot(_planarForward, _lastGroundedPlanarForward));
		//Sign it
		float _angleInputForward = Vector3.SignedAngle(_lastGroundedPlanarForward, _planarForward, Vector3.up);
		_airControlAngle *= _angleInputForward/Mathf.Abs(_angleInputForward);
		//Apply it to character direction (we use _lastGrounded while in air)
		_lastGroundedPlanarForward = Quaternion.AngleAxis(_airControlAngle, Vector3.up) * _lastGroundedPlanarForward;
	}

	/// <summary>
	/// 	Decrease planar speed while in air, faster if input are not in same direction as fall.
	/// </summary>
	private void ApplyDrag()
	{
		float _inputOrientationFactor = (-Vector3.Dot(_lastGroundedPlanarForward, _planarForward) + 3f) / 4f;
		if (_moveInput == Vector2.zero)
		{
			_inputOrientationFactor = 0.75f;
        }
		_planarSpeed = Mathf.Clamp(_planarSpeed - _characterConfig.dragDecceleration * Time.deltaTime * _inputOrientationFactor, 0, _characterConfig.runSpeed);
	}

    /// <summary>
    /// 	Calculate _movement with _planarSpeed and _lastGroundedPlanarForward.
    /// </summary>
    private void CreateMovementFall()
    {
        _movement = _planarSpeed * _lastGroundedPlanarForward;
    }

    /// <summary>
    /// 	handle gravity modifier, and jump delay
    /// </summary>
    private void ApplyGravity()
	{
		_gravitySpeed += _characterConfig.gravity * Time.deltaTime;

        _movement = new Vector3(_movement.x, _gravitySpeed, _movement.z);
	}

	/// <summary>
	///		Detect edges point while falling or jumping
	/// </summary>
	private void DetectEdges()
	{
		Vector3 _start = new Vector3(transform.position.x, transform.position.y + _controller.height - _controller.radius, transform.position.z);
		float _radius = _controller.radius + _characterConfig.skinWidth;
		float _distance = _controller.height - 2 * _controller.radius;
        _edgeHits = Physics.SphereCastAll(_start, _radius, Vector3.down, _distance, _raycastLayerMask);

		if (_edgeHits.Length > 0 )
		{
            _edgeHit = _edgeHits[0];
        }
		else
		{
            _edgeHit = new RaycastHit();
        }
    }

	/// <summary>
	///		Select the edge hit that should have the priority
	/// </summary>
	private void SortEdgeHits()
	{
		_edgeHit = new RaycastHit();

		foreach (RaycastHit _hit in _edgeHits)
		{

		}

		for (int i = 1; i < _edgeHits.Length; i++)
		{
			//Study only point below character edge climb height
			if (_edgeHits[i].point.y - transform.position.y < _characterConfig.edgeMaxClimbingHeight)
			{
				
			}
		}
	}

	private enum EdgeType 
	{
		NONE,
		HIGH,
		LOW
	}

	/// <summary>
	///		
	/// </summary>
	private void ApplyEdgesSpeed()
	{
		if (_edgeHit.collider == null) { return;}

        Vector3 _edgeSlopeSlideLeft = Vector3.Cross(_edgeHit.normal, Vector3.up).normalized;
        Vector3 _edgeSlopeSlideDown = Vector3.Cross(_edgeHit.normal, _edgeSlopeSlideLeft).normalized;

        UnityEngine.Debug.DrawRay(transform.position, _edgeHit.normal, Color.blue);

        _movement += -_edgeSlopeSlideDown.normalized * _gravitySpeed;

        UnityEngine.Debug.DrawRay(transform.position, - _edgeSlopeSlideDown * _gravitySpeed, Color.cyan);
    }

	/// <summary>
	/// 	moves the character towards the input directions 
	/// </summary>
	private void HandleMovement()
	{
        _controller.Move(_movement * Time.deltaTime);
		_movement = Vector3.zero;

		// - update variables -
		if (_rsoCharacterPosition.value != _characterDirection.position) { _rsoCharacterPosition.value = _characterDirection.position; }
		if (_rsoCharacterForward.value != _characterDirection.forward) { _rsoCharacterForward.value = _characterDirection.forward; }
    }

    #endregion

    #region inputs

    /// <summary>
    /// 	Add character behavior to player inputs based on animation state
    /// </summary>
    private void SubscribeInputs()
    {
        switch (_currentState)
        {
            case AnimationState.LOCOMOTION:
                _rseMove.action += Move;
				_rseRun.action += Run;
                _rseJump.action += Jump;
                _rseThrow.action += ToggleAim;
                _rseCraft.action += ToggleCraft;
                _rseToggleInHand.action += ToggleInHand;
                _rseCancelAction.action += CancelAction;
                _rseInteract.action += Interact;
                _rseKillCharacter.action += HandleDeath;
				ToggleCraftInput(_hasBackpack);
                break;
            case AnimationState.JUMP:
                _rseMove.action += Move;
                _rseRun.action += Run;
                _rseThrow.action += ToggleAim;
                _rseToggleInHand.action += ToggleInHand;
                _rseCancelAction.action += CancelAction;
                _rseInteract.action += Interact;
                _rseKillCharacter.action += HandleDeath;
                break;
            case AnimationState.FALL:
                _rseMove.action += Move;
                _rseRun.action += Run;
                _rseJump.action += Jump;
                _rseThrow.action += ToggleAim;
                _rseToggleInHand.action += ToggleInHand;
                _rseCancelAction.action += CancelAction;
                _rseInteract.action += Interact;
                _rseKillCharacter.action += HandleDeath;
                
                break;
            case AnimationState.CRAFT:
                _rseRun.action += Run;
                _rseCraft.action += ToggleCraft;
                _rseCancelAction.action += CancelAction;
                _rseKillCharacter.action += HandleDeath;
                break;
            case AnimationState.ROPE:
                _rseMove.action += Move;
                _rseRun.action += Run;
                _rseJump.action += Jump;
                _rseThrow.action += ToggleAim;
                _rseCraft.action += ToggleCraft;
                _rseToggleInHand.action += ToggleInHand;
                _rseCancelAction.action += CancelAction;
                _rseInteract.action += Interact;
                _rseKillCharacter.action += HandleDeath;
                break;
            case AnimationState.LADDER:
                _rseMove.action += Move;
                _rseRun.action += Run;
                _rseJump.action += Jump;
                _rseThrow.action += ToggleAim;
                _rseCraft.action += ToggleCraft;
                _rseToggleInHand.action += ToggleInHand;
                _rseCancelAction.action += CancelAction;
                _rseInteract.action += Interact;
                _rseKillCharacter.action += HandleDeath;
                break;
            case AnimationState.AIM:
                _rseMove.action += Move;
                _rseRun.action += Run;
                _rseJump.action += Jump;
                _rseThrow.action += ToggleAim;
                _rseCancelAction.action += CancelAction;
                _rseKillCharacter.action += HandleDeath;
                break;
        }
    }

    /// <summary>
    /// 	remove character behavior from player inputs
    /// </summary>
    private void UnsubscribeInputs()
    {
        _rseMove.action -= Move;
        _rseRun.action -= Run;
        _rseJump.action -= Jump;
        _rseThrow.action -= ToggleAim;
        _rseCraft.action -= ToggleCraft;
        _rseToggleInHand.action -= ToggleInHand;
        _rseCancelAction.action -= CancelAction;
        _rseInteract.action -= Interact;
		_rseKillCharacter.action -= HandleDeath;
		_rseHolding.action -= Holding;
        _rseRecycle.action -= Recycle;
    }

	public void ToggleCraftInput(bool _isActive)
	{
		if (_isActive)
		{
			_rseCraft.action += ToggleCraft;
			_rseRecycle.action += Recycle;
		}
		else
		{
			_rseCraft.action -= ToggleCraft;
			_rseRecycle.action -= Recycle;
		}
    }

	private void ToggleInputs()
	{
		if (_rsoGamePaused.value)
		{
			CancelAction();
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
	/// 	Set _isJumping to true.
	/// 	Subscribed to RSE_Jump only in locomotion State.
	/// </summary>
	private void Jump()
	{
        _isJumping = true;
	}

	/// <summary>
	/// 	update the sprint input value
	/// </summary>
	/// <param name="isRunning">is the input pressed</param>
	private void Run(bool isRunning)
	{
		_isRunning = isRunning;
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
					_gravitySpeed = 0f;
				}
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				if (_isHolding)
				{
                    // reset the gravity velocity
                    _gravitySpeed = 0f;
                }
				else
				{
					_rope.UpdateHoldLength();
				}
				break;
		}
	}

	/// <summary>
	/// 	Try to activate permanent object in hand
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
		if (_interactables.Count > 0)
		{
			Interactible nearest = GetNearestInteractible();
			CheckShowInteract();
			if (nearest != null) CheckShowRecycle(nearest.isRecyclable);
			else CheckShowRecycle(false);
		}


		// speed calculations
		CheckWalkRun();
		ApplySlope();
		ApplyInputs();
		ApplyAcceleration();
		CreateMovement();
		ApplyStatus();
		ApplySnapGravity();

		// move controller
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
		_gravitySpeed = 0;
		_lastGroundedPlanarForward = _planarForward;
    }

	#endregion

	#region jump state

	private void EnterJumpState()
	{
		_rseJump.action -= Jump;
        _rseCraft.action -= ToggleCraft;

		if (_isGroundedLastFrame) {CheckWalkRun();}
		ApplyJumpImpulsePlanarSpeed();
        if (_isGroundedLastFrame) {ApplyInputs(); };
        if (_isGroundedLastFrame) {ApplyAcceleration();};
        ApplyJumpImpulseVerticalSpeed();
	}

	private void UpdateJumpState()
	{
		//ApplyAirControl();
		ApplyDrag();
		CreateMovementFall();
        ApplyGravity();

		DetectEdges();

        HandleMovement();
	}

	private void LateUpdateJumpState()
	{

	}


    private void ExitJumpState()
	{
		_rseJump.action += Jump;
        _rseCraft.action += ToggleCraft;
	}

	#endregion

	#region fall state

	private void EnterFallState()
	{
		_rseCraft.action -= ToggleCraft;

        _positionStartFall = transform.position;

        if (_isGroundedLastFrame) { CheckWalkRun(); }
        if (_isGroundedLastFrame) { ApplyInputs(); };
        if (_isGroundedLastFrame) { ApplyAcceleration(); };
    }

	private void UpdateFallState()
	{
        //ApplyAirControl();
        ApplyDrag();
        CreateMovementFall();
        ApplyGravity();

        DetectEdges();
		ApplyEdgesSpeed();

        HandleMovement();
	}

	private void LateUpdateFallState()
	{

	}


    private void ExitFallState()
	{
		_rseCraft.action += ToggleCraft;
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
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
        _rseThrow.action -= ToggleAim;
		_rseToggleInHand.action -= ToggleInHand;
        _rseInteract.action -= Interact;
	}

	private void UpdateCraftState()
	{
		ApplySlope();
		ApplyAcceleration();
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
		_rseMove.action += Move;
		_rseJump.action += Jump;
        _rseThrow.action += ToggleAim;
		_rseToggleInHand.action += ToggleInHand;
        _rseInteract.action += Interact;
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
        CheckWalkRun();
        ApplySlope();
        ApplyInputs();
        ApplyAcceleration();
        CreateMovement();
        ApplyStatus();
        ApplySnapGravity();

        // move controller
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
            //ApplyAirControl();
            ApplyDrag();
            CreateMovementFall();
            ApplyGravity();

            HandleMovement();
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
					_rope = _craftInHand as Rope;
					if (_rope != null) _rope?.Attach(_harness);

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
		_rseJump.action -= Jump;
        _rseCraft.action -= ToggleCraft;
		_rseToggleInHand.action -= ToggleInHand;
        _rseInteract.action -= Interact;
    }

    private void UpdateAimState()
    {
        // speed calculations
        CheckWalkRun();
        ApplySlope();
        ApplyInputs();
        ApplyAcceleration();
        CreateMovement();
        ApplyStatus();
        ApplySnapGravity();

        // move controller
        HandleMovement();
    }

	private void LateUpdateAimState()
	{
        _craftInHand.PreviewThrow(_thirdPersonCamera.transform);
    }

    private void ExitAimState()
    {
		_rseJump.action += Jump;
        _rseCraft.action += ToggleCraft;
		_rseToggleInHand.action += ToggleInHand;
        _rseInteract.action += Interact;
    }

    #endregion
	
    #region interaction
    private void Interact()
    {
		if (_interactables.Count == 0
			|| _currentState != AnimationState.LOCOMOTION) return;

		Interactible nearest = GetNearestInteractible();
		if (nearest != null) nearest.InteractionTrigger();

    }

    private void Recycle()
    {
        if (_interactables.Count == 0
            || _currentState != AnimationState.LOCOMOTION) return;

        Interactible interactible = GetNearestInteractible();
        if (interactible == null) return;

        if (interactible.isRecyclable
            && interactible.objectToRecycle != null)
        {
            _interactables.Remove(interactible);
            _validInteractibles.Remove(interactible);
            Destroy(interactible.objectToRecycle);
            CheckShowInteract();
            CheckShowRecycle(false);
        }
    }

    private Interactible GetNearestInteractible()
	{
		_validInteractibles = FilterInteractiblesByAngle();
		if (_validInteractibles.Count == 0) return null;

		return FilterInteractiblesByDistance();
    }

	private List<Interactible> FilterInteractiblesByAngle()
	{
		List<Interactible> validInteractibles = new List<Interactible>();

        for (int i = 0; i < _interactables.Count; i++)
        {
			Vector3 towardsInteract = _interactables[i].transform.position - transform.position;
			if (Vector3.Dot(
				new Vector3(_characterDirection.transform.forward.x, 0, _characterDirection.transform.forward.z).normalized, 
				new Vector3(towardsInteract.x, 0, towardsInteract.z).normalized
				) > 0.5)
				validInteractibles.Add(_interactables[i]);
        }

		return validInteractibles;
    }

	private Interactible FilterInteractiblesByDistance()
	{
		Interactible nearestInteractible = _validInteractibles[0];

        for (int i = 1; i < _validInteractibles.Count; i++)
        {
			if ((_validInteractibles[i].transform.position - this.transform.position).sqrMagnitude <
                     (nearestInteractible.transform.position - this.transform.position).sqrMagnitude)
            {
                nearestInteractible = _validInteractibles[i];
            }
        }

		return nearestInteractible;
    }

    public void AddToInteractList(Interactible _interactibleObject)
    {
        _interactables.Add(_interactibleObject);
    }

    public void RemoveFromInteractList(Interactible _interactibleObject)
    {
        _interactables.Remove(_interactibleObject);
		_validInteractibles.Remove(_interactibleObject);
		CheckShowInteract();
		CheckShowRecycle(false);
    }

	private void CheckShowInteract()
	{
		_rseCanInteract.Call(_validInteractibles.Count > 0 && _currentState == AnimationState.LOCOMOTION);
    }

	private void CheckShowRecycle(bool isRecyclable)
	{ 
		_rseCanRecycle.Call(isRecyclable && _currentState == AnimationState.LOCOMOTION);
	}
    #endregion
}
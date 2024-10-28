using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedHierarchy.Icons;
using NaughtyAttributes;
using Obi;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
	#region exposed variables

	[Header("Internal references")]
	[SerializeField] private Transform _cameraTransform;
	[SerializeField] private Transform _characterDirection;
	[SerializeField] private Transform _handSocket;
	[SerializeField] private Transform _robotHandSocket;
	[SerializeField] private Transform _ropeAttach;
	[SerializeField] private CharacterController _controller;
	public ObiCollider obiCollider;
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

    #endregion

    #region runtime variables

    [Header("debug: animation")]
	private AnimationState _currentState;

	[Header("debug: move")]
	private Vector2 _moveInput;
	private float _planarSpeed;
	private float _targetPlanarSpeed;
	private float _gravitySpeed;
	private Vector3 _movement;
	private bool _isRunning;
	private float _coyoteTime;

	[Header("debug: slope")]
	private float _slopePercentage;

	[Header("debug: fall")]
	private bool _isGrounded;
    private bool[] _groundChecks = new bool[5];
	private bool _isStunned = false;
	private bool _isSlowed = false;
	private bool _isSlowedPostStun = false;

	[Header("debug: momentum")]
	private Vector3 _positionStartFall;
	private Vector3 _lastGroundedPlanarForward;
	private float _fallHeight;

	[Header("debug: permanent")]
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
    private Interactible _nearestInteractible;

    // - permanent -
	private PreRope _currentPreRope;
	private Coroutine _craftCoroutine;

	// ----- CONST -----
	private const float _TERMINAL_VERTICAL_VELOCITY = 53.0f;

	#endregion

	#region monobehaviour functions

	private void Start()
	{
		SwitchState(AnimationState.LOCOMOTION);

        // creation of the interaction list
        _interactables = new List<Interactible>();

		_raycastLayerMask |= (1 << LayerMask.NameToLayer("Default"));
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
		_rseMove.action += Move;
		_rseJump.action += Jump;
		_rseRun.action += Run;
		_rseThrow.action += ToggleAim;
		_rseCraft.action += ToggleCraft;
		_rseToggleInHand.action += ToggleInHand;
		_rseCancelAction.action += CancelAction;
        _rseInteract.action += Interact;
    }

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
		_rseRun.action -= Run;
		_rseThrow.action -= ToggleAim;
		_rseCraft.action -= ToggleCraft;
		_rseToggleInHand.action -= ToggleInHand;
        _rseCancelAction.action -= CancelAction;
        _rseInteract.action -= Interact;
    }

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
    }

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

    #region death

	/// <summary>
	/// 	kill the character
	/// </summary>
    private void HandleDeath()
	{
		_rsoPlayerDeath.value = true;
		Destroy(gameObject);
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
			if(_currentState != AnimationState.FALL) _coyoteTime = _characterConfig.coyoteTime; 
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

		// - rope -
		if (Input.GetKeyDown(KeyCode.G))
		{
			_currentPreRope = Instantiate(_ropeConfig.pfPreRope);
		}

		if (Input.GetKeyUp(KeyCode.G))
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

	/// <summary>
	/// 	Set _isJumping to true.
	/// 	Subscribed to RSE_Jump only in locomotion State.
	/// </summary>
	private void Jump()
	{
        _isJumping = true;
	}

	/// <summary>
	/// 	Update the sprint input value.
	/// </summary>
	/// <param name="isRunning">is the input pressed</param>
	private void Run(bool isRunning)
	{
		_isRunning = isRunning;
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

	}


	#endregion

	#region locomotion state

	private void EnterLocomotionState()
	{
		
	}

	private void UpdateLocomotionState()
	{
		// temp
		HandleInputs();

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

		// HandleRopeLength();
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
		// temp
		HandleInputs();
		ApplySlope();
		// HandleStun();
		// HandleSlow();
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
    private IEnumerator Craft(CraftType _objectToCraft,float _craftDuration)
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
                _craftInHand.transform.position = _handSocket.transform.position;
                break;

			case CraftType.Ladder:
				_craftInHand = Instantiate(_ladderConfig.PF_Ladder, _handSocket.transform);
                _craftInHand.transform.position = _handSocket.transform.position;
                break;

			case CraftType.Rope: 
				break;
		}

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

    private void EnterRopeState()
	{

	}

	private void UpdateRopeState()
	{

	}

	private void LateUpdateRopeState()
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
                    _craftInHand = null;
					if (_craftInRobot != null)
					{
                        _craftInRobot.transform.SetParent(_handSocket, false);
						_craftInHand = _craftInRobot;
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
		// temp
        HandleInputs();

        ApplySlope();
        // HandleStun();
        // HandleSlow();
        ApplyAcceleration();
        ApplyGravity();
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
    }

    public void RemoveFromInteractList(Interactible _interactibleObject)
    {
        _interactables.Remove(_interactibleObject);
    }
    #endregion
}
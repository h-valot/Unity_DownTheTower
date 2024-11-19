using System.Collections;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
    #region REFERENCES

    [Header("Internal references")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private CapsuleCollider _collider;
	[SerializeField] private Transform _handSocket;
	[SerializeField] private Transform _robotSocket;
	[SerializeField] private Transform _harness;
	[SerializeField] private Transform _aimingLookTo;
	[SerializeField] private Transform _cameraTarget;
	[SerializeField] private CharacterGraphics _graphics;

	[Header("Scriptable references")]
    [SerializeField] private HPPC_CharacterConfig _characterConfig;
	[SerializeField] private TorchConfig _torchConfig;
	[SerializeField] private RopeConfig _ropeConfig;
	[Space(5)]
    [SerializeField] private RSE_Move _rseMove;
    [SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Craft _rseCraft;
	[SerializeField] private RSE_Throw _rseThrow;
	[Space(5)]
	[SerializeField] private RSO_MovementDatas _rsoMovementDatas;
	[SerializeField] private RSO_CameraStyle _rsoCameraStyle;

	#endregion

	#region VARIABLES

	// - Inputs -
	private Vector2 _moveInput = new Vector2();

    // - Collisions -
    private LayerMask _layerMaskToIgnore;
    private RaycastHit[] _raycastHits;

	// - Camera -
	private CameraManager _camera;

	// - Movement -
	private bool _isGrounded;
    private Vector3 _groundNormal;
    private bool _isRunning;
    private bool _hasRope;
    private bool _hasJump;
    private bool _isCrafting;

    // - State machine -
    private BehaviorState _currentState;

	// - Craft state -
	private CraftType _craftType;
	private Coroutine _craftCoroutine;
	private Permanent _handObject;
	private Permanent _robotObject;
	private bool _isAiming;

	// - Rope state -
	private Rope _rope;

	#endregion

	#region MONOBEHAVIOR

	private void Awake()
    {
        // Update drag in rigidbody if changed in characterConfig
        _characterConfig.OnConfigChanged += UpdateDrag;
        // Layer mask to remove character for cast, use ~_layerMaskToIgnore
        _layerMaskToIgnore |= 1 << LayerMask.NameToLayer("Character");

		CheckGround();
        DetermineState();

		_camera = Instantiate(_characterConfig.pfCamera, transform.position, Quaternion.identity, null).GetComponentInChildren<CameraManager>();
		_camera.Initialize(_aimingLookTo, _cameraTarget);

		_graphics.Initialize(_aimingLookTo);
	}

    private void OnDestroy()
    {
        UnsubscibeAllInputs();
        _characterConfig.OnConfigChanged -= UpdateDrag;
    }

    void FixedUpdate()
	{
		CheckGround();
        DetermineState();
        FixedUpdateState();
    }

    private void LateUpdate()
    {
		MovementDatas _movementDatas = new MovementDatas();

        _movementDatas.dataToString.Add((Mathf.Round(_rigidbody.velocity.magnitude * 100f) / 100f).ToString());
        _movementDatas.dataToString.Add(_isGrounded.ToString());
        _movementDatas.dataToString.Add(_currentState.ToString());
        _rsoMovementDatas.value = _movementDatas;

		if (_isAiming) _handObject.PreviewThrow(_camera.transform);
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if(_raycastHits != null)
        {
            foreach (RaycastHit _hit in _raycastHits)
            {
                Gizmos.DrawSphere(_hit.point, 0.05f);
            }
        }
    }

    #endregion

    #region INPUTS

    private void UpdateMoveInput(Vector2 _input)
    {
        _moveInput = _input;
    }

    private void UnsubscibeAllInputs()
    {
        _rseMove.action -= UpdateMoveInput;
        _rseJump.action -= Jump;
		_rseCraft.action -= ToggleCraft;
		_rseThrow.action -= ToggleAim;
	}

    private void SubscribeStateInputs()
    {
        switch (_currentState)
        {
            case BehaviorState.LOCOMOTION:
                _rseMove.action += UpdateMoveInput;
                _rseJump.action += Jump;
				_rseCraft.action += ToggleCraft;
				_rseThrow.action += ToggleAim;
				break;

            case BehaviorState.FALL:
                _rseMove.action += UpdateMoveInput;
				_rseThrow.action += ToggleAim;
				break;

            case BehaviorState.CRAFT:
				_rseCraft.action += ToggleCraft;
				break;

            case BehaviorState.ROPE:
				_rseMove.action += UpdateMoveInput;
				// _rseJump.action += Jump;
				_rseCraft.action += ToggleCraft;
				_rseThrow.action += ToggleAim;
				break;
        }
    }

    private void UpdateWalkRun(bool ispressed)
    {
        if (ispressed)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }
    }

    #endregion

    #region STATE MACHINE

    /// <summary>
    /// Determine which behavior state the player should be and trigger a switch of state if neccessary.
    /// </summary>
    private void DetermineState()
    {
        if (_currentState != BehaviorState.LOCOMOTION && _isGrounded && !_isCrafting)
        {
            SwitchState(BehaviorState.LOCOMOTION);
        }
        else if (_currentState != BehaviorState.FALL && !_isGrounded && !_hasRope)
        {
            SwitchState(BehaviorState.FALL);
        }
        else if (_currentState != BehaviorState.ROPE && !_isGrounded && _hasRope)
        {
            SwitchState(BehaviorState.ROPE);
        }
        else if (_currentState != BehaviorState.CRAFT && _currentState == BehaviorState.LOCOMOTION && _isCrafting)
        {
            SwitchState(BehaviorState.CRAFT);
        }
    }

    /// <summary>
    /// Switch to new state by triggering old state exit then new state enter
    /// </summary>
    /// <param name="_newState">New state to switch to</param>
    private void SwitchState(BehaviorState _newState)
    {
        ExitState();
        EnterState(_newState);
    }

    /// <summary>
    /// (1) Update _currentState value
    /// (2) Call EnterState method of the new state
    /// </summary>
    /// <param name="_newState">New state to trigger</param>
    private void EnterState(BehaviorState _newState)
    {
        _currentState = _newState;
		SubscribeStateInputs();

		switch (_currentState)
        {
            case BehaviorState.LOCOMOTION:
                EnterLocomotionState();
                break;

            case BehaviorState.FALL:
                EnterFallState();
                break;

            case BehaviorState.CRAFT:
                EnterCraftState();
                break;

            case BehaviorState.ROPE:
                EnterRopeState();
                break;
        }
	}

    /// <summary>
    /// Trigger the current state FixedUpdate method
    /// </summary>
    private void FixedUpdateState()
    {
        switch (_currentState)
        {
            case BehaviorState.LOCOMOTION:
                FixedUpdateLocomotionState();
                break;

            case BehaviorState.FALL:
                FixedUpdateFallState();
                break;

            case BehaviorState.CRAFT:
                FixedUpdateCraftState();
                break;

            case BehaviorState.ROPE:
                FixedUpdateRopeState();
                break;
        }
    }

    /// <summary>
    /// Trigger the current state Exit method
    /// </summary>
    private void ExitState()
	{
		UnsubscibeAllInputs();

		switch (_currentState)
        {
            case BehaviorState.LOCOMOTION:
                ExitLocomotionState();
                break;

            case BehaviorState.FALL:
                ExitFallState();
                break;

            case BehaviorState.CRAFT:
                ExitCraftState();
                break;

            case BehaviorState.ROPE:
                ExitRopeState();
                break;
		}
	}

    #endregion

    #region GROUND

    private void CheckGround()
    {
        _isGrounded = false;
        _groundNormal = Vector3.down;

        Vector3 _start = transform.position + Vector3.up * (_collider.height - _collider.radius);
        float _radius = _collider.radius + _characterConfig.skinWidth;
        Vector3 _direction = Vector3.down;
        float _distance = _collider.height - 2 * _collider.radius;
        _raycastHits = Physics.SphereCastAll(_start, _radius, _direction, _distance, ~_layerMaskToIgnore);

        //check each points
        foreach (RaycastHit _hit in _raycastHits)
        {
            //check if it is on the bottom round part of the capsule
            if (_hit.point.y < transform.position.y + _collider.radius)
            {
                float _angle = Vector3.Angle(_hit.normal, Vector3.up);
                if (_angle < 46f)
                {
                    _isGrounded = true;
                    //take the smallest normal from ground check as the new ground normal
                    if (Vector3.Dot(_hit.normal, Vector3.up) > Vector3.Dot(_groundNormal, Vector3.up))
                    {
                        _groundNormal = _hit.normal;
                    }
                }
            }
        }
    }

    #endregion    
    
    #region MOVEMENT

    private void Move()
    {
        SetFriction();

        if  (_moveInput != Vector2.zero)
        {
            Vector3 _desiredSpeed = (_camera.PlanarRight * _moveInput.x + _camera.PlanarForward * _moveInput.y).normalized;
            _desiredSpeed *= _characterConfig.walkSpeed;
            _rigidbody.AddForce(_desiredSpeed - _rigidbody.velocity, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// if player is not moving and grounded:
    /// Set the friction to a high value to prevent sliding on slope while immobile
    /// else
    /// Set the friction to a low value to slide against wall while falling and walking
    /// </summary>
    private void SetFriction()
    {
        if (_moveInput == Vector2.zero && _isGrounded)
        {
            _collider.sharedMaterial.dynamicFriction = _characterConfig.frictionNotMovingGround;
            _collider.sharedMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
        }
        else
        {
            _collider.sharedMaterial.dynamicFriction = _characterConfig.frictionMovingFalling;
            _collider.sharedMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        }
    }

    private void UpdateDrag()
    {
        if (_currentState == BehaviorState.LOCOMOTION)
        {
            _rigidbody.drag = _characterConfig.dragGround;
        }
        else if (_currentState == BehaviorState.FALL || _currentState == BehaviorState.ROPE)
        {
            _rigidbody.drag = _characterConfig.dragFall;
        }
    }

    /// <summary>
    /// Move function used when grounded
    /// </summary>
    private void MoveGrounded()
    {
        SetFriction();

        if (_moveInput != Vector2.zero)
        {
            Vector3 _desiredSpeed = (_camera.PlanarRight * _moveInput.x + _camera.PlanarForward * _moveInput.y).normalized;

            //orient speed along slope
            Vector3 _slopeRight = Vector3.Cross(Vector3.up, _groundNormal);
            _desiredSpeed = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, _groundNormal, _slopeRight), _slopeRight) * _desiredSpeed;

            //set desired speed magnitude based on walk/run state
            if (_isRunning)
            {
                _desiredSpeed *= _characterConfig.runSpeed;
            }
            else
            {
                _desiredSpeed *= _characterConfig.walkSpeed;
            }

            //apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
            _rigidbody.AddForce(_desiredSpeed - _rigidbody.velocity, ForceMode.Acceleration);
        }
    }

    private void Jump(bool input)
    {
        _rigidbody.AddForce(Vector3.up * _characterConfig.jumpForce, ForceMode.Impulse);
    }

    private void HandleStepOn()
    {
        if(_raycastHits.Length > 1 && _moveInput != Vector2.zero)
        {
            Vector3 _stepOnHeightTarget = transform.position;
            Vector3 _moveInput3D = (_camera.PlanarRight * _moveInput.x + _camera.PlanarForward * _moveInput.y).normalized;

            foreach (RaycastHit _hit in _raycastHits)
            {
                Vector3 _hitDirection = _hit.point - transform.position;
                _hitDirection = new Vector3(_hitDirection.x, 0, _hitDirection.z);

                //check if hit is in front of character
                if (Vector3.Dot(_moveInput3D,_hitDirection) > 0.1)
                {
                    if (_hit.point.y - transform.position.y < _characterConfig.StepOnHeight)
                    {
                        if (_hit.point.y > _stepOnHeightTarget.y)
                        {
                            _stepOnHeightTarget = _hit.point;
                        }
                    }
                }
            }

            if(_stepOnHeightTarget != transform.position)
            {
                _rigidbody.position = _stepOnHeightTarget;
            }
        }
    }

    #endregion

    #region LOCOMOTION STATE

    private void EnterLocomotionState()
    {
        UpdateDrag();
    }

    private void FixedUpdateLocomotionState()
    {
        HandleStepOn();
        MoveGrounded();
    }

    private void ExitLocomotionState()
    {

    }

    #endregion

    #region FALL STATE

    private void EnterFallState()
    {
        UpdateDrag();
        SetFriction();
    }

    private void FixedUpdateFallState()
    {

    }

    private void ExitFallState()
    {

    }

    #endregion

    #region ROPE STATE

    private void EnterRopeState()
	{
		UpdateDrag();
	}

    private void FixedUpdateRopeState()
	{
		Move();
	}

    private void ExitRopeState()
    {
		
    }

	#endregion

	#region CRAFT STATE

	private void EnterCraftState()
	{
		if (_craftType == CraftType.TORCH)
		{
			if (_handObject != null
			&& _handObject.Type != CraftType.TORCH
			&& _robotObject?.Type != CraftType.TORCH)
			{
				Destroy(_handObject.gameObject);
			}
			_craftCoroutine = StartCoroutine(Craft(CraftType.TORCH, _torchConfig.craftingDuration));
		}
		else if (_craftType == CraftType.ROPE)
		{
			if (_handObject != null)
			{
				if (_handObject.Type == CraftType.TORCH)
				{
					// _handObject.transform.SetParent(_robotSocket, false);
					// _robotObject = _handObject;
					// _handObject = null;

					SwitchObjects(ref _handObject, ref _robotObject, _robotSocket);
				}
				else if (_handObject.Type != CraftType.ROPE)
				{
					Destroy(_handObject.gameObject);
				}
			}
			_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, _ropeConfig.craftingDuration));
		}
	}

	private void FixedUpdateCraftState()
    {
		
    }

    private void ExitCraftState()
    {
		if (_craftCoroutine != null)
		{
			StopCoroutine(_craftCoroutine);
			_craftCoroutine = null;
			
			// if (_backpack != null) _backpack.EndCrafting();
		}
	}

	private void ToggleCraft(CraftType craftType, bool isInputPressed)
	{
		// Prevent switching to craft state if not in locomotion or crafting state or already crafting another item
		if (_currentState != BehaviorState.LOCOMOTION 
		|| _currentState != BehaviorState.CRAFT)
		{
			// If craft button is pressed
			if (isInputPressed)
			{
				_craftType = craftType;
				_isCrafting = true;
			}
			// If craft button is released
			else
			{
				_isCrafting = false;
			}
		}
	}

	/// <summary>
	/// 	Instantiate the torch prefab after the fixed duration.
	/// </summary>
	private IEnumerator Craft(CraftType craftType, float duration)
	{
		// Wait the crafting duration
		// if (_backpack != null) _backpack.StartCrafting(duration);

		yield return new WaitForSeconds(duration);

		_handObject = Instantiate(
			craftType == CraftType.TORCH ? (Permanent)_torchConfig.pfTorch : (Permanent)_ropeConfig.pfRope, 
			_handSocket.transform.position,
			Quaternion.identity,
			_handSocket.transform
		);

		// _backpack.EndCrafting();
		_craftCoroutine = null;
	}

	private void ToggleAim(bool isInputPressed)
	{
		// Assert: can't throw null
		if (_handObject == null) return;

		_isAiming = isInputPressed;
		_rsoCameraStyle.value = _isAiming ? CameraStyle.AIMING : CameraStyle.BASIC;

		// Handle preview on input pressed
		if (_isAiming)
		{
			_handObject.InitializePreview();
		}

		// Handle pernament throw on input released
		else
		{
			// Assert: object can't be thrown
			if (!_handObject.Throw(_camera.transform)) return;

			// Exception: rope attachment
			_rope = _handObject as Rope;
			if (_rope != null) _rope?.Attach(_harness);

			_handObject = null;
			SwitchObjects(ref _robotObject, ref _handObject, _handSocket);
		}
	}

	/// <summary>
	/// 	Set the position of the "from" permanent at the position of the "to" permanent.
	/// 	"to" being the one on the "socket" transform position.
	/// </summary>
	private void SwitchObjects(ref Permanent from, ref Permanent to, Transform socket)
	{
		// Assertion
		if (from == null) return;

		Permanent toCache = to;
		from.transform.SetParent(socket, false);
		to = from;
		to.transform.rotation = socket.transform.rotation;
		from = toCache;
	}

	#endregion
}
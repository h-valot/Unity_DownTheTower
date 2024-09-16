using System;
using NaughtyAttributes;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private CharacterController _controller;
	[SerializeField] private CapsuleCollider _triggerCapsule;
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
	[Header("Cinemachine")]
	[ShowNonSerializedField] private float _cinemachineTargetYaw;
	[ShowNonSerializedField] private float _cinemachineTargetPitch;

	[Header("Speed")]
	[ShowNonSerializedField] private bool _isSprinting;
	[ShowNonSerializedField] private Vector2 _moveInput;
	[ShowNonSerializedField] private float _speed;
	[ShowNonSerializedField] private float _targetSpeed;
	[ShowNonSerializedField] private float _animationBlend;
	[ShowNonSerializedField] private float _slopePercentage;

	[Header("Edging")]
	[ShowNonSerializedField] private bool _isEdging;

	[Header("Falling")]
	[ShowNonSerializedField] private bool _isGrounded;
	[ShowNonSerializedField] private float _lastGroundedSpeed;
	[ShowNonSerializedField] private Vector3 _lastGroundedPosition;
	[ShowNonSerializedField] private Vector3 _lastGroundedDirection;
	[ShowNonSerializedField] private float _lastDistanceTravelled;
	[ShowNonSerializedField] private bool _isStunned;
	[ShowNonSerializedField] private float _stunTimer;
	[ShowNonSerializedField] private bool _isSlowed;
	[ShowNonSerializedField] private float _slowTimer;
	[ShowNonSerializedField] private float _slowModifier;

	[Header("Rotation")]
	[ShowNonSerializedField] private float _targetRotation = 0.0f;
	[ShowNonSerializedField] private float _rotationVelocity;
	[ShowNonSerializedField] private float _verticalVelocity;

	[Header("Delay timer")]
	[ShowNonSerializedField] private float _fallDelayTimer;
	[ShowNonSerializedField] private float _jumpDelayTimer;

	// ----- ANIMATIONS PARAMS -----
	private int _animSpeed;
	private int _animGrounded;
	private int _animJump;
	private int _animFreeFall;
	private int _animMotionSpeed;

    // ----- PUBLIC VARIABLES -----
	public bool torchInHand;

	// ----- PRIVATE VARIABLES -----
	private bool _groundedCheckLocked;

	// ----- CONSTS -----
	private const float _TERMINAL_VELOCITY = 53.0f;
	private const float _LOOK_THRESHOLD = 0.01f;

	// ----- PROPERTIES -----
	private bool CanMove => !_isEdging;

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

		// update last grounded position to avoid instant death on spawn
		_lastGroundedPosition = transform.position;
	}

	private void Update()
	{
		// apply movement effect
		HandleStun();
		HandleSlow();

		// status checker
		CheckGrounded();

		// moves if possible
		if (CanMove)
		{
			Accelerate();
		}

		// apply mandatory forces
		ApplyGravity();
		Move();

		// fix edge motor cases 
		HandleEdging();
	}

	private void LateUpdate()
	{
		HandleCamera();
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

	private void HandleDeath()
	{
		Destroy(gameObject);
		_rsoPlayerDeath.value = true;
	}

	private void CheckGrounded()
	{
		// set ray with offset
		Vector3 rayPosition = new Vector3(transform.position.x, transform.position.y - _characterConfig.groundedOffset, transform.position.z);
		Vector3 downVector = transform.TransformDirection(Vector3.down);
		Vector3 forwardVector = transform.TransformDirection(Vector3.forward);
		_isGrounded = false;

		// check lethal death
		_lastDistanceTravelled = Math.Abs(transform.position.y - _lastGroundedPosition.y);
		if (_lastDistanceTravelled >= _characterConfig.lethalHeight)
		{
			// stops camera movements
			_cinemachineCameraTarget.transform.SetParent(_lockedCameraTarget.transform);
		}

		if (Physics.Raycast(new Ray(rayPosition, downVector), out var result, _characterConfig.groundedRadius, _characterConfig.groundLayers, QueryTriggerInteraction.Ignore))
		{
			// first time the character touches the ground after being falling
			if (!_isGrounded)
			{
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

			_isGrounded = true;
			_lastGroundedPosition = transform.position;

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
			_lastGroundedPosition = transform.position;

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

		if (_isSlowed)
		{
			_targetSpeed *= 1 - _slowModifier;
		}

		if (_isStunned)
		{
			_targetSpeed = 0;
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
		// - variables -
		Vector3 calculation; // used for calculations
		Vector3 direction = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized; // normalised input direction

		// if there is a move input rotate player when the player is moving
		// Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		if (_moveInput != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _characterConfig.rotationSmoothTime);

			// rotate to face input direction relative to camera position
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
		float currentSpeed = _speed;

		if (_isGrounded)
		{
			// - slipping jumping gravity - 
			if (_groundSlopeAngle > _controller.slopeLimit)
			{
				// movement left/right while slipping down
				// player rotation to slope
				Vector3 slopeRight = Quaternion.LookRotation(Vector3.right) * _groundSlopeDir;
				float dot = Vector3.Dot(slopeRight, transform.right);
				// move on X axis, with Y rotation relative to slopeDir
				direction = slopeRight * (dot > 0 ? transform.forward.normalized.x : -transform.forward.normalized.x);

				// speed
				_targetSpeed = Mathf.Lerp(_speed, _characterConfig.sprintSpeed, 5f * Time.deltaTime);

				// increase angular gravity
				calculation = Vector3.Slerp(_fauxGravity, _groundSlopeDir * _characterConfig.sprintSpeed, 4f * Time.deltaTime);
				_fauxGravity = calculation.normalized * _fauxGravity.magnitude;
			}
			else
			{
				// reset angular fauxGravity movement
				_fauxGravity.x = 0;
				_fauxGravity.z = 0;

				// constant grounded gravity
				if (_fauxGravity.y < 0)
				{
					_fauxGravity.y = Mathf.Lerp(_fauxGravity.y, -1f, 4f * Time.deltaTime);
				}
			}
		}
		else
		{
			// handle air control
			targetDirection = _lastGroundedDirection + (targetDirection * _characterConfig.airSpeed);
			currentSpeed = _lastGroundedSpeed;
		}

		// - move -
		calculation = direction * _speed * Time.deltaTime;
		calculation += _fauxGravity * Time.deltaTime;
		_controller.Move(calculation);


		_controller.Move(targetDirection.normalized * (currentSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		// update rso character transform data
        _rsoPlayerTranform.value = transform;

		// - debug - 
#if UNITY_EDITOR
		// slope angle and fauxGravity debug info
		if (showGizmos)
		{
			calculation = transform.position;
			calculation.y += _groundCheckY;
			UnityEngine.Debug.DrawRay(calculation, _groundSlopeDir.normalized * 5f, Color.blue);
			UnityEngine.Debug.DrawRay(calculation, _fauxGravity, Color.green);
		}
#endif

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

	[Header("Debug Gizmos")]
	[Tooltip("Show debug gizmos and lines")]
	public bool showGizmos = false;                  // Show debug gizmos and lines

	public Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
	public Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);

	private void HandleEdging()
	{
		Vector3 origin = new Vector3(transform.position.x, transform.position.y + _groundCheckY, transform.position.z);

		// now that's all fine and dandy, but on edges, corners, etc, we get angle values that we don't want.
		// to correct for this, let's do some raycasts. You could do more raycasts, and check for more
		// edge cases here. there are lots of situations that could pop up, so test and see what gives you trouble.

		// FIRST RAYCAST
		if (Physics.Raycast(origin + rayOriginOffset1, Vector3.down, out var slopeHit1, _raycastLength))
		{
#if UNITY_EDITOR
			// debug line to first hit point
			if (showGizmos) { UnityEngine.Debug.DrawLine(origin + rayOriginOffset1, slopeHit1.point, Color.red); }
#endif
			// get angle of slope on hit normal
			float angleOne = Vector3.Angle(slopeHit1.normal, Vector3.up);

			// 2ND RAYCAST
			if (Physics.Raycast(origin + rayOriginOffset2, Vector3.down, out var slopeHit2, _raycastLength))
			{
#if UNITY_EDITOR
				// debug line to second hit point
				if (showGizmos) { UnityEngine.Debug.DrawLine(origin + rayOriginOffset2, slopeHit2.point, Color.red); }
#endif
				// get angle of slope of these two hit points.
				float angleTwo = Vector3.Angle(slopeHit2.normal, Vector3.up);

				// 3 collision points: Take the MEDIAN by sorting array and grabbing middle.
				float[] tempArray = new float[] { _groundSlopeAngle, angleOne, angleTwo };
				System.Array.Sort(tempArray);
				_groundSlopeAngle = tempArray[1];
			}
			else
			{
				// 2 collision points (sphere and first raycast): AVERAGE the two
				float average = (_groundSlopeAngle + angleOne) / 2;
				_groundSlopeAngle = average;
			}
		}

		// if (_isGrounded	== false
		// 	&& _speed <= _characterConfig.edgingSpeedThreshold
		// 	&& _triggerCapsule.isTrigger)
		// {
		// 	_isEdging = true;
		// 	Debug.Log("CHARACTER_MOTOR: edging.");

		// 	// get character direction vectors
		// 	Vector3 forwardVector = transform.TransformDirection(Vector3.forward);
		// 	Vector3 downVector = transform.TransformDirection(Vector3.down);
		// 	Vector3 upVector = transform.TransformDirection(Vector3.up);

		// 	// box cast at the front base of the character capsule to detect edges
		// 	Vector3 boxCenter = transform.position + (_controller.radius / 2f) * (forwardVector.normalized + upVector.normalized);
		// 	Vector3 boxSize = new Vector3(2 * _controller.radius, _controller.radius, _controller.radius);
		// 	if (Physics.BoxCast(boxCenter, boxSize, forwardVector, out var boxInfo))
		// 	{
		// 		Debug.Log("CHARACTER_MOTOR: box touched.");

		// 		// ray cast down with a forward and up offset, starting it at the edge of the capsule demi-sphere
		// 		Vector3 forwardOffset = forwardVector.normalized * _controller.radius * _characterConfig.edgingForwardOffsetScalar;
		// 		Vector3 upOffset = new Vector3(0, _controller.radius, 0);
		// 		Vector3 origin = transform.position + forwardOffset + upOffset;
		// 		if (Physics.Raycast(origin, downVector, out var groundInfo, _controller.height))
		// 		{
		// 			Debug.Log("CHARACTER_MOTOR: ground touched.");

		// 			// dot value determines if the face of the touched ground has the same direction with the down direction vector
		// 			if (Vector3.Dot(groundInfo.normal, downVector) < _characterConfig.edgingDotProductThreshold)
		// 			{
		// 				// exit, if the character height does not fit into the ground it tries to snap to
		// 				if (Physics.Raycast(groundInfo.point, groundInfo.normal, out var roofInfo, _controller.height))
		// 				{
		// 					Debug.Log("CHARACTER_MOTOR: roof touched. rejected");
		// 					transform.position -= forwardVector.normalized * 0.2f;
		// 					return;
		// 				}

		// 				// finally, snap the character
		// 				Debug.Log("CHARACTER_MOTOR: roof not detected. snapped");
		// 				transform.position = groundInfo.point;
		// 			}
		// 		}
		// 	}
		// }
		// else
		// {
		// 	_isEdging = false;
		// }
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


	[Header("Grounded Settings")]
	[Tooltip("The starting position of the isGrounded spherecast. Set to the sphereCastRadius plus the CC Skin Width. Enable showGizmos to visualize.")]
	// this should be just above the base of the cc, in the amount of the skin width (in case the cc sinks in)
	public float _groundCheckY = 0.33f;               // 0.25 + 0.08 (sphereCastRadius + CC skin width)

	[Space(5)]
	public float _sphereCastRadius = 0.25f;           // radius of area to detect for ground
	public float _sphereCastDistance = 0.75f;         // How far spherecast moves down from origin point

	[Space(5)]
	public float _raycastLength = 0.75f;              // secondary raycasts (match to sphereCastDistance)

	[Space(5)]
	private float _lastSpeed = 0;                     // reference for calculating speed
	private Vector3 _fauxGravity = Vector3.zero;      // calculated gravity

	[Space(5)]
	public float _groundSlopeAngle = 0f;              // Angle of the slope in degrees
	public Vector3 _groundSlopeDir = Vector3.zero;    // The calculated slope as a vector

	[Space(5)]
	public bool _cursorActive = false;                // cursor state

	// this script pushes all rigidbodies that the character touches
	// private void OnControllerColliderHit(ControllerColliderHit hit)
	// {
	// 	Rigidbody body = hit.collider.attachedRigidbody;

	// 	// no rigidbody
	// 	if (body == null || body.isKinematic)
	// 	{
	// 		return;
	// 	}

	// 	// We dont want to push objects below us
	// 	if (hit.moveDirection.y < -0.3f)
	// 	{
	// 		return;
	// 	}

	// 	// If you know how fast your character is trying to move,
	// 	// then you can also multiply the push velocity by that.
	// 	body.velocity = hit.moveDirection * _lastSpeed;
	// }

	// Debug Gizmos
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (!showGizmos) return;
		
		Vector3 startPoint = new Vector3(transform.position.x, transform.position.y + _groundCheckY, transform.position.z);
		Vector3 endPoint = startPoint + new Vector3(0, -_sphereCastDistance, 0);

		Gizmos.color = _isGrounded == true ? Color.green : Color.white;
		Gizmos.DrawWireSphere(startPoint, _sphereCastRadius);

		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(endPoint, _sphereCastRadius);

		Gizmos.DrawLine(startPoint, endPoint);
	}
#endif
}

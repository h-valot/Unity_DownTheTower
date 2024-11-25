using System.Collections;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
    #region REFERENCES

    [Header("Internal references")]
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private CapsuleCollider m_collider;
	[SerializeField] private Transform m_handSocket;
	[SerializeField] private Transform m_robotSocket;
	[SerializeField] private Transform m_harness;
	[SerializeField] private Transform m_aimingLookTo;
	[SerializeField] private Transform m_cameraTarget;
	[SerializeField] private CharacterGraphics m_characterGraphics;

	[Header("Scriptable references")]
    [SerializeField] private NewCharacterConfig m_characterConfig;
	[SerializeField] private TorchConfig m_torchConfig;
	[SerializeField] private RopeConfig m_ropeConfig;
	[Space(5)]
    [SerializeField] private RSE_Move m_rseMove;
    [SerializeField] private RSE_Jump m_rseJump;
	[SerializeField] private RSE_Craft m_rseCraft;
	[SerializeField] private RSE_Throw m_rseThrow;
	[SerializeField] private RSE_Run m_rseRun;
	[SerializeField] private RSE_Climb m_rseClimb;
	[SerializeField] private RSE_Cancel m_rseCancel;
	[SerializeField] private RSE_BackpackCrafting m_rseBackpackCrafting;
	[Space(5)]
	[SerializeField] private RSO_MovementDatas m_rsoMovementDatas;
	[SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	#endregion

	#region VARIABLES

	// - Inputs -
	private Vector2 m_moveInput = new Vector2();

    // - Collisions -
    private LayerMask m_layerMaskToIgnore;
    private RaycastHit[] m_raycastHits;

	// - Camera -
	private CameraMotor m_cameraMotor;

    // - Ground -
	private bool m_isGrounded;
    private Vector3 m_groundNormal;
    private float m_coyoteTime;

	// - Movement -
    private bool m_isRunning;
    private bool m_hasJumped;
	public bool IsJumpingPressed { get; private set; }
	private bool m_isCrafting;

	// - Craft state -
	private CraftType m_craftType;
	private Coroutine m_craftCoroutine;
	private Permanent m_handObject;
	private Permanent m_robotObject;
	private bool m_isAiming;

	// - Rope state -
	private Rope m_rope;
	public bool m_isHolding;
	private bool HasRope => m_rope && m_rope.IsPlaced;

	// Cancel
	private bool m_isCancellingRope;
	private Coroutine m_cancelRopeCoroutine;
	private float m_cancelRopeTimer;

	// Climbing
	public bool m_isClimbing;
	public float m_currentClimbSpeed;

	// Misc
	private bool m_ropeConstraintAppliedLastly;
	private Vector3 m_positionStartFall;
	private const float k_fallingForcesThreshold = 0.2f;

	#endregion

	#region MONOBEHAVIOR

	public void Initialize(Quaternion startRotation)
    {
        // Update drag in rigidbody if changed in characterConfig
        m_characterConfig.OnConfigChanged += UpdateDrag;
        // Layer mask to remove character for cast, use ~_layerMaskToIgnore
        m_layerMaskToIgnore |= 1 << LayerMask.NameToLayer("Character");

        m_rigidbody.position = Vector3.zero;

        m_cameraMotor = Instantiate(m_characterConfig.pfCamera, transform.position, Quaternion.identity, null).GetComponentInChildren<CameraMotor>();
        m_cameraMotor.Initialize(m_aimingLookTo, m_cameraTarget, startRotation);

        m_characterGraphics.Initialize(m_aimingLookTo, m_rigidbody, startRotation);

		m_rsoCraftInputLocked.value = false;
		m_rsoRecycleInputLocked.value = false;
	}

    private void OnEnable()
    {
        CheckGround();
        DetermineState();
    }

    private void OnDisable()
    {
        UnsubscibeAllInputs();
        m_characterConfig.OnConfigChanged -= UpdateDrag;
        m_rsoCharacterState.value = BehaviorState.NONE;
    }

    private void FixedUpdate()
	{
        // Tkt fréro c'est pour pas soft lock le spherecast de detection du sol
        if (m_rigidbody.position == Vector3.zero)
        {
            m_rigidbody.position = new Vector3(0.01f, 0f, 0f);
        }

		CheckGround();
        DetermineState();
        FixedUpdateState();

		m_rsoCharacterPosition.value = m_rigidbody.position;
	}

    private void LateUpdate()
    {
		MovementDatas _movementDatas = new MovementDatas();

        _movementDatas.dataToString.Add((Mathf.Round(m_rigidbody.velocity.magnitude * 100f) / 100f).ToString());
        _movementDatas.dataToString.Add(m_isGrounded.ToString());
        _movementDatas.dataToString.Add(m_rsoCharacterState.value.ToString());
        m_rsoMovementDatas.value = _movementDatas;

		if (m_isAiming) m_handObject.PreviewThrow(m_cameraMotor.transform);
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if(m_raycastHits != null)
        {
            foreach (RaycastHit _hit in m_raycastHits)
            {
                Gizmos.DrawSphere(_hit.point, 0.05f);
            }
		}

		if (m_rope && m_rope.IsPlaced)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(m_rope.CurrentFold, m_rope.HoldLength);
		}
	}

#endif

    #endregion

    #region INPUTS

    private void UnsubscibeAllInputs()
    {
        m_rseMove.action -= UpdateMoveInput;
		m_rseRun.action -= UpdateRunInput;
		m_rseRun.action -= UpdateHoldInput;
		m_rseJump.action -= Jump;
		m_rseCraft.action -= ToggleCraft;
		m_rseThrow.action -= ToggleAim;
		m_rseClimb.action -= UpdateClimbInput;
		m_rseCancel.action -= CancelRope;
	}

    private void SubscribeStateInputs()
	{
		switch (m_rsoCharacterState.value)
        {
            case BehaviorState.LOCOMOTION:
                m_rseMove.action += UpdateMoveInput;
				m_rseRun.action += UpdateRunInput;
                m_rseJump.action += Jump;
				m_rseCraft.action += ToggleCraft;
				m_rseThrow.action += ToggleAim;
				m_rseCancel.action += CancelRope;
				break;

			case BehaviorState.ROPE:
				m_rseMove.action += UpdateMoveInput;
				m_rseRun.action += UpdateHoldInput;
				m_rseThrow.action += ToggleAim;
				m_rseClimb.action += UpdateClimbInput;
				m_rseCancel.action += CancelRope;
				break;

			case BehaviorState.FALL:
                m_rseMove.action += UpdateMoveInput;
				m_rseRun.action += UpdateRunInput;
				m_rseThrow.action += ToggleAim;
				break;

            case BehaviorState.CRAFT:
				m_rseCraft.action += ToggleCraft;
				break;
        }
	}

	private void UpdateMoveInput(Vector2 input)
	{
		m_moveInput = input;
	}

	private void UpdateRunInput(bool isPressed)
	{
		m_isRunning = isPressed;
    }

	private void UpdateHoldInput(bool isHolding)
	{
		if (IsJumpingPressed && !isHolding) return;

		if (m_rope == null)
		{
			m_isHolding = false;
			return;
		}

		// Handle both hold methods
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				ToggleRopeHolding(isHolding);
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				ToggleRopeHolding(!isHolding);
				break;
		}

		m_isHolding = isHolding;
	}

	private void UpdateHoldInput(Triome isHolding)
	{
		if (isHolding == Triome.NEITHER)
		{
			print("CHARACTER_MOTOR: Assert - isHolding value is equal to NEITHER.");
			return;
		}

		UpdateHoldInput(isHolding == Triome.TRUE);
	}

	private void UpdateClimbInput(bool isClimbing)
	{
		m_isClimbing = isClimbing;
	}

	#endregion

	#region STATE MACHINE

	/// <summary>
	/// Determine which behavior state the player should be and trigger a switch of state if neccessary.
	/// </summary>
	private void DetermineState()
    {
        if (m_rsoCharacterState.value != BehaviorState.LOCOMOTION && m_isGrounded && !m_isCrafting)
        {
            SwitchState(BehaviorState.LOCOMOTION);
        }
        else if (m_rsoCharacterState.value != BehaviorState.FALL && !m_isGrounded && !HasRope)
        {
            SwitchState(BehaviorState.FALL);
        }
        else if (m_rsoCharacterState.value != BehaviorState.ROPE && !m_isGrounded && HasRope)
        {
            SwitchState(BehaviorState.ROPE);
        }
        else if (m_rsoCharacterState.value != BehaviorState.CRAFT && m_rsoCharacterState.value == BehaviorState.LOCOMOTION && m_isCrafting)
        {
            SwitchState(BehaviorState.CRAFT);
        }
    }

    /// <summary>
    /// Switch to new state by triggering old state exit then new state enter
    /// </summary>
    /// <param name="newState">New state to switch to</param>
    private void SwitchState(BehaviorState newState)
    {
        ExitState();
        EnterState(newState);
    }

    /// <summary>
    /// (1) Update _currentState value
    /// (2) Call EnterState method of the new state
    /// </summary>
    /// <param name="newState">New state to trigger</param>
    private void EnterState(BehaviorState newState)
    {
		m_rsoCharacterState.value = newState;
		SubscribeStateInputs();

		switch (m_rsoCharacterState.value)
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
        switch (m_rsoCharacterState.value)
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

		switch (m_rsoCharacterState.value)
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
        m_isGrounded = false;
        m_groundNormal = Vector3.down;

        Vector3 _start = transform.position + Vector3.up * (m_collider.height - m_collider.radius);
        float _radius = m_collider.radius + m_characterConfig.skinWidth;
        Vector3 _direction = Vector3.down;
        float _distance = m_collider.height - 2 * m_collider.radius;
        m_raycastHits = Physics.SphereCastAll(_start, _radius, _direction, _distance, ~m_layerMaskToIgnore);

        //check each points
        foreach (RaycastHit hit in m_raycastHits)
        {
            //exclude hit point that come from the spherecast spawning inside a collider
            if (hit.point == Vector3.zero)
            {
                continue;
            }
            //check if it is on the bottom round part of the capsule
            if (hit.point.y < transform.position.y + m_collider.radius)
            {
                float _angle = Vector3.Angle(hit.normal, Vector3.up);
                if (_angle < 46f)
                {
                    m_isGrounded = true;
                    //take the smallest normal from ground check as the new ground normal
                    if (Vector3.Dot(hit.normal, Vector3.up) > Vector3.Dot(m_groundNormal, Vector3.up))
                    {
                        m_groundNormal = hit.normal;
                    }
                }
            }
        }
    }

    private void StartCoyoteTime()
    {
        if (!m_hasJumped)
        {
            m_coyoteTime = m_characterConfig.CoyoteTime;
            m_rseJump.action += Jump;
        }
    }

    private void UpdateCoyoteTime()
    {
        if (m_hasJumped || m_coyoteTime <= 0)
        {
            m_rseJump.action -= Jump;
            return;
        }

        m_coyoteTime -= Time.fixedDeltaTime;
    }

    #endregion    
    
    #region MOVEMENT

    /// <summary>
    /// if player is not moving and grounded:
    /// Set the friction to a high value to prevent sliding on slope while immobile
    /// else
    /// Set the friction to a low value to slide against wall while falling and walking
    /// </summary>
    private void SetFriction()
    {
        if (m_moveInput == Vector2.zero && m_isGrounded)
        {
            m_collider.sharedMaterial.dynamicFriction = m_characterConfig.frictionNotMovingGround;
            m_collider.sharedMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
        }
        else
        {
            m_collider.sharedMaterial.dynamicFriction = m_characterConfig.frictionMovingFalling;
            m_collider.sharedMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        }
    }

    /// <summary>
    /// Update drag based on behavior state to allow the player to fall faster
    /// </summary>
    private void UpdateDrag()
    {
        if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION)
        {
            m_rigidbody.drag = m_characterConfig.dragGround;
        }
        else if (m_rsoCharacterState.value == BehaviorState.FALL || m_rsoCharacterState.value == BehaviorState.ROPE)
        {
            m_rigidbody.drag = m_characterConfig.dragFall;
        }
    }

    /// <summary>
    /// (1) Update friction based on gorunded or not to not slide on slope if immobile
    /// (2) Calculate desired speed force based on input and camera direction
    /// (3) Orient speed force on floor
    /// (4) Multiply desired speed force by walk/run speed
    /// (5) Apply force and auto clamp it by susubstractiong actual speed to desired speed
    /// </summary>
    private void MoveGrounded()
    {
        SetFriction();

        if (m_moveInput != Vector2.zero)
        {
            Vector3 desiredSpeed = (m_cameraMotor.PlanarRight * m_moveInput.x + m_cameraMotor.PlanarForward * m_moveInput.y).normalized;

            // Orient speed along slope
            Vector3 slopeRight = Vector3.Cross(Vector3.up, m_groundNormal);
            desiredSpeed = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, m_groundNormal, slopeRight), slopeRight) * desiredSpeed;

            // Set desired speed magnitude based on walk/run state
            desiredSpeed *= m_isRunning ? m_characterConfig.runSpeed : m_characterConfig.walkSpeed;

            // Apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
            m_rigidbody.AddForce(desiredSpeed - m_rigidbody.velocity, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// (1) Check if there is valid points to step on
    /// (2) Select the highest point among the point in front the character
    /// (3) On the selected point, check if there is really a object to step on if front
    /// (4) Check if there there is a flat surface to step onto (<45 degrees)
    /// </summary>
    private void HandleStepOn()
    {
        if (m_raycastHits.Length > 1 && m_moveInput != Vector2.zero)
        {
            Vector3 stepOnTarget = transform.position;
            Vector3 moveInput3D = (m_cameraMotor.PlanarRight * m_moveInput.x + m_cameraMotor.PlanarForward * m_moveInput.y).normalized;

            foreach (RaycastHit _hit in m_raycastHits)
            {
                Vector3 hitDirection = _hit.point - transform.position;
                hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z);

                // Check if hit is in front of character
                if (Vector3.Dot(moveInput3D, hitDirection) > 0.15)
                {
                    if (_hit.point.y - transform.position.y < m_characterConfig.stepOnHeight)
                    {
                        // We take the highest that is higher than skin width to not trigger step on very small objects
                        if (_hit.point.y > stepOnTarget.y && _hit.point.y > transform.position.y + m_characterConfig.skinWidth)
                        {
                            stepOnTarget = _hit.point;
                        }
                    }
                }
            }

            if (stepOnTarget != transform.position)
            {
                // Check if there is really an object to step on in the speed direction, to prevent steping on end of slope
                Vector3 start = new Vector3(m_rigidbody.position.x, m_rigidbody.position.y + m_characterConfig.skinWidth, m_rigidbody.position.z);
                Vector3 direction = m_rigidbody.velocity.normalized;
                float distance = m_collider.radius * 2;
                if (Physics.Raycast(start, direction, distance, ~m_layerMaskToIgnore))
                {
                    // Check if there is a flat surface to step on (<45 degrees)
                    start = stepOnTarget + (new Vector3(stepOnTarget.x, 0, stepOnTarget.z) - new Vector3(m_rigidbody.position.x, 0, m_rigidbody.position.z)).normalized * m_characterConfig.skinWidth + new Vector3(0, m_characterConfig.skinWidth, 0);
                    direction = Vector3.down;
                    distance = m_characterConfig.skinWidth * 2;
                    if (Physics.Raycast(start, direction, distance, ~m_layerMaskToIgnore))
                    {
                        m_rigidbody.position = new Vector3(m_rigidbody.position.x, stepOnTarget.y, m_rigidbody.position.z);
                    }
                }
            }
        }
    }

    /// <summary>
    /// If the input is pressed and If the player hasn't jumped:
    /// Add a vertical impulse to the player
    /// </summary>
    private void Jump(bool isPressed)
    {
		// Assertions
        if (!isPressed) return;
		if(m_hasJumped) return;

		m_rigidbody.AddForce(Vector3.up * m_characterConfig.jumpForce, ForceMode.Impulse);
		m_hasJumped = true;
    }

    /// <summary>
    /// (1) Calculate desired speed force based on input and camera direction
    /// (2) Multiply desired speed force by walk/run speed
    /// (3) Apply force and auto clamp it by susubstractiong actual speed to desired speed
    /// (4) Multiply said speed force by falling factor
    /// </summary>
    private void MoveFalling()
    {
        Vector3 desiredSpeedForce = (m_cameraMotor.PlanarRight * m_moveInput.x + m_cameraMotor.PlanarForward * m_moveInput.y).normalized;

        // Set desired speed magnitude based on walk/run state
        if (m_isRunning)
        {
            desiredSpeedForce *= m_characterConfig.runSpeed;
        }
        else
        {
            desiredSpeedForce *= m_characterConfig.walkSpeed;
        }

        // Apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
        m_rigidbody.AddForce((desiredSpeedForce - m_rigidbody.velocity) * m_characterConfig.fallingControlFactor, ForceMode.Acceleration);
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
        StartCoyoteTime();
    }

    private void FixedUpdateFallState()
    {
        UpdateCoyoteTime();
        MoveFalling();
    }

    private void ExitFallState()
    {
        m_hasJumped = false;
    }

	#endregion

	#region ROPE STATE

	// TODO - Jump off the rope on motion
	// TODO - Re-equip an already-used rope (debug version)
	// TODO - In partial suspension, make the character able to jump off the wall
	// TODO - In partial suspension, make the character unable to move while off the wall
	// TODO - In partial suspension, make the character unable to be snap against a cambered wall 
	// TODO - In complete suspension, make the character pivot with the rope inclination
	// TODO - Lerp the rope stop deceleration

	private void EnterRopeState()
	{
		EnterFallState();
		ToggleRopeConstraint(true);
	}

	private void FixedUpdateRopeState()
	{
		if (IsFallingWithRope())
		{
			FixedUpdateFallState();
			return;
		}

		if (m_rope.GetTotalLength() > m_ropeConfig.MaxLength)
		{
			DesequipRope();
			return;
		}

		HandleRopeMovement();
		HandleRopeClimbing();
	}

	private void ExitRopeState()
	{
		ToggleRopeConstraint(false);
		m_isHolding = false;
		m_isClimbing = false;
	}

	private void HandleRopeMovement()
	{
		// TODO
	}

	private void HandleRopeClimbing()
	{
		// Assert: holding input method
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				// While hold to stop, we don't constraint the character if the player IS NOT holding the button
				if (!m_isHolding) return;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				// While hold to let go, we don't constraint the character if the player IS holding the button
				if (m_isHolding) return;
				break;
		}

		if (!m_isClimbing)
		{
			m_currentClimbSpeed = m_characterConfig.climbAcceleration;
			return;
		}

		m_currentClimbSpeed += m_currentClimbSpeed * Time.fixedDeltaTime;
		float clampedClimbSpeed = Mathf.Clamp(m_currentClimbSpeed, 0, m_characterConfig.maxClimbSpeed);
		m_rope.ChangeHoldLength(-clampedClimbSpeed * Time.fixedDeltaTime);
	}

	private void CancelRope(bool isPressed)
	{
		if (!HasRope) return;

		m_isCancellingRope = isPressed;

		if (m_isCancellingRope) 
		{
			m_cancelRopeTimer = 0f;
			m_cancelRopeCoroutine = StartCoroutine(StartCancellingRope());
		}
		else if (!m_isCancellingRope 
		&& m_cancelRopeCoroutine != null)
		{
			StopCoroutine(m_cancelRopeCoroutine);
		}
	}

	private IEnumerator StartCancellingRope()
	{
		while (m_cancelRopeTimer < m_characterConfig.cancelRopeDuration)
		{
			m_cancelRopeTimer += Time.deltaTime;
			yield return null;
		}
		DesequipRope();
	}

	private bool IsFallingWithRope()
	{
		// Assert: the character is falling if there is no more rope
		if (!m_rope) return true;

		bool isFalling = false;
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				isFalling = !m_isHolding;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				if (m_isHolding)
				{
					isFalling = true;
				}
				else
				{
					// If the character IS NOT holding the rope, let it fall till it reaches the rope limit constraint
					isFalling = (m_rope.CurrentFold - m_rsoCharacterPosition.value).magnitude < m_rope.HoldLength - k_fallingForcesThreshold;
				}
				break;
		}
		return isFalling;
	}

	private void DesequipRope()
	{
		m_rope.Detach();
		m_rope = null;
		m_isHolding = false;
	}

	private void ToggleRopeHolding(bool isEnabled)
	{
		ToggleRopeConstraint(isEnabled);

		if (!isEnabled)
		{
			m_positionStartFall = m_rsoCharacterPosition.value;

			ApplyFreeRopeForce(IsJumpingPressed
				? m_characterConfig.jumpOffRopeModifier
				: m_characterConfig.freeFallFromRopeModifier
			);
		}
	}

	private void ToggleRopeConstraint(bool isEnabled)
	{
		// Assertion
		if (!HasRope) return;

		if (isEnabled) m_rope.UpdateHoldLength();
		else m_rope.SetHoldLength(9999);

		if (m_rope.HoldLength == -1) DesequipRope(); // Handle error code 
	}

	private void ApplyFreeRopeForce(float modifier)
	{
		m_rigidbody.AddForce(m_rigidbody.velocity.magnitude * modifier * m_rigidbody.velocity.normalized, ForceMode.Impulse);
	}

	#endregion

	#region CRAFT STATE

	private void EnterCraftState()
	{
		if (m_craftType == CraftType.TORCH)
		{
			if (m_handObject != null
			&& m_handObject.Type != CraftType.TORCH
			&& m_robotObject?.Type != CraftType.TORCH)
			{
				Destroy(m_handObject.gameObject);
			}
			m_craftCoroutine = StartCoroutine(Craft(CraftType.TORCH, m_torchConfig.craftingDuration));
		}
		else if (m_craftType == CraftType.ROPE)
		{
			if (m_handObject != null)
			{
				if (m_handObject.Type == CraftType.TORCH)
				{
					SwitchObjects(ref m_handObject, ref m_robotObject, m_robotSocket);
				}
				else if (m_handObject.Type != CraftType.ROPE)
				{
					Destroy(m_handObject.gameObject);
				}
			}
			m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ropeConfig.CraftingDuration));
		}
	}

	private void FixedUpdateCraftState()
    {
		
    }

    private void ExitCraftState()
    {
		if (m_craftCoroutine != null)
		{
			StopCoroutine(m_craftCoroutine);
			m_craftCoroutine = null;

			m_rseBackpackCrafting.Call(false, -1);
		}

		m_isCrafting = false;
	}

	private void ToggleCraft(CraftType craftType, bool isInputPressed)
	{
		// Prevent switching to craft state if not in locomotion or crafting state or already crafting another item
		if (m_rsoCharacterState.value != BehaviorState.LOCOMOTION 
		|| m_rsoCharacterState.value != BehaviorState.CRAFT)
		{
			// If craft button is pressed
			if (isInputPressed)
			{
				m_craftType = craftType;
				m_isCrafting = true;
			}
			// If craft button is released
			else
			{
				m_isCrafting = false;
			}
		}
	}

	/// <summary>
	/// 	Instantiate the torch prefab after the fixed duration.
	/// </summary>
	private IEnumerator Craft(CraftType craftType, float duration)
	{
		m_rseBackpackCrafting.Call(true, duration);

		yield return new WaitForSeconds(duration);

		m_handObject = Instantiate(
			craftType == CraftType.TORCH ? (Permanent)m_torchConfig.pfTorch : (Permanent)m_ropeConfig.PfRope, 
			m_handSocket.transform.position,
			Quaternion.identity,
			m_handSocket.transform
		);

		m_rseBackpackCrafting.Call(false, -1);
		m_craftCoroutine = null;
	}

	private void ToggleAim(bool isInputPressed)
	{
		// Assert: can't throw null
		if (m_handObject == null) return;

		m_isAiming = isInputPressed;
		m_rsoCameraStyle.value = m_isAiming ? CameraStyle.AIMING : CameraStyle.BASIC;

		// Handle preview on input pressed
		if (m_isAiming)
		{
			m_handObject.InitializePreview();
		}

		// Handle pernament throw on input released
		else
		{
			// Assert: object can't be thrown
			if (!m_handObject.Throw(m_cameraMotor.transform)) return;

			// Exception: rope attachment
			m_rope = m_handObject as Rope;
			if (m_rope != null) m_rope?.Attach(m_harness, m_rigidbody);

			m_handObject = null;
			SwitchObjects(ref m_robotObject, ref m_handObject, m_handSocket);
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
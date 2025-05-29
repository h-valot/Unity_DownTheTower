using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public class CharacterMotor : MonoBehaviour
{
	#region REFERENCES

	[FoldoutGroup("Internal references")][SerializeField] private Rigidbody m_rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] private CapsuleCollider m_collider;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_handSocket;
	[FoldoutGroup("Internal references")][SerializeField] public Transform BagRobotSocket;
	[FoldoutGroup("Internal references")][SerializeField] public Transform BagCraftSocket;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_aimingLookTo;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_cameraTarget;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_harness;
	[FoldoutGroup("Internal references")][SerializeField] private CharacterGraphics m_characterGraphics;

	[FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ssoDeathLanding;
	[FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ssoDeathGuardian;
	[FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ssoDeathMushroom;
	[FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ssoRopeBlocked;
	[FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ssoRopeFree;

	[FoldoutGroup("SSO")][SerializeField] private SSO_Character m_ssoCharacter;
	[FoldoutGroup("SSO")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("SSO")][SerializeField] private SSO_Rope m_ssoRope;

	[FoldoutGroup("RSE")][SerializeField] private RSE_Move m_rseMove;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Jump m_rseJump;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Craft m_rseCraft;
	[FoldoutGroup("RSE")][SerializeField] private RSE_ThrowRope m_rseThrowRope;
    [FoldoutGroup("RSE")][SerializeField] private RSE_ThrowTorch m_rseThrowTorch;
    [FoldoutGroup("RSE")][SerializeField] private RSE_Run m_rseRun;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Climb m_rseClimb;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("RSE")][SerializeField] private RSE_ToggleHandObject m_rseToggleHandObject;
	[FoldoutGroup("RSE")][SerializeField] private RSE_KillCharacter m_rseKillCharacter;
	[FoldoutGroup("RSE")][SerializeField] private RSE_BackpackCrafting m_rseBackpackCrafting;
	[FoldoutGroup("RSE")][SerializeField] private RSE_SetCharacterPosition m_rseSetCharacterPosition;
	[FoldoutGroup("RSE")][SerializeField] private RSE_InitializeCamera m_rseInitializeCamera;
	[FoldoutGroup("RSE")][SerializeField] private RSE_DisplayDeath m_rseDisplayDeath;
	[FoldoutGroup("RSE")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("RSE")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;
	[FoldoutGroup("RSE")][SerializeField] private RSE_StopSound m_rseStopSound;

	[FoldoutGroup("RSO")][SerializeField] private RSO_CancelConsumable m_rsoCancelConsumable;
	[FoldoutGroup("RSO")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("RSO")][SerializeField] private RSO_MovementDatas m_rsoMovementDatas;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[FoldoutGroup("RSO")][SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CancelPriority m_rsoCancelPriority;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CameraForward m_rsoCameraForward;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CameraRight m_rsoCameraRight;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CameraTransform m_rsoCameraTransform;
	[FoldoutGroup("RSO")][SerializeField] private RSO_HarnessPosition m_rsoHarnessPosition;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterLastPosition m_rsoCharacterLastPosition;
	[FoldoutGroup("RSO")][SerializeField] private RSO_Ropes m_rsoRopes;
	[FoldoutGroup("RSO")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("RSO")][SerializeField] private RSO_Pause m_rsoPause;
    [FoldoutGroup("RSO")][SerializeField] private RSO_CharacterElevator m_rsoCharacterElevator;

	#endregion

	#region VARIABLES

	private BehaviorState m_previousState;
	private bool m_isInitialize;
	private bool m_isMotorLocked;
	private bool IsMotorLocked
	{
		get => m_isMotorLocked;
		set
		{
			m_isMotorLocked = value;
			OnMotorLockedChanged(value);
		}
	}

	// - Inputs -
	private Vector2 m_moveInput = new Vector2();
	public bool DoMoveInputs => m_moveInput.magnitude > m_ssoCharacter.MoveAnalogStart;

	// - Collisions -
	private RaycastHit[] m_raycastHits;

    // - Ground -
	[HideInInspector] public bool m_isGrounded;
	private Vector3 m_groundNormal;
    private float m_coyoteTime;
	private Vector3 m_positionStartFall;
	private float m_fallHeight;
	private bool m_isCharacterDead;

	// - Movement -
	private Vector2 m_planarVelocity;
	private float m_maxGroundedSpeed;
	private bool m_isJumping;
	private bool m_jumpStartedFromGround;
	[HideInInspector] public bool m_hasJumped;
	private float m_desiredForce;
	private bool m_isCrafting;
	private float m_airControlTimeScalar;
	private float m_airControlDuration;

	// - Fall state - 
	private float m_stuckYTimer;

	// - Rope state -
	private Rope m_rope;
	private bool m_isHolding;
	public Rigidbody Rigidbody => m_rigidbody;
	public Transform Harness => m_harness;
	public bool IsHolding => m_isHolding;
	private float m_ropeDragTimer;
	private bool m_withinRopeLimit;
	private bool m_isClimbing;
    public bool IsClimbing => m_isClimbing;
    private float m_currentClimbSpeed;
	public bool IsRopeValid => m_rope && m_rope.IsPlaced;
	private const float k_fallingForcesThreshold = 0.2f;

	// - Craft state -
	private Coroutine m_craftCoroutine;
	private float m_craftTimer;
	public bool m_startAiming;
	public bool IsBalancing;
	[HideInInspector] public Permanent HandObject;
	[HideInInspector] public Permanent RobotObject;
	[HideInInspector] public bool IsAiming;
	private Permanent AimingObject;

	#endregion

	#region MONOBEHAVIOR

	public void Initialize(Vector3 position, Quaternion rotation)
    {
        // Update drag in rigidbody if changed in characterConfig
        m_ssoCharacter.OnSSOChanged += UpdateDrag;

		m_rseInitializeCamera.Call(m_aimingLookTo, m_cameraTarget, rotation);
        m_characterGraphics.Initialize(m_aimingLookTo, m_rigidbody, rotation);

		m_rigidbody.position = Vector3.zero;
		m_rsoCraftInputLocked.value = false;
		m_rsoRecycleInputLocked.value = false;
		SetCharacterPosition(position, rotation);

		m_isCrafting = false;
		m_isCharacterDead = false;

		m_isInitialize = true;
    }

    private void OnEnable()
    {
        CheckGround();
        DetermineState();

		Application.onBeforeRender += UpdatePreview;
		m_rsoPause.OnChanged += OnPaused;

        m_rsoCharacterElevator.value = false;
    }

    private void OnDisable()
    {
        UnsubscibeAllInputs();
        m_ssoCharacter.OnSSOChanged -= UpdateDrag;
        m_rsoCharacterState.value = BehaviorState.NONE;

        Application.onBeforeRender -= UpdatePreview;
		m_rsoPause.OnChanged -= OnPaused;
	}

	private void FixedUpdate()
	{
		if (!m_isInitialize) return;
		if (IsMotorLocked) return;

        // DINGUERIE: Prevent the character to soft lock the ground detection
        if (m_rigidbody.position == Vector3.zero) m_rigidbody.position = new Vector3(0.01f, 0f, 0f);

		CheckGround();
		DetermineState();
        FixedUpdateState();
		RefillTools();

		// Update useful variables
		if (m_rsoCharacterLastPosition.value != m_rsoCharacterPosition.value) m_rsoCharacterLastPosition.value = m_rsoCharacterPosition.value;
		if (m_rsoCharacterPosition.value != m_rigidbody.position) m_rsoCharacterPosition.value = m_rigidbody.position;
		m_rsoHarnessPosition.value = m_harness.position;
		m_planarVelocity = new Vector2(m_rigidbody.linearVelocity.x, m_rigidbody.linearVelocity.z);
		if (m_isGrounded && m_maxGroundedSpeed < m_planarVelocity.magnitude) m_maxGroundedSpeed = m_planarVelocity.magnitude;
		if (m_rsoInputsLocked.value) m_moveInput = new Vector2(0f, 0f);
	}

    private void LateUpdate()
    {
		MovementDatas _movementDatas = new MovementDatas();
        _movementDatas.dataToString.Add(m_desiredForce.ToString());
        _movementDatas.dataToString.Add(m_isGrounded.ToString());
        _movementDatas.dataToString.Add(m_rsoCharacterState.value.ToString());
		_movementDatas.dataToString.Add(m_rigidbody.position.ToString());
		m_rsoMovementDatas.value = _movementDatas;
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
    {
        if (m_raycastHits != null)
		{
			Gizmos.color = Color.cyan;
			foreach (var hit in m_raycastHits)
            {
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
		}

		if (IsRopeValid)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(m_rope.CurrentFold.Position, m_rope.HoldLength);
		}
	}

#endif

    #endregion

    #region INPUTS

    private void UnsubscibeAllInputs()
	{
		m_rseSetCharacterPosition.action -= SetCharacterPosition;
		m_rseKillCharacter.action -= HandleDeath;
		m_rseJump.action -= UpdateJumpInput;

		m_rseMove.action -= UpdateMoveInput;
        m_rseThrowRope.action -= UpdateHoldInput;
		m_rseJump.action -= JumpGround;
		m_rseJump.action -= JumpRope;
		m_rseThrowRope.action -= ToggleRopeAim;
        m_rseThrowTorch.action -= ToggleTorchAim;
        m_rseClimb.action -= UpdateClimbInput;
		m_rseCancel.action -= CancelAction;
		m_rseToggleHandObject.action -= ToggleTorch;
	}

    private void SubscribeStateInputs()
	{
		m_rseSetCharacterPosition.action += SetCharacterPosition;
		m_rseKillCharacter.action += HandleDeath;
		m_rseToggleHandObject.action += ToggleTorch;
		m_rseJump.action += UpdateJumpInput;

		switch (m_rsoCharacterState.value)
        {
            case BehaviorState.LOCOMOTION:
				m_rseMove.action += UpdateMoveInput;
                m_rseThrowRope.action += UpdateHoldInput;
				m_rseJump.action += JumpGround;
				m_rseClimb.action += UpdateClimbInput;
				m_rseThrowRope.action += ToggleRopeAim;
                m_rseThrowTorch.action += ToggleTorchAim;
                m_rseCancel.action += CancelAction;
				break;

			case BehaviorState.ROPE:
				m_rseMove.action += UpdateMoveInput;
                m_rseThrowRope.action += UpdateHoldInput;
				m_rseThrowRope.action += ToggleRopeAim;
                m_rseThrowTorch.action += ToggleTorchAim;
                m_rseJump.action += JumpRope;
				m_rseClimb.action += UpdateClimbInput;
				m_rseCancel.action += CancelAction;
				break;

			case BehaviorState.FALL:
                m_rseMove.action += UpdateMoveInput;
				m_rseThrowRope.action += ToggleRopeAim;
                m_rseThrowTorch.action += ToggleTorchAim;
                m_rseCancel.action += CancelAction;
                break;
        }
	}

	public void SetCharacterPosition(Vector3 position, Quaternion rotation)
	{
		m_positionStartFall = position;
		m_rigidbody.linearVelocity = Vector3.zero;
		m_rigidbody.position = position;
		m_characterGraphics.transform.rotation = rotation;
		m_rsoCharacterPosition.value = m_rigidbody.position;
		ResetInputValues();
	}

	/// <summary>
	/// Subscribe or unsubscribe inputs listener when the game paused. 
	/// </summary>
	private void OnPaused()
	{
		m_moveInput = Vector2.zero;
		OnMotorLockedChanged(m_rsoPause.value);
	}

	private void OnMotorLockedChanged(bool isMotorLocked)
	{
		if (isMotorLocked)
		{
			UnsubscibeAllInputs();
			ResetInputValues();
		}
		else
		{

			SubscribeStateInputs();
		}
	}

	private void ResetInputValues()
	{
		m_isHolding = false;
		m_isClimbing = false;
		m_isJumping = false;
	}

	private void UpdateMoveInput(Vector2 input)
	{
		if (m_rsoInputsLocked.value) return;

		m_moveInput = input;
	}

	private void UpdateHoldInput(bool isHolding)
	{
		if (m_rsoInputsLocked.value) return;
		
		if (!IsRopeValid
		|| m_withinRopeLimit)
		{
			m_isHolding = false;
			return;
		}

		if (!isHolding && m_isGrounded)
		{
			isHolding = true;
			ToggleRopeConstraint(false);
		}

		ToggleRopeConstraint(!isHolding);
		m_isHolding = isHolding;
    }

	private void ToggleTorch(bool isPressed)
	{
		// Assertion
		if (m_rsoInputsLocked.value) return;
		if (!(RobotObject as Torch)) return;
        if (!isPressed) return;

        ((Torch)RobotObject)?.ToggleHandEffect();
	}

	private void UpdateClimbInput(bool isClimbing)
	{
		// Assertion
		if (m_rsoInputsLocked.value) return;
		if (!IsRopeValid) return;

		m_isClimbing = isClimbing;
		if (m_isGrounded) m_rope.UpdateHoldLength(isClimbing);

		// Prevent the rope constraint to still being applied if the character is in the LOCOMOTION state
		if (!isClimbing
		&& m_rsoCharacterState.value == BehaviorState.LOCOMOTION
		&& m_rope.IsConstrained)
		{
			ToggleRopeConstraint(false);
		}
	}

	private void CancelAction(bool isPressed)
	{
		if (m_rsoInputsLocked.value) return;
		if (m_rsoCancelPriority.value != CancelState.IN_GAME) return;
		if (!m_rsoCancelConsumable.value) return;
		if (!isPressed) return;

		// Consume cancel input
		m_rsoCancelConsumable.value = false;

		if (m_startAiming)
		{
			CancelAim();
			return;
		}

        if (IsRopeValid) 
		{
			DesequipRope();

			// Apply planar force jump boost
			float force = m_ssoCharacter.JumpOffRopeModifier * m_rigidbody.linearVelocity.magnitude;
			Vector3 direction = (m_rsoCameraRight.value * m_moveInput.x + m_rsoCameraForward.value * m_moveInput.y).normalized;
			direction = new Vector3(direction.x, 0, direction.z);
			m_rigidbody.AddForce(direction * force, ForceMode.Impulse);
		}
	}

	private void UpdateJumpInput(bool isPressed)
	{
		if (m_rsoInputsLocked.value) return;
		m_isJumping = isPressed;
	}

	/// <summary>
	/// Add a vertical impulse to the character.
	/// </summary>
	private void JumpGround(bool isPressed)
	{
		// Assertions
		if (m_rsoInputsLocked.value) return;
		if (!isPressed) return;
		if (m_hasJumped) return;
		if (m_rsoInputsLocked.value) return;

		if ((m_rsoCharacterState.value == BehaviorState.FALL
		|| m_rsoCharacterState.value == BehaviorState.ROPE)
		&& m_coyoteTime < 0f) 
		{
			return;
		}

		m_rigidbody.AddForce(Vector3.up * m_ssoCharacter.JumpForce, ForceMode.Impulse);
		m_hasJumped = true;
		m_jumpStartedFromGround = true;
		StartCoroutine(JumpConfirmDelay());
	}

	IEnumerator JumpConfirmDelay()
	{
		yield return new WaitForSeconds(0.1f);
		JumpConfirm();
	}

	private void JumpConfirm()
	{
		if (m_isGrounded)
		{
			m_hasJumped = false;
		}
	}

	private void JumpRope(bool isPressed)
	{
		// Assertions
		if (m_rsoInputsLocked.value) return;
		if (!IsRopeValid) return;
		if (!m_jumpStartedFromGround) return;

		m_jumpStartedFromGround = false;
		UpdateHoldInput(isPressed);
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
        else if (m_rsoCharacterState.value != BehaviorState.FALL && !m_isGrounded && !IsRopeValid)
        {
            SwitchState(BehaviorState.FALL);
        }
        else if (m_rsoCharacterState.value != BehaviorState.ROPE && !m_isGrounded && IsRopeValid)
        {
            SwitchState(BehaviorState.ROPE);
        }
    }

    /// <summary>
    /// Switch to new state by triggering old state exit then new state enter
    /// </summary>
    /// <param name="newState">New state to switch to</param>
    private void SwitchState(BehaviorState newState)
    {
		m_previousState = m_rsoCharacterState.value;
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

            case BehaviorState.ROPE:
                ExitRopeState();
                break;
		}
	}

	#endregion

	#region DEATH

	public void HandleDeath(DeathType type)
	{
		if (m_isCharacterDead) return;

		m_isCharacterDead = true;

		if (IsRopeValid) DesequipRope();

		// Remove objects from lists before destroying the character
		if (HandObject as Rope) m_rsoRopes.value.Remove(HandObject as Rope);
		if (AimingObject as Rope) m_rsoRopes.value.Remove(AimingObject as Rope);
		if (RobotObject as Torch) m_rsoTorchManager.value.Remove(RobotObject as Torch);
		if (AimingObject as Torch) m_rsoTorchManager.value.Remove(AimingObject as Torch);

		// Disable character motor
		IsMotorLocked = true;

		m_rseDisplayDeath.Call(true);
		m_characterGraphics.SpawnRagdoll(IsCarryingLight(), BagRobotSocket ? BagRobotSocket.transform.position : Vector3.zero);

		switch (type)
		{
			case DeathType.GUARDIAN :
                m_rsePlaySound.Call(m_ssoDeathGuardian);
				StartCoroutine(AnimateDefaultDeath());
                break;

			case DeathType.HEIGHT:
                m_rsePlaySound.Call(m_ssoDeathLanding);
                StartCoroutine(AnimateDefaultDeath());
                break;

            case DeathType.GAS:
                m_rsePlaySound.Call(m_ssoDeathMushroom);
				StartCoroutine(AnimateGasDeath());
                break;
		}
	}

	public IEnumerator AnimateDefaultDeath()
	{
		yield return new WaitForSeconds(m_ssoCharacter.FallDeathDurationBeforeRespawn);

		m_rsoCharacterDeath.value = true;
		Destroy(gameObject);
	}

	public IEnumerator AnimateGasDeath()
	{
		// TODO Disable all inputs
		// TODO Blur and fade out
		// TODO Slow character's speed down to zero

		yield return new WaitForSeconds(m_ssoCharacter.FallDeathDurationBeforeRespawn);

		m_rsoCharacterDeath.value = true;
		Destroy(gameObject);
	}

	#endregion

	#region GROUND

	private void CheckGround()
    {
        m_isGrounded = false;
        m_groundNormal = Vector3.down;

        Vector3 start = transform.position + Vector3.up * m_collider.height;
        float radius = m_collider.radius + m_ssoCharacter.SkinWidth;
        Vector3 direction = Vector3.down;
        float distance = m_collider.height;
        m_raycastHits = Physics.SphereCastAll(start, radius, direction, distance, m_ssoCharacter.GroundLayerToInclude);

        foreach (var hit in m_raycastHits)
        {
            // Assert: Exclude hit point that come from the spherecast spawning inside a collider
            if (hit.point == Vector3.zero) continue;

            // Check if it is on the bottom round part of the capsule
            if (hit.point.y < transform.position.y + m_collider.radius
			&& Vector3.Angle(hit.normal, Vector3.up) < 46f)
			{
				m_isGrounded = true;

				// Take the smallest normal from ground check as the new ground normal
				if (Vector3.Dot(hit.normal, Vector3.up) > Vector3.Dot(m_groundNormal, Vector3.up))
				{
					m_groundNormal = hit.normal;
				}
			}
        }
    }
	
	private void ApplyFallHeight()
	{
		//assertion
        if (m_rsoCharacterElevator.value == true) return;

        m_fallHeight = (m_rigidbody.position.y - m_positionStartFall.y) * -1f;
		if (m_fallHeight >= m_ssoCharacter.LethalHeight)
		{
			HandleDeath(DeathType.HEIGHT);
		}
	}

	private void StartCoyoteTime()
    {
        if (!m_hasJumped)
        {
            m_coyoteTime = m_ssoCharacter.CoyoteTime;
            m_rseJump.action += JumpGround;
        }
    }

    private void UpdateCoyoteTime()
    {
        if (m_hasJumped || m_coyoteTime <= 0)
        {
            m_rseJump.action -= JumpGround;
            return;
        }

        m_coyoteTime -= Time.fixedDeltaTime;
    }

    #endregion    
    
    #region MOVEMENT

	/// <summary>
	/// Update frictions based on the character locomotion status.
	/// Used to stop character movement if move inputs are null.
	/// </summary>
	private void UpdateFriction()
    {
        if (!DoMoveInputs && m_isGrounded)
        {
            m_collider.sharedMaterial.dynamicFriction = m_ssoCharacter.FrictionDeceleration;
            m_collider.sharedMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;
        }
        else
        {
            m_collider.sharedMaterial.dynamicFriction = 0;
            m_collider.sharedMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        }
    }

    /// <summary>
    /// Update drag based on behavior state to allow the player to fall faster.
    /// </summary>
    private void UpdateDrag()
    {
        if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION)
        {
            m_rigidbody.linearDamping = m_ssoCharacter.DragGround;
        }
        else if (m_rsoCharacterState.value == BehaviorState.FALL || m_rsoCharacterState.value == BehaviorState.ROPE)
        {
            m_rigidbody.linearDamping = 0;
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
        UpdateFriction();

		// Assertion
        if (!DoMoveInputs) return;

		Vector3 desiredDirection = (m_rsoCameraRight.value * m_moveInput.x + m_rsoCameraForward.value * m_moveInput.y).normalized;

		// Orient speed along slope
		Vector3 slopeRight = Vector3.Cross(Vector3.up, m_groundNormal);
		desiredDirection = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, m_groundNormal, slopeRight), slopeRight) * desiredDirection;

		// Set desired speed magnitude based on walk/run state
        Vector2 desiredForcev2 = new Vector2(
			Mathf.Clamp01(Mathf.Abs(m_moveInput.x) - m_ssoCharacter.MoveAnalogStart) / (1 - m_ssoCharacter.MoveAnalogStart),
            Mathf.Clamp01(Mathf.Abs(m_moveInput.y) - m_ssoCharacter.MoveAnalogStart) / (1 - m_ssoCharacter.MoveAnalogStart)
		);
        desiredForcev2 = Mathf.Clamp(desiredForcev2.magnitude, 0, 1) * desiredForcev2.normalized;
        float desiredForce = desiredForcev2.magnitude * m_ssoCharacter.MaxMoveForce;
        m_desiredForce = desiredForce;

		// Apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
		m_rigidbody.AddForce(desiredDirection * desiredForce - m_rigidbody.linearVelocity, ForceMode.Acceleration);
    }

    /// <summary>
    /// (1) Check if there is valid points to step on
    /// (2) Select the highest point among the point in front the character
    /// (3) On the selected point, check if there is really a object to step on if front
    /// (4) Check if there there is a flat surface to step onto (<45 degrees)
    /// </summary>
    private void HandleStepOn()
    {
		// Assertion
        if (m_raycastHits.Length <= 1 || !DoMoveInputs) return;
		
		Vector3 stepOnTarget = transform.position;
		Vector3 moveInput3D = (m_rsoCameraRight.value * m_moveInput.x + m_rsoCameraForward.value * m_moveInput.y).normalized;

		foreach (var hit in m_raycastHits)
		{
			Vector3 hitDirection = hit.point - transform.position;
			hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z);

			// Check if hit is in front of character
			if (Vector3.Dot(moveInput3D, hitDirection) > 0.15)
			{
				if (hit.point.y - transform.position.y < m_ssoCharacter.StepOnHeight)
				{
					// We take the highest that is higher than skin width to not trigger step on very small objects
					if (hit.point.y > stepOnTarget.y 
					&& hit.point.y > transform.position.y + m_ssoCharacter.SkinWidth)
					{
						stepOnTarget = hit.point;
					}
				}
			}
		}

		if (stepOnTarget != transform.position)
		{
			// Check if there is really an object to step on in the speed direction, to prevent steping on end of slope
			Vector3 start = new Vector3(m_rigidbody.position.x, m_rigidbody.position.y + m_ssoCharacter.SkinWidth, m_rigidbody.position.z);
			Vector3 direction = m_rigidbody.linearVelocity.normalized;
			float distance = m_collider.radius * 2;
			if (Physics.Raycast(start, direction, distance, m_ssoCharacter.GroundLayerToInclude))
			{
				// Check if there is a flat surface to step on (<45 degrees)
				start = stepOnTarget + (new Vector3(stepOnTarget.x, 0, stepOnTarget.z) - new Vector3(m_rigidbody.position.x, 0, m_rigidbody.position.z)).normalized * m_ssoCharacter.SkinWidth + new Vector3(0, m_ssoCharacter.SkinWidth, 0);
				direction = Vector3.down;
				distance = m_ssoCharacter.SkinWidth * 2;
				if (Physics.Raycast(start, direction, distance, m_ssoCharacter.GroundLayerToInclude))
				{
					m_rigidbody.position = new Vector3(m_rigidbody.position.x, stepOnTarget.y, m_rigidbody.position.z);
				}
			}
		}
    }

    private void UpdatePreview()
    {
		if (IsAiming) AimingObject.PreviewThrow(m_rsoCameraTransform.value);
    }

    #endregion

    #region LOCOMOTION STATE

    private void EnterLocomotionState()
    {
        UpdateDrag();
		ApplyFallHeight();
        m_rseStopSound.Call(m_ssoRopeFree);
    }

    private void FixedUpdateLocomotionState()
	{
		HandleStepOn();
        MoveGrounded();

		// - Handle rope while grounded -
		if (!IsRopeValid) return;

		if (m_rope.GetTotalLength() >= m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset - 0.5f)
		{
			DesequipRope();
			return;
		}

		HandleRopeClimb();
	}

    private void ExitLocomotionState()
    {
		m_positionStartFall = m_rigidbody.position;
	}

	#endregion

	#region FALL STATE

	private void EnterFallState()
	{
		// Exception: Prevent the character from switching to rope or fall state while aiming 
		if (IsAiming) CancelAim();

		UpdateDrag();
        UpdateFriction();
        StartCoyoteTime();
		StartAirControl();
	}

    private void FixedUpdateFallState()
    {
        UpdateCoyoteTime();
		UpdateAirControl();
		UpdateZeroDrag();
		CheckFallHeight();
		CheckStuckY();
		MoveFalling();
	}

    private void ExitFallState()
    {
        m_hasJumped = false;
	}

	private void MoveFalling()
	{
		// Assertion
		if (!DoMoveInputs) return;

		Vector3 desiredDirection = (m_rsoCameraRight.value * m_moveInput.x + m_rsoCameraForward.value * m_moveInput.y).normalized;
		float desiredForce = Mathf.Clamp(m_planarVelocity.magnitude, 3f, m_planarVelocity.magnitude) * m_ssoCharacter.AirControlScalar;

		if (m_planarVelocity.magnitude >= m_maxGroundedSpeed * m_ssoCharacter.AirControlThreshold)
		{
			m_rigidbody.AddForce(-m_rigidbody.linearVelocity, ForceMode.Acceleration);
			return;
		}

		m_rigidbody.AddForce(
			(desiredDirection * desiredForce - m_rigidbody.linearVelocity) * m_airControlTimeScalar,
			ForceMode.Acceleration
		);
	}

	private void CheckStuckY()
	{
		if (m_rsoCharacterLastPosition.value.CutDigits(2) != m_rsoCharacterPosition.value.CutDigits(2))
		{
			m_stuckYTimer = m_ssoCharacter.StuckYDuration;
			return;
		}

		m_stuckYTimer -= Time.fixedDeltaTime;
		if (m_stuckYTimer < 0)
		{
			// Respawn character
			m_rseDisplayDeath.Call(false);
			m_rsoCharacterDeath.value = true;
			Destroy(gameObject);
		}
	}

	private void StartAirControl()
	{
		m_airControlDuration = m_ssoCharacter.AirControlDuration;
		m_airControlTimeScalar = 1f;
	}


	private void UpdateAirControl()
	{
		if (m_airControlDuration < 0)
		{
			m_airControlTimeScalar = 0f;
			return;
		}

		m_airControlDuration -= Time.fixedDeltaTime;
		m_airControlTimeScalar = m_airControlDuration / m_ssoCharacter.AirControlDuration;
	}

	private void UpdateZeroDrag()
	{
		float modifier = 1 - Time.fixedDeltaTime * m_ssoCharacter.ZeroDragScalar;
		Vector3 velocity = transform.InverseTransformDirection(m_rigidbody.linearVelocity);
		velocity.x *= modifier;
		velocity.z *= modifier;
		m_rigidbody.linearVelocity = transform.TransformDirection(velocity);
	}

	/// <summary>
	/// Handle fall death animation when the distance the character travelled on the y-axis exceed the lethal height. 
	/// </summary>
	private void CheckFallHeight()
	{
		// Assertions
		if (m_rsoCharacterState.value != BehaviorState.FALL) return;
		if (m_rsoCharacterElevator.value == true) return;

		m_fallHeight = Math.Abs(m_rigidbody.position.y - m_positionStartFall.y);
		if (m_fallHeight >= m_ssoRope.MaxLength + m_ssoCharacter.LethalHeight * 2f)
		{
			HandleDeath(DeathType.HEIGHT);
		}
	}

	#endregion

	#region ROPE STATE

	// TODO - In partial suspension, make the character able to jump off the wall
	// TODO - In partial suspension, make the character unable to move while off the wall
	// TODO - In partial suspension, make the character unable to be snap against a cambered wall 
	// TODO - In complete suspension, make the character pivot with the rope inclination
	// TODO - Lerp the rope stop deceleration

	private void EnterRopeState()
	{
		EnterFallState();

        m_rsePlaySound.Call(m_ssoRopeFree);

        if (!m_isClimbing
		&& !m_isJumping)
		{
			ToggleRopeConstraint(!m_isHolding);

			// If the character is falling and attached to a rope, we won't it to be slacken.
			// Handle rope extention within the limit of the current rope.
			if (m_previousState != BehaviorState.FALL
			&& m_rope.GetTotalLength() + m_ssoCharacter.EntranceOffset <= m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset)
			{
				m_rope.IncreaseHoldLength(m_ssoCharacter.EntranceOffset);
			}
		}
	}

	private float m_jumpRopeDelay;
	private void FixedUpdateRopeState()
	{
		if (m_rope.GetTotalLength() > m_ssoRope.MaxLength)
		{
			DesequipRope();
			return;
		}

		// Prevent the character from dying while on the rope.
		m_positionStartFall = m_rigidbody.position;

		UpdateJumpRopeDelay();

		if (IsFallingWithRope())
		{
			FixedUpdateFallState();
			return;
		}

		HandleRopeMovement();
		HandleRopeDrag();
		HandleRopeClimb();
		HandleRopeLimit();
	}

	private void ExitRopeState()
	{
		ToggleRopeConstraint(false);
		m_isHolding = false;
		ExitFallState();
	}

	private void HandleRopeMovement()
	{
		Vector3 direction =
			(Vector3Extention.GetPositionOnCercle(
				angle: m_ssoCharacter.RopeOffsetAngle,
				axis: m_cameraTarget.forward,
				direction: m_cameraTarget.right,
				origin: m_rope.CurrentFold.Position,
				radius: m_rope.HoldLength,
				starting: m_rigidbody.position
			) - m_rigidbody.position).normalized * m_moveInput.x +
			(Vector3Extention.GetPositionOnCercle(
				angle: m_ssoCharacter.RopeOffsetAngle,
				axis: m_cameraTarget.right,
				direction: m_cameraTarget.forward,
				origin: m_rope.CurrentFold.Position,
				radius: m_rope.HoldLength,
				starting: m_rigidbody.position
			) - m_rigidbody.position).normalized * m_moveInput.y;

		m_rigidbody.AddForce(direction * m_ssoCharacter.ropeMovementForce, ForceMode.Acceleration);
	}
	
	private void HandleRopeDrag()
	{
		// Assertion
		if (m_moveInput.magnitude > m_ssoCharacter.MoveMagnitudeApplyDragThreshold) 
		{
			m_rigidbody.linearDamping = 0;
			m_ropeDragTimer = m_ssoCharacter.RopeDragDuration;
			IsBalancing = true;
            return;
		}

        IsBalancing = false;
        m_ropeDragTimer -= Time.fixedDeltaTime;
		m_rigidbody.linearDamping = (1 - m_ropeDragTimer / m_ssoCharacter.RopeDragDuration) * m_ssoCharacter.RopeDrag;

	}

	private float k_climbingThreshold = 0.6f;
	private void HandleRopeClimb()
	{
		// Assertions
		if (!IsRopeValid
		|| !m_isClimbing
		|| m_isHolding
		|| m_rope.HoldLength < k_climbingThreshold)
		{
			m_currentClimbSpeed = m_ssoCharacter.ClimbAcceleration;
			if (m_rope.HoldLength < k_climbingThreshold)
			{
				m_isJumping = false;
				m_isClimbing = false;
				m_isHolding = false;
			}
			return;
		}

		if (!m_rope.IsConstrained) ToggleRopeConstraint(true);

		m_positionStartFall = m_rigidbody.position;
		m_currentClimbSpeed += m_currentClimbSpeed * Time.fixedDeltaTime;
		float clampedClimbSpeed = Mathf.Clamp(m_currentClimbSpeed, 0, m_ssoCharacter.MaxClimbSpeed);

		// Decreasing rope holding length
		m_rope.IncreaseHoldLength(-clampedClimbSpeed * Time.fixedDeltaTime);
		
		HandleEdgeCatching();
	}

	private void HandleEdgeCatching()
	{
		// Assertions
		if (m_raycastHits.Length <= 0) return;
		if (m_coyoteTime > 0) return;

		// - Get a highest position than the character's one -
		Vector3 candidatePos = m_rigidbody.position;
		foreach (var hit in m_raycastHits)
		{
			// Assert: The hit.point is lower than the candidate position
			if (hit.point.y < candidatePos.y) continue;

			// Assert: The hit.point is too far from the character's position
			if ((m_rigidbody.position - hit.point).magnitude > m_ssoCharacter.EdgeCatchingThreshold) continue;

			// Assert: There is not enough space above the hit.point
			if (Physics.Raycast(hit.point, Vector3.up, m_collider.height, m_ssoCharacter.GroundLayerToInclude)) continue;

			// Assert: There is not enough space around the hit.point
			if (Physics.SphereCast(hit.point + Vector3.up * m_collider.height, m_collider.radius, Vector3.down, out var sphereCastHit, m_collider.height * 0.75f, m_ssoCharacter.GroundLayerToInclude)) 
			{
				// Everything can block the character except the rope and itself
				if (!sphereCastHit.collider.TryGetComponent<Rope>(out var rope)
				&& sphereCastHit.collider.TryGetComponent<CharacterMotor>(out var character))
				{
					continue;
				}
			}

			candidatePos = hit.point;
		}

		// - Override the character's position -
		if (candidatePos != m_rigidbody.position)
		{
			StartCoroutine(HaulToPosition(candidatePos));
		}
	}

	private IEnumerator HaulToPosition(Vector3 position)
	{
		if (IsMotorLocked) yield break;

		IsMotorLocked = true;
		m_rope.IsFoldSystemDisabled = true;

		// Move the character up towards a y-axis alignment
		transform.DOMoveY(position.y, m_ssoCharacter.EdgeCatchingDuration).SetEase(Ease.OutCubic);
		
		// Move the character forwards the accurate planar (x, z) position
		Vector3 destinationDir = (position - m_rigidbody.position).normalized;
		destinationDir = new Vector3(destinationDir.x, 0, destinationDir.z);
		transform.DOMove(position + destinationDir * m_ssoCharacter.EdgeCatchingOffset, m_ssoCharacter.EdgeCatchingDuration).SetEase(Ease.InCubic);

		yield return new WaitForSeconds(m_ssoCharacter.EdgeCatchingDuration);

		Physics.SyncTransforms();
		m_rope.RemoveInvalidFolds();

		IsMotorLocked = false;
		m_rope.IsFoldSystemDisabled = false;
	}

	private void HandleRopeLimit()
	{
		// Assertion
		if (m_rope.GetTotalLength() < m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset
		|| m_isClimbing)
		{
			m_withinRopeLimit = false;
			return;
		}

		m_isHolding = false;
		m_withinRopeLimit = true;
		m_rope.SetHoldLength(m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset - m_rope.GetFixedLength());
	}

	private void UpdateJumpRopeDelay()
	{
		if (m_jumpRopeDelay >= 0) 
		{
			m_jumpRopeDelay -= Time.fixedDeltaTime;
		}
	}

	private bool IsFallingWithRope()
	{
		// Assert: the character is falling if there is no more rope
		if (!m_rope) return true;

		bool isFalling;
		if (m_isHolding)
		{
			isFalling = true;
            if (!m_isGrounded) m_rsePlaySound.Call(m_ssoRopeFree);
        }
		else
		{
			// If the character IS NOT holding the rope, let it fall till it reaches the rope limit constraint
			isFalling = (m_rope.CurrentFold.Position - m_rigidbody.position).magnitude < m_rope.HoldLength - k_fallingForcesThreshold;
		}
		return isFalling;
	}

	public void Equip(Rope rope)
	{
		m_rope = rope;
	}

	private void DesequipRope()
	{
		m_rope.Detach();
		m_rope = null;
		m_isHolding = false;
		m_rseStopSound.Call(m_ssoRopeFree);
	}

	private void ToggleRopeConstraint(bool isEnabled)
	{
		// Assertion
		if (!IsRopeValid) return;

		if (isEnabled)
		{
			m_rope.IsConstrained = true;
			m_positionStartFall = m_rigidbody.position;
			m_rope.UpdateHoldLength();
			if (m_rope.HoldLength == -1) DesequipRope(); // Handle error code
			m_rsePlaySound.Call(m_ssoRopeBlocked);
			m_rseStopSound.Call(m_ssoRopeFree);
		}
		else
		{
			m_rope.IsConstrained = false;
			m_rope.SetHoldLength(m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset - m_rope.GetFixedLength());
			if (!m_isGrounded) m_rsePlaySound.Call(m_ssoRopeFree);
		}
	}

	#endregion

	#region CRAFTING

	/// <summary>
	/// Instantiate the torch prefab after the fixed duration.
	/// </summary>
	private IEnumerator Craft(CraftType craftType, float duration)
    {
        m_rseBackpackCrafting.Call(true, duration);

        m_craftTimer = duration;

		while(m_craftTimer> 0)
		{
			m_craftTimer -= Time.deltaTime;
			yield return null;
		}

		switch (craftType)
		{
			case CraftType.TORCH:
                RobotObject = Instantiate(
                    (Permanent)m_ssoTorch.PfTorch,
                    BagRobotSocket.transform.position,
                    Quaternion.identity,
                    BagRobotSocket.transform
                );
                break;
			case CraftType.ROPE:
                HandObject = Instantiate(
                    (Permanent)m_ssoRope.PfRope,
                    BagCraftSocket.transform.position,
                    BagCraftSocket.rotation,
					BagCraftSocket.transform
                );
				((Rope)HandObject).IsOnBackpack = true;
				HandObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;
		}

		m_rseBackpackCrafting.Call(false, duration);
		m_craftCoroutine = null;
    }

	private void RefillTools()
	{
		if (RobotObject == null && m_craftCoroutine == null) m_craftCoroutine = StartCoroutine(Craft(CraftType.TORCH, m_ssoTorch.CraftingDuration));
        if (HandObject == null && m_craftCoroutine == null) m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ssoRope.CraftingDuration));
    }

	private void ToggleTorchAim(bool isInputPressed)
    {
		// Assertions 
		if (m_startAiming && isInputPressed) return;
		if (m_startAiming && AimingObject != RobotObject) return; 

        ToggleAim(isInputPressed, RobotObject);
	}

	private void ToggleRopeAim(bool isInputPressed)
	{
		// Assertions
		if (m_rsoInputsLocked.value) return;
		if (m_startAiming && isInputPressed) return;
        if (m_startAiming && AimingObject != HandObject) return;
        if (m_rsoCharacterState.value == BehaviorState.ROPE) return;
		if (m_rsoCharacterState.value == BehaviorState.FALL) return;

        ToggleAim(isInputPressed, HandObject);
	}

	private void ToggleAim(bool isInputPressed, Permanent itemToThrow)
	{
		// Assertions
		if (m_rsoInputsLocked.value) return;
		if (itemToThrow == null) return;

		IsAiming = isInputPressed;
		m_rsoCameraStyle.value = IsAiming 
			? (itemToThrow is Rope) ? CameraStyle.ROPE : CameraStyle.TORCH 
			: CameraStyle.BASIC;

		// Handle preview on input pressed
		if (IsAiming)
		{
			// Placing rope in hand
			if (itemToThrow is Rope)
			{
				itemToThrow.transform.parent = m_handSocket.transform;
				itemToThrow.transform.localPosition = Vector3.zero;
				itemToThrow.transform.rotation = m_handSocket.rotation;
			}
			itemToThrow.InitializePreview();
			m_startAiming = true;
			AimingObject = itemToThrow;
		}

		// Handle pernament throw on input released
		else
		{
			// Assertion
			if (!m_startAiming) return;
			if (!itemToThrow.Throw(m_rsoCameraTransform.value))
			{
				CancelAim();
				return;
			}

			// Rope Handling
			if (itemToThrow is Rope) {
                // Exception: rope attachment
                if (m_rope != null) DesequipRope();
                m_rope = itemToThrow as Rope;
                if (m_rope != null) m_rope?.Attach(m_harness, m_rigidbody);

                HandObject = null;
            }
			// Torch handling
			else
            {
                RobotObject = null;
            }

			AimingObject = null;
			m_startAiming = false;
		}
	}

    private void CancelAim()
    {
		// Placing rope back in bag
		if (AimingObject is Rope)
		{
			AimingObject.transform.parent = BagCraftSocket.transform;
			AimingObject.transform.rotation = BagCraftSocket.rotation;
			AimingObject.transform.localPosition = Vector3.zero;
		}

		AimingObject.DisablePreview();
        IsAiming = false;
        AimingObject = null;
        m_startAiming = false;
		m_rsoCameraStyle.value = CameraStyle.BASIC;
    }

    public bool IsCarryingLight()
    {
        return HandObject && (HandObject as Torch) && (HandObject as Torch).IsLit
        || RobotObject && (RobotObject as Torch) && (RobotObject as Torch).IsLit;
    }

	#endregion
}
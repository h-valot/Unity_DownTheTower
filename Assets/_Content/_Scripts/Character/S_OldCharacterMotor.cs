using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class OldCharacterMotor : MonoBehaviour
{
	#region exposed variables

	[Foldout("Internal references")] [SerializeField] private Transform m_cameraTransform;
	[Foldout("Internal references")] [SerializeField] private Transform m_characterDirection;
	[Foldout("Internal references")] [SerializeField] private Transform m_handSocket;
	[Foldout("Internal references")] [SerializeField] private Transform m_robotHandSocket;
	[Foldout("Internal references")] [SerializeField] private Transform m_harness;
	[Foldout("Internal references")] [SerializeField] private CharacterController m_controller;
	[Foldout("Internal references")] [SerializeField] private GameObject m_backpackAnchor;

	[Foldout("External references")] [SerializeField] private CameraMotor m_thirdPersonCamera;
	[Foldout("External references")] [SerializeField] private GameObject m_PF_backpack;

	[Foldout("Scriptable references")] [SerializeField] private OldCharacterConfig m_characterConfig;
	[Foldout("Scriptable references")] [SerializeField] private LadderConfig m_ladderConfig;
	[Foldout("Scriptable references")] [SerializeField] private RopeConfig m_ropeConfig;
	[Foldout("Scriptable references")] [SerializeField] private TorchConfig m_torchConfig;
	[Foldout("Scriptable references")] [SerializeField] private RSO_CharacterForward m_rsoCharacterForward;
	[Foldout("Scriptable references")] [SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[Foldout("Scriptable references")] [SerializeField] private RSO_CharacterDeath m_rsoPlayerDeath;
	[Foldout("Scriptable references")] [SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[Foldout("Scriptable references")] [SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Run m_rseRun;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Look m_rseLook;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Move m_rseMove;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Jump m_rseJump;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Throw m_rseThrow;
	[Foldout("Scriptable references")] [SerializeField] private RSE_ToggleInHand m_rseToggleInHand;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Craft m_rseCraft;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Interact m_rseInteract;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Cancel m_rseCancelAction;
	[Foldout("Scriptable references")] [SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Recycle m_rseRecycle;
	[Foldout("Scriptable references")] [SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[Foldout("Scriptable references")] [SerializeField] private RSE_KillCharacter m_rseKillCharacter;
	[Foldout("Scriptable references")] [SerializeField] private RSE_Climb m_rseClimb;
	[Foldout("Scriptable references")] [SerializeField] private RSE_SetCharacterPosition m_rseSetCharacterPosition;

	#endregion

	#region runtime variables

	// Debug header to separate possible private variables set public to be debugged
	//[Header("Debug")]

	// ----- PRIVATE VARIABLES -----
	// - animation -
	public AnimationState _currentState { get; private set; }

    // - move -
    public float PlanarSpeed { get; private set; }
	private Vector2 m_moveInput;
	private float m_targetPlanarSpeed;
	private float m_gravitySpeed;
	private Vector3 m_movement;
	private bool m_isRunning;
	private float m_coyoteTime;

	// - slope -
	private float m_slopePercentage;

	// - status -
	private float m_stunTimer;
	private float m_slowTimer;

	// - ground -
	private Vector3 m_origin;
    private Vector3 m_planarForward;
    private Vector3 m_planarRight;
	private bool m_isGroundedLastFrame = true;
	private LayerMask m_raycastLayerMask;
    private RaycastHit[] m_groundHits = new RaycastHit[5];
	private float m_discriminantForward;
    private float m_discriminantRight;

    // - jump -
    private bool m_wantJump;
	private bool m_canJump = true;
	private bool m_isJumping;
	private RaycastHit[] m_edgeHits;
	private RaycastHit m_edgeHit;

	// - prolonged-jump -
	public bool IsJumpingPressed { get; private set; }
    private float m_prolongedJumpTimer = 0f;
	public Triome IsJumpProlonged { get; private set; }

	// - fall -
	public bool IsGrounded { get; private set; }
    private bool[] m_groundChecks = new bool[5];
	private bool m_isStunned = false;
	private bool m_isSlowed = false;
	private bool m_isSlowedPostStun = false;

	// - momentum - 
	private Vector3 m_positionStartFall;
	private Vector3 m_lastGroundedPlanarForward;
	private float m_fallHeight;

	// - interact -
	private List<Interactable> m_interactables;
	private List<Interactable> m_validInteractibles;

	// - throw -
	public bool Aiming { get; private set; }

    // - craft -
    private bool m_crafting = false;
	private CraftType m_objectToCraft = CraftType.NONE;
	private Coroutine m_craftCoroutine;

	// - permanent -
	[HideInInspector] public bool HasBackpack { get; private set; }
    [HideInInspector] public Backpack Backpack { get; private set; }
	[HideInInspector] public Permanent CraftInHand { get; private set; }
    [HideInInspector] public Permanent CraftInRobot { get; private set; }

    // ---- CONSTS ----
    private const float _HOLDING_KEY_THRESHOLD = 0.2f;

	#endregion

	#region monobehaviour functions

	private void Start()
    {
        // creation of the interaction list
        m_interactables = new List<Interactable>();
        m_validInteractibles = new List<Interactable>();

		m_raycastLayerMask |= (1 << LayerMask.NameToLayer("Default"));
		m_raycastLayerMask |= (1 << LayerMask.NameToLayer("Collision_NoRaycast"));

		SwitchState(AnimationState.LOCOMOTION);

        if (m_characterConfig.startWithBag)
        {
			Backpack = FindAnyObjectByType<Backpack>();
            if (Backpack == null)
            {
                Backpack = Instantiate(m_PF_backpack, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Backpack>(); 
            }
            // Backpack.ForceSetupBackpack(this);
        }

        IsJumpProlonged = Triome.FALSE;
    }

	private void Update()
	{
		CalculateOriginForwardRight();
		CheckGround();
		CheckCoyoteTime();
		UpdateStatus(); 
		CheckProlongedJump();

		VerifyState();

		UpdateCurrentState();
    }

    private void LateUpdate()
    {
        LateUpdateCurrentState();
		if (Aiming)
		{
            CraftInHand.PreviewThrow(m_thirdPersonCamera.transform);
        }
    }

    private void OnEnable()
	{
        m_rseToggleInputs.action += ToggleInputs;
        SubscribeInputs();
    }

	private void OnDisable()
	{
		UnsubscribeInputs();
    }

	#if UNITY_EDITOR
	private void OnDrawGizmos()
    {
        if (m_characterConfig.showGroundedDebug)
        {
            if (m_discriminantForward > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_groundHits[1].point + (m_groundHits[2].point - m_groundHits[1].point).normalized *
                    (-Vector3.Dot((m_groundHits[2].point - m_groundHits[1].point).normalized, m_groundHits[1].point - m_origin) + Mathf.Sqrt(m_discriminantForward))
                    , 0.05f);
                Gizmos.DrawSphere(m_groundHits[1].point + (m_groundHits[2].point - m_groundHits[1].point).normalized *
                    (-Vector3.Dot((m_groundHits[2].point - m_groundHits[1].point).normalized, m_groundHits[1].point - m_origin) - Mathf.Sqrt(m_discriminantForward))
                    , 0.05f);
            }
            if (m_discriminantRight > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_groundHits[3].point + (m_groundHits[4].point - m_groundHits[3].point).normalized *
                    (-Vector3.Dot((m_groundHits[4].point - m_groundHits[3].point).normalized, m_groundHits[3].point - m_origin) + Mathf.Sqrt(m_discriminantRight))
                    , 0.05f);
                Gizmos.DrawSphere(m_groundHits[3].point + (m_groundHits[4].point - m_groundHits[3].point).normalized *
                    (-Vector3.Dot((m_groundHits[4].point - m_groundHits[3].point).normalized, m_groundHits[3].point - m_origin) - Mathf.Sqrt(m_discriminantRight))
                    , 0.05f);
            }
        }
		if (m_edgeHits != null && (_currentState == AnimationState.FALL || _currentState == AnimationState.JUMP))
		{
            Gizmos.color = Color.cyan;
            foreach (RaycastHit _hit in m_edgeHits)
            {
				Gizmos.DrawSphere(_hit.point, 0.05f);
            }
        }

        if (_rope)
        {
            if (_rope.IsPlaced)
            {
				// Magenta: rope limit & start position
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_rope.Folds[^1], _rope.HoldLength);
				Gizmos.DrawWireCube(_gizmoStartPendulumPosition, new Vector3(.5f, .5f, .5f));
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
		EnterState(newState);
        // m_rsoCharacterState.value = newState;

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
        }
	}

    /// <summary>
    /// 	Check variable of player to determine new player state.
    /// </summary>
    private void VerifyState()
    {
		// - CRAFT - from locomotion
		if (m_crafting && _currentState == AnimationState.LOCOMOTION)
		{
            SwitchState(AnimationState.CRAFT);
		}

		// - LOCOMOTION - from crafting
		else if (!m_crafting && _currentState == AnimationState.CRAFT)
		{
            SwitchState(AnimationState.LOCOMOTION);
		}

		// - JUMP -
		else if (m_canJump && m_wantJump && (IsGrounded || m_coyoteTime > 0f) && _currentState != AnimationState.JUMP)
        {
            SwitchState(AnimationState.JUMP);
            m_isJumping = true;
		}

		// - FALL & ROPE -
		else if (!IsGrounded && m_gravitySpeed <= 0 && (_currentState != AnimationState.FALL || _currentState != AnimationState.ROPE))
        {
			// Reset coyote time
            if (m_isGroundedLastFrame) m_coyoteTime = m_characterConfig.coyoteTime;

			// If the character is attached to a rope, switch to Rope state instead
			// This state handles free fall and rope-attached fall.
			if (_rope != null && _rope.IsPlaced && _rope.IsConnected 
			&& _currentState != AnimationState.ROPE && _currentState != AnimationState.FALL)
			{
				SwitchState(AnimationState.ROPE);
			}

			// Handle default fall state
			else if (_currentState != AnimationState.FALL && _currentState != AnimationState.ROPE 
			|| (_currentState == AnimationState.ROPE && _rope == null))
			{
				SwitchState(AnimationState.FALL);
			}
		}

		// - LOCOMOTION - default state back up
		else if (IsGrounded && !m_isJumping && _currentState != AnimationState.LOCOMOTION && !m_crafting)
        {
            ApplyFallHeight();
            SwitchState(AnimationState.LOCOMOTION);
		}

		// - SUB-ROPE - defines if the rope suspension is partial or complete
		if (_currentState == AnimationState.ROPE)
		{
			// Update sub-rope state
			_ropeState = CheckWall()
				? RopeState.PARTIAL_SUSPENSION
				: RopeState.COMPLETE_SUSPENSION;

			// Simulate rope sub-jump state by directly calling the rope jump function
			if (m_wantJump) JumpOffWall();
		}

		// Reset Jump if it is not possible to jump
		m_wantJump = false;
    }

    #endregion

    #region misc

    /// <summary>
    /// 	kill the character
    /// </summary>
    public void HandleDeath()
	{
		m_rsoPlayerDeath.value = true;
		Destroy(gameObject);
	}

	/// <summary>
	/// Put the backpack on player back.
	/// </summary>
	/// <param name="_skipAnim">Prevent grab backpack animation from playing.</param>
	public void PickupBackpack(bool _skipAnim, Backpack _newBackpack)
	{
		Backpack = _newBackpack;
        HasBackpack = true;
		
		RemoveFromInteractList(Backpack);
        ToggleCraftInput(HasBackpack);

		Backpack.transform.SetParent(m_backpackAnchor.transform, false);
		Backpack.transform.localPosition = Vector3.zero;
		Backpack.transform.localRotation = Quaternion.identity;
		Backpack.transform.localScale = Vector3.one;
    }

	#endregion

	#region movement

	/// <summary>
	/// 	Force character's tranform position and rotation to the given values.
	/// </summary>
	private void ForceCharacterPosition(Vector3 position, Quaternion rotation)
	{
		transform.position = position;
		transform.rotation = rotation;
		Physics.SyncTransforms();
	}

	/// <summary>
	/// 	Determine origin forward and right vectors based on character position, camera and inputs.	
	/// </summary>
	private void CalculateOriginForwardRight()
	{
		m_origin = new Vector3(transform.position.x, transform.position.y + m_controller.radius, transform.position.z);

        // Check if ground on 5 points align with player inputs or character direction if no inputs
        if (m_moveInput != Vector2.zero)
		{
			//Calculate input forward and right on character plane
            m_planarForward = (new Vector3(m_thirdPersonCamera.transform.forward.x, 0, m_thirdPersonCamera.transform.forward.z) * m_moveInput.y + new Vector3(m_thirdPersonCamera.transform.right.x, 0, m_thirdPersonCamera.transform.right.z) * m_moveInput.x).normalized;
            m_planarRight = new Vector3(-m_planarForward.z, 0, m_planarForward.x);
        }
		else
		{
            //Calculate character graphic forward and right on character plane
            m_planarForward = new Vector3(m_characterDirection.forward.x, 0, m_characterDirection.forward.z).normalized;
            m_planarRight = new Vector3(-m_planarForward.z, 0, m_planarForward.x);
        }

	}

	/// <summary>
	/// 	Use 5 raycasts to check if the character has a collider below it.
	/// </summary>
	private void CheckGround()
    {
		//Debug Line
		if (m_characterConfig.showGroundedDebug)
		{
			UnityEngine.Debug.DrawLine(m_origin, new Vector3(m_origin.x, m_origin.y - m_controller.radius * m_characterConfig.groundCheckYFactor, m_origin.z), Color.red);
			UnityEngine.Debug.DrawLine(m_origin + m_planarForward * m_controller.radius, new Vector3(m_origin.x + m_planarForward.x * m_controller.radius, m_origin.y - m_controller.radius * m_characterConfig.groundCheckYFactor, m_origin.z + m_planarForward.z * m_controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(m_origin - m_planarForward * m_controller.radius, new Vector3(m_origin.x - m_planarForward.x * m_controller.radius, m_origin.y - m_controller.radius * m_characterConfig.groundCheckYFactor, m_origin.z - m_planarForward.z * m_controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(m_origin + m_planarRight * m_controller.radius, new Vector3(m_origin.x + m_planarRight.x * m_controller.radius, m_origin.y - m_controller.radius * m_characterConfig.groundCheckYFactor, m_origin.z + m_planarRight.z * m_controller.radius), Color.red);
			UnityEngine.Debug.DrawLine(m_origin - m_planarRight * m_controller.radius, new Vector3(m_origin.x - m_planarRight.x * m_controller.radius, m_origin.y - m_controller.radius * m_characterConfig.groundCheckYFactor, m_origin.z - m_planarRight.z * m_controller.radius), Color.red);
		}

        //Raycast
        m_groundChecks[0] = Physics.Raycast(m_origin, Vector3.down, out m_groundHits[0], m_controller.radius * m_characterConfig.groundCheckYFactor, m_raycastLayerMask);
        m_groundChecks[1] = Physics.Raycast(m_origin + m_planarForward * m_controller.radius, Vector3.down, out m_groundHits[1], m_controller.radius * m_characterConfig.groundCheckYFactor, m_raycastLayerMask);
        m_groundChecks[2] = Physics.Raycast(m_origin - m_planarForward * m_controller.radius, Vector3.down, out m_groundHits[2], m_controller.radius * m_characterConfig.groundCheckYFactor, m_raycastLayerMask);
        m_groundChecks[3] = Physics.Raycast(m_origin + m_planarRight * m_controller.radius, Vector3.down, out m_groundHits[3], m_controller.radius * m_characterConfig.groundCheckYFactor, m_raycastLayerMask);
        m_groundChecks[4] = Physics.Raycast(m_origin - m_planarRight * m_controller.radius, Vector3.down, out m_groundHits[4], m_controller.radius * m_characterConfig.groundCheckYFactor, m_raycastLayerMask);

		m_isGroundedLastFrame = IsGrounded;
		IsGrounded = false;
		m_discriminantForward = -1f;
		m_discriminantRight = -1f;

		//Check if raycast directly below the character hit a surface near enough to consider grounded
		if (m_groundChecks[0])
		{
			if ((m_groundHits[0].point - m_origin).magnitude <= m_controller.radius + m_characterConfig.skinWidth)
			{
				IsGrounded = true;
			}
		}
		//Prevent unnecessary check if we already know the character is grounded
		if (!IsGrounded)
		{
            //Check if the raycast hits Forward/Backward make a line that cross player capsule+skin, which mean the player is grounded
            if (m_groundChecks[1] && m_groundChecks[2])
            {
                //Debug Line
                if (m_characterConfig.showGroundedDebug)
                {
                    UnityEngine.Debug.DrawLine(m_groundHits[1].point, m_groundHits[2].point, Color.yellow);
                }

                //discriminant of the equation between the sphere (centered on _origin and radius of _controller.radius+skinWidth) and the line resulting of the hits of the raycasts
                m_discriminantForward = Mathf.Pow(Vector3.Dot((m_groundHits[2].point - m_groundHits[1].point).normalized, m_groundHits[1].point - m_origin), 2) - ((m_groundHits[1].point - m_origin).sqrMagnitude - Mathf.Pow(m_controller.radius + m_characterConfig.skinWidth, 2));
                //discriminant > 0 means that the line cross the sphere in at least 2 points (no tangent)
                if (m_discriminantForward > 0)
                {
                    IsGrounded = true;
                }
            }
            //Check if the raycast hits Right/Left make a line that cross player capsule+skin, which mean the player is grounded
            if (m_groundChecks[3] && m_groundChecks[4])
            {
                //Debug Line
                if (m_characterConfig.showGroundedDebug)
                {
                    UnityEngine.Debug.DrawLine(m_groundHits[3].point, m_groundHits[4].point, Color.yellow);
                }
                //discriminant of the equation between the sphere (centered on _origin and radius of _controller.radius+skinWidth) and the line resulting of the hits of the raycasts
                m_discriminantRight = Mathf.Pow(Vector3.Dot((m_groundHits[4].point - m_groundHits[3].point).normalized, m_groundHits[3].point - m_origin), 2) - ((m_groundHits[3].point - m_origin).sqrMagnitude - Mathf.Pow(m_controller.radius + m_characterConfig.skinWidth, 2));
                //discriminant > 0 means that the line cross the sphere in at least 2 points (no tangent)
                if (m_discriminantRight > 0)
                {
                    IsGrounded = true;
                }
            }
        }
		if (m_isJumping && m_gravitySpeed <= 0)
		{
			m_isJumping = false;
		}
    }

	/// <summary>
	///		Check fall height and kill/stun/slow player if necessary
	/// </summary>
	private void ApplyFallHeight()
	{
        m_fallHeight = Math.Abs(m_rsoCharacterPosition.value.y - m_positionStartFall.y);
		if (m_fallHeight >= m_characterConfig.lethalHeight)
        {
            HandleDeath();
        }
        else if (m_fallHeight >= m_characterConfig.stunHeight)
        {
            // stun the character for x secondes
            m_isStunned = true;

            // cross product to get the stun mitiged value on a 0-1 scale
            float stunMitigedValue = (m_fallHeight - m_characterConfig.stunHeight) / (m_characterConfig.lethalHeight - m_characterConfig.stunHeight);
            m_stunTimer = m_characterConfig.stunDuration.Evaluate(stunMitigedValue);
        }
        else if (m_fallHeight >= m_characterConfig.slowHeight)
        {
            // slow the character for x secondes by y percent
            m_isSlowed = true;

            // cross product to get the slow mitiged value on a 0-1 scale
            float slowMitigedValue = (m_fallHeight - m_characterConfig.slowHeight) / (m_characterConfig.stunHeight - m_characterConfig.slowHeight);
            m_slowTimer = m_characterConfig.slowDuration.Evaluate(slowMitigedValue);
        }
    }

	/// <summary>
	/// 	Update coyote time
	/// </summary>
	private void CheckCoyoteTime()
	{
		if (m_coyoteTime > 0f)
		{
			m_coyoteTime -= Time.deltaTime;
		}
	}

	/// <summary>
	/// 	Set _targetSpeed based on player running input.
	/// </summary>
	private void CheckWalkRun()
	{
		if(m_isRunning)
		{
			m_targetPlanarSpeed = m_characterConfig.runSpeed;
		}
		else
		{
			m_targetPlanarSpeed = m_characterConfig.walkSpeed;
		}
	}

	/// <summary>
	/// 	Set the slope angle to the mean angle value amoung 5 raycasts.
	/// 	Get the slope deceleration or acceleration percentage based on the slope angle.
	/// </summary>
	private void ApplySlope()
	{
		if (!IsGrounded) return;

		Vector3 _hitsNormalSum = Vector3.zero;
		int _hitCount = 0;

		for (int _indexHit = 0; _indexHit < 5; _indexHit++)
		{
			if (m_groundChecks[_indexHit])
			{
                _hitsNormalSum += m_groundHits[_indexHit].normal;
				_hitCount++;
            }
		}

        // get the slope percentage to calculate slows later in the movement function
        m_slopePercentage = Vector3.Angle(_hitsNormalSum/_hitCount, Vector3.up) / m_controller.slopeLimit;

		// signed and scaled percent based on player input direction and mean normal
		m_slopePercentage *= -Vector3.Dot(new Vector3((_hitsNormalSum/_hitCount).x, 0, (_hitsNormalSum/_hitCount).z).normalized, m_planarForward)*2;

		m_targetPlanarSpeed *= m_characterConfig.slopeSpeedModifier.Evaluate(m_slopePercentage);
	}

	/// <summary>
	/// 	Increase _planarSpeed by minimal jup speed and clamp it to max walk/run speed 
	/// </summary>
	private void ApplyJumpImpulsePlanarSpeed()
	{
        if (m_isRunning)
		{
			PlanarSpeed = Mathf.Clamp(PlanarSpeed+m_characterConfig.jumpMinimalPlanarVelocity, 0, m_characterConfig.runSpeed);
		}
		else
		{
			PlanarSpeed = Mathf.Clamp(PlanarSpeed+m_characterConfig.jumpMinimalPlanarVelocity, 0, m_characterConfig.walkSpeed);
		}
	}

    /// <summary>
    /// 	Multiply target speed by input magnitude.
    /// </summary>
	private void ApplyInputs()
	{
        //multiply by input magnitude
        m_targetPlanarSpeed *= Mathf.Clamp(m_moveInput.magnitude, 0, 1);
        // TO DO: remap input magnitude from 0:1 to deadzone:1
    }

    /// <summary>
    /// 	Add acceleration or decceleration and clamp it.
    /// </summary>
    private void ApplyAcceleration()
	{
		// accelerate or decelerate to target speed
		if (PlanarSpeed <= m_targetPlanarSpeed)
		{
			PlanarSpeed = Mathf.Clamp(PlanarSpeed + m_characterConfig.groundAcceleration*Time.deltaTime, 0, m_targetPlanarSpeed);
		}
		else
		{
			PlanarSpeed = Mathf.Clamp(PlanarSpeed - m_characterConfig.groundDecceleration*Time.deltaTime, m_targetPlanarSpeed, m_characterConfig.runSpeed);
		}
	}

    /// <summary>
    /// 	Calculate _movement with _planarSpeed and _planarForward.
    /// </summary>
    private void CreateMovement()
	{
        //calculate _movement to apply to CharacterController
        m_movement += PlanarSpeed * m_planarForward;
    }

    /// <summary>
    /// 	Add positive vertical speed to make character jump
    /// </summary>
    private void ApplyJumpImpulseVerticalSpeed()
	{
        m_gravitySpeed = Mathf.Sqrt(m_characterConfig.jumpHeight * -3f * m_characterConfig.gravity) + m_characterConfig.gravity * Time.deltaTime;
	}

	/// <summary>
	///  Call to update timer and status without applying movement modif
	/// </summary>
	private void UpdateStatus()
	{
		if(m_isStunned)
		{
			m_stunTimer -= Time.deltaTime;

			if (m_stunTimer <= 0)
			{
				m_isStunned = false;
				m_isSlowed = true;
				m_isSlowedPostStun = true;
				m_slowTimer = m_characterConfig.slowTimePostStun;
			}
		}
		if(m_isSlowed)
		{
			m_slowTimer -= Time.deltaTime;
			
			if (m_slowTimer <= 0)
			{
				m_isSlowed = false;
				m_isSlowedPostStun = false;
			}
		}
	}

	/// <summary>
	/// Call to update timer and status and applying movement modif
	/// </summary>
	private void ApplyStatus()
	{
		if(m_isStunned)
		{
			m_movement += Vector3.zero;
		}
		else if(m_isSlowed)
		{
			if(!m_isSlowedPostStun)
			{
				m_movement *= m_characterConfig.slowPercentage.Evaluate((m_characterConfig.maxSlowTime - m_slowTimer)/m_characterConfig.maxSlowTime);
			}
			else
			{
				m_movement *= m_characterConfig.slowPercentage.Evaluate((m_characterConfig.slowTimePostStun - m_slowTimer)/m_characterConfig.slowTimePostStun);
			}
		}
	}

	/// <summary>
	/// 	Add fake gravity to snap the character to the floor while going down stairs and slopes.
	/// </summary>
	private void ApplySnapGravity()
	{
		m_movement += new Vector3(m_movement.x, m_characterConfig.SnapGravity, m_movement.z);
	}

	/// <summary>
	/// 	Allow the player to slighty turn during falling.
	/// </summary>
	private void ApplyAirControl()
	{
		//Angle to add based on time since last frame
		float _airControlAngle = m_characterConfig.airControlAngularSpeed * Time.deltaTime;
		//Factor it based on difference between input and character forward
		_airControlAngle *= m_characterConfig.airControlInputFactor.Evaluate(Vector3.Dot(m_planarForward, m_lastGroundedPlanarForward));
		//Sign it
		float _angleInputForward = Vector3.SignedAngle(m_lastGroundedPlanarForward, m_planarForward, Vector3.up);
		_airControlAngle *= _angleInputForward/Mathf.Abs(_angleInputForward);
		//Apply it to character direction (we use _lastGrounded while in air)
		m_lastGroundedPlanarForward = Quaternion.AngleAxis(_airControlAngle, Vector3.up) * m_lastGroundedPlanarForward;
	}

	/// <summary>
	/// 	Decrease planar speed while in air, faster if input are not in same direction as fall.
	/// </summary>
	private void ApplyDrag()
	{
		float _inputOrientationFactor = (-Vector3.Dot(m_lastGroundedPlanarForward, m_planarForward) + 3f) / 4f;
		if (m_moveInput == Vector2.zero)
		{
			_inputOrientationFactor = 0.75f;
        }
		PlanarSpeed = Mathf.Clamp(PlanarSpeed - m_characterConfig.dragDecceleration * Time.deltaTime * _inputOrientationFactor, 0, m_characterConfig.runSpeed);
	}

    /// <summary>
    /// 	Calculate _movement with _planarSpeed and _lastGroundedPlanarForward.
    /// </summary>
    private void CreateMovementFall()
    {
        m_movement += PlanarSpeed * m_lastGroundedPlanarForward;
    }

    /// <summary>
    /// 	handle gravity modifier, and jump delay
    /// </summary>
    private void ApplyGravity()
	{
		m_gravitySpeed += m_characterConfig.gravity * Time.deltaTime;

        m_movement += new Vector3(m_movement.x, m_gravitySpeed, m_movement.z);
	}

	/// <summary>
	///		Detect edges point while falling or jumping
	/// </summary>
	private void DetectEdges()
	{
		Vector3 _start = new Vector3(transform.position.x, transform.position.y + m_controller.height - m_controller.radius, transform.position.z);
		float _radius = m_controller.radius + m_characterConfig.skinWidth;
		float _distance = m_controller.height - 2 * m_controller.radius + m_characterConfig.skinWidth;
        m_edgeHits = Physics.SphereCastAll(_start, m_controller.radius, Vector3.down, _distance, m_raycastLayerMask);

		if (m_edgeHits.Length > 0 )
		{
            m_edgeHit = m_edgeHits[0];
			if(m_edgeHit.normal == Vector3.up) m_edgeHit = new RaycastHit();
        }
		else
		{
            m_edgeHit = new RaycastHit();
        }
    }

	/// <summary>
	///		Select the edge hit that should have the priority
	/// </summary>
	private void SortEdgeHits()
	{
		m_edgeHit = new RaycastHit();

		foreach (RaycastHit _hit in m_edgeHits)
		{

		}

		for (int i = 1; i < m_edgeHits.Length; i++)
		{
			//Study only point below character edge climb height
			if (m_edgeHits[i].point.y - transform.position.y < m_characterConfig.edgeMaxClimbingHeight)
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
		if (m_edgeHit.collider == null) { return;}

        Vector3 _edgeSlopeSlideLeft = Vector3.Cross(m_edgeHit.normal, Vector3.up).normalized;
        Vector3 _edgeSlopeSlideDown = Vector3.Cross(m_edgeHit.normal, _edgeSlopeSlideLeft).normalized;

		// UnityEngine.Debug.DrawRay(_rsoCharacterPosition.value, _edgeHit.normal, Color.blue);

		m_movement += -_edgeSlopeSlideDown.normalized * m_gravitySpeed;

        UnityEngine.Debug.DrawRay(m_rsoCharacterPosition.value, - _edgeSlopeSlideDown * m_gravitySpeed, Color.cyan);
    }

	/// <summary>
	/// 	moves the character towards the input directions 
	/// </summary>
	private void HandleMovement()
	{
        m_controller.Move(m_movement * Time.deltaTime);
		m_movement = Vector3.zero;

		// - update variables -
		if (m_rsoCharacterPosition.value != m_characterDirection.position) { m_rsoCharacterPosition.value = m_characterDirection.position; }
		if (m_rsoCharacterForward.value != m_characterDirection.forward) { m_rsoCharacterForward.value = m_characterDirection.forward; }
    }

    #endregion

    #region inputs

    /// <summary>
    /// 	Add character behavior to player inputs based on animation state
    /// </summary>
    private void SubscribeInputs()
	{
		ToggleCraftInput(HasBackpack);

		m_rseRun.action += Run;
		m_rseJump.action += Jump;
		m_rseCancelAction.action += CancelAction;
		m_rseKillCharacter.action += HandleDeath;
		m_rseClimb.action += Climb;
		m_rseSetCharacterPosition.action += ForceCharacterPosition;

		switch (_currentState)
        {
            case AnimationState.LOCOMOTION:
                m_rseMove.action += Move;
                m_rseThrow.action += ToggleAim;
                m_rseCraft.action += ToggleCraft;
                m_rseToggleInHand.action += ToggleInHand;
                m_rseInteract.action += Interact;
                break;
            case AnimationState.JUMP:
                m_rseMove.action += Move;
                m_rseThrow.action += ToggleAim;
                m_rseToggleInHand.action += ToggleInHand;
                m_rseInteract.action += Interact;
                break;
            case AnimationState.FALL:
                m_rseMove.action += Move;
                m_rseThrow.action += ToggleAim;
                m_rseToggleInHand.action += ToggleInHand;
                m_rseInteract.action += Interact;
                break;
            case AnimationState.CRAFT:
                m_rseCraft.action += ToggleCraft;
                break;
            case AnimationState.ROPE:
                m_rseMove.action += Move;
                m_rseThrow.action += ToggleAim;
                m_rseCraft.action += ToggleCraft;
                m_rseToggleInHand.action += ToggleInHand;
                m_rseInteract.action += Interact;
                break;
            case AnimationState.LADDER:
                m_rseMove.action += Move;
                m_rseThrow.action += ToggleAim;
                m_rseCraft.action += ToggleCraft;
                m_rseToggleInHand.action += ToggleInHand;
				m_rseInteract.action += Interact;
                break;
        }
    }

    /// <summary>
    /// 	Remove character behavior from player inputs.
    /// </summary>
    private void UnsubscribeInputs()
    {
        m_rseMove.action -= Move;
        m_rseRun.action -= Run;
        m_rseJump.action -= Jump;
        m_rseThrow.action -= ToggleAim;
        m_rseCraft.action -= ToggleCraft;
        m_rseToggleInHand.action -= ToggleInHand;
        m_rseCancelAction.action -= CancelAction;
        m_rseInteract.action -= Interact;
		m_rseKillCharacter.action -= HandleDeath;
        m_rseRecycle.action -= Recycle;
		m_rseClimb.action -= Climb;
		m_rseSetCharacterPosition.action -= ForceCharacterPosition;
	}

	public void ToggleCraftInput(bool _isActive)
	{
		if (_isActive)
		{
			m_rseCraft.action += ToggleCraft;
			m_rseRecycle.action += Recycle;
		}
		else
		{
			m_rseCraft.action -= ToggleCraft;
			m_rseRecycle.action -= Recycle;
		}
    }

	private void ToggleInputs()
	{
		if (m_rsoGamePaused.value)
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
		m_moveInput = input;
	}

	/// <summary>
	/// 	Set `_wantJump` to true.
	/// 	Subscribed to RSE_Jump only in locomotion State.
	/// </summary>
	/// <param name="isJumping">Is the input pressed.</param>
	private void Jump(bool isJumping)
	{
        m_wantJump = isJumping; // Resetted after switch state check
		IsJumpingPressed = isJumping;
	}

	/// <summary>
	/// 	Called in Update(). Check is the jump input is held by the player.
	/// </summary>
	private void CheckProlongedJump()
	{
		if (!IsJumpingPressed) 
		{
			m_prolongedJumpTimer = 0f;
			IsJumpProlonged = Triome.FALSE;
			return;
		}

		m_prolongedJumpTimer += Time.deltaTime;
		if (m_prolongedJumpTimer >= _HOLDING_KEY_THRESHOLD
		&& IsJumpProlonged == Triome.FALSE)
		{
			IsJumpProlonged = Triome.TRUE;
		}
	}

	/// <summary>
	/// 	update the sprint input value
	/// </summary>
	/// <param name="isRunning">is the input pressed</param>
	private void Run(bool isRunning)
	{
		m_isRunning = isRunning;
		Holding(isRunning);
	}

	/// <summary>
	///     Update the holding rope input value.
	/// </summary>
	/// <param name="isHolding">is the input pressed</param>
	private void Holding(bool isHolding)
	{
		if (IsJumpingPressed && !isHolding) return;

		if (_rope == null)
		{
			_isHolding = false;
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

		_isHolding = isHolding;
	}

	private void Holding(Triome isHolding)
	{
		if (isHolding == Triome.NEITHER) 
		{
			print("CHARACTER_MOTOR: Assert - isHolding value is equal to NEITHER.");
			return;
		}

		Holding(isHolding == Triome.TRUE);
	}

	private void Climb(bool isClimbing)
	{
		_isClimbing = isClimbing;
	}

	/// <summary>
	/// 	Try to activate permanent object in hand
	/// </summary>
	private void ToggleInHand()
	{
		CraftInHand?.ToggleInHand();
	}

	private void CancelAction()
	{
		// The cancel action is contextual
		// Do various things based on the context

		// Rope context
		if (_rope != null && _rope.IsPlaced)
		{
			DesequipRope();
		}
	}

	#endregion

	#region locomotion state

	private void EnterLocomotionState()
	{
		
	}

	private void UpdateLocomotionState()
	{
		if (m_interactables.Count > 0)
		{
			Interactable nearest = GetNearestInteractible();
			CheckShowInteract();
			if (nearest != null) CheckShowRecycle(nearest.IsRecyclable);
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
	}

	private void LateUpdateLocomotionState()
	{

	}


    private void ExitLocomotionState()
	{
		m_gravitySpeed = 0;
		m_lastGroundedPlanarForward = m_planarForward;
    }

	#endregion

	#region jump state

	private void EnterJumpState()
	{
        m_rseCraft.action -= ToggleCraft;

		m_canJump = false;

		if (m_isGroundedLastFrame) {CheckWalkRun();}
		ApplyJumpImpulsePlanarSpeed();
        if (m_isGroundedLastFrame) {ApplyInputs(); };
        if (m_isGroundedLastFrame) {ApplyAcceleration();};
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
        ToggleCraftInput(HasBackpack);

		m_canJump = true;

	}

	#endregion

	#region fall state

	private void EnterFallState()
	{
		m_rseCraft.action -= ToggleCraft;

        m_positionStartFall = m_rsoCharacterPosition.value;

        if (m_isGroundedLastFrame) { CheckWalkRun(); }
        if (m_isGroundedLastFrame) { ApplyInputs(); };
        if (m_isGroundedLastFrame) { ApplyAcceleration(); };
    }

	private void UpdateFallState()
	{
        // ApplyAirControl();
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
		ToggleCraftInput(HasBackpack);
	}

    #endregion

    #region craft state

    private void ToggleCraft(CraftType _craftName, bool _isInputPressed)
    {
        //Prevent switching to craft state if not in locomotion or crafting state or already crafting another item
        if (_currentState != AnimationState.LOCOMOTION || _currentState != AnimationState.CRAFT)
        {
            //If craft button is pressed
            if (_isInputPressed)
            {
                m_objectToCraft = _craftName;
                m_crafting = true;
            }
            else // if craft button is released
            {
                m_crafting = false;
            }
        }
    }

	private void EnterCraftState()
	{
		m_rseMove.action -= Move;
        m_rseThrow.action -= ToggleAim;
		m_rseToggleInHand.action -= ToggleInHand;
        m_rseInteract.action -= Interact;

		m_canJump = false;

		switch (m_objectToCraft)
        {
            case CraftType.NONE:
                break;

            case CraftType.TORCH:
                if (CraftInHand != null)
                {
                    if (CraftInHand.Type != CraftType.TORCH && CraftInRobot?.Type != CraftType.TORCH)
                    {
                        Destroy(CraftInHand.gameObject);
                        m_craftCoroutine = StartCoroutine(Craft(CraftType.TORCH, m_torchConfig.craftingDuration));
                    }
                }
                else
                {
                    m_craftCoroutine = StartCoroutine(Craft(CraftType.TORCH, m_torchConfig.craftingDuration));
                }
                break;

            case CraftType.DEPRECATED_LADDER:
                if (CraftInHand != null)
                {
                    if (CraftInHand.Type == CraftType.TORCH)
                    {
                        CraftInHand.transform.SetParent(m_robotHandSocket, false);
                        CraftInRobot = CraftInHand;
                        CraftInRobot.transform.rotation = m_robotHandSocket.rotation;
                        CraftInHand = null;
                        m_craftCoroutine = StartCoroutine(Craft(CraftType.DEPRECATED_LADDER, m_torchConfig.craftingDuration));
                    }
                    else if (CraftInHand.Type != CraftType.DEPRECATED_LADDER)
                    {
                        Destroy(CraftInHand.gameObject);
                        m_craftCoroutine = StartCoroutine(Craft(CraftType.DEPRECATED_LADDER, m_torchConfig.craftingDuration));
                    }
                }
                else
                {
                    m_craftCoroutine = StartCoroutine(Craft(CraftType.DEPRECATED_LADDER, m_torchConfig.craftingDuration));
                }
                break;

            case CraftType.ROPE:
                if (CraftInHand != null)
                {
                    if (CraftInHand.Type == CraftType.TORCH)
                    {
                        CraftInHand.transform.SetParent(m_robotHandSocket, false);
                        CraftInRobot = CraftInHand;
                        CraftInHand = null;
                        m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ropeConfig.craftingDuration));
                    }
                    else if (CraftInHand.Type != CraftType.ROPE)
                    {
                        Destroy(CraftInHand.gameObject);
                        m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ropeConfig.craftingDuration));
                    }
                }
                else
                {
                    m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ropeConfig.craftingDuration));
                }
                break;
        }
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

        yield return new WaitForSeconds(_craftDuration);

		// instantiate the crafted object
		switch (_objectToCraft)
		{
            case CraftType.NONE:
                break;

            case CraftType.TORCH:
                CraftInHand = Instantiate(m_torchConfig.pfTorch, m_handSocket.transform);
                break;

			case CraftType.DEPRECATED_LADDER:
				CraftInHand = Instantiate(m_ladderConfig.PF_Ladder, m_handSocket.transform);
                break;

			case CraftType.ROPE:
				CraftInHand = Instantiate(m_ropeConfig.pfRope, m_handSocket.transform);
				break;
		}


        CraftInHand.transform.position = m_handSocket.transform.position;

		m_craftCoroutine = null;
    }

    private void ExitCraftState()
	{
        if (m_craftCoroutine != null)
        {
            StopCoroutine(m_craftCoroutine);
            m_craftCoroutine = null;
        }

		m_canJump = true;

		m_rseMove.action += Move;
        m_rseThrow.action += ToggleAim;
		m_rseToggleInHand.action += ToggleInHand;
        m_rseInteract.action += Interact;
	}


	#endregion

	#region rope state

	// [x] Climb the rope
	// [x] Jump off the rope on motion
	// [x] Re-equip an already-used rope (debug version)
	// [ ] In partial suspension, make the character able to jump off the wall
	// [ ] In partial suspension, make the character unable to move while off the wall
	// [ ] In partial suspension, make the character unable to be snap against a cambered wall 
	// [ ] In complete suspension, make the character pivot with the rope inclination
	// [ ] Lerp the rope stop deceleration

	#region variables

	[Header("Rope")]
	public RopeState _ropeState;
	private bool _isHolding;
	private bool _isAgainstWall;
	private bool _canStartSwinging = true;
	private Rope _rope;

	// ---- PRIVATE VARIABLES ----
	// Velocities
	private Vector3 _pendulumVelocity;
	private Vector3 _verticalVelocity;
	private Vector3 _suspensionVelocity;
	private Vector3 _ropeVelocity;

	// Inputs
	private Vector3 _ropeInputDirection;
	private bool _inputsPressed;
	private bool _inputsTowardsVertical;

	// Climb
	public bool _isClimbing;
	private float _currentClimbSpeed;

	// Jump-off & free fall
	public Triome _isJumpProlongedCached = Triome.NEITHER;
	private float _currTime;
	private Coroutine _ropeConstraintTimer;
	public bool _ropeConstraintAppliedLastly;

	// Mics
	private Vector3 _towardsCharacter;

	// Gizmos
	private Vector3 _gizmoStartPendulumPosition;

	// ---- CONST ----
	private const float _TOWARDS_VERTICAL_THRESHOLD = 0.75f;
	private const float _FALLING_FORCES_THRESHOLD = 0.2f;

	#endregion

	#region animation-state-related functions

	private void EnterRopeState()
	{
		m_rseCraft.action -= ToggleCraft;

		_isJumpProlongedCached = Triome.NEITHER;

		_rope.UpdateHoldLength();
		
		EnterFallState();
	}

	private void UpdateRopeState()
	{
		// Checks
		CheckGround();
		DetectEdges();
		ApplyEdgesSpeed(); 
		HandleProlongedJumpOnRope();

		// Assert: there is no equipped rope 
		if (_rope == null) return;

		// State machine update rope state
		switch (_ropeState)
		{
			case RopeState.PARTIAL_SUSPENSION:
				UpdateRopePartialSuspensionState();
				break;

			case RopeState.COMPLETE_SUSPENSION:
				UpdateRopeCompleteSuspensionState();
				break;
		}

		HandleClimbing();
		HandleMovement();

		// Apply rope holding constraint after the input movements.
		// This allow to avoid glitchy movements.
		HandleRopeConstraint();

		// Debug
		UnityEngine.Debug.DrawRay(m_rsoCharacterPosition.value, _ropeVelocity.normalized);
	}

	private void LateUpdateRopeState()
	{

	}

	private void ExitRopeState()
	{
		_isHolding = false;
		_isJumpProlongedCached = Triome.NEITHER;

		// Update inputs subscriptions
		ToggleCraftInput(HasBackpack);
	}

	#endregion

	#region rope-state-related functions

	/// <summary>
	/// 	Handle movement related to the front wall. 
	/// 	Left / right, jump, go down the rope movement.
	/// 	Jumping and falling off the wall on an edge, change from partial to complete suspension state.
	/// 	Touching the ground, change from partial to grounded state.
	/// </summary>
	private void UpdateRopePartialSuspensionState()
	{
		// Temporary shortcut
		UpdateRopeCompleteSuspensionState();
	}

	/// <summary>
	/// 	Handle movement in the void suspended to the rope. 
	/// 	Left / right, forward / backward, go down the rope movement.
	/// 	Gain support against a wall, change from complete to partial suspension state.
	/// 	Touching the ground, change from complete to grounded state.
	/// </summary>
	private void UpdateRopeCompleteSuspensionState()
	{
		HandleRopeMovement();
		HandleRopeLength();
	}

	#endregion

	#region rope functions

	/// <summary>
	/// 	Using a cached variable of '_isJumpProlonged' to switch if the character is holding the rope or not.
	/// </summary>
	private void HandleProlongedJumpOnRope()
	{
		// Assertions
		if (m_characterConfig.ropeHoldingMethod != RopeHolding.HOLD_TO_LET_GO) return;
		if (_isJumpProlongedCached == IsJumpProlonged) return;

		_isJumpProlongedCached = IsJumpProlonged;
		Holding(_isJumpProlongedCached);
	}

	/// <summary>
	/// 	Check if there is a collider in front of the character using a raycast.
	/// </summary>
	private bool CheckWall()
	{
		// Lisibility varaibles
		Vector3 origin = m_rsoCharacterPosition.value + new Vector3(0, 0.5f, 0);
		float length = m_characterConfig.againstWallRayCastLength;
		LayerMask layerMask = m_characterConfig.againstWallLayerToInclude;

		// Raycasts variables
		bool[] raycastHits = new bool[8];
		RaycastHit[] raycastInfos = new RaycastHit[8];

		// Raycasts
		raycastHits[0] = Physics.Raycast(origin, Vector3.forward, out raycastInfos[0], length, layerMask);	// Forward
		raycastHits[1] = Physics.Raycast(origin, Vector3.right, out raycastInfos[1], length, layerMask);	// Right
		raycastHits[2] = Physics.Raycast(origin, -Vector3.forward, out raycastInfos[2], length, layerMask);	// Backward
		raycastHits[3] = Physics.Raycast(origin, -Vector3.right, out raycastInfos[3], length, layerMask);	// Left
		raycastHits[4] = Physics.Raycast(origin, (Vector3.forward + Vector3.right).normalized, out raycastInfos[4], length, layerMask);	// Forward-Right
		raycastHits[5] = Physics.Raycast(origin, (Vector3.forward - Vector3.right).normalized, out raycastInfos[5], length, layerMask);	// Forward-Left
		raycastHits[6] = Physics.Raycast(origin, (-Vector3.forward + Vector3.right).normalized, out raycastInfos[6], length, layerMask);// Backward-Right
		raycastHits[7] = Physics.Raycast(origin, (-Vector3.forward - Vector3.right).normalized, out raycastInfos[7], length, layerMask);// Backward-Left

		// Check if a raycast is touching a valid collider
		_isAgainstWall = false;
		List<RaycastHit> hitInfos = new List<RaycastHit>();
		for (int i = 0; i < raycastHits.Length; i++)
		{
			if (raycastHits[i])
			{
				_isAgainstWall = true;
				hitInfos.Add(raycastInfos[i]);
			}
		}

		// Average the position from all valid raycasts
		Vector3 averagedPosition = new Vector3();
		for (int i = 0; i < hitInfos.Count; i++)
		{
			averagedPosition += hitInfos[i].point;
		}
		averagedPosition /= hitInfos.Count;

		if (_isAgainstWall)
		{
			// Simple re-direction
			Vector3 hitPoint = new Vector3(averagedPosition.x, m_rsoCharacterPosition.value.y, averagedPosition.z);
			Vector3 touchedDirection = hitPoint - m_rsoCharacterPosition.value;
			m_characterDirection.forward = touchedDirection.normalized;
		}

		return _isAgainstWall;
	}

	/// <summary>
	/// 	Apply a force to `_movement` backward the character.
	/// 	Called from the switch state fonction.
	/// </summary>
	private void JumpOffWall()
	{
		Vector3 direction = new Vector3();
		float force = 0;

		// Reset velocities
		_verticalVelocity = Vector3.zero;
		_pendulumVelocity = Vector3.zero;
		_suspensionVelocity = Vector3.zero;

		switch (_ropeState)
		{
			case RopeState.PARTIAL_SUSPENSION:

				force = m_characterConfig.jumpOffWallForce;

				direction = Vector3Extention.GetPositionOnCercle(
					angle: m_characterConfig.ropeOffsetAngle,
					axis: m_characterDirection.right,
					direction: -m_characterDirection.forward,
					origin: _rope.Folds[^1],
					radius: _rope.HoldLength,
					starting: m_rsoCharacterPosition.value
				);

				break;

			case RopeState.COMPLETE_SUSPENSION:
				break;
		}

		// Apply jump force
		m_movement += direction * force;
	}

	private void ApplyFreeRopeForce(float modifier)
	{
		Vector3 direction = _ropeVelocity.normalized;
		float force = _ropeVelocity.magnitude * modifier;

		// Apply jump force
		_currTime = 0;
		StartCoroutine(ApplyFreeFallForce(direction, force, 2));
	}

	private IEnumerator ApplyFreeFallForce(Vector3 direction, float force, float duration)
	{
		while (_currTime < duration)
		{
			if (IsGrounded) yield break;

			_currTime += Time.deltaTime;
			float percentage = _currTime / duration;
			m_movement += direction * force * (1 - percentage);
			yield return new WaitForNextFrameUnit();
		}
	}
	
	/// <summary>
	/// 	Desequip the rope from the character is total length is exceeded.
	/// </summary>
	private void HandleRopeLength()
	{
		// Assert: total rope length is smaller than the max length
		if (_rope.GetTotalLength() <= m_ropeConfig.maxLength) return;

		DesequipRope();
	}

	/// <summary>
	/// 	Detach the rope from the character.
	/// </summary>
	private void DesequipRope()
	{
		_rope.Detach();
		_rope = null;
		_isHolding = false;
	}

	private void HandleRopeMovement()
	{
		// ---- CHARACTER IS FALLING WITH THE ROPE ----

		if (IsFallingWithRope())
		{
			// Apply regular falling functions
			// ApplyAirControl();
			ApplyDrag();
			CreateMovementFall();
			ApplyGravity();
			return;
		}

		// ---- CHARACTER IS HOLDING THE ROPE ----

		// Populate useful varaibles
		Vector3 verticalPoint = _rope.Folds[^1] + Vector3.down * _rope.HoldLength;
		Vector3 towardsVertical = (m_rsoCharacterPosition.value - verticalPoint).normalized;
		_towardsCharacter = (m_rsoCharacterPosition.value - _rope.Folds[^1]).normalized;

		// Get input related data
		_ropeInputDirection = m_cameraTransform.forward * m_moveInput.y + m_cameraTransform.right * m_moveInput.x;
		_inputsTowardsVertical = Vector3.Dot(_ropeInputDirection, towardsVertical) <= _TOWARDS_VERTICAL_THRESHOLD;
		_inputsPressed = _ropeInputDirection.magnitude > 0;

		// Calculate different velocities to apply to the `_controller`
		HandlePendulum();
		HandleSuspension();
		HandleVertical();

		// Apply velocities
		_ropeVelocity = _suspensionVelocity + _verticalVelocity + _pendulumVelocity;
		m_movement += _ropeVelocity;
	}

	/// <summary>
	/// 	Is the character falling based on the rope holding method.
	/// </summary>
	private bool IsFallingWithRope()
	{
		// Assert: the character is falling if there is no more rope
		if (_rope == null) return true;

		bool isFalling = false;
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				isFalling = !_isHolding;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				if (_isHolding)
				{
					isFalling = true;
				}
				else
				{
					// If the character IS NOT holding the rope, let it fall till it reaches the rope limit constraint
					isFalling = (_rope.Folds[^1] - m_rsoCharacterPosition.value).magnitude < _rope.HoldLength - _FALLING_FORCES_THRESHOLD;
				}
				break;
		}
		return isFalling;
	}

	/// <summary>
	/// 	Get velocity from a simple pendulum effect.
	/// </summary>
	private void HandlePendulum()
	{
		if (!_inputsPressed || _inputsTowardsVertical)
		{
			if (_canStartSwinging)
			{
				ResetPendulumVelocity();
				_canStartSwinging = false;
			}

			UpdatePendulumVelocity();
		}
		else
		{
			_canStartSwinging = true;
			ResetPendulumVelocity();
		}
	}

	/// <summary>
	/// 	Reset `_pendulumVelocity` which makes the acceleration process start over.
	/// </summary>
	private void ResetPendulumVelocity()
	{
		_gizmoStartPendulumPosition = m_rsoCharacterPosition.value;
		_pendulumVelocity = Vector3.zero;
	}

	private void UpdatePendulumVelocity()
	{
		// Add gravity free fall
		// Character gravity force is negative so we reverse it
		float gravityForce = m_characterConfig.mass * -m_characterConfig.gravity;

		// Apply the gravity to `m_CurrentVelocity`
		_pendulumVelocity += Vector3.down * gravityForce * Time.fixedDeltaTime;

		// Cache pivot and bob positions
		Vector3 pivotPositionCache = _rope.Folds[^1];
		Vector3 bobPositionCache = m_rsoCharacterPosition.value;

		// Get bob's position after applying gravity force
		Vector3 auxiliaryMovementDelta = _pendulumVelocity * Time.fixedDeltaTime;
		float distanceAfterGravity = Vector3.Distance(pivotPositionCache, bobPositionCache + auxiliaryMovementDelta);

		// The bob acceleration is mesured in this statement. Returning an updated `m_CurrentVelocity`
		if (distanceAfterGravity > _rope.HoldLength
		|| Mathf.Approximately(distanceAfterGravity, _rope.HoldLength))
		{
			Vector3 tensionDirection = (pivotPositionCache - bobPositionCache).normalized;

			// The nearest the bob is from the vertical point, the greatest the tension force will be.
			float inclinationAngle = Vector3.Angle(bobPositionCache - pivotPositionCache, Vector3.down);
			float tensionForce = gravityForce * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);

			// Generate the counter force to make the bob stay within the circle : centripetal force
			tensionForce += m_characterConfig.mass * Mathf.Pow(_pendulumVelocity.magnitude, 2) / _rope.HoldLength;

			// Apply the tension to `m_CurrentVelocity`
			_pendulumVelocity += tensionDirection * tensionForce * Time.fixedDeltaTime;
		}

		// Apply a counter velocity force: a drag
		_pendulumVelocity -= _pendulumVelocity * (m_characterConfig.drag / gravityForce);
	}

	/// <summary>
	/// 	Calculate the suspension velocity based on the player's inputs.
	/// </summary>
	private void HandleSuspension()
	{
		// Get desired position on the cercle offset by given angle
		Vector3 completeDirection =
			(Vector3Extention.GetPositionOnCercle(
				angle: m_characterConfig.ropeOffsetAngle,
				axis: m_cameraTransform.forward,
				direction: m_cameraTransform.right,
				origin: _rope.Folds[^1],
				radius: _rope.HoldLength,
				starting: m_rsoCharacterPosition.value
			) - m_rsoCharacterPosition.value).normalized * m_moveInput.x +
			(Vector3Extention.GetPositionOnCercle(
				angle: m_characterConfig.ropeOffsetAngle,
				axis: m_cameraTransform.right,
				direction: m_cameraTransform.forward,
				origin: _rope.Folds[^1],
				radius: _rope.HoldLength,
				starting: m_rsoCharacterPosition.value
			) - m_rsoCharacterPosition.value).normalized * m_moveInput.y;

		Vector3 partialDirection =
			(Vector3Extention.GetPositionOnCercle(
				angle: m_characterConfig.ropeOffsetAngle,
				axis: m_cameraTransform.forward,
				direction: m_cameraTransform.right,
				origin: _rope.Folds[^1],
				radius: _rope.HoldLength,
				starting: m_rsoCharacterPosition.value
			) - m_rsoCharacterPosition.value).normalized * m_moveInput.x;

		float suspensionForce = _isAgainstWall ? m_characterConfig.partialSuspensionSpeed : m_characterConfig.completeSuspensionSpeed;
		Vector3 suspensionDirection = _isAgainstWall ? partialDirection : completeDirection;
		_suspensionVelocity = suspensionDirection * suspensionForce;
	}

	/// <summary>
	/// 	Calculate the attraction velocity towards vertical.
	/// </summary>
	public void HandleVertical()
	{
		float angleCharacterVertical = Mathf.Clamp(Vector3.Angle(Vector3.down, _towardsCharacter), 0, m_characterConfig.maxSideAngle);
		Vector3 totalForces = -(_pendulumVelocity + _suspensionVelocity);
		_verticalVelocity = angleCharacterVertical * totalForces / m_characterConfig.maxSideAngle;

		if (!_inputsPressed)
		{
			_verticalVelocity = Vector3.zero;
		}
	}

	/// <summary>
	/// 	Add spherical locomotion constraint to the character movement. 
	/// </summary>
	private void HandleRopeConstraint()
	{
		// Assert: holding input method
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				// While hold to stop, we don't constraint the character if the player IS NOT holding the button
				if (!_isHolding) return;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				// While hold to let go, we don't constraint the character if the player IS holding the button
				if (_isHolding) return;
				break;
		}

		// Get the distance between the current character's position and the position of the last fold
		Vector3 towardCharacter = m_rsoCharacterPosition.value - _rope.Folds[^1];

		// Re-snap the character's position within the spherical constraint
		if (towardCharacter.magnitude > _rope.HoldLength)
		{
			if (_ropeConstraintTimer != null) StopCoroutine(_ropeConstraintTimer);
			_ropeConstraintTimer = StartCoroutine(AddRopeConstraintTimer());

			transform.position = _rope.Folds[^1] + towardCharacter.normalized * _rope.HoldLength;

			// Transform position of the character controller has been modified outside the movement function
			// Call this unity function to synchronize transform to avoid glitchy movement effects
			Physics.SyncTransforms();
		}
	}

	private IEnumerator AddRopeConstraintTimer()
	{
		_ropeConstraintAppliedLastly = true;
		yield return new WaitForSeconds(1f);
		_ropeConstraintAppliedLastly = false;
	}

	private void HandleClimbing()
	{
		// Assert: holding input method
		switch (m_characterConfig.ropeHoldingMethod)
		{
			case RopeHolding.HOLD_TO_STOP:
				// While hold to stop, we don't constraint the character if the player IS NOT holding the button
				if (!_isHolding) return;
				break;

			case RopeHolding.HOLD_TO_LET_GO:
				// While hold to let go, we don't constraint the character if the player IS holding the button
				if (_isHolding) return;
				break;
		}

		if (!_isClimbing) 
		{
			_currentClimbSpeed = 0f;
			return;
		}

		_currentClimbSpeed += m_characterConfig.climbAcceleration * Time.fixedDeltaTime;
		float clampedClimbSpeed = Mathf.Clamp(_currentClimbSpeed, 0, m_characterConfig.maxClimbSpeed);
		_rope.ChangeHoldLength(-clampedClimbSpeed * Time.fixedDeltaTime);
	}

	private void ToggleRopeHolding(bool enable)
	{
		if (enable)
		{
			_rope.UpdateHoldLength();
			if (_rope.HoldLength == -1) DesequipRope(); // handle error code 
		}
		else
		{
			// Reset the gravity velocity
			m_positionStartFall = m_rsoCharacterPosition.value;
			m_gravitySpeed = 0f;

			if (_ropeConstraintAppliedLastly && IsJumpingPressed) DesequipRope();

			ApplyFreeRopeForce(IsJumpingPressed 
				? m_characterConfig.jumpOffRopeModifier 
				: m_characterConfig.freeFallFromRopeModifier
			);
		}
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

    #region aiming/throwing

	private void ToggleAim(bool _isPressed)
	{
        if (_isPressed && CraftInHand != null)
		{
            Aiming = true;
            // _thirdPersonCamera.SwitchStyle(CameraStyle.AIMING);
            CraftInHand.InitializePreview();
        }
		else if (CraftInHand != null)
		{
            Aiming = false;

			if (CraftInHand != null)
			{
                if (CraftInHand.Throw(m_thirdPersonCamera.transform))
                {
                    // rope attachment exception
                    // _rope = CraftInHand as Rope;
                    // if (_rope != null) _rope?.Attach(m_harness);

                    CraftInHand = null;
                    if (CraftInRobot != null)
                    {
                        CraftInRobot.transform.SetParent(m_handSocket, false);
                        CraftInHand = CraftInRobot;
                        CraftInHand.transform.rotation = m_handSocket.transform.rotation;
                        CraftInRobot = null;
                    }
                }
            }

            // _thirdPersonCamera.SwitchStyle(CameraStyle.BASIC);
        }
	}

    #endregion
	
    #region interaction
	
    private void Interact()
    {
		// Assertion
		if (m_interactables.Count == 0 || _currentState != AnimationState.LOCOMOTION) return;

		Interactable nearest = GetNearestInteractible();
		if (nearest != null) nearest.InteractionTrigger();
    }

    private void Recycle()
    {
		// Assertion
        if (m_interactables.Count == 0 || _currentState != AnimationState.LOCOMOTION) return;

        Interactable interactible = GetNearestInteractible();

		// Assertion
        if (interactible == null) return;
        if (!interactible.IsRecyclable) return;

		m_interactables.Remove(interactible);
		m_validInteractibles.Remove(interactible);
		CheckShowInteract();
		CheckShowRecycle(false);
		interactible.Recycle();
    }

    private Interactable GetNearestInteractible()
	{
		m_validInteractibles = FilterInteractiblesByAngle();
		if (m_validInteractibles.Count == 0) return null;

		return FilterInteractiblesByDistance();
    }

	private List<Interactable> FilterInteractiblesByAngle()
	{
		List<Interactable> validInteractibles = new List<Interactable>();

        for (int i = 0; i < m_interactables.Count; i++)
        {
			// Assertion
			if (m_interactables[i] == null) continue;

			Vector3 towardsInteract = m_interactables[i].transform.position - transform.position;

			if (Vector3.Dot(
				new Vector3(m_characterDirection.transform.forward.x, 0, m_characterDirection.transform.forward.z).normalized, 
				new Vector3(towardsInteract.x, 0, towardsInteract.z).normalized
				) > 0.5)
			{
				validInteractibles.Add(m_interactables[i]);
			}
        }

		return validInteractibles;
    }

	private Interactable FilterInteractiblesByDistance()
	{
		Interactable nearestInteractible = m_validInteractibles[0];

        for (int i = 1; i < m_validInteractibles.Count; i++)
        {
			if ((m_validInteractibles[i].transform.position - transform.position).sqrMagnitude <
            	(nearestInteractible.transform.position - transform.position).sqrMagnitude)
            {
                nearestInteractible = m_validInteractibles[i];
            }
        }

		return nearestInteractible;
    }

    public void AddToInteractList(Interactable _interactibleObject)
    {
        m_interactables.Add(_interactibleObject);
    }

    public void RemoveFromInteractList(Interactable _interactibleObject)
    {
        m_interactables.Remove(_interactibleObject);
		m_validInteractibles.Remove(_interactibleObject);

		CheckShowInteract();
		CheckShowRecycle(false);
    }

	private void CheckShowInteract()
	{
		// m_rseCanInteract.value = (
		// 	m_validInteractibles.Count > 0 
		// 	&& _currentState == AnimationState.LOCOMOTION
		// );
    }

	private void CheckShowRecycle(bool isRecyclable)
	{ 
		m_rsoRecycleInputLocked.value = (
			isRecyclable 
			&& _currentState == AnimationState.LOCOMOTION
		);
	}

	#endregion
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	[SerializeField] private Transform m_backpackAnchor;
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
	[SerializeField] private RSE_Interact m_rseInteract;
	[SerializeField] private RSE_Recycle m_rseRecycle;
	[SerializeField] private RSE_CanInteract m_rseCanInteract;
	[SerializeField] private RSE_CanRecycle m_rseCanRecycle;
	[Space(5)]
	[SerializeField] private RSO_MovementDatas m_rsoMovementDatas;
	[SerializeField] private RSO_CameraStyle m_rsoCameraStyle;

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
    private bool m_hasRope;
    private bool m_isCrafting;

    // - State machine -
    private BehaviorState m_currentState;

	// - Craft state -
	private bool m_hasBackpack;
	private Backpack m_backpack;
	private CraftType m_craftType;
	private Coroutine m_craftCoroutine;
	private Permanent m_handObject;
	private Permanent m_robotObject;
	private bool m_isAiming;

	// - Rope state -
	private Rope m_rope;

	// - Interaction -
	private List<Interactable> m_interactables = new List<Interactable>();

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

        GetBackpackDebug();
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
        m_currentState = BehaviorState.NONE;
    }

    private void FixedUpdate()
	{
        //tkt fréro c'est pour pas soft lock le spherecast de detection du sol
        if (m_rigidbody.position == Vector3.zero)
        {
            m_rigidbody.position = new Vector3(0.01f, 0f, 0f);
        }

		CheckGround();
        DetermineState();
        FixedUpdateState();
    }

    private void LateUpdate()
    {
		MovementDatas _movementDatas = new MovementDatas();

        _movementDatas.dataToString.Add((Mathf.Round(m_rigidbody.velocity.magnitude * 100f) / 100f).ToString());
        _movementDatas.dataToString.Add(m_isGrounded.ToString());
        _movementDatas.dataToString.Add(m_currentState.ToString());
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
    }

    #endif

    #endregion

    #region INPUTS

    private void UnsubscibeAllInputs()
    {
        m_rseMove.action -= UpdateMoveInput;
        m_rseJump.action -= Jump;
		m_rseCraft.action -= ToggleCraft;
		m_rseThrow.action -= ToggleAim;
		m_rseRecycle.action -= Recycle;
		m_rseInteract.action -= Interact;
	}

    private void SubscribeStateInputs()
    {
        switch (m_currentState)
        {
            case BehaviorState.LOCOMOTION:
                m_rseMove.action += UpdateMoveInput;
                m_rseJump.action += Jump;
				m_rseCraft.action += ToggleCraft;
				m_rseThrow.action += ToggleAim;
				m_rseRecycle.action += Recycle;
				m_rseInteract.action += Interact;
				break;

            case BehaviorState.FALL:
                m_rseMove.action += UpdateMoveInput;
                m_rseThrow.action += ToggleAim;
				break;

            case BehaviorState.CRAFT:
				m_rseCraft.action += ToggleCraft;
				break;

            case BehaviorState.ROPE:
				m_rseMove.action += UpdateMoveInput;
				// _rseJump.action += Jump;
				m_rseCraft.action += ToggleCraft;
				m_rseThrow.action += ToggleAim;
				break;
        }
	}

	private void UpdateMoveInput(Vector2 input)
	{
		m_moveInput = input;
	}

	private void UpdateWalkRun(bool ispressed)
	{
		m_isRunning = ispressed;
    }

	public void ToggleCraftInput(bool enable)
	{
		if (enable)
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

	#endregion

	#region STATE MACHINE

	/// <summary>
	/// Determine which behavior state the player should be and trigger a switch of state if neccessary.
	/// </summary>
	private void DetermineState()
    {
        if (m_currentState != BehaviorState.LOCOMOTION && m_isGrounded && !m_isCrafting)
        {
            SwitchState(BehaviorState.LOCOMOTION);
        }
        else if (m_currentState != BehaviorState.FALL && !m_isGrounded && !m_hasRope)
        {
            SwitchState(BehaviorState.FALL);
        }
        else if (m_currentState != BehaviorState.ROPE && !m_isGrounded && m_hasRope)
        {
            SwitchState(BehaviorState.ROPE);
        }
        else if (m_currentState != BehaviorState.CRAFT && m_currentState == BehaviorState.LOCOMOTION && m_isCrafting)
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
        m_currentState = newState;
		SubscribeStateInputs();

		switch (m_currentState)
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
        switch (m_currentState)
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

		switch (m_currentState)
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

    private void Move()
    {
        SetFriction();

        if  (m_moveInput != Vector2.zero)
        {
            Vector3 _desiredSpeed = (m_cameraMotor.PlanarRight * m_moveInput.x + m_cameraMotor.PlanarForward * m_moveInput.y).normalized;
            _desiredSpeed *= m_characterConfig.walkSpeed;
            m_rigidbody.AddForce(_desiredSpeed - m_rigidbody.velocity, ForceMode.Acceleration);
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
        if (m_currentState == BehaviorState.LOCOMOTION)
        {
            m_rigidbody.drag = m_characterConfig.dragGround;
        }
        else if (m_currentState == BehaviorState.FALL || m_currentState == BehaviorState.ROPE)
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

            //orient speed along slope
            Vector3 slopeRight = Vector3.Cross(Vector3.up, m_groundNormal);
            desiredSpeed = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, m_groundNormal, slopeRight), slopeRight) * desiredSpeed;

            //set desired speed magnitude based on walk/run state
            if (m_isRunning)
            {
                desiredSpeed *= m_characterConfig.runSpeed;
            }
            else
            {
                desiredSpeed *= m_characterConfig.walkSpeed;
            }

            //apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
            m_rigidbody.AddForce(desiredSpeed - m_rigidbody.velocity, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// (1) Check if there is valid points to step on
    /// (2) Select the highest point among the point in front the character
    /// (3) On the selected point, check if there is really a object to step on if front
    /// (4) Check if there there is a flat surface to step onto (<45�)
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

                //check if hit is in front of character
                if (Vector3.Dot(moveInput3D, hitDirection) > 0.15)
                {
                    if (_hit.point.y - transform.position.y < m_characterConfig.stepOnHeight)
                    {
                        //We take the highest that is higher than skin width to not trigger step on very small objects
                        if (_hit.point.y > stepOnTarget.y && _hit.point.y > transform.position.y + m_characterConfig.skinWidth)
                        {
                            stepOnTarget = _hit.point;
                        }
                    }
                }
            }

            if(stepOnTarget != transform.position)
            {
                //Check if there is really an object to step on in the speed direction, to prevent steping on end of slope
                Vector3 start = new Vector3(m_rigidbody.position.x, m_rigidbody.position.y + m_characterConfig.skinWidth, m_rigidbody.position.z);
                Vector3 direction = m_rigidbody.velocity.normalized;
                float distance = m_collider.radius * 2;
                if (Physics.Raycast(start, direction, distance, ~m_layerMaskToIgnore))
                {
                    //Check if there is a flat surface to step on (<45�)
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
        if (isPressed)
        {
            if(!m_hasJumped)
            {
                m_rigidbody.AddForce(Vector3.up * m_characterConfig.jumpForce, ForceMode.Impulse);
                m_hasJumped = true;
            }
        }
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

        //set desired speed magnitude based on walk/run state
        if (m_isRunning)
        {
            desiredSpeedForce *= m_characterConfig.runSpeed;
        }
        else
        {
            desiredSpeedForce *= m_characterConfig.walkSpeed;
        }

        //apply final force to move character, auto clamp the speed by substractiong actual speed to desired speed
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
					// _handObject.transform.SetParent(_robotSocket, false);
					// _robotObject = _handObject;
					// _handObject = null;

					SwitchObjects(ref m_handObject, ref m_robotObject, m_robotSocket);
				}
				else if (m_handObject.Type != CraftType.ROPE)
				{
					Destroy(m_handObject.gameObject);
				}
			}
			m_craftCoroutine = StartCoroutine(Craft(CraftType.ROPE, m_ropeConfig.craftingDuration));
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
			
			if (m_backpack != null) m_backpack.EndCrafting();
		}
	}

	private void ToggleCraft(CraftType craftType, bool isInputPressed)
	{
		// Prevent switching to craft state if not in locomotion or crafting state or already crafting another item
		if (m_currentState != BehaviorState.LOCOMOTION 
		|| m_currentState != BehaviorState.CRAFT)
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
		// Wait the crafting duration
		// if (_backpack != null) _backpack.StartCrafting(duration);

		yield return new WaitForSeconds(duration);

		m_handObject = Instantiate(
			craftType == CraftType.TORCH ? (Permanent)m_torchConfig.pfTorch : (Permanent)m_ropeConfig.pfRope, 
			m_handSocket.transform.position,
			Quaternion.identity,
			m_handSocket.transform
		);

		m_backpack.EndCrafting();
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
			if (m_rope != null) m_rope?.Attach(m_harness);

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

	#region INTERACTION

	private void Interact()
	{
		// Assertion
		if (m_interactables.Count == 0 || m_currentState != BehaviorState.LOCOMOTION) return;

		GetNearestInteractable()?.InteractionTrigger();
	}

	private void Recycle()
	{
		// Assertion
		if (m_interactables.Count == 0 || m_currentState != BehaviorState.LOCOMOTION) return;

		Interactable interactable = GetNearestInteractable();

		// Assert: interactable isn't valid
		if (!interactable || !interactable.IsRecyclable) return;

		Remove(interactable, doRecycle: true);
	}

	/// <summary>
	/// 	Add the given interactable into the interactable list.
	/// </summary>
	public void Add(Interactable interactable)
	{
		m_interactables.Add(interactable);
	}

	/// <summary>
	/// 	Remove the given interactable from the interactable list.
	/// 	The character will no longer be able to interact with it.
	/// </summary>
	public void Remove(Interactable interactable, bool doRecycle = false)
	{
		interactable.IsValid = false;
		m_interactables.Remove(interactable);

		CheckShowInteract();
		CheckShowRecycle(false);

		if (doRecycle) interactable.Recycle();
	}

	/// <summary>
	/// 	Return the nearest interactable in front of the character.
	/// </summary>
	private Interactable GetNearestInteractable()
	{
		// - Get interactable in front of the character -
		var counter = 0;
		foreach (var interactable in m_interactables)
		{
			// Assertion
			if (!interactable) continue;

			Vector3 towardsInteract = interactable.transform.position - transform.position;

			interactable.IsValid = Vector3.Dot(
				new Vector3(m_characterGraphics.transform.forward.x, 0, m_characterGraphics.transform.forward.z).normalized,
				new Vector3(towardsInteract.x, 0, towardsInteract.z).normalized
			) > 0.5f;
			
			if (interactable.IsValid) counter++;
		}

		// Assert: there is no interactable in front of the character.
		if (counter == 0) return null;

		// - Get the nearest interactable object from the character -
		var nearest = m_interactables.FirstOrDefault(i => i.IsValid);
		foreach (var valid in m_interactables.Where(i => i.IsValid))
		{
			if ((valid.transform.position - transform.position).sqrMagnitude <
				(nearest.transform.position - transform.position).sqrMagnitude)
			{
				nearest = valid;
			}
		}
		return nearest;
	}

	private void CheckShowInteract()
	{
		m_rseCanInteract.Call(
			m_interactables.Count(i => i.IsValid) > 0
			&& m_currentState == BehaviorState.LOCOMOTION
		);
	}

	private void CheckShowRecycle(bool isRecyclable)
	{
		m_rseCanRecycle.Call(
			isRecyclable
			&& m_currentState == BehaviorState.LOCOMOTION
		);
	}

	#endregion

	#region BACKPACK

	private void GetBackpackDebug()
	{
		// Assertion
		if (m_characterConfig.startWithBag) return;

		m_backpack = FindAnyObjectByType<Backpack>();
		if (m_backpack == null) m_backpack = Instantiate(m_characterConfig.pfBackpack);
		m_backpack.ForceSetupBackpack(this);
	}

	public void Pickup(Backpack backpack)
	{
		m_backpack = backpack;
		m_hasBackpack = true;

		ToggleCraftInput(m_hasBackpack);
		m_backpack.transform.SetParent(m_backpackAnchor.transform, false);
		m_backpack.transform.localPosition = Vector3.zero;
		m_backpack.transform.localRotation = Quaternion.identity;
		m_backpack.transform.localScale = Vector3.one;
		Remove(m_backpack);
	}

	#endregion
}
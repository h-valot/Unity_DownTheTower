using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
	#region REFERENCES

	[Title("Tweakable values")]
	[SerializeField] private SSO_Guardian m_ssoGuardian;

	[InfoBox("If true (default), on patrol state, the guardian will follow the given path. Otherwise, it will return to the given waypoint if it returns to patrol state (NOTE: The guardian will be look towards the waypoint forward).", InfoMessageType.None)]
	[SerializeField] private bool m_usePatrolPath = true;
	[PropertySpace(SpaceAfter = 15f, SpaceBefore = 0f)]
	[ShowIf("m_usePatrolPath")][SerializeField] private PatrolPath m_patrolPath;
	[PropertySpace(SpaceAfter = 15f, SpaceBefore = 0f)]
	[HideIf("m_usePatrolPath")][SerializeField] private Waypoint m_waypoint;

	[FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent m_agent;
	[FoldoutGroup("Internal References")][SerializeField] private MeshRenderer m_guardianMeshRenderer;
    [FoldoutGroup("Internal References")][SerializeField] private Transform m_beamTransform;
    [FoldoutGroup("Internal References")][SerializeField] private MeshRenderer m_beamMeshRenderer;
    [FoldoutGroup("Internal References")][SerializeField] private Light m_beamLight;
    [FoldoutGroup("Internal References")][SerializeField] private Transform m_frontEye;
	[FoldoutGroup("Internal References")][SerializeField] private GuardianActivator m_activator;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_GuardianFootstep m_rseGuardianFootstep;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;

	#endregion

	#region VARIABLES

	private bool m_canSwitchState = true;
	private GuardianBehaviorState m_currentState;

	// Patrol
	[SerializeField] private bool IsPatrolPathValid => m_patrolPath && m_patrolPath.Waypoints.Count > 0;
	private int m_currentWaypoint;
	private float m_updateWaypointTimer;
	private float m_currentAngleSight;

	// Aggro
	public List<Candidate> m_candidates = new List<Candidate>();
	private Candidate m_currentTarget;
	private float m_minTargetDistance;
	private bool m_hasTargetInSight;
	private float m_aggroTimeoutTimer;
	private Vector3 m_startAggroPosition;

	// Seek
	private float m_omniscienceTimer;
	private Candidate m_omniscienceTarget;
	private float m_seekingTimer;
	private bool m_targetNotFound;
	private int m_seekTargetId;
	private float m_seekTimeoutTimer;
	private float m_stuckTimeoutTimer;
	private Vector3 m_lastPosition;

	// Graphics
	private MaterialPropertyBlock m_guardianPropertyBlock;
    private MaterialPropertyBlock m_beamPropertyBlock;

    #endregion

    #region MONOBEHAVIOR

	private void Awake()
	{
		UpdateBeamGraphics(m_ssoGuardian.PatrolColor, m_ssoGuardian.PatrolFocus, m_ssoGuardian.PatrolOpacity);
	}

    private void Start()
	{
		m_currentState = GuardianBehaviorState.SEEK;
		ToggleAngleSightExtension(false);

		UpdateCandidates();
		SelectTarget();
		DetermineState();
    }

    private void Update()
    {
		// Assertion
		if (!m_activator.IsActive) return;

		UpdateCandidates();
		SelectTarget();
        DetermineState();
        UpdateState();
		HandleSteps();

		m_lastPosition = transform.position;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			StartCoroutine(AnimateCharacterKill(character));
		}

		if (collider.TryGetComponent<Torch>(out var torch))
		{
			StartCoroutine(AnimateTorchDestroy(torch));
		}

		if (collider.TryGetComponent<Rope>(out var rope))
		{
			StartCoroutine(AnimateRopeDestroy(rope));
		}
	}

	#endregion

	#region STATE MACHINE

	private void UpdateCandidates()
	{
		// Assertions
		if (m_rsoTorchManager.value == null) return;
		if (m_rsoRopes.value == null) return;

		m_candidates.ForEach(c => c.IsUpdated = false);

		// Update character's candidate
		int id = 0;
		if (m_candidates.Any(c => c.Id == id))
		{
			m_candidates.First(c => c.Id == id).Update(m_rsoCharacterPosition.value + Vector3.up * 1f);
		}
		else
		{
			m_candidates.Add(new Candidate(id, m_rsoCharacterPosition.value + Vector3.up * 1f));
		}

		// Update torch candidates
		foreach (var torch in m_rsoTorchManager.value.Torches)
		{
			if (torch.IsInHand) continue;	// Assert: The guardian sees the character not the torch they hold

			id = torch.GetHashCode();
			if (m_candidates.Any(c => c.Id == id))
			{
				m_candidates.First(c => c.Id == id).Update(torch.RaycastTarget.position);
			}
			else
			{
				m_candidates.Add(new Candidate(id, torch.RaycastTarget.position));
			}
		}

		// Update rope candidates
		foreach (var rope in m_rsoRopes.value)
		{
			if (rope.IsOnBackpack) continue;	// Assert: The guardian sees the character not the rope in the backpack

			id = rope.GetHashCode();
			if (m_candidates.Any(c => c.Id == id))
			{
				m_candidates.First(c => c.Id == id).Update(rope.RaycastTarget.position);
			}
			else
			{
				m_candidates.Add(new Candidate(id, rope.RaycastTarget.position));
			}
		}

		// Remove unused candidates
		for (int i = m_candidates.Count - 1; i >= 0; i--)
		{
			// Assert: Don't remove updated candidate
			if (m_candidates[i].IsUpdated) continue;

			m_candidates.Remove(m_candidates[i]);
		}
	}

	private void SelectTarget()
	{
		// Select a candidate
		m_minTargetDistance = m_ssoGuardian.LongRange;
		Candidate bestTarget = m_currentTarget;
		m_hasTargetInSight = false;
		foreach (var candidate in m_candidates)
		{
			// Assert: ban candidates are ignored
			if (candidate.IsBan) continue;

			// Assert: there is a better target near to the guardian
			float distance = Vector3.Distance(transform.position, candidate.Position);
			if (distance > m_minTargetDistance) continue;

			// Assert: the linecast touches nothing
			Debug.DrawLine(m_frontEye.position, candidate.Position);
			if (!Physics.Linecast(m_frontEye.position, candidate.Position, out var hit, ~m_ssoGuardian.TargetLayerToIgnore)) continue;

			// Assert: the candidate isn't a valid class
			bool isCharacter = !hit.collider.TryGetComponent<CharacterMotor>(out var character);
			bool isTorch = !hit.collider.TryGetComponent<Torch>(out var torch);
			bool isRope = !hit.collider.TryGetComponent<Rope>(out var rope);
			if (!isCharacter && !isTorch && !isRope) continue;

			// Assert: torch and rope are still in character's hand
			if (torch && torch.IsInHand) continue;
			if (rope && !rope.IsPlaced) continue;

			// Gather candidate informations such as lighting and positioning
			Vector3 guardianCandidateDirection = (candidate.Position - transform.position).normalized;
			bool isTargetInSightCone = Vector3.Dot(transform.forward, guardianCandidateDirection) >= m_currentAngleSight;
			bool isTargetLit = character && character.IsCarryingLight() || torch && torch.IsLit;

			// Assertions
			if (!isTargetLit && !isTargetInSightCone && distance > m_ssoGuardian.LethalRange) continue;
			if (isTargetLit && !isTargetInSightCone && distance > m_ssoGuardian.ClearRange) continue;
			if (isTargetLit && isTargetInSightCone && distance > m_ssoGuardian.LongRange) continue;
			if (!isTargetLit && distance > m_ssoGuardian.ClearRange) continue;

			// Update the best target with the candidate
			m_hasTargetInSight = true;
			m_minTargetDistance = distance;
			bestTarget = candidate;
		}

		// Set candidate as the current target
		m_currentTarget = bestTarget;
    }

    /// <summary>
    /// Determine which behavior state the player should be and trigger a switch of state if neccessary.
    /// </summary>
    private void DetermineState()
	{
		// Don't switch state while a coroutine is running
		if (!m_canSwitchState) return;

		if (m_currentState != GuardianBehaviorState.PATROL
		&& m_currentState == GuardianBehaviorState.SEEK 
		&& m_targetNotFound
		&& !GetCandidateById(0).IsAggroedLately)
		{
			m_targetNotFound = false;
            SwitchState(GuardianBehaviorState.PATROL);
			return;
		}

        if (m_currentState != GuardianBehaviorState.AGGRO 
		&& m_hasTargetInSight)
		{
			SwitchState(GuardianBehaviorState.AGGRO);
			return;
		}

		if (m_currentState != GuardianBehaviorState.SEEK
		&& m_currentState == GuardianBehaviorState.AGGRO
		&& !m_hasTargetInSight)
		{
			SwitchState(GuardianBehaviorState.SEEK);
			return;
		}
	}

    private void SwitchState(GuardianBehaviorState newState, bool exitCurrentState = true)
	{
		if (exitCurrentState) ExitState();
        EnterState(newState);
	}

	private void ExitState()
	{
		switch (m_currentState)
		{
			case GuardianBehaviorState.PATROL:
				ExitPatrolState();
				break;

			case GuardianBehaviorState.AGGRO:
				ExitAggroState();
				break;

			case GuardianBehaviorState.SEEK:
				ExitSeekState();
				break;
		}
	}

	private void EnterState(GuardianBehaviorState newState)
	{
		// Don't switch state while a coroutine is running
		if (!m_canSwitchState) return;

		m_currentState = newState;

        switch (m_currentState)
        {
            case GuardianBehaviorState.PATROL:
                EnterPatrolState();
                break;

            case GuardianBehaviorState.AGGRO:
                EnterAggroState();
                break;

			case GuardianBehaviorState.SEEK:
				EnterSeekState();
				break;
		}
    }
    
    private void UpdateState()
	{
		switch (m_currentState)
        {
            case GuardianBehaviorState.PATROL:
                UpdatePatrolState();
                break;

            case GuardianBehaviorState.AGGRO:
                UpdateAggroState();
                break;

			case GuardianBehaviorState.SEEK:
				UpdateSeekState();
				break;
		}
    }

    #endregion

    #region PATROL

    private void EnterPatrolState()
	{
		UpdateBeamGraphics(m_ssoGuardian.PatrolColor, m_ssoGuardian.PatrolFocus, m_ssoGuardian.PatrolOpacity);

		m_agent.destination = m_usePatrolPath ? m_patrolPath.Waypoints[m_currentWaypoint].Position : m_waypoint.Position;
		m_agent.speed = m_ssoGuardian.PatrolSpeed;

		m_beamTransform.localRotation = Quaternion.Euler(20f,0,0);
	}

    private void UpdatePatrolState()
    {
		// Assertion
		if (!m_usePatrolPath) 
		{
			Reorientate();
			return;
		}

		Patrolling();
		CheckWaypoints();
	}

    private void ExitPatrolState()
    {
		// Do nothing
	}

	private void Reorientate()
	{
		if ((transform.position - m_waypoint.Position).magnitude <= m_ssoGuardian.WaypointDistanceTolerance)
		{
			transform.LookAt(m_waypoint.transform.position + m_waypoint.transform.forward);
		}
	}

	private void Patrolling()
	{
		// Assertion
		if (!IsPatrolPathValid) return;

		if ((transform.position - m_patrolPath.Waypoints[m_currentWaypoint].Position).magnitude <= m_ssoGuardian.WaypointDistanceTolerance)
		{
			m_currentWaypoint++;
			if (m_currentWaypoint >= m_patrolPath.Waypoints.Count) m_currentWaypoint = 0;

			m_updateWaypointTimer = m_patrolPath.Waypoints[m_currentWaypoint].OverrideWaitDurationOnWaypointReached
				? m_patrolPath.Waypoints[m_currentWaypoint].WaitDurationOnWaypointReached
				: m_ssoGuardian.WaitDurationOnWaypointReached;
		}
	}

	private void CheckWaypoints()
	{
		if (m_updateWaypointTimer <= 0) return;

		m_updateWaypointTimer -= Time.fixedDeltaTime;
		if (m_updateWaypointTimer <= 0)
		{
			m_agent.destination = m_patrolPath.Waypoints[m_currentWaypoint].Position;
		}
	}

	#endregion

	#region AGGRO

	private void EnterAggroState()
    {
		UpdateBeamGraphics(m_ssoGuardian.AggroColor, m_ssoGuardian.AggroFocus, m_ssoGuardian.AggroOpacity);
        m_agent.speed = m_ssoGuardian.AggroSpeed;
		m_aggroTimeoutTimer = m_ssoGuardian.AggroTimeout;
		m_startAggroPosition = transform.position;
	}

    private void UpdateAggroState()
	{
		ChaseTarget();
		HandleLockedByEnviro();
		HandleStuckTimeout();

		m_beamTransform.LookAt(m_currentTarget.Position);
    }

    private void ExitAggroState()
    {
		// Do nothing
	}

	private void ChaseTarget()
	{
		// Assertion
		if (!m_hasTargetInSight) return;

		m_agent.destination = m_currentTarget.Position;
		m_currentTarget.IsAggroedLately = true;
	}

	private void HandleLockedByEnviro()
	{
		m_aggroTimeoutTimer -= Time.deltaTime;
		if (m_aggroTimeoutTimer <= 0f)
		{
			if ((m_startAggroPosition - transform.position).magnitude <= m_ssoGuardian.LockedThreshold)
			{
				m_hasTargetInSight = false;
			}
			else
			{
				m_aggroTimeoutTimer = m_ssoGuardian.AggroTimeout;
			}
		}
	}

	private IEnumerator AnimateCharacterKill(CharacterMotor character)
	{
		m_canSwitchState = false;
		yield return new WaitForSeconds(m_ssoGuardian.DelayKillCharacter.x);
		character.HandleDeath(DeathType.GUARDIAN);
		yield return new WaitForSeconds(m_ssoGuardian.DelayKillCharacter.y);
		m_canSwitchState = true;
		m_targetNotFound = true;
	}

	private IEnumerator AnimateTorchDestroy(Torch torch)
	{
		m_canSwitchState = false;
		yield return new WaitForSeconds(m_ssoGuardian.DelayDestroyTorch.x);
		m_rsoTorchManager.value.Remove(torch, true);
		yield return new WaitForSeconds(m_ssoGuardian.DelayDestroyTorch.y);
		m_canSwitchState = true;
		m_targetNotFound = true;
	}

	private IEnumerator AnimateRopeDestroy(Rope rope)
	{
		m_canSwitchState = false;
		yield return new WaitForSeconds(m_ssoGuardian.DelayDestroyRope.x);
		rope.Detach();
		Destroy(rope.gameObject);
		yield return new WaitForSeconds(m_ssoGuardian.DelayDestroyRope.y);
		m_canSwitchState = true;
		m_targetNotFound = true;
	}

	#endregion

	#region SEEK

	private void EnterSeekState()
	{
		UpdateBeamGraphics(m_ssoGuardian.SeekColor, m_ssoGuardian.SeekFocus, m_ssoGuardian.SeekOpacity);

		m_omniscienceTimer = m_ssoGuardian.OmniscienceDuration;
		m_seekingTimer = m_ssoGuardian.SeekingDuration;
		m_seekTimeoutTimer = m_ssoGuardian.SeekTimeoutTimer;

		m_seekTargetId = m_currentTarget.Id;
		m_omniscienceTarget = GetCandidateById(m_seekTargetId);
		m_targetNotFound = false;

        m_beamTransform.localRotation = Quaternion.Euler(0, 0, 0);
    }

	private void UpdateSeekState()
	{
		HandleOmniscience();
		HandleSeek();
		HandleStuckTimeout();
		HandleSeekTimeout();
	}

	private void ExitSeekState()
	{
		ToggleAngleSightExtension(false);
	}

	private void HandleOmniscience()
	{
		m_omniscienceTarget = GetCandidateById(m_seekTargetId);

		m_omniscienceTimer -= Time.fixedDeltaTime;
		if (m_omniscienceTimer <= 0) 
		{
			if (m_omniscienceTarget == null) return;    		// Assert: Target at id doesn't exist
			if (!m_omniscienceTarget.IsAggroedLately) return;   // Assert: Target hasn't been chased lately
			if (m_omniscienceTarget.IsBan) return;				// Assert: Target is ban - it can't be seek anymore

			m_agent.destination = m_omniscienceTarget.Position;
		}
	}

	private void HandleSeek()
	{
		if ((transform.position - m_agent.destination).magnitude > m_ssoGuardian.WaypointDistanceTolerance) return;

		ToggleAngleSightExtension(true);

		m_seekingTimer -= Time.fixedDeltaTime;
		if (m_seekingTimer < 0f)
		{
			// Seek back to the last character position is chased lately
			if (GetCandidateById(0).Id != -1		// The character candidate is valid
			&& GetCandidateById(0).IsAggroedLately	// AND The character has been chased at least once since the last time the guardian was patrolling
			&& m_omniscienceTarget.Id != 0)         // AND The current target the guardian is seeking isn't the character
			{
				m_omniscienceTimer = m_ssoGuardian.OmniscienceDuration;
				m_seekingTimer = m_ssoGuardian.SeekingDuration;
				m_seekTimeoutTimer = m_ssoGuardian.SeekTimeoutTimer;

				m_seekTargetId = 0;
				return;
			}

			m_targetNotFound = true;
		}
	}

	private void HandleStuckTimeout()
	{
		// Assert: the position of the guardian is actually changing
		if (transform.position.CutDigits(1) != m_lastPosition.CutDigits(1)) 
		{
			m_stuckTimeoutTimer = m_ssoGuardian.StuckTimeoutTimer;
			return;
		}

		m_stuckTimeoutTimer -= Time.fixedDeltaTime;
		if (m_stuckTimeoutTimer <= 0f)
		{
			m_targetNotFound = true;
			if (CurrentTarget != null) 
			{
				if (CurrentTarget.Id != 0)
				{
					// Ban unreachable targets
					CurrentTarget.IsBan = true;
				}
				else
				{
					// Unaggro the character instead of banning it
					CurrentTarget.IsAggroedLately = false;
				}
			}
		}
	}

	private void HandleSeekTimeout()
	{
		m_seekTimeoutTimer -= Time.fixedDeltaTime;
		if (m_seekTimeoutTimer <= 0f)
		{
			m_targetNotFound = true;
			if (m_omniscienceTarget != null) m_omniscienceTarget.IsAggroedLately = false;
		}
	}

	private void ToggleAngleSightExtension(bool isEnabled)
	{
		m_currentAngleSight = isEnabled ? m_ssoGuardian.ExtendedAngleSight : m_ssoGuardian.DefaultAngleSight;
	}

	/// <summary>
	/// Returns the current target chased or seeked based on the current guardian state.
	/// </summary>
	private Candidate CurrentTarget => m_currentState == GuardianBehaviorState.SEEK ? m_omniscienceTarget : m_currentTarget;

	/// <summary>
	/// Return the candidate filter by id using FirstOrDefault function. Note: 0 is the character.
	/// </summary>
	private Candidate GetCandidateById(int id)
	{
		var candidate = m_candidates.FirstOrDefault(c => c.Id == id);
		if (candidate == null)
		{
			return new Candidate(-1, Vector3.zero);
		}
		else
		{
			return candidate;
		}
	}

	#endregion

	#region GRAPHICS

	// - TEMPORARY REGION BEGINS -
	// This function simulate the walking animation of the guardian to call periodically the OnFootstep() function
	// TODO - Connect the guardian walk and run animation to the OnFootstep() function, then delete this temporary segment
	private float m_stepTimer;
	private void HandleSteps()
	{
		// Assert: The guardian isn't moving
		if (transform.position.CutDigits(3) == m_lastPosition.CutDigits(3)) return;

		m_stepTimer -= Time.deltaTime;
		if (m_stepTimer <= 0)
		{
			m_stepTimer = m_agent.speed / 3f;
			OnFootstep();
		}
	}
	// - TEMPORARY REGION ENDS -

	private void OnFootstep()
	{
		float distanceCharacter = Vector3.Distance(m_rsoCharacterPosition.value, transform.position);
		float stepStrengthPercent = m_ssoGuardian.FootstepCurve.Evaluate(1 - Mathf.Clamp01(distanceCharacter / m_ssoGuardian.LongRange));
		m_rseGuardianFootstep.Call(stepStrengthPercent);
	}

	private void UpdateBeamGraphics(Color newColor, float focusPercent, float opacity)
	{
		m_beamLight.color = newColor;

		if (m_beamPropertyBlock == null) m_beamPropertyBlock = new MaterialPropertyBlock();
		m_beamPropertyBlock.SetColor("_beamColor", newColor);
		m_beamPropertyBlock.SetFloat("_focus", focusPercent);
        m_beamPropertyBlock.SetFloat("_opacity", opacity);
		m_beamMeshRenderer.SetPropertyBlock(m_beamPropertyBlock);

		if (m_guardianPropertyBlock == null) m_guardianPropertyBlock = new MaterialPropertyBlock();
		m_guardianPropertyBlock.SetColor("_EyesColor", newColor);
		m_guardianMeshRenderer.SetPropertyBlock(m_guardianPropertyBlock);
	}

	#endregion
}

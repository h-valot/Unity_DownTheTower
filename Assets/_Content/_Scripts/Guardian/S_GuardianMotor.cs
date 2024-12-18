using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
	#region REFERENCES

	[Title("Override")]
	[InfoBox("If true, the following value will override the one filled in the SSO_Guardian. NOTE: Use this to create unique guardian.", InfoMessageType.None)]
	[SerializeField] private bool m_overridePatrolSpeed;
	[ShowIf("m_overridePatrolSpeed")][SerializeField] private float m_patrolSpeed;

	[InfoBox("If true, the following value will override the one filled in the SSO_Guardian.", InfoMessageType.None)]
	[SerializeField] private bool m_overrideAggroSpeed;
	[ShowIf("m_overrideAggroSpeed")][SerializeField] private float m_aggroSpeed;

	[InfoBox("If true (default), on patrol state, the guardian will follow the given path. Otherwise, it will return to the given waypoint if it returns to patrol state (NOTE: The guardian will be look towards the waypoint forward).", InfoMessageType.None)]
	[SerializeField] private bool m_usePatrolPath = true;
	[PropertySpace(SpaceAfter = 15f, SpaceBefore = 0f)]
	[ShowIf("m_usePatrolPath")][SerializeField] private PatrolPath m_patrolPath;
	[PropertySpace(SpaceAfter = 15f, SpaceBefore = 0f)]
	[HideIf("m_usePatrolPath")][SerializeField] private Waypoint m_waypoint;

	[FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent m_agent;
	[FoldoutGroup("Internal References")][SerializeField] private MeshRenderer m_eyes;
	[FoldoutGroup("Internal References")][SerializeField] private TextMeshProUGUI m_tmpTarget;
	[FoldoutGroup("Internal References")][SerializeField] private TextMeshProUGUI m_tmpState;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Guardian m_ssoGuardian;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GuardianState m_rsoGuardianState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;

	#endregion

	#region VARIABLES

	[Title("Debug")]
	private bool m_canSwitchState = true;

	// Patrol
	public bool IsPatrolPathValid => m_patrolPath && m_patrolPath.Waypoints.Count > 0;
	private int m_currentWaypoint;
	private float m_updateWaypointTimer;
	private float m_currentAngleSight;

	// Aggro
	public List<Candidate> m_candidates = new List<Candidate>();
	private Candidate m_currentTarget;
	private float m_minTargetDistance;
	private bool m_hasTargetInSight;
	public bool m_characterAggroedLately;

	// Seek
	private float m_omniscienceTimer;
	private Candidate m_omniscienceTarget;
	private float m_seekingTimer;
	private bool m_targetNotFound;
	private int m_seekTargetId;

	#endregion

	#region MONOBEHAVIOR

	private void Start()
	{
		m_rsoGuardianState.value = GuardianBehaviorState.SEEK;
		ToggleAngleSightExtension(false);

		SelectTarget();
		DetermineState();
	}

    private void Update()
    {
        SelectTarget();
        DetermineState();
        UpdateState();

        UpdateDebugUI();
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

	private void SelectTarget()
	{
		// Fill candidates
		int id = 0;
		m_candidates = new List<Candidate> { new Candidate(id, m_rsoCharacterPosition.value + Vector3.up * 0.8f) };
		foreach (var torch in m_rsoTorchManager.value.Torches)
		{
			id++;
			m_candidates.Add(new Candidate(id, torch.RaycastTarget.position));
		}
		foreach (var rope in m_rsoRopes.value)
		{
			id++;
			m_candidates.Add(new Candidate(id, rope.RaycastTarget.position));
		}

		// Select a candidate
		m_minTargetDistance = m_ssoGuardian.MaxRange;
		Candidate bestTarget = m_currentTarget;
		m_hasTargetInSight = false;
		foreach (var candidate in m_candidates)
		{
			// Assert: there is a better target near to the guardian
			float distance = Vector3.Distance(transform.position, candidate.Position);
			if (distance > m_minTargetDistance) continue;

			// Assert: the candidate isn't a valid class
			Debug.DrawLine(m_eyes.transform.position, candidate.Position);
			Physics.Linecast(m_eyes.transform.position, candidate.Position, out var hit, ~m_ssoGuardian.TargetLayerToIgnore);
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
			if (!isTargetLit && !isTargetInSightCone) continue;
			if (isTargetLit && !isTargetInSightCone && distance > m_ssoGuardian.MinRange) continue;
			if (isTargetLit && isTargetInSightCone && distance > m_ssoGuardian.MaxRange) continue;
			if (!isTargetLit && distance > m_ssoGuardian.MinRange) continue;

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

		if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL
		&& m_rsoGuardianState.value == GuardianBehaviorState.SEEK 
		&& m_targetNotFound
		&& !m_characterAggroedLately)
		{
			print("entering patrol");
			m_targetNotFound = false;
            SwitchState(GuardianBehaviorState.PATROL);
			return;
		}

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO 
		&& m_hasTargetInSight)
		{
			print("entering aggro");
			SwitchState(GuardianBehaviorState.AGGRO);
			return;
		}

		if (m_rsoGuardianState.value != GuardianBehaviorState.SEEK
		&& m_rsoGuardianState.value == GuardianBehaviorState.AGGRO
		&& !m_hasTargetInSight)
		{
			print("entering seek");
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
		switch (m_rsoGuardianState.value)
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

		m_rsoGuardianState.value = newState;

        switch (m_rsoGuardianState.value)
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
		switch (m_rsoGuardianState.value)
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
		m_eyes.sharedMaterial = m_ssoGuardian.PatrolMaterial;
		m_agent.destination = m_usePatrolPath ? m_patrolPath.Waypoints[m_currentWaypoint].Position : m_waypoint.Position;
		m_agent.speed = m_overridePatrolSpeed ? m_patrolSpeed : m_ssoGuardian.PatrolSpeed;
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
		m_eyes.sharedMaterial = m_ssoGuardian.AggroMaterial;
		m_agent.speed = m_overrideAggroSpeed ? m_aggroSpeed : m_ssoGuardian.AggroSpeed;
	}

    private void UpdateAggroState()
	{

		ChaseTarget();
    }

    private void ExitAggroState()
    {

	}

	private void ChaseTarget()
	{
		// Assertion
		if (!m_hasTargetInSight) return;

		m_agent.destination = m_currentTarget.Position;
		if (!m_characterAggroedLately && m_currentTarget.Id == 0)
		{
			m_characterAggroedLately = true;
		}
	}

	private IEnumerator AnimateCharacterKill(CharacterMotor character)
	{
		m_canSwitchState = false;
		yield return new WaitForSeconds(m_ssoGuardian.DelayKillCharacter.x);
		character.HandleDeath();
		yield return new WaitForSeconds(m_ssoGuardian.DelayKillCharacter.y);
		m_canSwitchState = true;
		m_targetNotFound = true;
	}

	private IEnumerator AnimateTorchDestroy(Torch torch)
	{
		m_canSwitchState = false;
		yield return new WaitForSeconds(m_ssoGuardian.DelayDestroyTorch.x);
		m_rsoTorchManager.value.Remove(torch);
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
		m_omniscienceTimer = m_ssoGuardian.OmniscienceDuration;
		m_seekingTimer = m_ssoGuardian.SeekingDuration;

		m_seekTargetId = m_currentTarget.Id;
		m_omniscienceTarget = GetCandidateById(m_seekTargetId);
		m_targetNotFound = false;
	}

	private void UpdateSeekState()
	{
		HandleOmniscience();
		HandleSeek();
	}

	private void ExitSeekState()
	{

	}

	private void HandleOmniscience()
	{
		m_omniscienceTimer -= Time.fixedDeltaTime;
		m_omniscienceTarget = GetCandidateById(m_seekTargetId);

		if (m_omniscienceTimer > 0f
		&& m_omniscienceTarget.Id != -1)
		{
			m_agent.destination = m_omniscienceTarget.Position;
		}
	}

	private void HandleSeek()
	{
		if ((transform.position - m_agent.destination).magnitude > m_ssoGuardian.WaypointDistanceTolerance) return;

		ToggleAngleSightExtension(true);

		m_seekingTimer -= Time.fixedDeltaTime;
		if (m_seekingTimer >= 0f) return;

		if (m_characterAggroedLately 
		&& m_seekTargetId != 0)
		{
			m_characterAggroedLately = false;
			m_omniscienceTimer = m_ssoGuardian.OmniscienceDuration;
			m_seekingTimer = m_ssoGuardian.SeekingDuration;
			m_seekTargetId = 0;
			return;
		}

		ToggleAngleSightExtension(false);
		m_targetNotFound = true;
	}

	private void ToggleAngleSightExtension(bool isEnabled)
	{
		m_currentAngleSight = isEnabled ? m_ssoGuardian.ExtendedAngleSight : m_ssoGuardian.DefaultAngleSight;
	}

	private Candidate GetCandidateById(int id)
	{
		foreach (var candidate in m_candidates.Where(c => c.Id == id))
		{
			return candidate;
		}
		return new Candidate(-1, Vector3.zero);
	}

	#endregion

	#region DEBUG

	private void UpdateDebugUI()
	{
		m_tmpState.text = m_rsoGuardianState.value.ToString();
		m_tmpTarget.text = m_hasTargetInSight ? m_currentTarget.Position.ToString() : "none";
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, m_ssoGuardian.MinRange);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, m_ssoGuardian.MaxRange);
	}

    #endregion
}

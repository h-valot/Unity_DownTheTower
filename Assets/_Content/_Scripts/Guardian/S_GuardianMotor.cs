using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
	#region REFERENCES

	[Title("Override")]
	[SerializeField] private bool m_overridePatrolSpeed;
	[ShowIf("m_overridePatrolSpeed")][SerializeField] private float m_patrolSpeed;

	[SerializeField] private bool m_overrideAggroSpeed;
	[ShowIf("m_overrideAggroSpeed")][SerializeField] private float m_aggroSpeed;

	[SerializeField] private bool m_usePatrolPath = true;
	[ShowIf("m_usePatrolPath")][SerializeField] private PatrolPath m_patrolPath;
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

	#endregion

	#region VARIABLES

	[Title("Debug")]

	// Aggro
	public List<Vector3> m_candidateTargetPositions = new List<Vector3>();
	private Vector3 m_currentTargetPosition;
	private float m_minTargetDistance;
	public bool m_hasTargetInSight;

	// Patrol
	public bool IsPatrolPathValid => m_patrolPath && m_patrolPath.Waypoints.Count > 0;
	public int m_currentWaypoint;
	private float m_updateWaypointTimer;

	private bool m_isCoroutineRunning;

	#endregion

	#region MONOBEHAVIOR

	private void Start()
	{
		m_rsoGuardianState.value = GuardianBehaviorState.NONE;
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
	}

	#endregion

	#region STATE MACHINE

	private void SelectTarget()
	{
		// Fill candidates
		m_candidateTargetPositions = new List<Vector3> { m_rsoCharacterPosition.value + Vector3.up * 0.8f };
		foreach (var torch in m_rsoTorchManager.value.Torches)
		{
			m_candidateTargetPositions.Add(torch.RaycastTarget.position);
		}

		// Select a candidate
		m_minTargetDistance = m_ssoGuardian.SightRange;
		Vector3 bestTargetPosition = m_currentTargetPosition;
		m_hasTargetInSight = false;
		foreach (var candidateTargetPosition in m_candidateTargetPositions)
		{
			// Assert: there is a better target near to the guardian
			float distance = Vector3.Distance(transform.position, candidateTargetPosition);
			if (distance > m_minTargetDistance) continue;

			// Assert: the candidate is outside the sight or the passive range
			Vector3 guardianCandidateDirection = (candidateTargetPosition - transform.position).normalized;
			bool isTargetInSightCone = Vector3.Dot(transform.forward, guardianCandidateDirection) >= m_ssoGuardian.AngleSight;
			if (distance > (isTargetInSightCone ? m_ssoGuardian.SightRange : m_ssoGuardian.PassiveRange)) continue;

			// Assert: the target isn't in direct sight
			Physics.Linecast(m_eyes.transform.position, candidateTargetPosition, out var hit, ~m_ssoGuardian.TargetLayerToIgnore);
			if (!hit.collider.TryGetComponent<CharacterMotor>(out var character) && !hit.collider.TryGetComponent<Torch>(out var torch)) continue;

			// Update the best target position with the candidate
			m_hasTargetInSight = true;
			m_minTargetDistance = distance;
			bestTargetPosition = candidateTargetPosition;

		}

		// Set candidate as the current target
		if (m_currentTargetPosition != bestTargetPosition) 
		{
			m_currentTargetPosition = bestTargetPosition;
		}
    }

    /// <summary>
    /// Determine which behavior state the player should be and trigger a switch of state if neccessary.
    /// </summary>
    private void DetermineState()
    {
		// Don't change state while a coroutine is running
		if (m_isCoroutineRunning) return;

        if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL && !m_hasTargetInSight)
        {
			print("st patrol");
            SwitchState(GuardianBehaviorState.PATROL);
        }

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO && m_hasTargetInSight)
        {
			print("st aggro");
            SwitchState(GuardianBehaviorState.AGGRO);
        }
    }

    private void SwitchState(GuardianBehaviorState newState)
    {
        ExitState();
        EnterState(newState);
    }

    private void EnterState(GuardianBehaviorState newState)
	{
		// Don't change state while a coroutine is running
		if (m_isCoroutineRunning) return;

		m_rsoGuardianState.value = newState;

        switch (m_rsoGuardianState.value)
        {
            case GuardianBehaviorState.PATROL:
                EnterPatrolState();
                break;

            case GuardianBehaviorState.AGGRO:
                EnterAggroState();
                break;
        }
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
		if (!m_usePatrolPath) return;

		Patrolling();
		CheckWaypoints();
	}

    private void ExitPatrolState()
    {

	}

	private void Patrolling()
	{
		// Assertion
		if (!IsPatrolPathValid) return;

		if ((transform.position - m_patrolPath.Waypoints[m_currentWaypoint].Position).magnitude <= m_ssoGuardian.WaypointDistanceTolerance)
		{
			m_currentWaypoint++;
			if (m_currentWaypoint >= m_patrolPath.Waypoints.Count) m_currentWaypoint = 0;

			m_updateWaypointTimer = m_ssoGuardian.WaitDurationOnWaypointReached;
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
		StartCoroutine(StartSearchingCharacter());
	}

	private void ChaseTarget()
	{
		// Assertion
		if (!m_hasTargetInSight) return;

		m_agent.destination = m_currentTargetPosition;
	}

	private IEnumerator StartSearchingCharacter()
	{
		m_isCoroutineRunning = true;
		while ((transform.position - m_agent.destination).magnitude > m_ssoGuardian.WaypointDistanceTolerance)
		{
			yield return null;
		}
		yield return new WaitForSeconds(m_ssoGuardian.WaitDurationOnLastTargetPositionReached);
		m_isCoroutineRunning = false;
	}

	private IEnumerator AnimateTorchDestroy(Torch torch)
	{
		m_isCoroutineRunning = true;
		yield return new WaitForSeconds(m_ssoGuardian.TimeToDestroyTorch);
		m_rsoTorchManager.value.Remove(torch);
		m_isCoroutineRunning = false;
	}

	private IEnumerator AnimateCharacterKill(CharacterMotor character)
	{
		m_isCoroutineRunning = true;
		yield return new WaitForSeconds(m_ssoGuardian.TimeToKillCharacter);
		character.HandleDeath();
		m_isCoroutineRunning = false;
	}

    #endregion

    #region DEBUG

    private void UpdateDebugUI()
	{
		m_tmpState.text = m_rsoGuardianState.value.ToString();
		m_tmpTarget.text = m_hasTargetInSight ? m_currentTargetPosition.ToString() : "none";
	}

    #endregion
}

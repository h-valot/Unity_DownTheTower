using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
	#region REFERENCES

	[Title("External references")]
	[SerializeField] private PatrolPath m_patrolPath;

	[FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent m_agent;
	[FoldoutGroup("Internal References")][SerializeField] private MeshRenderer m_guardianEyes;
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
	public List<Vector3> m_candidateTargetPositions = new List<Vector3>();
	private Vector3 m_currentTargetPosition;
	private CandidateType CurrentTargetType => m_currentTargetPosition == m_rsoCharacterPosition.value ? CandidateType.CHARACTER : CandidateType.TORCH;
	private float m_minTargetDistance;
	public bool m_hasTargetInSight;

	// Patrolling
	public bool IsPatrolPathValid => m_patrolPath && m_patrolPath.Waypoints.Count > 0;
	public int m_currentWaypoint;

	#endregion

	#region MONOBEHAVIOR

	private void OnEnable()
	{
		m_rsoGuardianState.value = GuardianBehaviorState.NONE;
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
			character.HandleDeath();
		}

		if (collider.TryGetComponent<Torch>(out var torch))
		{
			m_rsoTorchManager.value.Remove(torch);
		}
	}

	#endregion

	#region STATE MACHINE

	private void SelectTarget()
	{
		// Fill candidates
		m_candidateTargetPositions = new List<Vector3> { m_rsoCharacterPosition.value };
		foreach (var torch in m_rsoTorchManager.value.Torches)
		{
			m_candidateTargetPositions.Add(torch.transform.position);
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
        if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL && !m_hasTargetInSight)
        {
            SwitchState(GuardianBehaviorState.PATROL);
        }

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO && m_hasTargetInSight)
        {
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
		m_guardianEyes.sharedMaterial = m_ssoGuardian.PatrolMaterial;
		m_agent.destination = m_patrolPath.Waypoints[m_currentWaypoint].Position;
	}

    private void UpdatePatrolState()
    {
		Patrolling();
	}

    private void ExitPatrolState()
    {

	}

	private void Patrolling()
	{
		// Assertion
		if (!IsPatrolPathValid) return;

		if ((transform.position - m_patrolPath.Waypoints[m_currentWaypoint].Position).magnitude <= 0.5f)
		{
			m_currentWaypoint++;
			if (m_currentWaypoint >= m_patrolPath.Waypoints.Count) m_currentWaypoint = 0;

			m_agent.destination = m_patrolPath.Waypoints[m_currentWaypoint].Position;
		}
	}

	#endregion

	#region AGGRO

	private void EnterAggroState()
    {
		m_guardianEyes.sharedMaterial = m_ssoGuardian.AggroMaterial;
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

		m_agent.destination = m_currentTargetPosition;
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

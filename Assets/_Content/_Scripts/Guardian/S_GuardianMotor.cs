using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
	#region REFERENCES

	[FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent m_agent;
	[FoldoutGroup("Internal References")][SerializeField] private TextMeshProUGUI m_tmpTarget;
	[FoldoutGroup("Internal References")][SerializeField] private TextMeshProUGUI m_tmpState;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Guardian m_ssoGuardian;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GuardianState m_rsoGuardianState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;

	#endregion

	#region VARIABLES

	public List<Vector3> m_validPositions = new List<Vector3>();
	private Vector3 m_currentTargetPosition;
    private float m_minTargetDistance;
	private bool m_hasTargetInSight;

	// Patrolling
	[SerializeField] private Transform[] m_waypoints; // TODO - Path and waypoints system
    public bool IsPatrolling;
	private int m_currentWaypoint;


	#endregion

	#region MONOBEHAVIOR

	private void OnEnable()
    {
        DetermineState();
	}

    private void Update()
    {
        SelectTarget();

        DetermineState();
        UpdateState();
        UpdateDebugUI();
    }

	#endregion

	#region STATE MACHINE

	private void SelectTarget()
	{
		// Fill candidates
		m_validPositions = new List<Vector3> { m_rsoCharacterPosition.value };
		foreach (var torch in m_rsoTorchManager.value.Torches)
		{
			m_validPositions.Add(torch.transform.position);
		}

		// Select a candidate
		m_minTargetDistance = m_ssoGuardian.MaxRange;
		Vector3 bestPosition = m_currentTargetPosition;
		m_hasTargetInSight = false;
		foreach (var position in m_validPositions)
		{
			float distance = Vector3.Distance(bestPosition, position);

			// Assertions
			if (distance > m_ssoGuardian.MaxRange) continue;
			if (distance > m_minTargetDistance) continue;

			m_hasTargetInSight = true;
			m_minTargetDistance = distance;
			bestPosition = position;
		}

		// Set candidate as the current target
		if (m_currentTargetPosition != bestPosition) 
		{
			m_currentTargetPosition = bestPosition;
		}
    }

    /// <summary>
    /// Determine which behavior state the player should be and trigger a switch of state if neccessary.
    /// </summary>
    private void DetermineState()
    {
        if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL && !m_hasTargetInSight)
        {
            Debug.Log("switching to patrol");
            SwitchState(GuardianBehaviorState.PATROL);
        }

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO && m_hasTargetInSight)
        {
            Debug.Log("switching to aggro");
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
		if (m_waypoints.Length <= 0) return;

		if ((transform.position - m_waypoints[m_currentWaypoint].position).magnitude <= 0.5f)
		{
			m_currentWaypoint++;
			if (m_currentWaypoint >= m_waypoints.Length) m_currentWaypoint = 0;

			m_agent.destination = m_waypoints[m_currentWaypoint].transform.position;
		}
	}

	#endregion

	#region AGGRO

	private void EnterAggroState()
    {
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

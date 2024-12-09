using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
    #region References

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_GuardianState m_rsoGuardianState;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Guardian _guardianRef;

    [FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent _agent;

    [FoldoutGroup("Scriptable")][SerializeField] private TextMeshProUGUI _guardianTarget;
    [FoldoutGroup("Scriptable")][SerializeField] private TextMeshProUGUI _stateText;

    #endregion

    #region Variables

    public List<GameObject> _potentialTargets = new List<GameObject>();
    private float _targetDistance = 99999;
    private GameObject _target;
    private GameObject m_newValidTarget;
    public bool isPatrolling;

    #endregion

    #region Monobehavior

    private void OnEnable()
    {
        DetermineState();
    }

    private void Update()
    {
        SelectTarget();

        DetermineState();
        UpdateState();
        // UpdateDebugUI();
    }

    #endregion

    #region Target

    private void SelectTarget()
    {
        if (_potentialTargets.Count > 0)
        {
            m_newValidTarget = null;
            _targetDistance = 99999;

            foreach (GameObject target in _potentialTargets)
            {
                if (target != null)
                {
                    if (Vector3.Distance(this.transform.position, target.transform.position) <= _targetDistance)
                    {
                        m_newValidTarget = target;
                    }
                }
            }
        }

        if (_target != m_newValidTarget) _target = m_newValidTarget;
    }

    public void AddToPotentialTargets(GameObject objectRef)
    {
        _potentialTargets.Add(objectRef);
    }

    public void RemovePotentialTargets(GameObject objectRef)
    {
        _potentialTargets.Remove(objectRef);
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Determine which behavior state the player should be and trigger a switch of state if neccessary.
    /// </summary>
    private void DetermineState()
    {
        if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL && _target == null)
        {
            Debug.Log("switching to patrol");
            SwitchState(GuardianBehaviorState.PATROL);
        }

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO && _target != null)
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

    #region Patrol State

    private void EnterPatrolState()
    {
        isPatrolling = true;
    }

    private void UpdatePatrolState()
    {

    }

    private void ExitPatrolState()
    {
        isPatrolling = false;
    }

    #endregion

    #region Aggro State

    private void EnterAggroState()
    {
    }

    private void UpdateAggroState()
    {
        _agent.destination = _target.transform.position;
    }

    private void ExitAggroState()
    {

    }

    public void DestroyTorch(Torch torch)
    {
        torch.DestroyTorch();
    }

    #endregion

    #region Debug

    private void UpdateDebugUI()
    {
        if (_guardianRef.debugMode)
        {
            if (_target)
            {
                _guardianTarget.text = _target?.ToString();
                _stateText.text = m_rsoGuardianState.value.ToString();
            }

            else
            {
                // _guardianTarget.text = "null";
                _stateText.text = m_rsoGuardianState.value.ToString();
            }
        }
    }

    #endregion
}

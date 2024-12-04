using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GuardianMotor : MonoBehaviour
{
    #region References

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_GuardianState m_rsoGuardianState;

    #endregion

    #region Variables

    private List<GameObject> _potentialTargets = new List<GameObject>();
    private float _targetDistance = 99999;
    private GameObject _target;

    #endregion

    #region Monobehavior

    private void OnEnable()
    {
        DetermineState();
    }

    private void Update()
    {
        DetermineState();
        SelectTarget();
    }

    #endregion

    #region Target

    private void SelectTarget()
    {
        foreach (GameObject target in _potentialTargets)
        {
            if(Vector3.Distance(this.transform.position, target.transform.position) <= _targetDistance)
            {
                _target = target;
            }
        }
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
        if (m_rsoGuardianState.value != GuardianBehaviorState.PATROL)
        {
            SwitchState(GuardianBehaviorState.PATROL);
        }

        if (m_rsoGuardianState.value != GuardianBehaviorState.AGGRO)
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

    #endregion

    #region Patrol State

    private void EnterPatrolState()
    {
        
    }

    private void ExitPatrolState()
    {

    }

    #endregion

    #region Aggro State

    private void EnterAggroState()
    {

    }

    private void ExitAggroState()
    {

    }

    #endregion
}

using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GuardianMotor : MonoBehaviour
{
    #region References

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_GuardianState m_rsoGuardianState;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Guardian _guardianRef;

    [FoldoutGroup("Internal References")][SerializeField] private NavMeshAgent _agent;
    [FoldoutGroup("Internal References")][SerializeField] private CharacterMotor _playerRef;

    [FoldoutGroup("Scriptable")][SerializeField] private TextMeshProUGUI _guardianTarget;
    [FoldoutGroup("Scriptable")][SerializeField] private TextMeshProUGUI _stateText;

    #endregion

    #region Variables

    public List<GameObject> _potentialTargets = new List<GameObject>();
    private GameObject _target;
    private float _targetDistance;
    private bool _targetAssigned;
    private int _noGuardianLayer = 6;
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
        UpdateDebugUI();
    }

    #endregion

    #region Target

    private void SelectTarget()
    {
        _targetAssigned = false;
        _targetDistance = _guardianRef.maxRange;
        if(Vector3.Distance(this.transform.position, m_rsoCharacterPosition.value) < _targetDistance)
        {
            Physics.Linecast(transform.position, m_rsoCharacterPosition.value, out RaycastHit hitInfo);
            UnityEngine.Debug.DrawLine(transform.position, m_rsoCharacterPosition.value, Color.red, 1f);
            
            if (hitInfo.transform.GetComponent<CharacterMotor>())
            {
                print(hitInfo.ToString());
                _targetDistance = Vector3.Distance(this.transform.position, m_rsoCharacterPosition.value);
                _target = hitInfo.transform.gameObject;
                _targetAssigned = true;
            }
        }

        foreach (Torch target in m_rsoTorchManager.value.m_torches)
        {
            if (Vector3.Distance(this.transform.position, target.gameObject.transform.position) < _targetDistance)
            {
                _targetDistance = Vector3.Distance(this.transform.position, m_rsoCharacterPosition.value);
                _target = target.gameObject;
                _targetAssigned = true;
            }
        }

        if (!_targetAssigned)
        {
            _target = null;
        }









        //if (_potentialTargets.Count > 0)
        //{
        //    m_newValidTarget = null;
        //    _targetDistance = 99999;

        //    foreach (GameObject target in _potentialTargets)
        //    {
        //        if (target != null)
        //        {
        //            if (Vector3.Distance(this.transform.position, target.transform.position) <= _targetDistance)
        //            {
        //                m_newValidTarget = target;
        //            }
        //        }
        //    }
        //}

        //if (_target != m_newValidTarget) _target = m_newValidTarget;
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
                _guardianTarget.text = "null";
                _stateText.text = m_rsoGuardianState.value.ToString();
            }
        }
    }

    #endregion
}

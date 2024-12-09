using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPatrol : MonoBehaviour
{ 
    [Header("Internal reference")]
    [SerializeField] public GuardianMotor _guardianRef;

    // public
    [Header("Patrol Path")]
    [SerializeField] private Transform[] _patrolPoints;

    // private
    [SerializeField] private NavMeshAgent _agent;
    private bool _aggro;
    private int _targetPoint;

    void Start()
    {
        _targetPoint = 0;
        Patrolling();
    }

    // Update is called once per frame
    void Update()
    {
        if ( _guardianRef.isPatrolling == true)
        {
            IncreaseTargetInt();
            GetAggro();  
        }
    }

    void Patrolling()
    {
        _agent.destination = _patrolPoints[_targetPoint].transform.position;
    }
    void IncreaseTargetInt()
    {
        Patrolling();
        if ((transform.position - _patrolPoints[_targetPoint].position).magnitude <= 0.5f)
        {
            _targetPoint++;
            if (_targetPoint >= _patrolPoints.Length)
            {
                _targetPoint = 0;
            }
            

        }

    }

    public void GoingBackToPatrol()
    {
        GetAggro();
        if (_aggro == false)
            {
                Patrolling();
            }
    }

    void GetAggro()
    {
        //_aggro = _guardianRef.StateAggro();
    }



}

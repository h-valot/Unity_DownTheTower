using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPatrol : MonoBehaviour
{ 
    [Header("Internal reference")]
    [SerializeField] public Guardian _guardianRef;

    // public
    [Header("Patrol Path")]
    [SerializeField] private Transform[] _patrolPoints;

    // private
    private NavMeshAgent _agent;
    private bool _aggro;
    private bool dontPatrol;
    private int _targetPoint;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetPoint = 0;
        Patrolling();
    }

    // Update is called once per frame
    void Update()
    {
        if ( dontPatrol == false)
        {
            if (_guardianRef.IsActif() == true)
            {
                IncreaseTargetInt();
                GetAggro();
            }
        }
    }

    void Patrolling()
    {
        if (_aggro == false)
        {
            _agent.destination = _patrolPoints[_targetPoint].transform.position;
        }

    }
    void IncreaseTargetInt()
    {
        if ((transform.position - _patrolPoints[_targetPoint].position).magnitude <= 0.5f)
        {
            _targetPoint++;
            if (_targetPoint >= _patrolPoints.Length)
            {
                _targetPoint = 0;
            }
            if (_aggro == false) 
            {
                Patrolling();
            }
        }
    }

    public void GoingBackToPatrol()
    {
        GetAggro();
        if (_aggro == false)
            {
                Patrolling();
                dontPatrol = false;
            }
    }

    void GetAggro()
    {
        _aggro = _guardianRef.StateAggro();
    }



}

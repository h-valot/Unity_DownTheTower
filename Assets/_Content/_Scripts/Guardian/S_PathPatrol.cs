using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPatrol : MonoBehaviour
{
    public Transform[] _patrolPoints;
    public int _targetPoint;
    public float _speed;
    public bool _aggro;
    private NavMeshAgent _agent;
    [SerializeField] private Guardian _guardianRef;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetPoint = 0;
        _aggro = false;
    }

    // Update is called once per frame
    void Update()
    {
        Patrolling();
        GetAggro();
    }

    void Patrolling()
    {
        if (_aggro == false)
        {
            if ((transform.position - _patrolPoints[_targetPoint].position).magnitude <= 0.1f)
            {
                IncreaseTargetInt();
            }
            _agent.destination = _patrolPoints[_targetPoint].transform.position;
        }

    }
    void IncreaseTargetInt()
    {
        _targetPoint++;
        if(_targetPoint >= _patrolPoints.Length)
        {
            _targetPoint = 0;
        }
    }

    public void GoingBackToPatrol()
    {
        Debug.Log("retour en patrouille");
        _agent.destination = _patrolPoints[_targetPoint].transform.position;
    }

    void GetAggro()
    {
        _aggro = _guardianRef.StateAggro();
    }



}

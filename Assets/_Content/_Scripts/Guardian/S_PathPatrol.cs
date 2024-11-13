using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathPatrol : MonoBehaviour
{
    [SerializeField] public Guardian _guardianRef;

    //private
    public NavMeshAgent _agent;

    //public
    public Transform[] _patrolPoints;
    public int _targetPoint;
    public bool _aggro;
    public bool dontPatrol;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _targetPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if ( dontPatrol == false)
        {
            if (_guardianRef.IsActif() == true)
            {
                Patrolling();
                GetAggro();
            }
        }
        
    }

    void Patrolling()
    {
        if (_aggro == false)
        {
            _guardianRef.ChangeColor(2f);
            if ((transform.position - _patrolPoints[_targetPoint].position).magnitude <= 0.5f)
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
        Debug.Log("Je tente de revenir en patrouille");
        GetAggro();
        
        if (_aggro == false)
            {
                Debug.Log("Patrol mode");
                Patrolling();
                //_agent.destination = _patrolPoints[_targetPoint].transform.position;
                dontPatrol = false;
            }
        
        
    }

    void GetAggro()
    {
        _aggro = _guardianRef.StateAggro();
    }



}

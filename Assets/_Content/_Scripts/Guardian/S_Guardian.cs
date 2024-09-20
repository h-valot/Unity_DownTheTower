using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class S_Guardian : MonoBehaviour
{
    private NavMeshAgent _agent;
    private NewCharacterMotor _playerRef;
    private Coroutine _coroutine;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    IEnumerator CheckForXSecond(float X)
    {
        yield return new WaitForSeconds(X);
        Idle();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<NewCharacterMotor>(out _playerRef))
        {
            Debug.Log("Devant moi");
            if (_coroutine != null )
            {
                StopCoroutine(_coroutine);
            }
            AgroState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _playerRef)
        {
            ToIdle();
        }
    }

    private void AgroState()
    {
        _agent.destination = _playerRef.transform.position;
    }

    private void ToIdle()
    {
        _coroutine = StartCoroutine(CheckForXSecond(3));
    }

    private void Idle ()
    {
        _agent.destination = Vector3.zero;
    }
}

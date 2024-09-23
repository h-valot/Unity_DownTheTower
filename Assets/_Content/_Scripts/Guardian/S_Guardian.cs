using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Guardian : MonoBehaviour
{
    [SerializeField] private PathPatrol _pathPatrol;
    private NavMeshAgent _agent;
    private NewCharacterMotor _playerRef;
    private Coroutine _coroutine;
    private bool _aggro;

    //raycast
    [SerializeField] private GameObject _raycastHead;
    [SerializeField] private GameObject _raycastEyes;
    [SerializeField] private GameObject _raycastFeet;
    private RaycastHit hitDataHead;
    private RaycastHit hitDataEyes;
    private RaycastHit hitDataFeet;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _aggro = false;
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
            if (_coroutine != null )
            {
                StopCoroutine(_coroutine);
            }
            AgroState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NewCharacterMotor>(out _playerRef))
        {
            Debug.Log("plus devant");
            ToIdle();
        }
    }

    private void AgroState()
    {
        _agent.destination = _playerRef.transform.position;
        _aggro = true;
    }

    private void ToIdle()
    {
        Debug.Log("To idle");
        _coroutine = StartCoroutine(CheckForXSecond(3));
    }

    private void Idle ()

    {
        Debug.Log("Idle");
        _aggro = false;
        _pathPatrol.GoingBackToPatrol();
    }

    public bool StateAggro()
    {
        return _aggro;
    }

    bool CheckRaycast()
    {
        FireRay();

        if (Physics.Raycast(ray)

        return false;
    }

    private void FireRay()
    {
        // Head raycast
        Ray rayHead = new Ray(_raycastHead.transform.position, transform.forward);
        RaycastHit hitDataHead;
        Physics.Raycast(rayHead, out hitDataHead);

        // Eyes raycast
        Ray rayEyes = new Ray(_raycastEyes.transform.position, transform.forward);
        RaycastHit hitDataEyes;
        Physics.Raycast(rayHead, out hitDataEyes);

        // Feet raycast
        Ray rayFeet = new Ray(_raycastFeet.transform.position, transform.forward);
        RaycastHit hitDataFeet;
        Physics.Raycast(rayFeet, out hitDataFeet);
    }
}

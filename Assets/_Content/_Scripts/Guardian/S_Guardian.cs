using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class Guardian : MonoBehaviour
{
    [SerializeField] private PathPatrol _pathPatrol;
    private NavMeshAgent _agent;
    private NewCharacterMotor _playerRef;
    private Coroutine _coroutine;
    private bool _aggro;

    //Collision
    [SerializeField] private GameObject _SmallCollider;
    [SerializeField] private GameObject _BigCollider;

    //raycast
    [SerializeField] private GameObject _raycastHead;
    [SerializeField] private GameObject _raycastEyes;
    [SerializeField] private GameObject _raycastFeet;

    //Height
    float headHeight;
    float eyesHeight;
    float feetHeight;


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
            if (CheckRaycast() == true)
            {
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                }
                AgroState();
            }
            
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
        //_agent.destination = _playerRef.transform.position;
        //_aggro = true;
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
        //RaycastHit hitDataHead = 0;
        CheckPlayerHeight();
        Physics.Raycast(_raycastHead.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastHead.transform.position).normalized, out var hitDataHead);
        UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

        Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataEyes);

        Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataFeet);

        if (hitDataHead.transform == _playerRef.transform && hitDataEyes.transform == _playerRef.transform && hitDataFeet.transform == _playerRef.transform)
        {
            Debug.Log("pas de mur entre");
            return true;
        }
        else
        {

            Debug.Log(" mur entre");
            return false;
        }
    }
    private void CheckPlayerHeight()
    {
        CharacterController CharacterControllerRef = _playerRef.GetComponent<CharacterController>();
        headHeight = CharacterControllerRef.height*0.8f;
        eyesHeight = CharacterControllerRef.height*0.5f;
        feetHeight = CharacterControllerRef.height*0.2f;
        
    }

}

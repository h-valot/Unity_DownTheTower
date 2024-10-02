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
    [SerializeField] public bool _isActif;
    private NavMeshAgent _agent;
    private CharacterMotor _playerRef;
    private Coroutine _coroutine;
    private Coroutine _coroutineUpdate;
    private bool _aggro;

    public Material _aggroMaterial;
    public Material _scanMaterial;
    public Material _dormantMaterial;
    public GameObject _colliderDeath;

    public GameObject _scanCube;

    //raycast
    [SerializeField] private GameObject _raycastEyes;

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

    IEnumerator UpdatePlayerPosition()
    {
        int i = 0;

        while (i < 10)
        {
            SetDestination();
            i++;
            yield return null;
        }

        while (i > 0)
        {
            SetDestination();
            i--;
            yield return null;
        }

        // All done!
    }
    public void PlayerStayIn()
    {
        if (IsActif() == true)
        {
            if (CheckRaycast() == true)
            {
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                    StopCoroutine(_coroutineUpdate);
                }
                AggroState();
            }
        }
    }


    public void PlayerExit()
    {
        if (IsActif() == true)
        {
            Debug.Log("plus devant");
            ToIdle();
        }
    }

    public void ChangeColor(float X)
    {
        MeshRenderer my_renderer = _scanCube.GetComponent<MeshRenderer>();
        if (my_renderer != null)
        {
            Material my_material = my_renderer.material;

            if (X == 1)
            {
                my_renderer.material = _aggroMaterial;
            }

            if (X == 2)
            {
                my_renderer.material = _scanMaterial;
            }

            if (X == 3)
            {
                my_renderer.material = _dormantMaterial;
            }
        }

        
    }

    private void AggroState()
    {
        SetDestination();
        _aggro = true;
        ChangeColor(1f);
    }
    private void SetDestination()
    {
        _agent.destination = _playerRef.transform.position;
    }

    public bool IsActif()
    {
        if (_isActif == true)
        {
            return true;
        }
        else
        {
            return false; 
        }  
    }

    private void ToIdle()
    {
        Debug.Log("To idle");
        _coroutine = StartCoroutine(CheckForXSecond(3f));
        _coroutineUpdate = StartCoroutine(UpdatePlayerPosition());
    }

    private void Idle ()

    {
        Debug.Log("Idle");
        _aggro = false;
        _pathPatrol.GoingBackToPatrol();
        StopCoroutine(_coroutineUpdate);
        ChangeColor(2f);
    }

    public bool StateAggro()
    {
        return _aggro;
    }

    bool CheckRaycast()
    {
        CheckPlayerHeight();
        Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataHead);
        UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);
        Debug.Log(hitDataHead.transform.name);

        Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataEyes);
        UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

        Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataFeet);
        UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

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

    public void MakePLayerRef(CharacterMotor Player)
    {
        _playerRef = Player;
    }

}

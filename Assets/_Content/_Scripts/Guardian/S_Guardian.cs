using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Guardian : MonoBehaviour
{
    [SerializeField] private PathPatrol _pathPatrol;
    [SerializeField] public bool _isActif;

    [Header("Scriptable references")]
    [SerializeField] private GuardianConfig _guardianConfig;

    private NavMeshAgent _agent;
    private CharacterMotor _playerRef;
    private Torch _torchRef;
    private bool _isPlayerTarget;
    private Coroutine _coroutine;
    private Coroutine _coroutineUpdate;
    private bool _aggro;
    private GameObject _actualTarget;
    private float _playerDistance;
    private float _torchDistance;
    private bool _isPlayerSeen = false;
    [SerializeField] private float _timeToDestroy;
    [SerializeField] private float _killTime;

    public float timeToDesaggro;
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

    // --- WIP ---
    
    // Private ---

    Dictionary<GameObject, ClassGuardianTarget> _potentialTarget = new Dictionary<GameObject, ClassGuardianTarget> ();

    #region BaseScript
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
    }

    public IEnumerator KillPlayer()
    {
        StopCoroutine(UpdatePlayerPosition());
        yield return new WaitForSeconds(_killTime);
        _playerRef.HandleDeath();
        ResetTarget();
        DestroyedTarget();
    }

    public IEnumerator DestroyTorchTime()
    {
        yield return new WaitForSecondsRealtime(_timeToDestroy);
        DestroyedTarget();
    }
    public void TargetStayIn()
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
                    SelectTargetSequence();
                    AggroState();
                }
        }
    }

    public void TargetExit()
    {
        if (IsActif() == true)
        {
            if ( _aggro == true && _actualTarget == _playerRef)
            {
                if (_isPlayerSeen == true) 
                {
                    SetDestination();
                }

                if (_torchRef == null)
                {
                    ToIdle();
                }
            }
           
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
        _agent.speed = 7f;
    }
    private void SetDestination()
    {
        _agent.destination = _actualTarget.transform.position;
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
        {
            Debug.Log("To idle");
            _coroutine = StartCoroutine(CheckForXSecond(1f));
            _coroutineUpdate = StartCoroutine(UpdatePlayerPosition());
        }
    }

    private void Idle ()

    {
        if (_coroutineUpdate != null)
        {
            StopCoroutine(_coroutineUpdate);
        }
        _agent.speed = 5f;
        Debug.Log("Idle");
        _aggro = false;
        _pathPatrol.GoingBackToPatrol();
        ChangeColor(2f);
        ResetTarget();
    }

    public bool StateAggro()
    {
        return _aggro;
    }

    bool CheckRaycast()
    {
        if (_isPlayerTarget && _playerRef != null)
        {
            CheckPlayerHeight();
            Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataHead);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);
            Debug.Log(hitDataHead.transform.name);

            Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataEyes);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

            Physics.Raycast(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataFeet);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_playerRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

            if (hitDataHead.transform == _playerRef.transform || hitDataEyes.transform == _playerRef.transform || hitDataFeet.transform == _playerRef.transform)
            {
                _isPlayerSeen = true;
                return true;
            }
            else
            {
                _isPlayerSeen = false;
                return false;
            }
        }
        else if (_torchRef != null)
        {
            Physics.Linecast(_raycastEyes.transform.position, _torchRef.transform.position, out var hitDatatorch);
            // UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, (_torchRef.transform.position - _raycastEyes.transform.position).normalized, Color.red);

            if (hitDatatorch.collider.TryGetComponent<Torch>(out var torch)
                || hitDatatorch.collider.TryGetComponent<TorchPointLight>(out var torchPointLight))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else { return false; }
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
        _isPlayerTarget = true;
    }

    public void MakeTorchRef(Torch torch)
    {
        _torchRef = torch;
        _isPlayerTarget = false;
    }

    public void CheckDistancePlayer()
    {
        if (_playerRef != null)
        {
            _playerDistance = Vector3.Distance(this.transform.position, _playerRef.transform.position);
        }
    }

    public void CheckDistanceTorch()
    {
        if (_torchRef != null)
        {
            _torchDistance = Vector3.Distance(this.transform.position, _torchRef.transform.position);
        }
    }

    public void SelectTarget()
    {
        if (_torchRef == null && _playerRef == null)
        {
            return;
        }
        if (_playerRef != null && _torchRef == null)
        {
            _actualTarget = _playerRef.gameObject;
        }
        if (_torchRef != null && _playerRef == null)
        {
            _actualTarget = _torchRef.gameObject;
        }
        if (_playerRef != null && _torchRef != null)
        {
            if (_playerDistance < _torchDistance)
            {
                _actualTarget = _playerRef.gameObject;
            }
            if (_torchDistance < _playerDistance)
            {
                _actualTarget = _torchRef.gameObject;
            }
        }
    }

    public void SelectTargetSequence()
    {
        CheckDistancePlayer();
        CheckDistanceTorch();
        SelectTarget();
    }

    public void ResetTarget()
    {
        _actualTarget = null;
        _torchRef = null;
        _playerRef = null;
        _isPlayerSeen = false;
    }

    public void DestroyedTarget()
    {
        if (_isPlayerSeen == true)
        {
            if (_coroutineUpdate != null)
            {
                StopCoroutine(_coroutineUpdate);
            }
            Debug.Log("je devrais poursuivre le joueur");
            TargetStayIn();
        }

        else
        { 
            Idle();
        }
    }
    #endregion

    #region Dictionary Manager
    public void AddToPotentialTargets(GameObject _targetRef)
    {
        if (_potentialTarget.ContainsKey(_targetRef) == false) 
        {
            _potentialTarget.Add(_targetRef, new ClassGuardianTarget());
        }
        IncreaseActiveColliders(_targetRef);
        CheckForTargets();
    }

    public void RemovePotentialTargets(GameObject _targetRef)
    {
        if(_potentialTarget.Count >0)
        {
            DecreaseActiveColliders(_targetRef);
            _potentialTarget.TryGetValue(_targetRef, out var data);
            if (data.activeColliders < 1)
            {
                _potentialTarget.Remove(_targetRef);
            }
            CheckForTargets();
        }
    }

    private void IncreaseActiveColliders(GameObject _objectRef)
    {
        _potentialTarget.TryGetValue(_objectRef, out var data);
        data.activeColliders++;
        Debug.Log(_objectRef + "," + data.activeColliders.ToString());
    }

    private void DecreaseActiveColliders(GameObject _objectRef)
    {
        _potentialTarget.TryGetValue(_objectRef, out var data);
        data.activeColliders--;
        Debug.Log(_objectRef + "," + data.activeColliders.ToString());
    }

    #endregion

    private void CheckForTargets()
    {
        List<GameObject> _potentialTargetsRef = new List<GameObject>();
        Dictionary<GameObject, float> _distance = new Dictionary<GameObject, float>();

        foreach (KeyValuePair<GameObject, ClassGuardianTarget> pair in _potentialTarget)
        {
           if (pair.Value.activeColliders > 1)
            {
                _potentialTargetsRef.AddUnique(pair.Key.gameObject);
                Debug.Log(_potentialTargetsRef.ToString());
                foreach (GameObject _objectRef in _potentialTargetsRef)
                {
                    _distance.Add(_objectRef, Vector3.Distance(this.transform.position, _objectRef.transform.position));
                }
                var keyAndValue = _distance.OrderBy(kvp => kvp.Value).First();
                _actualTarget = keyAndValue.Key;
                Debug.Log("Target is" + _actualTarget.ToString());
            }

           else
            {
                ResetTarget();
            }
        }
        

    }

    private void ResetTargets()
    {
        _actualTarget = null;
    }


    #region Behavior

    #endregion
    public class ClassGuardianTarget
    {
        public int activeColliders;
        public bool isSeen;
    }
}

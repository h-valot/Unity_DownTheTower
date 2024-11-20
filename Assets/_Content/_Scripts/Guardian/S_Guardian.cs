using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Guardian : MonoBehaviour
{
    #region Declarations

    [SerializeField] private PathPatrol _pathPatrol;
    [SerializeField] public bool _isActif;

    [Header("Scriptable references")]
    [SerializeField] private GuardianConfig _guardianConfig;

    [Header("External references")]
    [SerializeField] private TextMeshProUGUI _GuardianTarget;
    [SerializeField] private TextMeshProUGUI _dictionnayCountText;

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
    private float _aggroColor = 1;
    private float _scanColor = 2;
    private float _dormantColor = 3;


    public GameObject _colliderDeath;

    public GameObject _scanCube;

    //raycast
    [SerializeField] private GameObject _raycastEyes;

    //Height
    float headHeight;
    float eyesHeight;
    float feetHeight;

    // --- WIP ---

    // --- A mettre en config ---

    public float ResetAggroCD;
    
    // Private ---

    Dictionary<GameObject, ClassGuardianTarget> _potentialTarget = new Dictionary<GameObject, ClassGuardianTarget> ();
    private Coroutine _aggroCoroutine = null;
    public Coroutine destroyTorchCoroutine;

    public class ClassGuardianTarget
    {
        public int activeColliders;
        public bool isSeen;
    }

    #endregion

    #region Monobehavior Functions

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _aggro = false;
    }

    private void Update()
    {
        _dictionnayCountText.text = "Dictionnaire count = " + _potentialTarget.Count.ToString();
        if (destroyTorchCoroutine == null)
        {
            CheckForTargets();
        }
        if (_actualTarget != null)
        {
            SetDestination();
        }
    }

    #endregion

    #region State

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

    public bool StateAggro()
    {
        return _aggro;
    }

    #endregion

    #region Guardian action on objects

    //public IEnumerator KillPlayer()
    //{
    //    StopCoroutine(UpdatePlayerPosition());
    //    yield return new WaitForSeconds(_killTime);
    //    //_playerRef.HandleDeath();
    //    ResetTarget();
    //    DestroyedTarget();
    //}

    public IEnumerator DestroyTorchTime(GameObject _torchRef)
    {
        yield return new WaitForSecondsRealtime(_timeToDestroy);
        DestroyedTarget(_torchRef);
        destroyTorchCoroutine = null;
    }

    public void DestroyedTarget(GameObject _torchRef)
    {
        GameObject.Destroy(_torchRef);
    }
    #endregion

    #region LoSCheck

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

    #endregion

    #region Dictionary Manager
    public void AddToPotentialTargets(GameObject _targetRef)
    {
        if (_potentialTarget.ContainsKey(_targetRef) == false) 
        {
            _potentialTarget.Add(_targetRef, new ClassGuardianTarget());
        }
        IncreaseActiveColliders(_targetRef);
        //CheckForTargets();
    }

    public void RemovePotentialTargets(GameObject _targetRef)
    {
        if(_potentialTarget.Count >0)
        {
            if (_targetRef != null)
            {
                DecreaseActiveColliders(_targetRef);
                _potentialTarget.TryGetValue(_targetRef, out var data);
                if (data.activeColliders < 1)
                {
                    _potentialTarget.Remove(_targetRef);
                }
                //CheckForTargets();
            }
        }
    }

    private void IncreaseActiveColliders(GameObject _objectRef)
    {
        if (_objectRef != null)
        {
            _potentialTarget.TryGetValue(_objectRef, out var data);
            data.activeColliders++;
        }
    }

    private void DecreaseActiveColliders(GameObject _objectRef)
    {
        if (_objectRef != null)
        {
            _potentialTarget.TryGetValue(_objectRef, out var data);
            data.activeColliders--;
        }
    }

    #endregion

    #region Target Manager

    private void CheckForTargets()
    {
        List<GameObject> _potentialTargetsRef = new List<GameObject>();
        Dictionary<GameObject, float> _distance = new Dictionary<GameObject, float>();

        if (_potentialTarget.Count > 0)
        {
            foreach (KeyValuePair<GameObject, ClassGuardianTarget> pair in _potentialTarget)
            {
                if (pair.Value.activeColliders > 0)
                {
                    if (!_potentialTargetsRef.Contains(pair.Key.gameObject))
                    {
                        _potentialTargetsRef.AddUnique(pair.Key.gameObject);
                    }
                    foreach (GameObject _objectRef in _potentialTargetsRef)
                    {
                        if (!_distance.ContainsKey(_objectRef))
                        {
                            if (_objectRef != null)
                            {
                                _distance.Add(_objectRef, Vector3.Distance(this.transform.position, _objectRef.transform.position));
                            }
                            else
                            {
                                _potentialTarget.Remove(pair.Key.gameObject);
                            }
                        }
                        if (_distance.Count > 0)
                        {
                            var keyAndValue = _distance.OrderBy(kvp => kvp.Value).First();
                            UpdateTarget(keyAndValue.Key.gameObject);
                        }
                    }

                }

                else
                {
                    if (_aggroCoroutine == null)
                    {
                        StopCoroutine(WaitForAggroReset());
                        _aggroCoroutine = StartCoroutine(WaitForAggroReset());
                    }
                }
            }
        }

        else
        {
            if (_aggroCoroutine == null)
            {
                StopCoroutine(WaitForAggroReset());
                _aggroCoroutine = StartCoroutine(WaitForAggroReset());
            }
        }
    }

    private IEnumerator WaitForAggroReset()
    {
        yield return new WaitForSeconds(ResetAggroCD);
        if (_potentialTarget.Count < 1)
        {
            UpdateTarget(null);
        }
        _aggroCoroutine = null;
    }

    private void UpdateTarget(GameObject _objectRef)
    {


        if (_objectRef != null)
        {
            _actualTarget = _objectRef;
            _GuardianTarget.text = _actualTarget.ToString();
            ChangeColor(_aggroColor);
        }
        else if (_objectRef == null)
        {
            _actualTarget = _objectRef;
            ChangeColor(_scanColor);
            _GuardianTarget.text = "None";
        }
    }

    private void SetDestination()
    {
        _agent.destination = _actualTarget.transform.position;
    }

    #endregion

    #region Graphics Manager

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

    #endregion


    
}

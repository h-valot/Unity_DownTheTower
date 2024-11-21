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

    [SerializeField] private PathPatrol _pathPatrol; // ref à reconstruire
    [SerializeField] public bool _isActif;

    [Header("Scriptable references")]
    [SerializeField] private GuardianConfig _guardianConfig;

    [Header("Debug References")]
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

    public float resetAggroCD;
    public float shiftToPatrolCD;
    public float shiftToPursuitCD;
    
    // Private ---

    Dictionary<GameObject, ClassGuardianTarget> _potentialTarget = new Dictionary<GameObject, ClassGuardianTarget> ();
    private Coroutine _aggroCoroutine = null;
    private Coroutine _shiftCoroutine;
    private bool _shiftIsFinished = true;
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
            if (_aggroCoroutine == null)
            {
                CheckForTargets();
            }
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

    bool CheckRaycast(GameObject _objectRef)
    {
        if (_objectRef != null)
        {
            CheckPlayerHeight(_objectRef);
            Physics.Raycast(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataHead);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, headHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);
            Debug.Log(hitDataHead.transform.name);

            Physics.Raycast(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataEyes);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, eyesHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

            Physics.Raycast(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, out var hitDataFeet);
            UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, ((_objectRef.transform.position + new Vector3(0, feetHeight, 0)) - _raycastEyes.transform.position).normalized, Color.red);

            Physics.Linecast(_raycastEyes.transform.position, _objectRef.transform.position, out var hitDatatorch);

            if (hitDataHead.transform == _objectRef.transform || hitDataEyes.transform == _objectRef.transform || hitDataFeet.transform == _objectRef.transform ||  hitDatatorch.collider.TryGetComponent<Torch>(out var torch)
                || hitDatatorch.collider.TryGetComponent<TorchPointLight>(out var torchPointLight))
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
        //else if (_objectRef != null)
        //{
        //    Physics.Linecast(_raycastEyes.transform.position, _objectRef.transform.position, out var hitDatatorch);
        //    // UnityEngine.Debug.DrawRay(_raycastEyes.transform.position, (_torchRef.transform.position - _raycastEyes.transform.position).normalized, Color.red);

        //    if (hitDatatorch.collider.TryGetComponent<Torch>(out var torch)
        //        || hitDatatorch.collider.TryGetComponent<TorchPointLight>(out var torchPointLight))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        // }
        else { return false; }
    }
    private void CheckPlayerHeight(GameObject _objectRef)
    {
        if (_objectRef.TryGetComponent<CharacterController>(out var _characterControllerRef))
        {
            headHeight = _characterControllerRef.height * 0.8f;
            eyesHeight = _characterControllerRef.height * 0.5f;
            feetHeight = _characterControllerRef.height * 0.2f;
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

    #region Behavior Manager

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
                                if (CheckRaycast(_objectRef))
                                {
                                    _distance.Add(_objectRef, Vector3.Distance(this.transform.position, _objectRef.transform.position));
                                }                              
                            }
                            else
                            {
                                _potentialTarget.Remove(pair.Key.gameObject);
                            }
                        }
                        if (_distance.Count > 0)
                        {
                            var keyAndValue = _distance.OrderBy(kvp => kvp.Value).First();
                            if(_shiftCoroutine == null)
                            {
                                _shiftIsFinished = false;
                                _shiftCoroutine = StartCoroutine(ShifToPursuit(keyAndValue.Key.gameObject));
                            }
                            else if(_shiftIsFinished == true)
                            {
                                UpdateTarget(keyAndValue.Key.gameObject);
                            }
                            
                        }
                    }

                }

                else
                {
                    if (_aggroCoroutine != null)
                    {
                        StopCoroutine(WaitForAggroReset());
                        _aggroCoroutine = StartCoroutine(WaitForAggroReset());
                    }
                    _aggroCoroutine = StartCoroutine(WaitForAggroReset());
                    Debug.Log("sortie 2");
                }
            }
        }

        else if (_aggro == true)
        {
            if (_aggroCoroutine != null)
            {
                StopCoroutine(WaitForAggroReset());
                _aggroCoroutine = StartCoroutine(WaitForAggroReset());
            }
            _aggroCoroutine = StartCoroutine(WaitForAggroReset());
            Debug.Log("sortie 1");
        }
    }

    private void UpdateTarget(GameObject _objectRef)
    {

        if (_objectRef != null)
        {
            _actualTarget = _objectRef;
            _GuardianTarget.text = _actualTarget.ToString();
        }
        else if (_objectRef == null)
        {
            _actualTarget = _objectRef;
            ChangeColor(_scanColor);
            _GuardianTarget.text = "None";
            StartCoroutine(ShiftToPatrol());
        }
    }

    private void SetDestination()
    {
        _agent.destination = _actualTarget.transform.position;
    }

    private void GetBackToPatrol()
    {
        _aggro = false;
        _shiftCoroutine = null;
        _pathPatrol.GoingBackToPatrol();
    }

    private IEnumerator WaitForAggroReset()
    {
        yield return new WaitForSeconds(resetAggroCD);
        if (_potentialTarget.Count < 1)
        {
            UpdateTarget(null);
        }
        _aggroCoroutine = null;
        Debug.Log("Coroutine aggro reset");
    }

    private IEnumerator ShiftToPatrol()
    {
        yield return new WaitForSeconds(shiftToPatrolCD);
        GetBackToPatrol();
    }

    private IEnumerator ShifToPursuit(GameObject _target)
    {
        _actualTarget = this.gameObject;
        _aggro = true;
        ChangeColor(_aggroColor);
        yield return new WaitForSeconds(shiftToPursuitCD);
        UpdateTarget(_target);
        _shiftIsFinished = true;
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

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MushroomBatch : MonoBehaviour
{
    #region editor variables

    [Header("Area Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private float _radius = 3;
    [FoldoutGroup("Spawning")][SerializeField] private float _density = 5;

    [Header("Mushroom Placement Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private float _overlapModifier = 0.5f;
    [FoldoutGroup("Spawning")][SerializeField] private float _minSizeMultiplier = 0.5f;
    [FoldoutGroup("Spawning")][SerializeField] private float _maxSizeMultiplier = 1.5f;

    // DrawMeshInstanced can only draw up to 1023 meshes at a time, so we need a new list for every 1023 mushrooms
    public List<MatrixList> mushroomLists = new List<MatrixList>();

    [FoldoutGroup("External References")][SerializeField] private GameObject _mushroomTriggerPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject _mushroomPrefab;
    [FoldoutGroup("External References")][SerializeField] private Mesh _mushroomMesh;
    [FoldoutGroup("External References")][SerializeField] private Material _masterMaterial;


    [FoldoutGroup("Behavior")][SerializeField] private float _deflateTime = 0.5f;
    [FoldoutGroup("Behavior")][SerializeField] private float _inactiveTime = 10f;
    [FoldoutGroup("Behavior")][SerializeField] private float _inflateTime = 1f;
    [FoldoutGroup("Behavior")][SerializeField] private float _propagationSpeed = 6f;


    // --- INSTANCIATED VARIABLES ---
    [HideInInspector] public GameObject _mushroomTrigger;
    [HideInInspector] public Material _mushroomMaterial;

    // --- PRIVATE VARIABLES ---
    // not used during runtime
    private float _furthestShroom = -1f;

    // used during runtime
    private MushroomState _currentState = MushroomState.REST;

    #endregion

    #region spawning logic

    [Title("Functions")]
    [InfoBox("Draw spawns mushrooms in the radius defined in the spawning properties. Clear removes all which have spawned.", InfoMessageType = InfoMessageType.None)]
    [Button]
    public void Draw()
    {
        ClearAll();
        

        if (_mushroomMaterial == null) _mushroomMaterial = Instantiate(_masterMaterial);

        float phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
        int samples = GetRaycastSamples();

        LayerMask raycastLayerMask = new LayerMask();
        raycastLayerMask |= (1 << LayerMask.NameToLayer("NoCollision_NoRaycast"));

        for (int i = 0; i < samples; i++)
        {
            float y = 1f - ((float)i / ((float)samples - 1f)) * 2f;
            float yRadius = Mathf.Sqrt(1 - y * y);

            float theta = phi * i;

            float x = Mathf.Cos(theta) * yRadius;
            float z = Mathf.Sin(theta) * yRadius;

            Vector3 localDirection = new Vector3(x, y, z);
            if (Physics.Raycast(transform.position, localDirection, out RaycastHit hitInfo, _radius, ~raycastLayerMask)) SpawnMushroom(hitInfo);
        }

        if (mushroomLists.Count > 0)
        {
            // 1. Instantiate Death Sphere (collision)
            _mushroomTrigger = Instantiate(_mushroomTriggerPrefab, transform.position, Quaternion.identity, transform);
            _mushroomTrigger.GetComponent<SphereCollider>().radius = _furthestShroom;
            _mushroomTrigger.transform.SetSiblingIndex(0);
        }
    }

    [Button]
    public void ShowGameObjects()
    {
        ClearGameObjects();
        foreach (MatrixList list in mushroomLists)
        {
            foreach (Matrix4x4 mushroom in list.matrices)
            {
                GameObject newMushroom = Instantiate(_mushroomPrefab, mushroom.GetPosition(), mushroom.rotation, transform);
                newMushroom.transform.localScale = mushroom.lossyScale;
                newMushroom.name = "List" + mushroomLists.IndexOf(list) + "Mushroom" + list.matrices.IndexOf(mushroom);
                newMushroom.GetComponent<MeshRenderer>().material = _mushroomMaterial;
            }
        }
    }

    [Button]
    public void ClearGameObjects()
    {
        Debug.Log("List size: " + mushroomLists[0].matrices.Count);
        LayerMask raycastLayerMask = new LayerMask();
        raycastLayerMask |= (1 << LayerMask.NameToLayer("NoCollision_NoRaycast"));
        foreach (Collider collider in Physics.OverlapSphere(transform.position, _radius, raycastLayerMask)) 
            if(collider.gameObject.TryGetComponent<Mushroom>(out Mushroom mushroom)) DestroyImmediate(mushroom.gameObject);
    }

    [Button]
    public void ClearAll()
    {
        if (mushroomLists.Count == 0) return;
        ClearGameObjects();

        while (mushroomLists.Count > 0)
        {
            mushroomLists.RemoveAt(0);
        }
        if (_mushroomTrigger != null)
        {
            DestroyImmediate(_mushroomTrigger);
            _mushroomTrigger = null;
        }
    }

    private int GetRaycastSamples()
    {
        float area = 4 * Mathf.PI * Mathf.Pow(_radius, 2);
        return Mathf.RoundToInt(area * _density);
    }

    private void SpawnMushroom(RaycastHit hitInfo)
    {
        if (SimplexNoise3D.SimplexNoise(hitInfo.point, 0.37f) < 0.5f) return;
        float scale = _mushroomPrefab.transform.localScale.x * Random.Range(_minSizeMultiplier, _maxSizeMultiplier);

        if (!IsNormalFacingOrigin(hitInfo) || !HasEnoughRoom(hitInfo, scale * 0.5f * _overlapModifier)) return;

        GameObject newMushroom = Instantiate(_mushroomPrefab, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), transform);
        newMushroom.transform.localScale = new Vector3(scale, scale, scale);
        newMushroom.GetComponent<MeshRenderer>().material = _mushroomMaterial;
        AddMatrixToList(newMushroom.transform.localToWorldMatrix);
        newMushroom.name = "List" + mushroomLists.Count + "Mushroom" + mushroomLists[mushroomLists.Count - 1].matrices.Count;
        CheckFurthest(newMushroom);
    }

    private bool HasEnoughRoom(RaycastHit hitInfo, float radius)
    {
        // Doesn't overlap with other mushrooms
        LayerMask raycastLayerMask = new LayerMask();
        raycastLayerMask |= (1 << LayerMask.NameToLayer("NoCollision_NoRaycast"));
        Collider[] hitlist = Physics.OverlapSphere(hitInfo.point, radius, raycastLayerMask, QueryTriggerInteraction.Collide);
        return hitlist.Length < 1;
    }

    private bool IsNormalFacingOrigin(RaycastHit hitInfo)
    {
        Vector3 rayDirection = (transform.position - hitInfo.point).normalized;
        return 0.2f <= Vector3.Dot(rayDirection.normalized, hitInfo.normal.normalized);
    }

    private void CheckFurthest(GameObject mushroom)
    {
        if (_furthestShroom == -1f) _furthestShroom = Vector3.Distance(transform.position, mushroom.transform.position);
        else if (Vector3.Distance(transform.position, mushroom.transform.position) > _furthestShroom) 
            _furthestShroom = Vector3.Distance(transform.position, mushroom.transform.position);
    }

    public void AddMatrixToList(Matrix4x4 matrix)
    {
        if (mushroomLists.Count == 0) mushroomLists.Add(new MatrixList());
        if (mushroomLists[mushroomLists.Count - 1].matrices.Count == 1023) mushroomLists.Add(new MatrixList());
        mushroomLists[mushroomLists.Count - 1].matrices.Add(matrix);
    }

    public bool RemoveMushroomFromList(Vector3 position)
    {
        foreach (MatrixList list in mushroomLists)
        {
            foreach (Matrix4x4 mushroom in list.matrices)
            {
                if (position.Equals(mushroom.GetPosition()))
                {
                    list.matrices.Remove(mushroom);
                    return true;
                }
            }
        }

        return false;
    }

    #endregion spawning

    #region monobehavior functions

    private void Start()
    {
        ClearGameObjects();
        if (mushroomLists[0].matrices.Count == 0) return;
        _mushroomMaterial.SetFloat("_propagationSpeed", _propagationSpeed);
        UpdateState(MushroomState.REST);
    }

    private void Update()
    {
        foreach (MatrixList list in mushroomLists)
            Graphics.DrawMeshInstanced(_mushroomMesh, 0, _mushroomMaterial, list.matrices);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    #endregion

    #region effect

    public void InitiateExplosion(Vector3 source)
    {
        if(_currentState != MushroomState.REST) return;

        _mushroomMaterial.SetVector("_explosionSource", source);
        IEnumerator coroutine = ExecuteEffect(source);
        StartCoroutine(coroutine);
    }

    IEnumerator ExecuteEffect(Vector3 source)
    {
        float expTime = 0;

        UpdateState(MushroomState.DEFLATE);
        while (expTime <= _radius * 2f / _propagationSpeed + _deflateTime)
        {
            _mushroomMaterial.SetFloat("_timeSinceExplosion", expTime);
            expTime += Time.deltaTime;
            yield return null;
        }

        float inactiveTime = 0;
        while (inactiveTime <= _inactiveTime)
        {
            inactiveTime += Time.deltaTime;
            yield return null;
        }

        expTime = _radius * 2f / _propagationSpeed + _inflateTime;
        _mushroomMaterial.SetVector("_explosionSource", source + (transform.position - source) * 2f);
        UpdateState(MushroomState.INFLATE);

        while (expTime >= 0f)
        {
            _mushroomMaterial.SetFloat("_timeSinceExplosion", expTime);
            expTime -= Time.deltaTime;
            yield return null;
        }

        UpdateState(MushroomState.REST);
    }

    private void UpdateState(MushroomState newState)
    {
        switch (newState) 
        {
            case MushroomState.REST:
                _mushroomMaterial.SetFloat("_animTime", _deflateTime);
                break;
            case MushroomState.DEFLATE:
                break;
            case MushroomState.INACTIVE:
                break;
            case MushroomState.INFLATE:
                _mushroomMaterial.SetFloat("_animTime", _inflateTime);
                break;
        }

        _currentState = newState;
    }



    #endregion
}
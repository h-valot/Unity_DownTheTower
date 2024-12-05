using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<GameObject> mushroomList = new List<GameObject>();

    [FoldoutGroup("External References")][SerializeField] private GameObject _mushroomTriggerPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject _mushroomPrefab;
    [FoldoutGroup("External References")][SerializeField] private Material _masterMaterial;


    [FoldoutGroup("Effect")][SerializeField] private float _releaseTime;
    [FoldoutGroup("Effect")][SerializeField] private float _chargeTime;

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

    [Title("ziruguilrgh")]
    [InfoBox("Draw spawns mushrooms in the radius defined in the spawning properties. Clear removes all which have spawned.", InfoMessageType = InfoMessageType.None)]
    [Button]
    public void Draw()
    {
        Clear();
        

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

        if (mushroomList.Count > 0)
        {
            // 1. Instantiate Death Sphere (collision)
            _mushroomTrigger = Instantiate(_mushroomTriggerPrefab, transform.position, Quaternion.identity, transform);
            _mushroomTrigger.GetComponent<SphereCollider>().radius = _furthestShroom;
            _mushroomTrigger.transform.SetSiblingIndex(0);
        }
    }

    [Button]
    public void Clear()
    {
        while (mushroomList.Count > 0)
        {
            GameObject tempMushroom = mushroomList[0];
            mushroomList.RemoveAt(0);
            DestroyImmediate(tempMushroom);
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
        float scale = _mushroomPrefab.transform.localScale.x * UnityEngine.Random.Range(_minSizeMultiplier, _maxSizeMultiplier);

        if (!IsNormalFacingOrigin(hitInfo) || !HasEnoughRoom(hitInfo, scale * 0.5f * _overlapModifier)) return;

        GameObject newMushroom = Instantiate(_mushroomPrefab, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), transform);
        newMushroom.transform.localScale = new Vector3(scale, scale, scale);
        newMushroom.GetComponent<MeshRenderer>().material = _mushroomMaterial;
        newMushroom.name = "Mushroom" + (mushroomList.Count + 1);
        mushroomList.Add(newMushroom);
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

    #endregion spawning

    #region monobehavior functions

    private void Start()
    {
        
    }

    #endregion

    public void InitiateExplosion(Vector3 source)
    {
        if(_currentState != MushroomState.REST) return;

        _mushroomMaterial.SetVector("_explosionSource", source);
        StartCoroutine("TimeSinceExplosion");
    }

    IEnumerator TimeSinceExplosion()
    {
        
        float normalizedTime = 0;
        while (normalizedTime <= 10f)
        {
            _mushroomMaterial.SetFloat("_timeSinceExplosion", normalizedTime);
            normalizedTime += Time.deltaTime;
            yield return null;
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MushroomBatch : MonoBehaviour
{
    #region editor variables

    [Header("Area Properties")]
    [FoldoutGroup("Spawning")][SerializeField] public float m_radius = 3;
    [FoldoutGroup("Spawning")][SerializeField] private float m_density = 5;

    [Header("Mushroom Placement Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private float m_overlapModifier = 0.5f;
    [FoldoutGroup("Spawning")][SerializeField] private float m_minSizeMultiplier = 0.5f;
    [FoldoutGroup("Spawning")][SerializeField] private float m_maxSizeMultiplier = 1.5f;

    // DrawMeshInstanced can only draw up to 1023 meshes at a time, so we need a new list for every 1023 mushrooms
    [HideInInspector] public List<MatrixList> mushroomLists = new List<MatrixList>();

    [FoldoutGroup("External References")][SerializeField] public SSO_Mushrooms m_ssoMushrooms;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_mushroomTriggerPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_mushroomPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_particlePrefab;
    [FoldoutGroup("External References")][SerializeField] private Mesh m_mushroomMesh;
    [FoldoutGroup("External References")][SerializeField] private Material m_masterMaterial;


    [FoldoutGroup("Behavior")]
    [InfoBox("Duration while the mushrooms are safe to cross.", InfoMessageType = InfoMessageType.None)]
    [PropertyRange(0, 100)]
    [FoldoutGroup("Behavior")][SerializeField] private float m_safeDuration = 10f;


    // --- INSTANCIATED VARIABLES ---
    [HideInInspector] public GameObject MushroomTrigger;
    [HideInInspector] public GameObject ParticleSystem;

    // --- PRIVATE VARIABLES ---
    // not used during runtime
    private float m_furthestShroom = -1f;

    // used during runtime
    private MushroomState m_currentState = MushroomState.CHARGED;
    private float m_distanceFurtherestMushroom;

    private MaterialPropertyBlock m_propertyBlock;

    [HideInInspector][SerializeField] private List<GameObject> m_spawnedGameObjects = new List<GameObject>();

    #endregion

    #region spawning logic

    [Title("Functions")]
    [InfoBox("Draw spawns mushrooms in the radius defined in the spawning properties. Clear removes all which have spawned.", InfoMessageType = InfoMessageType.None)]
    [Button]
    public void Draw()
    {
        ClearAll();

        float phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
        int samples = GetRaycastSamples();

        LayerMask raycastLayerMask = new LayerMask();
        raycastLayerMask |= (1 << LayerMask.NameToLayer("NoCollision_NoRaycast"));
        raycastLayerMask |= (1 << LayerMask.NameToLayer("Collision_NoRaycast"));

        for (int i = 0; i < samples; i++)
        {
            float y = 1f - ((float)i / ((float)samples - 1f)) * 2f;
            float yRadius = Mathf.Sqrt(1 - y * y);

            float theta = phi * i;

            float x = Mathf.Cos(theta) * yRadius;
            float z = Mathf.Sin(theta) * yRadius;

            Vector3 localDirection = new Vector3(x, y, z);
            if (Physics.Raycast(transform.position, localDirection, out RaycastHit hitInfo, m_radius, ~raycastLayerMask)) SpawnMushroom(hitInfo);
        }

        if (mushroomLists.Count > 0 && m_mushroomTriggerPrefab != null)
        {
            // 1. Instantiate Death Sphere (collision)
            MushroomTrigger = Instantiate(m_mushroomTriggerPrefab, transform.position, Quaternion.identity, transform);
            MushroomTrigger.GetComponent<SphereCollider>().radius = m_furthestShroom;
            MushroomTrigger.transform.SetSiblingIndex(0);

            // 2. Instantiate Particles
            ParticleSystem = Instantiate(m_particlePrefab, transform.position, Quaternion.identity, transform);
            ParticleSystem.ShapeModule shape = ParticleSystem.GetComponent<ParticleSystem>().shape;
            shape.radius = m_furthestShroom;
            ParticleSystem.transform.SetSiblingIndex(1);
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
                GameObject newMushroom = Instantiate(m_mushroomPrefab, mushroom.GetPosition(), mushroom.rotation, transform);
                m_spawnedGameObjects.Add(newMushroom);
                newMushroom.transform.localScale = mushroom.lossyScale;
                newMushroom.name = "List" + mushroomLists.IndexOf(list) + "Mushroom" + list.matrices.IndexOf(mushroom);
                newMushroom.GetComponent<MeshRenderer>().material = m_masterMaterial;
            }
        }
    }

    [Button]
    public void ClearGameObjects()
    {
        foreach (GameObject spawnedObject in m_spawnedGameObjects) 
            if (spawnedObject != null) DestroyImmediate(spawnedObject);
        m_spawnedGameObjects.Clear();
    }

    [Button]
    public void ClearAll()
    {
        ClearGameObjects();

        int children = transform.childCount;
        for (int i = 0; i < children; ++i)
            DestroyImmediate(transform.GetChild(0).gameObject);

        mushroomLists.Clear();
        MushroomTrigger = null;
        ParticleSystem = null;
    }

    private int GetRaycastSamples()
    {
        float area = 4 * Mathf.PI * Mathf.Pow(m_radius, 2);
        return Mathf.RoundToInt(area * m_density);
    }

    private void SpawnMushroom(RaycastHit hitInfo)
    {
        if (SimplexNoise3D.SimplexNoise(hitInfo.point, 0.37f) < 0.5f) return;
        float scale = m_mushroomPrefab.transform.localScale.x * Random.Range(m_minSizeMultiplier, m_maxSizeMultiplier);

        if (!IsNormalFacingOrigin(hitInfo) || !HasEnoughRoom(hitInfo, scale * 0.5f * m_overlapModifier)) return;

        GameObject newMushroom = Instantiate(m_mushroomPrefab, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), transform);
        m_spawnedGameObjects.Add(newMushroom);
        newMushroom.transform.localScale = new Vector3(scale, scale, scale);
        newMushroom.GetComponent<MeshRenderer>().material = m_masterMaterial;
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
        if (m_furthestShroom == -1f) m_furthestShroom = Vector3.Distance(transform.position, mushroom.transform.position);
        else if (Vector3.Distance(transform.position, mushroom.transform.position) > m_furthestShroom) 
            m_furthestShroom = Vector3.Distance(transform.position, mushroom.transform.position);
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

        m_propertyBlock = new MaterialPropertyBlock();
        m_propertyBlock.SetFloat("_deflateWaveSpeed", m_ssoMushrooms.DeflateWaveSpeed);
        m_propertyBlock.SetFloat("_deflateDuration", m_ssoMushrooms.DeflateDuration);
        m_propertyBlock.SetFloat("_isInflating", 1f);
        m_propertyBlock.SetFloat("_inflateDuration", m_ssoMushrooms.InflateDuration);

        UpdateState(MushroomState.CHARGED);
    }

    private void Update()
    {
        foreach (MatrixList list in mushroomLists)
            Graphics.DrawMeshInstanced(m_mushroomMesh, 0, m_masterMaterial, list.matrices, m_propertyBlock);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_radius);
    }

    #endregion

    #region effect

    public void InitiateExplosion(Vector3 source)
    {
        if(m_currentState != MushroomState.CHARGED) return;

        List<MushroomBatch> mushroomBatches = new List<MushroomBatch>();
        mushroomBatches.AddUnique(this);

        Collider[] Colliders = Physics.OverlapSphere(transform.position, m_radius, m_ssoMushrooms.LayerToFindOverlappingTrigger);
        //Find every mushroomBactch that collide using this FindOverlappingBatches
        if (Colliders.Length > 0)
        {
            foreach (Collider collider in Colliders)
            {
                MushroomBatch mushroomBatch = collider.GetComponentInParent<MushroomBatch>();

                if (mushroomBatch != null && mushroomBatch != this)
                {
                    int batchesNumber = mushroomBatches.Count;
                    mushroomBatches.AddUnique(mushroomBatch);
                    // true if a new batch was added
                    if (batchesNumber < mushroomBatches.Count)
                    {
                        mushroomBatch.FindOverlappingBatches(ref mushroomBatches);
                    }
                }
            }
        }

        m_distanceFurtherestMushroom = Vector3.Distance(transform.position, source) + m_radius;

        if (mushroomBatches.Count > 1)
        {
            foreach(MushroomBatch mushroomBatch in mushroomBatches)
            {
                if (mushroomBatch != this)
                {
                    float newDistance = ((mushroomBatch.transform.position - source) + (mushroomBatch.transform.position - source).normalized * mushroomBatch.m_radius).magnitude;

                    if (newDistance > m_distanceFurtherestMushroom) m_distanceFurtherestMushroom = newDistance;
                }
            }
        }

        foreach (MushroomBatch mushroomBatch in mushroomBatches)
        {
            mushroomBatch.Explode(source, m_distanceFurtherestMushroom);
        }
    }

    public void FindOverlappingBatches(ref List<MushroomBatch> mushroomBatches)
    {
        Collider[] Colliders = Physics.OverlapSphere(transform.position, m_radius, m_ssoMushrooms.LayerToFindOverlappingTrigger);
        //Find every mushroomBactch that collide using this FindOverlappingBatches
        if (Colliders.Length > 0)
        {
            foreach (Collider collider in Colliders)
            {
                MushroomBatch mushroomBatch = collider.GetComponentInParent<MushroomBatch>();

                if (mushroomBatch != null && mushroomBatch != this)
                {
                    int batchesNumber = mushroomBatches.Count;
                    mushroomBatches.AddUnique(mushroomBatch);
                    // true if a new batch was added
                    if (batchesNumber < mushroomBatches.Count)
                    {
                        mushroomBatch.FindOverlappingBatches(ref mushroomBatches);
                    }
                }
            }
        }
    }

    public void Explode(Vector3 source ,float distanceFurtherestMushroom)
    {
        m_propertyBlock.SetVector("_source", source);
        m_distanceFurtherestMushroom = distanceFurtherestMushroom;

        IEnumerator coroutine = ExecuteEffect();
        StartCoroutine(coroutine);
    }

    IEnumerator ExecuteEffect()
    {
        UpdateState(MushroomState.DEFLATE);

        yield return new WaitForSeconds(m_distanceFurtherestMushroom / m_ssoMushrooms.DeflateWaveSpeed + m_ssoMushrooms.DeflateIdleDuration);

        UpdateState(MushroomState.SAFE);

        yield return new WaitForSeconds(m_safeDuration - m_ssoMushrooms.InflateDuration);

        UpdateState(MushroomState.INFLATE);

        yield return new WaitForSeconds(m_ssoMushrooms.InflateDuration);

        UpdateState(MushroomState.CHARGED);
    }

    private void UpdateState(MushroomState newState)
    {
        switch (newState) 
        {
            case MushroomState.CHARGED:
                break;

            case MushroomState.DEFLATE:
                m_propertyBlock.SetFloat("_isInflating", 0f);
                m_propertyBlock.SetFloat("_startTime", Time.time);

                ParticleSystem.MainModule main = ParticleSystem.GetComponent<ParticleSystem>().main;
                main.duration = m_distanceFurtherestMushroom / m_ssoMushrooms.DeflateWaveSpeed;
                main.startLifetime = m_distanceFurtherestMushroom / m_ssoMushrooms.DeflateWaveSpeed + m_ssoMushrooms.DeflateIdleDuration;

                ParticleSystem.GetComponent<ParticleSystem>().Play();
                break;

            case MushroomState.SAFE:
                break;

            case MushroomState.INFLATE:
                m_propertyBlock.SetFloat("_isInflating", 1f);
                m_propertyBlock.SetFloat("_startTime", Time.time);
                m_propertyBlock.SetFloat("_inflateDuration", m_ssoMushrooms.InflateDuration);
                break;
        }

        m_currentState = newState;
    }

    public MushroomState GetState()
    {
        return m_currentState;
    }


    #endregion
}
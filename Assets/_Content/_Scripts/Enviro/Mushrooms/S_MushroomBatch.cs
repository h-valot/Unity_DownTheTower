using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MushroomBatch : MonoBehaviour
{
    #region editor variables

    [Header("Area Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private float m_radius = 3;
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
    [InfoBox("Deflate Time is part of the attack time. Attack time might be a bit higher in game depending on propagation.", InfoMessageType = InfoMessageType.None)]
    [PropertyRange(0, "m_attackTime")]
    [FoldoutGroup("Behavior")][SerializeField] private float m_deflateTime = 0.5f;
    [FoldoutGroup("Behavior")][SerializeField] private float m_attackTime = 2f;
    [FoldoutGroup("Behavior")][SerializeField] private float m_inactiveTime = 10f;
    [FoldoutGroup("Behavior")][SerializeField] private float m_inflateTime = 1f;
    [FoldoutGroup("Behavior")][SerializeField] private float m_propagationSpeed = 6f;


    // --- INSTANCIATED VARIABLES ---
    [HideInInspector] public GameObject MushroomTrigger;
    [HideInInspector] public GameObject ParticleSystem;

    // --- PRIVATE VARIABLES ---
    // not used during runtime
    private float m_furthestShroom = -1f;

    // used during runtime
    private MushroomState m_currentState = MushroomState.REST;

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
        if (mushroomLists.Count == 0) return;
        ClearGameObjects();

        while (mushroomLists.Count > 0)
        {
            mushroomLists.RemoveAt(0);
        }
        if (MushroomTrigger != null)
        {
            DestroyImmediate(MushroomTrigger);
            DestroyImmediate(ParticleSystem);
            MushroomTrigger = null;
            ParticleSystem = null;
        }
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
        m_propertyBlock.SetFloat("_propagationSpeed", m_propagationSpeed);
        UpdateState(MushroomState.REST);
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
        if(m_currentState != MushroomState.REST) return;

        m_propertyBlock.SetVector("_explosionSource", source);
        IEnumerator coroutine = ExecuteEffect(source);
        StartCoroutine(coroutine);
    }

    IEnumerator ExecuteEffect(Vector3 source)
    {
        float expTime = 0;

        UpdateState(MushroomState.DEFLATE);
        while (expTime <= m_radius * 2f / m_propagationSpeed + m_attackTime)
        {
            m_propertyBlock.SetFloat("_timeSinceExplosion", expTime);
            expTime += Time.deltaTime;
            yield return null;
        }

        UpdateState(MushroomState.INACTIVE);
        float inactiveTime = 0;
        while (inactiveTime <= m_inactiveTime)
        {
            inactiveTime += Time.deltaTime;
            yield return null;
        }

        expTime = m_radius * 2f / m_propagationSpeed + m_inflateTime;
        m_propertyBlock.SetVector("_explosionSource", source + (transform.position - source) * 2f);
        UpdateState(MushroomState.INFLATE);

        while (expTime >= 0f)
        {
            m_propertyBlock.SetFloat("_timeSinceExplosion", expTime);
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
                m_propertyBlock.SetFloat("_animTime", m_deflateTime);
                break;
            case MushroomState.DEFLATE:
                // Setting duration and lifetime only works here
                ParticleSystem.MainModule main = m_particlePrefab.GetComponent<ParticleSystem>().main;
                main.duration = m_radius * 2f / m_propagationSpeed;
                main.startLifetime = m_attackTime;

                ParticleSystem.GetComponent<ParticleSystem>().Play();
                break;
            case MushroomState.INACTIVE:
                break;
            case MushroomState.INFLATE:
                m_propertyBlock.SetFloat("_animTime", m_inflateTime);
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
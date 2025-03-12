using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

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

    [FoldoutGroup("Spawning")][SerializeField] private float m_thresholdCoplanarAngle = 0f;
    [FoldoutGroup("Spawning")][SerializeField] private float m_thresholdCoplanarDistance = 0f;

    // DrawMeshInstanced can only draw up to 1023 meshes at a time, so we need a new list for every 1023 mushrooms
    [HideInInspector] public List<MatrixList> mushroomLists = new List<MatrixList>();

    [FoldoutGroup("External References")][SerializeField] public SSO_Mushrooms m_ssoMushrooms;
    [FoldoutGroup("External References")][SerializeField] public RSO_CharacterPosition m_rsoCharPos;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_mushroomTriggerPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_mushroomPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_VFXSporesPrefab;
    [FoldoutGroup("External References")][SerializeField] private GameObject m_VFXExplosionPrefab;
    [FoldoutGroup("External References")][SerializeField] private Mesh m_mushroomMesh;
    [FoldoutGroup("External References")][SerializeField] private Material m_masterMaterial;


    [FoldoutGroup("Behavior")]
    [InfoBox("Duration while the mushrooms are safe to cross.", InfoMessageType = InfoMessageType.None)]
    [PropertyRange(0, 100)]
    [FoldoutGroup("Behavior")][SerializeField] private float m_safeDuration = 10f;


    // --- INSTANCIATED VARIABLES ---
    [HideInInspector] public GameObject MushroomTrigger;
    [HideInInspector] public VisualEffect VFXspores;
    [HideInInspector] public VisualEffect VFXexplosion;

    #endregion

    #region PRIVATE VARIABLES

    private float m_furthestShroom = -1f;

    // used during runtime
    private MushroomState m_currentState = MushroomState.CHARGED;
    private float m_distanceFurtherestMushroom;

    private MaterialPropertyBlock m_propertyBlock;

    [HideInInspector][SerializeField] private List<GameObject> m_spawnedGameObjects = new List<GameObject>();
    
    [System.Serializable]
    public class CoplanarMushroomsGroup
    {
        public Vector3 Center;
        public Vector3 Normal;
        public float Radius;

        public List<GameObject> GameObjects = new List<GameObject>();
    }

    [HideInInspector][SerializeField] private List<CoplanarMushroomsGroup> m_mushroomGroups = new List<CoplanarMushroomsGroup>();

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
            MushroomTrigger.transform.SetSiblingIndex(0); // to make it a the top of the hierarchy

            //2. Instantiate particle systems
            VFXspores = Instantiate(m_VFXSporesPrefab, transform.position, Quaternion.identity, transform).GetComponent<VisualEffect>();
            VFXspores.transform.SetSiblingIndex(1); // to make it second on the hierarchy
            VFXexplosion = Instantiate(m_VFXExplosionPrefab, transform.position, Quaternion.identity, transform).GetComponent<VisualEffect>();
            VFXexplosion.transform.SetSiblingIndex(2); // to make it third on the hierarchy

            //3. Create Coplanar group of mushroom and setup VFX system
            UpdateMushroomGroupsVFX();
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
        m_mushroomGroups.Clear();

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
        VFXspores = null;
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

        if (!IsNormalFacingOrigin(hitInfo) || !HasEnoughRoom(hitInfo, m_mushroomPrefab.GetComponent<SphereCollider>().radius * scale * 0.5f * m_overlapModifier)) return;

        Quaternion rotation;
        rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), hitInfo.normal) * Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

        GameObject newMushroom = Instantiate(m_mushroomPrefab, hitInfo.point, rotation, transform);
        m_spawnedGameObjects.Add(newMushroom);
        newMushroom.transform.localScale = new Vector3(scale, scale, scale);
        AddMatrixToList(newMushroom.transform.localToWorldMatrix);
        newMushroom.GetComponent<MeshRenderer>().material = m_masterMaterial;
        newMushroom.name = "List" + mushroomLists.Count + "Mushroom" + mushroomLists[mushroomLists.Count - 1].matrices.Count;
        newMushroom.GetComponent<MushroomInstance>().Setup();
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

    public bool UpdateMushroomFromList(Vector3 position, Matrix4x4 matrix)
    {
        foreach (MatrixList list in mushroomLists)
        {
            for (int i = 0; i < list.matrices.Count; i++)
            {
                if (position.Equals(list.matrices[i].GetPosition()))
                {
                    list.matrices[i] = matrix;
                    UpdateMushroomGroupsVFX();
                    return true;
                }
            }
        }

        return false;
    }

    [Button]
    private void UpdateMushroomGroupsVFX()
    {
        m_mushroomGroups.Clear();

        foreach(GameObject go in m_spawnedGameObjects)
        {
            if (m_mushroomGroups.Count < 1)
            {
                m_mushroomGroups.Add(new CoplanarMushroomsGroup());
                m_mushroomGroups[^1].Center = go.transform.position;
                m_mushroomGroups[^1].Normal = go.transform.up;
                m_mushroomGroups[^1].Radius = 0f;
                m_mushroomGroups[^1].GameObjects.Add(go);
            }
            else
            {
                for(int i = 0; i < m_mushroomGroups.Count; i++)
                {
                    if (Vector3.Dot(go.transform.up, m_mushroomGroups[i].Normal) >= 1f - m_thresholdCoplanarAngle
                        && (Vector3.Dot(go.transform.position, m_mushroomGroups[i].Normal) / Vector3.Dot(m_mushroomGroups[i].Normal, m_mushroomGroups[i].Normal) * m_mushroomGroups[i].Normal).magnitude >= (Vector3.Dot(m_mushroomGroups[i].Center, m_mushroomGroups[i].Normal) / Vector3.Dot(m_mushroomGroups[i].Normal, m_mushroomGroups[i].Normal) * m_mushroomGroups[i].Normal).magnitude - m_thresholdCoplanarDistance
                        && (Vector3.Dot(go.transform.position, m_mushroomGroups[i].Normal) / Vector3.Dot(m_mushroomGroups[i].Normal, m_mushroomGroups[i].Normal) * m_mushroomGroups[i].Normal).magnitude <= (Vector3.Dot(m_mushroomGroups[i].Center, m_mushroomGroups[i].Normal) / Vector3.Dot(m_mushroomGroups[i].Normal, m_mushroomGroups[i].Normal) * m_mushroomGroups[i].Normal).magnitude + m_thresholdCoplanarDistance
                        )
                    {
                        m_mushroomGroups[i].GameObjects.Add(go);
                        break;
                    }
                    else if (i+1 == m_mushroomGroups.Count)
                    {
                        m_mushroomGroups.Add(new CoplanarMushroomsGroup());
                        m_mushroomGroups[^1].Center = go.transform.position;
                        m_mushroomGroups[^1].Normal = go.transform.up;
                        m_mushroomGroups[^1].Radius = 0f;
                        m_mushroomGroups[^1].GameObjects.Add(go);
                        break;
                    }
                }
            }
        }

        //Sort by number of mushroom in each group
        m_mushroomGroups.Sort((s1, s2) => s2.GameObjects.Count.CompareTo(s1.GameObjects.Count));
        //keep only the first 4 group, the bigger
        while (m_mushroomGroups.Count > 4)
        {
            m_mushroomGroups.RemoveAt(4);
        }

        foreach (CoplanarMushroomsGroup group in m_mushroomGroups)
        {
            Vector3 minPos = group.GameObjects[0].transform.position;
            Vector3 maxPos = group.GameObjects[0].transform.position;

            foreach (GameObject go in group.GameObjects)
            {
                minPos = new Vector3(Mathf.Min(minPos.x, go.transform.position.x), Mathf.Min(minPos.y, go.transform.position.y), Mathf.Min(minPos.z, go.transform.position.z));
                maxPos = new Vector3(Mathf.Max(maxPos.x, go.transform.position.x), Mathf.Max(maxPos.y, go.transform.position.y), Mathf.Max(maxPos.z, go.transform.position.z));
            }

            group.Center = (maxPos - minPos)/2 + minPos;

            group.Radius = 0f;

            foreach (GameObject go in group.GameObjects)
            {
                group.Radius = Mathf.Max(group.Radius, Vector3.Distance(group.Center, go.transform.position) + go.GetComponent<SphereCollider>().radius);
            }
        }

        float area = 0f;
        VFXspores.SetInt("NumberOfGroupsMinusOne", m_mushroomGroups.Count);
        VFXexplosion.SetInt("NumberOfGroupsMinusOne", m_mushroomGroups.Count);

        for (int i = 0; i < m_mushroomGroups.Count; i++)
        {
            float groupArea = m_mushroomGroups[i].Radius * m_mushroomGroups[i].Radius * Mathf.PI;
            VFXspores.SetVector3("G" + i.ToString() + "_center", m_mushroomGroups[i].Center);
            VFXspores.SetVector3("G" + i.ToString() + "_normal", m_mushroomGroups[i].Normal);
            VFXspores.SetFloat("G" + i.ToString() + "_radius", m_mushroomGroups[i].Radius);
            VFXspores.SetFloat("G" + i.ToString() + "_area", groupArea);
            VFXexplosion.SetVector3("G" + i.ToString() + "_center", m_mushroomGroups[i].Center);
            VFXexplosion.SetVector3("G" + i.ToString() + "_normal", m_mushroomGroups[i].Normal);
            VFXexplosion.SetFloat("G" + i.ToString() + "_radius", m_mushroomGroups[i].Radius);
            VFXexplosion.SetFloat("G" + i.ToString() + "_area", groupArea);
            area += groupArea;
        }

        VFXspores.SetFloat("Area", area);
        VFXexplosion.SetFloat("Area", area);
        VFXexplosion.SetVector3("Center", transform.position);
        VFXexplosion.SetFloat("Radius", m_radius*2f);
    }

    private void CheckCharacterDistance()
    {
        if(Vector3.Distance(m_rsoCharPos.value, transform.position) < m_ssoMushrooms.distanceDisplay)
        {
            if (m_currentState == MushroomState.CHARGED)
            {
                VFXspores.enabled = true;
            }
        }
        else
        {
            VFXspores.enabled = false;
        }
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

        VFXspores.enabled = false;

        VFXexplosion.SetFloat("DeflateDuration", m_ssoMushrooms.DeflateDuration);
        VFXexplosion.SetFloat("DeflateWaveSpeed", m_ssoMushrooms.DeflateWaveSpeed);
        VFXexplosion.SetFloat("Lifetime", m_distanceFurtherestMushroom / m_ssoMushrooms.DeflateWaveSpeed + m_ssoMushrooms.DeflateIdleDuration);
        VFXexplosion.SetFloat("MaxRandomLifetimeAdded", m_ssoMushrooms.DeflateIdleDuration * 0.25f);

        UpdateState(MushroomState.CHARGED);
    }

    private void OnEnable()
    {
        m_rsoCharPos.OnChanged += CheckCharacterDistance;
    }

    private void OnDisable()
    {
        m_rsoCharPos.OnChanged -= CheckCharacterDistance;
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


        Gizmos.color = Color.green;
        if (m_mushroomGroups.Count > 0)
        {
            foreach (CoplanarMushroomsGroup group in m_mushroomGroups)
            {
                if(group.GameObjects.Count > 0)
                {
                    Gizmos.DrawWireSphere(group.Center, group.Radius);
                }
            }
        }
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
        VFXexplosion.SetVector3("Source", source);
        m_distanceFurtherestMushroom = distanceFurtherestMushroom;

        IEnumerator coroutine = ExecuteEffect();
        StartCoroutine(coroutine);
    }

    IEnumerator ExecuteEffect()
    {
        UpdateState(MushroomState.DEFLATE);

        yield return new WaitForSeconds(m_distanceFurtherestMushroom / m_ssoMushrooms.DeflateWaveSpeed);

        VFXspores.enabled = false;

        yield return new WaitForSeconds(m_ssoMushrooms.DeflateIdleDuration);

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
                VFXspores.enabled = true;
                break;

            case MushroomState.DEFLATE:
                m_propertyBlock.SetFloat("_isInflating", 0f);
                m_propertyBlock.SetFloat("_startTime", Time.time);

                VFXexplosion.SetFloat("StartTime", Time.time);

                VFXexplosion.SendEvent("OnPlay");
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
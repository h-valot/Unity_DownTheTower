using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FoliageBatch : MonoBehaviour
{
    #region editor variables

    [Header("Area Properties")]
    [FoldoutGroup("Spawning")][SerializeField] public float m_radius = 3;
    [FoldoutGroup("Spawning")][SerializeField] private float m_density = 5;


    [Serializable]
    public class Foliage
    {
        public Mesh m_mesh;
        public Material m_material;
        public GameObject m_foliagePrefab;
        public float m__overlapModifier = 1f;
        public float m_minSizeMultiplier = 1f;
        public float m_maxSizeMultiplier = 1f;

        private MaterialPropertyBlock m_propertyBlock;
        private List<MatrixList> m_positionLists = new List<MatrixList>();
    }

    [Header("Foliage Placement Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private Foliage[] m_foliages;

    // DrawMeshInstanced can only draw up to 1023 meshes at a time, so we need a new list for every 1023 mushrooms
    


    

    [HideInInspector][SerializeField] private List<GameObject> m_spawnedGameObjects = new List<GameObject>();

    #endregion

    #region spawning logic

    [Title("Functions")]
    [InfoBox("Draw spawns mushrooms in the radius defined in the spawning properties. Clear removes all which have spawned.", InfoMessageType = InfoMessageType.None)]
    [Button]
    public void Draw()
    {
        //ClearAll();

        //float phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);
        //int samples = GetRaycastSamples();

        //LayerMask raycastLayerMask = new LayerMask();
        //raycastLayerMask |= (1 << LayerMask.NameToLayer("NoCollision_NoRaycast"));

        //for (int i = 0; i < samples; i++)
        //{
        //    float y = 1f - ((float)i / ((float)samples - 1f)) * 2f;
        //    float yRadius = Mathf.Sqrt(1 - y * y);

        //    float theta = phi * i;

        //    float x = Mathf.Cos(theta) * yRadius;
        //    float z = Mathf.Sin(theta) * yRadius;

        //    Vector3 localDirection = new Vector3(x, y, z);
        //    if (Physics.Raycast(transform.position, localDirection, out RaycastHit hitInfo, m_radius, ~raycastLayerMask)) SpawnMushroom(hitInfo);
        //}

        //if (m_foliageLists.Count > 0 && m_mushroomTriggerPrefab != null)
        //{
        //    // 1. Instantiate Death Sphere (collision)
        //    MushroomTrigger = Instantiate(m_mushroomTriggerPrefab, transform.position, Quaternion.identity, transform);
        //    MushroomTrigger.GetComponent<SphereCollider>().radius = m_furthestShroom;
        //    MushroomTrigger.transform.SetSiblingIndex(0);

        //    // 2. Instantiate Particles
        //    ParticleSystem = Instantiate(m_particlePrefab, transform.position, Quaternion.identity, transform);
        //    ParticleSystem.ShapeModule shape = ParticleSystem.GetComponent<ParticleSystem>().shape;
        //    shape.radius = m_furthestShroom;
        //    ParticleSystem.transform.SetSiblingIndex(1);
        //}
    }

    [Button]
    public void ShowGameObjects()
    {
        //ClearGameObjects();
        //foreach (MatrixList list in m_foliageLists)
        //{
        //    foreach (Matrix4x4 mushroom in list.matrices)
        //    {
        //        GameObject newMushroom = Instantiate(m_mushroomPrefab, mushroom.GetPosition(), mushroom.rotation, transform);
        //        m_spawnedGameObjects.Add(newMushroom);
        //        newMushroom.transform.localScale = mushroom.lossyScale;
        //        newMushroom.name = "List" + m_foliageLists.IndexOf(list) + "Mushroom" + list.matrices.IndexOf(mushroom);
        //        newMushroom.GetComponent<MeshRenderer>().material = m_masterMaterial;
        //    }
        //}
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

        m_foliageLists.Clear();
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
        //if (SimplexNoise3D.SimplexNoise(hitInfo.point, 0.37f) < 0.5f) return;
        //float scale = m_mushroomPrefab.transform.localScale.x * UnityEngine.Random.Range(m_minSizeMultiplier, m_maxSizeMultiplier);

        //if (!IsNormalFacingOrigin(hitInfo) || !HasEnoughRoom(hitInfo, scale * 0.5f * m_overlapModifier)) return;

        //GameObject newMushroom = Instantiate(m_mushroomPrefab, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), transform);
        //m_spawnedGameObjects.Add(newMushroom);
        //newMushroom.transform.localScale = new Vector3(scale, scale, scale);
        //newMushroom.GetComponent<MeshRenderer>().material = m_masterMaterial;
        //AddMatrixToList(newMushroom.transform.localToWorldMatrix);
        //newMushroom.name = "List" + m_foliageLists.Count + "Mushroom" + m_foliageLists[m_foliageLists.Count - 1].matrices.Count;
        //CheckFurthest(newMushroom);
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
        if (m_foliageLists.Count == 0) m_foliageLists.Add(new MatrixList());
        if (m_foliageLists[m_foliageLists.Count - 1].matrices.Count == 1023) m_foliageLists.Add(new MatrixList());
        m_foliageLists[m_foliageLists.Count - 1].matrices.Add(matrix);
    }

    public bool RemoveMushroomFromList(Vector3 position)
    {
        foreach (MatrixList list in m_foliageLists)
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
        //ClearGameObjects();
        //if (m_foliageLists[0].matrices.Count == 0) return;

        //m_propertyBlock = new MaterialPropertyBlock();
        //m_propertyBlock.SetFloat("_deflateWaveSpeed", m_ssoMushrooms.DeflateWaveSpeed);
        //m_propertyBlock.SetFloat("_deflateDuration", m_ssoMushrooms.DeflateDuration);
        //m_propertyBlock.SetFloat("_isInflating", 1f);
        //m_propertyBlock.SetFloat("_inflateDuration", m_ssoMushrooms.InflateDuration);

        ////UpdateState(MushroomState.CHARGED);
    }

    private void Update()
    {
        //foreach (MatrixList list in m_foliageLists)
        //    Graphics.DrawMeshInstanced(m_mushroomMesh, 0, m_masterMaterial, list.matrices, m_propertyBlock);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_radius);
    }

    #endregion

}
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FoliageBatch : MonoBehaviour
{
    #region editor variables

    [FoldoutGroup("Scriptables")][SerializeField] private SSO_Foliages m_ssoFoliages;

    [Header("Area Properties")]
    [FoldoutGroup("Spawning")][SerializeField] public float m_radius = 3;

    [Serializable]
    public class Foliage
    {
        public FoliageType type;
        public Mesh Mesh;
        public GameObject Prefab;
        public float Density = 5f;
        public LayerMask LayerToFindSurface;
        public float OverlapModifier = 1f;

        public SpawnType spawnType;

        public bool ForceOrientationUp = false;
        public float RotationMin = -180f;
        public float RotationMax = 180f;

        public float NoiseScale = 0.5f;
        public float NoiseStep = 0.6f;
        public float NoiseStepMax = 0.8f;

        public bool UniformScale = false;

        public Vector3 MinScaleNoiseMultiplier = Vector3.one;
        public Vector3 MaxScaleNoiseMultiplier = Vector3.one;

        public Vector3 MinSizeMultiplier = Vector3.one;
        public Vector3 MaxSizeMultiplier = Vector3.one;

        public FoliageType[] CanSpawnInFoliage = new FoliageType[] { FoliageType.NONE};

        public Texture2D ColorMap;

        [HideInInspector] public MaterialPropertyBlock PropertyBlock;
        [HideInInspector] public List<MatrixList> TransformLists = new List<MatrixList>();
    }

    [Header("Foliage Placement Properties")]
    [FoldoutGroup("Spawning")][SerializeField] private Material m_masterMaterial;
    [FoldoutGroup("Spawning")][SerializeField] private Foliage[] m_foliages;
    
    [HideInInspector][SerializeField] private List<FoliageInstance> m_spawnedFoliages = new List<FoliageInstance>();

    #endregion

    #region spawning logic

    [Title("Functions")]
    [InfoBox("Draw spawns mushrooms in the radius defined in the spawning properties. Clear removes all which have spawned.", InfoMessageType = InfoMessageType.None)]
    [Button]
    public void Draw()
    {
        ClearAll();

        float phi = Mathf.PI * (Mathf.Sqrt(5f) - 1f);

        for (int i = 0; i< m_foliages.Length; i++)
        {
            int samples = GetRaycastSamples(ref m_foliages[i]);

            for (int j = 0; j < samples; j++)
            {
                float y = 1f - ((float)j / ((float)samples - 1f)) * 2f;
                float yRadius = Mathf.Sqrt(1 - y * y);

                float theta = phi * j;

                float x = Mathf.Cos(theta) * yRadius;
                float z = Mathf.Sin(theta) * yRadius;

                Vector3 localDirection = new Vector3(x, y, z);
                if (Physics.Raycast(transform.position, localDirection, out RaycastHit hitInfo, m_radius, m_foliages[i].LayerToFindSurface)) SpawnFoliage(hitInfo, ref m_foliages[i]);
            }
        }
    }

    [Button]
    public void ShowGameObjects()
    {
        ClearGameObjects();

        foreach (Foliage foliage in m_foliages)
        {
            foreach (MatrixList list in foliage.TransformLists)
            {
                for(int i = 0; i < list.matrices.Count; i++)
                {
                    FoliageInstance newFoliage = Instantiate(foliage.Prefab, list.matrices[i].GetPosition(), list.matrices[i].rotation, transform).GetComponent<FoliageInstance>();
                    m_spawnedFoliages.Add(newFoliage);
                    newFoliage.transform.localScale = list.matrices[i].lossyScale;
                    newFoliage.position = newFoliage.transform.position;
                    newFoliage.name = newFoliage.FoliageType.ToString() + "_" + i.ToString();
                }
            }
        }
    }

    [Button]
    public void ClearGameObjects()
    {
        foreach (FoliageInstance spawnedFoliage in m_spawnedFoliages)
            if (spawnedFoliage != null) DestroyImmediate(spawnedFoliage.gameObject);
        m_spawnedFoliages.Clear();
    }

    [Button]
    public void ClearAll()
    {
        ClearGameObjects();

        int children = transform.childCount;
        for (int i = 0; i < children; ++i)
            DestroyImmediate(transform.GetChild(0).gameObject);

        foreach (Foliage foliage in m_foliages)
        {
            foliage.TransformLists.Clear();
        }
    }

    private int GetRaycastSamples(ref Foliage foliage)
    {
        float area = 4 * Mathf.PI * Mathf.Pow(m_radius, 2);
        return Mathf.RoundToInt(area * foliage.Density);
    }

    private void SpawnFoliage(RaycastHit hitInfo, ref Foliage foliage)
    {
        //assertion based on surface orientation
        if (foliage.spawnType == SpawnType.NOWHERE) return;
        else if (foliage.spawnType == SpawnType.FLOOR)
        {
            if (Vector3.Dot(hitInfo.normal, Vector3.up) < m_ssoFoliages.DotProductFloor) return;
        }
        else if (foliage.spawnType == SpawnType.FLOOR_CEILLING)
        {
            if (Vector3.Dot(hitInfo.normal, Vector3.up) < m_ssoFoliages.DotProductFloor && Vector3.Dot(hitInfo.normal, Vector3.up) > m_ssoFoliages.DotProductCeilling) return;
        }
        else if (foliage.spawnType == SpawnType.WALL)
        {
            if (Vector3.Dot(hitInfo.normal, Vector3.up) > m_ssoFoliages.DotProductWall || Vector3.Dot(hitInfo.normal, Vector3.up) < -m_ssoFoliages.DotProductWall) return;
        }

        float noise = SimplexNoise3D.SimplexNoise(hitInfo.point, foliage.NoiseScale);

        if (noise > foliage.NoiseStep)
        {
            Vector3 scale;
            //create the scale for the foliage using prefab base scale, scale coming from the noise and the random scale
            if (foliage.UniformScale)
            {
                scale = Vector3.Scale(foliage.Prefab.transform.localScale, Vector3.Lerp(foliage.MinScaleNoiseMultiplier, foliage.MaxScaleNoiseMultiplier, Matha.RemapClamped(foliage.NoiseStep, foliage.NoiseStepMax, 0f, 1f, noise))) * UnityEngine.Random.Range(foliage.MinSizeMultiplier.x, foliage.MaxSizeMultiplier.x);
            }
            else
            {
                scale = Vector3.Scale(Vector3.Scale(foliage.Prefab.transform.localScale, Vector3.Lerp(foliage.MinScaleNoiseMultiplier, foliage.MaxScaleNoiseMultiplier, Matha.RemapClamped(foliage.NoiseStep, foliage.NoiseStepMax, 0f, 1f, noise))), Matha.RandomRangeVector3(foliage.MinSizeMultiplier, foliage.MaxSizeMultiplier));
            }
            
            if (/*!IsNormalFacingOrigin(hitInfo) ||*/ !HasEnoughRoom(hitInfo, Mathf.Max(Mathf.Max(scale.x,scale.y),scale.z) * 0.5f * foliage.OverlapModifier, ref foliage)) return;

            Quaternion rotation;
            if (foliage.ForceOrientationUp)
            {
                rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(foliage.RotationMin, foliage.RotationMax), hitInfo.normal) * Quaternion.LookRotation(-Vector3.up, hitInfo.normal);
            }
            else
            {
                rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(foliage.RotationMin, foliage.RotationMax), hitInfo.normal) * Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }

            FoliageInstance newFoliage = Instantiate(foliage.Prefab, hitInfo.point, rotation, transform).GetComponent<FoliageInstance>();
            m_spawnedFoliages.Add(newFoliage);
            newFoliage.transform.localScale = scale;
            newFoliage.position = newFoliage.transform.position;

            AddMatrixToList(newFoliage.transform.localToWorldMatrix, ref foliage);
            newFoliage.name = newFoliage.FoliageType.ToString() + "_" + ((foliage.TransformLists.Count - 1) * 1023 + foliage.TransformLists[^1].matrices.Count).ToString();

            Physics.SyncTransforms();
        }
    }

    private bool HasEnoughRoom(RaycastHit hitInfo, float radius, ref Foliage foliage)
    {
        Collider[] hitlist = Physics.OverlapSphere(hitInfo.point, radius);
        bool isNotColliding = true;

        foreach(Collider collider in hitlist) 
        {
            if (collider.TryGetComponent<FoliageInstance>(out FoliageInstance foliageInstance))
            {
                foreach(FoliageType foliageType in foliage.CanSpawnInFoliage)
                {
                    if (foliageType == FoliageType.NONE)
                    {
                        isNotColliding = false;
                        break;
                    }
                    else if (foliageType == FoliageType.ALL)
                    {
                        isNotColliding = true;
                        break;
                    }
                    else
                    {
                        if(foliageInstance.FoliageType != foliageType)
                        {
                            isNotColliding = false;
                        }
                        else
                        {
                            isNotColliding = true;
                            break;
                        }
                    }
                }
            }
            if (!isNotColliding)
            {
                break;
            }
        }

        return isNotColliding;
    }

    private bool IsNormalFacingOrigin(RaycastHit hitInfo)
    {
        Vector3 rayDirection = (transform.position - hitInfo.point).normalized;
        return 0.2f <= Vector3.Dot(rayDirection.normalized, hitInfo.normal.normalized);
    }

    public void AddMatrixToList(Matrix4x4 matrix, ref Foliage foliage)
    {
        if (foliage.TransformLists.Count == 0) foliage.TransformLists.Add(new MatrixList());
        else if (foliage.TransformLists[^1].matrices.Count == 1023) foliage.TransformLists.Add(new MatrixList());
        foliage.TransformLists[^1].matrices.Add(matrix);
    }

    public bool RemoveFoliageFromList(Vector3 position, FoliageType type)
    {
        foreach (Foliage foliage in m_foliages)
        {
            if (foliage.type == type)
            {
                foreach (MatrixList list in foliage.TransformLists)
                {
                    foreach (Matrix4x4 matrice in list.matrices)
                    {
                        if (position.Equals(matrice.GetPosition()))
                        {
                            list.matrices.Remove(matrice);
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }

    public bool UpdateFoliageFromList(Vector3 position, FoliageType type, Matrix4x4 matrix)
    {
        foreach (Foliage foliage in m_foliages)
        {
            if (foliage.type == type)
            {
                foreach (MatrixList list in foliage.TransformLists)
                {
                    for (int i = 0; i < list.matrices.Count; i++)
                    {
                        if (position.Equals(list.matrices[i].GetPosition()))
                        {
                            list.matrices[i] = matrix;
                            return true;
                        }
                    }
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

        foreach (Foliage foliage in m_foliages)
        {
            foliage.PropertyBlock ??= new MaterialPropertyBlock();
            foliage.PropertyBlock.SetTexture("_ColorMap", foliage.ColorMap);
        }
    }

    private void Update()
    {
        foreach (Foliage foliage in m_foliages)
        {
            foreach (MatrixList transformList in foliage.TransformLists)
                Graphics.DrawMeshInstanced(foliage.Mesh, 0, m_masterMaterial, transformList.matrices, foliage.PropertyBlock);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_radius);
    }

    #endregion
}

public enum SpawnType
{
    EVERYWHERE,
    NOWHERE,
    FLOOR,
    WALL,
    FLOOR_CEILLING
}
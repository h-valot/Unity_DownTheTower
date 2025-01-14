using NUnit.Framework;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class RoofBorder : MonoBehaviour
{
    #region REFERENCES

    [Title("Internal references")]
    [SerializeField] private BoxCollider m_boxCollider0;
    [SerializeField] private BoxCollider m_boxCollider1;
    [SerializeField] private BoxCollider m_boxCollider2;
    [SerializeField] private BoxCollider m_boxCollider3;
    [Title("Borders")]
    [SerializeField] private Mesh m_brickMesh;
    [SerializeField] private float m_brickOffset;
    [SerializeField] private Material[] m_brickMaterials = new Material[3];

    // -- Private Variable --
    [HideInInspector, SerializeField] private List<Matrix4x4> m_bricksMatrixList = new List<Matrix4x4>();
    [HideInInspector, SerializeField] private List<Matrix4x4> m_bricksGPUList0 = new List<Matrix4x4>();
    [HideInInspector, SerializeField] private List<Matrix4x4> m_bricksGPUList1 = new List<Matrix4x4>();
    [HideInInspector, SerializeField] private List<Matrix4x4> m_bricksGPUList2 = new List<Matrix4x4>();
    [HideInInspector, SerializeField] private List<int> m_bricksMaterialIndexList = new List<int>();

    #endregion

    #region MONOBEHAVIOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.localRotation, transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void Update()
    {
        Graphics.DrawMeshInstanced(m_brickMesh, 0, m_brickMaterials[0], m_bricksGPUList0);
        Graphics.DrawMeshInstanced(m_brickMesh, 0, m_brickMaterials[1], m_bricksGPUList1);
        Graphics.DrawMeshInstanced(m_brickMesh, 0, m_brickMaterials[2], m_bricksGPUList2);
    }

    #endregion

    #region GENERATE

    [Button]
    public void Generate()
    {
        ClearAll();

        Vector3 brickExtents = m_brickMesh.bounds.extents;
        int numberOfBrickX = Mathf.RoundToInt((transform.localScale.x + m_brickOffset * 2) / (brickExtents.x * 2));
        float brickWidthXPercent = (transform.localScale.x + m_brickOffset * 2) / numberOfBrickX / (brickExtents.x * 2);
        int numberOfBrickZ = Mathf.RoundToInt((transform.localScale.z + m_brickOffset * 2 - brickExtents.z * 4) / (brickExtents.x * 2));
        float brickWidthZPercent = (transform.localScale.z + m_brickOffset * 2 - brickExtents.z * 4) / numberOfBrickZ / (brickExtents.x * 2);

        Vector3 offsetVertical = transform.up * transform.localScale.y / 2;

        for (int i = 0; i < numberOfBrickX; i++)
        {
            Matrix4x4 matrix = new Matrix4x4();

            matrix = Matrix4x4.TRS(transform.position + offsetVertical + transform.forward * transform.localScale.z / 2 + transform.forward * m_brickOffset - transform.right * transform.localScale.x / 2 - transform.right * m_brickOffset + transform.right * brickExtents.x * 2 * i * brickWidthXPercent,
                                    Quaternion.Euler(new Vector3(0, 0, 0) + transform.eulerAngles),
                                    new Vector3(brickWidthXPercent, 1, 1)
                                    );

            m_bricksMatrixList.Add(matrix);

            m_bricksMaterialIndexList.Add(Mathf.FloorToInt((m_brickMaterials.Length - 0.000001f) * Random.value));
        }

        for (int i = 0; i < numberOfBrickZ; i++)
        {
            Matrix4x4 matrix = new Matrix4x4();

            matrix = Matrix4x4.TRS(transform.position + offsetVertical + transform.right * transform.localScale.x / 2 + transform.right * m_brickOffset + transform.forward * transform.localScale.z / 2 + transform.forward * m_brickOffset - transform.forward * brickExtents.z * 2 - transform.forward * brickExtents.x * 2 * i * brickWidthZPercent,
                                    Quaternion.Euler(new Vector3(0, 90, 0) + transform.eulerAngles),
                                    new Vector3(brickWidthZPercent, 1, 1)
                                    );

            m_bricksMatrixList.Add(matrix);
            m_bricksMaterialIndexList.Add(Mathf.FloorToInt((m_brickMaterials.Length - 0.000001f) * Random.value));
        }

        for (int i = 0; i < numberOfBrickX; i++)
        {
            Matrix4x4 matrix = new Matrix4x4();

            matrix = Matrix4x4.TRS(transform.position + offsetVertical - transform.forward * transform.localScale.z / 2 - transform.forward * m_brickOffset + transform.right * transform.localScale.x / 2 + transform.right * m_brickOffset - transform.right * brickExtents.x * 2 * i * brickWidthXPercent,
                                    Quaternion.Euler(new Vector3(0, 180, 0) + transform.eulerAngles),
                                    new Vector3(brickWidthXPercent, 1, 1)
                                    );

            m_bricksMatrixList.Add(matrix);
            m_bricksMaterialIndexList.Add(Mathf.FloorToInt((m_brickMaterials.Length - 0.000001f) * Random.value));
        }

        for (int i = 0; i < numberOfBrickZ; i++)
        {
            Matrix4x4 matrix = new Matrix4x4();

            matrix = Matrix4x4.TRS(transform.position + offsetVertical - transform.right * transform.localScale.x / 2 - transform.right * m_brickOffset - transform.forward * transform.localScale.z / 2 - transform.forward * m_brickOffset + transform.forward * brickExtents.z * 2 + transform.forward * brickExtents.x * 2 * i * brickWidthZPercent,
                                    Quaternion.Euler(new Vector3(0, 270, 0) + transform.eulerAngles),
                                    new Vector3(brickWidthZPercent, 1, 1)
                                    );

            m_bricksMatrixList.Add(matrix);
            m_bricksMaterialIndexList.Add(Mathf.FloorToInt((m_brickMaterials.Length - 0.000001f) * Random.value));
        }

        m_bricksMaterialIndexList[^1] = m_bricksMaterialIndexList[0];

        for (int i = 0; i < m_bricksMaterialIndexList.Count; i++)
        {
            switch (m_bricksMaterialIndexList[i])
            {
                case 0:
                    m_bricksGPUList0.Add(m_bricksMatrixList[i]);
                    break;
                case 1:
                    m_bricksGPUList1.Add(m_bricksMatrixList[i]);
                    break;
                case 2:
                    m_bricksGPUList2.Add(m_bricksMatrixList[i]);
                    break;
            }
        }

        m_boxCollider0.enabled = true;
        m_boxCollider1.enabled = true;
        m_boxCollider2.enabled = true;
        m_boxCollider3.enabled = true;

        m_boxCollider0.size = new Vector3(1 + 2 / transform.localScale.x * m_brickOffset, 2 / transform.localScale.y * brickExtents.y, 2 / transform.localScale.z * brickExtents.z);
        m_boxCollider0.center = new Vector3(0, 0.5f + 1 / transform.localScale.y * brickExtents.y, 0.5f - (1 / transform.localScale.z * brickExtents.z) + (1 / transform.localScale.z * m_brickOffset));
        m_boxCollider1.size = new Vector3(2 / transform.localScale.x * brickExtents.z, 2 / transform.localScale.y * brickExtents.y, 1 + 2 / transform.localScale.z * m_brickOffset);
        m_boxCollider1.center = new Vector3(0.5f - (1 / transform.localScale.x * brickExtents.z) + (1 / transform.localScale.x * m_brickOffset), 0.5f + 1 / transform.localScale.y * brickExtents.y, 0);
        m_boxCollider2.size = new Vector3(1 + 2 / transform.localScale.x * m_brickOffset, 2 / transform.localScale.y * brickExtents.y, 2 / transform.localScale.z * brickExtents.z);
        m_boxCollider2.center = new Vector3(0, 0.5f + 1 / transform.localScale.y * brickExtents.y, -0.5f + (1 / transform.localScale.z * brickExtents.z) - (1 / transform.localScale.z * m_brickOffset));
        m_boxCollider3.size = new Vector3(2 / transform.localScale.x * brickExtents.z, 2 / transform.localScale.y * brickExtents.y, 1 + 2 / transform.localScale.z * m_brickOffset);
        m_boxCollider3.center = new Vector3(-0.5f + (1 / transform.localScale.x * brickExtents.z) - (1 / transform.localScale.x * m_brickOffset), 0.5f + 1 / transform.localScale.y * brickExtents.y, 0);
    }

    [Button]
    public void ClearAll()
    {
        m_bricksMatrixList = new List<Matrix4x4>();
        m_bricksGPUList0 = new List<Matrix4x4>();
        m_bricksGPUList1 = new List<Matrix4x4>();
        m_bricksGPUList2 = new List<Matrix4x4>();
        m_bricksMaterialIndexList = new List<int>();
        m_boxCollider0.enabled = false;
        m_boxCollider1.enabled = false;
        m_boxCollider2.enabled = false;
        m_boxCollider3.enabled = false;
    }

    #endregion
}

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class MushroomBatch : MonoBehaviour
{

    [Header("Sphere Properties")]
    [SerializeField] private float _radius = 3;
    [SerializeField] private float _density = 5;

    [Header("Mushroom Placement Properties")]
    [SerializeField] private float _overlapModifier = 0.5f;
    [SerializeField] private bool _isRandom = true;
    [EnableIf("_isRandom")]
    [SerializeField] private float _minSizeMultiplier = 0.5f;
    [EnableIf("_isRandom")]
    [SerializeField] private float _maxSizeMultiplier = 1.5f;

    [Header("Mushroom Properties")]
    [SerializeField] private GameObject _mushroomPrefab;

    public List<GameObject> mushroomList = new List<GameObject>();

    [Button]
    public void Draw()
    {
        Clear();

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
        newMushroom.name = "Mushroom" + (mushroomList.Count + 1);
        mushroomList.Add(newMushroom);

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
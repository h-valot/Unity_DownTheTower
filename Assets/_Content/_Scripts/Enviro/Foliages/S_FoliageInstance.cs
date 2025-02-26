using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class FoliageInstance : MonoBehaviour
{
    [SerializeField, HideInInspector] public Vector3 position;

    public FoliageType FoliageType;

    [Button]
    private void Delete()
    {
        if (transform.parent.TryGetComponent<FoliageBatch>(out FoliageBatch batch))
            if (!batch.RemoveFoliageFromList(position, FoliageType)) Debug.LogWarning("Could not find foliage in list.");
        DestroyImmediate(gameObject);
    }

    [Button]
    private void UpdateMatrix()
    {
        if (transform.parent.TryGetComponent<FoliageBatch>(out FoliageBatch batch))
        {
            if (!batch.UpdateFoliageFromList(position, FoliageType, transform.localToWorldMatrix)) Debug.LogWarning(position);
            else position = transform.position;
        }
    }
}

public enum FoliageType
{
    NONE,
    ALL,
    MOSS,
    SARRACENIAL,
    MONSTERA,
    RAFFLESIE,
}
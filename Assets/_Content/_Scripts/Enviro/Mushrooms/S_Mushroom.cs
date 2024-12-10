using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [Button]
    private void Delete()
    {
        if (transform.parent.TryGetComponent<MushroomBatch>(out MushroomBatch batch))
            if (!batch.RemoveMushroomFromList(transform.position)) Debug.LogWarning("Could not find mushroom in list.");
        DestroyImmediate(gameObject);
    }

    [Button]
    private void PrintMatrix()
    {
        Debug.Log(PrintUtils.Matrix4x4(transform.localToWorldMatrix));
    }
}

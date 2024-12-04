using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [Button]
    private void Delete()
    {
        if(transform.parent.TryGetComponent<MushroomBatch>(out MushroomBatch batch)) batch.mushroomList.Remove(gameObject);
        DestroyImmediate(gameObject);
    }
}

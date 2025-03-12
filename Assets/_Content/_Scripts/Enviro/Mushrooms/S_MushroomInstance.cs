using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

public class MushroomInstance : MonoBehaviour
{
    [HideInInspector][SerializeField]private Vector3 position;

    public void Setup()
    {
        position = transform.position;
    }

    [Button]
    private void Delete()
    {
        if (transform.parent.TryGetComponent<MushroomBatch>(out MushroomBatch batch))
            if (!batch.RemoveMushroomFromList(position)) Debug.LogWarning("Could not find mushroom in list.");
        DestroyImmediate(gameObject);
    }

    [Button]
    private void UpdateMatrix()
    {
        if (transform.parent.TryGetComponent<MushroomBatch>(out MushroomBatch batch))
        {
            if (!batch.UpdateMushroomFromList(position, transform.localToWorldMatrix)) Debug.LogWarning("Could not find mushroom instance in matrix list.");
            else
            {
                position = transform.position;
            }
        }
    }
}

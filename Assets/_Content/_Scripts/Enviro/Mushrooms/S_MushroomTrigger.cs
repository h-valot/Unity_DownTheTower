using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomTrigger : MonoBehaviour
{
    private MushroomBatch _parent;

    private void Start()
    {
        _parent = transform.parent.GetComponent<MushroomBatch>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _parent.InitiateExplosion(other.gameObject.transform.position);
    }
}

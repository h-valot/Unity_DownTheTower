using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomTrigger : MonoBehaviour
{
    private MushroomBatch _parent;
    private List<ObjectPosition> _objectLastPositions;

    private void Start()
    {
        _parent = transform.parent.GetComponent<MushroomBatch>();
        _objectLastPositions = new List<ObjectPosition>();
    }

    private void OnTriggerStay(Collider other)
    {
        Vector3 newPos = other.transform.position;
        foreach (ObjectPosition op in _objectLastPositions)
        {
            if (!(op.gObject == other.gameObject)) continue;

            if (Mathf.Round(op.position.sqrMagnitude) == Mathf.Round(newPos.sqrMagnitude)) return;

            op.position = new Vector3(newPos.x, newPos.y, newPos.z);
            _parent.InitiateExplosion(newPos);
            return;
        }

        ObjectPosition newItem = new ObjectPosition();
        newItem.gObject = other.gameObject;
        newItem.position = newPos;
        _objectLastPositions.Add(newItem);
        _parent.InitiateExplosion(newItem.position);
    }

    private void OnTriggerExit(Collider other)
    {
        ObjectPosition toDelete = new ObjectPosition();
        foreach (ObjectPosition op in _objectLastPositions)
        {
            if (op.gObject == other.gameObject)
            {
                toDelete = op;
                break;
            }
        }
        if (toDelete.gObject != null) _objectLastPositions.Remove(toDelete);
    }
}

public class ObjectPosition
{
    public GameObject gObject;
    public Vector3 position;
}

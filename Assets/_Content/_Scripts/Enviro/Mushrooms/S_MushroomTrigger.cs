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
        foreach (ObjectPosition objectPosition in _objectLastPositions)
        {
			// Assertion
            if (!(objectPosition.gObject == other.gameObject)) continue;
            if (Mathf.Round(objectPosition.position.sqrMagnitude) == Mathf.Round(newPos.sqrMagnitude)) return;

            objectPosition.position = new Vector3(newPos.x, newPos.y, newPos.z);
            _parent.InitiateExplosion(newPos);

            if (other.gameObject.TryGetComponent<CharacterMotor>(out var character) 
			&& _parent.GetState() == MushroomState.DEFLATE)
			{
				character.HandleDeath(DeathType.GAS);
			}

            return;
        }

        ObjectPosition newItem = new ObjectPosition();
        newItem.gObject = other.gameObject;
        newItem.position = newPos;
        _objectLastPositions.Add(newItem);
        _parent.InitiateExplosion(newItem.position); 

        if (other.gameObject.TryGetComponent<CharacterMotor>(out var newChara) 
		&& _parent.GetState() == MushroomState.DEFLATE) 
		{
			newChara.HandleDeath(DeathType.GAS);
		}
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

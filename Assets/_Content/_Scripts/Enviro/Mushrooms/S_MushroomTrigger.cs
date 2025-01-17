using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MushroomTrigger : MonoBehaviour
{
    private MushroomBatch _parent;

    private void Awake()
    {
        _parent = transform.parent.GetComponent<MushroomBatch>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
        {
            if (rigidbody.velocity.magnitude > _parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                _parent.InitiateExplosion(other.transform.position);
            }
        }
        else if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
        {
            if (navMeshAgent.velocity.magnitude > _parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                _parent.InitiateExplosion(other.transform.position);
            }
        }
    }
}

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MushroomTrigger : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayFallDeath m_rsePlayFallDeath;

    private MushroomBatch _parent;

    private void Awake()
    {
        _parent = transform.parent.GetComponent<MushroomBatch>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            if (rigidbody.velocity.magnitude > _parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                if(other.TryGetComponent<CharacterMotor>(out var character))
                {
                    if(_parent.GetState() == MushroomState.DEFLATE || _parent.GetState() == MushroomState.CHARGED)
                    {
						character.HandleDeath(DeathType.GAS);
					}
                }
                _parent.InitiateExplosion(other.transform.position);
            }
        }
        else if (other.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
        {
            if (navMeshAgent.velocity.magnitude > _parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                _parent.InitiateExplosion(other.transform.position);
            }
        }
    }
}

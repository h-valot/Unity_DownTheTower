using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class MushroomTrigger : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayDeath m_rseDisplayDeath;

    private MushroomBatch m_parent;

    private void Awake()
    {
        m_parent = transform.parent.GetComponent<MushroomBatch>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            if (rigidbody.linearVelocity.magnitude > m_parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                if (other.TryGetComponent<CharacterMotor>(out var character))
                {
                    if (m_parent.GetState() == MushroomState.DEFLATE || m_parent.GetState() == MushroomState.CHARGED)
                    {
						character.HandleDeath(DeathType.GAS);
					}
                }
                m_parent.InitiateExplosion(other.transform.position);
            }
        }
        else if (other.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
        {
            if (navMeshAgent.velocity.magnitude > m_parent.m_ssoMushrooms.MinimalVelocityToTrigger)
            {
                m_parent.InitiateExplosion(other.transform.position);
            }
        }
    }
}

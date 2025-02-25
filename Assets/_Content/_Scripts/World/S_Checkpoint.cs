using Sirenix.OdinInspector;
using UnityEngine;

public class Checkpoint : GameStart
{
	[FoldoutGroup("Internal references")][SerializeField] private SphereCollider m_sphereCollider;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_LastCheckpointReached m_rsoLastCheckpointReached;

	private void OnTriggerEnter(Collider other)
	{
		m_rsoLastCheckpointReached.value = this;
	}

#if UNITY_EDITOR

	public new void OnDrawGizmos()
	{
		// Display the game start gizmos in editor
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, 1f);
		Gizmos.DrawLine(transform.position, 1.5f * transform.forward.normalized + transform.position);

		// Display the checkpoint gizmos in editor
		if (m_sphereCollider)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position + m_sphereCollider.center, m_sphereCollider.radius);
		}
	}
	
#endif
}
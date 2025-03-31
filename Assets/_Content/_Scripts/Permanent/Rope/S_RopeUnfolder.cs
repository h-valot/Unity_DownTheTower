using Sirenix.OdinInspector;
using UnityEngine;

public class RopeUnfolder : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private SphereCollider m_sphereCollider;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	private RopeGraphics m_ropeGraphics;
	private Rope m_rope;
	private float m_distanceLimit;
	private int m_physicInstantiatedAmount;
	private Vector3 m_startPosition;

	private void FixedUpdate()
	{
		// Assertion
		if (!m_rope) return;

		// Avoid the unfold to pass through colliders
		if (Physics.Raycast(transform.position + Vector3.up * m_ssoRope.MaxLengthOffset, Vector3.down, out var hitInfo, m_ssoRope.MaxLengthOffset * 2f, m_ssoRope.UnfolderLayerToInclude))
		{
			Destroy(gameObject);
			return;
		}

		// Avoid the total length to exceed the max rope length
		if (m_rope.GetTotalLength() + m_distanceLimit * m_physicInstantiatedAmount >= m_ssoRope.MaxLength - m_ssoRope.MaxLengthOffset)
		{
			Destroy(gameObject);
			return;
		}

		// Exit without destroying when the unfolder hasn't reach another spawn distance threshold
		if ((transform.position - m_startPosition).magnitude / m_physicInstantiatedAmount < m_distanceLimit)
		{
			return;
		}

		// - Spawn new physic segment -
		m_physicInstantiatedAmount++;

		var newPhysic = Instantiate(m_ssoRope.PfRopePhysic, m_ropeGraphics.Physics[^1].transform);
		newPhysic.transform.rotation = Quaternion.LookRotation(Vector3.down);
		newPhysic.transform.position = m_startPosition + Vector3.down * m_distanceLimit * m_physicInstantiatedAmount;
		newPhysic.OnInteractedWithRef += m_ropeGraphics.OnInteracted;

		newPhysic.SetLimit(m_distanceLimit);
		newPhysic.ToggleFree(true);

		m_ropeGraphics.Physics[^1].ToggleFree(false);
		m_ropeGraphics.Physics[^1].Connect(newPhysic.Rigidbody);
		m_ropeGraphics.Physics.Add(newPhysic);
	}

	public void Initialize(Rope rope, RopeGraphics ropeGraphics)
	{
		m_rope = rope;
		m_ropeGraphics = ropeGraphics;

		m_sphereCollider.radius = m_ssoRope.PhysicSphereRadius;
		m_startPosition = transform.position;
		m_distanceLimit = m_ssoRope.PhysicSphereRadius + m_ssoRope.PhysicJointOffset;
	}
}
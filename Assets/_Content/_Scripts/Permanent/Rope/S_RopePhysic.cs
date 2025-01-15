using Sirenix.OdinInspector;
using UnityEngine;

public class RopePhysic : RopeInteractable
{
	[FoldoutGroup("Internal references")][SerializeField] public Rigidbody Rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] public ConfigurableJoint Joint;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereCollider;

	private SoftJointLimit m_softJointLimit;

	public void Connect(Rigidbody rigidbody)
	{
		Joint.connectedBody = rigidbody;
	}

	public void SetLimit(float limit)
	{
		m_softJointLimit.limit = limit;
		Joint.linearLimit = m_softJointLimit;
	}

	public void ToggleFree(bool isEnabled)
	{
		var newMotion = isEnabled ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;

		Joint.xMotion = newMotion;
		Joint.yMotion = newMotion;
		Joint.zMotion = newMotion;
	}
}
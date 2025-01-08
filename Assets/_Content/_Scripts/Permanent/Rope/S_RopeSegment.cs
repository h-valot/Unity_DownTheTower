using Sirenix.OdinInspector;
using UnityEngine;

public class RopeSegment : Interactable
{
	[Title("Rope Segment")]
	public float ColliderRadius;
	public LayerMask LayerToExclude;

	[FoldoutGroup("Internal references")][SerializeField] public Rigidbody Rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] public ConfigurableJoint Joint;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereTrigger;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereCollider;

	private SoftJointLimit m_softJointLimit;

	private void Start()
	{
		SphereCollider.radius = ColliderRadius;
	}

	public void Connect(Rigidbody rigidbody)
	{
		Joint.connectedBody = rigidbody;
	}

	public void SetLimit(float limit)
	{
		m_softJointLimit.limit = limit;
		Joint.linearLimit = m_softJointLimit;
	}

	public void Freeze()
	{
		Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		Rigidbody.isKinematic = true;
	}

	public void Free()
	{
		Joint.xMotion = ConfigurableJointMotion.Free;
		Joint.yMotion = ConfigurableJointMotion.Free;
		Joint.zMotion = ConfigurableJointMotion.Free;
	}

	public void DisableCollider()
	{
		SphereCollider.excludeLayers = LayerToExclude;
	}
}
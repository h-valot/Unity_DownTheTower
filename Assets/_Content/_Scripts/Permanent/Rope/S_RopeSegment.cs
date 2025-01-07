using Sirenix.OdinInspector;
using UnityEngine;

public class RopeSegment : Interactable
{
	[Title("Rope Segment")]
	public float ColliderRadius;
	[SerializeField] private float m_inBetweenDistance;

	[FoldoutGroup("Internal references")][SerializeField] public Rigidbody Rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] public ConfigurableJoint Joint;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereTrigger;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereCollider;

	public float InBetweenDistance => m_inBetweenDistance + ColliderRadius;

	private void Start()
	{
		SphereCollider.radius = ColliderRadius;
	}

	public void Connect(Rigidbody rigidbody)
	{
		Joint.connectedBody = rigidbody;
	}
}
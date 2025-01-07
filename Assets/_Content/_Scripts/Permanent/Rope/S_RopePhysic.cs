using Sirenix.OdinInspector;
using UnityEngine;

public class RopePhysic : MonoBehaviour
{
	public float Length;

	[FoldoutGroup("Internal references")][SerializeField] public Rigidbody Rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] private ConfigurableJoint m_joint;
	[FoldoutGroup("Internal references")][SerializeField] private BoxCollider m_boxCollider;

	private void Start()
	{
		m_boxCollider.size = new Vector3(Length / 4, Length / 4, Length);
	}

	public void Connect(Rigidbody rigidbody)
	{
		m_joint.connectedBody = rigidbody;
	}
}
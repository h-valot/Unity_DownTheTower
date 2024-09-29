using UnityEngine;

public class RopeSegment : MonoBehaviour
{
	[Header("Internal references")]
	public Rigidbody rb;
	public ConfigurableJoint configurableJoint;
	public CapsuleCollider capsuleCollider;
	public Transform top, bot;

	[Header("Scriptable references")]
	public RopeConfig _ropeConfig;

	public void Connect(Rigidbody connectedRigidbody)
	{
		configurableJoint.connectedBody = connectedRigidbody;
	}
}
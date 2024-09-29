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

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		Gizmos.DrawCube(
			transform.position + capsuleCollider.center, 
			new Vector3(capsuleCollider.radius, capsuleCollider.height, capsuleCollider.radius)
		);
	}

#endif
}
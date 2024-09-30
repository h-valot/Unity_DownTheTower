using UnityEngine;

public class RopeSegment : MonoBehaviour
{
	[Header("Internal references")]
	public Rigidbody rb;
	public ConfigurableJoint joint;
	public CapsuleCollider capsuleCollider;
	public Transform next;
	public GameObject graphicsParent;

	[Header("Scriptable references")]
	public RopeConfig _ropeConfig;

	public void Connect(Rigidbody connectedBody)
	{
		joint.connectedBody = connectedBody;
	}

	public void Disconnect()
	{
		joint.connectedBody = null;
	}

	public void Hide()
	{
		graphicsParent.SetActive(false);
		capsuleCollider.enabled = false;
	}

	public void Show()
	{
		graphicsParent.SetActive(true);
		capsuleCollider.enabled = true;
	}
}
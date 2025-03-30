using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class RopePhysic : RopeInteractable
{
	[FoldoutGroup("Tweakable values")]
	[InfoBox("Linear damping of the rigidbody when the physic spawns.", InfoMessageType.None)]
	[SerializeField] private float m_startLinearDamping;

	[FoldoutGroup("Tweakable values")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Final value of the lerp of the linear damping of the rigidbody.", InfoMessageType.None)]
	[SerializeField] private float m_defaultLinearDamping;

	[FoldoutGroup("Tweakable values")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Value substracted to the start linear damping value every frame until it reaches the default linear damping value.", InfoMessageType.None)]
	[SerializeField] private float m_fadeLinearDamping;

	[FoldoutGroup("Internal references")][SerializeField] public Rigidbody Rigidbody;
	[FoldoutGroup("Internal references")][SerializeField] public ConfigurableJoint Joint;
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereCollider;

	private SoftJointLimit m_softJointLimit;
	private Rigidbody m_connectedRigidbody;

	public void Connect(Rigidbody rigidbody)
	{
		Joint.connectedBody = rigidbody;
		m_connectedRigidbody = rigidbody;

		Rigidbody.linearDamping = m_startLinearDamping;
		StartCoroutine(FadeLinearDamping());
	}

	private IEnumerator FadeLinearDamping()
	{
		while (Rigidbody.linearDamping > m_defaultLinearDamping)
		{
			Rigidbody.linearDamping -= m_fadeLinearDamping;
			yield return null;
		}
		Rigidbody.linearDamping = m_defaultLinearDamping;
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

	public void SetDormant(bool isDormant)
	{
		Rigidbody.isKinematic = isDormant;
		Joint.connectedBody = isDormant ? null : m_connectedRigidbody;
		SphereCollider.enabled = !isDormant;
	}
}
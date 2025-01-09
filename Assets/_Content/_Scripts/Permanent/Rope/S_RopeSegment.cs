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
	private SoftJointLimitSpring m_softJointLimitSpring;
	private LayerMask m_defaultLayerToExclude;

	public bool IsConnected => Joint.connectedBody != null;

	private void Start()
	{
		m_defaultLayerToExclude = SphereCollider.excludeLayers;
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

	public void ToggleSpring(bool isEnabled)
	{
		if (isEnabled)
		{
			m_softJointLimitSpring.spring = 100;
			m_softJointLimitSpring.damper = 10;
			Joint.connectedMassScale = 1000f;
		}
		else
		{
			m_softJointLimitSpring.spring = 0;
			m_softJointLimitSpring.damper = 0;
			Joint.connectedMassScale = 1f;
		}
		Joint.linearLimitSpring = m_softJointLimitSpring;
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

	public void ToggleCollider(bool isEnabled)
	{
		if (isEnabled)
		{
			SphereCollider.excludeLayers = m_defaultLayerToExclude;
		}
		else
		{
			SphereCollider.excludeLayers = LayerToExclude;
		}
	}

	public void ToggleTrigger(bool isEnabled)
	{
		if (isEnabled)
		{
			SphereTrigger.excludeLayers = m_defaultLayerToExclude;
		}
		else
		{
			SphereTrigger.excludeLayers = LayerToExclude;
		}
	}
}
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class RopeUnfolder : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private SphereCollider m_sphereCollider;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_graphicsParent;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	private RopeGraphics m_ropeGraphics;
	private Rope m_rope;
	private float m_distanceLimit;
	private float m_timeout;
	private int m_physicInstantiatedAmount;
	private Vector3 m_startPosition;

	private void Start()
	{
		m_sphereCollider.radius = m_ssoRope.PhysicSphereRadius;

		m_startPosition = transform.position;
		m_distanceLimit = m_ssoRope.PhysicSphereRadius + m_ssoRope.PhysicJointOffset;
		m_timeout = m_ssoRope.UnfolderTimeoutDelay;
	}

	private void FixedUpdate()
	{
		// Assertion
		if (!m_rope) return;

		float totalDistanceTravelled = (transform.position - m_startPosition).magnitude;

		if (m_rope.GetTotalLength() + m_distanceLimit * m_physicInstantiatedAmount < m_ssoRope.MaxLength
		&& totalDistanceTravelled / m_physicInstantiatedAmount >= m_distanceLimit)
		{
			m_physicInstantiatedAmount++;
			m_timeout = m_ssoRope.UnfolderTimeoutDelay;

			// Spawn new physic segment
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
		else
		{
			// Handle timeout
			m_timeout -= Time.fixedDeltaTime;
			if (m_timeout <= 0f)
			{
				Disappear();
			}
		}
	}

	public void Initialize(Rope rope, RopeGraphics ropeGraphics)
	{
		m_rope = rope;
		m_ropeGraphics = ropeGraphics;
	}

	public void Disappear()
	{
		Sequence sequenceDestroy = DOTween.Sequence().Pause();
		sequenceDestroy.Insert(0f, m_graphicsParent.DOScale(0f, m_ssoRope.DestroyDuration));
		sequenceDestroy.SetId($"{gameObject.GetInstanceID()}-disappear");
		sequenceDestroy.Play().OnComplete(Destroy);
	}

	private void Destroy()
	{
		DOTween.Kill($"{gameObject.GetInstanceID()}-disappear");
		Destroy(gameObject);
	}
}
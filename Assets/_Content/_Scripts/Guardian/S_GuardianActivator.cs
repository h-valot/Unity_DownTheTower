using Sirenix.OdinInspector;
using UnityEngine;

public class GuardianActivator : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private SphereCollider m_sphereCollider;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Guardian m_ssoGuardian;

	public bool IsActive;

	private void Start()
	{
		m_sphereCollider.radius = m_ssoGuardian.ActivationRadius;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.TryGetComponent<CharacterMotor>(out var character)
		| collider.TryGetComponent<Torch>(out var torch)
		| collider.TryGetComponent<Rope>(out var rope))
		{
			IsActive = true;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			IsActive = false;
		}
	}
}
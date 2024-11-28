using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterGraphics : MonoBehaviour
{
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;

	private Transform m_aimingLookAt;
	private Rigidbody m_rigidbody;
	private bool m_isInitialized;

	private const float k_MinimumThreshold = 0.1f;

	public void Initialize(Transform aimingLookAt, Rigidbody rigidbody, Quaternion startRotation)
	{
		m_aimingLookAt = aimingLookAt;
		m_rigidbody = rigidbody;
		m_isInitialized = true;

		transform.localRotation = startRotation;
    }

	private void LateUpdate()
    {
		if (!m_isInitialized) return;

		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			Vector3 planarMovement = new Vector3(m_rigidbody.velocity.x, 0, m_rigidbody.velocity.z);
			if (planarMovement.magnitude >= k_MinimumThreshold)
			{
				transform.localRotation = Quaternion.Lerp(
					transform.localRotation,
					Quaternion.LookRotation(planarMovement, Vector3.up),
					Time.deltaTime * m_ssoCharacter.RotationSpeed
				);
			}
		}
		else if (m_rsoCameraStyle.value == CameraStyle.AIMING)
		{
			transform.forward = m_aimingLookAt.position - new Vector3(
				transform.position.x, 
				m_aimingLookAt.position.y, 
				transform.position.z
			);
			transform.forward = -transform.forward;
		}
	}
}
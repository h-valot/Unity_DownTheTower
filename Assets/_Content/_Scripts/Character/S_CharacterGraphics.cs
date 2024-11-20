using UnityEngine;

public class CharacterGraphics : MonoBehaviour
{
	[Header("Scriptable references")]
	[SerializeField] private OldCharacterConfig m_characterConfig;
	[Space(5)]
	[SerializeField] private RSO_CameraStyle m_rsoCameraStyle;

	private Transform m_aimingLookAt;
	private Rigidbody m_rigidbody;
	private bool m_isInitialized;

	private const float k_MinimumThreshold = 0.1f;

	public void Initialize(Transform aimingLookAt, Rigidbody rigidbody)
	{
		m_aimingLookAt = aimingLookAt;
		m_rigidbody = rigidbody;
		m_isInitialized = true;
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
					Time.deltaTime * m_characterConfig.rotationSpeed
				);
			}
		}
		else if (m_rsoCameraStyle.value == CameraStyle.AIMING)
		{
			transform.forward = m_aimingLookAt.position - new Vector3(
				transform.transform.position.x, 
				m_aimingLookAt.position.y, 
				transform.transform.position.z
			);
		}
	}
}
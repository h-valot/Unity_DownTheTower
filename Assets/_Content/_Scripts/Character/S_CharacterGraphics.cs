using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterGraphics : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private List<Transform> m_ragdollTransforms;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Camera m_ssoCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;

	private Transform m_aimingLookAt;
	private Rigidbody m_rigidbody;
	private bool m_isInitialized;

	private const float k_MinimumThreshold = 0.1f;

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
					Time.deltaTime * m_ssoCamera.RotationSpeed
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

	public void Initialize(Transform aimingLookAt, Rigidbody rigidbody, Quaternion startRotation)
	{
		m_aimingLookAt = aimingLookAt;
		m_rigidbody = rigidbody;
		m_isInitialized = true;

		transform.localRotation = startRotation;
	}

	public void SpawnRagdoll(bool isCarryingLight)
	{
		gameObject.SetActive(false);

		var ragdoll = Instantiate(m_ssoCharacter.PfCharacterRagdoll);
		ragdoll.Initialize(m_ragdollTransforms, isCarryingLight);
	}
}
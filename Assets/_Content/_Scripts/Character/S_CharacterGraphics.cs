using UnityEngine;

public class CharacterGraphics : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private Rigidbody m_rigidbody;

	[Header("Scriptable references")]
	[SerializeField] private OldCharacterConfig m_characterConfig;
	[Space(5)]
	[SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[Space(5)]
	[SerializeField] private RSE_Move m_rseMove;

	private Transform m_aimingLookAt;
	private Vector2 m_moveInput;

	private const float k_MinimumThreshold = 0.1f;

	private void OnEnable()
	{
		m_rseMove.action += UpdateMoveInput;
	}

	private void OnDisable()
	{
		m_rseMove.action += UpdateMoveInput;
	}

	private void LateUpdate()
    {
		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			// Character is facing the movement direction
			// But not is the moveInput is null or equals to zero
			Vector3 moveDirection = transform.forward * m_moveInput.y + transform.right * m_moveInput.x;
			if (m_moveInput != Vector2.zero)
			{
				transform.forward = Vector3.Slerp(
					transform.forward,
					moveDirection.normalized,
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

	public void Initialize(Transform aimingLookAt)
	{
		m_aimingLookAt = aimingLookAt;
	}

	private void UpdateMoveInput(Vector2 input)
	{
		m_moveInput = input;
	}
}

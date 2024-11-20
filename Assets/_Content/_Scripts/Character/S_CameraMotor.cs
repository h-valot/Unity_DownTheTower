using Cinemachine;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
	[Header("External references")]
	[SerializeField] private CinemachineVirtualCamera m_aimingCamera;
	[SerializeField] private CinemachineVirtualCamera m_thirdPersonCamera;

	[Header("Scriptable references")]
	[SerializeField] private OldCharacterConfig m_characterConfig;
	[Space(5)]
	[SerializeField] private RSE_Look m_rseLook;
	[SerializeField] private RSE_Move m_rseMove;
	[Space(5)]
	[SerializeField] private RSO_ControlScheme m_rsoControlScheme;
	[SerializeField] private RSO_PlayerDeath m_rsoPlayerDeath;
	[SerializeField] private RSO_CameraStyle m_rsoCameraStyle;

	// - Private variables -
	private Vector2 m_lookInput;
	private float m_cinemachineTargetYaw;
	private float m_cinemachineTargetPitch;
	private Transform m_cameraTarget;

	// - Proprieties -
	public Vector3 PlanarForward { get; private set; }
	public Vector3 PlanarRight { get; private set; }

	private void LateUpdate()
	{
		HandleRotation();
		CalculatePlanarVectors();
	}

	private void OnEnable()
	{
		m_rseLook.action += UpdateLookInput;
		m_rsoCameraStyle.OnChanged += SwitchStyle;
		m_rsoPlayerDeath.OnChanged += HandleDeath;
	}

	private void OnDisable()
	{
		m_rseLook.action -= UpdateLookInput;
		m_rsoCameraStyle.OnChanged -= SwitchStyle;
		m_rsoPlayerDeath.OnChanged -= HandleDeath;
	}

	public void Initialize(Transform aimingLookAt, Transform cameraTarget)
	{
		m_cameraTarget = cameraTarget;
		m_aimingCamera.Follow = cameraTarget;
		m_aimingCamera.LookAt = aimingLookAt;
		m_thirdPersonCamera.Follow = cameraTarget;
		m_thirdPersonCamera.LookAt = cameraTarget;

		PlanarForward = new Vector3(transform.forward.x, 0, transform.forward.z);
		PlanarRight = new Vector3(transform.right.x, 0, transform.right.z);

		m_rsoCameraStyle.value = m_characterConfig.startingStyle;
		m_cinemachineTargetYaw = m_cameraTarget.rotation.eulerAngles.y;
	}

	private void HandleRotation()
	{
		// Clamp our rotations so our values are limited 360 degrees
		m_cinemachineTargetYaw = Matha.ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		m_cinemachineTargetPitch = Matha.ClampAngle(m_cinemachineTargetPitch, m_characterConfig.bottomClamp, m_characterConfig.topClamp);

		// Stops the camera if the character is dead
		if (m_rsoPlayerDeath.value) return;

		// Cinemachine will follow this target
		m_cameraTarget.rotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0.0f);
	}

	public void SwitchStyle()
	{
		m_aimingCamera.gameObject.SetActive(false);
		m_thirdPersonCamera.gameObject.SetActive(false);

		if (m_rsoCameraStyle.value == CameraStyle.BASIC) m_thirdPersonCamera.gameObject.SetActive(true);
		if (m_rsoCameraStyle.value == CameraStyle.AIMING) m_thirdPersonCamera.gameObject.SetActive(true);
	}

	public void CalculatePlanarVectors()
	{
		if (m_lookInput == Vector2.zero) return;

		PlanarForward = new Vector3(transform.forward.x, 0, transform.forward.z);
		PlanarRight = new Vector3(transform.right.x, 0, transform.right.z);
	}

	private void UpdateLookInput(Vector2 input)
	{
		m_lookInput = input;

		m_cinemachineTargetYaw += input.x;
		m_cinemachineTargetPitch += input.y;
    }

	private void HandleDeath()
	{
		if (!m_rsoPlayerDeath.value) return;

		// Set parent as scene root 
		m_cameraTarget.transform.parent = null;
	}
}

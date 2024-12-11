using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
	[Title("External references")]
	[SerializeField] private CinemachineVirtualCamera m_aimingCamera;
	[SerializeField] private CinemachineVirtualCamera m_thirdPersonCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Look m_rseLook;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_InitializeCamera m_rseInitializeCamera;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayFallDeath m_rsePlayFallDeath;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraForward m_rsoCameraForward;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraRight m_rsoCameraRight;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraTransform m_rsoCameraTransform;

	// - Private variables -
	private Vector2 m_lookInput;
	private float m_cinemachineTargetYaw;
	private float m_cinemachineTargetPitch;
	private Transform m_cameraTarget;

	private void OnEnable()
	{
		m_rseLook.action += UpdateLookInput;
		m_rsoCameraStyle.OnChanged += SwitchStyle;
		m_rsoCharacterDeath.OnChanged += HandleDeath;
		m_rseInitializeCamera.action += Initialize;
		m_rsePlayFallDeath.action += FreeCamera;
	}

	private void OnDisable()
	{
		m_rseLook.action -= UpdateLookInput;
		m_rsoCameraStyle.OnChanged -= SwitchStyle;
		m_rsoCharacterDeath.OnChanged -= HandleDeath;
		m_rseInitializeCamera.action -= Initialize;
		m_rsePlayFallDeath.action -= FreeCamera;
	}

	private void LateUpdate()
	{
		HandleRotation();
		CalculatePlanarVectors();

		m_rsoCameraTransform.value = transform;
	}

	public void Initialize(Transform aimingLookAt, Transform cameraTarget, Quaternion startRotation)
	{
		m_cameraTarget = cameraTarget;
		m_aimingCamera.Follow = cameraTarget;
		m_aimingCamera.LookAt = aimingLookAt;
		m_thirdPersonCamera.Follow = cameraTarget;
		m_thirdPersonCamera.LookAt = cameraTarget;

		m_rsoCameraForward.value = new Vector3(transform.forward.x, 0, transform.forward.z);
		m_rsoCameraRight.value = new Vector3(transform.right.x, 0, transform.right.z);

		m_rsoCameraStyle.value = m_ssoCharacter.StartingStyle;
		m_cinemachineTargetYaw = startRotation.eulerAngles.y;
		HandleRotation();
	}

	private void HandleRotation()
	{
		// Clamp our rotations so our values are limited 360 degrees
		m_cinemachineTargetYaw = Matha.ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		m_cinemachineTargetPitch = Matha.ClampAngle(m_cinemachineTargetPitch, m_ssoCharacter.BottomClamp, m_ssoCharacter.TopClamp);

		// Stops the camera if the character is dead
		if (m_rsoCharacterDeath.value) return;

		// Cinemachine will follow this target
		m_cameraTarget.rotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0.0f);
	}

	public void SwitchStyle()
	{
		m_aimingCamera.gameObject.SetActive(false);
		m_thirdPersonCamera.gameObject.SetActive(false);

		if (m_rsoCameraStyle.value == CameraStyle.BASIC) m_thirdPersonCamera.gameObject.SetActive(true);
		if (m_rsoCameraStyle.value == CameraStyle.AIMING) m_aimingCamera.gameObject.SetActive(true);
	}

	public void CalculatePlanarVectors()
	{
		if (m_lookInput == Vector2.zero) return;

		m_rsoCameraForward.value = new Vector3(transform.forward.x, 0, transform.forward.z);
		m_rsoCameraRight.value = new Vector3(transform.right.x, 0, transform.right.z);
	}

	private void UpdateLookInput(Vector2 input)
	{
		m_lookInput = input;

		// Multiplying by fixedDeltaTime. Otherwise, look sensibility is frame based.
		m_cinemachineTargetYaw += input.x * Time.fixedDeltaTime;
		m_cinemachineTargetPitch += input.y * Time.fixedDeltaTime;
    }

	private void HandleDeath()
	{
		if (!m_rsoCharacterDeath.value) return;

		FreeCamera();
	}

	/// <summary>
	/// Set parent as scene root.
	/// </summary>
	private void FreeCamera()
	{
		m_cameraTarget.transform.parent = null;
	}
}

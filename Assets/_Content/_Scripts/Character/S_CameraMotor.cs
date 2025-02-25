using Cinemachine;
using Mono.Cecil.Cil;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
	[Title("External references")]
	[SerializeField] private CinemachineVirtualCamera m_aimingCamera;
	[SerializeField] private CinemachineVirtualCamera m_thirdPersonCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Camera m_ssoCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Look m_rseLook;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_InitializeCamera m_rseInitializeCamera;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayDeath m_rsePlayFallDeath;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraForward m_rsoCameraForward;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraRight m_rsoCameraRight;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraTransform m_rsoCameraTransform;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;

	// - Private variables -
	private Vector2 m_lookInput;
	private float m_cinemachineTargetYaw;
	public float m_cinemachineTargetPitch;
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
		// Assertion
		if (m_rsoInputsLocked.value) return;

		HandleRotation();
		CalculatePlanarVectors();
		HandleSuspended();
		HandleLocomotion();
		m_rsoCameraTransform.value = transform;
	}

	public void Initialize(Transform aimingLookAt, Transform cameraTarget, Quaternion startRotation)
	{
		m_cameraTarget = cameraTarget;
		m_defaultTargetLocalPosition = cameraTarget.localPosition;
		m_aimingCamera.Follow = cameraTarget;
		m_aimingCamera.LookAt = aimingLookAt;
		m_thirdPersonCamera.Follow = cameraTarget;
		m_thirdPersonCamera.LookAt = cameraTarget;

		m_targetDistance = m_ssoCamera.DefaultDistance;
		m_3rdPersonFollow = m_thirdPersonCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as Cinemachine3rdPersonFollow;

		m_rsoCameraForward.value = new Vector3(transform.forward.x, 0, transform.forward.z);
		m_rsoCameraRight.value = new Vector3(transform.right.x, 0, transform.right.z);

		m_rsoCameraStyle.value = m_ssoCamera.StartingStyle;
		m_cinemachineTargetYaw = startRotation.eulerAngles.y;
		HandleRotation();
	}

	private void HandleRotation()
	{
		// Clamp our rotations so our values are limited 360 degrees
		m_cinemachineTargetYaw = Matha.ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		m_cinemachineTargetPitch = Matha.ClampAngle(m_cinemachineTargetPitch, m_ssoCamera.BottomClamp, m_ssoCamera.TopClamp);

		// Stops the camera if the character is dead
		if (m_rsoCharacterDeath.value) return;

		// Cinemachine will follow this target
		m_cameraTarget.rotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0.0f);
	}

	private Cinemachine3rdPersonFollow m_3rdPersonFollow;

	private float m_targetDistance;
	private Vector3 m_targetPosition;

	private Vector3 m_cameraTargetSmoothVelocity;
	private Vector3 m_defaultTargetLocalPosition;

	private void HandleSuspended()
	{
		// Choose targets 
		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			if (m_rsoCharacterState.value == BehaviorState.ROPE)
			{
				m_targetDistance = m_ssoCamera.SuspendedDistance;
				m_targetPosition = m_defaultTargetLocalPosition + Vector3.down * m_ssoCamera.SuspendedTargetLocalOffset;
			}
			else
			{
				m_targetDistance = m_ssoCamera.DefaultDistance;
				m_targetPosition = m_defaultTargetLocalPosition;
			}
		}

		// Lerp towards target distance
		m_3rdPersonFollow.CameraDistance = Mathf.Lerp(
			m_3rdPersonFollow.CameraDistance, 
			m_targetDistance, 
			Time.deltaTime * m_ssoCamera.DistanceTransition
		);

		// Lerp the camera look at target towards target position
		m_cameraTarget.localPosition = Vector3.SmoothDamp(
			m_cameraTarget.localPosition,
			m_targetPosition,
			ref m_cameraTargetSmoothVelocity,
			Time.deltaTime * m_ssoCamera.TargetTransition
		);
	}

	private float m_defaultShoulderOffsetZ = 0.0f;
	private float m_targetShoulderOffsetZ;
	private float m_locomotionShoulderOffsetZ = 1f;
	private float m_thresholdShouldOffsetZ = 55f;

	private float m_defaultCameraSide = 0.5f;
	private float m_targetCameraSide;
	private float m_locomotionCameraSide = 0.5f;
	private float m_thresholdCameraSide = -10f;

	public void HandleLocomotion()
	{
		// Choose targets 
		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			m_targetShoulderOffsetZ = m_defaultShoulderOffsetZ;
			m_targetCameraSide = m_defaultCameraSide;

			// Override those targets
			if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION)
			{
				// Camera is looking downwards ---- 70 = down
				if (m_cinemachineTargetPitch >= m_thresholdShouldOffsetZ)
				{
					m_targetShoulderOffsetZ = m_locomotionShoulderOffsetZ;
				}

				// Camera is looking upwards ---- -60 = up
				if (m_cinemachineTargetPitch <= m_thresholdCameraSide)
				{
					m_targetCameraSide = m_defaultCameraSide + m_locomotionCameraSide * Matha.Remap(m_thresholdCameraSide, m_ssoCamera.BottomClamp, 0f, 1f, m_cinemachineTargetPitch);
				}
			}
		}

		// Lerp towards target shoulder offset z
		m_3rdPersonFollow.ShoulderOffset.z = Mathf.Lerp(
			m_3rdPersonFollow.ShoulderOffset.z,
			m_targetShoulderOffsetZ,
			Time.deltaTime * m_ssoCamera.DistanceTransition
		);

		// Lerp towards target camera side
		m_3rdPersonFollow.CameraSide = Mathf.Lerp(
			m_3rdPersonFollow.CameraSide,
			m_targetCameraSide,
			Time.deltaTime * 5
		);
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
		// Assertion
		if (m_rsoInputsLocked.value) return;
		
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

	private void FreeCamera()
	{
		m_cameraTarget.transform.parent = null;
	}
}

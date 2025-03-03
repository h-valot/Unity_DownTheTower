using System.Collections;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
	#region REFERENCES

	[Title("External references")]
	[SerializeField] private CinemachineVirtualCamera m_aimingCamera;
	[SerializeField] private CinemachineVirtualCamera m_3rdPersonCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Camera m_ssoCamera;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Look m_rseLook;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_InitializeCamera m_rseInitializeCamera;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayDeath m_rseDisplayDeath;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraForward m_rsoCameraForward;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraRight m_rsoCameraRight;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CameraTransform m_rsoCameraTransform;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

	#endregion

	#region VARIABLES

	private bool m_isInitialized;
	private bool m_isCameraFrozen;

	// - Movement -
	private Vector2 m_lookInput;
	private float m_cinemachineTargetYaw;
	private float m_cinemachineTargetPitch;
	private Transform m_lookAt;

	// - Custom effect value targets - 
	private Cinemachine3rdPersonFollow m_3rdPersonFollow;
	private Cinemachine3rdPersonFollow ThirdPersonFollow
	{
		get 
		{
			if (!m_3rdPersonFollow) m_3rdPersonFollow = m_3rdPersonCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as Cinemachine3rdPersonFollow;
			return m_3rdPersonFollow;
		}
	}


	// Look at 
	private float m_bufferLookAtLocalY;
	private float m_targetLookAtLocalY;
	private float m_defaultLookAtLocalY;

	// Camera Distance
	private float m_targetCameraDistance;

	// Shoulder Offset Z
	private float m_targetShoulderOffsetZ;
	private const float k_defaultShoulderOffsetZ = 0.0f;

	// Camera Side
	private float m_targetCameraSide;
	private const float k_defaultCameraSide = 0.5f;

	#endregion 
	
	#region MONOBEHAVIOR

	private void OnEnable()
	{
		m_rseLook.action += UpdateLookInput;
		m_rsoCameraStyle.OnChanged += SwitchStyle;
		m_rseInitializeCamera.action += Initialize;

		m_rsoCharacterDeath.OnChanged += OnCharacterSpawn;
		m_rseDisplayDeath.action += OnCharacterDie;
	}

	private void OnDisable()
	{
		m_rseLook.action -= UpdateLookInput;
		m_rsoCameraStyle.OnChanged -= SwitchStyle;
		m_rseInitializeCamera.action -= Initialize;

		m_rsoCharacterDeath.OnChanged -= OnCharacterSpawn;
		m_rseDisplayDeath.action -= OnCharacterDie;
	}

	private void LateUpdate()
	{
		// Assertion
		if (m_rsoInputsLocked.value) return;
		if (m_isCameraFrozen) return;

		HandleRotation();
		CalculatePlanarVectors();
		HandleSuspended();
		HandleLocomotion();

		m_rsoCameraTransform.value = transform;
	}

	#endregion 

	#region MOTOR	

	public void Initialize(Transform aimingLookAt, Transform cameraTarget, Quaternion startRotation)
	{
		m_lookAt = cameraTarget;
		m_defaultLookAtLocalY = cameraTarget.localPosition.y;
		m_aimingCamera.Follow = cameraTarget;
		m_aimingCamera.LookAt = aimingLookAt;
		m_3rdPersonCamera.Follow = cameraTarget;
		m_3rdPersonCamera.LookAt = cameraTarget;

		m_targetCameraDistance = m_ssoCamera.DefaultCameraDistance;
		ThirdPersonFollow.ShoulderOffset.y = m_ssoCamera.ShoulderOffsetY;

		m_rsoCameraForward.value = new Vector3(transform.forward.x, 0, transform.forward.z);
		m_rsoCameraRight.value = new Vector3(transform.right.x, 0, transform.right.z);

		m_rsoCameraStyle.value = m_ssoCamera.StartingStyle;
		m_cinemachineTargetYaw = startRotation.eulerAngles.y;
		HandleRotation();

		m_isInitialized = true;
	}

	private void HandleRotation()
	{
		// Clamp our rotations so our values are limited 360 degrees
		m_cinemachineTargetYaw = Matha.ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		m_cinemachineTargetPitch = Matha.ClampAngle(m_cinemachineTargetPitch, m_ssoCamera.BottomClamp, m_ssoCamera.TopClamp);

		// Stops the camera if the character is dead
		if (m_rsoCharacterDeath.value) return;

		// Cinemachine will follow this target
		m_lookAt.rotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0.0f);
	}

	private void HandleSuspended()
	{
		// Choose targets 
		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			m_targetCameraDistance = m_ssoCamera.DefaultCameraDistance;
			m_targetLookAtLocalY = m_defaultLookAtLocalY;

			if (m_rsoCharacterState.value == BehaviorState.ROPE)
			{
				m_targetCameraDistance = m_ssoCamera.SuspendedCameraDistance;
				m_targetLookAtLocalY = m_defaultLookAtLocalY + m_ssoCamera.SuspendedLookAtOffsetY;
			}
		}

		// Lerp towards target distance
		ThirdPersonFollow.CameraDistance = Mathf.Lerp(
			ThirdPersonFollow.CameraDistance, 
			m_targetCameraDistance, 
			Time.deltaTime * m_ssoCamera.TransitionCameraDistance
		);

		// Lerp the camera look at target towards target position
		m_bufferLookAtLocalY = Mathf.Lerp(
			m_bufferLookAtLocalY,
			m_targetLookAtLocalY,
			Time.deltaTime * m_ssoCamera.TransitionLookAt
		);
		m_lookAt.localPosition = new Vector3(m_lookAt.localPosition.x, m_bufferLookAtLocalY, m_lookAt.localPosition.z);
	}

	public void HandleLocomotion()
	{
		// Choose targets 
		if (m_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			m_targetShoulderOffsetZ = k_defaultShoulderOffsetZ;
			m_targetCameraSide = k_defaultCameraSide;

			// Override those targets
			if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION)
			{
				// Camera is looking downwards ---- 70 = down
				if (m_cinemachineTargetPitch >= m_ssoCamera.ThresholdShouldOffsetZ)
				{
					m_targetShoulderOffsetZ = m_ssoCamera.LocomotionShoulderOffsetZ;
				}

				// Camera is looking upwards ---- -60 = up
				if (m_cinemachineTargetPitch <= m_ssoCamera.ThresholdCameraSide)
				{
					m_targetCameraSide = 
						k_defaultCameraSide 
						+ m_ssoCamera.LocomotionCameraSide 
						* Matha.Remap(m_ssoCamera.ThresholdCameraSide, m_ssoCamera.BottomClamp, 0f, 1f, m_cinemachineTargetPitch);
				}
			}
		}

		// Lerp towards target shoulder offset z
		ThirdPersonFollow.ShoulderOffset.z = Mathf.Lerp(
			ThirdPersonFollow.ShoulderOffset.z,
			m_targetShoulderOffsetZ,
			Time.deltaTime * m_ssoCamera.TransitionShoulderOffsetZ
		);

		// Lerp towards target camera side
		ThirdPersonFollow.CameraSide = Mathf.Lerp(
			ThirdPersonFollow.CameraSide,
			m_targetCameraSide,
			Time.deltaTime * m_ssoCamera.TransitionCameraSide
		);
	}

	public void SwitchStyle()
	{
		m_aimingCamera.gameObject.SetActive(false);
		m_3rdPersonCamera.gameObject.SetActive(false);

		if (m_rsoCameraStyle.value == CameraStyle.BASIC) m_3rdPersonCamera.gameObject.SetActive(true);
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

	private void OnCharacterDie()
	{
		m_lookAt.transform.parent = null;
		m_isCameraFrozen = true;
	}

	private void OnCharacterSpawn()
	{
		// Assertions
		if (m_rsoCharacterDeath.value) return;
		if (!m_isInitialized) return;

		m_isCameraFrozen = false;
		StartCoroutine(SetZeroDampForSeconds(1f));
	}

	public IEnumerator SetZeroDampForSeconds(float duration)
	{
		m_3rdPersonFollow.DampingFromCollision = 0f;
		yield return new WaitForSeconds(duration);
		m_3rdPersonFollow.DampingFromCollision = m_ssoCamera.DampingFromCollision;
	}

	#endregion
}

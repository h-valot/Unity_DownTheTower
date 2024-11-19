using UnityEngine;

public class CharacterGraphics : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private Rigidbody _rigidbody;

	[Header("Scriptable references")]
	[SerializeField] private CharacterConfig _characterConfig;
	[Space(5)]
	[SerializeField] private RSO_CameraStyle _rsoCameraStyle;
	[Space(5)]
	[SerializeField] private RSE_Move _rseMove;

	private Transform _aimingLookAt;
	private Vector2 _moveInput;

	private const float k_MinimumThreshold = 0.1f;

	private void OnEnable()
	{
		_rseMove.action += UpdateMoveInput;
	}

	private void OnDisable()
	{
		_rseMove.action += UpdateMoveInput;
	}

	private void LateUpdate()
    {
		if (_rsoCameraStyle.value == CameraStyle.BASIC)
		{
			// Character is facing the movement direction
			// But not is the moveInput is null or equals to zero
			Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
			if (_moveInput != Vector2.zero)
			{
				transform.forward = Vector3.Slerp(
					transform.forward,
					moveDirection.normalized,
					Time.deltaTime * _characterConfig.rotationSpeed
				);
			}
		}
		else if (_rsoCameraStyle.value == CameraStyle.AIMING)
		{
			transform.forward = _aimingLookAt.position - new Vector3(
				transform.transform.position.x, 
				_aimingLookAt.position.y, 
				transform.transform.position.z
			);
		}
	}

	public void Initialize(Transform aimingLookAt)
	{
		_aimingLookAt = aimingLookAt;
	}

	private void UpdateMoveInput(Vector2 input)
	{
		_moveInput = input;
	}
}

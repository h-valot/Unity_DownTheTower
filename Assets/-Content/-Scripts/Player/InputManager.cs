using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class InputManager : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private PlayerInput _playerInput;

	[Header("External references")]
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;

	[Header("Debugging")]
	[ReadOnly] public Vector2 move;
	[ReadOnly] public Vector2 look;
	[ReadOnly] public bool sprint;

	private const float _CONTROL_SCHEME_CHECK_DELAY = 1f;
	private float _controlSchemeCheckTimer;

	private void Start()
	{
		// reset the sprint value
		sprint = false;
		_rseSprint.Call(false);
	}

	private void Update()
	{
		// check if the control scheme has changed every _CONTROL_SCHEME_CHECK_DELAY seconds
		_controlSchemeCheckTimer += Time.deltaTime;
		if (_controlSchemeCheckTimer >= _CONTROL_SCHEME_CHECK_DELAY)
		{
			_controlSchemeCheckTimer = 0;
			_rsoControlScheme.value = _playerInput.currentControlScheme;
		}
	}

	public void OnMove(InputValue value)
	{
		move = value.Get<Vector2>();
		_rseMove.Call(move);
	}

	public void OnLook(InputValue value)
	{
		look = value.Get<Vector2>();
		_rseLook.Call(look);
	}

	public void OnJump(InputValue value)
	{
		_rseJump.Call();
	}

	public void OnSprint(InputValue value)
	{
		sprint = value.isPressed;
		_rseSprint.Call(sprint);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		// set cursor state
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
	}
}
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class InputManager : MonoBehaviour
{
	[Header("Tweakables values")]
	public bool analogMovement;
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;

	[Header("Debugging")]
	[ReadOnly] public Vector2 move;
	[ReadOnly] public Vector2 look;
	[ReadOnly] public bool jump;
	[ReadOnly] public bool sprint;

	public void OnMove(InputValue value)
	{
		move = value.Get<Vector2>();
	}

	public void OnLook(InputValue value)
	{
		if (cursorInputForLook)
		{
			look = value.Get<Vector2>();
		}
	}

	public void OnJump(InputValue value)
	{
		jump = value.isPressed;
	}

	public void OnSprint(InputValue value)
	{
		sprint = value.isPressed;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		// set cursor state
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
	}
}
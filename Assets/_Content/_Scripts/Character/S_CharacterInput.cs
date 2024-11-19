using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private PlayerInput _playerInput;

	[Header("Scriptable references")]
	[SerializeField] private InputsConfig _inputsConfig;
	[SerializeField] private RSO_GamePaused _rsoGamePaused;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Run _rseRun;
	[SerializeField] private RSE_Interact _rseInteract;
	[SerializeField] private RSE_CancelAction _rseCancelAction;
    [SerializeField] private RSE_Craft _rseCraft;
	[SerializeField] private RSE_Throw _rseThrow;
    [SerializeField] private RSE_ToggleInHand _rseToggleInHand;
    [SerializeField] private RSE_Pause _rsePause;
    [SerializeField] private RSE_HideUI _rseHideUI;
    [SerializeField] private RSE_Recycle _rseRecycle;
	[SerializeField] private RSE_ToggleCursor _rseToggleCursor;
	[SerializeField] private RSE_Climb _rseClimb;

	private Vector2 _move;
	private Vector2 _look;
	private bool _run;
	private bool _throw;
	private bool _jump;
	private bool _climb;

	private void Start()
	{
		// Reset values
		_run = false;
		_rseRun.Call(false);

		_throw = false;
		_rseThrow.Call(false);

		_jump = false;
		_rseJump.Call(false);
	}

	private void Update()
	{
		if (_look != Vector2.zero) 
		{
			_rseLook.Call(_look);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		// Set cursor state
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
	}

	private void OnEnable()
	{
		_rseToggleCursor.action += OnEnableCursor;
	}

	private void OnDisable()
	{
		_rseToggleCursor.action -= OnEnableCursor;
	}

	public void OnMove(InputValue value)
	{
		_move = value.Get<Vector2>();
		_rseMove.Call(_move);
	}

	public void OnLook(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();

		if (_playerInput.currentControlScheme == "Gamepad")
		{
			_look = new Vector2(
				input.x * _inputsConfig.gamepadSensibilityX,
				input.y * _inputsConfig.gamepadSensibilityY
			);
		}
		else
		{
			_look = new Vector2(
				input.x * _inputsConfig.mouseSensibilityX,
				input.y * _inputsConfig.mouseSensibilityY * (_inputsConfig.InvertMouseY ? -1 : 1)
			);
		}

		if (_rsoGamePaused.value) _look = Vector2.zero;
		_rseLook.Call(_look);
    }

	public void OnJump(InputValue value)
	{
		_jump = value.isPressed;
		_rseJump.Call(_jump);
	}

	public void OnRun(InputValue value)
	{
		_run = value.isPressed;
		_rseRun.Call(_run);
	}

	public void OnThrow(InputValue value)
    {
		if (value.Get<float>() >= 0.05f)
		{
			if (!_throw)
			{
                _throw = true;
                _rseThrow.Call(_throw);
            }
		}
		else
		{
            if (_throw)
            {
                _throw = false;
                _rseThrow.Call(_throw);
            }
        }
	}

	public void OnToggleInHand()
	{
		_rseToggleInHand.Call();
	}

	public void OnInteract()
	{
		_rseInteract.Call();
	}

    public void OnCancelAction()
    {
        _rseCancelAction.Call();
    }

    public void OnCraftTorch(InputValue value)
    {
        _rseCraft.Call(CraftType.TORCH, value.isPressed);
    }

    public void OnCraftLadder(InputValue value)
    {
        _rseCraft.Call(CraftType.LADDER, value.isPressed);
    }

    public void OnCraftRope(InputValue value)
    {
        _rseCraft.Call(CraftType.ROPE, value.isPressed);
	}

    public void OnRecycle()
    {
        _rseRecycle.Call();
    }

	public void OnEnableCursor(bool value)
	{
		Cursor.visible = value;
		Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
	}

	public void OnClimb(InputValue value)
	{
		_climb = value.isPressed;
		_rseClimb.Call(_climb);
	}

	public void OnHideUI()
	{
		_rseHideUI.Call();
	}
}
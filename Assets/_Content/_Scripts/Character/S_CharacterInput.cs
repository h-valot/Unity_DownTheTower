using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInput : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private PlayerInput _playerInput;

	[Header("Scriptable references")]
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
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
	[SerializeField] private RSE_Holding _rseHolding;
    [SerializeField] private RSE_Pause _rsePause;
    [SerializeField] private RSE_HideUI _rseHideUI;
    [SerializeField] private RSE_Recycle _rseRecycle;
	[SerializeField] private RSE_ToggleCursor _rseToggleCursor;

	[Header("Debug")]
	public Vector2 _move;
	public Vector2 _look;
	public bool _run;
	public bool _throw;

	private void Start()
	{
		// Reset values
		_run = false;
		_rseRun.Call(false);

		_throw = false;
		_rseThrow.Call(false);
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

	private void UpdateControlScheme()
	{
		if (_look == Vector2.zero) return;

		_rsoControlScheme.value = _playerInput.currentControlScheme;
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
		if (_rsoGamePaused.value) _look = Vector2.zero;
        else _look = value.Get<Vector2>();

		UpdateControlScheme();
		_rseLook.Call(_look);
    }

	public void OnJump(InputValue value)
	{
		_rseJump.Call();
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

	public void OnHolding(InputValue input)
	{
		_rseHolding.Call(input.isPressed);
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

	public void OnHideUI()
	{
		_rseHideUI.Call();
	}
}
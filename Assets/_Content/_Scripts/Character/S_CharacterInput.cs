using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class CharacterInput : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private PlayerInput _playerInput;

	[Header("External references")]
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
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
    [SerializeField] private RSO_GamePaused _rsoGamePaused;

	[Header("Debugging")]
	[ReadOnly] public Vector2 _move;
	[ReadOnly] public Vector2 _look;
	[ReadOnly] public bool _run;
	[ReadOnly] public bool _throw;

	private float _controlSchemeCheckTimer;

    private const float _CONTROL_SCHEME_CHECK_DELAY = 1f;

	private void Start()
	{
		// reset the sprint value
		_run = false;
		_rseRun.Call(false);

		_throw = false;
		_rseThrow.Call(false);
	}

	private void Update()
	{
		UpdateControlScheme();

		if (_look != Vector2.zero) 
		{
			_rseLook.Call(_look);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		// set cursor state
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
	}

	private void UpdateControlScheme()
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
		_move = value.Get<Vector2>();
		_rseMove.Call(_move);
	}

	public void OnLook(InputValue value)
	{
		if (_rsoGamePaused.value) _look = Vector2.zero;
        else _look = value.Get<Vector2>();

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
        _rseCraft.Call(CharacterMotor.CraftType.Torch, value.isPressed);
    }

    public void OnCraftLadder(InputValue value)
    {
        _rseCraft.Call(CharacterMotor.CraftType.Ladder, value.isPressed);
    }

    public void OnCraftRope(InputValue value)
    {
        _rseCraft.Call(CharacterMotor.CraftType.Rope, value.isPressed);
	}

	public void OnHolding(InputValue input)
	{
		_rseHolding.Call(input.isPressed);
	}

    public void OnRecycle()
    {
        _rseRecycle.Call();
    }
}
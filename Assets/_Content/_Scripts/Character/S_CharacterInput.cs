using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class CharacterInput : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private PlayerInput _playerInput;

	[Header("External references")]
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Jump _rseJump;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSO_ControlScheme _rsoControlScheme;
	[SerializeField] private RSE_Throw _rseThrow;
    [SerializeField] private RSE_ToggleLight _rseToggleLight;
    [SerializeField] private RSE_CraftTorch _rseCraftTorch;
	[SerializeField] private RSE_Interact _rseInteract;
	[SerializeField] private RSE_CancelAction _rseCancelAction;
    [SerializeField] private RSE_CraftLadder _rseCraftLadder;
    [SerializeField] private RSE_CraftRope _rseCraftRope;

    [Header("Debugging")]
	[ReadOnly] public Vector2 move;
	[ReadOnly] public Vector2 look;
	[ReadOnly] public bool sprint;

	private float _controlSchemeCheckTimer;

    private const float _CONTROL_SCHEME_CHECK_DELAY = 1f;

	private void Start()
	{
		// reset the sprint value
		sprint = false;
		_rseSprint.Call(false);
	}

	private void Update()
	{
		UpdateControlScheme();
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

	public void OnThrow(InputValue value)
	{
		_rseThrow.Call();
	}

	public void OnToggleLight()
	{
		_rseToggleLight.Call();
	}

	public void OnCraftTorch()
	{
		_rseCraftTorch.Call();
	}

	public void OnInteract()
	{
		_rseInteract.Call();
	}

    public void OnCancelAction()
    {
        _rseCancelAction.Call();
    }

    public void OnCraftLadder()
    {
        _rseCraftLadder.Call();
    }

    public void OnCraftRope()
    {
        _rseCraftRope.Call();
    }
}
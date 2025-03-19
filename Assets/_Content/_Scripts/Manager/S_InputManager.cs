using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	#region REFERENCES

	[FoldoutGroup("Internal references")][SerializeField] private PlayerInput m_playerInput;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Inputs m_ssoInputs;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Move m_rseMove;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Look m_rseLook;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Jump m_rseJump;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Run m_rseRun;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Interact m_rseInteract;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Craft m_rseCraft;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ThrowRope m_rseThrowRope;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_ThrowTorch m_rseThrowTorch;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleHandObject m_rseToggleHandObject;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Recycle m_rseRecycle;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Climb m_rseClimb;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_SwitchTabLeft m_rseSwitchTabLeft;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_SwitchTabRight m_rseSwitchTabRight;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_StartAction m_rseStartAction;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_Return m_rseReturn;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_GameEnd m_rseGameEnd;

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CancelConsumable m_rsoCancelConsumable;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdviceDisplayed m_rsoInputAdviceDisplayed;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_GameStarted m_rsoGameStarted;

    #endregion

    #region VARIABLES

    private Vector2 m_move;
	private Vector2 m_look;
	private bool m_run;
	private bool m_throwRope;
    private bool m_throwTorch;
	private bool m_jump;
	private bool m_climb;
	private bool m_isCursorEnabled;
	private bool m_interact;
	private bool m_pause;
	private bool m_gameEnded;

    #endregion

    #region MONOBEHAVIOR

    private void Awake()
	{
		// Reset values
		m_rsoCraftInputLocked.value = false;
		m_rsoRecycleInputLocked.value = false;
		m_gameEnded = false;
        m_rseLook.Call(Vector2.zero);
		m_rsoCurrentScheme.value = InputScheme.PAUSE;

    }

	private void Start()
	{
		// Reset values
		m_run = false;
		m_rseRun.Call(false);

		m_throwRope = false;
		m_rseThrowRope.Call(false);

        m_throwTorch = false;
        m_rseThrowTorch.Call(false);

        m_jump = false;
		m_rseJump.Call(false);

		m_interact = false;
		m_rseInteract.Call(false);
	}

	private void Update()
	{
		if (m_look != Vector2.zero) 
		{
			m_rseLook.Call(m_look);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		Cursor.lockState = hasFocus && !m_isCursorEnabled 
			? CursorLockMode.Locked 
			: CursorLockMode.None;
	}

	private void OnEnable()
	{
		m_rseToggleCursor.action += OnEnableCursor;
		m_playerInput.onControlsChanged += OnControlsChanged;
		m_rsoCurrentScheme.OnChanged += UpdateScheme;
		m_rseGameEnd.action += EndGame;
    }

	private void OnDisable()
	{
		m_rseToggleCursor.action -= OnEnableCursor;
        m_rsoCurrentScheme.OnChanged -= UpdateScheme;
        m_rseGameEnd.action -= EndGame;
    }

	# endregion

	#region ACTION LISTENER - PLAYER

	public void OnMove(InputValue value)
	{
		m_move = value.Get<Vector2>();
		m_rseMove.Call(m_move);
	}

	public void OnLook(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();

		switch (m_rsoCurrentControls.value)
		{
			case ControlType.GAMEPAD:
                m_look = new Vector2(
                    input.x * m_ssoInputs.SensitivityValue,
                    input.y * m_ssoInputs.SensitivityValue * m_ssoInputs.SensitivityMultiplierY * (m_ssoInputs.InvertAxisY ? 1 : -1)
                    );
				break;
			case ControlType.KEYBOARDMOUSE:
                m_look = new Vector2(
                    input.x * m_ssoInputs.SensitivityValue * m_ssoInputs.SensitivityMouseMultiplier,
                    input.y * m_ssoInputs.SensitivityValue * m_ssoInputs.SensitivityMultiplierY * m_ssoInputs.SensitivityMouseMultiplier * (m_ssoInputs.InvertAxisY ? 1 : -1)
                    );
                break;
        }


        if (m_rsoPause.value) m_look = Vector2.zero;
    }

	public void OnJump(InputValue value)
	{
		m_jump = value.isPressed;
		m_rseJump.Call(m_jump);
	}

	public void OnRun(InputValue value)
	{
		m_run = value.isPressed;
		m_rseRun.Call(m_run);
	}

	public void OnThrowRope(InputValue value)
    {
		if (value.Get<float>() >= 0.05f)
		{
			if (!m_throwRope)
			{
				m_throwRope = true;
				m_rseThrowRope.Call(m_throwRope);
			}
		}
		else
		{
			if (m_throwRope)
			{
				m_throwRope = false;
				m_rseThrowRope.Call(m_throwRope);
			}
		}
	}

    public void OnThrowTorch(InputValue value)
    {
        if (value.Get<float>() >= 0.05f)
        {
            if (!m_throwTorch)
            {
                m_throwTorch = true;
                m_rseThrowTorch.Call(m_throwTorch);
            }
        }
        else
        {
            if (m_throwTorch)
            {
                m_throwTorch = false;
                m_rseThrowTorch.Call(m_throwTorch);
            }
        }
    }

    public void OnToggleHandObject(InputValue value)
	{
        if (!value.isPressed) return;

        m_rseToggleHandObject.Call(value.isPressed);
	}

	public void OnInteract(InputValue value)
	{
		m_interact = value.isPressed;
		m_rseInteract.Call(m_interact);
	}

    public void OnCancel(InputValue value)
    {
		if (!value.isPressed) return;

		// Consume cancel input
		m_rsoCancelConsumable.value = true;
		m_rseCancel.Call(value.isPressed);
    }

    public void OnCraftTorch(InputValue value)
    {
		// Assertion
		if (!m_rsoCraftInputLocked.value) return;

        m_rseCraft.Call(CraftType.TORCH, value.isPressed);
    }

    public void OnCraftRope(InputValue value)
	{
		// Assertion
		if (!m_rsoCraftInputLocked.value) return;

		m_rseCraft.Call(CraftType.ROPE, value.isPressed);
	}

    public void OnRecycle(InputValue value)
	{
		// Assertion
		if (!m_rsoRecycleInputLocked.value) return;

		m_rseRecycle.Call();
    }

	public void OnEnableCursor(bool value)
	{
		m_isCursorEnabled = value;
		Cursor.visible = value;
		Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
	}

	public void OnClimb(InputValue value)
	{
		m_climb = value.isPressed;
		m_rseClimb.Call(m_climb);
	}

	public void OnHideUI(InputValue value)
	{
		m_rsoInputAdviceDisplayed.value = !m_rsoInputAdviceDisplayed.value;
	}

    public void OnPause()
	{
		if(!m_gameEnded) m_rsoPause.value = !m_rsoPause.value;

    }

	public void OnControlsChanged(PlayerInput newInput)
	{
		if (newInput.currentControlScheme.Equals("Gamepad")) m_rsoCurrentControls.value = ControlType.GAMEPAD;
		else if (newInput.currentControlScheme.Equals("KeyboardMouse")) m_rsoCurrentControls.value = ControlType.KEYBOARDMOUSE;
	}

	public void UpdateScheme()
	{
		switch (m_rsoCurrentScheme.value)
		{
			case InputScheme.GAME:
				m_playerInput.SwitchCurrentActionMap("Player");
				break;
			case InputScheme.PAUSE:
                m_playerInput.SwitchCurrentActionMap("Pause");
                break;
		}
	}

    #endregion

    #region ACTION LISTENER - PAUSE

    public void OnStartAction()
    {
        m_rseStartAction.Call();

        if (m_rsoGameStarted.value) OnPause();
    }

    public void OnSwitchTabLeft()
    {
        m_rseSwitchTabLeft.Call();
    }

    public void OnSwitchTabRight()
    {
        m_rseSwitchTabRight.Call();
    }

	public void OnReturn(InputValue value)
	{
        if (!value.isPressed) return;

        // Consume cancel input
        m_rsoCancelConsumable.value = true;
        m_rseReturn.Call(value.isPressed);
    }

	private void EndGame()
	{
		m_gameEnded = true;
	}

    #endregion
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	#region REFERENCES

	[Header("References")]
	[SerializeField] private PlayerInput m_playerInput;

	[FoldoutGroup("SSO")][SerializeField] private SSO_Inputs m_ssoInputs;

	[FoldoutGroup("RSE")][SerializeField] private RSE_Move m_rseMove;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Look m_rseLook;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Jump m_rseJump;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Run m_rseRun;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Interact m_rseInteract;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Craft m_rseCraft;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Throw m_rseThrow;
	[FoldoutGroup("RSE")][SerializeField] private RSE_ToggleInHand m_rseToggleInHand;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("RSE")][SerializeField] private RSE_HideUI m_rseHideUI;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Recycle m_rseRecycle;
	[FoldoutGroup("RSE")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
	[FoldoutGroup("RSE")][SerializeField] private RSE_Climb m_rseClimb;

	[FoldoutGroup("RSO")][SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[FoldoutGroup("RSO")][SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;

	#endregion

	#region VARIABLES

	private Vector2 m_move;
	private Vector2 m_look;
	private bool m_run;
	private bool m_throw;
	private bool m_jump;
	private bool m_climb;

	#endregion

	#region MONOBEHAVIOR

	private void Awake()
	{
		// Reset values
		m_rsoCraftInputLocked.value = false;
		m_rsoRecycleInputLocked.value = false;
	}

	private void Start()
	{
		// Reset values
		m_run = false;
		m_rseRun.Call(false);

		m_throw = false;
		m_rseThrow.Call(false);

		m_jump = false;
		m_rseJump.Call(false);
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
		// Set cursor state
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
	}

	private void OnEnable()
	{
		m_rseToggleCursor.action += OnEnableCursor;
	}

	private void OnDisable()
	{
		m_rseToggleCursor.action -= OnEnableCursor;
	}

	# endregion

	#region ACTION LISTENER

	public void OnMove(InputValue value)
	{
		m_move = value.Get<Vector2>();
		m_rseMove.Call(m_move);
	}

	public void OnLook(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();

		if (m_playerInput.currentControlScheme == "Gamepad")
		{
			m_look = new Vector2(
				input.x * m_ssoInputs.gamepadSensibilityX,
				input.y * m_ssoInputs.gamepadSensibilityY
			);
		}
		else
		{
			m_look = new Vector2(
				input.x * m_ssoInputs.mouseSensibilityX,
				input.y * m_ssoInputs.mouseSensibilityY * (m_ssoInputs.InvertMouseY ? -1 : 1)
			);
		}

		if (m_rsoGamePaused.value) m_look = Vector2.zero;
		m_rseLook.Call(m_look);
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

	public void OnThrow(InputValue value)
    {
		if (value.Get<float>() >= 0.05f)
		{
			m_throw = !m_throw;
			m_rseThrow.Call(m_throw);
		}
		else
		{
			m_throw = !m_throw;
			m_rseThrow.Call(m_throw);
        }
	}

	public void OnToggleInHand()
	{
		m_rseToggleInHand.Call();
	}

	public void OnInteract(InputValue value)
	{
		m_rseInteract.Call();
	}

    public void OnCancel(InputValue value)
    {
        m_rseCancel.Call(value.isPressed);
    }

    public void OnCraftTorch(InputValue value)
    {
		// Assertion
		if (!m_rsoCraftInputLocked.value) return;

        m_rseCraft.Call(CraftType.TORCH, value.isPressed);
    }

    public void OnCraftLadder(InputValue value)
	{
		// Assertion
		if (!m_rsoCraftInputLocked.value) return;

		m_rseCraft.Call(CraftType.DEPRECATED_LADDER, value.isPressed);
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
		Cursor.visible = value;
		Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
	}

	public void OnClimb(InputValue value)
	{
		m_climb = value.isPressed;
		m_rseClimb.Call(m_climb);
	}

	public void OnHideUI()
	{
		m_rseHideUI.Call();
	}

	#endregion
}
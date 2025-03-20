using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISettings : UIWindow
{
    [FoldoutGroup("Tweakable values")][SerializeField] protected bool m_isMainMenu;

    [FoldoutGroup("Internal references")][SerializeField] private UITabManager m_tabManager;
	[FoldoutGroup("Internal references")][SerializeField] private UISlider m_valueSensitivity;
	[FoldoutGroup("Internal references")][SerializeField] private UIToggleable m_toggleInvertCameraY;
    [ShowIf("m_isMainMenu")][FoldoutGroup("Internal references")][SerializeField] private S_MenuManager m_mainMenu;

    [FoldoutGroup("Scriptable")][SerializeField] protected RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Inputs m_ssoInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;



    protected override void OnEnable()
	{
		base.OnEnable();
		m_rsoPause.OnChanged += OnPaused;
    }

	protected override void OnDisable()
	{
		base.OnDisable();
		m_rsoPause.OnChanged -= OnPaused;
    }

	public override void Show()
	{
		base.Show();
        m_tabManager.OpenCurrentTab();
		InitializeSettings();
	}

    public override void Hide()
    {
        m_tabManager.CloseCurrentTab();
        base.Hide();

    }

    public override void Return(bool isPressed)
    {
        // Assertions
        if (!m_toggleReturn) return;
        if (!IsActive) return;
        if (m_rsoCancelPriority.value != m_requiredState) return;
        if (!m_rsoCancelConsumable.value) return;

        m_rsoCancelConsumable.value = false;
        Hide();
        if (m_previousUISelect != null && m_rsoCurrentControls.value == ControlType.GAMEPAD) EventSystem.current.SetSelectedGameObject(m_previousUISelect);
        m_rsoCancelPriority.value = m_previousState;

        if (m_isMainMenu) m_mainMenu.ShowMenu();
    }

    private void OnPaused()
	{
        // Assertion
        if (!m_rsoGameStarted.value) return;
        if (m_rsoPause.value) return;

		Hide();
	}

	private void InitializeSettings()
	{
        m_valueSensitivity.Initialize(m_ssoInputs.MinSensitivity, m_ssoInputs.MaxSensitivity, m_ssoInputs.SensitivityValue);
		m_toggleInvertCameraY.Initialize(m_ssoInputs.InvertAxisY);

    }

	public void UpdateSensitivity() => m_ssoInputs.SensitivityValue = m_valueSensitivity.Value;
    public void UpdateCameraAxisY() => m_ssoInputs.InvertAxisY = m_toggleInvertCameraY.Value;

}
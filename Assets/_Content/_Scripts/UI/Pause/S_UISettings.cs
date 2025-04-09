using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class UISettings : UIWindow
{
    [FoldoutGroup("Tweakable values")][SerializeField] protected bool m_isMainMenu;

    [ShowIf("m_isMainMenu")][FoldoutGroup("MainMenu")][SerializeField] private S_MenuManager m_mainMenu;

    // INPUT 
    [FoldoutGroup("Inputs")][SerializeField] private UITabManager m_tabManager;
	[FoldoutGroup("Inputs")][SerializeField] private UISlider m_valueSensitivity;
	[FoldoutGroup("Inputs")][SerializeField] private UIToggleable m_toggleInvertCameraY;

    // SOUND
    [FoldoutGroup("Sound")][SerializeField] private UISlider m_masterVolumeSlider;
    [FoldoutGroup("Sound")][SerializeField] private UISlider m_musicVolumeSlider;
    [FoldoutGroup("Sound")][SerializeField] private UISlider m_sfxVolumeSlider;

    // VIDEO
    [FoldoutGroup("Inputs")][SerializeField] private UIToggleable m_toggleVolumetricFog;

    [Title("External references")]
    [SerializeField] private AudioMixer m_audioMixer;
    [FoldoutGroup("Scriptable")][SerializeField] protected RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Settings m_ssoSettings;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_ChangeVideoSetting m_rseChangeVideoSetting;

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
        m_valueSensitivity.Initialize(m_ssoSettings.MinSensitivity, m_ssoSettings.MaxSensitivity, m_ssoSettings.SensitivityValue);
		m_toggleInvertCameraY.Initialize(m_ssoSettings.InvertAxisY);
        m_masterVolumeSlider.Initialize(m_ssoSettings.MinVolumeDB, m_ssoSettings.MaxVolumeDB, m_ssoSettings.MasterVolumeDB);
        m_musicVolumeSlider.Initialize(m_ssoSettings.MinVolumeDB, m_ssoSettings.MaxVolumeDB, m_ssoSettings.MusicVolumeDB);
        m_sfxVolumeSlider.Initialize(m_ssoSettings.MinVolumeDB, m_ssoSettings.MaxVolumeDB, m_ssoSettings.SfxVolumeDB);
        m_toggleVolumetricFog.Initialize(m_ssoSettings.VolumetricFog);
    }

	public void UpdateSensitivity() => m_ssoSettings.SensitivityValue = m_valueSensitivity.Value;
    public void UpdateCameraAxisY() => m_ssoSettings.InvertAxisY = m_toggleInvertCameraY.Value;



    public void UpdateMasterVolume(AudioMixerGroup group)
    {
        m_audioMixer.SetFloat(group.name, m_masterVolumeSlider.Value);
        m_ssoSettings.MasterVolumeDB = m_masterVolumeSlider.Value;
    }
    public void UpdateMusicVolume(AudioMixerGroup group)
    {
        m_audioMixer.SetFloat(group.name, m_musicVolumeSlider.Value);
        m_ssoSettings.MusicVolumeDB = m_musicVolumeSlider.Value;
    }
    public void UpdateSfxVolume(AudioMixerGroup group)
    {
        m_audioMixer.SetFloat(group.name, m_sfxVolumeSlider.Value);
        m_ssoSettings.SfxVolumeDB = m_sfxVolumeSlider.Value;
    }

    public void ToggleVolumetricFog()
    {
        m_ssoSettings.VolumetricFog = m_toggleVolumetricFog.Value;
        m_rseChangeVideoSetting.Call();
    }

}
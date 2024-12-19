using Sirenix.OdinInspector;
using UnityEngine;

public class UISettings : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private UITab m_openingTab;
	[FoldoutGroup("Internal references")][SerializeField] private UIValue m_valueMouseSensibilityX;
	[FoldoutGroup("Internal references")][SerializeField] private UIValue m_valueMouseSensibilityY;
	[FoldoutGroup("Internal references")][SerializeField] private UIValue m_valueGamepadSensibilityX;
	[FoldoutGroup("Internal references")][SerializeField] private UIValue m_valueGamepadSensibilityY;
	[FoldoutGroup("Internal references")][SerializeField] private UIToggleable m_toggleableInvertMouseY;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Inputs m_ssoInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	private void OnEnable()
	{
		m_rsoGamePaused.OnChanged += HideOnPaused;
	}

	private void OnDisable()
	{
		m_rsoGamePaused.OnChanged -= HideOnPaused;
	}

	public override void Show()
	{
		base.Show();
		m_openingTab.Highlight();
		InitializeSettings();
	}

	private void HideOnPaused()
	{
		// Assertion
		if (m_rsoGamePaused.value) return;

		Hide();
	}

	private void InitializeSettings()
	{
		m_valueMouseSensibilityX.Initialize(m_ssoInputs.MouseSensibilityX);
		m_valueMouseSensibilityY.Initialize(m_ssoInputs.MouseSensibilityY);
		m_toggleableInvertMouseY.Initialize(m_ssoInputs.InvertMouseY);
		m_valueGamepadSensibilityX.Initialize(m_ssoInputs.GamepadSensibilityX);
		m_valueGamepadSensibilityY.Initialize(m_ssoInputs.GamepadSensibilityY);
	}

	public void UpdateMouseSensibilityX() => m_ssoInputs.MouseSensibilityX = m_valueMouseSensibilityX.Value;
	public void UpdateMouseSensibilityY() => m_ssoInputs.MouseSensibilityY = m_valueMouseSensibilityY.Value;
	public void InvertMouseY() => m_ssoInputs.InvertMouseY = m_toggleableInvertMouseY.Value;
	public void UpdateGamepadSensibilityX() => m_ssoInputs.GamepadSensibilityX = m_valueGamepadSensibilityX.Value;
	public void UpdateGamepadSensibilityY() => m_ssoInputs.GamepadSensibilityY = m_valueGamepadSensibilityY.Value;
}
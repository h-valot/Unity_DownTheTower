using Sirenix.OdinInspector;
using UnityEngine;

public class RuntimeValueModifier : UIWindow
{
	[FoldoutGroup("Static variables")][SerializeField] private UIToggleable m_toggleableTorchLightRange;
	[FoldoutGroup("Static variables")][SerializeField] private UIToggleable m_toggleableGlobalLightIntensity;
	[FoldoutGroup("Static variables")][SerializeField] private UIToggleable m_toggleableCharacterRunSpeed;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	private float m_cachedTorchLightRange;
	private float m_cachedGlobalLightIntensity;
	private float m_cachedCharacterRunSpeed;

	private void OnEnable()
	{
		m_cachedTorchLightRange = m_ssoTorch.LightRange;
		m_cachedGlobalLightIntensity = m_ssoGame.GlobalLightIntensity;
		m_cachedCharacterRunSpeed = m_ssoCharacter.RunSpeed;

		m_toggleableTorchLightRange.Initialize(false);
		m_toggleableGlobalLightIntensity.Initialize(false);
		m_toggleableCharacterRunSpeed.Initialize(false);

		m_toggleableTorchLightRange.SetFlavor($"(A) Base : {m_cachedTorchLightRange} ---- (B) Alternate : {m_ssoGame.NewTorchLightRange}");
		m_toggleableGlobalLightIntensity.SetFlavor($"(A) Base : {m_cachedGlobalLightIntensity} ---- (B) Alternate : {m_ssoGame.NewGlobalLightIntensity}");
		m_toggleableCharacterRunSpeed.SetFlavor($"(A) Base : {m_cachedCharacterRunSpeed} ---- (B) Alternate : {m_ssoGame.NewCharacterRunSpeed}");
	}

	public void OnDisable()
	{
		m_ssoTorch.LightRange = m_cachedTorchLightRange;
		m_ssoGame.GlobalLightIntensity = m_cachedGlobalLightIntensity;
		m_ssoCharacter.RunSpeed = m_cachedCharacterRunSpeed;
	}

	private void ToggleFloatValue(ref float value, float newValue, float cachedValue)
	{
		value = value == cachedValue ? newValue : cachedValue;
	}

	public void ToggleTorchLightRange() => ToggleFloatValue(ref m_ssoTorch.LightRange, m_ssoGame.NewTorchLightRange, m_cachedTorchLightRange);
	public void ToggleGlobalLightIntensity() => ToggleFloatValue(ref m_ssoGame.GlobalLightIntensity, m_ssoGame.NewGlobalLightIntensity, m_cachedGlobalLightIntensity);
	public void ToggleCharacterRunSpeed() => ToggleFloatValue(ref m_ssoCharacter.RunSpeed, m_ssoGame.NewCharacterRunSpeed, m_cachedCharacterRunSpeed);
	
}	
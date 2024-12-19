using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UIDeath : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private Image m_imgDeath;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayFallDeath m_rsePlayFallDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	public override void Start()
	{
		base.Start();
		ResetFadeIn();
	}

	private void OnEnable()
	{
		m_rsePlayFallDeath.action += FadeIn;
		m_rsoCharacterDeath.OnChanged += OnCharacterDies;
	}

	private void OnDisable()
	{
		m_rsePlayFallDeath.action -= FadeIn;
		m_rsoCharacterDeath.OnChanged -= OnCharacterDies;
	}

	private void OnCharacterDies()
	{
		// Assertion
		if (!m_rsoCharacterDeath.value) return;

		ResetFadeIn();
	}

	private void FadeIn()
	{
		base.Show();
		m_imgDeath
			.DOFade(1f, m_ssoCharacter.FallDeathDurationBeforeRespawn)
			.SetEase(Ease.InQuad)
			.OnComplete(ResetFadeIn);
	}

	private void ResetFadeIn()
	{
		base.Hide();
		m_imgDeath.DOFade(0f, 0f);
	}
}
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDeath : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private Image m_imgDeath;
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_tmpText;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_DisplayDeath m_rseDisplayDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	public override void Start()
	{
		base.Start();
		FadeIn();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rseDisplayDeath.action += FadeOut;
		m_rsoCharacterDeath.OnChanged += OnCharacterDies;
	}

	protected override void OnDisable()
	{
		base.OnEnable();
		m_rseDisplayDeath.action -= FadeOut;
		m_rsoCharacterDeath.OnChanged -= OnCharacterDies;
	}

	private void OnCharacterDies()
	{
		// Assertion
		if (!m_rsoCharacterDeath.value) return;

		FadeIn();
	}

	private void FadeOut(bool isTextDisplayed)
	{
		base.Show();
		m_tmpText.SetActive(isTextDisplayed);
		m_imgDeath
			.DOFade(1f, m_ssoCharacter.FallDeathDurationBeforeRespawn)
			.SetEase(Ease.InQuad)
			.OnComplete(FadeIn);
	}

	private void FadeIn()
	{
		base.Hide();
		m_imgDeath.DOFade(0f, 0f);
	}
}
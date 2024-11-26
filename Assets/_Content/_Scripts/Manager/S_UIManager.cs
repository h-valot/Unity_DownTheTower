using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Internal references")]
    [SerializeField] private GameObject m_pausePanel;
    [SerializeField] private Image m_imgDeathPanel;

    [Header("Scriptable references")]
    [SerializeField] private RSE_Pause m_rsePause;
    [SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[Space(5)]
    [SerializeField] private RSO_GamePaused m_rsoGamePaused;
    [SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

    private void OnEnable()
    {
        m_rsePause.action += TogglePause;
        m_rsoCharacterDeath.OnChanged += DeathFade;
    }

    private void OnDisable()
    {
        m_rsePause.action -= TogglePause;
        m_rsoCharacterDeath.OnChanged -= DeathFade;
    }

    private void TogglePause()
    {
        m_rsoGamePaused.value = !m_rsoGamePaused.value;
        if (m_rsoGamePaused.value)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        m_rseToggleInputs.Call();
        m_pausePanel.SetActive(m_rsoGamePaused.value);
    }

    private void DeathFade()
    {
        if (!m_rsoCharacterDeath.value) return;

        m_imgDeathPanel.gameObject.SetActive(true);

        DOTweenModuleUI.DOFade(m_imgDeathPanel, 0, 5)
                       .SetEase(Ease.InExpo)
                       .OnComplete(ResetDeathFade);
    }

    private void ResetDeathFade()
    {
        m_imgDeathPanel.gameObject.SetActive(false);
        m_imgDeathPanel.color = Color.black;
    }
}
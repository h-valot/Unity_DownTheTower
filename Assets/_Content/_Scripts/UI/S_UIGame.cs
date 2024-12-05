using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [Title("Internal references")]
    [SerializeField] private Image m_imgDeath;
    [SerializeField] private GameObject m_pnlPause;
    [SerializeField] private GameObject m_pnlLog;
    [SerializeField] private TextMeshProUGUI m_tmpLogHeader;
    [SerializeField] private TextMeshProUGUI m_tmpLogBody;
	[SerializeField] private TextMeshProUGUI m_tmpVersion;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Pause m_rsePause;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleInputs m_rseToggleInputs;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_LogContent m_rseLogContent;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

    private bool m_isLogShowing = false;

	private void Start()
	{
		Hide();
	}

    private void OnEnable()
    {
        m_rsePause.action += TogglePausePanel;
        m_rseLogContent.action += ShowLog;
        m_rseCancel.action += HideLog;
        m_rsoCharacterDeath.OnChanged += DeathFade;
    }

    private void OnDisable()
    {
        m_rsePause.action -= TogglePausePanel;
        m_rseLogContent.action -= ShowLog;
        m_rseCancel.action -= HideLog;
        m_rsoCharacterDeath.OnChanged -= DeathFade;
    }

    private void TogglePausePanel()
	{
		m_tmpVersion.text = $"version: {m_ssoGame.Version} {m_ssoGame.BuildType.ToString().ToLower()}";

		if (m_pnlPause.activeInHierarchy)
		{
			Hide();
		}
		else 
		{
			Show();
		}
	}

	private void Show()
	{
		if (m_isLogShowing)
		{
			HideLog();
		}

		m_pnlPause.SetActive(true);
		TogglePauseGame(true);
		m_rseToggleCursor.Call(true);
	}

	public void Hide()
	{
		m_pnlPause.SetActive(false);
		TogglePauseGame(false);
		m_rseToggleCursor.Call(false);
	}

	public void Exit()
	{
		Application.Quit();
	}

	private void TogglePauseGame(bool isPaused)
    {
        m_rsoGamePaused.value = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
		
        m_rseToggleInputs.Call();
    }

    private void DeathFade()
    {
		// Assertion
        if (!m_rsoCharacterDeath.value) return;

        m_imgDeath.gameObject.SetActive(true);

        DOTweenModuleUI.DOFade(m_imgDeath, 0, 5)
                       .SetEase(Ease.InExpo)
                       .OnComplete(ResetDeathFade);
    }

    private void ResetDeathFade()
    {
        m_imgDeath.gameObject.SetActive(false);
        m_imgDeath.color = Color.black;
    }

    private void ShowLog(string title, List<string> body)
    {
        m_tmpLogHeader.text = title;
        m_tmpLogBody.text = BuildLogBody(body);

        TogglePauseGame(true);

        m_isLogShowing = true;
        m_pnlLog.SetActive(m_isLogShowing);
    }

    private void HideLog(bool isHide = true)
    {
        if (!m_isLogShowing) return;

        TogglePauseGame(false);

        m_isLogShowing = false;
        m_pnlLog.SetActive(m_isLogShowing);
    }

    private string BuildLogBody(List<string> bodyList)
    {
        string body = string.Empty;
        foreach (var paragraph in bodyList)
        {
            body = body + paragraph + "<br>";
        }
        return body;
    }
}
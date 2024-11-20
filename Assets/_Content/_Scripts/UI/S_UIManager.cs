using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Image _deathPanelIMG;
    [SerializeField] private GameObject _logPanel;
    [SerializeField] private TextMeshProUGUI _logHeader;
    [SerializeField] private TextMeshProUGUI _logBody;

    [Header("External References")]
    [SerializeField] private RSE_Pause _rsePause;
    [SerializeField] private RSE_CancelAction _rseCancelAction;
    [SerializeField] private RSE_ToggleInputs _rseToggleInputs;
    [SerializeField] private RSE_LogContent _rseLogContent;
    [SerializeField] private RSO_GamePaused _rsoGamePaused;
    [SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

    // --- PRIVATE VARIABLES ---
    private bool isLogShowing = false;

    private void OnEnable()
    {
        _rsePause.action += TogglePausePanel;
        _rseLogContent.action += ShowLog;
        _rseCancelAction.action += HideLog;
        _rsoPlayerDeath.OnChanged += DeathFade;
    }

    private void OnDisable()
    {
        _rsePause.action -= TogglePausePanel;
        _rseLogContent.action -= ShowLog;
        _rseCancelAction.action -= HideLog;
        _rsoPlayerDeath.OnChanged -= DeathFade;
    }

    private void TogglePausePanel()
    {
        if (isLogShowing) 
        {
            HideLog();
        }

        TogglePauseGame(!_rsoGamePaused.value);
        _pausePanel.SetActive(_rsoGamePaused.value);
    }

    private void TogglePauseGame(bool isPaused)
    {
        _rsoGamePaused.value = isPaused;
        if (_rsoGamePaused.value)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        _rseToggleInputs.Call();
    }

    private void DeathFade()
    {
        if (!_rsoPlayerDeath.value) return;

        _deathPanelIMG.gameObject.SetActive(true);

        DOTweenModuleUI.DOFade(_deathPanelIMG, 0, 5)
                       .SetEase(Ease.InExpo)
                       .OnComplete(ResetDeathFade);
    }

    private void ResetDeathFade()
    {
        _deathPanelIMG.gameObject.SetActive(false);
        _deathPanelIMG.color = Color.black;
    }

    private void ShowLog(string title, List<string> body)
    {
        _logHeader.text = title;
        _logBody.text = BuildLogBody(body);


        TogglePauseGame(true);

        isLogShowing = true;
        _logPanel.gameObject.SetActive(isLogShowing);
    }

    private void HideLog()
    {
        if (!isLogShowing) return;

        TogglePauseGame(false);

        isLogShowing = false;
        _logPanel.gameObject.SetActive(isLogShowing);
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

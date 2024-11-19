using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Image _deathPanelIMG;

    [Header("External References")]
    [SerializeField] private RSE_Pause _rsePause;
    [SerializeField] private RSE_ToggleInputs _rseToggleInputs;
    [SerializeField] private RSO_GamePaused _rsoGamePaused;
    [SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

    private void OnEnable()
    {
        _rsePause.action += TogglePause;
        _rsoPlayerDeath.OnChanged += DeathFade;
    }

    private void OnDisable()
    {
        _rsePause.action -= TogglePause;
        _rsoPlayerDeath.OnChanged -= DeathFade;
    }

    private void TogglePause()
    {
        _rsoGamePaused.value = !_rsoGamePaused.value;
        if (_rsoGamePaused.value)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        _rseToggleInputs.Call();
        _pausePanel.SetActive(_rsoGamePaused.value);
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
}

using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEnd : MonoBehaviour
{

    [FoldoutGroup("Tweakable Values")][SerializeField] private float m_fadeDuration;

    [FoldoutGroup("Internal references")][SerializeField] private CanvasGroup m_pnlEnd;
    [FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_txtTime;
    [FoldoutGroup("Internal references")][SerializeField] private Button m_btnQuit;

    [FoldoutGroup("External references")][SerializeField] private TextMeshProUGUI m_txtChrono;
    [FoldoutGroup("External references")][SerializeField] private SSO_Game m_ssoGame;
    [FoldoutGroup("External references")][SerializeField] private RSE_GameEnd m_rseGameEnd;
    [FoldoutGroup("External references")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;
    [FoldoutGroup("External references")][SerializeField] private RSE_RestartChrono m_rseRestartChrono;

    private void Awake()
    {
        m_pnlEnd.gameObject.SetActive(false);
        m_pnlEnd.DOFade(0, 0);
    }

    private void OnEnable()
    {
        m_rseGameEnd.action += EndGame;
    }

    private void OnDisable()
    {
        m_rseGameEnd.action -= EndGame;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        m_rseRestartChrono.Call();
        SceneManager.LoadScene("LVL_MainScene");
    }

    private void EndGame()
    {
        if(m_txtChrono != null || !m_ssoGame.EnableChrono) m_txtTime.text = "Final Time - " + m_txtChrono.text;
        else m_txtTime.gameObject.SetActive(false);
        StartCoroutine(EndGameTransition());
    }

    private IEnumerator EndGameTransition()
    {
        m_pnlEnd.gameObject.SetActive(true);
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, m_fadeDuration).SetUpdate(true);
        yield return m_pnlEnd.DOFade(1, m_fadeDuration).SetUpdate(true).SetEase(Ease.OutCubic).WaitForCompletion();

        m_rsoCurrentScheme.value = InputScheme.PAUSE;
        ControlsChanged();
        m_rsoCurrentControls.OnChanged += ControlsChanged;
    }

    private void ControlsChanged()
    {
        if (m_rsoCurrentControls.value == ControlType.GAMEPAD)
        {
            m_rseToggleCursor.Call(false);
            EventSystem.current.SetSelectedGameObject(m_btnQuit.gameObject);
        }
        else if (m_rsoCurrentControls.value == ControlType.KEYBOARDMOUSE)
        {
            EventSystem.current.SetSelectedGameObject(null);
            m_rseToggleCursor.Call(true);
        }
    }

}

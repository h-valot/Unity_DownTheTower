using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private GameObject _pausePanel;

    [Header("External References")]
    [SerializeField] private RSE_Pause _rsePause;
    [SerializeField] private RSO_GamePaused _rsoGamePaused;

    private void OnEnable()
    {
        _rsePause.action += TogglePause;
    }

    private void OnDisable()
    {
        _rsePause.action -= TogglePause;
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
        _pausePanel.SetActive(_rsoGamePaused.value);
    }
}

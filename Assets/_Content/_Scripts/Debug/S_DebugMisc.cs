using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class S_DebugMisc : MonoBehaviour
{
    [Header("External Variables")]
    [SerializeField] private TorchConfig _torchConfig;

    [Header("Internal Variables")]
    [SerializeField] private GameObject _graphicsParent;
    [SerializeField] private TextMeshProUGUI _torchButtonText;

    // ----- PRIVATE VARIABLES -----
    private bool _isPressed;
    private bool _isEnabled;

    private void Start()
    {
        Hide();
        UpdateTorchText();

    }

    private void Update()
    {
        HandleShortcut();

        // debug
        // if (Input.GetKey(KeyCode.Space)) Debug.Log($"GAME_START: debug");
    }

    private void HandleShortcut()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (!_isPressed)
            {
                Toggle();
            }
            _isPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.F3))
        {
            _isPressed = false;
        }
    }

    private void Toggle()
    {
        if (_isEnabled)
        {
            Hide();
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Show();
            Time.timeScale = 0.001f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Hide()
    {
        _graphicsParent.SetActive(false);
        _isEnabled = false;
    }

    private void Show()
    {
        _graphicsParent.SetActive(true);
        _isEnabled = true;
    }

    public void ToggleTorchAnim()
    {
        _torchConfig.activateBreakAnim = !_torchConfig.activateBreakAnim;
        UpdateTorchText();
    }

    private void UpdateTorchText()
    {
        if (_torchConfig.activateBreakAnim) _torchButtonText.SetText("Torch Break Anim: ON");
        else _torchButtonText.SetText("Torch Break Anim: OFF");
    }
}

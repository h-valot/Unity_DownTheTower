using TMPro;
using UnityEngine;

public class DebugMisc : MonoBehaviour
{
    [Header("External Variables")]
    [SerializeField] private TorchConfig _torchConfig;
	[SerializeField] private RSE_ToggleCursor _rseToggleCursor;

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
        }
        else
        {
            Show();
            Time.timeScale = 0.001f;
        }
    }

	public void Hide()
	{
		_rseToggleCursor.Call(false);
		_graphicsParent.SetActive(false);
        _isEnabled = false;
    }

    private void Show()
	{
		_rseToggleCursor.Call(true);
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
		_torchButtonText.text = _torchConfig.activateBreakAnim ? "Torch Break Anim: ON" : "Torch Break Anim: OFF";
	}
}
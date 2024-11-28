using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DebugMisc : MonoBehaviour
{
	[Title("Internal references")]
    [SerializeField] private GameObject m_graphicsParent;
    [SerializeField] private TextMeshProUGUI m_tmpTorch;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssotorch;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

    // ----- PRIVATE VARIABLES -----
    private bool m_isPressed;
    private bool m_isEnabled;

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
            if (!m_isPressed)
            {
                Toggle();
            }
            m_isPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.F3))
        {
            m_isPressed = false;
        }
    }

    private void Toggle()
    {
        if (m_isEnabled)
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
		m_rseToggleCursor.Call(false);
		m_graphicsParent.SetActive(false);
        m_isEnabled = false;
    }

    private void Show()
	{
		m_rseToggleCursor.Call(true);
		m_graphicsParent.SetActive(true);
        m_isEnabled = true;
    }

    public void ToggleTorchAnim()
    {
        m_ssotorch.ActivateBreakAnim = !m_ssotorch.ActivateBreakAnim;
        UpdateTorchText();
    }

    private void UpdateTorchText()
    {
		m_tmpTorch.text = m_ssotorch.ActivateBreakAnim ? "Torch Break Anim: ON" : "Torch Break Anim: OFF";
	}
}
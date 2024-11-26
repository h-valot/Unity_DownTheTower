using System.Text;
using TMPro;
using UnityEngine;

public class Console : MonoBehaviour	
{
	[Header("Tweakable values")]
	[SerializeField] private string m_colorCodeBase = "F0F0F0";
	[SerializeField] private string m_colorCodeWarning = "FFC107";
	[SerializeField] private string m_colorCodeError = "FF534A";

	[Header("Internal references")]
	[SerializeField] private GameObject m_graphicsParent;
	[SerializeField] private TextMeshProUGUI m_output;

	[Header("External references")]
	[SerializeField] private SSO_Game m_ssoGame;
	[SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

	private StringBuilder m_logBuilder;
	private bool m_isPressed;
	private bool m_isEnabled;

	private void Start()
	{
		m_logBuilder = new StringBuilder();
		Hide();
	}

	private void Update()
	{
		HandleShortcut();
	}

	public void Log(string input)
	{
		// slow-down framerate by overload a string 
		if (!m_ssoGame.enableConsoleLogging) return;
	
		var sentence = new Sentence(input, m_colorCodeBase);
		m_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		m_output.text = m_logBuilder.ToString();
	}

	public void LogWarning(string input)
	{
		// slow-down framerate by overload a string 
		if (!m_ssoGame.enableConsoleLogging) return;

		var sentence = new Sentence(input, m_colorCodeWarning);
		m_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		m_output.text = m_logBuilder.ToString();
	}

	public void LogError(string input)
	{
		// slow-down framerate by overload a string 
		if (!m_ssoGame.enableConsoleLogging) return;

		var sentence = new Sentence(input, m_colorCodeError);
		m_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		m_output.text = m_logBuilder.ToString();
	}

	private void HandleShortcut()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			if (!m_isPressed)
			{
				Toggle();
			}
			m_isPressed = true;
		}

		if (Input.GetKeyUp(KeyCode.F1))
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
}
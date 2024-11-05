using System.Text;
using TMPro;
using UnityEngine;

public class Console : MonoBehaviour	
{
	[Header("Tweakable values")]
	[SerializeField] private string _colorCodeBase = "F0F0F0";
	[SerializeField] private string _colorCodeWarning = "FFC107";
	[SerializeField] private string _colorCodeError = "FF534A";

	[Header("Internal references")]
	[SerializeField] private GameObject _graphicsParent;
	[SerializeField] private TextMeshProUGUI _output;

	[Header("External references")]
	[SerializeField] private GameConfig _gameConfig;
	[SerializeField] private RSE_ToggleCursor _rseToggleCursor;

	private StringBuilder _logBuilder;
	private bool _isPressed;
	private bool _isEnabled;

	private void Start()
	{
		_logBuilder = new StringBuilder();
		Hide();
	}

	public void Log(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;
	
		var sentence = new Sentence(input, _colorCodeBase);
		_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = _logBuilder.ToString();
	}

	public void LogWarning(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;

		var sentence = new Sentence(input, _colorCodeWarning);
		_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = _logBuilder.ToString();
	}

	public void LogError(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;

		var sentence = new Sentence(input, _colorCodeError);
		_logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = _logBuilder.ToString();
	}

	private void Update()
	{
		HandleShortcut();
	}

	private void HandleShortcut()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			if (!_isPressed)
			{
				Toggle();
			}
			_isPressed = true;
		}

		if (Input.GetKeyUp(KeyCode.F1))
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
}
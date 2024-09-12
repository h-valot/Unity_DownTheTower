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
	
	private StringBuilder logBuilder;
	private bool _isPressed;
	private bool _isEnabled;

	private void Start()
	{
		logBuilder = new StringBuilder();
		Hide();
	}

	public void Log(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;
	
		var sentence = new Sentence(input, _colorCodeBase);
		logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = logBuilder.ToString();
	}

	public void LogWarning(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;

		var sentence = new Sentence(input, _colorCodeWarning);
		logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = logBuilder.ToString();
	}

	public void LogError(string input)
	{
		// slow-down framerate by overload a string 
		if (!_gameConfig.enableConsoleLogging) return;

		var sentence = new Sentence(input, _colorCodeError);
		logBuilder.Append($"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}");
		_output.text = logBuilder.ToString();
	}

	private void Update()
	{
		HandleShortcut();

		// debug
		// if (Input.GetKey(KeyCode.Space)) Debug.Log($"GAME_START: debug");
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
}
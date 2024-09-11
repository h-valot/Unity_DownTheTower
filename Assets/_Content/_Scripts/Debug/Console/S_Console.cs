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

	private bool _isPressed;
	private bool _isEnabled;

	private void Start()
	{
		Hide();
	}

	public void Log(string input, Style style = Style.REGULAR)
	{
		var sentence = new Sentence(input, style, _colorCodeBase);
		_output.text += $"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}";
	}

	public void LogWarning(string input, Style style = Style.REGULAR)
	{
		var sentence = new Sentence(input, style, _colorCodeWarning);
		_output.text += $"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}";
	}

	public void LogError(string input, Style style = Style.REGULAR)
	{
		var sentence = new Sentence(input, style, _colorCodeError);
		_output.text += $"\n[{System.DateTime.UtcNow.ToString("HH:mm:ss")}] {sentence.GetStylizedSentence()}";
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

public enum Style
{
	REGULAR,
	BOLD,
	ITALIC
}
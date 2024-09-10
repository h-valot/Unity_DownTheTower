using UnityEngine;

public class SceneLoader : MonoBehaviour 
{
	[Header("Internal references")]
	[SerializeField] private GameObject _graphicsParent;

	private bool _isPressed;
	private bool _isEnabled;

	

	private void Update()
	{
		HandleShortcut();
	}

	private void HandleShortcut()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			if (!_isPressed)
			{
				Toggle();
			}
			_isPressed = true;
		}

		if (Input.GetKeyUp(KeyCode.F2))
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
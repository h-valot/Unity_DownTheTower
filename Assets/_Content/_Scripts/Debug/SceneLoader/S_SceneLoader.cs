using UnityEngine;

public class SceneLoader : MonoBehaviour 
{
	[Header("Internal references")]
	[SerializeField] private GameObject _graphicsParent;
	[SerializeField] private Transform _buttonsParent;

	[Header("External references")]
	[SerializeField] private SceneButton _pfSceneButton;
	[SerializeField] private RSE_ToggleCursor _rseToggleCursor;

	private bool _isPressed;
	private bool _isEnabled;
	private string[] _scenes;

	private void Start()
	{
		Hide();
	}

	private void GetAllScenes()
	{
		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

		if (sceneCount <= 0)
		{
			Debug.LogError($"SCENE_LOADER: No scenes in-built settings. Adds scenes to it.");
			return;
		}

		_scenes = new string[sceneCount];
		for (int i = 0; i < sceneCount; i++)
		{
			_scenes[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
		}
	}

	private void CreateButtons()
	{
		if (_scenes is null
		|| _scenes.Length <= 0)
		{
			GetAllScenes();
		}

		foreach (var scene in _scenes)
		{
			SceneButton newButton = Instantiate(_pfSceneButton, _buttonsParent);
			newButton.Initialize(scene);
		}
	}

	private void RemoveButtons()
	{
		foreach (Transform child in _buttonsParent)
		{
			Destroy(child.gameObject);
		}
	}

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

	public void Hide()
	{
		_rseToggleCursor.Call(false);
		_graphicsParent.SetActive(false);
		_isEnabled = false;
		RemoveButtons();
	}

	private void Show()
	{
		_rseToggleCursor.Call(true);
		_graphicsParent.SetActive(true);
		_isEnabled = true;
		CreateButtons();
	}
}
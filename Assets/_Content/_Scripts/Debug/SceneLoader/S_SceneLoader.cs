using Sirenix.OdinInspector;
using UnityEngine;

public class SceneLoader : MonoBehaviour 
{
	[Title("Internal references")]
	[SerializeField] private GameObject m_graphicsParent;
	[SerializeField] private Transform m_buttonsParent;

	[Title("External references")]
	[SerializeField] private SceneButton m_pfSceneButton;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

	private bool m_isPressed;
	private bool m_isEnabled;
	private string[] m_scenes;

	private void Start()
	{
		Hide();
	}

	private void Update()
	{
		HandleShortcut();
	}

	private void GetAllScenes()
	{
		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

		if (sceneCount <= 0)
		{
			Debug.LogError($"SCENE_LOADER: No scenes in-built settings. Adds scenes to it.");
			return;
		}

		m_scenes = new string[sceneCount];
		for (int i = 0; i < sceneCount; i++)
		{
			m_scenes[i] = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i));
		}
	}

	private void CreateButtons()
	{
		if (m_scenes is null
		|| m_scenes.Length <= 0)
		{
			GetAllScenes();
		}

		foreach (var scene in m_scenes)
		{
			SceneButton newButton = Instantiate(m_pfSceneButton, m_buttonsParent);
			newButton.Initialize(scene);
		}
	}

	private void RemoveButtons()
	{
		foreach (Transform child in m_buttonsParent)
		{
			Destroy(child.gameObject);
		}
	}

	private void HandleShortcut()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			if (!m_isPressed)
			{
				Toggle();
			}
			m_isPressed = true;
		}

		if (Input.GetKeyUp(KeyCode.F2))
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
		RemoveButtons();
	}

	private void Show()
	{
		// Assertion
		if (m_ssoGame.BuildType == BuildType.RELEASE) return;

		m_rseToggleCursor.Call(true);
		m_graphicsParent.SetActive(true);
		m_isEnabled = true;
		CreateButtons();
	}
}
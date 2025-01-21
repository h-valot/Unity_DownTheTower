using Sirenix.OdinInspector;
using UnityEngine;

public class SceneLoader : UIWindow
{
	[Title("Internal references")]
	[SerializeField] private Transform m_buttonsParent;

	[Title("External references")]
	[SerializeField] private SceneButton m_pfSceneButton;

	private string[] m_scenes;
	
	public override void Start()
	{
		base.Start();
		CreateButtons();
	}

	private void OnDestroy()
	{
		RemoveButtons();
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
}
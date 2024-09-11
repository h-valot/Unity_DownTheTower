using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private TextMeshProUGUI _tmpScene;

	private string _sceneName;

	public void Initialize(string newScene)
	{
		_sceneName = newScene;
		_tmpScene.text = _sceneName;
	}

	public void Press()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(_sceneName);
	}
}
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private TextMeshProUGUI m_tmpScene;

	private string m_sceneName;

	public void Initialize(string newScene)
	{
		m_sceneName = newScene;
		m_tmpScene.text = m_sceneName;
	}

	public void Press()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(m_sceneName);
	}
}
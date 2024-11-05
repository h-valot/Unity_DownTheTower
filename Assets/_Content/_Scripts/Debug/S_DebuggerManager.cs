using UnityEngine;
using UnityEngine.SceneManagement;

public class DebuggerManager : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private SceneLoader m_SceneLoader;
	[SerializeField] private DebugMisc m_DebugMisc;
	[SerializeField] private Console m_Console;

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += HideDebuggers;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= HideDebuggers;
	}

	private void HideDebuggers(Scene current, Scene former)
	{
		m_SceneLoader.Hide();
		m_DebugMisc.Hide();
		m_Console.Hide();
	}
}
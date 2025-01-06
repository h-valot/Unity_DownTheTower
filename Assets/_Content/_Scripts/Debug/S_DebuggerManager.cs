using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebuggerManager : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private SceneLoader m_sceneLoader;
	[SerializeField] private RuntimeValueModifier m_runtimeValueModifier;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	public bool IsActive => m_sceneLoader.IsActive || m_runtimeValueModifier.IsActive;

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += OnSceneChanged;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnSceneChanged;
	}

	private void Update()
	{
		// Assertion
		if (m_ssoGame.BuildType == BuildType.RELEASE) return;

		HandleShortcut();
	}

	private void HandleShortcut()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			if (m_sceneLoader.IsActive)
			{
				HideDebuggers();
			}
			else
			{
				HideDebuggers();
				m_sceneLoader.Show();
			}
			TogglePauseGame(IsActive);
		}

		if (Input.GetKeyDown(KeyCode.F3))
		{
			if (m_runtimeValueModifier.IsActive)
			{
				HideDebuggers();
			}
			else
			{
				HideDebuggers();
				m_runtimeValueModifier.Show();
			}
			TogglePauseGame(IsActive);
		}
	}

	private void TogglePauseGame(bool isPaused)
	{
		m_rsoGamePaused.value = isPaused;
	}

	private void OnSceneChanged(Scene current, Scene former)
	{
		HideDebuggers();
	}

	private void HideDebuggers()
	{
		m_sceneLoader.Hide();
		m_runtimeValueModifier.Hide();
	}
}
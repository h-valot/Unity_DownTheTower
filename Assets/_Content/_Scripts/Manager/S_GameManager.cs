using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Title("Tweakable values")]
	[Required("A Game Start must be assigned to start the game. If there is no in the scene, you can find the prefab here: Content/Prefabs/LevelDesign")]
	[SerializeField] private GameStart m_gameStart;

	[Required("There is only one directional light per level, you can find it under --LEVEL DESIGN--")]
	[SerializeField] private Light m_directionalLight;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Logs m_ssoLogs;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Ropes m_rsoRopes;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;


	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += HandleDeath;
		m_rsoGamePaused.OnChanged += Pause;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= HandleDeath;
		m_rsoGamePaused.OnChanged -= Pause;
	}

	private void Start()
	{
		if (!m_directionalLight)
		{
			Debug.LogError("GAME_MANAGER: Directional light reference is null. The global light intensity couldn't be setup correctly.");
		}
		else
		{
			m_directionalLight.intensity = m_ssoGame.GlobalLightIntensity;
		}

		Restart();
		DOTween.SetTweensCapacity(400, 400);
		m_rseToggleCursor.Call(false);

		// Reset runtime scriptable values
		m_rsoGamePaused.value = false;
		m_rsoRopes.value = new List<Rope>();

		if (m_ssoGame.ResetData)
		{
			foreach (var log in m_ssoLogs.Logs)
			{
				log.IsDiscovered = false;
			}
		}
	}

	/// <summary>
	/// Can be used to start or restart a run.
	///	Reload the game level without resetting custom items placement.
	/// </summary>
	private void Restart()
	{
		m_gameStart.SpawnCharacter();
	}

	/// <summary>
	/// Reload the game level by resetting all data (custom items placement).
	/// </summary>
	private void Reset()
	{
		Restart();
		// TODO - Handle game data reset
	}

	private void HandleDeath()
	{
		if (!m_rsoCharacterDeath.value) return;

        Restart();
	}

	private void Pause()
	{
		Time.timeScale = m_rsoGamePaused.value ? 0f : 1f;
	}
}
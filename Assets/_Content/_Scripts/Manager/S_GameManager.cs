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

	[FoldoutGroup("External references")][SerializeField] private CameraMotor m_cameraMotor;

	[FoldoutGroup("SSO")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("SSO")][SerializeField] private SSO_Logs m_ssoLogs;

	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("RSO")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("RSO")][SerializeField] private RSO_Pause m_rsoPause;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CancelPriority m_rsoCancelPriority;
	[FoldoutGroup("RSO")][SerializeField] private RSO_Ropes m_rsoRopes;
	[FoldoutGroup("RSO")][SerializeField] private RSO_LastCheckpointReached m_rsoLastCheckpointReached;
	[FoldoutGroup("RSO")][SerializeField] private RSO_CurrentTutoIndex m_rsoCurrentTutoIndex;

	[FoldoutGroup("RSE")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;


	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += HandleDeath;
		m_rsoPause.OnChanged += Pause;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= HandleDeath;
		m_rsoPause.OnChanged -= Pause;
	}

	private void Awake()
	{
		ResetRSO();
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
		m_rsoCurrentTutoIndex.value = -1;
		DOTween.SetTweensCapacity(400, 400);
		StartCoroutine(m_cameraMotor.SetZeroDampForSeconds(1f));
	}

	/// <summary>
	/// Can be used to start or restart a run.
	///	Reload the game level without resetting custom items placement.
	/// </summary>
	private void Restart()
	{
		if (m_ssoGame.UseCheckpoints
		&& m_rsoLastCheckpointReached.value)
		{
			m_rsoLastCheckpointReached.value.SpawnCharacter();
		}
		else
		{
			m_gameStart.SpawnCharacter();
		}

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
		Time.timeScale = m_rsoPause.value ? 0f : 1f;
		m_rsoInputsLocked.value = m_rsoPause.value;
	}

	private void ResetRSO()
	{
		// World
		m_rsoLastCheckpointReached.value = null;

		// Character
		m_rsoCharacterDeath.value = false;

		// Input
		m_rsoInputsLocked.value = false;
		m_rsoPause.value = false;
		m_rsoCancelPriority.value = CancelState.IN_GAME;

		// Permanent
		m_rsoRopes.value = new List<Rope>();

		// Data
		if (m_ssoGame.ResetData)
		{
			foreach (var log in m_ssoLogs.Logs)
			{
				log.IsDiscovered = false;
			}
		}
	}
}
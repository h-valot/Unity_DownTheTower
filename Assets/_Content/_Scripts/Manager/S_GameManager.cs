using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("Tweakable values")]
	[Required("A Game Start must be assigned to start the game. If there is no in the scene, you can find the prefab here: Content/Prefabs/LevelDesign")]
	[SerializeField] private GameStart _gameStart;

	[Header("Scriptables references")]
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSO_GamePaused _rsoGamePaused;

	private void Start()
	{
		Restart();
		Cursor.lockState = CursorLockMode.Locked;
		_rsoGamePaused.value = false;
    }

	/// <summary>
	/// 	Can be used to start or restart a run.
	///		Reload the game level without resetting custom items placement.
	/// </summary>
	private void Restart()
	{
		_gameStart.SpawnCharacter();
	}

	/// <summary>
	/// 	Reload the game level by resetting all data (custom items placement).
	/// </summary>
	private void Reset()
	{
		Restart();
		// [ ] Handle game data reset
	}

	private void HandleDeath()
	{
		if (!_rsoPlayerDeath.value) return;

        Restart();
	}

	private void OnEnable()
	{
		_rsoPlayerDeath.OnChanged += HandleDeath;
	}

	private void OnDisable()
	{
		_rsoPlayerDeath.OnChanged -= HandleDeath;
	}
}
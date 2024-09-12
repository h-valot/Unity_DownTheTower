using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("Tweakable values")]
	[Required("A Game Start must be assigned to start the game. If there is no in the scene, you can find the prefab here: Content/Prefabs/LevelDesign")]
	[SerializeField] private GameStart _gameStart;

	[Header("External references")]
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

	private void Start()
	{
		Restart();
        Cursor.lockState = CursorLockMode.Locked;
    }

	private void Restart()
	{
		// can be used to start or restart a run
		// reload the game level without resetting custom items placement

		_gameStart.SpawnCharacter();
	}

	private void Reset()
	{
		// reload the game level by resetting all data (custom items placement)

		Restart();
		// TODO - handle game data reset
	}

	private void HandleDeath()
	{
		if (!_rsoPlayerDeath.value) return;

		Restart();
		// TODO - fade in into ui to quit or restart
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
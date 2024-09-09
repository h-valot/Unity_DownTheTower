using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("Tweakable values")]
	[Required("A Game Start must be assigned to start the game. If there is no in the scene, you can find the prefab here: Content/Prefabs/LevelDesign")]
	[SerializeField] private GameStart _gameStart;
	
	private void Start()
	{
		Restart();
	}

	private void Restart()
	{
		// can be used to start or restart a run
		// reload the game level without resetting custom items placement

		_gameStart.SpawnPlayer();
	}

	private void Reset()
	{
		// reload the game level by resetting all data (custom items placement)

		Restart();
		// TODO - handle game data reset
	}
}
using UnityEngine;

public class GameStart : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private GameObject _pfPlayer;

	[Header("External references")]
	[SerializeField] private Transform _levelDesigneSpan;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

	private GameObject _currentCharacter;

	public void RemoveFormerCharacter()
	{
		// destroy the former character if exists
		if (_currentCharacter is null) return;
		Destroy(_currentCharacter);
	}

	public void SpawnCharacter()
	{
		RemoveFormerCharacter();

		// reset player related rso values
		_rsoPlayerDeath.value = false;

		// instantiate the prefab of the player
		_currentCharacter = Instantiate(_pfPlayer, transform.position, transform.rotation, _levelDesigneSpan);

		Debug.Log($"GAME_START: player instantiated");
	}

	public void OnDrawGizmos()
	{
		// display the game start gizmos in editor
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, 1f);
		Gizmos.DrawLine(transform.position, 1.5f * transform.forward.normalized + transform.position);
	}
}
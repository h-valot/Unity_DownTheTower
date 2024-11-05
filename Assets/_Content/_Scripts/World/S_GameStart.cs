using UnityEngine;

public class GameStart : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private GameObject _pfPlayer;

	[Header("Scriptable references")]
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

	[Header("External references")]
	[SerializeField] private Transform _levelDesigneSpan;

	private GameObject _currentCharacter;

	/// <summary>
	/// 	Destroy the former character if exists.
	/// </summary>
	public void RemoveFormerCharacter()
	{
		if (_currentCharacter is null) return;

		Destroy(_currentCharacter);
	}

	public void SpawnCharacter()
	{
		RemoveFormerCharacter();

		// Reset player related rso values
		_rsoPlayerDeath.value = false;

		// Instantiate the prefab of the player
		_currentCharacter = Instantiate(_pfPlayer, transform.position, transform.rotation, _levelDesigneSpan);

		Debug.Log($"GAME_START: Player instantiated.");
	}

#if UNITY_EDITOR
	public void OnDrawGizmos()
	{
		// display the game start gizmos in editor
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, 1f);
		Gizmos.DrawLine(transform.position, 1.5f * transform.forward.normalized + transform.position);
	}
#endif
}
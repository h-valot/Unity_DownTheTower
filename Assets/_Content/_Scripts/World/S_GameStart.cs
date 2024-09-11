using UnityEngine;

public class GameStart : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private GameObject _pfPlayer;

	[Header("External references")]
	[SerializeField] private Transform _levelDesigneSpan;

	public void SpawnPlayer()
	{
		// instantiate the prefab of the player
		Instantiate(_pfPlayer, transform.position, transform.rotation, _levelDesigneSpan);
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
using UnityEngine;

public class GameStart : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private GameObject m_pfCharacter;

	[Header("Scriptable references")]
	[SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	private GameObject m_currentCharacter;

	/// <summary>
	/// 	Destroy the former character if exists.
	/// </summary>
	public void RemoveFormerCharacter()
	{
		if (!m_currentCharacter) return;

		Destroy(m_currentCharacter);
	}

	public void SpawnCharacter()
	{
		RemoveFormerCharacter();

		// Reset player related rso values
		m_rsoCharacterDeath.value = false;

		// Instantiate the prefab of the player
		m_currentCharacter = Instantiate(m_pfCharacter, transform.position, transform.rotation, null);

		Debug.Log($"GAME_START: Player instantiated.");
	}

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{
		// Display the game start gizmos in editor
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, 1f);
		Gizmos.DrawLine(transform.position, 1.5f * transform.forward.normalized + transform.position);
	}
	
#endif
}
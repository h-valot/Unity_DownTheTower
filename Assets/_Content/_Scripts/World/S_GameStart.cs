using Sirenix.OdinInspector;
using UnityEngine;

public class GameStart : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private GameObject m_pfCharacter;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	private GameObject m_currentCharacter;

	[SerializeField] RSE_PlayMusic m_playMusic;
    [SerializeField] SSO_Sound m_Music;

    /// <summary>
    /// Destroy the former character if exists.
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
		m_currentCharacter = Instantiate(m_pfCharacter, null);
		m_currentCharacter.GetComponent<CharacterMotor>().Initialize(transform.position, transform.rotation);

		m_playMusic.Call(m_Music);

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
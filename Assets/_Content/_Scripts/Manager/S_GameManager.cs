using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Title("Tweakable values")]
	[Required("A Game Start must be assigned to start the game. If there is no in the scene, you can find the prefab here: Content/Prefabs/LevelDesign")]
	[SerializeField] private GameStart m_gameStart;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;

	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += HandleDeath;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= HandleDeath;
	}

	private void Start()
	{
		Restart();
		Cursor.lockState = CursorLockMode.Locked;
		m_rsoGamePaused.value = false;
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
}
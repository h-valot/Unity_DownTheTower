using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game")]
public class GameConfig : ScriptableObject
{
	[Header("Debug")]
	public bool enableConsoleLogging;
}	
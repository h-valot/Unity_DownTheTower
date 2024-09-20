using UnityEngine;

[CreateAssetMenu(fileName = "LadderConfig", menuName = "Configs/Ladder")]
public class LadderConfig : ScriptableObject
{
	[Header("Prefabs")]
	public PreLadder pfPreLadder;
	public Ladder pfLadder;

	[Header("Deploy")]
	public float maxHeight = 4f;
	public float additionalRaycastHeight = 0.5f;
	public LayerMask layersToIgnore;

	[Header("Placement")]
	public float minDistFromPlayer = 1f;
	public float maxDistFromPlayer = 4f;

	[Header("Camera")]
	public float minCameraAngle = 1f;
	public float maxCameraAngle = 30f;
}
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

	[Header("Checks")]
	public float minDistanceFromWall = 0.4f;
	public float heightForDistFromWall = 0.25f;

	[Header("Camera")]
    [Tooltip("1f = 0° ; 0f = 90° ; -1f = 180°")]
    public float maxGroundAngle = 0.5f;
	public float maxDistFromCamera = 8f;
	public float minCameraAngle = 10f;
	public float maxCameraAngle = 70f;
}
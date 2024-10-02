using UnityEngine;

[CreateAssetMenu(fileName = "LadderConfig", menuName = "Configs/Ladder")]
public class LadderConfig : ScriptableObject
{
	[Header("Prefabs")]
	public Ladder PF_Ladder;

	[Header("Deploy")]
	public float maxHeight = 4f;
	public float additionalRaycastHeight = 0.5f;
	public LayerMask layersToIgnore;
    [Tooltip("Horizontal is 90°, Vertical is 0°")]
    public float minWalkableAngle = 45f;

	[Header("Checks")]
	public float minDistanceFromWall = 0.4f;
	public float heightForDistFromWall = 0.25f;
    [Tooltip("1f = 0° ; 0f = 90° ; -1f = 180°")]
    public float maxGroundAngle = 0.5f;

    [Header("Camera")]
	public float maxDistFromCamera = 8f;
	public float cameraOffset = 0.3f;
	public float maxCameraDownwardClamp = 0.90f;
}
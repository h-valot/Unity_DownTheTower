using UnityEngine;

[CreateAssetMenu(fileName = "LadderConfig", menuName = "Static Scriptable/Ladder")]
public class LadderConfig : ScriptableObject
{
	[Header("Prefabs")]
	public Ladder PF_Ladder;

	[Header("Deploy")]
	public float maxHeight = 4f;
	public float additionalRaycastHeight = 0.5f;
	public LayerMask layersToIgnore;
    [Tooltip("Horizontal is 90 degrees, Vertical is 0 degree")]
    public float minWalkableAngle = 45f;

	[Header("Checks")]
	public float minDistanceFromWall = 0.4f;
	public float heightForDistFromWall = 0.25f;
    [Tooltip("1f = 0 degree ; 0f = 90 degrees ; -1f = 180 degrees")]
    public float maxGroundAngle = 0.5f;

    [Header("Camera")]
	public float maxDistFromCamera = 8f;
	public float cameraOffset = 0.3f;
	public float maxCameraDownwardClamp = 0.90f;
}
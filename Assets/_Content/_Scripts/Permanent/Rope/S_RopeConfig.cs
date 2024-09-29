using UnityEngine;

[CreateAssetMenu(fileName = "RopeConfig", menuName = "Configs/Rope")]
public class RopeConfig : ScriptableObject
{
	[Header("Prefabs")]
	public PreRope pfPreRope;
	public Rope pfRope;
	public RopeSegment pfSegment;

	[Header("Settings")]
	public float maxLength;
	public LayerMask foldLayerToIgnore;
	public float foldMinimalDistance = 0.25f;

	[Header("Deploy")]
	internal float heightLimit = 1f;
	public LayerMask layersToIgnore;

	[Header("Placement")]
	public float minDistFromPlayer = 1f;
	public float maxDistFromPlayer = 4f;

	[Header("Camera")]
	public float minCameraAngle = 1f;
	public float maxCameraAngle = 30f;
}
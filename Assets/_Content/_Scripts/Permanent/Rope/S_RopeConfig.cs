using UnityEngine;

[CreateAssetMenu(fileName = "RopeConfig", menuName = "Configs/Rope")]
public class RopeConfig : ScriptableObject
{
	[Header("Prefabs")]
	public PreRope pfPreRope;
	public Rope pfRope;

	[Header("Settings")]
	public float maxLength;
	public LayerMask foldLayer;

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
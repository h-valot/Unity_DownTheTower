using UnityEngine;

[CreateAssetMenu(fileName = "RopeConfig", menuName = "Configs/Rope")]
public class RopeConfig : ScriptableObject
{
	[Header("Prefabs")]
	public Rope pfRope;

	[Header("Settings")]
	public int maxSegmentInstantiatedPerFrame = 5;
	public float maxLength;
	public LayerMask foldLayerToIgnore;
	public float minFoldDistance = 0.25f;

	[Header("Color")]
	public Color safeColor;
	public Color midColor;
	public Color dangerColor;

	[Header("Deploy")]
	public float heightLimit = 1f;
	public LayerMask layersToIgnore;
	public float craftingDuration = 0f;

	[Header("Check")]
	public float minDistanceFromWall = 0.4f;
	[Tooltip("1f = 0 degree ; 0f = 90 degrees ; -1f = 180 degrees")]
	public float maxGroundAngle = 0.6f;

	[Header("Camera")]
	public float maxDistFromCamera = 8f;
	public float cameraOffsetAngle = 20f;
	public float maxCameraDownwardClamp = 45f;
}
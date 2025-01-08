using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Rope", menuName = "Static Scriptable/Rope")]
public class SSO_Rope : ScriptableObject
{
	[Title("Debug")]
	[InfoBox("Material applied to the rope line if the total length is less or equal than half of the max length.", InfoMessageType.None)]
	public Material SafeMaterial;

	[InfoBox("Material applied to the rope line if the total length is less or equal than three quarters of the max length.", InfoMessageType.None)]
	public Material MidMaterial;

	[PropertySpace(SpaceBefore = 0, SpaceAfter = 15)]
	[InfoBox("Material applied to the rope line if the total length is greater than three quarters of the max length.", InfoMessageType.None)]
	public Material DangerMaterial;

	
	#region PREFAB

	[FoldoutGroup("Prefabs")]
	[InfoBox("The prefab of the rope.", InfoMessageType.None)]
	public Rope PfRope;

	[FoldoutGroup("Prefabs")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The prefab of the rope line graphics.", InfoMessageType.None)]
	public RopeLine PfRopeLine;

	[FoldoutGroup("Prefabs")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The prefab of the physic and interactable component of the rope folds.", InfoMessageType.None)]
	public RopeSegment PfRopeSegment;

	#endregion

	#region GLOBAL SETTINGS

	[FoldoutGroup("Global settings")]
	[InfoBox("The max length of the rope. If this length is exceeded, the character will be detach from it.", InfoMessageType.None)]
	public float MaxLength;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Radius of the sphere trigger collider of the rope interactable.", InfoMessageType.None)]
	public float InteractableSphereRadius;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The rope folding system will only care about collider of the given layer mask.", InfoMessageType.None)]
	public LayerMask FoldLayerToInclude;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The distance between the accurate fold hit point and the offsetted fold.", InfoMessageType.None)]
	public float FoldOffset = 0.2f;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The distance between two following folds can't be smaller than the given float.", InfoMessageType.None)]
	public float MinFoldDistance = 0.25f;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Below this distance, the character can not climb up the rope.", InfoMessageType.None)]
	public float MinimumClimbLength = 1f;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("", InfoMessageType.None)]
	public float LineSegmentSize;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("", InfoMessageType.None)]
	public float LineWidth;

	[FoldoutGroup("Global settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("", InfoMessageType.None)]
	public float FadeInDistance;

	#endregion

	#region DEPLOYMENT

	[FoldoutGroup("Deployment")]
	[InfoBox("Duration the permanent will take to be crafted.", InfoMessageType.None)]
	public float CraftingDuration = 0f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Layer masks the deployment system will ignore for its physics calculations (raycasts).", InfoMessageType.None)]
	public LayerMask DeployLayersToIgnore;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum distance from the pivot point of the permanent to a potential collider above it. If a collider stands in-between, the deployment is invalid.", InfoMessageType.None)]
	public float HeightLimit = 1f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum distance from the permanent to a potential collider in front of it. If a collider stands in-between, the deployment is invalid.", InfoMessageType.None)]
	public float MinDistanceFromWall = 0.4f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[Range(-1f, 1f)]
	[InfoBox("Maximum angle the ground can be titled. Above this threshold, the deployment is invalid (with -1f corresponding to 180 degrees and 1f corresponding to 0 degree).", InfoMessageType.None)]
	public float MaxGroundAngle = 0.6f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum distance from the camera the permanent can be instantiate.", InfoMessageType.None)]
	public float MaxDistFromCamera = 8f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Angle used to offset vector of the camera to get the offset vector indicating the position of the permanent preview.", InfoMessageType.None)]
	public float CameraOffsetAngle = 20f;

	[FoldoutGroup("Deployment")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum angle the offset vector indicating the position of the permanent preview could be on the right axis.", InfoMessageType.None)]
	public float MaxCameraDownwardClamp = 45f;

	#endregion
}
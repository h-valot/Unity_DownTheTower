using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RopeConfig", menuName = "Configs/Rope")]
public class RopeConfig : ScriptableObject
{
	#region PREFAB

	[FoldoutGroup("Prefabs")]
	[InfoBox("The prefab of the rope.")]
	/// <summary> The prefab of the rope. </summary>
	public Rope PfRope;

	[FoldoutGroup("Prefabs")]
	[InfoBox("The prefab of the rope line graphics.")]
	/// <summary> The prefab of the rope line graphics. </summary>
	public RopeLine PfRopeLine;

	[FoldoutGroup("Prefabs")]
	[InfoBox("The prefab of the interatable component of the rope folds.")]
	/// <summary> The prefab of the interatable component of the rope folds. </summary>
	public Interactable PfRopeInteractible;

	#endregion

	#region GLOBAL SETTINGS

	[FoldoutGroup("Global settings")]
	[InfoBox("The max length of the rope. If this length is exceeded, the character will be detach from it.")]
	/// <summary> The max length of the rope. If this length is exceeded, the character will be detach from it. </summary>
	public float MaxLength;

	[FoldoutGroup("Global settings")]
	[InfoBox("The rope folding system will only care about collider of the given layer mask.")]
	/// <summary> The rope folding system will only care about collider of the given layer mask. </summary>
	public LayerMask FoldLayerToInclude;

	[FoldoutGroup("Global settings")]
	[InfoBox("The distance between two following folds can't be smaller than the given float.")]
	/// <summary> The distance between two following folds can't be smaller than the given float. </summary>
	public float MinFoldDistance = 0.25f;

	#endregion

	#region DEPLOYMENT

	[FoldoutGroup("Deployment")]
	[InfoBox("Duration the permanent will take to be crafted.")]
	/// <summary> Duration the permanent will take to be crafted. </summary>
	public float CraftingDuration = 0f;

	[FoldoutGroup("Deployment")]
	[InfoBox("Layer masks the deployment system will ignore for its physics calculations (raycasts).")]
	/// <summary> Layer masks the deployment system will ignore for its physics calculations (raycasts). </summary>
	public LayerMask DeployLayersToIgnore;

	[FoldoutGroup("Deployment")]
	[InfoBox("Minimum distance from the pivot point of the permanent to a potential collider above it. If a collider stands in-between, the deployment is invalid.")]
	/// <summary> Minimum distance from the pivot point of the permanent to a potential collider above it. If a collider stands in-between, the deployment is invalid. </summary>
	public float HeightLimit = 1f;

	[FoldoutGroup("Deployment")]
	[InfoBox("Minimum distance from the permanent to a potential collider in front of it. If a collider stands in-between, the deployment is invalid.")]
	/// <summary> Minimum distance from the permanent to a potential collider in front of it. If a collider stands in-between, the deployment is invalid. </summary>
	public float MinDistanceFromWall = 0.4f;

	[FoldoutGroup("Deployment")]
	[Range(-1f, 1f)]
	[InfoBox("Maximum angle the ground can be titled. Above this threshold, the deployment is invalid (with -1f corresponding to 180 degrees and 1f corresponding to 0 degree).")]
	/// <summary> Maximum angle the ground can be titled. Above this threshold, the deployment is invalid (with -1f corresponding to 180 degrees and 1f corresponding to 0 degree). </summary>
	public float MaxGroundAngle = 0.6f;

	[FoldoutGroup("Deployment")]
	[InfoBox("Maximum distance from the camera the permanent can be instantiate.")]
	/// <summary> Maximum distance from the camera the permanent can be instantiate. </summary>
	public float MaxDistFromCamera = 8f;

	[FoldoutGroup("Deployment")]
	[InfoBox("Angle used to offset vector of the camera to get the offset vector indicating the position of the permanent preview.")]
	/// <summary> Angle used to offset vector of the camera to get the offset vector indicating the position of the permanent preview. </summary>
	public float CameraOffsetAngle = 20f;

	[FoldoutGroup("Deployment")]
	[InfoBox("Maximum angle the offset vector indicating the position of the permanent preview could be on the right axis.")]
	/// <summary> Maximum angle the offset vector indicating the position of the permanent preview could be on the right axis. </summary>
	public float MaxCameraDownwardClamp = 45f;

	#endregion

	#region DEBUG

	[FoldoutGroup("Debug")]
	[InfoBox("Material applied to the rope line if the total length is less or equal than half of the max length.")]
	/// <summary> Material applied to the rope line if the total length is less or equal than half of the max length. </summary>
	public Material SafeMaterial;

	[FoldoutGroup("Debug")]
	[InfoBox("Material applied to the rope line if the total length is less or equal than three quarters of the max length.")]
	/// <summary> Material applied to the rope line if the total length is less or equal than three quarters of the max length. </summary>
	public Material MidMaterial;

	[FoldoutGroup("Debug")]
	[InfoBox("Material applied to the rope line if the total length is greater than three quarters of the max length.")]
	/// <summary> Material applied to the rope line if the total length is greater than three quarters of the max length. </summary>
	public Material DangerMaterial;

	#endregion
}
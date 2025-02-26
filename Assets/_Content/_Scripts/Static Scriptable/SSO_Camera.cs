using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Camera", menuName = "Static Scriptable/Camera")]
public class SSO_Camera : ScriptableObject
{
	#region GENERAL SETTINGS

	[FoldoutGroup("General settings")]
	[InfoBox("Starting style of the camera. BASIC: Default 3rd Person Camera. AIMING: 3rd Person Camera where the character always face the camera forward direction.", InfoMessageType.None)]
	public CameraStyle StartingStyle;

	[FoldoutGroup("General settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum angle of the camera pitch can move up to.", InfoMessageType.None)]
	public float TopClamp;

	[FoldoutGroup("General settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum angle of the camera pitch can move down to.", InfoMessageType.None)]
	public float BottomClamp;

	[FoldoutGroup("General settings")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Speed value of the lerp function that orient the character graphics in the direction of the movement.", InfoMessageType.None)]
	public float RotationSpeed;


	#endregion

	#region GLOBAL PARAMETERS

	[FoldoutGroup("Global parameters")]
	[PropertyRange(0f, 10f)]
	[InfoBox("Lerp when the camera zoom out back to its maximum distance.", InfoMessageType.None)]
	public float DampingFromCollision;

	[FoldoutGroup("Global parameters")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[PropertyRange(0.0f, 0.5f)]
	[InfoBox("Value added to the Cinemachine 3rd Person Camera on the Shoulder Offset Y when the distance between the character and the camera is below a threshold. (Default = 0.0f)", InfoMessageType.None)]
	public float ShoulderOffsetY;

	#endregion

	#region SUSPENDED BEHAVIOR

	[Title("Camera Distance")]
	[FoldoutGroup("Suspended behavior")]
	[InfoBox("Distance between the 3rd person camera and the character while not suspended on rope.", InfoMessageType.None)]
	public float DefaultCameraDistance;

	[FoldoutGroup("Suspended behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance between the 3rd person camera and the character while suspended on rope.", InfoMessageType.None)]
	public float SuspendedCameraDistance;

	[FoldoutGroup("Suspended behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the lerp transition between default and suspended camera distance. (Mathf.Lerp() so the smaller, the quicker)", InfoMessageType.None)]
	public float TransitionCameraDistance;


	[Title("Look at local offset Y")]
	[FoldoutGroup("Suspended behavior")]
	[InfoBox("Offset applied to the camera target on the y-axis.", InfoMessageType.None)]
	public float SuspendedLookAtOffsetY;

	[FoldoutGroup("Suspended behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the lerp transition between default and suspended camera target local Y offset. (Vector3.SmoothDamp() so the smaller, the slower)", InfoMessageType.None)]
	public float TransitionLookAt;

	#endregion

	#region LOCOMOTION BEHAVIOR

	[Title("Shoulder Offset Z")]
	[FoldoutGroup("Locomotion behavior")]
	[InfoBox("Value applied as an override to the Cinemachine 3rd Person Camera on the Shoulder Offset Z when the character is looking down. (Default = 0.0f)", InfoMessageType.None)]
	public float LocomotionShoulderOffsetZ = 1f;

	[FoldoutGroup("Locomotion behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Threshold of the camera pitch value to apply the custom value to the Shoulder Offset Z.", InfoMessageType.None)]
	public float ThresholdShouldOffsetZ = 55f;

	[FoldoutGroup("Locomotion behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Durationg of the lerp transition between default and custom Shoulder Offset Z. (Mathf.Lerp() so the smaller, the quicker)", InfoMessageType.None)]
	public float TransitionShoulderOffsetZ;


	[Title("Camera Side")]
	[FoldoutGroup("Locomotion behavior")]
	[PropertyRange(0.0f, 0.5f)]
	[InfoBox("Value added to the Cinemachine 3rd Person Camera on the Camera Side when the character is looking up. (Default = 0.5f)", InfoMessageType.None)]
	public float LocomotionCameraSide = 0.5f;

	[FoldoutGroup("Locomotion behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Threshold of the camera pitch value to start gradually adding the custom Camera Side value.", InfoMessageType.None)]
	public float ThresholdCameraSide = -10f;

	[FoldoutGroup("Locomotion behavior")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the lerp transition between default and custom Camera Side. (Mathf.Lerp() so the smaller, the quicker)", InfoMessageType.None)]
	public float TransitionCameraSide;

	#endregion
}
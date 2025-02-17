using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Camera", menuName = "Static Scriptable/Camera")]
public class SSO_Camera : ScriptableObject
{
	[InfoBox("Starting style of the camera. BASIC: default third person camera. AIMING: third person camera where the character always face the camera forward direction.", InfoMessageType.None)]
	public CameraStyle StartingStyle;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum angle of the camera pitch can move up to.", InfoMessageType.None)]
	public float TopClamp;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum angle of the camera pitch can move down to.", InfoMessageType.None)]
	public float BottomClamp;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Speed value of the lerp function that orient the character graphics in the direction of the movement.", InfoMessageType.None)]
	public float RotationSpeed;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance between the 3rd person camera and the character while not suspended on rope.", InfoMessageType.None)]
	public float DefaultDistance;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance between the 3rd person camera and the character while suspended on rope.", InfoMessageType.None)]
	public float SuspendedDistance;

	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the lerp transition between default and suspended.", InfoMessageType.None)]
	public float DistanceTransition;
}
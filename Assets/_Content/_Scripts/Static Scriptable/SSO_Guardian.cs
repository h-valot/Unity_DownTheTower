using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Guardian", menuName = "Static Scriptable/Guardian")]
public class SSO_Guardian : ScriptableObject
{
	[Title("Debug")]
	[InfoBox("", InfoMessageType.None)]
	public Material AggroMaterial;

	[InfoBox("", InfoMessageType.None)]
	public Material PatrolMaterial;

	[InfoBox("", InfoMessageType.None)]
	[PropertySpace(SpaceAfter = 15, SpaceBefore = 0)]
	public Material DormantMaterial;

	#region SIGHT

	[FoldoutGroup("Sight")]
	[InfoBox("", InfoMessageType.None)]
	public float MaxRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	public float MinRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	public LayerMask TargetLayerToIgnore;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	[PropertyRange(-1f, 1f)]
	public float DefaultAngleSight;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	[PropertyRange(-1f, 1f)]
	public float ExtendedAngleSight;

	#endregion

	#region PATROL

	[FoldoutGroup("Patrol")]
	[InfoBox("Speed of the guardian when it is patrolling.", InfoMessageType.None)]
	public float PatrolSpeed;

	[FoldoutGroup("Patrol")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	[PropertyRange(0.1f, 0.5f)]
	public float WaypointDistanceTolerance;

	[FoldoutGroup("Patrol")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration the guardian waits when it reaches the next waypoint. NOTE: This value depends on when the guardian is considered to have reached the waypoint (tweakable with Waypoint Distance Tolerance).", InfoMessageType.None)]
	public float WaitDurationOnWaypointReached;

	#endregion

	#region AGGRO

	[FoldoutGroup("Aggro")]
	[InfoBox("Speed of the guardian when it has a target aggroed.", InfoMessageType.None)]
	public float AggroSpeed;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still before killing the character.", InfoMessageType.None)]
	public Vector2 DelayKillCharacter;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still before destroying a torch.", InfoMessageType.None)]
	public Vector2 DelayDestroyTorch;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still before destroying a rope.", InfoMessageType.None)]
	public Vector2 DelayDestroyRope;

	#endregion

	#region SEEK

	[FoldoutGroup("Seek")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	public float OmniscienceDuration;

	[FoldoutGroup("Seek")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration the guardian waits when it reaches the last target position. During this time, the guardian has an extended angle sight.", InfoMessageType.None)]
	public float SeekingDuration;

	#endregion
}
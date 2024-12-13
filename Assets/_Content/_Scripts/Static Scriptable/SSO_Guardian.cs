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
	public float SightRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	public float PassiveRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	public LayerMask TargetLayerToIgnore;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("", InfoMessageType.None)]
	[PropertyRange(-1f, 1f)]
	public float AngleSight;

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

	[FoldoutGroup("Patrol")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration the guardian waits when it reaches the last target position.", InfoMessageType.None)]
	public float WaitDurationOnLastTargetPositionReached;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still before destroying a torch.", InfoMessageType.None)]
	public float TimeToDestroyTorch;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still before killing the character.", InfoMessageType.None)]
	public float TimeToKillCharacter;

	#endregion
}
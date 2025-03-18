using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Guardian", menuName = "Static Scriptable/Guardian")]
public class SSO_Guardian : ScriptableObject
{
    #region FEEDBACKS

    [FoldoutGroup("Feedbacks")]
    [InfoBox("Color applied to the guardian eyes when in aggro state.", InfoMessageType.None)]
	public Color AggroColor;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Opacity of the beam when in aggro state.", InfoMessageType.None)]
    public float AggroOpacity;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Size of the beam when in aggro state.", InfoMessageType.None)]
    public float AggroFocus;


    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Color applied to the guardian eyes when in patrol state.", InfoMessageType.None)]
	public Color PatrolColor;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Opacity of the beam when in patrol state.", InfoMessageType.None)]
    public float PatrolOpacity;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Size of the beam when in patrol state.", InfoMessageType.None)]
    public float PatrolFocus;


    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Color applied to the guardian eyes when in seek state.", InfoMessageType.None)]
	public Color SeekColor;
	
    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Opacity of the beam when in seek state.", InfoMessageType.None)]
    public float SeekOpacity;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Size of the beam when in seek state.", InfoMessageType.None)]
    public float SeekFocus;


    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Color applied to the guardian eyes when in dormant state.", InfoMessageType.None)]
	public Color DormantColor;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Opacity of the beam when in dormant state.", InfoMessageType.None)]
    public float DormantOpacity;

    [FoldoutGroup("Feedbacks")]
    [PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
    [InfoBox("Size of the beam when in dormant state.", InfoMessageType.None)]
    public float DormantFocus;

	#endregion

	#region SIGHT

	[FoldoutGroup("Sight")]
	[InfoBox("Radius of the guardian activation range. If a character, torch or rope enters this range, active guardian. Desactivate it, if character exit.", InfoMessageType.None)]
	public float ActivationRadius;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Radius of the guardian long sight range. If a light source enters the sphere of the given radius in front of the guardian (cf. DefaultAngleSight), it will become a target.", InfoMessageType.None)]
	public float LongRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Radius of the guardian clear sight range. If a light source enters the sphere of the given radius, it will become a target. If a rope, character or unlit torch enters the sphere in front of the guardian, it will also become a target.", InfoMessageType.None)]
	public float ClearRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Radius of the guardian letahl sight range. If a rope, character or torch enters the sphere of the given radius, it will become a target.", InfoMessageType.None)]
	public float LethalRange;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Layer masks ignored by the sight system.", InfoMessageType.None)]
	public LayerMask TargetLayerToIgnore;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Angle that defines the guardian sight cone where 1 is a closer vision and -1 is a vision all around.", InfoMessageType.None)]
	[PropertyRange(-1f, 1f)]
	public float DefaultAngleSight;

	[FoldoutGroup("Sight")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Angle that defines the guardian sight cone (same as DefaultAngleSight) when the guardian is seeking the target.", InfoMessageType.None)]
	[PropertyRange(-1f, 1f)]
	public float ExtendedAngleSight;

	#endregion

	#region PATROL

	[FoldoutGroup("Patrol")]
	[InfoBox("Speed of the guardian when patrolling.", InfoMessageType.None)]
	public float PatrolSpeed;

	[FoldoutGroup("Patrol")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Radius of the guardian waypoint reaching tolerance. The waypoint is marked as reached when the waypoint enters the sphere of the given radius.", InfoMessageType.None)]
	[PropertyRange(0.1f, 0.5f)]
	public float WaypointDistanceTolerance;

	[FoldoutGroup("Patrol")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration the guardian waits when it reaches the next waypoint. NOTE: This value depends on when the guardian is considered to have reached the waypoint (cf. WaypointDistanceTolerance).", InfoMessageType.None)]
	public float WaitDurationOnWaypointReached;

	#endregion

	#region AGGRO

	[FoldoutGroup("Aggro")]
	[InfoBox("Speed of the guardian when it has a target aggroed.", InfoMessageType.None)]
	public float AggroSpeed;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still BEFORE and AFTER killing the character.", InfoMessageType.None)]
	public Vector2 DelayKillCharacter;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still BEFORE and AFTER destroying a torch.", InfoMessageType.None)]
	public Vector2 DelayDestroyTorch;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian stand still BEFORE and AFTER destroying a rope.", InfoMessageType.None)]
	public Vector2 DelayDestroyRope;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Delay before exiting aggro state if the guardian isn't moving.", InfoMessageType.None)]
	public float AggroTimeout;

	[FoldoutGroup("Aggro")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Minimum distance travalled to be considered as moving while in aggro state.", InfoMessageType.None)]
	public float LockedThreshold;

	#endregion

	#region SEEK

	[FoldoutGroup("Seek")]
	[InfoBox("Duration during which the guardian knows and moves towards its target current position even if unsee.", InfoMessageType.None)]
	public float OmniscienceDuration;

	[FoldoutGroup("Seek")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration during which the guardian waits when it reaches the last target position while omnisciente. During this time, the guardian uses the ExtendedAngleSight instead of DefaultAngleSight.", InfoMessageType.None)]
	public float SeekingDuration;

	[FoldoutGroup("Seek")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration of the timeout delay. At the end of this duration, the seek state is forced quit.", InfoMessageType.None)]
	public float SeekTimeoutTimer;

	[FoldoutGroup("Seek")]
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	[InfoBox("Duration of the timeout delay. When the guardian isn't moving, a delay starts. At the end of it, the target is banned and the seek state is forced quit.", InfoMessageType.None)]
	public float StuckTimeoutTimer;

	#endregion
}
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Character", menuName = "Static Scriptable/Character")]
public class SSO_Character : ScriptableObject
{
    /// <summary>
	/// 	Unity in-built editor function called whenever the scriptable object is updated.
    /// 	Used to apply change to drag immediatly to rigidbody.
	/// </summary>
	public void OnValidate() => OnConfigChanged?.Invoke();
	public Action OnConfigChanged;


	[Title("Debug")]
	[InfoBox("If true, the character starts the play mode with a backpack.", InfoMessageType.None)]
	/// <summary> If true, the character starts the play mode with a backpack. </summary>
	public bool StartWithBag;

	[ShowIf("StartWithBag")]
	[PropertySpace(SpaceBefore = 0, SpaceAfter = 15)]
	[InfoBox("Prefab of the backpack. Spawned only if the `StartWithBag` debug is true.", InfoMessageType.None)]
	/// <summary> Prefab of the backpack. Spawned only if the `StartWithBag` debug is true. </summary>
	public Backpack PfBackpack;

	#region MOVEMENT

	[Title("Speed")]
	[FoldoutGroup("Movement")]
	[InfoBox("Magnitude of the character direction input when walking.", InfoMessageType.None)]
	/// <summary> Magnitude of the character direction input when walking. </summary>
	public float WalkSpeed;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Magnitude of the character direction input when running.", InfoMessageType.None)]
	/// <summary> Magnitude of the character direction input when running. </summary>
	public float RunSpeed;


	[Title("Drag")]
	[FoldoutGroup("Movement")]
	[InfoBox("Drag applied to the character while grounded.", InfoMessageType.None)]
	/// <summary> Drag applied to the character while grounded. </summary>
	public float DragGround;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Drag applied to the character while falling.", InfoMessageType.None)]
	/// <summary> Drag applied to the character while falling. </summary>
	public float DragFall;


	[Title("Friction")]
	[FoldoutGroup("Movement")]
	[InfoBox("Friction applied to the character while grounded and unmoving.", InfoMessageType.None)]
	/// <summary> Friction applied to the character while grounded and unmoving. </summary>
	public float FrictionNotMovingGround;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Friction applied to the character while falling or moving.", InfoMessageType.None)]
	/// <summary> Friction applied to the character while falling or moving. </summary>
	public float FrictionMovingFalling;

	#endregion

	#region JUMP

	[FoldoutGroup("Jump")]
	[InfoBox("Magnitude of the up vector when jumping.", InfoMessageType.None)]
	/// <summary> Magnitude of the up vector when jumping. </summary>
    public float JumpForce;

	[FoldoutGroup("Jump")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar applied to the the max speed while falling.", InfoMessageType.None)]
	/// <summary> Scalar applied to the character input direction while falling. </summary>
	public float AirControlSpeedFactor;

    [FoldoutGroup("Jump")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Scalar applied to the force applied to character while falling.", InfoMessageType.None)]
    /// <summary> Scalar applied to the character input direction while falling. </summary>
    public float MaxAirControlForceFactor;

    [FoldoutGroup("Jump")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Time it takes to decrease air control force factor.", InfoMessageType.None)]
    /// <summary> Scalar applied to the character input direction while falling. </summary>
    public float AirControlTime;

    [FoldoutGroup("Jump")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration during which the character is still able to jumb after start falling.", InfoMessageType.None)]
	/// <summary> Duration during which the character is still able to jumb after start falling. </summary>
	public float CoyoteTime;

	#endregion

	#region GROUND

	[FoldoutGroup("Ground")]
	[InfoBox("Check ground & handle step on systems raycasts will only include the following layer masks.", InfoMessageType.None)]
	/// <summary> Check ground & handle step on systems raycasts will only include the following layer masks. </summary>
	public LayerMask GroundLayerToInclude;

	[FoldoutGroup("Ground")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Width added to cast used to determine if it is triggering things.", InfoMessageType.None)]
	/// <summary> Width added to cast used to determine if it is triggering things. </summary>
	public float SkinWidth;

	[FoldoutGroup("Ground")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum height of a step the character will automatically step on.", InfoMessageType.None)]
	/// <summary> Maximum height of a step the character will automatically step on. </summary>
	public float StepOnHeight;

	#endregion

	#region ROPE

	[FoldoutGroup("Rope")]
	[InfoBox("The method the character uses to navigate the rope. HOLD_TO_LET_GO: when the holding input is pressed, the rope constraint will be released. HOLD_TO_STOP: when the holding is pressed, the rope constraint will be applied.", InfoMessageType.None)]
	/// <summary> The method the character uses to navigate the rope. HOLD_TO_LET_GO: when the holding input is pressed, the rope constraint will be released. HOLD_TO_STOP: when the holding is pressed, the rope constraint will be applied. </summary>
	public RopeHolding RopeHoldingMethod = RopeHolding.HOLD_TO_LET_GO;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration the player have to hold the cancel key to desequip the rope.", InfoMessageType.None)]
	/// <summary> Duration the player have to hold the cancel key to desequip the rope. </summary>
	public float CancelRopeDuration;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance the rope constraint length increases when the character enters the rope state.", InfoMessageType.None)]
	/// <summary> Distance the rope constraint length increases when the character enters the rope state. </summary>
	public float EntranceOffset;


	[Title("Suspension")]
	[FoldoutGroup("Rope")]
	[InfoBox("Maximum angle tolerated the character can swing with the rope (upper values are clamped onto this one).", InfoMessageType.None)]
	/// <summary> Maximum angle tolerated the character can swing with the rope (upper values are clamped onto this one). </summary>
	public float MaxPendulumAngle;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Angle used to determine a direction within the cercle generated by the rope condition.", InfoMessageType.None)]
	/// <summary> Angle used to determine a direction within the cercle generated by the rope condition. </summary>
	public float RopeOffsetAngle;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Force applied to the character direction while roped.", InfoMessageType.None)]
	/// <summary> . </summary>
	public float ropeMovementForce;


	[Title("Partial")]
	[FoldoutGroup("Rope")]
	[InfoBox("Length of raycasts used to check if the character stands against a wall.", InfoMessageType.None)]
	/// <summary> Length of raycasts used to check is the character stands against a wall. </summary>
	public float AgainstWallRaycastLength;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Layers included in raycasts used to check if the character stands against a wall.", InfoMessageType.None)]
	/// <summary> Layers included in raycasts used to check if the character stands against a wall. </summary>
	public LayerMask AgainstWallLayerToInclude;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Force of the vector normal to the wall when jumping while suspended with a rope.", InfoMessageType.None)]
	/// <summary> Force of the vector normal to the wall when jumping while suspended with a rope. </summary>
	public float JumpOffWallForce;


	[Title("Climbing")]
	[FoldoutGroup("Rope")]
	[InfoBox("Acceleration value used to determine the force to pull the character up the rope.", InfoMessageType.None)]
	/// <summary> Acceleration value used to determine the force to pull the character up the rope. </summary>
	public float ClimbAcceleration;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum force the character can use to pull itself up the rope.", InfoMessageType.None)]
	/// <summary> Maximum force the character can use to pull itself up the rope. </summary>
	public float MaxClimbSpeed;


	[Title("Jumping")]
	[FoldoutGroup("Rope")]
	[InfoBox("The method the character uses to jump off the rope. SLACKEN: increase the rope hold length of the `JumpRopeSlackenAmount`. RELEASE: desequip the rope.", InfoMessageType.None)]
	/// <summary> The method the character uses to jump off the rope. SLACKEN: increase the rope hold length of the `JumpRopeSlackenAmount`. RELEASE: desequip the rope. </summary>
	public JumpMethod JumpRopeMethod;

	[FoldoutGroup("Rope")]
	[ShowIf("@this.JumpRopeMethod == JumpMethod.SLACKEN")]
	[InfoBox("Amount the rope hold length will be increased when the character is jump off the rope. (Only if SLACKEN method is selected)", InfoMessageType.None)]
	/// <summary> Amount the rope hold length will be increased when the character is jump off the rope. (Only if SLACKEN method is selected) </summary>
	public float JumpRopeSlackenAmount;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Modifier applied to the last direction on the rope when the character switches from the rope to free fall.", InfoMessageType.None)]
	/// <summary> Modifier applied to the last direction on the rope when the character switches from the rope to free fall. </summary>
	public float FreeFallFromRopeModifier;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Modifier applied to the last direction on the rope when the character switches from the rope to jump of the rope.", InfoMessageType.None)]
	/// <summary> Modifier applied to the last direction on the rope when the character switches from the rope to jump of the rope. </summary>
	public float JumpOffRopeModifier;

	#endregion

	#region GLOW

	[FoldoutGroup("Glow")]
	[InfoBox("Up offset from the character position where the glow starts to be emitted.", InfoMessageType.None)]
	/// <summary> Up offset from the character position where the glow starts to be emitted. </summary>
	public float GlowHeight;

	[FoldoutGroup("Glow")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Radius of the shader glow effect.", InfoMessageType.None)]
	/// <summary> Radius of the shader glow effect. </summary>
	public float GlowRadius;

	[FoldoutGroup("Glow")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Intensity of the shader glow effect.", InfoMessageType.None)]
	/// <summary> Intensity of the shader glow effect. </summary>
	public float GlowStrength;

	[FoldoutGroup("Glow")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Color of the shader glow effect.", InfoMessageType.None)]
	/// <summary> Color of the shader glow effect. </summary>
	public Color GlowColor;

	#endregion

	#region STATUS

	[FoldoutGroup("Status")]
	[InfoBox("Maximum distance the character can travel on the y-axis between two grounded position without dying.", InfoMessageType.None)]
	/// <summary> Maximum distance the character can travel on the y-axis between two grounded position without dying. </summary>
	public float LethalHeight;


	[Title("Stun")]
	[FoldoutGroup("Status")]
	[InfoBox("Maximum distance the character can travel on the y-axis between two grounded position without being stunned.", InfoMessageType.None)]
	/// <summary> Maximum distance the character can travel on the y-axis between two grounded position without being stunned. </summary>
	public float StunHeight;

	[FoldoutGroup("Status")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the stun status based on the distance travelled on the y-axis.", InfoMessageType.None)]
	/// <summary> Duration of the stun status based on the distance travelled on the y-axis. </summary>
	public AnimationCurve StunDuration;

	[FoldoutGroup("Status")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the slow status applied after the stun status vanquishes.", InfoMessageType.None)]
	/// <summary> Duration of the slow status applied after a stun status vanquishes. </summary>
	public float PostStunSlowDuration;


	[Title("Slow")]
	[FoldoutGroup("Status")]
	[InfoBox("Maximum distance the character can travel on the y-axis between two grounded position without being slowed.", InfoMessageType.None)]
	/// <summary> Maximum distance the character can travel on the y-axis between two grounded position without being slowed. </summary>
	public float SlowHeight;

	[FoldoutGroup("Status")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the slow status based on the distance travelled on the y-axis.", InfoMessageType.None)]
	/// <summary> Duration of the slow status based on the distance travelled on the y-axis. </summary>
	public AnimationCurve SlowDuration;

	[FoldoutGroup("Status")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Percentage of the character speed reduction while under the slow status based on the distance travelled on the y-axis.", InfoMessageType.None)]
	/// <summary> Percentage of the character speed reduction while under the slow status based on the distance travelled on the y-axis. </summary>
	public AnimationCurve SlowPercentage;

	[FoldoutGroup("Status")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum duration of the slow status.", InfoMessageType.None)]
	/// <summary> Maximum duration of the slow status. </summary>
	public float MaxSlowDuration;

	#endregion

	#region CAMERA

	[FoldoutGroup("Camera")]
	[InfoBox("Starting style of the camera. BASIC: default third person camera. AIMING: third person camera where the character always face the camera forward direction.", InfoMessageType.None)]
	/// <summary> Starting style of the camera. BASIC: default third person camera. AIMING: third person camera where the character always face the camera forward direction. </summary>
	public CameraStyle StartingStyle;

	[FoldoutGroup("Camera")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Maximum angle of the camera pitch can move up to.", InfoMessageType.None)]
	/// <summary> Maximum angle of the camera pitch can move up to. </summary>
	public float TopClamp;

	[FoldoutGroup("Camera")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum angle of the camera pitch can move down to.", InfoMessageType.None)]
	/// <summary> Minimum angle of the camera pitch can move down to. </summary>
	public float BottomClamp;

	[FoldoutGroup("Camera")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Speed value of the lerp function that orient the character graphics in the direction of the movement.", InfoMessageType.None)]
	/// <summary> Speed value of the lerp function that orient the character graphics in the direction of the movement. </summary>
	public float RotationSpeed;

	#endregion
}
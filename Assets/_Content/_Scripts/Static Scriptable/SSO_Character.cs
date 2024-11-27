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


	public GameObject pfCamera;
	public Backpack pfBackpack;

	#region MOVEMENT

	[FoldoutGroup("Movement")]
	[InfoBox("Magnitude of the character direction input when walking.", InfoMessageType.None)]
	/// <summary> Magnitude of the character direction input when walking. </summary>
	public float WalkSpeed;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Magnitude of the character direction input when running.", InfoMessageType.None)]
	/// <summary> Magnitude of the character direction input when running. </summary>
	public float RunSpeed;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Drag applied to the character while grounded.", InfoMessageType.None)]
	/// <summary> Drag applied to the character while grounded. </summary>
	public float DragGround;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Drag applied to the character while falling.", InfoMessageType.None)]
	/// <summary> Drag applied to the character while falling. </summary>
	public float DragFall;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Friction applied to the character while falling or moving.", InfoMessageType.None)]
	/// <summary> Friction applied to the character while falling or moving. </summary>
	public float FrictionMovingFalling;

	[FoldoutGroup("Movement")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Friction applied to the character while grounded or unmoving.", InfoMessageType.None)]
	/// <summary> Friction applied to the character while grounded or unmoving. </summary>
	public float FrictionNotMovingGround;

	#endregion

	#region JUMP

	[FoldoutGroup("Jump")]
	[InfoBox("Magnitude of the up vector when jumping.", InfoMessageType.None)]
	/// <summary> Magnitude of the up vector when jumping. </summary>
    public float JumpForce;

	[FoldoutGroup("Jump")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Scalar applied to the character input direction while falling.", InfoMessageType.None)]
	/// <summary> Scalar applied to the character input direction while falling. </summary>
	public float FallingControlFactor;

	[FoldoutGroup("Jump")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration during which the character is still able to jumb after start falling.", InfoMessageType.None)]
	/// <summary> Duration during which the character is still able to jumb after start falling. </summary>
	public float CoyoteTime;

	#endregion

	#region GROUND

	[FoldoutGroup("Ground")]
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


	[Title("Suspension")]
	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
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
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
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

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox(".", InfoMessageType.None)]
	/// <summary> . </summary>
	public float ClimbAcceleration;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox(".", InfoMessageType.None)]
	/// <summary> . </summary>
	public float maxClimbSpeed;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox(".", InfoMessageType.None)]
	/// <summary> . </summary>
	public float freeFallFromRopeModifier;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox(".", InfoMessageType.None)]
	/// <summary> . </summary>
	public float jumpOffRopeModifier;

	[FoldoutGroup("Rope")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox(".", InfoMessageType.None)]
	/// <summary> . </summary>
	public float jumpRopeDuration;

	#endregion


	[Header("Glow")]
	public float glowHeight;
	public float glowRadius;
	public float glowStrength;
	public Color glowColor;


	[Header("Debug")]
	[Tooltip("If true, the character starts the play mode with a backpack.")]
	public bool startWithBag;


	[Header("Status")]
	public float lethalHeight;
	
	public float stunHeight;
	public AnimationCurve stunDuration;

	public float slowHeight;
	public AnimationCurve slowDuration;
	public float slowTimePostStun;
	public AnimationCurve slowPercentage;
	public float maxSlowTime;


	[Header("Camera")]
	public float rotationSpeed;
	public CameraStyle startingStyle;
	public float topClamp;
	public float bottomClamp;
}
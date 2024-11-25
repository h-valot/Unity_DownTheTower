using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Configs/New character")]
public class NewCharacterConfig : ScriptableObject
{
    /// <summary>
	/// 	Unity in-built editor function called whenever the scriptable object is updated.
    /// 	Used to apply change to drag immediatly to rigidbody.
	/// </summary>
	public void OnValidate() => OnConfigChanged?.Invoke();
	public Action OnConfigChanged;


	public GameObject pfCamera;
	public Backpack pfBackpack;


	[Header("Movement")]
    [Tooltip("Max speed when walking")]
    public float walkSpeed;

    [Tooltip("Max speed when running")]
    public float runSpeed;

    [Tooltip("Drag applied on character while grounded")]
    public float dragGround;

    [Tooltip("Drag applied on character while in falling")]
    public float dragFall;

    [Tooltip("Friction applied on character while falling or moving")]
    public float frictionMovingFalling;

    [Tooltip("Friction applied on character while grounded and not moving")]
    public float frictionNotMovingGround;


    [Header("Jump")]
    [Tooltip("Force used to make player jump")]
    public float jumpForce;

    [Tooltip("While falling, factor applied to the force used to make player move")]
    public float fallingControlFactor;

    [Tooltip("Time after start falling while the player can still jump")]
    public float CoyoteTime;


    [Header("Ground")]
    [Tooltip("Width added to cast to determine if touching things")]
    public float skinWidth;

    [Tooltip("Height that the character will automaticly step on")]
    public float stepOnHeight;


	[Header("Rope")]
	public RopeHolding ropeHoldingMethod;
	public float cancelRopeDuration;
	public float againstWallRayCastLength;
	public LayerMask againstWallLayerToInclude;
	public float jumpOffWallForce;
	public float ropeOffsetAngle;
	public float partialSuspensionSpeed;
	public float completeSuspensionSpeed;
	public float maxSideAngle;
	public float climbAcceleration;
	public float maxClimbSpeed;
	public float freeFallFromRopeModifier;
	public float jumpOffRopeModifier;
	public float jumpRopeDuration;


	[Header("Glow")]
	public float glowHeight;
	public float glowRadius;
	public float glowStrength;
	public Color glowColor;


	[Header("Debug")]
	[Tooltip("If true, the character starts the play mode with a backpack.")]
	public bool startWithBag;
}
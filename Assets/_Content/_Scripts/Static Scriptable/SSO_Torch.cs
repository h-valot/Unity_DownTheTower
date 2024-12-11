using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Torch", menuName = "Static Scriptable/Torch")]
public class SSO_Torch : ScriptableObject
{
	[Title("Debug")]
	[InfoBox("If true, the torch break animation will be played.", InfoMessageType.None)]
	public bool ActivateBreakAnim;

	[InfoBox("Prefabs of the break torch sfx.", InfoMessageType.None)]
	public GameObject TorchBreakSFX;

	[PropertySpace(SpaceBefore = 0, SpaceAfter = 15)]
	[InfoBox("Prefabs of the hit torch sfx.", InfoMessageType.None)]
	public GameObject TorchHitSFX;

	#region PREFABS

	[FoldoutGroup("Prefabs")]
	[InfoBox("The prefab of the torch.", InfoMessageType.None)]
	public Torch PfTorch;

	#endregion

	#region MANAGER

	[FoldoutGroup("Manager")]
	[InfoBox("Maximum amount of torches that can be light at the same time, if more are spawn the oldest one starts fading out.", InfoMessageType.None)]
	public int MaxTorchesSoft;

	[FoldoutGroup("Manager")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Very maximum amount of torches that can exists at the same time, if more are spawn the oldest one instant despawns.", InfoMessageType.None)]
	public int MaxTorchesHard;

	#endregion

	#region LIGHT

	[FoldoutGroup("Light")]
	[InfoBox("Default color of the torch point light.", InfoMessageType.None)]
	public Color LightColor = new Color(255, 170, 85, 255);

	[FoldoutGroup("Light")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Default range of the torch point light.", InfoMessageType.None)]
	public float LightRange;

	[FoldoutGroup("Light")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Default intensity of the torch point light.", InfoMessageType.None)]
	public float LightIntensity;

	[FoldoutGroup("Light")]
	[Unit(Units.Second)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration the torch point stays lit while on the ground.", InfoMessageType.None)]
	public float GroundedLightDuration;

	[FoldoutGroup("Light")]
	[Unit(Units.Second)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration the torch takes to lit.", InfoMessageType.None)]
	public float LitDuration;

	[FoldoutGroup("Light")]
	[Unit(Units.Second)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration the torch takes to unlit.", InfoMessageType.None)]
	public float UnlitDuration;

	[FoldoutGroup("Light")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance the top part of the torch will be offset when lit.", InfoMessageType.None)]
	public float TopTorchOffsetDistance;

	[FoldoutGroup("Light")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Distance the point light of the torch will be offset when collider with surfaces.", InfoMessageType.None)]
	public float LightOffsetDistance;

	#endregion

	#region THROW

	[FoldoutGroup("Throw")]
	[InfoBox("If true, the torch can be thrown by the character.", InfoMessageType.None)]
	public bool CanThrow = true;

	[FoldoutGroup("Throw")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The point light offset system will include only those layers.", InfoMessageType.None)]
	public LayerMask LayerColliderToInclude;

	[FoldoutGroup("Throw")]
	[MinMaxSlider(0, 30, true)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum and maximum force torches can be thrown.", InfoMessageType.None)]
	[SerializeField] private Vector2 m_launchForce = new Vector2();
	public MinMaxFloat LaunchForce { get =>  new MinMaxFloat(m_launchForce); private set => LaunchForce = value; }

	[FoldoutGroup("Throw")]
	[MinMaxSlider(0, 200, true)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimum and maximum angle the camera pitch affect the torch throw force.", InfoMessageType.None)]
	[SerializeField] private Vector2 m_launchCameraAngle = new Vector2();
	public MinMaxFloat LaunchCameraAngle { get => new MinMaxFloat(m_launchCameraAngle); private set => LaunchCameraAngle = value; }

	#endregion

	#region CRAFTING

	[FoldoutGroup("Crafting")]
	[InfoBox("Duration the permanent will take to be crafted.", InfoMessageType.None)]
	public float CraftingDuration;

	[FoldoutGroup("Crafting")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("If true, the torch spawns lit.", InfoMessageType.None)]
	public bool IsStartingLit = true;

	#endregion

	#region AIM PREVIEW

	[FoldoutGroup("Aim preview")]
	[Range(0.1f, 10f)]
	[InfoBox("Length of the preview.", InfoMessageType.None)]
	public float PreviewLength;

	[FoldoutGroup("Aim preview")]
	[Range(0.1f, 0.25f)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("The less, the smoother the preview will be.", InfoMessageType.None)]
	public float PreviewAccuracy;

	[FoldoutGroup("Aim preview")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Layer masks the preview system will ignore.", InfoMessageType.None)]
	public LayerMask PreviewLayersToIgnore;

    [FoldoutGroup("Aim preview")]
    [Range(0.01f, 1f)]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Size of line segments (in meters) used to approximate the curve", InfoMessageType.None)]
    public float LineSegmentSize = 0.15f;

    [FoldoutGroup("Aim preview")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Width of the line", InfoMessageType.None)]
    public float LineWidth = 0.1f;

    [FoldoutGroup("Aim preview")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Fade in distance (meters) at the start of the preview", InfoMessageType.None)]
    public float fadeInDistance = 0.5f;

    #endregion

    #region HEIGHT FEEDBACK

    [FoldoutGroup("Height feedback")]
	[InfoBox("Color of the torch point light if its distance travelled on y-axis is greater than the character lethal height.", InfoMessageType.None)]
	public Color DeathColor = new Color(255, 52, 52, 255);

	[FoldoutGroup("Height feedback")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration the torch stays lit before despawning when below the max rope length and the character lethal height.", InfoMessageType.None)]
	public float DesactivatingTime;

	[FoldoutGroup("Height feedback")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Minimal speed of the torch to play a hit sound.", InfoMessageType.None)]
	public float MinSpeedForHitSound;

	[FoldoutGroup("Height feedback")]
	[Unit(Units.Second)]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration in seconds between two hit sound.", InfoMessageType.None)]
	public float TimeBetweenHitSound;

	#endregion
}
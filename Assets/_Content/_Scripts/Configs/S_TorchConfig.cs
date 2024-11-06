using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "TorchConfig", menuName = "Configs/Torch")]
public class TorchConfig : ScriptableObject
{
	[Header("References")]

	[Tooltip("Torch prefab that is instantiated when crafted")]
	public Torch pfTorch;

	[Space(5f)]

	[Header("Light")]

	[Tooltip("Default torch color")]
    public Color lightColor = new Color(255, 170, 85, 255);

	[Tooltip("Intensity of the light")]
	public float lightIntensity;

	[Tooltip("Lit duration when on ground")]
	public float groundedLightDuration;

	[Tooltip("Time to light the torch")]
	public float lightOnDuration;

    [Tooltip("Time to extinguish the torch")]
    public float lightOffDuration;

    [Tooltip("Offset distance of the top part when lit")]
    public float topTorchOffsetDistance = 0.14f;

	[Tooltip("Offset distance of the point light from surfaces")]
	public float lightOffsetDistance = 0.5f;

    [Space(5f)]

    [Header("Throw")]

	[Tooltip("Can throw the torch when handled")]
    public bool canThrow;

    [Tooltip("Launch force of the throw")]
	public float minLaunchForce = 0.1f;
	public float maxLaunchForce = 20f;
	public float minLaunchCameraAngle = 0f;
	public float maxLaunchCameraAngle = 130f;

    [Space(5f)]

    [Header("Crafting")]

	[Tooltip("Wait this value after pressing the craft button to get the torch prefab instantiate")]
	public float craftingDuration;

	[Tooltip("If the torch should spawn lit or not")]
	public bool startLit = true;

    [Space(5f)]

    [Header("Aim Preview Variables")]
    public float minThrowAngleOffset = 0f;
    public float maxThrowAngleOffset = 20f;
	[Range(0.1f, 10f)] public float previewLength = 10f;
	[Range(0.1f, 0.25f)] public float previewSmoothing = 0.1f;
	public LayerMask layersToIgnorePreview;

    [Space(5f)]

    [Header("Height Feedback")]

    [Tooltip("Torch color when height higher than lethal death")]
    public Color deathColor = new Color(255, 52, 52, 255);

	[Tooltip("Time the torch stay lit before despawning when beneath rope lentgh + lethal height")]
	public float deactivatingTime = 3f;

    [Space(5f)]

    [Header("Debug")]
	public bool activateBreakAnim = true;
    public GameObject torchBreakSFX;
	public GameObject torchHitSFX;
}
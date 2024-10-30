using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "TorchConfig", menuName = "Configs/Torch")]
public class TorchConfig : ScriptableObject
{
	[Header("Prefabs")]
	[Tooltip("Torch prefab that is instantiated when crafted")]
	public Torch pfTorch;


	[Header("Light")]
	[Tooltip("Intensity of the light")]
	public float lightIntensity;

	[Tooltip("Lit duration when on ground")]
	public float groundedLightDuration;

	[Tooltip("Time to light the torch")]
	public float lightStartupDuration;

    [Tooltip("Time to extinguish the torch")]
    public float extinguishDuration;

    public Material litMaterial;
    public Material unlitMaterial;

	public Color lightColor = new Color(255, 170, 85, 255);
	public Color deathColor = new Color(255, 52, 52, 255);

	[Header("Throw")]
	[Tooltip("Can throw the torch when handled")]
    public bool canThrow;

    [Tooltip("Launch force of the throw")]
	public float minLaunchForce = 0.1f;
	public float maxLaunchForce = 20f;
	public float minLaunchCameraAngle = 0f;
	public float maxLaunchCameraAngle = 130f;


	[Header("Crafting")]
	[Tooltip("Wait this value after pressing the craft button to get the torch prefab instantiate")]
	public float craftingDuration;


	[Header("Aim Preview Variables")]
    public float minThrowAngleOffset = 0f;
    public float maxThrowAngleOffset = 20f;
	[Range(0.1f, 10f)] public float previewLength = 10f;
	[Range(0.1f, 0.25f)] public float previewSmoothing = 0.1f;
	public LayerMask layersToIgnorePreview;

	[Header("Debug")]
	public bool activateBreakAnim = true;
    public GameObject torchBreakSFX;
}
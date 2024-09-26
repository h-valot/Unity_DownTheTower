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

	public Material litMaterial;

	[Tooltip("Time to light the torch")]
	public float lightStartupDuration;

	public Material unlitMaterial;

    [Tooltip("Time to extinguish the torch")]
    public float extinguishDuration;


	[Header("Throw")]
	[Tooltip("Can throw the torch when handled")]
    public bool canThrow;

    [Tooltip("Launch force of the throw")]
    public float launchForce;


	[Header("Crafting")]
	[Tooltip("Wait this value after pressing the craft button to get the torch prefab instantiate")]
	public float craftingDuration;
}
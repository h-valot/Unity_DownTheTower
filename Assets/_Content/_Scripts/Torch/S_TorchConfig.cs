using UnityEngine;

[CreateAssetMenu(fileName = "TorchConfig", menuName = "Configs/Torch")]
public class TorchConfig : ScriptableObject
{
    [Header("Torch Variables")]
    [Tooltip("Intensity of the light")]
    public int torchIntensity;

    [Tooltip("Time to craft the torch")]
    public int timeToCraft;

    [Tooltip("Lit duration when on ground")]
    public int litDuration;

    [Tooltip("Time to lit the torch")]
    public int timeToLit;

    [Tooltip("Time to unlit the torch")]
    public int timeToUnlit;

    [Tooltip("Can throw the torch when handled")]
    public bool canThrow;

    
}

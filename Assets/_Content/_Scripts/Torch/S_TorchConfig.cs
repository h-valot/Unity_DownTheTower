using UnityEngine;

[CreateAssetMenu(fileName = "TorchConfig", menuName = "Configs/Torch")]
public class TorchConfig : ScriptableObject
{
    [Header("Torch Variables")]
    [Tooltip("Intensity of the light")]
    public int torchIntensity;

    [Tooltip("Lit duration when on ground")]
    public int litDuration;

    [Tooltip("Can throw the torch when handled")]
    public bool canThrow;

    
}

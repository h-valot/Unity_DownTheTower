using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Sound : ScriptableObject
{
    public string soundName;
    public SoundType soundType;
    public List<AudioClip> clips;
    public bool random;

    [Header("Volume")]
    [Range(0, 1)] public float minVolume = 1f;
    [Range(0, 1)] public float maxVolume = 1f;

    [Header("Pitch")]
    [Range(0, 2)] public float minPitch = 1f;
    [Range(0, 2)] public float maxPitch = 1f;

}

public enum SoundType
{
    MUSIC,
    SFX
}
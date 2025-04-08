using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Settings", menuName = "Static Scriptable/Settings")]
public class SSO_Settings : ScriptableObject
{
	#region INPUTS

	[Title("Camera")]
    [InfoBox("Minimum value when setting sensitivity.", InfoMessageType.None)]
    public float MinSensitivity;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Maximum value when setting sensitivity.", InfoMessageType.None)]
    public float MaxSensitivity;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Default sensitivity value (between min and max pls).", InfoMessageType.None)]
    public float SensitivityValue;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Multiplier to adapt Y sensibility value to X sensitivity.", InfoMessageType.None)]
    public float SensitivityMultiplierY;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Multiplier to adapt to mouse sensitivity.", InfoMessageType.None)]
    public float SensitivityMouseMultiplier;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Default value for Camera inversion.", InfoMessageType.None)]
    public bool InvertAxisY;

    #endregion

    #region VOLUME

    [Title("Volume")]
    [InfoBox("Default decibel value for each audio output when startying the game.", InfoMessageType.None)]
    public float DefaultVolumeDB;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Minimum decibel value for each audio output.", InfoMessageType.None)]
    public float MinVolumeDB;

    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Maximum decibel value for each audio output.", InfoMessageType.None)]
    public float MaxVolumeDB;


    #endregion
}
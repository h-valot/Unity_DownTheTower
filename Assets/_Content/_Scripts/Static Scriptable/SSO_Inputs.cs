using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Inputs", menuName = "Static Scriptable/Inputs")]
public class SSO_Inputs : ScriptableObject
{
	#region LOOK

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
}
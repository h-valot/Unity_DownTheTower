using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Mushrooms", menuName = "Static Scriptable/Mushrooms")]
public class SSO_Mushrooms : ScriptableObject
{
    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Minimal velocity of object to trigger the mushroom explosion.", InfoMessageType.None)]
    public float MinimalVelocityToTrigger;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Layer used by the mushroom triggers.", InfoMessageType.None)]
    public LayerMask LayerToFindOverlappingTrigger;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Speed at which the deflate wave move (graphic).", InfoMessageType.None)]
    public float DeflateWaveSpeed;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Duration for each mushroom to deflate (graphic).", InfoMessageType.None)]
    public float DeflateDuration;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Duration at the end of the deflate wave w ile mushroom stay deadly.", InfoMessageType.None)]
    public float DeflateIdleDuration;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Duration of the safe state.", InfoMessageType.None)]
	public float SafeDuration;

    [FoldoutGroup("Explosion")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Duration of the growth state (overlap the end of the safe state).", InfoMessageType.None)]
    public float InflateDuration;

    [FoldoutGroup("Visual")]
    [PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
    [InfoBox("Distance to display particles.", InfoMessageType.None)]
    public float distanceDisplay;
}

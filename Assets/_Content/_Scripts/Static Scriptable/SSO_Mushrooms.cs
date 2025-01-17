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
    [InfoBox("Duration since mushrooms trigger to re-enable the trigger.", InfoMessageType.None)]
	public float Cooldown;
}

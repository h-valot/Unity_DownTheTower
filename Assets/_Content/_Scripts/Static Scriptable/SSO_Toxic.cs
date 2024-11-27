using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Toxic", menuName = "Static Scriptable/Toxic")]
public class SSO_Toxic : ScriptableObject
{
	[InfoBox("Duration since mushrooms trigger to re-enable the trigger.", InfoMessageType.None)]
	/// <summary> Duration since mushrooms trigger to re-enable the trigger. </summary>
	public float Cooldown;
}

using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Debug", menuName = "Static Scriptable/Debug")]
public class SSO_Debug : ScriptableObject
{
	#region TORCH

	[FoldoutGroup("Torch")]
	[InfoBox("If true, the default value of ActivateBreakAnim in the Torch SSO will be override with the following one.", InfoMessageType.None)]
	public bool OverrideActivateBreakAnim;
	[FoldoutGroup("Torch")]
	public bool ValueActivateBreakAnim;
	[FoldoutGroup("Torch")]
	public string FlavorActivateBreakAnim;

	[FoldoutGroup("Torch")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("If true, the default value of LightIntensity in the Torch SSO will be override with the following one.", InfoMessageType.None)]
	public bool OverrideLightIntensity;
	[FoldoutGroup("Torch")]
	public float ValueLightIntensity;
	[FoldoutGroup("Torch")]
	public string FlavorLightIntensity;

	[FoldoutGroup("Torch")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("If true, the default value of DesactivatingTime in the Torch SSO will be override with the following one.", InfoMessageType.None)]
	public bool OverrideDesactivatingTime;
	[FoldoutGroup("Torch")]
	public float ValueDesactivatingTime;
	[FoldoutGroup("Torch")]
	public string FlavorDesactivatingTime;


	#endregion
}


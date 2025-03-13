using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Interface", menuName = "Static Scriptable/Interface")]
public class SSO_Interface : ScriptableObject
{
	public Color HighlightColor;
	public Color DefaultColor;

	#region Tutorial panels

	[FoldoutGroup("Tutorial panels")]
	[InfoBox("Offset of the panel local position on the x-axis at appear animation start and disappear animation ends.", InfoMessageType.None)]
	public float TutoLocalXOffset;

	[FoldoutGroup("Tutorial panels")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of fade out and x-axis translation of tuto panels.", InfoMessageType.None)]
	public float TutoAppearDuration;

	[FoldoutGroup("Tutorial panels")]
	[PropertySpace(SpaceBefore = 15, SpaceAfter = 0)]
	[InfoBox("Duration of the color changement and fade in on tuto completed.", InfoMessageType.None)]
	public float TutoDisappearDuration;

	#endregion
}
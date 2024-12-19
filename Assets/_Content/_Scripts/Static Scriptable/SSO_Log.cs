using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Log", menuName = "Static Scriptable/Log")]
public class SSO_Log : ScriptableObject
{
	[Title("Title")]
	[HideLabel]
	[MultiLineProperty(2)]
	public string Header;

	[Title("Flavor")]
	[HideLabel]
	[MultiLineProperty(7)]
	public string Body;

	// DYNAMIC DATA
	[Title("Debug")]
	public bool IsDiscovered;
}
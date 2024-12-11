using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Guardian", menuName = "Static Scriptable/Guardian")]
public class SSO_Guardian : ScriptableObject
{
	[Title("Debug")]
	public Material AggroMaterial;
	public Material ScanMaterial;
	public Material DormantMaterial;

	[Title("Sight")]
	public float MaxRange;
    public LayerMask TargetLayerToIgnore;
}
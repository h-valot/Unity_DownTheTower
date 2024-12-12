using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Guardian", menuName = "Static Scriptable/Guardian")]
public class SSO_Guardian : ScriptableObject
{
	[Title("Debug")]
	public Material AggroMaterial;
	public Material PatrolMaterial;
	public Material DormantMaterial;

	[Title("Sight")]
	public float SightRange;

	public float PassiveRange;

    public LayerMask TargetLayerToIgnore;

	[PropertyRange(-1f, 1f)]
	public float AngleSight;
}
using Sirenix.OdinInspector;
using UnityEngine;

public class RopeInteractable : Interactable
{
	[FoldoutGroup("Internal references")][SerializeField] public SphereCollider SphereTrigger;
}
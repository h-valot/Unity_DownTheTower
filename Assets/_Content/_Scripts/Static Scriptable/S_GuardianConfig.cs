using UnityEngine;

[CreateAssetMenu(fileName = "GuardianConfig", menuName = "Static Scriptable/Guardian")]
public class GuardianConfig : ScriptableObject
{
    public LayerMask targetLayerToIgnore;
}
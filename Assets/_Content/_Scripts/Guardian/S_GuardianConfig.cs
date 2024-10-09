using UnityEngine;

[CreateAssetMenu(fileName = "GuardianConfig", menuName = "Configs/Guardian")]
public class GuardianConfig : ScriptableObject
{
    public LayerMask targetLayerToIgnore;
}
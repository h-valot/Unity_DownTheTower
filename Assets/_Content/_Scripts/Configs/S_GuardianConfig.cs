using UnityEngine;

[CreateAssetMenu(fileName = "GuardianConfig", menuName = "Configs/Guardian")]
public class GuardianConfig : ScriptableObject
{

    [Header("Cooldowns")]
    public float resetAggroCD;
    public float shiftToPatrolCD;
    public float shiftToPursuitCD;
    public float timeToDestroy;
    public float killTime;

    [Header("Graphics")]
    public Material aggroMaterial;
    public Material scanMaterial;
    public Material dormantMaterial;

    [Header("Layers")]
    public LayerMask targetLayerToIgnore;
}
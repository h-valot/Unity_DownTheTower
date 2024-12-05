using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Guardian", menuName = "Static Scriptable/Guardian")]
public class SSO_Guardian : ScriptableObject
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

    [Header("Debug Mode")]
    public bool debugMode;
}
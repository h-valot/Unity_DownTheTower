using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Foliages", menuName = "Static Scriptable/Foliages")]
public class SSO_Foliages : ScriptableObject
{
    [Range(-1f, 1f)]
    [InfoBox("Value compared to the result of the dot product between the normal and the up vector to determine if it is the wall.", InfoMessageType.None)]
    public float DotProductWall = 0.05f;
    [Range(-1f, 1f)]
    [InfoBox("Value compared to the result of the dot product between the normal and the up vector to determine if it is the floor.", InfoMessageType.None)]
    public float DotProductFloor = 0.95f;
    [Range(-1f,1f)]
    [InfoBox("Value compared to the result of the dot product between the normal and the up vector to determine if it is the ceilling.", InfoMessageType.None)]
    public float DotProductCeilling = -0.95f;
}

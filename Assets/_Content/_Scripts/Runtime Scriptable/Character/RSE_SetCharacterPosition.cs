using RuntimeScriptables;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_SetCharacterPosition", menuName = "RSE/Character/Set character position")]
public class RSE_SetCharacterPosition : RuntimeScriptableEvent<Vector3, Quaternion> { }
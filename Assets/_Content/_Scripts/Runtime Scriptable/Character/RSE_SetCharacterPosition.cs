using RuntimeScriptables;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_SetCharacterPosition", menuName = "Runtime Scriptable/Character/Set character position")]
public class RSE_SetCharacterPosition : RuntimeScriptableEvent<Vector3, Quaternion> { }
using RuntimeScriptables;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_InitializeCamera", menuName = "Runtime Scriptable/Manager/Initialize camera")]
public class RSE_InitializeCamera : RuntimeScriptableEvent<Transform, Transform, Quaternion> { }

using RuntimeScriptables;
using UnityEngine;

[CreateAssetMenu(fileName = "RSE_CraftTorch", menuName = "Runtime Scriptable/Inputs/Craft")]
public class RSE_Craft : RuntimeScriptableEvent<CraftType, bool> {}
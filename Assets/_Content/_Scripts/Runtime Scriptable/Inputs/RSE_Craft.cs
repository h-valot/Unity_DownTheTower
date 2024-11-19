using RuntimeScriptables;
using UnityEngine;
using static OldCharacterMotor;

[CreateAssetMenu(fileName = "RSE_CraftTorch", menuName = "RSE/Inputs/CraftTorch")]
public class RSE_Craft : RuntimeScriptableEvent<CraftType, bool> {}
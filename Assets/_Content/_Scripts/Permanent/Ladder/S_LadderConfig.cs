using UnityEngine;

[CreateAssetMenu(fileName = "LadderConfig", menuName = "Configs/Ladder")]
public class LadderConfig : ScriptableObject
{
	[Header("Prefabs")]
	public PreLadder pfPreLadder;
	public Ladder pfLadder;

	[Header("Deploy")]
	public float maxHeight = 4f;
	public float additionalRaycastHeight = 0.5f;
	public LayerMask layersToIgnore;

	[Header("Camera")]
    [Tooltip("1f = 0° ; 0f = 90° ; -1f = 180°")]
    public float maxGroundAngle = 0.5f;
    public float maxDistFromCamera = 6f;
}
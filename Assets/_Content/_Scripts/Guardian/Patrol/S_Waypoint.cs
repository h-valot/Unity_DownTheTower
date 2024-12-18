using Sirenix.OdinInspector;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	[InfoBox("If true, the following value will override the one filled in the SSO_Guardian.", InfoMessageType.None)]
	[InfoBox("Duration the guardian waits when it reaches the next waypoint. NOTE: This value depends on when the guardian is considered to have reached the waypoint (tweakable with Waypoint Distance Tolerance in SSO_Guardian).", InfoMessageType.None)]
	[SerializeField] public bool OverrideWaitDurationOnWaypointReached;
	[ShowIf("OverrideWaitDurationOnWaypointReached")][SerializeField] public float WaitDurationOnWaypointReached;
	
	public Vector3 Position => transform.position;
}
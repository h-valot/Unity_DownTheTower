using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
	[InfoBox("To populate the waypoints list, you have to drag'n'drop PF_Waypoint (found in Prefabs/Guardian/Patrol) under this game object.", InfoMessageType.None)]
	[InfoBox("On game starts, this script will gather all waypoints in their children.", InfoMessageType.None)]
	public string _;

	[HideInInspector] public List<Waypoint> Waypoints = new List<Waypoint>();

	private void Awake()
	{
		PopulateWaypoints();
	}

	private void PopulateWaypoints()
	{
		Waypoints.Clear();
		foreach (Transform child in transform)
		{
			if (child.TryGetComponent<Waypoint>(out var waypoint))
			{
				Waypoints.Add(waypoint);
			}
		}
	}
}
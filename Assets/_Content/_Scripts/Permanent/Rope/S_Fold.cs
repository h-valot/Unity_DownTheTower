using System;
using UnityEngine;

[Serializable]
public class Fold 
{
	public Vector3 Position;
	public Vector3 Normal;

	public Fold(Vector3 newPosition)
	{
		Position = newPosition;
	}

	public Fold(Vector3 newPosition, Vector3 newNormal)
	{
		Position = newPosition;
		Normal = newNormal;
	}
}
using System;
using UnityEngine;

[Serializable]
public struct Candidate
{
	public int Id;
	public Vector3 Position;

	public Candidate(int id, Vector3 position)
	{
		Id = id;
		Position = position;
	}
}
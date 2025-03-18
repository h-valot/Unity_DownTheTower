using System;
using UnityEngine;

[Serializable]
public class Candidate
{
	public int Id = -1;
	public Vector3 Position;

	public bool IsUpdated;
	public bool IsAggroedLately;
	public bool IsBan;

	public Candidate(int id, Vector3 position)
	{
		Id = id;
		Position = position;
		IsUpdated = true;
	}

	public void Update(Vector3 position)
	{
		Position = position;
		IsUpdated = true;
	}
}
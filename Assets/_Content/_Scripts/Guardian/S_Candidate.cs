using System;
using UnityEngine;

[Serializable]
public class Candidate : MonoBehaviour
{
	public Vector3 Position;
	public CandidateType Type;

	public Candidate(Vector3 position, CandidateType type)
	{
		Position = position;
		Type = type;
	}
}

public enum CandidateType
{
	CHARACTER = 0,
	TORCH
}
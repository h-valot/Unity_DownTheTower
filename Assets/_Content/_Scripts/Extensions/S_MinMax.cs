using System;
using UnityEngine;

[Serializable]
public class MinMaxInt 
{
	public int Min;
	public int Max;

	public MinMaxInt() { }

	public MinMaxInt(int min, int max)
	{
		Min = min;
		Max = max;
	}

	public MinMaxInt(Vector2Int vector2Int)
	{
		Min = vector2Int.x;
		Max = vector2Int.y;
	}

	public int GetRandomValue => UnityEngine.Random.Range(Min, Max + 1);
}

[Serializable]
public class MinMaxFloat
{
	public float Min;
	public float Max;

	public MinMaxFloat() { }

	public MinMaxFloat(float min, float max)
	{
		Min = min;
		Max = max;
	}

	public MinMaxFloat(Vector2 vector2)
	{
		Min = vector2.x;
		Max = vector2.y;
	}

	public float GetRandomValue => UnityEngine.Random.Range(Min, Max);
}
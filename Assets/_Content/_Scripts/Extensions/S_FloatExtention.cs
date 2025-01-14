using UnityEngine;

public enum CuttingType
{
	ROUND = 0,
	FLOOR = 1,
	CEIL = 2
}

public static class FloatExtention
{
	/// <summary>
	/// Return the given float with the given amout of digit.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static float CutDigits(this float number, int digitAmount)
	{
		return number.CutDigits(digitAmount, CuttingType.ROUND);
	}

	/// <summary>
	/// Return the given float with the given amout of digit.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	/// <param name="type">Type of cutting. ROUND: digits are rounded to nearest, FLOOR: digits are rounded down, CEIL: digits are rounded up.</param>
	public static float CutDigits(this float number, int digitAmount, CuttingType type)
	{
		float scalar = Mathf.Pow(10f, digitAmount);
		if (type == CuttingType.ROUND) return Mathf.Round(number * scalar) / scalar;
		else if (type == CuttingType.FLOOR) return Mathf.Floor(number * scalar) / scalar;
		else return Mathf.Ceil(number * scalar) / scalar;
	}
}
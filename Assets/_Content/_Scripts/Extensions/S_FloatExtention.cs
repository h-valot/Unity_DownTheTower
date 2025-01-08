using UnityEngine;

public static class FloatExtention
{
	/// <summary>
	/// Return the given float with the given amout of digit rounded.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static float CutDigitsRound(this float number, int digitAmount = 0)
	{
		float scalar = Mathf.Pow(10f, digitAmount);
		return Mathf.Round(number * scalar) / scalar;
	}

	/// <summary>
	/// Return the given float with the given amout of digit rounded down.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static float CutDigitsFloor(this float number, int digitAmount = 0)
	{
		float scalar = Mathf.Pow(10f, digitAmount);
		return Mathf.Floor(number * scalar) / scalar;
	}

	/// <summary>
	/// Return the given float with the given amout of digit rounded up.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static float CutDigitsCeil(this float number, int digitAmount = 0)
	{
		float scalar = Mathf.Pow(10f, digitAmount);
		return Mathf.Ceil(number * scalar) / scalar;
	}
}
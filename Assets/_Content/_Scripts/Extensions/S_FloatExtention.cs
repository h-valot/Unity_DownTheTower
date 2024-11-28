using UnityEngine;

public static class FloatExtention
{
	/// <summary>
	/// Return the given float with the given amout of digit.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static float CutDigits(this float number, int digitAmount = 0)
	{
		float scalar = Mathf.Pow(10f, digitAmount);
		return Mathf.Round(number * scalar) / scalar;
	}
}
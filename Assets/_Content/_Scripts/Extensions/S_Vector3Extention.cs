using UnityEngine;

public static class Vector3Extention
{
	/// <summary>
	/// 	return the given vector3's x, y and z with the given amout of digit.
	/// </summary>
	/// <param name="digitAmount">number of digit left after the comma. 0 by default = similar to floor to int</param>
	public static Vector3 CutDigits(this Vector3 vector, int digitAmount = 0)
	{
		return new Vector3(
			vector.x.CutDigits(digitAmount),
			vector.y.CutDigits(digitAmount),
			vector.z.CutDigits(digitAmount)
		);
	}
}
using System;
using UnityEngine;

public static class Vector3Extention
{
	/// <summary>
	/// 	Return the given vector3's x, y and z with the given amout of digit.
	/// </summary>
	/// <param name="digitAmount">Number of digit left after the comma. 0 by default = similar to floor to int.</param>
	public static Vector3 CutDigits(this Vector3 vector, int digitAmount = 0)
	{
		return new Vector3(
			vector.x.CutDigits(digitAmount),
			vector.y.CutDigits(digitAmount),
			vector.z.CutDigits(digitAmount)
		);
	}

	/// <summary>
	/// 	Return the position of a point on a cercle with the given origin and radius perpendicular to the given axis.
	/// 	From the given starting point, creates a rotation which rotates given angle degree around the given axis of the cercle.
	/// </summary>
	/// <param name="angle">Creates a rotation in degrees around the given axis in the given direction.</param>
	/// <param name="axis">Axis perpendicular to the cercle.</param>
	/// <param name="direction">Direction of the rotation. Must be perpendicular to the given direction.</param>
	/// <param name="origin">Center of the cercle.</param>
	/// <param name="radius">Radius of the cercle.</param>
	/// <param name="starting">Starting point from which the rotation starts.</param>
	/// <returns>The position on the cercle from the rotation.</returns>
	public static Vector3 GetPositionOnCercle(float angle, Vector3 axis, Vector3 direction, Vector3 origin, float radius, Vector3 starting)
	{
		Vector3 rotatedPosition = starting + Quaternion.AngleAxis(angle, axis) * direction;
		Vector3 snapPoint = origin + (rotatedPosition - origin).normalized * radius;
		return snapPoint;
	}
}
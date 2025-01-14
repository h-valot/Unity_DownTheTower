using UnityEngine;

public static class Matha
{
	public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

    public static float Remap(float inMin, float inMax, float outMin, float outMax, float value)
    {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }
}
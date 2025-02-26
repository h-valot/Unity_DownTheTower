using UnityEngine;
using static FoliageBatch;

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

    public static float RemapClamped(float inMin, float inMax, float outMin, float outMax, float value)
    {
        return Mathf.Clamp(outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin), outMin, outMax);
    }

    public static Vector3 RandomRangeVector3(Vector3 min, Vector3 max)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }
}
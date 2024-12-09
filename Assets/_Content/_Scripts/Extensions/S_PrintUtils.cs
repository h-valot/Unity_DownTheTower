using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintUtils
{
    public static string VectorPrecise(Vector3 vector)
    {
        return "(" + vector.x.ToString() + ", " + vector.y.ToString() + ", " + vector.z.ToString() + ")";
    }

    public static string VectorPrecise(Vector4 vector)
    {
        return "(" + vector.x.ToString() + ", " + vector.y.ToString() + ", " + vector.z.ToString() + ", " + vector.w.ToString() + ")";
    }

    public static string Matrix4x4(Matrix4x4 matrix)
    {
        return "Position: " + matrix.GetPosition().ToString() + ", Rotation (EULR): " + matrix.rotation.eulerAngles.ToString() + ", Scale: " + matrix.lossyScale.ToString();
    }
}

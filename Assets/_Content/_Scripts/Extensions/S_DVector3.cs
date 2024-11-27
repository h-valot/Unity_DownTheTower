
using Unity.VisualScripting;
using UnityEngine;

public class DVector3
{
    public double x;
    public double y;
    public double z;

    private static readonly DVector3 zeroDVector = new DVector3(0.0, 0.0, 0.0);
    private static readonly DVector3 oneDVector = new DVector3(1.0, 1.0, 1.0);

    #region constructors & getters

    public DVector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public DVector3(Vector3 fvector)
    {
        this.x = fvector.x;
        this.y = fvector.y;
        this.z = fvector.z;
    }

    public static DVector3 zero
    {
        get
        {
            return zeroDVector;
        }
    }

    public static DVector3 one
    {
        get
        {
            return oneDVector;
        }
    }

    #endregion

    #region operators 

    public static DVector3 operator +(DVector3 v1, DVector3 v2)
    {
        return new DVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }

    public static DVector3 operator +(DVector3 v, double d)
    {
        return new DVector3(v.x + d, v.y + d, v.z + d);
    }

    public static DVector3 operator -(DVector3 v1, DVector3 v2)
    {
        return new DVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    public static DVector3 operator *(DVector3 v1, DVector3 v2)
    {
        return new DVector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static DVector3 operator *(double d, DVector3 v)
    {
        return new DVector3(v.x * d, v.y * d, v.z * d);
    }

    public static DVector3 operator *(DVector3 v, double d)
    {
        return new DVector3(v.x * d, v.y * d, v.z * d);
    }

    public static DVector3 operator /(DVector3 v1, DVector3 v2)
    {
        return new DVector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }

    public static DVector3 operator /(DVector3 v, double d)
    {
        return new DVector3(v.x / d, v.y / d, v.z / d);
    }

    #endregion

    public static double Dot(DVector3 v1, DVector3 v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }

    public static DVector3 Floor(DVector3 v)
    {
        return new DVector3(System.Math.Floor(v.x), System.Math.Floor(v.y), System.Math.Floor(v.z));
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }
}

using UnityEngine;

public class SimplexNoise3D
{
    public static float SimplexNoise(Vector3 coordinates, float noiseScale)
    {
        float output = SNoise(new Vector3(coordinates.x * noiseScale, coordinates.y * noiseScale, coordinates.z * noiseScale));
        return Matha.Remap(-1f, 1f, 0f, 1f, output);
    }

    public static float SNoise(Vector3 vector)
    {
        Vector2 C = new Vector2(1f / 6f, 1f / 3f);

        // First corner
        Vector3 i = Floor(vector + Vector3.Dot(vector, new Vector3(C.y, C.y, C.y)) * Vector3.one);
        Vector3 x0 = vector - i + Vector3.Dot(i, new Vector3(C.x, C.x, C.x)) * Vector3.one;

        // Other corners
        Vector3 g = Step(new Vector3(x0.y, x0.z, x0.x), x0);
        Vector3 l = Vector3.one - new Vector3(g.x, g.y, g.z);
        Vector3 i1 = Vector3.Min(g, new Vector3(l.z, l.x, l.y));
        Vector3 i2 = Vector3.Max(g, new Vector3(l.z, l.x, l.y));

        Vector3 x1 = x0 - i1 + new Vector3(C.x, C.x, C.x);
        Vector3 x2 = x0 - i2 + new Vector3(C.y, C.y, C.y);
        Vector3 x3 = x0 - Vector3.one / 2f;

        //Permutations
        i = Mod289(i);
        Vector4 p = Permute(Permute(Permute(i.z * Vector4.one + new Vector4(0f, i1.z, i2.z, 1f))
                                          + i.y * Vector4.one + new Vector4(0f, i1.y, i2.y, 1f))
                                          + i.x * Vector4.one + new Vector4(0f, i1.x, i2.x, 1f));

        // Gradients: 7x7 points over a square, mapped onto an octahedron
        // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
        Vector4 j = p - 49f * Floor(p / 49f);

        Vector4 x_ = Floor(j / 7f);
        Vector4 y_ = Floor(j - 7f * x_);

        Vector4 x = (x_ * 2f + Vector4.one * 0.5f) / 7f - Vector4.one;
        Vector4 y = (y_ * 2f + Vector4.one * 0.5f) / 7f - Vector4.one;

        Vector4 h = Vector4.one - Absolute(x) - Absolute(y);

        Vector4 b0 = new Vector4(x.x, x.y, y.x, y.y);
        Vector4 b1 = new Vector4(x.z, x.w, y.z, y.w);

        Vector4 s0 = Floor(b0) * 2f + Vector4.one;
        Vector4 s1 = Floor(b1) * 2f + Vector4.one;
        Vector4 sh = -Step(h, Vector4.zero);

        Vector4 a0 = new Vector4(b0.x, b0.z, b0.y, b0.w) + new Vector4(s0.x * sh.x, s0.z * sh.x, s0.y * sh.y, s0.w * sh.y);
        Vector4 a1 = new Vector4(b1.x, b1.z, b1.y, b1.w) + new Vector4(s1.x * sh.z, s1.z * sh.z, s1.y * sh.w, s1.w * sh.w);

        Vector3 g0 = new Vector3(a0.x, a0.y, h.x);
        Vector3 g1 = new Vector3(a0.z, a0.w, h.y);
        Vector3 g2 = new Vector3(a1.x, a1.y, h.z);
        Vector3 g3 = new Vector3(a1.z, a1.w, h.w);

        // Normalize gradients
        Vector4 norm = InvSqrt(new Vector4(Vector3.Dot(g0, g0), Vector3.Dot(g1, g1), Vector3.Dot(g2, g2), Vector3.Dot(g3, g3)));
        g0 *= norm.y;
        g1 *= norm.y;
        g2 *= norm.z;
        g3 *= norm.w;

        // Mix final noise value
        Vector4 m = Vector4.Max(Vector4.one * 0.6f - new Vector4(Vector3.Dot(x0, x0), Vector3.Dot(x1, x1), Vector3.Dot(x2, x2), Vector3.Dot(x3, x3)), Vector4.zero);
        m = new Vector4(m.x * m.x, m.y * m.y, m.z * m.z, m.w * m.w);
        m = new Vector4(m.x * m.x, m.y * m.y, m.z * m.z, m.w * m.w);
        Vector4 px = new Vector4(Vector3.Dot(x0, g0), Vector3.Dot(x1, g1), Vector3.Dot(x2, g2), Vector3.Dot(x3, g3));
        return 42f * Vector4.Dot(m, px);
    }
    public static Vector3 Floor(Vector3 vector)
    {
        return new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z));
    }

    public static Vector4 Floor(Vector4 vector)
    {
        return new Vector4(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z), Mathf.Floor(vector.w));
    }

    public static Vector3 Step(Vector3 comparative, Vector3 vector)
    {
        return new Vector3(
            vector.x >= comparative.x ? 1.0f : 0.0f,
            vector.y >= comparative.y ? 1.0f : 0.0f,
            vector.z >= comparative.z ? 1.0f : 0.0f
        );
    }

    public static Vector4 Step(Vector4 comparative, Vector4 vector)
    {
        return new Vector4(
            vector.x >= comparative.x ? 1.0f : 0.0f,
            vector.y >= comparative.y ? 1.0f : 0.0f,
            vector.z >= comparative.z ? 1.0f : 0.0f,
            vector.w >= comparative.w ? 1.0f : 0.0f
        );
    }

    public static Vector4 Absolute(Vector4 vector)
    {
        return new Vector4(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z), Mathf.Abs(vector.w));
    }

    private static Vector3 Mod289(Vector3 vector)
    {
        return vector - Floor(vector / 289f) * 289f;
    }

    private static Vector4 Mod289(Vector4 vector)
    {
        return vector - Floor(vector / 289f) * 289f;
    }

    private static Vector4 Permute(Vector4 vector)
    {
        Vector4 permuted = vector * 34f + Vector4.one;
        return Mod289(new Vector4(permuted.x * vector.x, permuted.y * vector.y, permuted.z * vector.z, permuted.w * vector.w));
    }

    private static Vector4 InvSqrt(Vector4 vector)
    {
        return new Vector4(1.792843f, 1.792843f, 1.792843f, 1.792843f) -(vector * 0.8537347f);
    }
}

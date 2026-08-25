using UnityEngine;

/// <summary>
/// 3x3 matrix + vector operations.
/// Internal convention matches Python: z is "up" (Earth axes), NOT Unity's y-up.
/// Converts to unity axes when using transforms
/// </summary>
public struct Mat3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;

    public static Mat3 FromRows(Vector3 r0, Vector3 r1, Vector3 r2)
    {
        return new Mat3
        {
            m00 = r0.x, m01 = r0.y, m02 = r0.z,
            m10 = r1.x, m11 = r1.y, m12 = r1.z,
            m20 = r2.x, m21 = r2.y, m22 = r2.z
        };
    }

    public Vector3 MultiplyVector(Vector3 v)
    {
        return new Vector3(
            m00 * v.x + m01 * v.y + m02 * v.z,
            m10 * v.x + m11 * v.y + m12 * v.z,
            m20 * v.x + m21 * v.y + m22 * v.z
        );
    }

    public Mat3 Transpose()
    {
        return new Mat3
        {
            m00 = m00, m01 = m10, m02 = m20,
            m10 = m01, m11 = m11, m12 = m21,
            m20 = m02, m21 = m12, m22 = m22
        };
    }

    public static Mat3 operator *(Mat3 a, Mat3 b)
    {
        return new Mat3
        {
            m00 = a.m00*b.m00 + a.m01*b.m10 + a.m02*b.m20,
            m01 = a.m00*b.m01 + a.m01*b.m11 + a.m02*b.m21,
            m02 = a.m00*b.m02 + a.m01*b.m12 + a.m02*b.m22,

            m10 = a.m10*b.m00 + a.m11*b.m10 + a.m12*b.m20,
            m11 = a.m10*b.m01 + a.m11*b.m11 + a.m12*b.m21,
            m12 = a.m10*b.m02 + a.m11*b.m12 + a.m12*b.m22,

            m20 = a.m20*b.m00 + a.m21*b.m10 + a.m22*b.m20,
            m21 = a.m20*b.m01 + a.m21*b.m11 + a.m22*b.m21,
            m22 = a.m20*b.m02 + a.m21*b.m12 + a.m22*b.m22,
        };
    }
}
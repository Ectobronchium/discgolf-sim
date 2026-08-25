using UnityEngine;

/// <summary>
/// Direct port of transforms.py. Axes: 1=Earth, 2=Body, 3=Zero side-slip, 4=Wind.
/// Angles in radians throughout, matching Python.
/// </summary>
public static class Transforms
{
    // Earth -> Body. attitude = (phi, theta, psi)
    public static Mat3 T_12(Vector3 attitude)
    {
        float phi = attitude.x, theta = attitude.y, psi = attitude.z;
        float cph = Mathf.Cos(phi), sph = Mathf.Sin(phi);
        float cth = Mathf.Cos(theta), sth = Mathf.Sin(theta);
        float cps = Mathf.Cos(psi), sps = Mathf.Sin(psi);

        return Mat3.FromRows(
            new Vector3(cth*cps, sph*sth*cps - cph*sps, cph*sth*cps + sph*sps),
            new Vector3(cth*sps, sph*sth*sps + cph*cps, cph*sth*sps - sph*cps),
            new Vector3(-sth, sph*cth, cph*cth)
        );
    }

    // Body -> Zero side-slip. Rotation around z by beta.
    public static Mat3 T_23(float beta)
    {
        float c = Mathf.Cos(beta), s = Mathf.Sin(beta);
        return Mat3.FromRows(
            new Vector3(c, -s, 0),
            new Vector3(s, c, 0),
            new Vector3(0, 0, 1)
        );
    }

    // Zero side-slip -> Wind. Rotation around y by alpha.
    public static Mat3 T_34(float alpha)
    {
        float c = Mathf.Cos(alpha), s = Mathf.Sin(alpha);
        return Mat3.FromRows(
            new Vector3(c, 0, -s),
            new Vector3(0, 1, 0),
            new Vector3(s, 0, c)
        );
    }

    // Earth -> Wind (composite)
    public static Vector3 T_14(Vector3 vec, Vector3 attitude, float beta, float alpha)
    {
        Vector3 v2 = T_12(attitude).MultiplyVector(vec);
        Vector3 v3 = T_23(beta).MultiplyVector(v2);
        Vector3 v4 = T_34(alpha).MultiplyVector(v3);
        return v4;
    }

    // Body -> Earth (inverse of T_12)
    public static Mat3 T_21(Vector3 attitude) => T_12(attitude).Transpose();

    // Zero side-slip -> Body (inverse of T_23)
    public static Mat3 T_32(float beta) => T_23(beta).Transpose();

    // Wind -> Zero side-slip (inverse of T_34)
    public static Mat3 T_43(float alpha) => T_34(alpha).Transpose();

    // Wind -> Earth (composite inverse)
    public static Vector3 T_41(Vector3 vec, Vector3 attitude, float beta, float alpha)
    {
        Vector3 v3 = T_43(alpha).MultiplyVector(vec);
        Vector3 v2 = T_32(beta).MultiplyVector(v3);
        Vector3 v1 = T_21(attitude).MultiplyVector(v2);
        return v1;
    }

    // Body-rate -> Earth angular velocity (used for roll integration)
    public static Vector3 T_31(Vector3 vec, Vector3 attitude, float beta)
    {
        Vector3 v2 = T_32(beta).MultiplyVector(vec);
        Vector3 v1 = T_21(attitude).MultiplyVector(v2);
        return v1;
    }
}
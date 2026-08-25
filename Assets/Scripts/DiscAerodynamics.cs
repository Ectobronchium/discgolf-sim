using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class DiscDataRaw
{
    public float diameter;
    public float J_xy;
    public float J_z;
    public float[] alpha;
    public float[] Cl;
    public float[] Cd;
    public float[] Cm;
}

public class DiscAerodynamics
{
    public float Diameter { get; private set; }
    public float Mass { get; private set; }
    public float Area { get; private set; }
    public float I_xy { get; private set; }
    public float I_z { get; private set; }

    // Expanded tables after _flip in degrees
    float[] _alpha, _Cl, _Cd, _Cm;

    public DiscAerodynamics(string discName, float mass)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Discs", discName + ".json");
        string json = File.ReadAllText(path);
        DiscDataRaw raw = JsonConvert.DeserializeObject<DiscDataRaw>(json);

        Diameter = raw.diameter;
        Mass = mass;
        Area = Mathf.PI * Diameter * Diameter / 4f;
        I_xy = mass * raw.J_xy;
        I_z = mass * raw.J_z;

        Flip(raw.alpha, raw.Cl, raw.Cd, raw.Cm,
             out _alpha, out _Cl, out _Cd, out _Cm);
    }

    // Direct port of _flip() from projectile.py
    void Flip(float[] a, float[] cl, float[] cd, float[] cm,
              out float[] a2, out float[] cl2, out float[] cd2, out float[] cm2)
    {
        int n = a.Length;

        // Find index of value closest to 0 — mirrors np.argmin(abs(a))
        int idx = 0;
        float minAbs = Mathf.Abs(a[0]);
        for (int i = 1; i < n; i++)
        {
            float v = Mathf.Abs(a[i]);
            if (v < minAbs) { minAbs = v; idx = i; }
        }

        a2  = new float[2 * n];
        cl2 = new float[2 * n];
        cd2 = new float[2 * n];
        cm2 = new float[2 * n];

        // Copy original data into middle of expanded array
        for (int i = 0; i < n; i++)
        {
            a2 [idx + i] = a [i];
            cl2[idx + i] = cl[i];
            cd2[idx + i] = cd[i];
            cm2[idx + i] = cm[i];
        }

        // Fill lower extension, antisymmetric for Cl/Cm, symmetric for Cd
        for (int i = 0; i < idx; i++)
        {
            a2 [i] = -(180f + a[idx - i]);
            cl2[i] = -cl[idx - i];
            cd2[i] =  cd[idx - i];
            cm2[i] = -cm[idx - i];
        }

        // Fill upper extension
        for (int i = idx + n; i < 2 * n; i++)
        {
            int src = idx + 2*n - i - 2;
            a2 [i] =  180f - a [src];
            cl2[i] = -cl[src];
            cd2[i] =  cd[src];
            cm2[i] = -cm[src];
        }
    }

    // Port of _normalize_angle() keeps alpha in (-pi, pi)
    float NormalizeAngleRad(float rad)
    {
        return Mathf.Atan2(Mathf.Sin(rad), Mathf.Cos(rad));
    }

    // Port of scipy interp1d(kind='linear') assumes x is sorted ascending
    float Interp(float[] x, float[] y, float xi)
    {
        if (xi <= x[0]) return y[0];
        if (xi >= x[x.Length - 1]) return y[x.Length - 1];

        int lo = 0, hi = x.Length - 1;
        while (hi - lo > 1)
        {
            int mid = (lo + hi) / 2;
            if (x[mid] <= xi) lo = mid;
            else hi = mid;
        }

        float t = (xi - x[lo]) / (x[hi] - x[lo]);
        return y[lo] + t * (y[hi] - y[lo]);
    }

    public float Cl(float alphaRad)
    {
        float deg = NormalizeAngleRad(alphaRad) * Mathf.Rad2Deg;
        return Interp(_alpha, _Cl, deg);
    }

    public float Cd(float alphaRad)
    {
        float deg = NormalizeAngleRad(alphaRad) * Mathf.Rad2Deg;
        return Interp(_alpha, _Cd, deg);
    }

    public float Cm(float alphaRad)
    {
        float deg = NormalizeAngleRad(alphaRad) * Mathf.Rad2Deg;
        return Interp(_alpha, _Cm, deg);
    }
}
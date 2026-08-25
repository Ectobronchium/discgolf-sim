using UnityEngine;

public class DiscFlightModel
{
    public readonly DiscAerodynamics aero;
    public readonly float omega;

    const float g = -9.81f;
    const float rho = 1.225f;

    public DiscFlightModel(DiscAerodynamics aero, float omega)
    {
        this.aero  = aero;
        this.omega = omega;
    }

    public void Forces(
                        Vector3 position, 
                        Vector3 velocity, 
                        Vector3 attitude, 
                        Vector3 wind, 
                        out float alpha, 
                        out float beta,
                        out float Fd, 
                        out float Fl, 
                        out float M, 
                        out Vector3 g4)
    {
        Vector3 urel = velocity - wind;

        Vector3 u2 = Transforms.T_12(attitude).MultiplyVector(urel);
        beta = -Mathf.Atan2(u2.y, u2.x);

        Vector3 u3 = Transforms.T_23(beta).MultiplyVector(u2);
        alpha = -Mathf.Atan2(u3.z, u3.x);

        Vector3 u4 = Transforms.T_34(alpha).MultiplyVector(u3);

        Vector3 gravityEarth = new Vector3(0f, 0f, aero.Mass * g);
        g4 = Transforms.T_14(gravityEarth, attitude, beta, alpha);

        float q = 0.5f * rho * u4.x * u4.x;
        float S = aero.Area;
        float D = aero.Diameter;

        Fd = q * S * aero.Cd(alpha);
        Fl = q * S * aero.Cl(alpha);
        M = q * S * D * aero.Cm(alpha);
    }

    public void Advance(Vector3 position, 
                        Vector3 velocity, 
                        Vector3 attitude,
                        Vector3 wind,
                        out Vector3 velocityOut, 
                        out Vector3 accelerationOut,
                        out Vector3 angVelOut)
    {
        Forces(position, velocity, attitude, wind,
               out float alpha, out float beta,
               out float Fd, out float Fl, out float M, out Vector3 g4);

        float m = aero.Mass;

        Vector3 acc4 = new Vector3(
            (-Fd + g4.x) / m,
            ( g4.y) / m,
            ( Fl + g4.z) / m
        );

        float   dphidt  = -M / (omega * (aero.I_xy - aero.I_z));
        Vector3 angvel3 = new Vector3(dphidt, 0f, 0f);

        accelerationOut = Transforms.T_41(acc4, attitude, beta, alpha);
        angVelOut = Transforms.T_31(angvel3, attitude, beta);
        velocityOut = velocity;
    }
}
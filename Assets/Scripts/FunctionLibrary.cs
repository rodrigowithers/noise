using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate Vector3 MathFunction(float u, float v, float time);

    public static MathFunction GetFunction(FunctionName name)
    {
        try
        {
            return _functions[(int) name];
        }
        catch
        {
            return Wave;
        }
    }

    public enum FunctionName
    {
        Wave,
        MultiWave,
        Ripple,
        Sphere,
        Torus
    }

    private static readonly MathFunction[] _functions =
    {
        Wave,
        MultiWave,
        Ripple,
        Sphere,
        Torus
    };

    public static Vector3 Morph(float u, float v, float t, MathFunction from, MathFunction to, float morph)
    {
        return Vector3.Lerp(from(u, v, t), to(u, v, t), morph);
    }

    private static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;

        return p;
    }

    private static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += Sin(2f * PI * (v + t)) * 0.5f;
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= 1f / 2.5f;
        p.y *= 0.1f + Sin(t);
        p.z = v;

        return p;
    }

    private static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);

        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= (1f + 10f * d);
        p.y *= 0.1f + Sin(t);

        p.z = v;

        return p;
    }

    private static Vector3 Circle(float u, float v, float t)
    {
        Vector3 p = Vector3.zero;
        p.x = Sin(u * PI);
        p.y = v;
        p.z = Cos(u * PI);

        return p;
    }

    private static Vector3 Sphere(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v - t));
        float s = r * Cos(0.5f * PI * v);

        Vector3 p = Vector3.zero;
        p.x = s * Cos(u * PI);
        p.y = r * Sin(0.5f * v * PI);
        p.z = s * Sin(u * PI);

        return p;
    }

    private static Vector3 Torus(float u, float v, float t)
    {
        float r1 = 0.75f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.25f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));

        float s = r1 + r2 * Cos(PI * v);

        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);

        return p;
    }
}
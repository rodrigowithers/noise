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
        Ripple
    }

    private static readonly MathFunction[] _functions =
    {
        Wave,
        MultiWave,
        Ripple
    };

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
        p.z = v;

        return p;
    }
}
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate float MathFunction(float x, float z, float time);

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

    private static float Wave(float x, float z, float t)
    {
        return Sin(PI * (x + z + t));
    }

    private static float MultiWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + 0.5f * t));
        y += Sin(2f * PI * (z + t)) * 0.5f;
        y += Sin(PI * (x + z + 0.25f * t));

        return y * (1f / 2.5f);
    }

    private static float Ripple(float x, float z, float t)
    {
        float d = Sqrt(x * x + z * z);
        float y = Sin(PI * (4f * d - t));

        return y / (1f + 10f * d);
    }
}
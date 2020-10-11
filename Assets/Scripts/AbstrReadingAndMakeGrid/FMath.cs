using System;
using System.Runtime.CompilerServices;

public class FMath
{
    public const float EPS = 1e-18f;

    public const float PI = (float)Math.PI;

    public const float Rad2Deg = 180.0f / PI;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SafeDiv(float n, float d, float eps = EPS)
    {
        if (Math.Abs(d) < eps) return 0.0f;
        return n / d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignOrZero(float v)
    {
        if (v == 0) return 0;
        return Math.Sign(v);        //negative or positive sign of number
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public class CreateAspectMap : AbstrMakeImilorGrid
{
    protected override bool OnChange()
    {
        return currentColorMode != coloredGradient;
    }

    protected override void CreateMap()
    {

        Texture2D aspectMap = new Texture2D(gridSizeXactual, gridSizeYactual);

        for (int y = 0; y < gridSizeYactual; y++)
        {
            for (int x = 0; x < gridSizeXactual; x++)
            {
                Vector2 d1 = GetFirstDerivative(x, y);

                float aspect = (float)Aspect(d1.x, d1.y);

                var color = Colorize(aspect, 0, true);

                aspectMap.SetPixel(x, y, color);
            }

        }

        aspectMap.Apply();
        material.mainTexture = aspectMap;
    }

    private float Aspect(float zx, float zy)
    {
        float gyx = FMath.SafeDiv(zy, zx);
        float gxx = FMath.SafeDiv(zx, Math.Abs(zx));

        float aspect = 180 - Mathf.Atan(gyx) * FMath.Rad2Deg + 90 * gxx;
        aspect /= 360;

        return aspect;
    }
}

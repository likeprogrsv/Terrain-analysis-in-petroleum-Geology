using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CURVATURE_TYPE
{
    Horizontal, Vertical, Accumulation
};
public class CreateCurvatureMap : AbstrMakeImilorGrid
{
    public CURVATURE_TYPE curvatureType = CURVATURE_TYPE.Horizontal;

    private CURVATURE_TYPE currentCurvatureType;

    protected override bool OnChange()
    {
        return currentCurvatureType != curvatureType || currentColorMode != coloredGradient;
    }

    protected override void CreateMap()
    {
        currentCurvatureType = curvatureType;

        Texture2D curveMap = new Texture2D(gridSizeXactual, gridSizeYactual);

        for (int z = 0; z < gridSizeYactual; z++)
        {            
            for (int x = 0; x < gridSizeXactual; x++)
            {
                Vector2 d1;
                Vector3 d2;
                GetDerivatives(x, z, out d1, out d2);

                float curvature = 0;
                Color color = Color.white;

                switch (curvatureType)
                {
                    case CURVATURE_TYPE.Horizontal:
                        curvature = HorizontalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                        color = Colorize(curvature, 2.0f, false);
                        break;

                    case CURVATURE_TYPE.Vertical:
                        curvature = VerticalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                        color = Colorize(curvature, 2.0f, false);
                        break;

                    case CURVATURE_TYPE.Accumulation:
                        curvature = AccumulationCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                        color = Colorize(curvature, 5.0f, false);
                        break;
                };

                curveMap.SetPixel(x, z, color);                
            }
        }

        curveMap.Apply();
        material.mainTexture = curveMap;
    }



    // Kh - horizontal curvature
    // Same as plan curvature but multiplied by the sine of the slope angle.
    // Does not take on extremely large values when slope is small.
    // aka Tangential curvature.
    private float HorizontalCurvature(float yx, float yz, float yxx, float yzz, float yxz)
    {
        float yx2 = yx * yx;
        float yz2 = yz * yz;
        float p = yx2 + yz2;

        float n = yz2 * yxx - 2.0f * yxz * yx * yz + yx2 * yzz;
        float d = p * Mathf.Pow(p + 1, 0.5f);       //here "p" is "p*p + q*q" in the book

        return FMath.SafeDiv(n, d);
    }

    /// <summary>
    /// Kv
    /// Vertical curvature measures the rate of change of the slope.
    /// Is negative for slope increasing downhill and positive for slope decreasing dowhill.
    /// aka profile curvature.
    /// </summary>
    private float VerticalCurvature(float yx, float yz, float yxx, float yzz, float yxz)
    {
        float yx2 = yx * yx;
        float yz2 = yz * yz;
        float p = yx2 + yz2;

        float n = yx2 * yxx + 2.0f * yxz * yx * yz + yz2 * yzz;
        float d = p * Mathf.Pow(p + 1, 1.5f);

        return FMath.SafeDiv(n, d);
    }

    /// <summary>
    /// Ka
    /// A measure of the extent of the flow accumulation.
    /// </summary>
    private float AccumulationCurvature(float yx, float yz, float yxx, float yzz, float yxz)
    {
        float Kh = HorizontalCurvature(yx, yz, yxx, yzz, yxz);
        float Kv = VerticalCurvature(yx, yz, yxx, yzz, yxz);

        return Kh * Kv;
    }
}

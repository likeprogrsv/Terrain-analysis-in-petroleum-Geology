using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LANDFORM_TYPE { Accumulation };

public class CreateLandformMap : AbstrMakeImilorGrid
{
    public LANDFORM_TYPE landformType = LANDFORM_TYPE.Accumulation;

    private LANDFORM_TYPE currentLandformType;

    protected override bool OnChange()
    {
        return currentLandformType != landformType || currentColorMode != coloredGradient;
    }

    protected override void CreateMap()
    {
        currentLandformType = landformType;

        Texture2D landformMap = new Texture2D(gridSizeXactual, gridSizeYactual);

        for (int z = 0; z < gridSizeYactual; z++)
        {
            
            for (int x = 0; x < gridSizeXactual; x++)
            {
                Vector2 d1;
                Vector3 d2;
                GetDerivatives(x, z, out d1, out d2);

                float landform = 0;
                Color color = Color.white;

                switch (landformType)
                {
                    case LANDFORM_TYPE.Accumulation:
                        landform = AccumulationLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                        color = Colorize(landform, 0, true);
                        break;
                };

                landformMap.SetPixel(x, z, color);
                
            }
            landformMap.Apply();
            material.mainTexture = landformMap;
        }        
    }


    /// <summary>
    /// Ranges from 0 to 1.
    /// value 1 where flows dissperse from.
    /// value 0.75 where flow over convex shape.
    /// value 0.5 where flat.
    /// value 0.25 where flow over concave shape.
    /// value 0 where flows accumalate to.
    /// </summary>
    private float AccumulationLandform(float yx, float yz, float yxx, float yzz, float yxz)
    {
        float Kh = HorizontalCurvature(yx, yz, yxx, yzz, yxz);
        float Kv = VerticalCurvature(yx, yz, yxx, yzz, yxz);

        //Dissipation flows.
        if (Kh > 0 && Kv > 0)
            return 1;

        //Convex transitive.
        if (Kh > 0 && Kv < 0)
            return 0.75f;

        //Planar transitive.
        //Should be very rare.
        if (Kh == 0 || Kv == 0)
            return 0.5f;

        //Concave trasitive.
        if (Kh < 0 && Kv > 0)
            return 0.25f;

        //Accumulative flows.
        if (Kh < 0 && Kv < 0)
            return 0;

        throw new System.Exception("Unhandled lanform");
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
}

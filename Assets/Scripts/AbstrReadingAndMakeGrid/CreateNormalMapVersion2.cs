﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNormalMapVersion2 : AbstrMakeImilorGrid
{
    protected override void CreateMap()
    {
        Texture2D normalMap = new Texture2D(gridSizeX, gridSizeY);

        for (int z = 0; z < gridSizeY; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {

                //Debug.Log("X: " + x + "  Z: " + z);
                // Debug.Log("~~~~~~~~~~~~~~~~~~~");


                Vector2 d1 = GetFirstDerivative(x, z);

                //Not to sure of the orientation.
                //Might need to flip x or y

                var n = new Vector3();

                /*
                n.x = d1.x * 0.5f + 0.5f;
                n.y = 1f;
                n.z = -d1.y * 0.5f + 0.5f;               
                */

                n.x = d1.x * 0.5f + 0.5f;
                n.y = -d1.y * 0.5f + 0.5f;
                n.z = 1.0f;


                n.Normalize();

                //Debug.Log(n);

                normalMap.SetPixel(x, z, new Color(n.x, n.y, n.z, 1f));
            }
        }

        normalMap.Apply();
        material.mainTexture = normalMap;
    }

}
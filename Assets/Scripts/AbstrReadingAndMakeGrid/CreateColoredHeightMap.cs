using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateColoredHeightMap : AbstrMakeImilorGrid
{
    public Gradient heightGradient;
    private Gradient currentGradient;
    Color[] colors;

    protected override bool ColorizeHeightMap()
    {
        return true;           //True
    }     

    // Create color gradient for colored heightmap
    protected override void CreateMap()
    {       
        colors = new Color[gridSizeXactual * gridSizeYactual];
        for (int i = 0, z = 0; z < gridSizeYactual; z++)
        {
            for (int x = 0; x < gridSizeXactual; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, zValues[i]);
                colors[i] = heightGradient.Evaluate(height);
                i++;
            }
        }

        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;            //Index buffer can either be 16 bit (supports up to 65535 vertices in a mesh), or 32 bit (supports up to 4 billion vertices). 
                                                                                //Default index format is 16 bit, since that takes less memory and bandwidth.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;      
        mesh.RecalculateNormals();
    }   
}


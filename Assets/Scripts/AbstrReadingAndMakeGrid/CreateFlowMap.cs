using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateFlowMap : AbstrMakeImilorGrid
{

    public float amount = 0.0001f;
    public int iterationsNumber = 5;

    protected override void CreateMap()
    {
        float[,] waterMap = new float[gridSizeX, gridSizeY];
        float[,,] outFlow = new float[gridSizeX, gridSizeY, 4];

        FillWaterMap(amount, waterMap, gridSizeX, gridSizeY);

        for (int i = 0; i < iterationsNumber; i++)
        {
            ComputeOutFlow(waterMap, outFlow, zValues, gridSizeX, gridSizeY);
        }

    }


    private void FillWaterMap(float amount, float[,] waterMap, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                waterMap[x, y] = amount;
            }
        }
    }

    private void ComputeOutFlow (float [,] waterMap, float[,,] outFlow, float[] heightMap, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int xn1 = (x == 0) ? 0 : x - 1;
                int xp1 = (x == width - 1) ? width - 1 : x + 1;
                int yn1 = (y == 0) ? 0 : y - 1;
                int yp1 = (y == height - 1) ? height - 1 : y + 1;

                float waterHt = waterMap[x, y];
                float waterHts0 = waterMap[xn1, y];
                float waterHts1 = waterMap[xp1, y];
                float waterHts2 = waterMap[x, yn1];
                float waterHts3 = waterMap[x, yp1];


                // Maybe heightMap values should be multiply by -1, because they are negative. BUT PROBABLY SHOULDN'T BE
                float landHt = heightMap[x + y * width];            // Maybe should be smth like [x + z * (gridSizeX + 1)].y;
                float landHts0 = heightMap[xn1 + y * width];
                float landHts1 = heightMap[xp1 + y * width];
                float landHts2 = heightMap[x + yn1 * width];
                float landHts3 = heightMap[x + yp1 * width];

                float diff0 = (waterHt + landHt) - (waterHts0 + landHts0);
                float diff1 = (waterHt + landHt) - (waterHts1 + landHts1);
                float diff2 = (waterHt + landHt) - (waterHts2 + landHts2);
                float diff3 = (waterHt + landHt) - (waterHts3 + landHts3);

                //out flow is previous flow plus flow for this time step.
                float flow0 = Mathf.Max(0, outFlow[x, y, 0] + diff0);
                float flow1 = Mathf.Max(0, outFlow[x, y, 1] + diff1);
                float flow2 = Mathf.Max(0, outFlow[x, y, 2] + diff2);
                float flow3 = Mathf.Max(0, outFlow[x, y, 3] + diff3);

                float sum = flow0 + flow1 + flow2 + flow3;


            }
        }
    }
}

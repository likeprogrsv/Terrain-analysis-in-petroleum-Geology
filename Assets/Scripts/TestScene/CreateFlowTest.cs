using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateFlowTest : AbstrMakeImilorGrid        //MeshGeneratorAbstr
{

    public float amount = 0.0001f;
    public int iterationsNumber = 5;

    private const float TIME = 0.2f;

    private const int LEFT = 0;
    private const int RIGHT = 1;
    private const int BOTTOM = 2;
    private const int TOP = 3;

    private float[] _zOut;

    protected override void CreateMap()
    {
        float[,] waterMap = new float[gridSizeXactual, gridSizeYactual];
        float[,,] outFlow = new float[gridSizeXactual, gridSizeYactual, 4];
        _zOut = new float[gridSizeXactual * gridSizeYactual];

        for (int i = 0, z = 0; z < gridSizeYactual; z++)
        {
            for (int x = 0; x < gridSizeXactual; x++)
            {
                _zOut[i] = Zout[x, z];
                i++;
            }
        }


        FillWaterMap(amount, waterMap, gridSizeXactual, gridSizeYactual);

        for (int i = 0; i < iterationsNumber; i++)
        {
            ComputeOutFlow(waterMap, outFlow, _zOut, gridSizeXactual, gridSizeYactual);
            UpdateWaterMap(waterMap, outFlow, gridSizeXactual, gridSizeYactual);
        }

        float[,] velocityMap = new float[gridSizeXactual, gridSizeYactual];

        CalculateVelocityField(velocityMap, outFlow, gridSizeXactual, gridSizeYactual);
        NormalizeMap(velocityMap, gridSizeXactual, gridSizeYactual);

        Texture2D flowMap = new Texture2D(gridSizeXactual, gridSizeYactual);

        for (int y = 0; y < gridSizeYactual; y++)
        {
            for (int x = 0; x < gridSizeXactual; x++)
            {
                float v = velocityMap[x, y];
                flowMap.SetPixel(x, y, new Color(v, v, v, 1));
            }
        }

        flowMap.Apply();
        material.mainTexture = flowMap;
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

    private void ComputeOutFlow(float[,] waterMap, float[,,] outFlow, float[] heightMap, int width, int height)
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

                if (sum > 0.0f)
                {
                    //If the sum of the outflow flux exceeds the amount in the cell
                    //flow value will be scaled down by a factor K to avoid negative update.
                    float K = waterHt / (sum * TIME);
                    if (K > 1.0f) K = 1.0f;
                    if (K < 0.0f) K = 0.0f;

                    outFlow[x, y, 0] = flow0 * K;
                    outFlow[x, y, 1] = flow1 * K;
                    outFlow[x, y, 2] = flow2 * K;
                    outFlow[x, y, 3] = flow3 * K;
                }
                else
                {
                    outFlow[x, y, 0] = 0.0f;
                    outFlow[x, y, 1] = 0.0f;
                    outFlow[x, y, 2] = 0.0f;
                    outFlow[x, y, 3] = 0.0f;
                }
            }
        }
    }

    private void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float flowOut = outFlow[x, y, 0] + outFlow[x, y, 1] + outFlow[x, y, 2] + outFlow[x, y, 3];
                float flowIn = 0.0f;

                //Flow in is inflow from neighour cells. Note for the cell on the left you need 
                //thats cells flow to the right (ie it flows into this cell)
                flowIn += (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT];
                flowIn += (x == width - 1) ? 0.0f : outFlow[x + 1, y, LEFT];
                flowIn += (y == 0) ? 0.0f : outFlow[x, y - 1, TOP];
                flowIn += (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM];

                float ht = waterMap[x, y] + (flowIn - flowOut) * TIME;
                if (ht < 0.0f) ht = 0.0f;

                //Result is net volume change over time
                waterMap[x, y] = ht;
            }
        }
    }

    private void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dLeft = (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT] - outFlow[x, y, LEFT];

                float dRight = (x == width - 1) ? 0.0f : outFlow[x, y, RIGHT] - outFlow[x + 1, y, LEFT];

                float dTop = (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM] - outFlow[x, y, TOP];

                float dBottom = (y == 0) ? 0.0f : outFlow[x, y, BOTTOM] - outFlow[x, y - 1, TOP];

                float vx = (dLeft + dRight) * 0.5f;
                float vy = (dBottom + dTop) * 0.5f;

                velocityMap[x, y] = Mathf.Sqrt(vx * vx + vy * vy);
            }
        }
    }

    public static void NormalizeMap(float[,] map, int width, int height)
    {
        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = map[x, y];
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        float size = max - min;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = map[x, y];

                if (size < 1e-12f)
                    v = 0;
                else
                    v = (v - min) / size;

                map[x, y] = v;
            }
        }
    }
}

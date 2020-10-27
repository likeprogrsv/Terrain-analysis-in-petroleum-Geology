using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public enum VISUALIZE_GRADIENT { WARM, COOL, COOL_WARM, GREY_WHITE, GREY_BLACK, BLACK_WHITE };


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public abstract class AbstrMakeImilorGrid : MonoBehaviour
{

    protected Mesh mesh;
    ReadGridFromFile readGridFromFile = new ReadGridFromFile();

    protected Vector3[] vertices;
    protected int[] triangles;
    Vector2[] uvs;

    string filePath = @"E:\GitRepositories\Terrain-analysis-in-petroleum-Geology\GridSFile\OG_A-OG_T3_smooth_10.is-txt";            //OG_A_(10+15)_subtruct_OG_T(10+15)100m         //H_A_step_50m

    public bool smoothTerrain;
    public int numberOfSmoothingIterations = 2;

    public Material material;       //Grid(or mesh) material
    public bool coloredGradient;

    //grid settings(parameters)
    public float cellsize = 50;                               //ReadGridFromFile.stepX;
    //public Vector3 gridOffset;
    protected int gridSizeX;                                    //Grid width            ReadGridFromFile.countX;
    protected int gridSizeY;                                    //Grid height           ReadGridFromFile.countY; 

    protected int gridSizeXactual;                              //actual width size (X-coord) in origin file
    protected int gridSizeYactual;                              //actual height size (Z-coord or Y) in origin file

    float xMin;
    float yMin;
    float cutCoordValues = 100f;        //Cutting very large grid starting coordinates values

    protected float[] zValues;
    int gridMultiplier;
    protected float minTerrainHeight = 9999;
    protected float maxTerrainHeight = -9999;

    private Texture2D gradient, posGradient, negGradient;
    protected bool currentColorMode;


    public enum gridMultipl
    {
        NegativeGridValues = 0,
        PositiveGridValues = 1
    };

    public gridMultipl GridMultiplier = gridMultipl.NegativeGridValues;


    //Use this for initialization
    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

    }

    void Start()
    {
        readGridFromFile.GetGridParameters(filePath);
        readGridFromFile.GetDataValues();
        xMin = readGridFromFile.TakeXmin();
        yMin = readGridFromFile.TakeYmin();

        Debug.Log("Xmin: " + xMin + " Ymin: " + yMin);
        Debug.Log("Reading file is DONE");

        transform.Translate(xMin / cutCoordValues, 0, yMin / cutCoordValues);       //Setting starting coordinates values from grid settings


        if (GridMultiplier == gridMultipl.PositiveGridValues)
        {
            gridMultiplier = -1;
        }
        else
        {
            gridMultiplier = 1;
        }


        GetCountXY();        
        MakeGrid();

        ////////////CreateNormalMap(); 


        // Create color gradient
        currentColorMode = coloredGradient;
        CreateGradients(coloredGradient);        



        //Smooth terrain if you need
        if (smoothTerrain)
        {
            for (int i = 0; i < numberOfSmoothingIterations; i++)
            {                
                SmoothHeightMap();                
            }
            MakeGrid();
        }
            
        CreateMap();

        if(!ColorizeHeightMap())
            UpdateMesh();


        //if(saveGridAsAsset) SaveMeshAsAsset();            //It doesn't works

        Debug.Log("Vertex count: " + mesh.vertexCount);
        Debug.Log("Vertex arr length: " + vertices.Length);
        Debug.Log("Triangles arr length: " + triangles.Length);
                
    }

    private void Update()
    {
        //If settings changed then recreate map
        if (OnChange())
        {
            CreateGradients(coloredGradient);
            CreateMap();

            currentColorMode = coloredGradient;

            Debug.Log("Changed");
        }

        //if (ColorizeHeightMap())                      // Real time updating gradient colors
         //   CreateMap();
                  
    }


    void GetCountXY()
    {
        gridSizeX = ReadGridFromFile.countX - 1;
        gridSizeY = ReadGridFromFile.countY - 1;

        gridSizeXactual = gridSizeX + 1;
        gridSizeYactual = gridSizeY + 1;

        zValues = ReadGridFromFile.dataArrayValues;

        Debug.Log("CountX in GetCountXY: " + gridSizeX + "\nCountY: " + gridSizeY);
        Debug.Log("CountZ in GetCountXY: " + zValues);
    }

    protected void SmoothHeightMap()
    {       
        //var heights = new float[gridSizeXactual * gridSizeYactual];
        float[] heights = zValues;

        

        var gaussianKernel5 = new float[,]
        {
            {1,4,6,4,1},
            {4,16,24,16,4},
            {6,24,36,24,6},
            {4,16,24,16,4},
            {1,4,6,4,1}
        };

        float gaussScale = 1.0f / 256.0f;

        for (int z = 0; z < gridSizeYactual; z++)
        {
            for (int x = 0; x < gridSizeXactual; x++)
            {
                float sum = 0;

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        int xi = x - 2 + i;
                        int zi = z - 2 + j;

                        sum += GetNormalizedHeight(xi, zi) * gaussianKernel5[i, j] * gaussScale;
                    }
                }

                heights[x + z * gridSizeXactual] = sum;
            }
        }

        zValues = heights;
        MakeGrid();        
    }

    void MakeGrid()
    {
        //setting array sizes
        vertices = new Vector3[(gridSizeX + 1) * (gridSizeY + 1)];
        triangles = new int[gridSizeX * gridSizeY * 6];
        uvs = new Vector2[vertices.Length];

        //set tracker integers
        int v = 0;
        int t = 0;

        //set vertex offset
        float vertexOffset = cellsize * 0.5f;       //cellsize*0.5f   после всех тестов и исправлений поправить обратно    !!!!!!!!!!!!!!

        //create vertex grid
        //In Unity coordinate system vertical axis is "y-axis" instead of "z-axis"


        for (int z = 0; z <= gridSizeY; z++)        //for (int x = 0; x <= gridSizeX; x++)  
        {

            for (int x = 0; x <= gridSizeX; x++)        //for (int y = 0; y <= gridSizeY; y++)   
            {
                vertices[v] = new Vector3(x * cellsize, gridMultiplier * zValues[v], z * cellsize);     //in previous version used "(x * cellsize) - vertexOffset"
                uvs[v] = new Vector2(vertices[v].x / (float)gridSizeX / cellsize, vertices[v].z / (float)gridSizeY / cellsize);

                if(zValues[v] > maxTerrainHeight)
                    maxTerrainHeight = zValues[v];
                if (zValues[v] < minTerrainHeight)
                    minTerrainHeight = zValues[v];

                v++;
            }
        }

        //reset vertex tracker
        v = 0;

        //setting each cell's triangles
        for (int x = 0; x < gridSizeY; x++)
        {
            for (int y = 0; y < gridSizeX; y++)
            {
                triangles[t] = v;
                triangles[t + 1] = triangles[t + 4] = v + (gridSizeX + 1);
                triangles[t + 2] = triangles[t + 3] = v + 1;
                triangles[t + 5] = v + (gridSizeX + 1) + 1;
                v++;
                t += 6;
            }
            v++;
        }


    }

    void SaveMeshAsAsset()
    {
        AssetDatabase.CreateAsset(mesh, "Assets/PrefabGrid.obj");
        AssetDatabase.SaveAssets();
    }


    
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;            //Index buffer can either be 16 bit (supports up to 65535 vertices in a mesh), or 32 bit (supports up to 4 billion vertices). 
                                                                                //Default index format is 16 bit, since that takes less memory and bandwidth.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs; 
        mesh.RecalculateNormals();
    }
    

    protected virtual bool OnChange()                           // Default mode is nothing changes.
    {
        return false;
    }

    protected virtual bool ColorizeHeightMap()
    {
        return false;
    }

    protected abstract void CreateMap();                        // Create the map. Update to derivered class to implement.
    

    float GetNormalizedHeight(int x, int z)
    {

        //Debug.Log("X: " + x + "  Z: " + z);

        x = Mathf.Clamp(x, 0, gridSizeX);       //x = Mathf.Clamp(x, 0, xSize - 1);
        z = Mathf.Clamp(z, 0, gridSizeY);

        /*
        Debug.Log("==============================");
        Debug.Log("X: " + x + "  Z: " + z);
        Debug.Log(x + z * (xSize + 1));
        Debug.Log(vertices[x + z * (xSize + 1)].y);
        Debug.Log("==============================");
        */

        return vertices[x + z * gridSizeXactual].y;       //Take y-cordinate value (i.e. height) at point(x,0,z)
    }

    float GetHeight(int x, int z)
    {
        return GetNormalizedHeight(x, z);       //* m_terrainHeight
    }


    public Vector2 GetFirstDerivative(int x, int z)
    {
        float w = cellsize;
        float y1 = GetHeight(x - 1, z + 1);
        float y2 = GetHeight(x + 0, z + 1);
        float y3 = GetHeight(x + 1, z + 1);
        float y4 = GetHeight(x - 1, z + 0);
        float y6 = GetHeight(x + 1, z + 0);
        float y7 = GetHeight(x - 1, z - 1);
        float y8 = GetHeight(x + 0, z - 1);
        float y9 = GetHeight(x + 1, z - 1);

        // Find derivatives p (dy/dx), q (dy/dz) using Evans-Young method
        float yx = (y3 + y6 + y9 - y1 - y4 - y7) / (6.0f * w);
        float yz = (y1 + y2 + y3 - y7 - y8 - y9) / (6.0f * w);
        
       
        /*
        Debug.Log("______________________________");
        Debug.Log(yx);
        Debug.Log(yz);
        Debug.Log("______________________________");
        */
        return new Vector2(-yx, -yz);
    }

    //Get the heigts maps first and second derivative using Evans-Young method.
    protected void GetDerivatives(int x, int z, out Vector2 d1, out Vector3 d2)
    {
        float w = cellsize;
        float w2 = w * w;
        float y1 = GetHeight(x - 1, z + 1);
        float y2 = GetHeight(x + 0, z + 1);
        float y3 = GetHeight(x + 1, z + 1);
        float y4 = GetHeight(x - 1, z + 0);
        float y5 = GetHeight(x + 0, z + 0);
        float y6 = GetHeight(x + 1, z + 0);
        float y7 = GetHeight(x - 1, z - 1);
        float y8 = GetHeight(x + 0, z - 1);
        float y9 = GetHeight(x + 1, z - 1);

        // Find derivatives p (dy/dx), q (dy/dz) using Evans-Young method
        float yx = (y3 + y6 + y9 - y1 - y4 - y7) / (6.0f * w);
        float yz = (y1 + y2 + y3 - y7 - y8 - y9) / (6.0f * w);

        // Find second order derivatives r (d*dy/dx*x), t (d*dy/dz*z), s (d*dy/dx*dz)
        float yxx = (y1 + y3 + y4 + y6 + y7 + y9 - 2.0f * (y2 + y5 + y8)) / (3.0f * w2);
        float yzz = (y1 + y2 + y3 + y7 + y8 + y9 - 2.0f * (y4 + y5 + y6)) / (3.0f * w2);
        float yxz = (y3 + y7 - y1 - y9) / (4.0f * w2);

        d1 = new Vector2(-yx, -yz);
        d2 = new Vector3(-yxx, -yzz, -yxz);     //is zxy or -zxy?
    }


    // Take a parameter, rescale it and return as a 
    // color using a gradient. Helps visualize some 
    // parameters better especially if they have a 
    // wide dynamic range and can be negative. 
    protected Color Colorize(float v, float exponent, bool nonNegative)
    {
        if(exponent > 0)
        {
            float sign = FMath.SignOrZero(v);
            float pow = Mathf.Pow(10, exponent);
            float log = Mathf.Log(1.0f + pow * Mathf.Abs(v));

            v = sign * log;
        }

        if (nonNegative)
            return gradient.GetPixelBilinear(v, 0);
        else
        {
            if (v > 0)
                return posGradient.GetPixelBilinear(v, 0);
            else
                return negGradient.GetPixelBilinear(-v, 0);
        }
    }

    private void CreateGradients(bool colored)
    {
        if (colored)
        {
            gradient = CreateGradient(VISUALIZE_GRADIENT.COOL_WARM);
            posGradient = CreateGradient(VISUALIZE_GRADIENT.WARM);
            negGradient = CreateGradient(VISUALIZE_GRADIENT.COOL);
        }
        else
        {
            gradient = CreateGradient(VISUALIZE_GRADIENT.BLACK_WHITE);
            posGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_WHITE);
            negGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_BLACK);
        }

        gradient.Apply();
        posGradient.Apply();
        negGradient.Apply();
    }

    private Texture2D CreateGradient(VISUALIZE_GRADIENT g)
    {
        switch (g)
        {
            case VISUALIZE_GRADIENT.WARM:
                return CreateWarmGradient();

            case VISUALIZE_GRADIENT.COOL:
                return CreateCoolGradient();

            case VISUALIZE_GRADIENT.COOL_WARM:
                return CreateCoolToWarmGradient();

            case VISUALIZE_GRADIENT.GREY_WHITE:
                return CreateGreyToWhiteGradient();

            case VISUALIZE_GRADIENT.GREY_BLACK:
                return CreateGreyToBlackGradient();

            case VISUALIZE_GRADIENT.BLACK_WHITE:
                return CreateBlackToWhiteGradient();
        }

        return null;
    }

    private Texture2D CreateWarmGradient()
    {
        var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(80, 230, 80, 255));
        gradient.SetPixel(1, 0, new Color32(180, 230, 80, 255));
        gradient.SetPixel(2, 0, new Color32(230, 230, 80, 255));
        gradient.SetPixel(3, 0, new Color32(230, 180, 80, 255));
        gradient.SetPixel(4, 0, new Color32(230, 80, 80, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }

    private Texture2D CreateCoolGradient()
    {
        var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(80, 230, 80, 255));
        gradient.SetPixel(1, 0, new Color32(80, 230, 180, 255));
        gradient.SetPixel(2, 0, new Color32(80, 230, 230, 255));
        gradient.SetPixel(3, 0, new Color32(80, 180, 230, 255));
        gradient.SetPixel(4, 0, new Color32(80, 80, 230, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }

    private Texture2D CreateCoolToWarmGradient()
    {
        var gradient = new Texture2D(9, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(80, 80, 230, 255));
        gradient.SetPixel(1, 0, new Color32(80, 180, 230, 255));
        gradient.SetPixel(2, 0, new Color32(80, 230, 230, 255));
        gradient.SetPixel(3, 0, new Color32(80, 230, 180, 255));
        gradient.SetPixel(4, 0, new Color32(80, 230, 80, 255));
        gradient.SetPixel(5, 0, new Color32(180, 230, 80, 255));
        gradient.SetPixel(6, 0, new Color32(230, 230, 80, 255));
        gradient.SetPixel(7, 0, new Color32(230, 180, 80, 255));
        gradient.SetPixel(8, 0, new Color32(230, 80, 80, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }

    private Texture2D CreateGreyToWhiteGradient()
    {
        var gradient = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(128, 128, 128, 255));
        gradient.SetPixel(1, 0, new Color32(192, 192, 192, 255));
        gradient.SetPixel(2, 0, new Color32(255, 255, 255, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }

    private Texture2D CreateGreyToBlackGradient()
    {
        var gradient = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(128, 128, 128, 255));
        gradient.SetPixel(1, 0, new Color32(64, 64, 64, 255));
        gradient.SetPixel(2, 0, new Color32(0, 0, 0, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }

    private Texture2D CreateBlackToWhiteGradient()
    {
        var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
        gradient.SetPixel(0, 0, new Color32(0, 0, 0, 255));
        gradient.SetPixel(1, 0, new Color32(64, 64, 64, 255));
        gradient.SetPixel(2, 0, new Color32(128, 128, 128, 255));
        gradient.SetPixel(3, 0, new Color32(192, 192, 192, 255));
        gradient.SetPixel(4, 0, new Color32(255, 255, 255, 255));
        gradient.wrapMode = TextureWrapMode.Clamp;

        return gradient;
    }
}
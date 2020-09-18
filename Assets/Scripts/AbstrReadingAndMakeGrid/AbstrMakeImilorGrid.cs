using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public abstract class AbstrMakeImilorGrid : MonoBehaviour
{

    Mesh mesh;
    ReadGridFromFile readGridFromFile = new ReadGridFromFile();

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    string filePath = @"D:\GitRepositories\Terrain-analysis-in-petroleum-Geology\GridSFile\H_A_step_50m.is-txt";            //Otr_goriz_A___step_50m_H_A_260716         //Imilor_Ach3_2

    public Material material;
    //grid settings(parameters)
    public float cellsize = 1;                               //ReadGridFromFile.stepX;
    //public Vector3 gridOffset;
    public int gridSizeX;                                   //ReadGridFromFile.countX;
    public int gridSizeY;                                    //ReadGridFromFile.countY; 

    float xMin;
    float yMin;
    float cutCoordValues = 100f;        //Cutting very large grid starting coordinates values

    float[] zValues;
    int gridMultiplier;


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
        CreateMap();

        UpdateMesh();
        //SaveMeshAsAsset();

        Debug.Log("Vertex count: " + mesh.vertexCount);
        Debug.Log("Vertex arr length: " + vertices.Length);
        Debug.Log("Triangles arr length: " + triangles.Length);

        /*
        
        Debug.Log("countX test: " + ReadGridFromFile.countX);
        Debug.Log("countY test: " + ReadGridFromFile.countY);
        Debug.Log("countX test: " + gridSizeX);
        Debug.Log("countX test: " + gridSizeX.GetType());
        */
    }


    void GetCountXY()
    {
        gridSizeX = ReadGridFromFile.countX - 1;
        gridSizeY = ReadGridFromFile.countY - 1;
        zValues = ReadGridFromFile.dataArrayValues;

        Debug.Log("CountX in GetCountXY: " + gridSizeX + "\nCountY: " + gridSizeY);
        Debug.Log("CountZ in GetCountXY: " + zValues);
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

        return vertices[x + z * (gridSizeX + 1)].y;       //Take y-cordinate value (i.e. height) at point(x,0,z)
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

        /*
        Debug.Log("------------------------------");
        Debug.Log(y1);
        Debug.Log(y2);
        Debug.Log(y3);
        Debug.Log(y4);
        Debug.Log(y6);
        Debug.Log(y7);
        Debug.Log(y8);
        Debug.Log(y9);
        Debug.Log("------------------------------");
        */

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


    public void CreateNormalMap()
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
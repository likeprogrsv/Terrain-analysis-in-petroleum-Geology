using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MakeImilorGrid : MonoBehaviour
{

    Mesh mesh;
    ReadGridFromFile readGridFromFile = new ReadGridFromFile();

    Vector3[] vertices;
    int[] triangles;

    string filePath = @"E:\Unity3D\__Unity3D_Projects__\Imilor Grid in Unity3D\Imilor Grid in Unity\GridSFile\H_T_step_50m.is-txt";            //Otr_goriz_A___step_50m_H_A_260716         //Imilor_Ach3_2


    //grid settings(parameters)
    public float cellsize = 1;                               //ReadGridFromFile.stepX;
    //public Vector3 gridOffset;
    int gridSizeX;                                   //ReadGridFromFile.countX;
    int gridSizeY;                                    //ReadGridFromFile.countY;

    float xMin;
    float yMin;
    float cutCoordValues = 10f;

    float[] zValues;
    int gridMultiplier;


    public enum gridMultipl {
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

        transform.Translate(xMin / cutCoordValues, 0, yMin / cutCoordValues);
       

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

        //set tracker integers
        int v = 0;
        int t = 0;

        //set vertex offset
        float vertexOffset = cellsize * 0.5f;       //cellsize*0.5f   после всех тестов и исправлений поправить обратно    !!!!!!!!!!!!!!

        //create vertex grid
        //In Unity coordinate system vertical axis is "y-axis" instead of "z-axis"

        float xTemp;
        float zTemp;    //or mentally "yTemp"
        for (int z = 0; z <= gridSizeY; z++)        //for (int x = 0; x <= gridSizeX; x++)  
        {
            zTemp = (yMin + z * cellsize) / cutCoordValues;
            for (int x = 0; x <= gridSizeX; x++)        //for (int y = 0; y <= gridSizeY; y++)   
            {
                xTemp = (xMin + x * cellsize) / cutCoordValues;
                vertices[v] = new Vector3(xTemp - vertexOffset, gridMultiplier * zValues[v], zTemp - vertexOffset);
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
        mesh.RecalculateNormals();
    }



}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct strGRD_Header
{
    public int Nx;
    public int Ny;
    public float Xmin;
    public float Xmax;
    public float Ymin;
    public float Ymax;
    public float Zmin;
    public float Zmax;
}

public struct Node
{
    float priority;     //Elevation of current node
    int priority2;      //Global addition order
    int c;              //column index of current node
    int r;              //row index of current node
}

public struct Queue
{
    Node[] node;
    int n;
}

public abstract class MeshGeneratorAbstr : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public bool NormalMap = false;
    public bool FillDepressions;
    public Material m_material;

    public bool drawSphere = false;

    public int xSize = 20;
    public int zSize = 20;
    public float m_cellLength = 1;      //map cell size (length) in Unity units  


    // Variables describes DEM
    protected float[,] Z;
    protected float Xmin, Ymin, Xmax, Ymax, StepX, StepY, SizeX, SizeY;
    protected int Nx, Ny;
    protected float Zmax = -9999f, Zmin = 9999f;


    public const int GRD_FMT = 0;
    public const float GRD_NODATA = -9999f;

    //объявление переменной производного типа (GRD_Header - переменная, tGRDHeader - тип, объявленный выше)
    strGRD_Header GRD_Header = new strGRD_Header();

    protected float[,] Zout;            // массивы для обработки и вывода данных
    [HideInInspector]
    public float nodata;


    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        SetVariables();
        Zout = CreateZout(Nx, Ny);

        if (FillDepressions) FillDepr(Z, Zout, Nx, Ny, Zmin, Zmax, StepX, StepY, nodata);

        

        if(NormalMap) CreateMap();
        
    }

    void Update()
    {
        UpdateMesh();
    }
    

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uvs = new Vector2[vertices.Length];
        Z = new float[xSize + 1, zSize + 1];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[i] = new Vector3(x * m_cellLength, y, z * m_cellLength);
                uvs[i] = new Vector2(vertices[i].x / (float)xSize, vertices[i].z / (float)zSize);
                Z[x, z] = y;

                if (vertices[i].y > Zmax)
                    Zmax = vertices[i].y;
                if (vertices[i].y < Zmin)
                    Zmin = vertices[i].y;

                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z ++)
        {
            for (int x = 0; x < xSize; x++)
            {

                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }


    protected void SetVariables()
    {
        Nx = xSize + 1;
        Ny = zSize + 1;
        Xmin = vertices[0].x;
        Xmax = vertices[Nx].x;
        Ymin = vertices[0].z;
        Ymax = vertices[Ny].z;
        // Zmax and Zmin calculated in CreateShape()
        StepX = m_cellLength;
        StepY = m_cellLength;
        nodata = GRD_NODATA;
    }


    protected float[,] CreateZout(int Nx, int Ny)
    {
        float[,] zOut = new float[Nx, Ny];
        for (int y = 0; y < Ny; y++)
        {
            for (int x = 0; x < Nx; x++)
            {
                zOut[x, y] = nodata;
            }
        }
        return zOut;
    }


    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (drawSphere)
        {
            if (vertices == null)
            {
                return;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.DrawSphere(vertices[i], .1f);
            }
        }
        
    }

    protected abstract void CreateMap();

    protected float GetNormalizedHeight(int x, int z)
    {
        x = Mathf.Clamp(x, 0, xSize);       //x = Mathf.Clamp(x, 0, xSize - 1);
        z = Mathf.Clamp(z, 0, zSize);

        return vertices[x + z * (xSize + 1)].y;       //Take y-cordinate value (i.e. height) at point(x,0,z)
    }

    protected float GetHeight(int x, int z)
    {
        return GetNormalizedHeight(x, z);       //* m_terrainHeight
    }


    protected Vector2 GetFirstDerivative(int x, int z)
    {
        float w = m_cellLength;
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
        return new Vector2(-yx, -yz);
    }


    //////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///
    ///
    ///
    ////////////////////////////////////////////////////////////////////////////////////////////

    protected void FillDepr(float[,] Z, float[,] Zout, int Nx, int Ny, float Zmin, float Zmax, float StepX, float StepY, float NODATA)
    {
        //Внутренние переменные подпрограммы
        float[,] Zout_temp;         //Промежуточная матрица высот
        float[,] Z_flat;         //Для работы программы заполнения нужна модель, заполненная «под плоскость»
        int[,] depr;                //Матрица, ячейки которой содержат информацию о понижениях, границах и точках выхода
        int[] outlets_list;         //Список точек выхода
        int numberOfOutlets = 0;

        Z_flat = new float[Nx, Ny];
        depr = new int[Nx, Ny];
        outlets_list = SetZerosOutlets(Nx, Ny);

        Wang_Liu2(Z, Z_flat, depr, outlets_list, numberOfOutlets, Zmax, Nx, Ny, NODATA);

    }


    protected int[] SetZerosOutlets(int Nx, int Ny)
    {
        int[] outletsList = new int[Nx * Ny];
        for (int i = 0; i < outletsList.Length; i++)
        {
            outletsList[i] = 0;
        }
        return outletsList;
    }

    protected int[,] SetZeros2dIntArr(int Nx, int Ny)
    {
        int[,] deprArr = new int[Nx, Ny];
        for (int y = 0; y < Ny; y++)
        {
            for (int x = 0; x < Nx; x++)
            {
                deprArr[x, y] = 0;
            }
        }
        return deprArr;
    }

    protected void Wang_Liu2(float[,] Z, float[,] Z_flat, int[,] depr, int[] outlets_list, int q_out, float Zmax, int Nx, int Ny, float NODATA)
    {
        Queue q = new Queue();      //Очередь с приоритетом
        Node x = new Node();        //Элементарный узел очереди с приоритетом
        int[,] mask;                //Массив, в котором будет отмечаться просмотренность/непросмотренность ячейки. 
                                    //0 - не просмотрена, 1 - в очереди, 2 - просмотрена

        int q2, q_max;              //Общее количество точек для обработки.
                                    //В начале работы программы равно количеству ячеек в матрице, затем снижается (за счёт значений "нет данных")

        float Z1, Z2;

        //Скользящее окно 3х3(сначала просмотр прямых соседей, потом соседей по диагонали)
        int k;
        int[] kx = { 1, 0, -1, 0, 1, -1, -1, 1 };
        int[] ky = { 0, 1, 0, -1, 1, 1, -1, -1 };


        //Начало работы
        q_max = Nx * Ny;
        Z_flat = Z;         //Модель для заполнения "под плоскость"
        depr = SetZeros2dIntArr(Nx, Ny);
        mask = SetZeros2dIntArr(Nx, Ny);

        // Определение первоначального списка точек
        q2 = 0;
        InitializeSet();

    }

    protected void InitializeSet()
    {

    }

    protected void Top(Queue Q)
    {

    }
}
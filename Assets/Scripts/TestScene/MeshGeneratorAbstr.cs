using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeshGeneratorAbstr : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public Material m_material;

    public bool drawSphere = false;

    public int xSize = 20;
    public int zSize = 20;
    public float m_cellLength = 1;      //map cell size (length) in Unity units  

    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        CreateMap();
        
    }

    void Update()
    {
        UpdateMesh();
    }
    

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[i] = new Vector3(x * m_cellLength, y, z * m_cellLength);
                uvs[i] = new Vector2(vertices[i].x / (float)xSize, vertices[i].z / (float)zSize);
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

        //Debug.Log("X: " + x + "  Z: " + z);

        x = Mathf.Clamp(x, 0, xSize);       //x = Mathf.Clamp(x, 0, xSize - 1);
        z = Mathf.Clamp(z, 0, zSize);

        /*
        Debug.Log("==============================");
        Debug.Log("X: " + x + "  Z: " + z);
        Debug.Log(x + z * (xSize + 1));
        Debug.Log(vertices[x + z * (xSize + 1)].y);
        Debug.Log("==============================");
        */

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

}
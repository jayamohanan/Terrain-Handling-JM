using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshGenerator
{
    private float[,] map;
    private int chunkSize;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    public MeshGenerator(float[,] map)
    {
        this.map = map;
        this.chunkSize = map.GetLength(0);
    }
    public MeshData CreateMesh(int lod)// lod and it's mesh generted sould be added to lodDictionary for future reuse
    {
        int numVertices = (chunkSize - 1) / lod + 1;
        vertices = new Vector3[numVertices * numVertices];
        triangles = new int[(numVertices - 1) * (numVertices - 1) * 6];
        uvs = new Vector2[numVertices * numVertices];
        float corner = -chunkSize / 2f;
        float extraDistanceX = 0;
        float extraDistanceZ = 0;
        for (int j = 0; j < chunkSize; j += lod)
        {
            for (int i = 0; i < chunkSize; i += lod)
            {
                int a = (j / lod) * numVertices + (i / lod);
                vertices[a] = new Vector3(corner + i + extraDistanceX, map[i, j], corner + j + extraDistanceZ);
                extraDistanceX = 0;
                extraDistanceZ = 0;
            }
        }
        int index = 0;
        for (int j = 0; j < numVertices - 1; j++)
        {
            for (int i = 0; i < numVertices - 1; i++)
            {
                int a = j * numVertices + i;
                triangles[index] = a + numVertices;
                triangles[index + 1] = a + 1;
                triangles[index + 2] = a;
                triangles[index + 3] = a + numVertices;
                triangles[index + 4] = a + numVertices + 1;
                triangles[index + 5] = a + 1;
                index += 6;
            }
        }
        MeshData meshData = new MeshData(numVertices, vertices, triangles);
        return meshData;
    }
    public Mesh AssignMesh(MeshData meshData, Dictionary<int, Mesh> lodDictionary, int lod)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();
        if (lodDictionary != null && !lodDictionary.ContainsKey(lod))
        {
            lodDictionary.Add(lod, mesh);
        }
        return mesh;
    }
}
public struct MeshData
{
    public int numVertices;
    public Vector3[] vertices;
    public int[] triangles;
    public MeshData(int numVertices, Vector3[] vertices, int[] triangles)
    {
        this.numVertices = numVertices;
        this.vertices = vertices;
        this.triangles = triangles;
    }
}
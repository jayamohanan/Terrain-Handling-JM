using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
public class TerrainChunk
{
    static int chunkSize;
    int lod;
    float[,] map;
    float presistance;
    NoiseSettings noiseSettings;
    Vector3 position;
    Transform parent;
    Material terrainMat;
    public Dictionary<int, Mesh> lodDictionary = new Dictionary<int, Mesh>();
    public GameObject terrainChunk;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public TerrainChunk(NoiseSettings noiseSettings, int chunkSize, int lod, Vector3 position, Material terrainMat, Transform parent)
    {
        this.noiseSettings = noiseSettings;
        this.position = position;
        TerrainChunk.chunkSize = chunkSize;
        this.lod = lod;
        this.parent = parent;
        this.terrainMat = terrainMat;
        if (Application.isPlaying)
        {
            CreateTerrainChunk();
        }
        else
        {
            CreateTerrainChunkEditor();
        }
    }
    private void CreateTerrainChunk()
    {
        Noise noise = new Noise(noiseSettings);
        map = noise.GenerateMap();
        
        terrainChunk = new GameObject();
        terrainChunk.transform.position = position;
        terrainChunk.transform.SetParent(parent);
        meshFilter = terrainChunk.AddComponent<MeshFilter>();
        meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
        meshCollider = terrainChunk.AddComponent<MeshCollider>();
        meshRenderer.material = terrainMat;
        GenerateLODMesh(lod);
    }
    public void GenerateLODMesh(int lod)
    {
        if (lodDictionary.ContainsKey(lod))
        {
            meshFilter.mesh = lodDictionary[lod];
            //meshCollider.sharedMesh = lodDictionary[lod];
        }
        else
        {
            LODMesh lodMesh = new LODMesh(lod, map);
            Mesh mesh = lodMesh.CreateMesh(lodDictionary);
            meshFilter.mesh = mesh;
            //meshCollider.sharedMesh = mesh;
        }
    }
    private void CreateTerrainChunkEditor()
    {
        Noise noise = new Noise(noiseSettings);
        map = noise.GenerateMap();
        GameObject editorChunk = GameObject.FindGameObjectWithTag("Terrain");
        if (editorChunk)
        {
            MeshFilter meshFilter = editorChunk.GetComponent<MeshFilter>();
            MeshCollider meshCollider = editorChunk.GetComponent<MeshCollider>();
            editorChunk.GetComponent<MeshRenderer>().material = terrainMat;
            LODMesh lodMesh = new LODMesh(lod, map);
            Mesh mesh = lodMesh.CreateMesh(null);
            meshFilter.mesh = mesh;
            //meshCollider.sharedMesh = mesh;
        }
        else
        {
            Debug.Log("GameObject with tag Terrain doesn't exest");
        }
    }
    public class LODMesh
    {
        private int lod;
        private float[,] map;
        private Mesh mesh;
        public LODMesh(int lod, float[,] map)
        {
            this.lod = lod;
            this.map = map;
        }
        public Mesh CreateMesh(Dictionary<int, Mesh> lodDictionary)
        {
            int numVertices = (chunkSize - 1) / lod + 1;
            NativeArray<Vector3> vertices = new NativeArray<Vector3>(numVertices * numVertices, Allocator.Persistent); 
            NativeArray<int> triangles = new NativeArray<int>((numVertices - 1) * (numVertices - 1) * 6, Allocator.Persistent);
            NativeArray<float> mapNativeArray = new NativeArray<float>(chunkSize * chunkSize, Allocator.Persistent);
            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    mapNativeArray[i * chunkSize + j] = map[i, j];
                }
            }

            new MeshCreatorStruct()
            {
                lod = lod,
                numVertices = numVertices,
                vertices = vertices,
                triangles = triangles,
                mapNativeArray = mapNativeArray
            }.Schedule().Complete();

            Vector3[] verticesManaged = new Vector3[vertices.Length];
            int[] trianglesManaged = new int[triangles.Length];
            vertices.CopyTo(verticesManaged);
            triangles.CopyTo(trianglesManaged);

            vertices.Dispose();
            triangles.Dispose();
            mapNativeArray.Dispose();

            Mesh mesh = new Mesh();
            mesh.vertices = verticesManaged;
            mesh.triangles = trianglesManaged;
            mesh.RecalculateNormals();
            if (lodDictionary != null)
            {
                lodDictionary.Add(lod, mesh);
            }
            return mesh;
        }
        public struct MeshCreatorStruct : IJob
        {
            public int lod;
            public int numVertices;
            public NativeArray<Vector3> vertices;
            public NativeArray<int> triangles;
            public NativeArray<float> mapNativeArray;
            public void Execute()
            {
                Debug.Log("MeshCreatorStruct");
                float corner = -chunkSize / 2f;
                float extraDistanceX = 0;
                float extraDistanceZ = 0;
                for (int j = 0; j < chunkSize; j += lod)
                {
                    for (int i = 0; i < chunkSize; i += lod)
                    {
                        int a = (j / lod) * numVertices + (i / lod);
                        vertices[a] = new Vector3(corner + i + extraDistanceX, mapNativeArray[i*chunkSize+ j], corner + j + extraDistanceZ);
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
            }
        }

        /*public Mesh CreateMesh(Dictionary<int, Mesh> lodDictionary)
        {
            int numVertices = (chunkSize - 1) / lod + 1;
            Vector3[] vertices = new Vector3[numVertices * numVertices];
            int[] triangles = new int[(numVertices - 1) * (numVertices - 1) * 6];
            Vector2[] uvs = new Vector2[numVertices * numVertices];
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
                    //uvs[a] = 
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
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            if (lodDictionary != null)
            {
                lodDictionary.Add(lod, mesh);
            }
            return mesh;
        }*/

    }
}

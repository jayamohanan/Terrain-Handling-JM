using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
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
    MeshGenerator meshGenerator;
    Noise noise;
    bool mapGenInit;
    Coord coord;
    public TerrainChunk(NoiseSettings noiseSettings, int chunkSize, int lod, Vector3 position, Material terrainMat, Transform parent, Coord coord)
    {
        this.noiseSettings = noiseSettings;
        this.position = position;
        TerrainChunk.chunkSize = chunkSize;
        this.lod = lod;
        this.parent = parent;
        this.terrainMat = terrainMat;
        this.coord = coord;
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
        terrainChunk = new GameObject();
        terrainChunk.transform.position = position;
        terrainChunk.transform.SetParent(parent);
        meshFilter = terrainChunk.AddComponent<MeshFilter>();
        meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
        meshCollider = terrainChunk.AddComponent<MeshCollider>();
        meshRenderer.material = terrainMat;

        noise = new Noise(noiseSettings);
        ThreadStart threadStart = GetMapOnThread;
        new Thread(threadStart).Start();
    }
    void GetMapOnThread()
    {
        map = noise.GenerateMap();
        
        lock (GameManager.threadInfoMapQueue)
        {
        GameManager.threadInfoMapQueue.Enqueue(new ThreadInfoMap(OnMapDataReceived));
        }
    }
    public void OnMapDataReceived()//this is called from main thread after dequeueing from the threadInfoMap queue
    {
        meshGenerator = new MeshGenerator(map);
        mapGenInit = true;
        Debug.Log("Mesh Generator initialised");
        GenerateLODMeshOnThread(lod, "OnMapDataReceived");
    }
    public void GenerateLODMeshOnThread(int lod, string name)//this iscalled from main thread, i.e b OnMapData received function
    {
        if (lodDictionary.ContainsKey(lod))
        {
            meshFilter.mesh = lodDictionary[lod];
            meshCollider.sharedMesh = lodDictionary[lod];
        }
        else
        {
            ThreadStart threadStart =delegate() { ThreadLODMesh(lod); };
            new Thread(threadStart).Start();
        }
    }
    void ThreadLODMesh(int lod)//thread
    {
        MeshData meshData = meshGenerator.CreateMesh(lod);
        ThreadInfoMesh threadInfoMesh = new ThreadInfoMesh(OnMeshDataReceived, meshData);
        lock (GameManager.threadInfoMeshQueue)
        {
            GameManager.threadInfoMeshQueue.Enqueue(threadInfoMesh);
        }
    }
    public void OnMeshDataReceived(MeshData meshData)//main thread
    {
        Mesh mesh = meshGenerator.AssignMesh(meshData, lodDictionary, lod);
        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;
        if(!GameManager.coordDictionary.ContainsKey(coord))
            GameManager.coordDictionary.Add(coord, this);
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
            MeshGenerator meshGenerator = new MeshGenerator(map);
            MeshData meshData = meshGenerator.CreateMesh(lod);
            Mesh mesh = meshGenerator.AssignMesh(meshData, lodDictionary, lod);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
        else
        {
            Debug.Log("GameObject with tag Terrain doesn't exest");
        }
    }
}
public struct ThreadInfoMesh
{
    public Action<MeshData> action;
    public MeshData meshData;
    public ThreadInfoMesh(Action<MeshData> action, MeshData meshData)
    {
        this.action = action;
        this.meshData = meshData;
    }
}
public struct ThreadInfoMap
{
    public Action action;
    public ThreadInfoMap(Action action)
    {
        this.action = action;
    }
}

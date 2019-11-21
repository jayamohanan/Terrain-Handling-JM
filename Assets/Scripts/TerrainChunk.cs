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
    public MeshGenerator meshGenerator;
    TerrainType[] terrainTypes;
    Noise noise;

    public TerrainChunk(NoiseSettings noiseSettings, int chunkSize, int lod, Vector3 position, Material terrainMat, Transform parent)
    {
        this.noiseSettings = noiseSettings;
        this.position = position;
        TerrainChunk.chunkSize = chunkSize;
        this.lod = lod;
        this.parent = parent;
        this.terrainMat = terrainMat;
        //this.terrainTypes = terrainTypes;
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
        //Creates GameObject
        terrainChunk = new GameObject();
        terrainChunk.transform.position = position;
        terrainChunk.transform.SetParent(parent);
        meshFilter = terrainChunk.AddComponent<MeshFilter>();
        meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
        meshCollider = terrainChunk.AddComponent<MeshCollider>();
        meshRenderer.material = terrainMat;

        //Adds shape
        noise = new Noise(noiseSettings);
        new Thread(MapDataThread).Start();
    }
    void MapDataThread()//From sub thread
    {
        map = noise.GenerateMap();
        lock (GameManager.threadInfoMapQueue)
        {
        GameManager.threadInfoMapQueue.Enqueue(new ThreadInfoMap(OnMapDataReceived));
        }

    }
    public void OnMapDataReceived()//Main Thread
    {
        meshGenerator = new MeshGenerator(map);
        RequestMeshData(lod);
    }
    public void RequestMeshData(int lod)//this iscalled from main thread, i.e b OnMapData received function
    {
        if (lodDictionary.ContainsKey(lod))
        {
            meshFilter.mesh = lodDictionary[lod];
           // meshCollider.sharedMesh = lodDictionary[lod];
        }
        else
        {
            ThreadStart threadStart =delegate{ MeshDataThread(lod); };
            new Thread(threadStart).Start();
        }
    }
    public void MeshDataThread(int lod)//thread
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
        Mesh mesh = meshGenerator.AssignMesh(meshData);
        this.meshFilter.mesh = mesh;
        //this.meshCollider.sharedMesh = mesh;
        if (!lodDictionary.ContainsKey(lod))
        {
            lodDictionary.Add(lod, mesh);
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
            MeshGenerator meshGenerator = new MeshGenerator(map);
            MeshData meshData = meshGenerator.CreateMesh(lod);
            Mesh mesh = meshGenerator.AssignMesh(meshData);
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
        else
        {
            Debug.Log("GameObject with tag Terrain doesn't exist");
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

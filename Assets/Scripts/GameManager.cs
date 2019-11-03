using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public bool mouseMovement;
    public float angularSpeed = 1000f;
    public bool autoUpdate;
    [Range(0, 4)]
    public int lodIndex;
    private int lod;
    int[] lodArray = new int[] { 1, 2, 4, 8, 12, 120, 240 };
    [Range(0, 100)]
    public int scale;
    public int height;
    [Range(1, 6)]
    public int octaves;
    private int chunkSize = 241;
    float[,] map;
    MeshFilter meshFilter;
    public bool inverseLerp;
    public float persistence;
    [Range(1, 10)]
    public float lacunarity;
    public AnimationCurve animationCurve;
    [Header("Grid")]
    public int gridCount;
    public static Dictionary<Coord, TerrainChunk> coordDictionary = new Dictionary<Coord, TerrainChunk>();
    List<Coord> activeCoord = new List<Coord>();
    Transform parent;
    Vector3 lastPosition;
    public Material terrainMat;
    Vector2 offset;
    GameObject editorTerrain;
    MeshFilter editorMeshFilter;
    NoiseSettings noiseSettings;
    Vector3 mouseWorldPosition;
    public static Queue<ThreadInfoMesh> threadInfoMeshQueue = new Queue<ThreadInfoMesh>();
    public static Queue<ThreadInfoMap> threadInfoMapQueue = new Queue<ThreadInfoMap>();

    
    void Start()
    {
        parent = new GameObject("Parent").transform;
        InitializeAndClear();
        GetCurrentCoord();

    }
    
    public void InitializeAndClear()
    {
        activeCoord.Clear();
        coordDictionary.Clear();

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
        Coord coord = GetCoordFromWorldPosition(mouseWorldPosition);
    }
    private void GetCurrentCoord()
    {
        if (mouseMovement)
        {
            //Terrain along mousePosition
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
        }
        else
        {
            //Terrain along X axis, used for checking frame rates
            mouseWorldPosition += new Vector3(100, 0, 0);
        }
        float deltaDistance = Vector3.Distance(mouseWorldPosition, lastPosition);
        if (deltaDistance > 150)
        {
            lastPosition = mouseWorldPosition;
            deltaDistance = 0;
            Coord coord = GetCoordFromWorldPosition(mouseWorldPosition);
            CreateGrid(coord);
        }

    }
    private void Update()
    {
        GetCurrentCoord();
        while (threadInfoMeshQueue.Count > 0)
        {
            ThreadInfoMesh threadInfoMesh = threadInfoMeshQueue.Dequeue();
            threadInfoMesh.action(threadInfoMesh.meshData);
        }
        while (threadInfoMapQueue.Count > 0)
        {
            ThreadInfoMap threadInfoMap = threadInfoMapQueue.Dequeue();
            threadInfoMap.action();
        }
    }
    public Coord GetCoordFromWorldPosition(Vector3 mouseWorldPosition)
    {
    /*
       Called when looking to change the location of terrain chunk  
    */
    foreach (Coord coords in activeCoord)
    {
        coordDictionary[coords].terrainChunk.SetActive(false);
    }
    activeCoord.Clear();
    Coord coord = new Coord(Mathf.RoundToInt(mouseWorldPosition.x / chunkSize), Mathf.RoundToInt(mouseWorldPosition.z / chunkSize));
    return coord;
    }
    public void CreateGrid(Coord coord)
    {
        int top;
        int bottom;
        if (gridCount % 2 == 0)
        {
            bottom = Mathf.FloorToInt(gridCount / 2);
            top = Mathf.FloorToInt(gridCount / 2) - 1;
        }
        else
        {
            bottom = top = Mathf.FloorToInt(gridCount / 2);
        }
        for (int j = -bottom; j <= top; j++)
        {
            for (int i = -bottom; i <= top; i++)
            {
                if (i == 0 && j == 0)
                {
                    lodIndex = 0;
                }
                else if (Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)
                {
                    lodIndex = 3;
                }
                else
                {
                    lodIndex = 6;
                }
                lod = lodArray[lodIndex];
                Coord neighbourCoord = new Coord((coord.xCoord + i), (coord.yCoord + j));
                //call thread to calculate mesh
                //thread then calls call back to this object and adds the mesh to terrain chunk if any or else create terrain chunk and add
                //
                if (coordDictionary.ContainsKey(neighbourCoord))
                {
                    if (coordDictionary[neighbourCoord].meshGenerator != null)
                    {
                        coordDictionary[neighbourCoord].RequestMeshData(lod);
                    }
                    coordDictionary[neighbourCoord].terrainChunk.SetActive(true);
                    
                }
                else
                {
                    Vector3 position = new Vector3(neighbourCoord.xCoord * chunkSize, 0, neighbourCoord.yCoord * chunkSize);
                    offset = new Vector2(neighbourCoord.xCoord, neighbourCoord.yCoord) * chunkSize;
                    noiseSettings = new NoiseSettings(chunkSize, octaves, persistence, lacunarity, scale, offset, height, animationCurve, inverseLerp);
                    TerrainChunk terrainChunk = new TerrainChunk(noiseSettings, chunkSize, lod, position, terrainMat, parent);
                    coordDictionary.Add(neighbourCoord, terrainChunk);
                }
                activeCoord.Add(neighbourCoord);
                

            }
        }
    }
    public void CreateTerrainChunkInEditor()
    {
        lod = lodArray[lodIndex];
        offset = Vector2.zero;
        noiseSettings = new NoiseSettings(chunkSize, octaves, persistence, lacunarity, scale, offset, height, animationCurve, inverseLerp);
        new TerrainChunk(noiseSettings, chunkSize, lod, Vector3.zero, terrainMat, parent);
    }
    void OnValidate()
    {
        if (persistence <= 0)
        {
            persistence = 0.01f;
        }
        if (persistence > 1)
        {
            persistence = 1;
        }
        if (chunkSize < 2)
        {
            chunkSize = 2;
        }//jaya
    }
}
public struct Coord
{
    public int xCoord;
    public int yCoord;
    public Coord(int xCoord, int yCoord)
    {
        this.xCoord = xCoord;
        this.yCoord = yCoord;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public float angularSpeed = 1f;
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
    Vector3 mouseWorldPosition;
    //public GameObject obj;
    Dictionary<Coord, TerrainChunk> coordDictionary = new Dictionary<Coord, TerrainChunk>();
    List<Coord> activeCoord = new List<Coord>();
    Transform parent;
    Vector3 lastPosition;
    public Material terrainMat;
    Vector2 offset;
    GameObject editorTerrain;
    MeshFilter editorMeshFilter;
    Vector2 mousePositionXZ;
    NoiseSettings noiseSettings;
    bool initialTerrainDrawn = false;
    void Start()
    {
        parent = new GameObject("Parent").transform;
        InitializeAndClear();
    }
    public void InitializeAndClear()
    {
        activeCoord.Clear();
        coordDictionary.Clear();
    }
    private void Update()
    {
        //Terrain at Mouse Point
        //mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));

        //Terrain along X axis, used for checking frame rates
        mouseWorldPosition += new Vector3(100, 0, 0);

        float deltaDistance = Vector3.Distance(mouseWorldPosition, lastPosition);
        if (!initialTerrainDrawn)
        {
            lastPosition = mouseWorldPosition;
            Coord coord = GetCoordFromWorldPosition(mouseWorldPosition);
            CreateGrid(coord);
            initialTerrainDrawn = true;
        }
        if (deltaDistance > 150 )
        {
            lastPosition = mouseWorldPosition;
            deltaDistance = 0;
            Coord coord = GetCoordFromWorldPosition(mouseWorldPosition);
            CreateGrid(coord);
        }
    }
    public Coord GetCoordFromWorldPosition(Vector3 mouseWorldPosition)
    {
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
                if (coordDictionary.ContainsKey(neighbourCoord))
                {
                    coordDictionary[neighbourCoord].GenerateLODMesh(lod);
                    coordDictionary[neighbourCoord].terrainChunk.SetActive(true);
                    activeCoord.Add(neighbourCoord);
                }
                else
                {
                    Vector3 position = new Vector3(neighbourCoord.xCoord * chunkSize, 0, neighbourCoord.yCoord * chunkSize);
                    offset = new Vector2(neighbourCoord.xCoord, neighbourCoord.yCoord) * chunkSize;
                    noiseSettings = new NoiseSettings(chunkSize, octaves, persistence, lacunarity, scale, offset, height, animationCurve, inverseLerp);
                    TerrainChunk terrainChunk = new TerrainChunk(noiseSettings, chunkSize, lod, position, terrainMat, parent);
                    activeCoord.Add(neighbourCoord);
                    coordDictionary.Add(neighbourCoord, terrainChunk);
                }
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
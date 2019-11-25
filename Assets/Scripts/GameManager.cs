using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    #region Variables
    public Transform player;
    public bool mouseMovement;
    public float angularSpeed;
    public bool autoUpdate;
    [Range(0, 4)]
    private int lodIndex =0;
    private int lod = 1;
    int[] lodArray = new int[] { 1, 2, 4, 8, 12, 120, 240 };
    [HideInInspector] public bool mapDataFoldout;
    [HideInInspector] public bool terrainDataFoldout;

    float[,] map;
    MeshFilter meshFilter;
    public bool inverseLerp;
    
    [Header("Grid")]
    public int gridCount;
    public static Dictionary<Coord, TerrainChunk> coordDictionary = new Dictionary<Coord, TerrainChunk>();
    List<Coord> activeCoord = new List<Coord>();
    Transform parent;
    Vector3 lastPosition = new Vector3(1000,0,1000);
    public Material terrainMat;
    Vector2 offset;
    GameObject editorTerrain;
    MeshFilter editorMeshFilter;
    NoiseSettings noiseSettings;
    Vector3 mouseWorldPosition;
    public static Queue<ThreadInfoMesh> threadInfoMeshQueue = new Queue<ThreadInfoMesh>();
    public static Queue<ThreadInfoMap> threadInfoMapQueue = new Queue<ThreadInfoMap>();

    [HideInInspector] public MapData mapData;//scriptable object contents will be shown by calling a dedicated edior function
    [HideInInspector] public TerrainData terrainData;//scriptable object contents will be shown by calling a dedicated edior function
    #endregion

    void Start()
    {
        SetShaderValues();
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
            //mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
            mouseWorldPosition = new Vector3(player.transform.position.x, 0, player.transform.position.z);
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
    Coord coord = new Coord(Mathf.RoundToInt(mouseWorldPosition.x / mapData.chunkSize), Mathf.RoundToInt(mouseWorldPosition.z / mapData.chunkSize));
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
                    lodIndex = 0;
                }
                else
                {
                    lodIndex = 2;
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
                    Vector3 position = new Vector3(neighbourCoord.xCoord * (mapData.chunkSize - 1), 0, neighbourCoord.yCoord * (mapData.chunkSize - 1));
                    offset = new Vector2(neighbourCoord.xCoord, neighbourCoord.yCoord) * (mapData.chunkSize-1);
                    noiseSettings = new NoiseSettings(mapData.chunkSize, mapData.octaves, mapData.persistence, mapData.lacunarity, mapData.scale, offset, mapData.height, mapData.animationCurve, inverseLerp);
                    TerrainChunk terrainChunk = new TerrainChunk(noiseSettings, mapData.chunkSize, lod, position, terrainMat, parent);
                    coordDictionary.Add(neighbourCoord, terrainChunk);
                }
                activeCoord.Add(neighbourCoord);
            }
        }
    }

    public void CreateTerrainChunkInEditor()
    {
        if (!Application.isPlaying)
        {
            lod = lodArray[lodIndex];
            offset = Vector2.zero;
            noiseSettings = new NoiseSettings(mapData.chunkSize, mapData.octaves, mapData.persistence, mapData.lacunarity, mapData.scale, offset, mapData.height, mapData.animationCurve, inverseLerp);
            SetShaderValues();
            new TerrainChunk(noiseSettings, mapData.chunkSize, lod, Vector3.zero, terrainMat, parent);
        }
    }   
    public void SetShaderValues()
    {
        mapData.SetShaderHeights(terrainMat);
        terrainData.SetTerrainData(terrainMat);
    }
    private void OnValidate()
    {
        if (mapData != null)
        {
            mapData.DataUpdateEvent -= CreateTerrainChunkInEditor;
            mapData.DataUpdateEvent += CreateTerrainChunkInEditor;
        }
        if(terrainData!=null)
        {
            terrainData.DataUpdateEvent -= CreateTerrainChunkInEditor;
            terrainData.DataUpdateEvent += CreateTerrainChunkInEditor;
        }
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


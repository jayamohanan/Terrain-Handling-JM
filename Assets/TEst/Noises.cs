using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Noises : MonoBehaviour
{
    public ComputeShader shader;
    Renderer rend;
    RenderTexture myRt;
    bool started;

    public int resolution = 256;
    public int unitWidth;
    public int unitLength;
    public int depth;
    
    int kernel;
    ComputeBuffer ponitsBuffer;
    ComputeBuffer dimensionBuffer;
    ComputeBuffer maxDstBuffer;
    void Start()
    {
        myRt = new RenderTexture(resolution, resolution, 24);
        myRt.enableRandomWrite = true;
        myRt.Create();
        rend = GetComponent<Renderer>();
        
        SetComputeBuffer();
        started = true;

    }
    void SetComputeBuffer()
    {
        Grid grid = new Grid(resolution, unitWidth, unitLength, depth);
        Debug.Log("new grid created with "+ " resolution = "+ resolution+" unitWidth "+unitWidth+" unitLength "+unitLength);
        Vector2[] points = grid.CreateGrid();
        kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Result", myRt);
        ponitsBuffer = new ComputeBuffer(grid.rows * grid.columns * grid.depth, sizeof(float) * 2, ComputeBufferType.Default);
        ponitsBuffer.SetData(points);
        shader.SetBuffer(kernel, "points", ponitsBuffer);

        float[] unitDimensions = new float[] {grid.unitWidth, grid.unitLength, grid.rows, grid.columns };
        dimensionBuffer = new ComputeBuffer(4, sizeof(float), ComputeBufferType.Default);
        dimensionBuffer.SetData(unitDimensions);
        shader.SetBuffer(kernel, "unitDimensions", dimensionBuffer);

        float[] maxDst = new float[] {Mathf.Sqrt(Mathf.Pow(grid.unitLength*2,2)+ Mathf.Pow(grid.unitWidth * 2, 2) )};
        maxDst[0] *= 0.4f;//else too big value
        maxDstBuffer = new ComputeBuffer(1, sizeof(float), ComputeBufferType.Default);
        maxDstBuffer.SetData(maxDst);
        shader.SetBuffer(kernel, "maxDst", maxDstBuffer);


        shader.Dispatch(kernel, resolution / 8, resolution / 8, 1);

        rend.material.SetTexture("_MainTex", myRt);
    }
    private void OnValidate()
    {
        if (started)
        {
            SetComputeBuffer();
        }
    }
}

public class Grid
{
    public int resolution;
    public int unitWidth;
    public int unitLength;
    public int depth;
    public int rows;
    public int columns;
    public Grid(int resolution, int unitWidth, int unitLength, int depth)
    {
        this.resolution = resolution;
        this.unitWidth = unitWidth;
        this.unitLength = unitLength;
        this.depth = depth;
        rows = (int)Mathf.Ceil(resolution / (float)unitLength);
        columns = (int)Mathf.Ceil(resolution / (float)unitWidth);
        Debug.Log("Constructor called rows = "+rows+" columns "+columns);
    }
    public Vector2[] CreateGrid()
    {
        Vector2[] points = new Vector2[rows * columns * depth];
        for (int k = 0; k < depth; k++)
        {
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    points[k*rows*columns + i * rows + j] = new Vector2(Random.Range(i * unitWidth, (i + 1) * unitWidth), Random.Range(j * unitLength, (j + 1) * unitLength));
                }
            }
        }
        return points;
    }
}
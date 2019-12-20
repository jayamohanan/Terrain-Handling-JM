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
    public int unitDepth;
    
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
        Grid grid = new Grid(resolution, unitWidth, unitLength, unitDepth);
        Debug.Log("new grid created with "+ " resolution = "+ resolution+" unitWidth "+unitWidth+" unitLength "+unitLength);
        Vector3[] points = grid.Create3DGrid();
        kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Result", myRt);
        ponitsBuffer = new ComputeBuffer(grid.rows * grid.columns * grid.depth, sizeof(float) * 3, ComputeBufferType.Default);
        ponitsBuffer.SetData(points);
        shader.SetBuffer(kernel, "points", ponitsBuffer);

        Vector3[] unitDimensions = new Vector3[] {new Vector3( grid.unitWidth, grid.unitLength, grid.unitDepth), new Vector3(grid.rows, grid.columns, grid.stacks) };
        dimensionBuffer = new ComputeBuffer(2, sizeof(float)*3, ComputeBufferType.Default);//size along axes and cells along axis
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
    public int unitDepth;

    public int depth;
    public int rows;
    public int columns;
    public int stacks;
    public Grid(int resolution, int unitWidth, int unitLength, int unitDepth)
    {
        this.resolution = resolution;
        this.unitWidth = unitWidth;
        this.unitLength = unitLength;
        this.unitDepth = unitDepth;

        rows = (int)Mathf.Ceil(resolution / (float)unitLength);
        columns = (int)Mathf.Ceil(resolution / (float)unitWidth);
        stacks = (int)Mathf.Ceil(resolution / (float)unitDepth);

        Debug.Log("Constructor called rows = "+rows+" columns "+columns);
    }
    public Vector3[] Create3DGrid()
    {
        Vector3[] points = new Vector3[rows * columns * depth];
        for (int k = 0; k < stacks; k++)
        {
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    points[k*rows*columns + i * rows + j] = new Vector3(Random.Range(i * unitWidth, (i + 1) * unitWidth), Random.Range(j * unitLength, (j + 1) * unitLength), Random.Range(k * unitDepth, (k + 1) * unitDepth));
                }
            }
        }
        return points;
    }
}
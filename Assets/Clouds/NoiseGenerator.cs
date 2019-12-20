using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float[] CreateNoise(int gridSize, System.Random prng)
    {
        float[] points = new float[gridSize*gridSize*gridSize];
        for (int k  = 0; k < gridSize; k++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                for (int i = 0; i < gridSize; i++)
                {
                    points[i + j * gridSize + k * gridSize * gridSize] = (float)prng.NextDouble();
                }
            }
        }
        return points;
    }

    void CreateTexture(int dimensions)
    {
        //var tex = new Text
        Texture3D a;
    }
}

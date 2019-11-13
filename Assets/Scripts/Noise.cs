using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Noise
{
    int chunkSize;
    int octaves;
    float scale;
    float persistence;
    float lacunarity;
    bool inverseLerp;
    AnimationCurve animationCurve;
    Vector3 offset;
    float height;

    private static int count =0;
    public Noise(NoiseSettings noiseSettings)
    {
        this.chunkSize = noiseSettings.chunkSize;
        this.octaves = noiseSettings.octaves;
        this.scale = noiseSettings.scale;
        this.persistence = noiseSettings.persistence;
        this.lacunarity = noiseSettings.lacunarity;
        this.inverseLerp = noiseSettings.inverseLerp;
        //this.animationCurve = noiseSettings.animationCurve;
        this.animationCurve = new AnimationCurve(noiseSettings.animationCurve.keys);
        this.offset = noiseSettings.offset;
        this.height = noiseSettings.height;
    }
    public float[,] GenerateMap()
    {
        float[,] map = new float[chunkSize, chunkSize];
        float frequency;
        float amplitude = 1;
        float maxAmplitude = 0;
        float maxValue = float.MinValue;
        float minValue = float.MaxValue;
        for (int i = 0; i < octaves; i++)
        {
            maxAmplitude += amplitude;
            amplitude *= persistence;
        }
        for (int i = 0;  i< chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                
                amplitude = 1;
                frequency = 1;
                float perlinX = ((i + offset.x) / (float)chunkSize) * scale;
                float perlinY = ((j + offset.y) / (float)chunkSize) * scale;
                for (int k = 0; k < octaves; k++)
                {
                    map[i, j] += Mathf.PerlinNoise(perlinX * frequency, perlinY * frequency) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
            }
        }
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                if (map[i, j] >= maxValue)
                    maxValue = map[i, j];
                if (map[i, j] <= minValue)
                    minValue = map[i, j];
            }
        }
        minValue *= 0.5f;
        maxAmplitude /= 1.5f;
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                float heightValue = Mathf.Clamp01((map[i, j]-minValue) / maxAmplitude);
                map[i, j] = animationCurve.Evaluate(heightValue) * height;
                if (count<100)
                {
                    Debug.Log(map[i,j]);
                    count++;
                }
                //map[i, j] = heightValue * height;
            }
        }
        return map;
    }
}

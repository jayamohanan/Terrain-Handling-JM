using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
public class Noise
{
    private NoiseSettings noiseSettings;
    public Noise(NoiseSettings noiseSettings)
    {
        this.noiseSettings = noiseSettings;
    }
    public float[,] GenerateMap()
    {
        float[,] map = new float[noiseSettings.chunkSize,noiseSettings.chunkSize];
        float[] map1D = new float[noiseSettings.chunkSize*noiseSettings.chunkSize];
       NativeArray<float> mapNativeArray = new NativeArray<float>(noiseSettings.chunkSize*noiseSettings.chunkSize, Allocator.Temp);
        new MapCreatorStruct(noiseSettings, ref mapNativeArray).Schedule().Complete();
        mapNativeArray.CopyTo(map1D);
        int width = map.GetLength(0);
        int length = map.GetLength(1);
        int x = 0;
        int y = 0;
        for (int i = 0; i < map1D.Length; i++)
        {
            map[x, y] = map1D[i];
            if(i == width - 1)
            {
                x = 0;
                y++;
            }
        }
        return map;
    }
}

struct MapCreatorStruct : IJob
{
    private NoiseSettings noiseSettings;
    private NativeArray<float> mapNativeArray;
    public MapCreatorStruct(NoiseSettings noiseSettings, ref NativeArray<float> mapNativeArray)
    {
        this.noiseSettings = noiseSettings;
        this.mapNativeArray = mapNativeArray;
    }
    public void Execute()
    {
        float[,] map = new float[noiseSettings.chunkSize, noiseSettings.chunkSize];
        float frequency;
        float amplitude = 1;
        float maxAmplitude = 0;
        float maxValue = float.MinValue;
        float minValue = float.MaxValue;
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            maxAmplitude += amplitude;
            amplitude *= noiseSettings.persistence;
        }
        for (int j = 0; j < noiseSettings.chunkSize; j++)
        {
            for (int i = 0; i < noiseSettings.chunkSize; i++)
            {
                amplitude = 1;
                frequency = 1;
                float perlinX = ((i + noiseSettings.offset.x) / (float)noiseSettings.chunkSize) * noiseSettings.scale;
                float perlinY = ((j + noiseSettings.offset.y) / (float)noiseSettings.chunkSize) * noiseSettings.scale;
                for (int k = 0; k < noiseSettings.octaves; k++)
                {
                    map[i, j] += Mathf.PerlinNoise(perlinX * frequency, perlinY * frequency) * amplitude;
                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }
            }
        }
        for (int i = 0; i < noiseSettings.chunkSize; i++)
        {
            for (int j = 0; j < noiseSettings.chunkSize; j++)
            {
                if (map[i, j] >= maxValue)
                    maxValue = map[i, j];
                if (map[i, j] < minValue)
                    minValue = map[i, j];
            }
        }
        for (int i = 0; i < noiseSettings.chunkSize; i++)
        {
            for (int j = 0; j < noiseSettings.chunkSize; j++)
            {
                map[i, j] = noiseSettings.animationCurve.Evaluate(Mathf.InverseLerp(minValue, maxValue, map[i, j])) * noiseSettings.height;
            }
        }
    }
}
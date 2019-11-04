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
        NativeArray<float> mapNativeArray = new NativeArray<float>(noiseSettings.chunkSize*noiseSettings.chunkSize, Allocator.Persistent);
        new MapCreatorStruct(noiseSettings, mapNativeArray).Schedule().Complete();
        map = ChangeArrayDimension(mapNativeArray);
        mapNativeArray.Dispose();
       
        return map;
    }
    public static float[,] ChangeArrayDimension(NativeArray<float> a)
    {
        int arrayLength = a.Length;
        int width = (int)Mathf.Sqrt(arrayLength);
        float[,] b = new float[width, width];

        for (int i = 0; i < arrayLength; i++)
        {
            b[(i / width), (i % width)] = a[i];
        }
        return b;
    }

}

struct MapCreatorStruct : IJob
{
    private NoiseSettings noiseSettings;
    private NativeArray<float> mapNativeArray;
    public MapCreatorStruct(NoiseSettings noiseSettings, NativeArray<float> mapNativeArray)
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
        for (int i = 0; i < noiseSettings.chunkSize; i++)
        {
            for (int j = 0; j < noiseSettings.chunkSize; j++)
            {
                amplitude = 1;
                frequency = 1;
                float perlinX = ((i + noiseSettings.offset.x) / (float)noiseSettings.chunkSize) * noiseSettings.scale;
                float perlinY = ((j + noiseSettings.offset.y) / (float)noiseSettings.chunkSize) * noiseSettings.scale;
                for (int k = 0; k < noiseSettings.octaves; k++)
                {
                    mapNativeArray[i* noiseSettings.chunkSize+ j] += Mathf.PerlinNoise(perlinX * frequency, perlinY * frequency) * amplitude;
                    amplitude *= noiseSettings.persistence;
                    frequency *= noiseSettings.lacunarity;
                }
            }
        }
       
        for (int i = 0; i < noiseSettings.chunkSize; i++)
        {
            for (int j = 0; j < noiseSettings.chunkSize; j++)
            {
                if (mapNativeArray[i * noiseSettings.chunkSize + j] >= maxValue)
                    maxValue = mapNativeArray[i * noiseSettings.chunkSize + j];
                if (mapNativeArray[i * noiseSettings.chunkSize + j] < minValue)
                    minValue = mapNativeArray[i * noiseSettings.chunkSize + j];
            }
        }
        for (int i = 0; i < noiseSettings.chunkSize; i++)
        {
            for (int j = 0; j < noiseSettings.chunkSize; j++)
            {
                int index = (int)(Mathf.Clamp01(Mathf.InverseLerp(minValue, maxValue, mapNativeArray[i * noiseSettings.chunkSize + j])) * 255);
                mapNativeArray[i * noiseSettings.chunkSize + j] = GameManager.animationKeys[index] * noiseSettings.height;
            }
        }
    }
}
﻿using System.Collections;
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
    public Noise(NoiseSettings noiseSettings)
    {
        this.chunkSize = noiseSettings.chunkSize;
        this.octaves = noiseSettings.octaves;
        this.scale = noiseSettings.scale;
        this.persistence = noiseSettings.persistence;
        this.lacunarity = noiseSettings.lacunarity;
        this.inverseLerp = noiseSettings.inverseLerp;
        this.animationCurve = noiseSettings.animationCurve;
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
        for (int j = 0; j < chunkSize; j++)
        {
            for (int i = 0; i < chunkSize; i++)
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
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                map[i, j] = animationCurve.Evaluate(Mathf.InverseLerp(minValue, maxValue, map[i, j])) * height;
            }
        }
        return map;
    }
}

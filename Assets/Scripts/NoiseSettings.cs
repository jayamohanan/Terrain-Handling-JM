using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct NoiseSettings
{
    public int chunkSize;
    public int octaves;
    public float lacunarity;
    public int scale;
    public bool inverseLerp;
    public float persistence;
    public Vector2 offset;
    public float height;
    public NoiseSettings(int chunkSize, int octaves, float persistence, float lacunarity, int scale, Vector2 offset, float height, bool inverseLerp)
    {
        this.chunkSize = chunkSize;
        this.octaves = octaves;
        this.lacunarity = lacunarity;
        this.scale = scale;
        this.inverseLerp = inverseLerp;
        this.persistence = persistence;
        this.offset = offset;
        this.height = height;
    }
}

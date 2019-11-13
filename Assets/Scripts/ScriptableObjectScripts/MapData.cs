using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MapData", menuName ="Window/Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    [Range(0, 100)] public int scale;
    public int height;
    [Range(1, 6)] public int octaves;
    [HideInInspector] public int chunkSize = 241;
    [Range(0, 1)] public float persistence;
    [Range(1, 10)] public float lacunarity;
    public AnimationCurve animationCurve;

    private void OnValidate()
    {
        if (chunkSize < 2)
            chunkSize = 2;
    }
}

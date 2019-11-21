using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MapData", menuName ="Scriptable Objects/MapData")]
public class MapData : UpdatableData
{
    [Range(0, 100)] public int scale;
    public int height;
    [Range(1, 6)] public int octaves;
    [HideInInspector] public int chunkSize = 241;
    [Range(0, 1)] public float persistence;
    [Range(1, 10)] public float lacunarity;
    public AnimationCurve animationCurve;

    public float minHeight
    {
        get { return height * animationCurve.Evaluate(0); }
    }
    public float maxHeight
    {
        get { return height * animationCurve.Evaluate(1); }
    }

    public void SetShaderHeights(Material terrainMat)
    {
        terrainMat.SetFloat("minHeight", minHeight);
        terrainMat.SetFloat("maxHeight", maxHeight);
    }


    protected override void OnValidate()
    {
        if (chunkSize < 2)
            chunkSize = 2;
        base.OnValidate();
    }
}

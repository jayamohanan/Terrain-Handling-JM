using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName ="Terrain Data", menuName = "Scriptable Objects/Terrain Data")]
public class TerrainData : UpdatableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    int layerCount;
    public TerrainType[] terrainTypes;
        
    public void SetTerrainData(Material terrainMat)
    {
        layerCount = terrainTypes.Length;

        terrainMat.SetInt("layerCount", layerCount);
        terrainMat.SetFloatArray("startHeight", terrainTypes.Select(x=>x.startHeight).ToArray());
        terrainMat.SetColorArray("tintColor", terrainTypes.Select(x => x.tintColor).ToArray());
        terrainMat.SetFloatArray("tintStrength", terrainTypes.Select(x => x.tintStrength).ToArray());
        terrainMat.SetFloatArray("blendStrength", terrainTypes.Select(x => x.blendStrength).ToArray());
        terrainMat.SetTexture("textureArray", ConvertToTexture2DArray(terrainTypes.Select(x=>x.texture).ToArray()));
    }
    Texture2DArray ConvertToTexture2DArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
[System.Serializable]
public struct TerrainType
{
    public string name;
    public Texture2D texture;
    [Range(0,1)]
    public float startHeight;
    public Color tintColor;
    public float tintStrength;
    [Range(0,0.25f)]
    public float blendStrength;
}
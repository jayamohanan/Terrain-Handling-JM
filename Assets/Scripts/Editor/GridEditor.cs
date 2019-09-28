using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GameManager))]
public class GridEditor : Editor
{
    bool firstTime = true;
    public override void OnInspectorGUI()
    {
        GameManager GameManager = (GameManager)target;
        if (DrawDefaultInspector())
        {
            if (GameManager.autoUpdate)
            {
                if (firstTime)
                {
                    GameManager.InitializeAndClear();
                    firstTime = false;
                }
                GameManager.CreateTerrainChunkInEditor();
            }
        }
        if (GUILayout.Button("Generate"))
        {
            if (firstTime)
            {
                GameManager.InitializeAndClear();
                firstTime = false;
            }
            GameManager.CreateTerrainChunkInEditor();
        }
    }
}

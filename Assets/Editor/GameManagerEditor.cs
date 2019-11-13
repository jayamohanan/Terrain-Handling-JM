using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    GameManager gManager;
    private void OnEnable()
    {
        gManager = (GameManager)target;
    }
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (DrawDefaultInspector())
        {
            if (gManager.autoUpdate)
            {
                gManager.CreateTerrainChunkInEditor();
            }
            if (GUILayout.Button("Generate"))
            {
                gManager.CreateTerrainChunkInEditor();
            }
        }
        
    }
}

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
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            DrawScriptableObjectsEditor(gManager.mapData, ref gManager.mapDataFoldout);
            if (check.changed)
            {
                if (gManager.autoUpdate)
                {
                    gManager.CreateTerrainChunkInEditor();
                }
            }
        }
        if (GUILayout.Button("Generate"))
        {
            gManager.CreateTerrainChunkInEditor();
        }
    }
    private void DrawScriptableObjectsEditor(Object so,ref bool foldout)
    {   
        Editor editor = CreateEditor(so);
        foldout = EditorGUILayout.InspectorTitlebar(foldout, so);
        if (foldout)
        {
            editor.OnInspectorGUI();
        }
    }
}

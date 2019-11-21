using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    UpdatableData updatableData;
    private void OnEnable()
    {
        updatableData = (UpdatableData)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Update Data"))
        {
            updatableData.RaiseDataUpdateEvent();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu()]
public class UpdatableData : ScriptableObject
{
    public bool autoUpdate;
    public event Action DataUpdateEvent;//event having Action type subscribers
    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update+= RaiseDataUpdateEvent;
        }
    }
    public void RaiseDataUpdateEvent()
    {
        UnityEditor.EditorApplication.update -= RaiseDataUpdateEvent;
        DataUpdateEvent?.Invoke();
    }
}

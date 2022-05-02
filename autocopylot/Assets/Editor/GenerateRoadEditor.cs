using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateRoad))]
public class GenerateRoadEditor : Editor
{
    private GenerateRoad script;

    private void OnEnable()
    {
        script = (GenerateRoad)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("generate"))
            script.Start();

    }
}

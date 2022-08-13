using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ENV))]
public class EnvEditor : Editor
{
    private ENV script;

    private void OnEnable()
    {
        script = (ENV)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate ENV"))
        {
        }

        if (GUILayout.Button("Reset ENV"))
        {
        }
    }
}

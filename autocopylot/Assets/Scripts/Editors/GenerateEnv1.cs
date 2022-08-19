using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Env))]
public class EnvEditor : Editor
{
    private Env script;

    private void OnEnable()
    {
        script = (Env)target;
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

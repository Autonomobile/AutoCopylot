using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateEnv))]
public class GenerateEnvEditor : Editor
{
    private GenerateEnv script;

    private void OnEnable()
    {
        script = (GenerateEnv)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("generate env"))
        {
            script.Start();
        }

        if (GUILayout.Button("reset env"))
        {
            script.Reset();
        }
    }
}

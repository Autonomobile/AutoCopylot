using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvGenerator))]
public class GenerateEnvEditor : Editor
{
    private EnvGenerator script;

    private void OnEnable()
    {
        script = (EnvGenerator)target;
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
            script.ResetEnv();
        }
    }
}

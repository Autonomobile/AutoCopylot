using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarPath))]
public class CarPathEditor : Editor {

    private CarPath script;

    private void OnEnable() {
        script = (CarPath) target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("create path")) {
            script.Start();
        }
    }
}

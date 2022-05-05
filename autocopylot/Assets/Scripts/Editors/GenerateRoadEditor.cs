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

        if (GUILayout.Button("generate road"))
        {
            script.GenerateSplines();
        }

        if (GUILayout.Button("save path"))
        {
            GenerateRoad.SavePath(script.RoadSpline, script.RoadSplineFolder + script.Name);
            GenerateRoad.SavePath(script.TrajectorySpline, script.TrajectorySplineFolder + script.Name);
        }

        if (GUILayout.Button("load path"))
        {
            script.LoadSplines(script.Name);
        }
    }
}

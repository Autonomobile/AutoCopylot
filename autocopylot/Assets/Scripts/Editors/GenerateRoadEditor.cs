using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateRoad))]
public class GenerateRoadEditor : Editor
{

    [Header("Spline Generator")]

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
            Vector3[] points = script.RandomPoints();
            GenerateRoad.ApplyPath(script.RoadSpline, points);
            GenerateRoad.ApplyPath(script.TrajectorySpline, points);
            script.PathUpdated();
        }

        if (GUILayout.Button("save path"))
        {
            GenerateRoad.SavePath(script.RoadSpline, script.RoadSplineFolder + script.Name);
            GenerateRoad.SavePath(script.TrajectorySpline, script.TrajectorySplineFolder + script.Name);
        }

        if (GUILayout.Button("load path"))
        {
            GenerateRoad.LoadPath(script.RoadSpline, script.TrajectorySplineFolder + script.Name);
            GenerateRoad.LoadPath(script.TrajectorySpline, script.TrajectorySplineFolder + script.Name);
            script.PathUpdated();
        }
        
        if (GUILayout.Button("Update"))
            script.PathUpdated();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadGenerator))]
public class GenerateRoadEditor : Editor
{

    private RoadGenerator script;

    private void OnEnable()
    {
        script = (RoadGenerator)target;
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
            RoadGenerator.SavePath(script.RoadSpline, script.RoadSplineFolder + script.Name);
            RoadGenerator.SavePath(script.TrajectorySpline, script.TrajectorySplineFolder + script.Name);
        }

        if (GUILayout.Button("load path"))
        {
            script.LoadSplines(script.Name);
        }
    }
}

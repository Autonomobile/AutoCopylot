using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


[RequireComponent(typeof(PathCreator))]
public class GenerateRoad : MonoBehaviour
{
    [Header("Spline Generator")]
    public int NumPoints = 3;
    public float MinDist = 5.0f;
    public bool closedLoop = true;
    private Transform[] waypoints;

    public void Start()
    {
        // create random transforms on xz axis with a min distance between them
        waypoints = new Transform[NumPoints];
        for (int i = 0; i < NumPoints; i++)
        {
            waypoints[i] = new GameObject("Waypoint " + i).transform;
            waypoints[i].parent = transform;
            if (i > 0)
            {
                waypoints[i].position = waypoints[i - 1].position;
                while (Vector3.Distance(waypoints[i - 1].position, waypoints[i].position) < MinDist)
                    waypoints[i].position += new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            }
        }

        // create path from random transforms
        BezierPath bezierPath = new BezierPath(waypoints, closedLoop, PathSpace.xz);
        GetComponent<PathCreator>().bezierPath = bezierPath;

    }
}

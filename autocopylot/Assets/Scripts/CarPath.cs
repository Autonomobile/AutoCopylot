using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CarPath : MonoBehaviour
{
    public PathCreator RoadSpline;
    public PathCreator CarSpline;

    public float RandomDist = 0.5f;
    public float speed = 1.0f;

    float t = 0.0f;
    float timesteps = 0.033f;

    
    public void Start()
    {
        CreateCarSpline();
        UpdateTransform(t);
    }

    public void CreateCarSpline()
    {
        int step = 6;
        int numPoints = RoadSpline.path.NumPoints / step;
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = RoadSpline.path.GetPoint(i * step) - RoadSpline.transform.position + RoadSpline.path.GetNormal(i) * Random.Range(-RandomDist, RandomDist);
        }
        BezierPath bezierPath = new BezierPath(points, true, PathSpace.xz);
        CarSpline.bezierPath = bezierPath;
        Debug.Log("BezierPath created");
    }

    public void UpdateTransform(float t)
    {
        Vector3 v1 = CarSpline.path.GetPointAtDistance(t * speed - 0.3f);
        Vector3 v2 = CarSpline.path.GetPointAtDistance(t * speed);

        transform.position = (v1 + v2) / 2.0f;
        transform.rotation = Quaternion.LookRotation(v2 - v1);
    }

    void Update()
    {
        UpdateTransform(t);
        t += timesteps;
    }
}

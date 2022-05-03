using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CarPath : MonoBehaviour
{
    public PathCreator RoadSpline;
    public PathCreator CarSpline;

    public float RandomDist = 0.5f;
    float t = 0.0f;

    void Start()
    {
        CreateCarSpline();
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
        transform.position = CarSpline.path.GetPointAtTime(t);
        transform.localRotation = CarSpline.path.GetRotation(t) * Quaternion.Euler(0.0f, 90.0f, 0.0f);

        // transform.SetPositionAndRotation(pos, rot);
    }

    void Update()
    {
        UpdateTransform(t);
        t += 0.001f;
    }
}

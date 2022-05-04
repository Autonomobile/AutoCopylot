using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

public class CarData
{
    public float steering;
    public float speed;
    public float throttle;
    public float[] zone;
    public CarData(float steering, float speed, float throttle, float[] zone)
    {
        this.steering = steering;
        this.speed = speed;
        this.throttle = throttle;
        this.zone = zone;
    }
}


public class CarPath : MonoBehaviour
{

    public PathCreator RoadSpline;
    public PathCreator TrajectorySpline;
    public PathCreator CarSpline;

    [Header("Synthetic Labels Generation Parameters")]
    public float RandomDist = 0.5f;
    public float timeLookahead = 0.2f;
    public float maxAngle = 25.0f;
    public float roadWidth = 0.4f;
    public float speed = 1.0f;
    public float timesteps = 0.033f;

    float t = 0.0f;


    public void Start()
    {
        if (RoadSpline == null || TrajectorySpline == null || CarSpline == null)
            throw new ArgumentNullException("RoadSpline, TrajectorySpline or CarSpline is null");

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
            points[i] = RoadSpline.path.GetPoint(i * step) - RoadSpline.transform.position + RoadSpline.path.GetNormal(i) * UnityEngine.Random.Range(-RandomDist, RandomDist);
        }
        BezierPath bezierPath = new BezierPath(points, true, PathSpace.xz);
        CarSpline.bezierPath = bezierPath;
        Debug.Log("BezierPath created");
    }

    public void UpdateTransform(float t)
    {
        float dist = t * speed;
        Vector3 v1 = CarSpline.path.GetPointAtDistance(dist - 0.3f);
        Vector3 v2 = CarSpline.path.GetPointAtDistance(dist);

        transform.position = (v2 + v2) / 2.0f;
        transform.rotation = Quaternion.LookRotation(v2 - v1);
    }

    public float GetSteering()
    {
        float dist = t * speed;

        Vector3 pos = CarSpline.path.GetPointAtDistance(dist);
        Vector3 rot = CarSpline.path.GetDirectionAtDistance(dist);

        float aheadDist = TrajectorySpline.path.GetClosestDistanceAlongPath(pos) + timeLookahead * speed;
        Vector3 targetPos = TrajectorySpline.path.GetPointAtDistance(aheadDist);
        Vector3 targetRot = TrajectorySpline.path.GetDirectionAtDistance(aheadDist);

        Quaternion InverseRot = Quaternion.Inverse(Quaternion.LookRotation(rot));
        Vector3 relativeTargetPos = InverseRot * (pos - targetPos);

        float deltaAngle = Mathf.Clamp(Vector3.SignedAngle(rot, targetRot, Vector3.up) / maxAngle, -1.0f, 1.0f);
        float xDist = -Mathf.Clamp(relativeTargetPos.x / roadWidth, -1.0f, 1.0f);

        float steering = deltaAngle * 0.25f + xDist * 0.75f;
        return steering;
    }

    public float GetThrottle()
    {
        // TODO: Find a policy
        return 0.0f;
    }

    public float[] GetZone()
    {
        return new float[3];
    }

    public void Step()
    {
        t += timesteps;
        UpdateTransform(t);
    }

    public void SaveJson(string path)
    {
        float steering = GetSteering();
        float throttle = GetThrottle();
        float[] zone = GetZone();

        string json = JsonUtility.ToJson(new CarData(steering, speed, throttle, zone));
        System.IO.File.WriteAllText(path, json);
    }


    // draw gizmos for the steering
    public void OnDrawGizmos()
    {
        if (CarSpline == null)
            return;

        Vector3 steeringVect = new Vector3(GetSteering(), 0, 0);

        float dist = t * speed;
        Vector3 pos = CarSpline.path.GetPointAtDistance(dist);
        Vector3 rot = CarSpline.path.GetDirectionAtDistance(dist);
        Quaternion rotQ = Quaternion.LookRotation(rot);

        Gizmos.color = Color.red;
        Vector3 endPos = pos + rotQ * steeringVect * 0.2f;
        Handles.DrawBezier(pos, endPos, pos, endPos, Color.red, null, 5.0f);
    }
}

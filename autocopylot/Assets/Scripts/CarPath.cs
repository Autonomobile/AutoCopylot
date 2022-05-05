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
    public float minSpeed = 1.0f;
    public float maxSpeed = 2.0f;

    private float t = 0.0f;
    private float dt = 0.0f;
    private float prevDist = 0.0f;
    public float speed = 0.0f;

    private Vector3 posMask = new Vector3(1.0f, 0.0f, 1.0f);


    public void Start()
    {
        if (RoadSpline == null || TrajectorySpline == null || CarSpline == null)
            throw new ArgumentNullException("RoadSpline, TrajectorySpline or CarSpline is null");

        CreateCarSpline();
    }

    public void CreateCarSpline()
    {
        int step = 6;
        int numPoints = RoadSpline.path.NumPoints / step;
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = RoadSpline.path.GetPoint(i * step) - RoadSpline.transform.position + RoadSpline.path.GetNormal(i) * UnityEngine.Random.Range(-RandomDist, RandomDist);
            points[i] += Vector3.up * UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
        BezierPath bezierPath = new BezierPath(points, true, PathSpace.xyz);
        CarSpline.bezierPath = bezierPath;
    }

    public void UpdateTransform(float t)
    {   
        dt = t - this.t;
        float dist = prevDist + dt * speed;

        Vector3 v1 = CarSpline.path.GetPointAtDistance(dist - 0.3f);
        Vector3 v2 = CarSpline.path.GetPointAtDistance(dist);

        Vector3 pos = new Vector3(v1.x + v2.x, 0.0f, v1.z + v2.z) / 2.0f;
        Vector3 diff = new Vector3(v2.x - v1.x, 0.0f, v2.z - v1.z);
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(diff);

        speed = (v1.y + v2.y) / 2.0f;
        prevDist = dist;;
        this.t = t;
    }

    public float GetSteering()
    {
        float dist = t * speed;

        Vector3 pos = transform.position;
        Vector3 rot = transform.forward;
        // Vector3 splinePos = TrajectorySpline.path.GetPointAtDistance(dist);

        float aheadDist = TrajectorySpline.path.GetClosestDistanceAlongPath(pos) + timeLookahead * speed;
        Vector3 targetPos = TrajectorySpline.path.GetPointAtDistance(aheadDist);
        
        Quaternion InverseRot = Quaternion.Inverse(transform.rotation);
        Vector3 relativeTargetPos = InverseRot * (pos - targetPos);
        // Vector3 relativeCenterPos = InverseRot * (pos - targetPos);

        float targetOff = -Mathf.Clamp(relativeTargetPos.x / roadWidth, -1.0f, 1.0f);
        // float lateralOff = Mathf.Clamp(relativeCenterPos.x / roadWidth, -1.0f, 1.0f);

        float steering = Mathf.Clamp(targetOff, -1.0f, 1.0f);;
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

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        
        float aheadDist = TrajectorySpline.path.GetClosestDistanceAlongPath(pos) + timeLookahead * speed;
        Vector3 targetPos = TrajectorySpline.path.GetPointAtDistance(aheadDist);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPos, 0.1f);
        Vector3 endPos = pos + rot * steeringVect * 0.2f;
        Handles.DrawBezier(pos, endPos, pos, endPos, Color.red, null, 5.0f);
        // Vector3 target = pos + dt * speed * transform.forward;
        // Handles.DrawBezier(pos, endPos, pos, endPos, Color.red, null, 5.0f);
    }
}

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
    public float minSpeed = 1.0f;
    public float maxSpeed = 2.0f;

    private float t = 0.0f;
    private float dt = 0.0f;
    private float prevDist = 0.0f;
    public float speed = 0.0f;

    private Vector3[] zones;
    private float[] averagedAngles;


    public void Start()
    {
        if (RoadSpline == null || TrajectorySpline == null || CarSpline == null)
            throw new ArgumentNullException("RoadSpline, TrajectorySpline or CarSpline is null");

        CreateCarSpline();
        CreateZone();
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

    public void CreateZone()
    {
        // Settings
        float pointsEvery = 1.0f;
        int pointsLookahead = 5;

        int numPoints = (int)(RoadSpline.path.length / pointsEvery);
        float[] angles = new float[numPoints];

        Quaternion prevRot = RoadSpline.path.GetRotationAtDistance(0.0f);
        for (int i = 0; i < zones.Length; i++)
        {
            float dist = i * pointsEvery;
            Quaternion rot = RoadSpline.path.GetRotationAtDistance(dist);
            angles[i] = Quaternion.Angle(prevRot, rot);
            prevRot = rot;
        }

        averagedAngles = new float[numPoints];
        for (int i = 0; i < averagedAngles.Length; i++)
        {
            float av = 0.0f;
            for (int j = i; j < i + pointsLookahead; j++)
                av += angles[j % averagedAngles.Length];
            av /= pointsLookahead;
            averagedAngles[i] = av;
        }

        zones = new Vector3[numPoints];
        for (int i = 0; i < zones.Length; i++)
        {
            float av = averagedAngles[i];
            // TODO
            // if (av > maxAngle * 0.75f)
            //     zones[i] = new Vector3(averagedAngles[i], 0.0f, 0.0f);
        }

        // float zonesDistWindow = 1.0f;
        // // float zonesLookAhead = 2.0f;
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
        prevDist = dist; ;
        this.t = t;
    }

    public float GetSteering()
    {
        float dist = t * speed;

        Vector3 pos = transform.position;
        Vector3 rot = transform.forward;

        float aheadDist = TrajectorySpline.path.GetClosestDistanceAlongPath(pos) + timeLookahead * speed;
        Vector3 targetPos = TrajectorySpline.path.GetPointAtDistance(aheadDist);
        Vector3 targetRot = TrajectorySpline.path.GetDirectionAtDistance(aheadDist);

        Quaternion InverseRot = Quaternion.Inverse(transform.rotation);
        float angle = Vector3.SignedAngle(transform.forward * -1.0f, (pos - targetPos), Vector3.up);
        float directionAngle = Vector3.SignedAngle(transform.forward, targetRot, Vector3.up);

        float steering = Mathf.Clamp((angle + directionAngle) / maxAngle, -1.0f, 1.0f);
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

        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(pos, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPos, 0.1f);

        Vector3 endPos = pos + rot * steeringVect * 0.2f;
        Handles.DrawBezier(pos, endPos, pos, endPos, Color.red, null, 5.0f);
        Handles.DrawBezier(pos, targetPos, pos, targetPos, Color.green, null, 5.0f);


        Gizmos.color = Color.blue;
        for (int i = 0; i < averagedAngles.Length; i++)
        {
            Gizmos.DrawSphere(RoadSpline.path.GetPointAtDistance(i * 1.0f), averagedAngles[i] / 100.0f + 0.1f);
        }
    }
}

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
    public float speed = 0.0f;

    [Header("Zone Parameters")]
    public float pointsEvery = 0.25f;
    public float distAverage = 0.5f;
    public float distBrakeLookahead = 2.0f;
    public float turnTh = 20f;
    public bool doDrawTurns = true;

    private float t = 0.0f;
    private float dt = 0.0f;
    private float prevDist = 0.0f;

    private (float, Vector3)[] zones;
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
        // TODO: find a better way to approx angle
        int pointsAverage = Mathf.CeilToInt(distAverage / pointsEvery);
        int pointsLookahead = Mathf.CeilToInt(distBrakeLookahead / pointsEvery);

        int numPoints = (int)(RoadSpline.path.length / pointsEvery);
        float[] angles = new float[numPoints];

        Quaternion prevRot = RoadSpline.path.GetRotationAtDistance(0.0f);
        for (int i = 0; i < numPoints; i++)
        {
            float dist = i * pointsEvery;
            Quaternion rot = RoadSpline.path.GetRotationAtDistance(dist);
            angles[i] = Quaternion.Angle(prevRot, rot) / pointsEvery;
            prevRot = rot;
        }

        averagedAngles = new float[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float av = 0.0f;
            for (int j = -pointsAverage; j < pointsAverage; j++)
            {
                if (j < 0)
                    av += angles[(numPoints - j) % numPoints];
                else
                    av += angles[(i + j) % numPoints];
            }
            av /= (pointsAverage * 2 + 1);
            averagedAngles[i] = av;
        }

        zones = new (float, Vector3)[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float dist = i * pointsEvery;
            float time = dist / RoadSpline.path.length;

            float av = averagedAngles[i];

            // look ahead
            int j = 0;
            for (; j < pointsLookahead; j++)
            {
                float aheadAngle = averagedAngles[(i + j) % numPoints];
                if (aheadAngle > turnTh)
                    break;
            }

            Vector3 zone;
            if (j == pointsLookahead) // straight
                zone = Vector3.right;
            else if (j == 0) // turn
                zone = Vector3.up;
            else // brake 
                zone = Vector3.forward;

            zones[i] = (time, zone);
        }
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
        Vector3 pos = transform.position;
        Vector3 rot = transform.forward;

        float aheadDist = TrajectorySpline.path.GetClosestDistanceAlongPath(pos) + timeLookahead * speed;
        Vector3 targetPos = TrajectorySpline.path.GetPointAtDistance(aheadDist);
        Vector3 targetRot = TrajectorySpline.path.GetDirectionAtDistance(aheadDist);

        Quaternion InverseRot = Quaternion.Inverse(transform.rotation);
        float angle = Vector3.SignedAngle(transform.forward * -1.0f, (pos - targetPos), Vector3.up);
        float directionAngle = Vector3.SignedAngle(transform.forward, targetRot, Vector3.up);

        float steering = Mathf.Clamp((angle * 0.4f + directionAngle * 0.1f) / maxAngle, -1.0f, 1.0f);
        Debug.Log(steering);
        return steering;
    }

    public float GetThrottle()
    {
        // TODO: Find a policy
        return 0.0f;
    }

    public float[] GetZone()
    {
        if (zones == null)
            return new float[3];

        Vector3 pos = transform.position;
        float t = RoadSpline.path.GetClosestTimeOnPath(pos);

        int idx1 = zones.Length - 1;
        int idx2 = 0;
        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i].Item1 > t)
            {
                idx1 = i - 1;
                idx2 = i;
                break;
            }
        }

        float t1 = zones[idx1].Item1;
        float t2 = zones[idx2].Item1;

        float tt = (t - t1) / (t2 - t1);
        Vector3 zone = zones[idx1].Item2 * (1.0f - tt) + zones[idx2].Item2 * tt;
        return new float[3] { zone.x, zone.y, zone.z };
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

        if (doDrawTurns && averagedAngles != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < averagedAngles.Length; i++)
            {
                if (averagedAngles[i] > turnTh)
                    Gizmos.DrawSphere(RoadSpline.path.GetPointAtDistance(i * pointsEvery), averagedAngles[i] / 180.0f);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CarData
{
    public float steering;
    public float speed;
    public float throttle;
    public CarData(float steering, float speed, float throttle)
    {
        this.steering = steering;
        this.speed = speed;
        this.throttle = throttle;
    }
}


public class CarPath : MonoBehaviour
{
    static string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/collect/";
    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    public PathCreator RoadSpline;
    public PathCreator TrajectorySpline;
    public PathCreator CarSpline;

    public float RandomDist = 0.5f;
    public float speed = 1.0f;

    float t = 0.0f;
    float timesteps = 0.033f;


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
        Vector3 v1 = CarSpline.path.GetPointAtDistance(t * speed - 0.3f);
        Vector3 v2 = CarSpline.path.GetPointAtDistance(t * speed);

        transform.position = (v1 + v2) / 2.0f;
        transform.rotation = Quaternion.LookRotation(v2 - v1);
    }

    public float GetSteering()
    {
        // create a steering value to get to the optimal trajectorySpline point

        // Vector3 v1 = CarSpline.path.GetPointAtDistance(t * speed - 0.3f);
        // Vector3 v2 = CarSpline.path.GetPointAtDistance(t * speed);
        // Vector3 v3 = CarSpline.path.GetPointAtDistance(t * speed + 0.3f);

        // Vector3 v4 = TrajectorySpline.path.GetPointAtDistance(t * speed);

        // Vector3 v5 = v4 - v2;
        // Vector3 v6 = v3 - v2;

        // float angle = Vector3.Angle(v5, v6);
        // float steering = angle / 90.0f;
        // return steering;
        return 0.0f;
    }

    public float GetThrottle()
    {
        return 0.0f;
    }

    public void SaveJson()
    {
        float steering = GetSteering();
        float throttle = GetThrottle();


        string json = JsonUtility.ToJson(new CarData(steering, speed, throttle));
        System.IO.File.WriteAllText(saveFolder + GetCurrentTime() + ".json", json);
    }

    void Update()
    {
        UpdateTransform(t);
        SaveJson();

        t += timesteps;
    }
    
    public static string GetCurrentTime()
    {
        return (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString().Replace(",", ".");
    }
}

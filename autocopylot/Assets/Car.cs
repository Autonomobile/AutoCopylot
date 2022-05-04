using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarPath))]
[RequireComponent(typeof(CameraSensor))]
public class Car : MonoBehaviour
{

    static string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/collect/";
    static System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

    public bool doSave = true;

    CarPath carPath;
    CameraSensor cameraSensor;

    void Start()
    {
        carPath = GetComponent<CarPath>();
        cameraSensor = GetComponent<CameraSensor>();
    }

    void Update()
    {
        carPath.Step();

        if (doSave)
        {
            string curTime = GetCurrentTime();
            cameraSensor.SaveImage(saveFolder + curTime + ".png");
            carPath.SaveJson(saveFolder + curTime + ".json");
        }

    }

    public static string GetCurrentTime()
    {
        return (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString().Replace(",", ".");
    }
}

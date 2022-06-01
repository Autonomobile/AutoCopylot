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

    public GenerateEnv generateEnv;
    public GenerateRoad generateRoad;

    public bool DoSave = true;
    public float RandomizeEvery = 20.0f;
    public float timesteps = 0.033f;

    CarPath carPath;
    CameraSensor cameraSensor;
    float t = 0.0f;
    float counter = 0.0f;

    void Start()
    {
        carPath = GetComponent<CarPath>();
        cameraSensor = GetComponent<CameraSensor>();

        if (!System.IO.Directory.Exists(saveFolder))
            System.IO.Directory.CreateDirectory(saveFolder);

        Randomize();
    }

    public void Randomize()
    {
        if (generateRoad != null)
            generateRoad.Start();
        if (generateEnv != null)
            generateEnv.Start();

        carPath.Start();
    }

    void Update()
    {
        counter += timesteps;
        if (counter > RandomizeEvery)
        {
            Randomize();
            carPath.UpdateTransform(t);
            counter = 0.0f;
        }
        else
        {
            t += timesteps;
            carPath.UpdateTransform(t);

            if (DoSave)
            {
                string curTime = GetCurrentTime();
                cameraSensor.SaveImage(saveFolder + curTime + ".png");
                carPath.SaveJson(saveFolder + curTime + ".json");
            }
        }
    }

    public static string GetCurrentTime()
    {
        return (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString().Replace(",", ".");
    }
}

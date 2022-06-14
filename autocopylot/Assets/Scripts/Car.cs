using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarPath))]
[RequireComponent(typeof(CameraSensor))]
public class Car : MonoBehaviour
{

    public string saveName = "";
    private static DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    private static DateTime Now = System.DateTime.Now;
    private string saveFolder => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/collect/" +
                                 Now.ToString("yyyy-MM-dd_HH:mm:ss") + "_" + saveName + "/";

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

        if (DoSave)
        {
            if (!System.IO.Directory.Exists(saveFolder))
                System.IO.Directory.CreateDirectory(saveFolder);
        }

        carPath.Start();
        Randomize();
    }

    public void Randomize()
    {
        //TODO: make cleaner
        // if (generateRoad != null)
        //     generateRoad.Start();
        
        if (generateEnv != null)
            generateEnv.Start();

        // carPath.Start(); // do once
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

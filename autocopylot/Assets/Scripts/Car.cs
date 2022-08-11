using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarPath))]
[RequireComponent(typeof(CameraSensor))]
public class Car : MonoBehaviour
{

    private static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static DateTime Now = DateTime.Now;

    public string saveName = "";
    public string collectFolder = "collect";
    
    private string timeNow => Now.ToString("yyyy-MM-dd_HH-mm-ss");
    private string homeFolder => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    private string saveFolder => homeFolder + "/" + collectFolder + "/" + timeNow + "_" + saveName + "/";
    
    public GenerateEnv generateEnv;
    public GenerateRoad generateRoad;

    public int generation = 1;
    public bool save = false;
    public float RandomizationFrequency = 20.0f;
    public float timesteps = 0.033f;

    CarPath carPath;
    CameraSensor cameraSensor;
    float t = 0.0f;
    float counter = 0.0f;

    int step = 0;
    public int nbImages = 1000;
    private int skipUpdate = 5; // skip n first Unity update call

    void Start()
    {
        carPath = GetComponent<CarPath>();
        cameraSensor = GetComponent<CameraSensor>();

        Env.Instance.Init();

        if (save)
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
        // this is used to prevent the script to generate black images
        // for the first skipUpdate Unity update calls
        if (skipUpdate > 0)
        {
            skipUpdate--;
            return;
        }

        counter += timesteps;
        if (counter > RandomizationFrequency)
        {
            Randomize();
            carPath.UpdateTransform(t);
            counter = 0.0f;
        }
        else
        {
            t += timesteps;
            carPath.UpdateTransform(t);

            step++;
            if (step > nbImages)
            {
                Now = DateTime.Now;
                generation--;
                step = 0;
                if (generation < 1)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                    return;
                }
                if (!System.IO.Directory.Exists(saveFolder))
                    System.IO.Directory.CreateDirectory(saveFolder);
                carPath.Start();
                return;
            }

            if (save)
            {
                string curTime = GetCurrentTime();
                cameraSensor.SaveImage(saveFolder + curTime + ".png");
                carPath.SaveJson(saveFolder + curTime + ".json");
            }
        }

    }

    public static string GetCurrentTime()
    {
        return (DateTime.UtcNow - epochStart).TotalSeconds.ToString().Replace(",", ".");
    }
}

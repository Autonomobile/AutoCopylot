using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public sealed class ENV : MonoBehaviour {
    public static ENV Instance;

    [Header("PathCreators")]
    public PathCreator RoadSpline;
    public PathCreator TrajectorySpline;
    public RoadGenerator roadGenerator;
    public EnvGenerator envGenerator;

    public DomeController domeController;
    public DomeProjection domeProjection;

    [Header("Virtual Env Settings")]
    public bool generateWalls = true;
    public bool randomizeLights = true;
    public bool generateChairs = false;

    [Header("Light Settings")]
    public Color lowLerpColor = Color.yellow;
    public Color highLerpColor = Color.white;
    public float maxLightAngle = 30.0f;
    public float lowLerpIntensity = 0.5f;
    public float highLerpIntensity = 1.5f;

    public float materialColorProbability = 0.5f;
    public float laneAppearProbability = 0.7f;

    [Header("Synthetic Labels Generation Parameters")]
    public float RandomDist = 0.2f;
    public float timeLookahead = 0.5f;
    public float maxAngle = 25.0f;
    public float minSpeed = 1.0f;
    public float maxSpeed = 2.0f;
    public float speed = 1.0f;

    [Header("Zone Parameters")]
    public float pointsEvery = 0.25f;
    public float distAverage = 1f;
    public float distBrakeLookahead = 1.0f;
    public float turnTh = 15f;
    public bool doDrawTurns = true;
    public float[] lookupZone = { 0.4f, 0.3f, 0.2f };

    [Header("Spline Loader")]
    public bool RandomTrack = false;
    public bool LoadTrack = false;
    public bool RandomMaterial = false;
    public int NumPoints = 10;
    public float MinDist = 3.0f;
    public string Name = "spline.txt";

    [Header("Road Builder")]
    public bool DoBuildRoad = true;
    public Material undersideMaterial;
    public float roadWidth = 0.4f;
    public float thickness = 0.01f;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void Init() {
        Debug.Log("Env Init");
    }

    public void SliderCameraFOV(float value) {
        int fov = (int) value;
        GameObject.Find("FOV Text Value").GetComponent<UnityEngine.UI.Text>().text = fov.ToString();
        domeController.FOV = fov;
    }

    public void SliderCameraXRotation(float value) {
        int val = (int) value;
        GameObject.Find("X Text Value").GetComponent<UnityEngine.UI.Text>().text = val.ToString();
        domeController.GetComponent<Transform>().rotation = Quaternion.Euler(val - 33, 0, 0);
    }

    public void SliderCameraYTranslation(float value) {
        int val = (int) value;
        GameObject.Find("Y Text Value").GetComponent<UnityEngine.UI.Text>().text = val.ToString();
        Vector3 pos = domeController.GetComponent<Transform>().position;
        domeController.GetComponent<Transform>().position = new Vector3(pos.x, val / 100.0f, pos.z);
    }


    public void SliderCameraZTranslation(float value) {
        int val = (int) value;
        GameObject.Find("Z Text Value").GetComponent<UnityEngine.UI.Text>().text = val.ToString();
        Vector3 pos = domeController.GetComponent<Transform>().position;
        domeController.GetComponent<Transform>().position = new Vector3(pos.x, pos.y, val / 100.0f);
    }
}

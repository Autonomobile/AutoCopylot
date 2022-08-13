using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

public class EnvGenerator : MonoBehaviour
{
    public string TextureFolder => "Materials/Materials/";
    public string RoadMatFolder => "Roads/Materials/";

    public PathCreator RoadSpline;

    [Header("Virtual Env Settings")]
    public bool generateWalls = true;
    public bool randomizeLights = true;
    public bool generateChairs = false;

    [Header("Light Settings")]
    public Light floorLightObjet;
    public Light ceilLightObject;
    public Color lowLerpColor = Color.yellow;
    public Color highLerpColor = Color.white;
    public float maxLightAngle = 30.0f;
    public float lowLerpIntensity = 0.8f;
    public float highLerpIntensity = 1.1f;

    public float materialColorProbability = 0.5f;
    public float laneAppearProbability = 0.7f;

    public GameObject ChairObject;
    public int numChairs = 20;

    float margin = 2.0f;
    float wallHeight = 2.5f;

    string floorObjectName = "Floor";
    string ceilObjectName = "Ceiling";
    string wallObjectName1 = "Wall 1";
    string wallObjectName2 = "Wall 2";
    string wallObjectName3 = "Wall 3";
    string wallObjectName4 = "Wall 4";


    void InitEnvVariables()
    {
        generateWalls = ENV.Instance.generateWalls;
        randomizeLights = ENV.Instance.randomizeLights;
        generateChairs = ENV.Instance.generateChairs;
        lowLerpColor = ENV.Instance.lowLerpColor;
        highLerpColor = ENV.Instance.highLerpColor;
        maxLightAngle = ENV.Instance.maxLightAngle;
        lowLerpIntensity = ENV.Instance.lowLerpIntensity;
        highLerpIntensity = ENV.Instance.highLerpIntensity;
        materialColorProbability = ENV.Instance.materialColorProbability;
        laneAppearProbability = ENV.Instance.laneAppearProbability;
    }

    public void Start()
    {
        InitEnvVariables();
        GenerateVirtualEnv();
    }

    public void GenerateVirtualEnv()
    {
        if (RoadSpline is null)
            throw new ArgumentNullException("RoadSpline is null.");

        if (generateWalls)
        {
            Bounds bounds = RoadSpline.path.bounds;
            bounds.Expand(margin);
            GenerateBox(bounds);
        }

        if (randomizeLights)
            RandomizeLights();

        if (generateChairs)
            GenerateChairs(numChairs);
    }

    public void DeleteObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
            DestroyImmediate(obj);
    }

    public void DeleteVirtualEnv()
    {
        DeleteObject(floorObjectName);
        DeleteObject(ceilObjectName);
        DeleteObject(wallObjectName1);
        DeleteObject(wallObjectName2);
        DeleteObject(wallObjectName3);
        DeleteObject(wallObjectName4);
    }

    public void ResetEnv()
    {
        DeleteVirtualEnv();
    }

    /// <summary>
    /// Modify rotation of lights
    /// </summary>
    public void RandomizeLights()
    {
        floorLightObjet.transform.rotation = Quaternion.Euler(90 + UnityEngine.Random.Range(-maxLightAngle, maxLightAngle), UnityEngine.Random.Range(-maxLightAngle, maxLightAngle), 0);
        floorLightObjet.color = Color.Lerp(highLerpColor, lowLerpColor, UnityEngine.Random.value);
        floorLightObjet.intensity = Mathf.Lerp(lowLerpIntensity, highLerpIntensity, UnityEngine.Random.value);

        ceilLightObject.transform.rotation = Quaternion.Euler(-90 + UnityEngine.Random.Range(-maxLightAngle, maxLightAngle), UnityEngine.Random.Range(-maxLightAngle, maxLightAngle), 0);
        ceilLightObject.color = Color.Lerp(highLerpColor, lowLerpColor, UnityEngine.Random.value);
        ceilLightObject.intensity = Mathf.Lerp(lowLerpIntensity, highLerpIntensity, UnityEngine.Random.value);
    }

    #region virtual env generation

    /// <summary>
    /// Generate chairs
    /// </summary>
    /// <param name="num"></param>
    public void GenerateChairs(int num) {
        for (int i = 0; i < num; i++) {
            Vector3 pos = RoadSpline.path.GetPointAtDistance(UnityEngine.Random.Range(0, RoadSpline.path.length));
            Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            GameObject go = Instantiate(ChairObject, pos, rot);
            go.transform.parent = transform;
        }
    }

    /// <summary>
    /// Generate walls, floor and ceiling
    /// </summary>
    /// <param name="bounds"></param>
    public void GenerateBox(Bounds bounds)
    {
        float centerx = (bounds.min.x + bounds.max.x) / 2.0f;
        float centerz = (bounds.min.z + bounds.max.z) / 2.0f;
        
        // floor
        GenerateFloor(centerx, centerz, bounds.size.z, bounds.size.x, floorObjectName);
        
        // ceiling
        GenerateCeiling(centerx, wallHeight, bounds.size.z, bounds.size.x, ceilObjectName);

        // walls
        GenerateWall(bounds.min.x, centerz, bounds.size.z, wallHeight, Vector3.right, wallObjectName1);
        GenerateWall(bounds.max.x, centerz, bounds.size.z, wallHeight, Vector3.left, wallObjectName2);
        GenerateWall(centerx, bounds.min.z, bounds.size.x, wallHeight, Vector3.forward, wallObjectName3);
        GenerateWall(centerx, bounds.max.z, bounds.size.x, wallHeight, Vector3.back, wallObjectName4);
    }

    /// <summary>
    /// Generate floor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="xsize"></param>
    /// <param name="ysize"></param>
    /// <param name="name"></param>
    public void GenerateFloor(float x, float y, float xsize, float ysize, string name)
    {
        GameObject plane = GameObject.Find(name);
        if (plane is null)
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = name;
            plane.AddComponent<BoxCollider>();
        }

        plane.transform.parent = transform;
        plane.transform.localScale = new Vector3(xsize / 10.0f, 1, ysize / 10.0f);
        plane.transform.position = new Vector3(x, 0, y);
        plane.transform.rotation = Quaternion.LookRotation(Vector3.right);
        plane.GetComponent<Renderer>().material = RandomTexture();
    }

    /// <summary>
    /// Generate Ceilling
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="xsize"></param>
    /// <param name="ysize"></param>
    /// <param name="name"></param>
    public void GenerateCeiling(float x, float y, float xsize, float ysize, string name)
    {
        GameObject plane = GameObject.Find(name);
        if (plane is null)
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = name;
            plane.AddComponent<BoxCollider>();
        }

        plane.transform.parent = transform;
        plane.transform.localScale = new Vector3(xsize / 10.0f, 1, ysize / 10.0f);
        plane.transform.position = new Vector3(x, y, 0);
        plane.transform.rotation = Quaternion.LookRotation(Vector3.right);
        plane.transform.Rotate(0, 0, 180);
        plane.GetComponent<Renderer>().material = RandomTexture();
    }

    /// <summary>
    /// Generate Walls
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="xsize"></param>
    /// <param name="ysize"></param>
    /// <param name="upwards"></param>
    /// <param name="name"></param>
    public void GenerateWall(float x, float y, float xsize, float ysize, Vector3 upwards, string name)
    {
        GameObject wall = GameObject.Find(name);
        if (wall is null)
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Plane);
            wall.name = name;
            wall.AddComponent<BoxCollider>();
        }

        wall.transform.parent = transform;
        wall.transform.localScale = new Vector3(xsize / 10.0f, 1, ysize / 10.0f);
        wall.transform.position = new Vector3(x, ysize / 2.0f, y);
        wall.transform.rotation = Quaternion.LookRotation(Vector3.down, upwards);
        wall.GetComponent<MeshRenderer>().material = RandomTexture();
    }

    #endregion

    /// <summary>
    /// Set new material for the road
    /// </summary>
    public void UpdateRoadMaterial()
    {
        GameObject road = GameObject.Find("Road Mesh Holder");        
        if (road is null) return;
        
        road.GetComponent<MeshRenderer>().material = GetRandomRoadMaterial();
    }

    /// <summary>
    /// Returns a random Color
    /// </summary>
    /// <returns></returns>
    public Material RandomColor() {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = UnityEngine.Random.ColorHSV();
        return mat;
    }

    /// <summary>
    /// Get random material from list of materials
    /// </summary>
    /// <returns></returns>
    public Material RandomTexture()
    {
        if (UnityEngine.Random.value < materialColorProbability) {
            return RandomColor();
        }

        Material[] materials = Resources.LoadAll<Material>(TextureFolder);
        return materials[UnityEngine.Random.Range(0, materials.Length)];
    }

    /// <summary>
    /// Get Random Road Material
    /// </summary>
    /// <returns></returns>
    public Material GetRandomRoadMaterial()
    {
        Material[] materials = Resources.LoadAll<Material>(RoadMatFolder);
        float random = UnityEngine.Random.value;
        
        if (random < laneAppearProbability)
        {
            Material lane;
        
            if (UnityEngine.Random.value < .5f)
                lane =  materials[0];
            else 
                lane = materials[1];

            Material copy = new Material(lane);
            copy.color = UnityEngine.Random.ColorHSV();
            return copy;
        }
        
        return materials[UnityEngine.Random.Range(0, materials.Length)];
    }

    /// <summary>
    /// Set white lane as default material
    /// </summary>
    public void SetDefaultMaterial() {
        GameObject road = GameObject.Find("Road Mesh Holder");
        if (road is null) return;

        Material[] materials = Resources.LoadAll<Material>(TextureFolder);
        road.GetComponent<MeshRenderer>().material = materials[1];
    }

    
}

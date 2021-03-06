using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GenerateEnv : MonoBehaviour
{
    public string FloorMatFolder => "Floors/Materials/";
    public string WallMatFolder => "Walls/Materials/";

    public PathCreator RoadSpline;

    public bool doGenerateWalls = true;
    public bool doRandomizeSun = true;
    public bool doGenerateChairs = false;

    public Light Sun;
    public float maxAngleSun = 30.0f;
    public Color lowLerpColor = Color.yellow;
    public Color highLerpColor = Color.white;
    public float lowLerpIntensity = 0.8f;
    public float highLerpIntensity = 1.1f;

    public GameObject Chair;
    public int numChairs = 20;

    float margin = 2.0f;
    float wallHeight = 2.5f;

    public void Start()
    {
        if (RoadSpline is null)
            throw new ArgumentNullException("RoadSpline is null.");

        if (doGenerateWalls)
        {
            Bounds bounds = RoadSpline.path.bounds;
            bounds.Expand(margin);
            GenerateBox(bounds);
        }
        
        if (doRandomizeSun)
        {
            SunRandomization();
        }

        if (doGenerateChairs)
        {
            GenerateChairs(numChairs);
        }

    }

    public void SunRandomization()
    {
        Sun.transform.rotation = Quaternion.Euler(90 + UnityEngine.Random.Range(-maxAngleSun, maxAngleSun), UnityEngine.Random.Range(-maxAngleSun, maxAngleSun), 0);
        Sun.color = Color.Lerp(highLerpColor, lowLerpColor, UnityEngine.Random.value);
        Sun.intensity = Mathf.Lerp(lowLerpIntensity, highLerpIntensity, UnityEngine.Random.value);

    }

    public void GenerateChairs(int num)
    {
        // TODO
        for (int i = 0; i < num; i++)
        {
            Vector3 pos = RoadSpline.path.GetPointAtDistance(UnityEngine.Random.Range(0, RoadSpline.path.length));
            Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            GameObject go = Instantiate(Chair, pos, rot);
            go.transform.parent = transform;
        }
    }


    public void GenerateBox(Bounds bounds)
    {
        float centerx = (bounds.min.x + bounds.max.x) / 2.0f;
        float centerz = (bounds.min.z + bounds.max.z) / 2.0f;
        CreateFloor(centerx, centerz, bounds.size.z, bounds.size.x, Vector3.right, "Floor");

        CreateWall(bounds.min.x, centerz, bounds.size.z, wallHeight, Vector3.down, Vector3.right, "Wall 1");
        CreateWall(bounds.max.x, centerz, bounds.size.z, wallHeight, Vector3.down, Vector3.left, "Wall 2");
        CreateWall(centerx, bounds.min.z, bounds.size.x, wallHeight, Vector3.down, Vector3.forward, "Wall 3");
        CreateWall(centerx, bounds.max.z, bounds.size.x, wallHeight, Vector3.down, Vector3.back, "Wall 4");
    }

    public void CreateFloor(float x, float y, float xsize, float ysize, Vector3 Orientation, string name)
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
        plane.transform.rotation = Quaternion.LookRotation(Orientation);
        plane.GetComponent<Renderer>().material = GetRandomFloorMaterial();
    }

    public void CreateWall(float x, float y, float xsize, float ysize, Vector3 forward, Vector3 upwards, string name)
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
        wall.transform.rotation = Quaternion.LookRotation(forward, upwards);
        wall.GetComponent<MeshRenderer>().material = GetRandomWallMaterial();
    }


    Material GetRandomFloorMaterial()
    {
        Material[] materials = Resources.LoadAll<Material>(FloorMatFolder);
        return materials[UnityEngine.Random.Range(0, materials.Length)];
    }
    Material GetRandomWallMaterial()
    {
        Material[] materials = Resources.LoadAll<Material>(WallMatFolder);
        return materials[UnityEngine.Random.Range(0, materials.Length)];
    }

}

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using PathCreation;

public class GenerateRoad : MonoBehaviour
{
    public string RoadSplineFolder => "Assets/Resources/RoadSplines/";
    public string TrajectorySplineFolder => "Assets/Resources/TrajectorySplines/";
    public string RoadMatFolder => "Roads/Materials/";


    [Header("PathCreators")]
    public PathCreator RoadSpline;
    public PathCreator TrajectorySpline;


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

    Mesh RoadMesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    [SerializeField, HideInInspector]
    GameObject meshHolder;


    public void Start()
    {
        if (RoadSpline is null || TrajectorySpline is null)
            throw new ArgumentNullException("One of the splines is null.");

        if (RandomTrack)
        {
            if (LoadTrack)
                LoadRandomSplines();
            else
                GenerateSplines();
        }
        else if (LoadTrack)
            LoadSplines(Name);
    }

    public static void ApplyPath(PathCreator spline, Vector3[] waypoints)
    {
        BezierPath bezierPath = new BezierPath(waypoints, true, PathSpace.xz);
        spline.bezierPath = bezierPath;
    }

    public Vector3[] RandomPoints()
    {
        Vector3[] waypoints = new Vector3[NumPoints];
        for (int i = 0; i < NumPoints; i++)
        {
            waypoints[i] = Vector3.zero;
            if (i > 0)
            {
                waypoints[i] = waypoints[i - 1];
                while (Vector3.Distance(waypoints[i - 1], waypoints[i]) < MinDist)
                    waypoints[i] += new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0, UnityEngine.Random.Range(-5.0f, 5.0f));
            }
        }
        return waypoints;
    }


    public static void SavePath(PathCreator spline, string path)
    {
        int numPoints = spline.bezierPath.NumPoints;
        string[] lines = new string[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 waypoint = spline.bezierPath.GetPoint(i);
            string line = $"{waypoint.x} {waypoint.y} {waypoint.z}";
            lines[i] = line;
        }
        System.IO.File.WriteAllLines(path, lines);
    }

    public static void LoadPath(PathCreator spline, string path)
    {
        string[] lines = System.IO.File.ReadAllLines(path);
        int length = lines.Length / 3;
        Vector3[] waypoints = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            string[] line = lines[i * 3].Split(' ');
            waypoints[i] = new Vector3(float.Parse(line[0]), float.Parse(line[1]), float.Parse(line[2]));
        }
        ApplyPath(spline, waypoints);
    }

    public void LoadSplines(string name)
    {
        LoadPath(RoadSpline, RoadSplineFolder + name);
        LoadPath(TrajectorySpline, TrajectorySplineFolder + name);
        PathUpdated();
    }

    public void LoadRandomSplines()
    {
        string[] roadPaths = Directory.GetFiles(RoadSplineFolder, "*.txt");
        string[] trajPaths = Directory.GetFiles(RoadSplineFolder, "*.txt");

        int roadIndex = UnityEngine.Random.Range(0, roadPaths.Length);
        LoadPath(RoadSpline, roadPaths[roadIndex]);
        LoadPath(TrajectorySpline, trajPaths[roadIndex]);
        PathUpdated();
    }

    public void GenerateSplines()
    {
        Vector3[] points = RandomPoints();
        GenerateRoad.ApplyPath(RoadSpline, points);
        GenerateRoad.ApplyPath(TrajectorySpline, points);
        PathUpdated();
    }


    private void PathUpdated()
    {
        if (DoBuildRoad && RoadSpline != null)
        {
            AssignMeshComponents();
            AssignMaterials();
            BuildRoad();
        }
    }

    private void BuildRoad()
    {
        VertexPath path = RoadSpline.path;
        Vector3[] verts = new Vector3[path.NumPoints * 8];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        // Vertices for the top of the road are layed out:
        // 0  1
        // 8  9
        // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        bool usePathNormals = !(path.space == PathSpace.xyz);

        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
            Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;
            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - localUp * thickness;
            verts[vertIndex + 3] = vertSideB - localUp * thickness;

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

            // Top of road normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of road normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;
            // Sides of road normals
            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;

            // Set triangle indices
            if (i < path.NumPoints - 1 || path.isClosedLoop)
            {
                for (int j = 0; j < triangleMap.Length; j++)
                {
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++)
                {
                    sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                }

            }

            vertIndex += 8;
            triIndex += 6;
        }

        RoadMesh.Clear();
        RoadMesh.vertices = verts;
        RoadMesh.uv = uvs;
        RoadMesh.normals = normals;
        RoadMesh.subMeshCount = 3;
        RoadMesh.SetTriangles(roadTriangles, 0);
        RoadMesh.SetTriangles(underRoadTriangles, 1);
        RoadMesh.SetTriangles(sideOfRoadTriangles, 2);
        RoadMesh.RecalculateBounds();
    }
    private void AssignMeshComponents()
    {

        if (meshHolder == null)
        {
            meshHolder = new GameObject("Road Mesh Holder");
        }

        meshHolder.transform.rotation = Quaternion.identity;
        meshHolder.transform.position = Vector3.zero + Vector3.up * 0.001f;
        meshHolder.transform.localScale = new Vector3(1, 0.001f, 1);


        // Ensure mesh renderer and filter components are assigned
        if (!meshHolder.gameObject.GetComponent<MeshFilter>())
        {
            meshHolder.gameObject.AddComponent<MeshFilter>();
        }
        if (!meshHolder.GetComponent<MeshRenderer>())
        {
            meshHolder.gameObject.AddComponent<MeshRenderer>();
        }

        meshRenderer = meshHolder.GetComponent<MeshRenderer>();
        meshFilter = meshHolder.GetComponent<MeshFilter>();
        if (RoadMesh == null)
        {
            RoadMesh = new Mesh();
        }
        meshFilter.sharedMesh = RoadMesh;
    }

    private void AssignMaterials()
    {
        if (RandomMaterial && undersideMaterial != null)
        {
            meshRenderer.sharedMaterials = new Material[] { GetRandomMaterial(), undersideMaterial, undersideMaterial };
            meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, RoadSpline.path.length);
        }
    }

    Material GetRandomMaterial()
    {
        Material[] materials = Resources.LoadAll<Material>(RoadMatFolder);
        return materials[UnityEngine.Random.Range(0, materials.Length)];
    }


}
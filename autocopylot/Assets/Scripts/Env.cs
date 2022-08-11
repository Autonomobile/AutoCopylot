using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Env : MonoBehaviour
{
    public static Env Instance;

    [Header("Virtual Env Settings")]
    public bool generateWalls = true;
    public bool randomizeLights = true;
    public bool generateChairs = false;

    [Header("Light Settings")]
    public Color lowLerpColor = Color.yellow;
    public Color highLerpColor = Color.white;
    public float maxLightAngle = 30.0f;
    public float lowLerpIntensity = 0.8f;
    public float highLerpIntensity = 1.1f;

    public float materialColorProbability = 0.5f;
    public float laneAppearProbability = 0.7f;

    private void Awake()
    {
        Debug.Log("Init ur mom");

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        Debug.Log("Env Init");
    }
}

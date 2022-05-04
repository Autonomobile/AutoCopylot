using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarPath))]
[RequireComponent(typeof(CameraSensor))]
public class Car : MonoBehaviour
{
    public bool doSave = true;

    public CarPath carPath;


    void Update()
    { 
        SaveImage(saveFolder + GetCurrentTime() + ".png");

    }
}

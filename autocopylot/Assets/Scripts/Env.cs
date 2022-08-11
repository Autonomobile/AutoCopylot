using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Env : MonoBehaviour
{

    public static Env Instance;

    private void Awake()
    {
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

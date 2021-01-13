using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateSingleton
{
    [MenuItem("JTRP/Create Trail Singleton")]
    static void CreateTrail()
    {
        // var go = GameObject.Find("__Singleton");
        // if (!go)
        //     go = new GameObject("__Singleton");
        // if (go.GetComponent<TrailManager>() == null)
        // {
        //     go.AddComponent<TrailManager>();
        // }
        // var sig = go.GetComponent<TrailManager>();
        // var builders = GameObject.FindObjectsOfType<TrailBuilder>();
        // foreach (var item in builders)
        // {
        //     item.trailManager = sig;
        // }
    }
}

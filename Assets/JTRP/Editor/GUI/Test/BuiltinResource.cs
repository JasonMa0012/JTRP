using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public class BuiltInResourceWindow : EditorWindow
{
    [MenuItem("Tools/BuiltInResourceWindow")]
    static void Init()
    {
        EditorWindow window = EditorWindow.CreateInstance<BuiltInResourceWindow>();
        window.Show();
    }

    List<Texture2D> builtInTexs = new List<Texture2D>();
    void GetBultinAsset()
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
        var bundle = info.Invoke(null, new object[0]) as AssetBundle;
        UnityEngine.Object[] objs = bundle.LoadAllAssets();
        if (null != objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is Texture2D)
                {
                    builtInTexs.Add(objs[i] as Texture2D);
                }
            }
        }
    }

    void OnEnable()
    {
        GetBultinAsset();
    }

    Vector2 scrollPos = Vector2.zero;
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < builtInTexs.Count; i++)
        {
            EditorGUILayout.ObjectField(builtInTexs[i], typeof(Texture2D), false, GUILayout.Width(100),GUILayout.Height(100));
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}
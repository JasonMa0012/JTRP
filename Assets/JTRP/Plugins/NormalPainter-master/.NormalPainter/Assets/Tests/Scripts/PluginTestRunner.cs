#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public static class PluginTestRunner
{
    [MenuItem("Test/GenerateTangents")]
    public static void RunTestGenerateTangents()
    {
        Run("TestNormalsAndTangents");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Run(string name)
    {
        SetLibraryPath();
        RunImpl(name);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void RunImpl(string name)
    {
        RunTest(name);
        Debug.Log(Marshal.PtrToStringAnsi(GetLogMessage()));
    }

    static void SetLibraryPath()
    {
        var libraryPath = Application.dataPath + "Tests/Plugins/x86_64";
        var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
        if (!path.Contains(libraryPath))
        {
            path = libraryPath + ";" + path;
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        }
    }

    [DllImport("TestDLL")] static extern IntPtr GetLogMessage();
    [DllImport("TestDLL")] static extern void RunTest(string name);
    [DllImport("TestDLL")] static extern void RunAllTests();
}
#endif

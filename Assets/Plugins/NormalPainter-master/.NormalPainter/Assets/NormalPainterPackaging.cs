#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


public class NormalPainterPackaging
{
    [MenuItem("Assets/Make NormalPainter.unitypackage")]
    public static void MakePackage()
    {
        string[] files = new string[]
        {
            "Assets/UTJ/NormalPainter",
        };
        AssetDatabase.ExportPackage(files, "NormalPainter.unitypackage", ExportPackageOptions.Recurse);
    }

}
#endif // UNITY_EDITOR

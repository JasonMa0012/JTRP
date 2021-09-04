using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Collections.Generic;

namespace Pcl4Editor
{
    public class VersionWindow : EditorWindow
    {
        static Dictionary<string, VersionWindow> versionWindowDict = new Dictionary<string, VersionWindow> ();
        static Texture pencilLogoTexture;

        int windowWidth { get { return 400; } }
        int windowHeight { get { return 180 + (isMainPackage ? 40 : 0); } }
        bool isMainPackage { get { return packageInfo != null && packageInfo.name == "jp.co.psoft.pencil_4.line.unity"; } }

        Pcl4PackageInfo packageInfo;

        [MenuItem("Pencil+ 4/About", false, 2)]
        private static void Open()
        {
            OpenWithPackageManifestGUID("20df8acb73a53e64f8cf286ec34ff0c9");
        }

        protected static void OpenWithPackageManifestGUID(string guid)
        {
            if (!pencilLogoTexture)
            {
                pencilLogoTexture = EditorCommons.FindPencilLogoTexture();
            }

            // マニフェストGUIDに紐づいたバージョンウィンドウが存在するかを調べる
            VersionWindow versionWindow;
            if (versionWindowDict.TryGetValue(guid, out versionWindow))
            {
                // 既存の開いているウィンドウがあればフォーカスするだけにする
                if (versionWindow)
                {
                    versionWindow.Focus();
                    return;
                }

                // ウィンドウを閉じた場合、インスタンスはnullを返すので辞書から外す
                versionWindowDict.Remove(guid);
            }

            // 新規ウィンドウ生成
            versionWindow = CreateInstance<VersionWindow>();
            versionWindowDict.Add(guid, versionWindow);
            versionWindow.Initialize(guid);
        }

        void Initialize(string guid)
        {
            packageInfo = Pcl4PackageInfo.LoadFromGUID(guid);

            titleContent = new GUIContent("About " 
                + (packageInfo != null && packageInfo.displayName != null ? packageInfo.displayName : ""));
            maxSize = minSize = new Vector2(windowWidth, windowHeight);

            ShowUtility();

            position = new Rect(
                (Screen.currentResolution.width / 2) - (windowWidth / 2),
                (Screen.currentResolution.height / 2) - (windowHeight / 2),
                windowWidth,
                windowHeight);
        }

        void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Box(pencilLogoTexture, GUIStyle.none);
            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Space(10);
            if (packageInfo != null && packageInfo.version != null)
            {
                EditorGUILayout.LabelField("Version " + packageInfo.version, labelStyle);
            }

            if (isMainPackage)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(Pencil_4.PencilLineLicense.GetLicenseTypeString(), labelStyle);
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Copyright (c) P SOFTHOUSE. All rights reserved", labelStyle);
        }
    }

    [Serializable]
    public class Pcl4PackageInfo
    {
        public static Pcl4PackageInfo LoadFromGUID(string guid)
        {
            var filePath = AssetDatabase.GUIDToAssetPath(guid);
            if (filePath != null && filePath.Length > 0)
            {
                using (var reader = new StreamReader(filePath))
                {
                    return JsonUtility.FromJson<Pcl4PackageInfo>(reader.ReadToEnd());
                }
            }

            return null;
        }
        public string name;
        public string displayName;
        public string version;
    }
}

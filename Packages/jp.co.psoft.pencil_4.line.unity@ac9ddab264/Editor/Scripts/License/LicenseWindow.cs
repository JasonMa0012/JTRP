using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{

    public class LicenseWindow : EditorWindow
    {
        static bool isApplicationActive = false;
        static bool isFirstUpdate = true;

        [InitializeOnLoadMethod]
        static void LicenseInitialize()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += (arg) =>
#else
            EditorApplication.playmodeStateChanged += () =>
#endif
            {
                if (EditorApplication.isPlaying)
                {
                    PencilLineLicense.RefreshLicense();
                }
            };

            EditorApplication.update += () =>
            {
                if(isFirstUpdate)
                {
                    if (!NativeFunctions.CheckDllCommitHashValid())
                    {
                        return;
                    }
                    Pencil_4.PencilLineLicense.Initialize();
                    isFirstUpdate = false;
                }

                //Unity Editorがアクティブになったらライセンスの状態を更新する
                if (!isApplicationActive
                    &&
                UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    PencilLineLicense.RefreshLicense();
                }

                isApplicationActive = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
            };
        }


        [MenuItem("Pencil+ 4/License", false, 1)]
        private static void Open()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo =
                        {
                            FileName = System.IO.Path.GetFullPath(EditorCommons.FindSLSettingsPath())
                        }
                    };
                    process.Start();
                    break;
                }
                case RuntimePlatform.OSXEditor:
                    // TODO: Implement
                    break;
                case RuntimePlatform.LinuxEditor:
                    break;
                default:
                    break;
            }

        }

    }
}
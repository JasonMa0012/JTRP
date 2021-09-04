using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pcl4Editor
{
    public class HDRPVersionWindow : VersionWindow
    {
        [MenuItem("Pencil+ 4/About HDRP", false, 3)]
        private static void Open()
        {
            OpenWithPackageManifestGUID("9cdf6f3db772ccc44960100e7dd2d2af");
        }
    }
}
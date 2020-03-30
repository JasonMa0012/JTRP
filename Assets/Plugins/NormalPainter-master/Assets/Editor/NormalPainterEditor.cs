using UnityEngine;
using UnityEditor;
using System.Collections;
using UTJ.NormalPainter;

namespace UTJ.NormalPainterEditor
{
    [CustomEditor(typeof(UTJ.NormalPainter.NormalPainter))]
    public class NormalPainterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Window"))
                NormalPainterWindow.Open();

            EditorGUILayout.Space();

            //// debug stuff here
            //var t = target as NormalPainter;
        }
    }
}

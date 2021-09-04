using System.Collections;
using System.Collections.Generic;
using Pencil_4;
using UnityEditor;
using UnityEngine;

namespace Pcl4Editor
{
    [CustomEditor(typeof(RenderElementsLineNode))]
    public class RenderElementsLineNodeEditor : Editor
    {
        private SerializedProperty _propTargetTexture;
        private SerializedProperty _propIsDrawVisibleLines;
        private SerializedProperty _propIsDrawHiddenLines;
        private SerializedProperty _propIsBackgroundColorEnabled;
        private SerializedProperty _propBackgroundColor;
        private SerializedProperty _propIsDrawEdgeOutline;
        private SerializedProperty _propIsDrawEdgeObject;
        private SerializedProperty _propIsDrawEdgeIntersection;
        private SerializedProperty _propIsDrawEdgeSmoothingBoundary;
        private SerializedProperty _propIsDrawEdgeMaterialIdBoundary;
        //private SerializedProperty _propIsDrawEdgeSelectedEdges;
        private SerializedProperty _propIsDrawEdgeNormalAngle;
        private SerializedProperty _propIsDrawEdgeWireframe;
        private SerializedProperty _propIsDrawLineSetId1;
        private SerializedProperty _propIsDrawLineSetId2;
        private SerializedProperty _propIsDrawLineSetId3;
        private SerializedProperty _propIsDrawLineSetId4;
        private SerializedProperty _propIsDrawLineSetId5;
        private SerializedProperty _propIsDrawLineSetId6;
        private SerializedProperty _propIsDrawLineSetId7;
        private SerializedProperty _propIsDrawLineSetId8;

        private bool _isTextureOutputFoldoutOpen = true;
        private bool _isOutputCategoryFoldoutOpen = true;
        private bool _isBackgroundColorFoldoutOpen = true;
        private bool _isOutputEdgeFoldoutOpen = true;
        private bool _isOutputLineSetIdFoldoutOpen = true;
        
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CreateTextureOutputGui();
            CreateOutputCategoryGui();
            CreateBackgroundColorGui();
            CreateOutputEdgeGui();
            CreateOutputLineSetIdGui();
            serializedObject.ApplyModifiedProperties();
        }


        private void OnEnable()
        {
            _propTargetTexture = serializedObject.FindProperty("TargetTexture");
            _propIsDrawVisibleLines = serializedObject.FindProperty("IsDrawVisibleLines");
            _propIsDrawHiddenLines = serializedObject.FindProperty("IsDrawHiddenLines");
            _propIsBackgroundColorEnabled = serializedObject.FindProperty("IsBackgroundColorEnabled");
            _propBackgroundColor = serializedObject.FindProperty("BackgroundColor");
            _propIsDrawEdgeOutline = serializedObject.FindProperty("IsDrawEdgeOutline");
            _propIsDrawEdgeObject = serializedObject.FindProperty("IsDrawEdgeObject");
            _propIsDrawEdgeIntersection = serializedObject.FindProperty("IsDrawEdgeIntersection");
            _propIsDrawEdgeSmoothingBoundary = serializedObject.FindProperty("IsDrawEdgeSmoothingBoundary");
            _propIsDrawEdgeMaterialIdBoundary = serializedObject.FindProperty("IsDrawEdgeMaterialIdBoundary");
            //_propIsDrawEdgeSelectedEdges = serializedObject.FindProperty("IsDrawEdgeSelectedEdges");
            _propIsDrawEdgeNormalAngle = serializedObject.FindProperty("IsDrawEdgeNormalAngle");
            _propIsDrawEdgeWireframe = serializedObject.FindProperty("IsDrawEdgeWireframe");
            _propIsDrawLineSetId1 = serializedObject.FindProperty("IsDrawLineSetId1");
            _propIsDrawLineSetId2 = serializedObject.FindProperty("IsDrawLineSetId2");
            _propIsDrawLineSetId3 = serializedObject.FindProperty("IsDrawLineSetId3");
            _propIsDrawLineSetId4 = serializedObject.FindProperty("IsDrawLineSetId4");
            _propIsDrawLineSetId5 = serializedObject.FindProperty("IsDrawLineSetId5");
            _propIsDrawLineSetId6 = serializedObject.FindProperty("IsDrawLineSetId6");
            _propIsDrawLineSetId7 = serializedObject.FindProperty("IsDrawLineSetId7");
            _propIsDrawLineSetId8 = serializedObject.FindProperty("IsDrawLineSetId8");
        }


        private void CreateTextureOutputGui()
        {
            //"Render Texture Output"
            _isTextureOutputFoldoutOpen = EditorGUILayout.Foldout(_isTextureOutputFoldoutOpen, "Render Texture Output");

            ++EditorGUI.indentLevel;
            if (_isTextureOutputFoldoutOpen)
            {
                EditorGUILayout.ObjectField(_propTargetTexture, new GUIContent("Target Texture"));
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
        }

        private void CreateOutputCategoryGui()
        {
            // "Output Category"
            _isOutputCategoryFoldoutOpen = EditorGUILayout.Foldout(_isOutputCategoryFoldoutOpen, "Output Category");

            // "Visible Lines"
            // "Hidden Lines"
            ++EditorGUI.indentLevel;
            if (_isOutputCategoryFoldoutOpen)
            {
                _propIsDrawVisibleLines.boolValue = 
                    EditorGUILayout.Toggle("Visible Lines", _propIsDrawVisibleLines.boolValue);
                _propIsDrawHiddenLines.boolValue =
                    EditorGUILayout.Toggle("Hidden Lines", _propIsDrawHiddenLines.boolValue);
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
        }


        private void CreateBackgroundColorGui()
        {
            // "Background Color"
            _isBackgroundColorFoldoutOpen = EditorGUILayout.Foldout(_isBackgroundColorFoldoutOpen, "Background Color");
            // "Enable"
            // "Color"
            ++EditorGUI.indentLevel;
            if (_isBackgroundColorFoldoutOpen)
            {
                _propIsBackgroundColorEnabled.boolValue =
                    EditorGUILayout.Toggle("Enabled", _propIsBackgroundColorEnabled.boolValue);
                using (new EditorGUI.DisabledGroupScope(!_propIsBackgroundColorEnabled.boolValue))
                {
                    _propBackgroundColor.colorValue =
                        EditorGUILayout.ColorField("Color", _propBackgroundColor.colorValue);
                }
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
        }

        private void CreateOutputEdgeGui()
        {
            // "Output Edge"
            _isOutputEdgeFoldoutOpen = EditorGUILayout.Foldout(_isOutputEdgeFoldoutOpen, "Output Edge");

            ++EditorGUI.indentLevel;
            if (_isOutputEdgeFoldoutOpen)
            {
                _propIsDrawEdgeOutline.boolValue = 
                    EditorGUILayout.Toggle("Outline", _propIsDrawEdgeOutline.boolValue);
                _propIsDrawEdgeObject.boolValue = 
                    EditorGUILayout.Toggle("Object", _propIsDrawEdgeObject.boolValue);
                _propIsDrawEdgeIntersection.boolValue =
                    EditorGUILayout.Toggle("Intersection", _propIsDrawEdgeIntersection.boolValue);
                _propIsDrawEdgeSmoothingBoundary.boolValue =
                    EditorGUILayout.Toggle("Smoothing Boundary", _propIsDrawEdgeSmoothingBoundary.boolValue);
                _propIsDrawEdgeMaterialIdBoundary.boolValue =
                    EditorGUILayout.Toggle("Material ID Boundary", _propIsDrawEdgeMaterialIdBoundary.boolValue);  
//                _propIsDrawEdgeSelectedEdges.boolValue =
//                    EditorGUILayout.Toggle("Selected Edge", _propIsDrawEdgeSelectedEdges.boolValue);
                _propIsDrawEdgeNormalAngle.boolValue =
                    EditorGUILayout.Toggle("Normal Angle", _propIsDrawEdgeNormalAngle.boolValue);
                _propIsDrawEdgeWireframe.boolValue =
                    EditorGUILayout.Toggle("Wireframe", _propIsDrawEdgeWireframe.boolValue);
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
        }

        private void CreateOutputLineSetIdGui()
        {
            // "Output Line Set ID"
            _isOutputLineSetIdFoldoutOpen = EditorGUILayout.Foldout(_isOutputLineSetIdFoldoutOpen, "Output Line Set ID");

            //EditorGUI.indentLevel += 2;
            if (_isOutputLineSetIdFoldoutOpen)
            {
                var maxWidth = GUILayout.MaxWidth(60);
                ++EditorGUI.indentLevel;
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {

                        _propIsDrawLineSetId1.boolValue =
                            EditorGUILayout.ToggleLeft("1", _propIsDrawLineSetId1.boolValue, maxWidth);

                        _propIsDrawLineSetId2.boolValue =
                            EditorGUILayout.ToggleLeft("2", _propIsDrawLineSetId2.boolValue, maxWidth);

                        _propIsDrawLineSetId3.boolValue =
                            EditorGUILayout.ToggleLeft("3", _propIsDrawLineSetId3.boolValue, maxWidth);

                        _propIsDrawLineSetId4.boolValue =
                            EditorGUILayout.ToggleLeft("4", _propIsDrawLineSetId4.boolValue, maxWidth);

                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _propIsDrawLineSetId5.boolValue =
                            EditorGUILayout.ToggleLeft("5", _propIsDrawLineSetId5.boolValue, maxWidth);

                        _propIsDrawLineSetId6.boolValue =
                            EditorGUILayout.ToggleLeft("6", _propIsDrawLineSetId6.boolValue, maxWidth);

                        _propIsDrawLineSetId7.boolValue =
                            EditorGUILayout.ToggleLeft("7", _propIsDrawLineSetId7.boolValue, maxWidth);

                        _propIsDrawLineSetId8.boolValue =
                            EditorGUILayout.ToggleLeft("8", _propIsDrawLineSetId8.boolValue, maxWidth);
                    }
                }

                --EditorGUI.indentLevel;
            }
            //EditorGUI.indentLevel -= 2;
            // ...
        }

        [MenuItem("GameObject/Pencil+ 4/Render Elements Line Node", priority = 20)]
        public static void OpenRenderElementsLineNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<RenderElementsLineNode>(menuCommand);
        }
    }
}
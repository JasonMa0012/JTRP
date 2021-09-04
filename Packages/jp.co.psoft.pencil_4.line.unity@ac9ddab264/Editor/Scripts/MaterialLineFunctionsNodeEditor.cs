﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(MaterialLineFunctionsNode))]
    public class MaterialLineFunctionsNodeEditor : Editor
    {
        private SerializedProperty _propTargetMaterials;
        private SerializedProperty _propOutlineOn;
        private SerializedProperty _propOutlineColor;
        private SerializedProperty _propOutlineAmount;
        private SerializedProperty _propObjectOn;
        private SerializedProperty _propObjectColor;
        private SerializedProperty _propObjectAmount;
        private SerializedProperty _propIntersectionOn;
        private SerializedProperty _propIntersectionColor;
        private SerializedProperty _propIntersectionAmount;
        private SerializedProperty _propSmoothOn;
        private SerializedProperty _propSmoothColor;
        private SerializedProperty _propSmoothAmount;
        private SerializedProperty _propMaterialOn;
        private SerializedProperty _propMaterialColor;
        private SerializedProperty _propMaterialAmount;
        private SerializedProperty _propNormalAngleOn;
        private SerializedProperty _propNormalAngleColor;
        private SerializedProperty _propNormalAngleAmount;
        private SerializedProperty _propWireframeOn;
        private SerializedProperty _propWireframeColor;
        private SerializedProperty _propWireframeAmount;
        private SerializedProperty _propDisableIntersection;
        private SerializedProperty _propDrawHiddenLines;
        private SerializedProperty _propDrawHiddenLinesOfTarget;
        private SerializedProperty _propDrawObjects;
        private SerializedProperty _propDrawMaterials;
        private SerializedProperty _propMaskHiddenLinesOfTarget;
        private SerializedProperty _propMaskObjects;
        private SerializedProperty _propMaskMaterials;

        private bool _foldoutReplaceLineColor = true;
        private bool _foldoutEdgeDetection = true;
        private static GUIStyle _indentStyle;

        private PencilReorderableList _reorderableTargetMaterialList;
        private PencilReorderableList _reorderableDrawObjectList;
        private PencilReorderableList _reorderableDrawMaterialList;
        private PencilReorderableList _reorderableMaskObjectList;
        private PencilReorderableList _reorderableMaskMaterialList;

        private const int DoubleClickDiff = 500;

        private void ChangeIndent()
        {
            var indent = EditorGUI.indentLevel > 0 ? EditorGUI.indentLevel : 0;
            _indentStyle.margin = new RectOffset(indent * 20, 0, 0, 0);
        }

        /// <summary>
        /// delegate用リスト取得関数
        /// </summary>
        /// <typeparam name="T">リストのタイプ</typeparam>
        /// <param name="node">取得を行うノード</param>
        /// <param name="varName">取得する変数名</param>
        /// <returns>取得リスト</returns>
        List<T> GetList<T>(NodeBase node, string varName)
            where T : UnityEngine.Object
        {
            var lineFunctionsNode = node as MaterialLineFunctionsNode;

            var newList = new List<T>();
            switch (varName)
            {
                case "TargetMaterials":
                    newList = lineFunctionsNode.TargetMaterials.Cast<T>().ToList();
                    break;

                case "DrawObjects":
                    newList = lineFunctionsNode.DrawObjects.Cast<T>().ToList();
                    break;

                case "DrawMaterials":
                    newList = lineFunctionsNode.DrawMaterials.Cast<T>().ToList();
                    break;

                case "MaskObjects":
                    newList = lineFunctionsNode.MaskObjects.Cast<T>().ToList();
                    break;

                case "MaskMaterials":
                    newList = lineFunctionsNode.MaskMaterials.Cast<T>().ToList();
                    break;

                default:
                    break;
            }

            return newList;
        }



        /// <summary>
        /// Objectを追加、削除を行うリストを作成
        /// </summary>
        /// <typeparam name="T">リストの型</typeparam>
        /// <param name="prop">リストのプロパティ</param>
        /// <param name="reorderableList">リスト</param>
        /// <param name="style">リストのスタイル</param>
        private void CreateObjectListGui<T>(SerializedProperty prop,
                         PencilReorderableList reorderableList,
                         GUIStyle style,
                         string label,
                         bool checkOtherNode = false)
            where T : UnityEngine.Object
        {
            var currentLineFunctions = target as MaterialLineFunctionsNode;

            var lineListNode =
                         currentLineFunctions.transform.parent != null ?
                         currentLineFunctions.transform.parent.GetComponent<LineListNode>() :
                         null;

            List<GameObject> parentList;

            if (lineListNode != null)
            {
                parentList = lineListNode.LineFunctionsList;
            }
            else
            {
                checkOtherNode = false;
                parentList = null;
            }

            EditorGUILayout.LabelField(label);


            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            // ドラッグアンドドロップ処理
            Common.DragAndDropObject<T, MaterialLineFunctionsNode>(
                                        lineListNode,
                                        parentList,
                                        prop,
                                        verticalLayout.rect,
                                        GetList<T>,
                                        checkOtherNode);

            // 表示
            using (verticalLayout)
            {
                reorderableList.HandleInputEventAndLayoutList();
            }

        }


        private void CreateTargetMaterialsGui()
        {

            var style = new GUIStyle {margin = new RectOffset(4, 8, 0, 4)};

            CreateObjectListGui<Material>(_propTargetMaterials,
                               _reorderableTargetMaterialList,
                               style,
                               "Target Materials",
                               true);

            EditorGUILayout.Separator();
        }


        /// <summary>
        /// ReplaceLineColor項目のGUIパーツ
        /// </summary>
        /// <param name="label">表示するラベル</param>
        /// <param name="propOn">Enable</param>
        /// <param name="propColor">色</param>
        /// <param name="propAmount">量</param>
        private void CreateReplaceLineColorPartsGui(string label,
                                       SerializedProperty propOn,
                                       SerializedProperty propColor,
                                       SerializedProperty propAmount)
        {
            propOn.boolValue = EditorGUILayout.Toggle(label, propOn.boolValue);

            ++EditorGUI.indentLevel;

            EditorGUI.BeginDisabledGroup(!propOn.boolValue); // Replace Line Color Disable

            propColor.colorValue = EditorGUILayout.ColorField("Replace Color", propColor.colorValue);
            propAmount.floatValue = EditorGUILayout.Slider("Replace Amount",
                                                           propAmount.floatValue,
                                                           0.0f, 1.0f);

            EditorGUI.EndDisabledGroup(); // End of Replace Line Color Disable

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// ReplaceLineColor項目のGUIの追加
        /// </summary>
        private void CreateReplaceLineColorGui()
        {
            _foldoutReplaceLineColor =
                EditorGUILayout.Foldout(_foldoutReplaceLineColor, "Replace Line Color");
            if (!_foldoutReplaceLineColor)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Outline
            CreateReplaceLineColorPartsGui("Outline",
                                      _propOutlineOn,
                                      _propOutlineColor,
                                      _propOutlineAmount);

            // Object
            CreateReplaceLineColorPartsGui("Object",
                                      _propObjectOn,
                                      _propObjectColor,
                                      _propObjectAmount);

            // Intersection
            CreateReplaceLineColorPartsGui("Intersection",
                                      _propIntersectionOn,
                                      _propIntersectionColor,
                                      _propIntersectionAmount);

            // Smooth
            CreateReplaceLineColorPartsGui("Smoothing Boundary",
                                      _propSmoothOn,
                                      _propSmoothColor,
                                      _propSmoothAmount);

            // Material
            CreateReplaceLineColorPartsGui("Material Boundary",
                                      _propMaterialOn,
                                      _propMaterialColor,
                                      _propMaterialAmount);

            // Normal Angle
            CreateReplaceLineColorPartsGui("Normal Angle",
                                      _propNormalAngleOn,
                                      _propNormalAngleColor,
                                      _propNormalAngleAmount);

            // Wireframe
            CreateReplaceLineColorPartsGui("Wireframe",
                                      _propWireframeOn,
                                      _propWireframeColor,
                                      _propWireframeAmount);

            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// EdgeDetection項目のGUIを追加
        /// </summary>
        private void CreateEdgeDetectionGui()
        {
            _foldoutEdgeDetection =
                EditorGUILayout.Foldout(_foldoutEdgeDetection, "EdgeDetection");
            if (!_foldoutEdgeDetection)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Disable Intersection
            _propDisableIntersection.boolValue =
                EditorGUILayout.Toggle("Disable Intersection",
                                       _propDisableIntersection.boolValue);

            // Draw Hidden Lines
            _propDrawHiddenLines.boolValue =
                EditorGUILayout.Toggle("Draw Hidden Lines",
                                        _propDrawHiddenLines.boolValue);


            using (new EditorGUI.DisabledGroupScope(_propDrawHiddenLines.boolValue))
            {
                // Draw Hidden Lines of Target
                EditorGUILayout.LabelField("Draw Hidden Lines of Target");

                ++EditorGUI.indentLevel;

                _propDrawHiddenLinesOfTarget.boolValue =
                    EditorGUILayout.Toggle("On",
                                            _propDrawHiddenLinesOfTarget.boolValue);

                var style = new GUIStyle {margin = new RectOffset(60, 8, 0, 4)};

                using (new EditorGUI.DisabledGroupScope(!_propDrawHiddenLinesOfTarget.boolValue))
                {
                    // Draw Objects
                    CreateObjectListGui<GameObject>(_propDrawObjects,
                                         _reorderableDrawObjectList,
                                         style,
                                         "Object List");

                    // Draw Materials
                    CreateObjectListGui<Material>(_propDrawMaterials,
                                       _reorderableDrawMaterialList,
                                       style,
                                       "Material List");

                }

                --EditorGUI.indentLevel;

                // Mask Hidden Lines of Target
                EditorGUILayout.LabelField("Mask Hidden Lines of Target");

                ++EditorGUI.indentLevel;

                _propMaskHiddenLinesOfTarget.boolValue =
                    EditorGUILayout.Toggle("On",
                                            _propMaskHiddenLinesOfTarget.boolValue);


                using (new EditorGUI.DisabledGroupScope(!_propMaskHiddenLinesOfTarget.boolValue))
                {
                    // Mask Objects
                    CreateObjectListGui<GameObject>(_propMaskObjects,
                                         _reorderableMaskObjectList,
                                         style,
                                         "Object List");

                    // Mask Materials
                    CreateObjectListGui<Material>(_propMaskMaterials,
                                       _reorderableMaskMaterialList,
                                       style,
                                       "Material List");
                }

                --EditorGUI.indentLevel;
            }


            --EditorGUI.indentLevel;

        }

        private void OnEnable()
        {
            if (_indentStyle == null)
            {
                _indentStyle = new GUIStyle
                {
                    border = new RectOffset(1, 1, 1, 1),
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(60, 0, 0, 0)
                };
            }

            _propTargetMaterials = serializedObject.FindProperty("TargetMaterials");
            _propOutlineOn = serializedObject.FindProperty("OutlineOn");
            _propOutlineColor = serializedObject.FindProperty("OutlineColor");
            _propOutlineAmount = serializedObject.FindProperty("OutlineAmount");
            _propObjectOn = serializedObject.FindProperty("ObjectOn");
            _propObjectColor = serializedObject.FindProperty("ObjectColor");
            _propObjectAmount = serializedObject.FindProperty("ObjectAmount");
            _propIntersectionOn = serializedObject.FindProperty("IntersectionOn");
            _propIntersectionColor = serializedObject.FindProperty("IntersectionColor");
            _propIntersectionAmount = serializedObject.FindProperty("IntersectionAmount");
            _propSmoothOn = serializedObject.FindProperty("SmoothOn");
            _propSmoothColor = serializedObject.FindProperty("SmoothColor");
            _propSmoothAmount = serializedObject.FindProperty("SmoothAmount");
            _propMaterialOn = serializedObject.FindProperty("MaterialOn");
            _propMaterialColor = serializedObject.FindProperty("MaterialColor");
            _propMaterialAmount = serializedObject.FindProperty("MaterialAmount");
            _propNormalAngleOn = serializedObject.FindProperty("NormalAngleOn");
            _propNormalAngleColor = serializedObject.FindProperty("NormalAngleColor");
            _propNormalAngleAmount = serializedObject.FindProperty("NormalAngleAmount");
            _propWireframeOn = serializedObject.FindProperty("WireframeOn");
            _propWireframeColor = serializedObject.FindProperty("WireframeColor");
            _propWireframeAmount = serializedObject.FindProperty("WireframeAmount");
            _propDisableIntersection = serializedObject.FindProperty("DisableIntersection");
            _propDrawHiddenLines = serializedObject.FindProperty("DrawHiddenLines");
            _propDrawHiddenLinesOfTarget = serializedObject.FindProperty("DrawHiddenLinesOfTarget");
            _propDrawObjects = serializedObject.FindProperty("DrawObjects");
            _propDrawMaterials = serializedObject.FindProperty("DrawMaterials");
            _propMaskHiddenLinesOfTarget = serializedObject.FindProperty("MaskHiddenLinesOfTarget");
            _propMaskObjects = serializedObject.FindProperty("MaskObjects");
            _propMaskMaterials = serializedObject.FindProperty("MaskMaterials");


            _reorderableTargetMaterialList = Common.CreateMaterialList(
                serializedObject,
                _propTargetMaterials,
                Resources.FindObjectsOfTypeAll<MaterialLineFunctionsNode>().SelectMany(x => x.TargetMaterials),
                selectedMaterials => 
                {
                    serializedObject.Update();
                    _propTargetMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    _reorderableTargetMaterialList.index = _reorderableTargetMaterialList.count - 1;
                });

            _reorderableDrawObjectList = Common.CreateObjectList(
                serializedObject,
                _propDrawObjects,
                new List<GameObject>(), 
                selectedObjects => 
                {
                    serializedObject.Update();
                    _propDrawObjects.AppendObjects(selectedObjects);
                    serializedObject.ApplyModifiedProperties();
                    _reorderableDrawObjectList.index = _reorderableDrawObjectList.count - 1;
                });


            _reorderableDrawMaterialList = Common.CreateMaterialList(
                serializedObject,
                _propDrawMaterials,
                new List<Material>(),
                selectedMaterials => 
                {
                    serializedObject.Update();
                    _propDrawMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    _reorderableDrawMaterialList.index = _reorderableDrawMaterialList.count - 1;
                });


            _reorderableMaskObjectList = Common.CreateObjectList(
                serializedObject,
                _propMaskObjects,
                new List<GameObject>(),
                selectedObjects => 
                {
                    serializedObject.Update();
                    _propMaskObjects.AppendObjects(selectedObjects);
                    serializedObject.ApplyModifiedProperties();
                    _reorderableMaskObjectList.index = _reorderableMaskObjectList.count - 1;
                });


            _reorderableMaskMaterialList = Common.CreateMaterialList(
                serializedObject,
                _propMaskMaterials,
                new List<Material>(),
                selectedMaterials => 
                {
                    serializedObject.Update();
                    _propMaskMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    _reorderableMaskMaterialList.index = _reorderableMaskMaterialList.count - 1;
                });

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Event.current.type == EventType.Repaint)
            {
                _reorderableDrawObjectList.OnRepaint();
                _reorderableDrawObjectList.OnRepaint();
                _reorderableMaskObjectList.OnRepaint();
                _reorderableMaskMaterialList.OnRepaint();
            }

            CreateTargetMaterialsGui();
            CreateReplaceLineColorGui();
            CreateEdgeDetectionGui();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineFunctionsノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Material Line Functions Node", priority = 20)]
        public static void OpenMaterialLineFunctionsNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<MaterialLineFunctionsNode>(menuCommand, typeof(LineListNode), "LineFunctionsList");
        }
    }
}
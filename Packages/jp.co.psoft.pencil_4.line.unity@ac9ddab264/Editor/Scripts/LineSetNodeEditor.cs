#pragma warning disable 0414
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{

    using Common = EditorCommons;
    using LineType = LineSetNode.LineType;

    [CustomEditor(typeof(LineSetNode))]
    public class LineSetNodeEditor : Editor
    {
        public bool foldoutBrush = true;
        public bool foldoutEdge = true;
        public bool foldoutReduction = true;

        // Maya版のタブの代わりにセット
        [SerializeField]
        public LineType currentLineType =
            LineType.Visible;

        private SerializedProperty propId;

        private SerializedProperty propObjects;
        private SerializedProperty propMaterials;

        private struct PropertyLineType
        {
            public SerializedProperty propBrushSettings;
            public SerializedProperty propBrushDetail;
            public SerializedProperty propBlendMode;
            public SerializedProperty propBlendAmount;
            public SerializedProperty propColor;
            public SerializedProperty propColorMap;
            public SerializedProperty propMapOpacity;
            public SerializedProperty propSize;
            public SerializedProperty propSizeMap;
            public SerializedProperty propSizeMapAmount;
            public SerializedProperty propStretch;
            public SerializedProperty propAngle;
            public SerializedProperty propEdgeOutlineOn;
            public SerializedProperty propEdgeOutlineOpen;
            public SerializedProperty propEdgeOutlineMergeGroups;
            public SerializedProperty propEdgeOutlineSpecificOn;
            public SerializedProperty propEdgeOutline;
            public SerializedProperty propEdgeObjectOn;
            public SerializedProperty propEdgeObjectOpen;
            public SerializedProperty propEdgeObjectSpecificOn;
            public SerializedProperty propEdgeObject;
            public SerializedProperty propEdgeIntersectionSelf;
            public SerializedProperty propEdgeIntersectionOn;
            public SerializedProperty propEdgeIntersectionSpecificOn;
            public SerializedProperty propEdgeIntersection;
            public SerializedProperty propEdgeSmoothOn;
            public SerializedProperty propEdgeSmoothSpecificOn;
            public SerializedProperty propEdgeSmooth;
            public SerializedProperty propEdgeMaterialOn;
            public SerializedProperty propEdgeMaterialSpecificOn;
            public SerializedProperty propEdgeMaterial;
            public SerializedProperty propEdgeNormalAngleOn;
            public SerializedProperty propEdgeNormalAngleSpecificOn;
            public SerializedProperty propEdgeNormalAngle;
            public SerializedProperty propEdgeNormalAngleMin;
            public SerializedProperty propEdgeNormalAngleMax;
            public SerializedProperty propEdgeWireframeOn;
            public SerializedProperty propEdgeWireframeSpecificOn;
            public SerializedProperty propEdgeWireframe;
            public SerializedProperty propSizeReductionOn;
            public SerializedProperty propSizeReduction;
            public SerializedProperty propAlphaReductionOn;
            public SerializedProperty propAlphaReduction;
        }

        private PropertyLineType visibleParams;
        private PropertyLineType hiddenParams;
        private PropertyLineType currentParams;

        private SerializedProperty propWeldsEdges;
        private SerializedProperty propMaskHiddenLines;

        private SerializedObject serializedBrushSettingsVisibleParams;
        private SerializedObject serializedBrushSettingsHiddenParams;
        private SerializedObject serializedBrushDetailVisibleParams;
        private SerializedObject serializedBrushDetailHiddenParams;

        private PencilReorderableList reorderableObjectsList;
        private PencilReorderableList reorderableMaterialsList;

        /// <summary>
        /// 現在のLineTypeに応じた設定に再設定を行うメソッド
        /// </summary>
        /// <param name="lineType">現在のLineType</param>
        private void ChangeDisplayingBlushSettings(LineType lineType)
        {
            switch (lineType)
            {
                case LineType.Visible:
                    currentParams = visibleParams;
                    break;

                case LineType.Hidden:
                    currentParams = hiddenParams;
                    break;
            }

        }



        /// <summary>
        /// EdgeのGUIの作成
        /// </summary>
        /// <param name="lineSetNode">現在のLineSetNode</param>
        /// <param name="label">ラベル</param>
        /// <param name="edgeProps"></param>
        private void CreateIndividualEdgeGui(
            LineSetNode lineSetNode,
            string label,
            EdgeProps edgeProps)
        {
            // Label
            EditorGUILayout.LabelField(label);

            ++EditorGUI.indentLevel;
            
            // On
            edgeProps.On.boolValue = EditorGUILayout.Toggle("On", edgeProps.On.boolValue);
            
            using (new EditorGUI.DisabledGroupScope(!edgeProps.On.boolValue))
            {
                // Open Edge
                if (edgeProps.OpenEdge != null)
                {
                    edgeProps.OpenEdge.boolValue = EditorGUILayout.Toggle("Open Edge", edgeProps.OpenEdge.boolValue);
                }
                
                // Self Intersection
                if (edgeProps.SelfIntersection != null)
                {
                    edgeProps.SelfIntersection.boolValue =
                        EditorGUILayout.Toggle("Self Intersection", edgeProps.SelfIntersection.boolValue);
                }
                
                // Merge Groups
                if (edgeProps.MergeGroups != null)
                {
                    edgeProps.MergeGroups.boolValue =
                        EditorGUILayout.Toggle("Merge Groups",edgeProps.MergeGroups.boolValue);
                }
                
                // Specific On
                edgeProps.SpecificOn.boolValue = EditorGUILayout.Toggle("Specific On", edgeProps.SpecificOn.boolValue);


                using (new EditorGUI.DisabledGroupScope(!edgeProps.SpecificOn.boolValue))
                {
                    // BrushSettings
                    EditorGUICustomLayout.PencilNodeField(
                        "",
                        typeof(BrushSettingsNode),
                        serializedObject,
                        edgeProps.BrushSettings,
                        (nodeObject) => { });
                }

                // Normal Angle Params
                if (edgeProps.NormalAngleMin != null && edgeProps.NormalAngleMax != null)
                {
                    edgeProps.NormalAngleMin.floatValue = EditorGUILayout.Slider("Min",
                        edgeProps.NormalAngleMin.floatValue,
                        0, 180);

                    edgeProps.NormalAngleMax.floatValue = EditorGUILayout.Slider("Max",
                        edgeProps.NormalAngleMax.floatValue,
                        0, 180);
                }
            }
            
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// ReductionのGUIの作成
        /// </summary>
        /// <param name="lineSetNode">現在のLineSetNode</param>
        /// <param name="label">ラベル</param>
        /// <param name="propReductionOn">On/Offの切り替えに使用するSerializeProperty型の変数</param>
        /// <param name="propReductionSettings">ReductionSettingsに使用するSerializeProperty型の変数</param>
        private void CreateIndividualReductionGui(LineSetNode lineSetNode,
                                string label,
                                SerializedProperty propReductionOn,
                                SerializedProperty propReductionSettings)
        {
            EditorGUILayout.LabelField(label);

            ++EditorGUI.indentLevel;

            // On
            propReductionOn.boolValue = EditorGUILayout.Toggle("On", propReductionOn.boolValue);

            using (new EditorGUI.DisabledGroupScope(!propReductionOn.boolValue))
            {
                // ReductionSettings
                EditorGUICustomLayout.PencilNodeField(
                        "",
                        typeof(ReductionSettingsNode),
                        serializedObject,
                        propReductionSettings,
                        (nodeObject) => { });
            }

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// ObjectsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateObjectsListGui(GUIStyle style)
        {
            var lineSetNode = target as LineSetNode;

            EditorGUILayout.LabelField("Objects");

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<GameObject, LineSetNode>(
                lineSetNode,
                null,
                propObjects,
                verticalLayout.rect,
                (node, _) => ((LineSetNode)node).Objects.ToList(),
                false);

            using (verticalLayout)
            {
                reorderableObjectsList.HandleInputEventAndLayoutList();
            }
        }


        /// <summary>
        /// MaterialsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateMaterialsListGui(GUIStyle style)
        {
            var lineSetNode = target as LineSetNode;

            EditorGUILayout.LabelField("Materials");


            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, LineSetNode>(
                lineSetNode,
                null,
                propMaterials,
                verticalLayout.rect,
                (node, _) => ((LineSetNode)node).Materials.ToList(),
                false);

            using (verticalLayout)
            {
                reorderableMaterialsList.HandleInputEventAndLayoutList();
            }
        }


        /// <summary>
        /// LineSetのGUIを作成
        /// </summary>
        private void CreateLineSetGui()
        {
            // Lists
            var style = new GUIStyle {margin = new RectOffset(4, 8, 0, 4)};

            CreateObjectsListGui(style);
            CreateMaterialsListGui(style);

            // ID
            propId.intValue = EditorGUILayout.IntSlider("ID", propId.intValue, 1, 8);

            // LineType (Visible or Hidden)
            // Tabの代わりにボタンを置く
            using (new EditorGUILayout.HorizontalScope())
            {
                using (var toggleChange = new EditorGUI.ChangeCheckScope())
                {
                    var isVisible = GUILayout.Toggle(currentLineType == LineType.Visible, "Visible",
                        BuiltInResources.VisibleToggleButtonStyle);
                    if (toggleChange.changed && isVisible)
                    {
                        currentLineType = LineSetNode.LineType.Visible;
                        Repaint();
                    }
                }
                
                using (var toggleChange = new EditorGUI.ChangeCheckScope())
                {
                    var isHidden = GUILayout.Toggle(currentLineType == LineType.Hidden, "Hidden",
                        BuiltInResources.HiddenToggleButtonStyle);
                    if (toggleChange.changed && isHidden)
                    {
                        currentLineType = LineSetNode.LineType.Hidden;
                        Repaint();
                    }
                }

            }

            ChangeDisplayingBlushSettings(currentLineType);
        }

        /// <summary>
        /// Brush項目のGUIを作成
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
        private void CreateBrushSectionGui(LineSetNode lineSetNode)
        {

            foldoutBrush =
                EditorGUILayout.Foldout(foldoutBrush, "Brush");
            if (!foldoutBrush)
            {
                return;
            }

            ++EditorGUI.indentLevel;


            EditorGUICustomLayout.PencilNodeField(
                "Brush Settings",
                typeof(BrushSettingsNode),
                serializedObject,
                currentParams.propBrushSettings,
                (nodeObject) => 
                {
                    VisibilitySelectionChanged();
                });


            var bsObj = currentParams.propBrushSettings.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bsObj == null))
            {
                MakeBrushSettings((GameObject)currentParams.propBrushSettings.objectReferenceValue);
            }

            EditorGUILayout.Separator();
        }

        /// <summary>
        /// Edgeのパラメータ
        /// </summary>
        private struct EdgeProps
        {
            public SerializedProperty On;
            public SerializedProperty OpenEdge;
            public SerializedProperty SelfIntersection;
            public SerializedProperty MergeGroups;
            public SerializedProperty SpecificOn;
            public SerializedProperty BrushSettings;
            public SerializedProperty NormalAngleMin;
            public SerializedProperty NormalAngleMax;
        };

        /// <summary>
        /// Edge項目のGUIを作成
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
        private void CreateEdgeGui(LineSetNode lineSetNode)
        {
            foldoutEdge =
                EditorGUILayout.Foldout(foldoutEdge, "Edge");
            if (!foldoutEdge)
            {
                return;
            }

            var suffixName =
                currentLineType == LineSetNode.LineType.Visible ?
                " Visible " :
                " Hidden ";


            ++EditorGUI.indentLevel;

            var before = false;
            GameObject beforeObj;

            Action<string, EdgeProps> createEdgeGroupGui =
                (label, props) =>
            {
                before = props.SpecificOn.boolValue;
                beforeObj = props.BrushSettings.objectReferenceValue as GameObject;

                CreateIndividualEdgeGui(lineSetNode, label, props);

                if (before != props.SpecificOn.boolValue &&
                    before == false &&
                    props.BrushSettings.objectReferenceValue == null)
                {
                    var newBrushSettings = Pcl4EditorUtilities.CreateNodeObject<BrushSettingsNode>(lineSetNode.gameObject.transform, suffixName);
                    props.BrushSettings.objectReferenceValue = newBrushSettings;

                    Undo.RegisterCreatedObjectUndo(props.BrushSettings.objectReferenceValue, 
                                                   "Create Brush Settings");

                    var newBrushDetails = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newBrushSettings.transform, suffixName);

                    // BrushSettingsにBrushDetailを接続
                    var newBrushSettingsNode = newBrushSettings.GetComponent<BrushSettingsNode>();
                    newBrushSettingsNode.BrushDetail = newBrushDetails;

                }

                if (beforeObj != props.BrushSettings.objectReferenceValue)
                {
                    props.BrushSettings = 
                        props.BrushSettings.UndoObject<BrushSettingsNode>(beforeObj);
                }
            };



            // Outline
            createEdgeGroupGui("Outline", new EdgeProps
            {
                On = currentParams.propEdgeOutlineOn,
                OpenEdge = currentParams.propEdgeOutlineOpen,
                MergeGroups = currentParams.propEdgeOutlineMergeGroups,
                SpecificOn = currentParams.propEdgeOutlineSpecificOn,
                BrushSettings = currentParams.propEdgeOutline
            });

            
            // Object
            createEdgeGroupGui("Object", new EdgeProps
            {
                On = currentParams.propEdgeObjectOn,
                OpenEdge = currentParams.propEdgeObjectOpen,
                SpecificOn = currentParams.propEdgeObjectSpecificOn,
                BrushSettings = currentParams.propEdgeObject
            });

            
            // Intersection
            createEdgeGroupGui("Intersection", new EdgeProps
            {
                On = currentParams.propEdgeIntersectionOn,
                SelfIntersection = currentParams.propEdgeIntersectionSelf,
                SpecificOn = currentParams.propEdgeIntersectionSpecificOn,
                BrushSettings = currentParams.propEdgeIntersection
            });

            
            // Smoothing Boundary
            createEdgeGroupGui("Smoothing Boundary", new EdgeProps
            {
                On = currentParams.propEdgeSmoothOn,
                SpecificOn = currentParams.propEdgeSmoothSpecificOn,
                BrushSettings = currentParams.propEdgeSmooth
            });

            
            // Material Boundary
            createEdgeGroupGui("Material Boundary", new EdgeProps
            {
                On = currentParams.propEdgeMaterialOn,
                SpecificOn = currentParams.propEdgeMaterialSpecificOn,
                BrushSettings = currentParams.propEdgeMaterial
            });

            
            // Normal Angle
            createEdgeGroupGui("Normal Angle", new EdgeProps
            {
                On = currentParams.propEdgeNormalAngleOn,
                SpecificOn = currentParams.propEdgeNormalAngleSpecificOn,
                BrushSettings = currentParams.propEdgeNormalAngle,
                NormalAngleMin = currentParams.propEdgeNormalAngleMin,
                NormalAngleMax = currentParams.propEdgeNormalAngleMax
            });

            
            // Wireframe            
            createEdgeGroupGui("Wireframe", new EdgeProps
            {
                On = currentParams.propEdgeWireframeOn,
                SpecificOn = currentParams.propEdgeWireframeSpecificOn,
                BrushSettings = currentParams.propEdgeWireframe
            });

            --EditorGUI.indentLevel;
            EditorGUILayout.Separator();
        }

        private void CreateEdgeCommonParamsGui()
        {
            // Welds Edge Between Object
            EditorGUILayout.LabelField("Welds Edges Between Objects");

            ++EditorGUI.indentLevel;

            propWeldsEdges.boolValue =
                EditorGUILayout.Toggle("On", propWeldsEdges.boolValue);

            --EditorGUI.indentLevel;

            // Mask Hidden Lines of Other Line Sets
            EditorGUILayout.LabelField("Mask Hidden Lines of Other Line Sets");

            ++EditorGUI.indentLevel;

            propMaskHiddenLines.boolValue =
                EditorGUILayout.Toggle("On",
                    propMaskHiddenLines.boolValue);
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// Reduction項目のGUIを追加
        /// </summary>
        /// <param name="lineSetNode">LineSetNode</param>
        private void CreateReductionGui(LineSetNode lineSetNode)
        {
            foldoutReduction =
                EditorGUILayout.Foldout(foldoutReduction, "Reduction");
            if (!foldoutReduction)
            {
                return;
            }

            var suffixName =
                currentLineType == LineSetNode.LineType.Visible ?
                " Visible " :
                " Hidden ";


            ++EditorGUI.indentLevel;

            var before = false;
            GameObject beforeObj;

            Action<string, SerializedProperty, SerializedProperty> ReductionGroup =
                (label, propOn, propReduction) =>
            {
                before = propOn.boolValue;
                beforeObj = propReduction.objectReferenceValue as GameObject;
                CreateIndividualReductionGui(lineSetNode, label,
                                   propOn,
                                   propReduction);

                if (before != propOn.boolValue && !before && !propReduction.objectReferenceValue)
                {
                    var newReduction = Pcl4EditorUtilities.CreateNodeObject<ReductionSettingsNode>(lineSetNode.gameObject.transform, suffixName);
                    propReduction.objectReferenceValue = newReduction;
                    Undo.RegisterCreatedObjectUndo(propReduction.objectReferenceValue,
                                                   "Create Reduction Settings");
                }
                if (beforeObj != propReduction.objectReferenceValue)
                {
                    propReduction = 
                        propReduction.UndoObject<ReductionSettingsNode>(beforeObj);
                }
            };

            // Size
            ReductionGroup("Size Reduction",
                           currentParams.propSizeReductionOn,
                           currentParams.propSizeReduction);

            // Alpha
            ReductionGroup("Alpha Reduction",
                           currentParams.propAlphaReductionOn,
                           currentParams.propAlphaReduction);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// BrushSettings部分のGUIの作成
        /// </summary>
        /// <param name="linSetObject"></param>
        private void MakeBrushSettings(GameObject linSetObject)
        {
            Action noBrushSettings = () =>
            {
                var dummyColor = new Color();
                //BrushSettingsNode.BlendModeType dummyEnum =
                //    BrushSettingsNode.BlendModeType.Normal;

                EditorGUILayout.ObjectField("Brush Detail", null, typeof(GameObject), true);
                //EditorGUILayout.EnumPopup("Blend Mode", dummyEnum);
                EditorGUILayout.Slider("Blend Amount", 1.0f, 0.0f, 1.0f);
                EditorGUILayout.ColorField("Color", dummyColor);
                EditorGUILayout.ObjectField("ColorMap", null, typeof(Material), false);
                EditorGUILayout.Slider("Map Opacity", 1.0f, 0.0f, 1.0f);
                EditorGUILayout.Slider("Size", 1.0f, 0.1f, 20.0f);
                EditorGUILayout.ObjectField("Size Map", null, typeof(Material), false);
                EditorGUILayout.Slider("Size Map Amount", 1.0f, 0.0f, 1.0f);

                MakeBrushDetail(null);

                --EditorGUI.indentLevel;
            };


            if (linSetObject == null)
            {
                noBrushSettings();
                return;
            }

            var brushSettingsNode = linSetObject.GetComponent<BrushSettingsNode>();
            if (brushSettingsNode == null)
            {
                noBrushSettings();
                return;
            }


            if (currentLineType == LineSetNode.LineType.Visible)
            {
                serializedBrushSettingsVisibleParams.Update();
            }
            else
            {
                serializedBrushSettingsHiddenParams.Update();
            }

            // Brush Detail

            EditorGUICustomLayout.PencilNodeField(
                        "Brush Detail",
                        typeof(BrushDetailNode),
                        (currentLineType == LineSetNode.LineType.Visible) ?
                            serializedBrushSettingsVisibleParams :
                            serializedBrushSettingsHiddenParams,
                        currentParams.propBrushDetail,
                        (nodeObject) =>
                        {
                            // LineSetNodeEditorが参照しているBrushDetailsのプロパティを再設定
                            if (nodeObject == null)
                            {
                                return;
                            }

                            if (currentLineType == LineSetNode.LineType.Visible)
                            {
                                serializedBrushDetailVisibleParams =
                                    new SerializedObject(nodeObject.GetComponent<BrushDetailNode>());

                                currentParams.propStretch = serializedBrushDetailVisibleParams.FindProperty("Stretch");
                                currentParams.propAngle = serializedBrushDetailVisibleParams.FindProperty("Angle");
                            }
                            else
                            {
                                serializedBrushDetailHiddenParams =
                                    new SerializedObject(nodeObject.GetComponent<BrushDetailNode>());

                                currentParams.propStretch = serializedBrushDetailHiddenParams.FindProperty("Stretch");
                                currentParams.propAngle = serializedBrushDetailHiddenParams.FindProperty("Angle");

                            }
                        });


            // BlendMode
            //var blendMode = (BrushSettingsNode.BlendModeType)Enum
            //                .GetValues(typeof(BrushSettingsNode.BlendModeType))
            //                .GetValue(currentParams.propBlendMode.enumValueIndex);

            //currentParams.propBlendMode.enumValueIndex =
            //    (int)(LineSetNode.LineType)EditorGUILayout.EnumPopup("Blend Mode", blendMode);

            // BlendAmount
            currentParams.propBlendAmount.floatValue = EditorGUILayout.Slider("Blend Amount",
                                                                currentParams.propBlendAmount.floatValue,
                                                                0.0f, 1.0f);

            // Color
            currentParams.propColor.colorValue =
                EditorGUILayout.ColorField("Color", currentParams.propColor.colorValue);

            // ColorMap
            EditorGUICustomLayout.PencilNodeField(
                "Color Map",
                typeof(TextureMapNode),
                currentLineType == LineSetNode.LineType.Visible ?
                    serializedBrushSettingsVisibleParams :
                    serializedBrushSettingsHiddenParams,
                currentParams.propColorMap,
                nodeObject => { },
                () =>
                {
                    var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushSettingsNode.transform);
                    currentParams.propColorMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });

            // MapOpacity
            currentParams.propMapOpacity.floatValue = EditorGUILayout.Slider("Map Opacity",
                                                               currentParams.propMapOpacity.floatValue,
                                                               0.0f, 1.0f);

            // Size
            currentParams.propSize.floatValue = EditorGUILayout.Slider("Size",
                                                         currentParams.propSize.floatValue,
                                                         0.1f, 20.0f);


            // SizeMap
            EditorGUICustomLayout.PencilNodeField(
                "Size Map",
                typeof(TextureMapNode),
                currentLineType == LineSetNode.LineType.Visible ?
                    serializedBrushSettingsVisibleParams :
                    serializedBrushSettingsHiddenParams,
                currentParams.propSizeMap,
                nodeObject => { },
                () =>
                {
                    var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushSettingsNode.transform);
                    currentParams.propSizeMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });

            // SizeMapAmount
            currentParams.propSizeMapAmount.floatValue = EditorGUILayout.Slider("Size Map Amount",
                                                                  currentParams.propSizeMapAmount.floatValue,
                                                                  0.0f, 1.0f);

            if (currentLineType == LineSetNode.LineType.Visible)
            {
                serializedBrushSettingsVisibleParams.ApplyModifiedProperties();
            }
            else
            {
                serializedBrushSettingsHiddenParams.ApplyModifiedProperties();
            }

            // BrushDetailParams
            var bdObj = currentParams.propBrushDetail.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bdObj == null))
            {
                MakeBrushDetail((GameObject)currentParams.propBrushDetail.objectReferenceValue);
            }

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// BrushDetail部分のGUIの作成
        /// </summary>
        /// <param name="brushDetailObject">BrushDetailコンポーネントを持っているGameObject</param>
        private void MakeBrushDetail(GameObject brushDetailObject)
        {

            if (brushDetailObject == null
                || brushDetailObject.GetComponent<BrushDetailNode>() == null)
            {
                EditorGUILayout.Slider("Stretch", 0, -1.0f, 1.0f);
                EditorGUILayout.Slider("Angle", 0, -3600.0f, 3600.0f);
                return;
            }


            if (serializedBrushDetailVisibleParams != null)
            {
                serializedBrushDetailVisibleParams.Update();
            }
            if (serializedBrushDetailHiddenParams != null)
            {
                serializedBrushDetailHiddenParams.Update();
            }

            // Stretch
            currentParams.propStretch.floatValue = EditorGUILayout.Slider("Stretch",
                                                            currentParams.propStretch.floatValue,
                                                            -1.0f, 1.0f);

            // Angle
            currentParams.propAngle.floatValue = EditorGUILayout.Slider("Angle",
                                                          currentParams.propAngle.floatValue,
                                                          -3600.0f, 3600.0f);

            if (serializedBrushDetailVisibleParams != null)
            {
                serializedBrushDetailVisibleParams.ApplyModifiedProperties();
            }
            if (serializedBrushDetailHiddenParams != null)
            {
                serializedBrushDetailHiddenParams.ApplyModifiedProperties();
            }

        }

        /// <summary>
        /// BrushSettingsのプロパティの再取得を行うメソッド
        /// </summary>
        private void VisibilitySelectionChanged()
        {
            // Set Brush Settings Properties
            var brushSettings = visibleParams.propBrushSettings.objectReferenceValue as GameObject;

            serializedBrushSettingsVisibleParams =
                brushSettings != null ?
                new SerializedObject(brushSettings.GetComponent<BrushSettingsNode>()) :
                null;

            if (serializedBrushSettingsVisibleParams != null)
            {
                visibleParams.propBrushDetail =
                    serializedBrushSettingsVisibleParams.FindProperty("BrushDetail");
                visibleParams.propBlendMode =
                    serializedBrushSettingsVisibleParams.FindProperty("BlendMode");
                visibleParams.propBlendAmount =
                    serializedBrushSettingsVisibleParams.FindProperty("BlendAmount");
                visibleParams.propColor =
                    serializedBrushSettingsVisibleParams.FindProperty("BrushColor");
                visibleParams.propColorMap =
                    serializedBrushSettingsVisibleParams.FindProperty("ColorMap");
                visibleParams.propMapOpacity =
                    serializedBrushSettingsVisibleParams.FindProperty("MapOpacity");
                visibleParams.propSize =
                    serializedBrushSettingsVisibleParams.FindProperty("Size");
                visibleParams.propSizeMap =
                    serializedBrushSettingsVisibleParams.FindProperty("SizeMap");
                visibleParams.propSizeMapAmount =
                    serializedBrushSettingsVisibleParams.FindProperty("SizeMapAmount");
            }


            brushSettings = hiddenParams.propBrushSettings.objectReferenceValue as GameObject;

            serializedBrushSettingsHiddenParams =
                brushSettings != null ?
                new SerializedObject(brushSettings.GetComponent<BrushSettingsNode>()) :
                null;

            if (serializedBrushSettingsHiddenParams != null)
            {
                hiddenParams.propBrushDetail =
                    serializedBrushSettingsHiddenParams.FindProperty("BrushDetail");
                hiddenParams.propBlendMode =
                    serializedBrushSettingsHiddenParams.FindProperty("BlendMode");
                hiddenParams.propBlendAmount =
                    serializedBrushSettingsHiddenParams.FindProperty("BlendAmount");
                hiddenParams.propColor =
                    serializedBrushSettingsHiddenParams.FindProperty("BrushColor");
                hiddenParams.propColorMap =
                    serializedBrushSettingsHiddenParams.FindProperty("ColorMap");
                hiddenParams.propMapOpacity =
                    serializedBrushSettingsHiddenParams.FindProperty("MapOpacity");
                hiddenParams.propSize =
                    serializedBrushSettingsHiddenParams.FindProperty("Size");
                hiddenParams.propSizeMap =
                    serializedBrushSettingsHiddenParams.FindProperty("SizeMap");
                hiddenParams.propSizeMapAmount =
                    serializedBrushSettingsHiddenParams.FindProperty("SizeMapAmount");
            }


            // Set Brush Detail Properties
            var brushDetail = visibleParams.propBrushDetail != null ?
                visibleParams.propBrushDetail.objectReferenceValue as GameObject :
                null;

            serializedBrushDetailVisibleParams =
                brushDetail != null ?
                new SerializedObject(brushDetail.GetComponent<BrushDetailNode>()) :
                null;

            if (serializedBrushDetailVisibleParams != null)
            {
                visibleParams.propStretch = serializedBrushDetailVisibleParams.FindProperty("Stretch");
                visibleParams.propAngle = serializedBrushDetailVisibleParams.FindProperty("Angle");
            }

            brushDetail = hiddenParams.propBrushDetail != null ?
                          hiddenParams.propBrushDetail.objectReferenceValue as GameObject :
                          null;

            serializedBrushDetailHiddenParams =
                brushDetail != null ?
                new SerializedObject(brushDetail.GetComponent<BrushDetailNode>()) :
                null;

            if (serializedBrushDetailHiddenParams == null) return;
            hiddenParams.propStretch = serializedBrushDetailHiddenParams.FindProperty("Stretch");
            hiddenParams.propAngle = serializedBrushDetailHiddenParams.FindProperty("Angle");
        }

        private void OnEnable()
        {
            propId = serializedObject.FindProperty("Id");

            // ---------- Objects List ----------
            propObjects =
                serializedObject.FindProperty("Objects");

            reorderableObjectsList =
                Common.CreateObjectList(
                    serializedObject,
                    propObjects,
                    ((LineSetNode)target).Objects,
                    selectedObjects =>
                    {
                        serializedObject.Update();
                        propObjects.AppendObjects(selectedObjects);
                        serializedObject.ApplyModifiedProperties();
                        reorderableObjectsList.index = reorderableObjectsList.count - 1;
                    });

            // ---------- Materials List ----------
            propMaterials =
                serializedObject.FindProperty("Materials");

            reorderableMaterialsList =
                Common.CreateMaterialList(
                    serializedObject,
                    propMaterials,
                    new List<Material>(), 
                    selectedMaterials =>
                    {
                        serializedObject.Update();
                        propMaterials.AppendObjects(selectedMaterials);
                        serializedObject.ApplyModifiedProperties();
                        reorderableMaterialsList.index = reorderableMaterialsList.count - 1;
                    });

            // --------------------
            visibleParams.propBrushSettings = 
                serializedObject.FindProperty("VBrushSettings");
            visibleParams.propEdgeOutlineOn = 
                serializedObject.FindProperty("VOutlineOn");
            visibleParams.propEdgeOutlineOpen =
                serializedObject.FindProperty("VOutlineOpen");
            visibleParams.propEdgeOutlineMergeGroups =
                serializedObject.FindProperty("VOutlineMergeGroups");
            visibleParams.propEdgeOutlineSpecificOn = 
                serializedObject.FindProperty("VOutlineSpecificOn");
            visibleParams.propEdgeOutline = 
                serializedObject.FindProperty("VOutline");
            visibleParams.propEdgeObjectOn = 
                serializedObject.FindProperty("VObjectOn");
            visibleParams.propEdgeObjectOpen =
                serializedObject.FindProperty("VObjectOpen");
            visibleParams.propEdgeObjectSpecificOn = 
                serializedObject.FindProperty("VObjectSpecificOn");
            visibleParams.propEdgeObject = 
                serializedObject.FindProperty("VObject");
            visibleParams.propEdgeIntersectionOn = 
                serializedObject.FindProperty("VIntersectionOn");
            visibleParams.propEdgeIntersectionSelf =
                serializedObject.FindProperty("VIntersectionSelf");
            visibleParams.propEdgeIntersectionSpecificOn = 
                serializedObject.FindProperty("VIntersectionSpecificOn");
            visibleParams.propEdgeIntersection = 
                serializedObject.FindProperty("VIntersection");
            visibleParams.propEdgeSmoothOn = 
                serializedObject.FindProperty("VSmoothOn");
            visibleParams.propEdgeSmoothSpecificOn = 
                serializedObject.FindProperty("VSmoothSpecificOn");
            visibleParams.propEdgeSmooth = 
                serializedObject.FindProperty("VSmooth");
            visibleParams.propEdgeMaterialOn = 
                serializedObject.FindProperty("VMaterialOn");
            visibleParams.propEdgeMaterialSpecificOn = 
                serializedObject.FindProperty("VMaterialSpecificOn");
            visibleParams.propEdgeMaterial = 
                serializedObject.FindProperty("VMaterial");
            visibleParams.propEdgeNormalAngleOn = 
                serializedObject.FindProperty("VNormalAngleOn");
            visibleParams.propEdgeNormalAngleSpecificOn = 
                serializedObject.FindProperty("VNormalAngleSpecificOn");
            visibleParams.propEdgeNormalAngle = 
                serializedObject.FindProperty("VNormalAngle");
            visibleParams.propEdgeNormalAngleMin = 
                serializedObject.FindProperty("VNormalAngleMin");
            visibleParams.propEdgeNormalAngleMax = 
                serializedObject.FindProperty("VNormalAngleMax");
            visibleParams.propEdgeWireframeOn = 
                serializedObject.FindProperty("VWireframeOn");
            visibleParams.propEdgeWireframeSpecificOn = 
                serializedObject.FindProperty("VWireframeSpecificOn");
            visibleParams.propEdgeWireframe = 
                serializedObject.FindProperty("VWireframe");
            visibleParams.propSizeReductionOn = 
                serializedObject.FindProperty("VSizeReductionOn");
            visibleParams.propSizeReduction = 
                serializedObject.FindProperty("VSizeReduction");
            visibleParams.propAlphaReductionOn = 
                serializedObject.FindProperty("VAlphaReductionOn");
            visibleParams.propAlphaReduction = 
                serializedObject.FindProperty("VAlphaReduction");

            hiddenParams.propBrushSettings = 
                serializedObject.FindProperty("HBrushSettings");
            hiddenParams.propEdgeOutlineOn = 
                serializedObject.FindProperty("HOutlineOn");
            hiddenParams.propEdgeOutlineOpen =
                serializedObject.FindProperty("HOutlineOpen");
            hiddenParams.propEdgeOutlineMergeGroups =
                serializedObject.FindProperty("HOutlineMergeGroups");
            hiddenParams.propEdgeOutlineSpecificOn = 
                serializedObject.FindProperty("HOutlineSpecificOn");
            hiddenParams.propEdgeOutline = 
                serializedObject.FindProperty("HOutline");
            hiddenParams.propEdgeObjectOn = 
                serializedObject.FindProperty("HObjectOn");
            hiddenParams.propEdgeObjectOpen =
                serializedObject.FindProperty("HObjectOpen");
            hiddenParams.propEdgeObjectSpecificOn = 
                serializedObject.FindProperty("HObjectSpecificOn");
            hiddenParams.propEdgeObject = 
                serializedObject.FindProperty("HObject");
            hiddenParams.propEdgeIntersectionOn =
                serializedObject.FindProperty("HIntersectionOn");
            hiddenParams.propEdgeIntersectionSelf =
                serializedObject.FindProperty("HIntersectionSelf");
            hiddenParams.propEdgeIntersectionSpecificOn =
                serializedObject.FindProperty("HIntersectionSpecificOn");
            hiddenParams.propEdgeIntersection =
                serializedObject.FindProperty("HIntersection");
            hiddenParams.propEdgeSmoothOn =
                serializedObject.FindProperty("HSmoothOn");
            hiddenParams.propEdgeSmoothSpecificOn =
                serializedObject.FindProperty("HSmoothSpecificOn");
            hiddenParams.propEdgeSmooth = 
                serializedObject.FindProperty("HSmooth");
            hiddenParams.propEdgeMaterialOn =
                serializedObject.FindProperty("HMaterialOn");
            hiddenParams.propEdgeMaterialSpecificOn =
                serializedObject.FindProperty("HMaterialSpecificOn");
            hiddenParams.propEdgeMaterial =
                serializedObject.FindProperty("HMaterial");
            hiddenParams.propEdgeNormalAngleOn =
                serializedObject.FindProperty("HNormalAngleOn");
            hiddenParams.propEdgeNormalAngleSpecificOn =
                serializedObject.FindProperty("HNormalAngleSpecificOn");
            hiddenParams.propEdgeNormalAngle = 
                serializedObject.FindProperty("HNormalAngle");
            hiddenParams.propEdgeNormalAngleMin =
                serializedObject.FindProperty("HNormalAngleMin");
            hiddenParams.propEdgeNormalAngleMax =
                serializedObject.FindProperty("HNormalAngleMax");
            hiddenParams.propEdgeWireframeOn = 
                serializedObject.FindProperty("HWireframeOn");
            hiddenParams.propEdgeWireframeSpecificOn =
                serializedObject.FindProperty("HWireframeSpecificOn");
            hiddenParams.propEdgeWireframe =
                serializedObject.FindProperty("HWireframe");
            hiddenParams.propSizeReductionOn = 
                serializedObject.FindProperty("HSizeReductionOn");
            hiddenParams.propSizeReduction = 
                serializedObject.FindProperty("HSizeReduction");
            hiddenParams.propAlphaReductionOn = 
                serializedObject.FindProperty("HAlphaReductionOn");
            hiddenParams.propAlphaReduction = 
                serializedObject.FindProperty("HAlphaReduction");

            propWeldsEdges = serializedObject.FindProperty("WeldsEdges");
            propMaskHiddenLines = serializedObject.FindProperty("MaskHiddenLines");


            VisibilitySelectionChanged();

        }

        public override void OnInspectorGUI()
        {
            var pclLineSetNode = target as LineSetNode;
            serializedObject.Update();
            
            if (Event.current.type == EventType.Repaint)
            {
                reorderableObjectsList.OnRepaint();
                reorderableMaterialsList.OnRepaint();
            }

            CreateLineSetGui();

            CreateBrushSectionGui(pclLineSetNode);
            CreateEdgeGui(pclLineSetNode);
            CreateEdgeCommonParamsGui();
            CreateReductionGui(pclLineSetNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineSetノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line Set Node", priority = 20)]
        public static void OpenLineSetNode(MenuCommand menuCommand)
        {
            var newLineSet = EditorCommons.CreateNodeObjectFromMenu<LineSetNode>(menuCommand, typeof(LineNode), "LineSets");

            // BrushSettingsの追加
            var lineSetNodeComponent = newLineSet.GetComponent<LineSetNode>();


            var newVBrushSettings = Pcl4EditorUtilities.CreateNodeObject<BrushSettingsNode>(newLineSet.transform, " Visible ");
            var newHBrushSettings = Pcl4EditorUtilities.CreateNodeObject<BrushSettingsNode>(newLineSet.transform, " Hidden ");

            lineSetNodeComponent.VBrushSettings = newVBrushSettings;
            lineSetNodeComponent.HBrushSettings = newHBrushSettings;


            // BrushDetailの追加
            var vBrushSettingsNodeComponent = newVBrushSettings.GetComponent<BrushSettingsNode>();
            var hBrushSettingsNodeComponent = newHBrushSettings.GetComponent<BrushSettingsNode>();

            var newVBrushDetail = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newVBrushSettings.transform, " Visible ");
            var newHBrushDetail = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newHBrushSettings.transform, " Hidden ");

            vBrushSettingsNodeComponent.BrushDetail = newVBrushDetail;
            hBrushSettingsNodeComponent.BrushDetail = newHBrushDetail;

        }
    }

}
//#define 

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(LineNode))]
    public class LineNodeEditor : Editor
    {

        // foldout
        public bool foldoutBasicParam = true;
        public bool foldoutLineSize = true;
        public bool foldoutOthers = true;
        public bool foldoutLineSet = true;
        public bool foldoutBrush = true;
        public bool foldoutEdge = true;
        public bool foldoutReduction = true;

        public LineSetNode.LineType currentLineType = LineSetNode.LineType.Visible;

        private SerializedProperty propLineSets;
        private SerializedProperty propObjects;
        private SerializedProperty propMaterials;
        private SerializedProperty propLineSize;
        private SerializedProperty propOverSampling;
        private SerializedProperty propAntialiasing;
        private SerializedProperty propOffScreenDistance;
        private SerializedProperty propRandomSeed;
        private SerializedProperty propOutputToRenderElementsOnly;

        private SerializedProperty propId;

        PencilReorderableList reorderableSetList;

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
            public SerializedProperty propEdgeIntersectionOn;
            public SerializedProperty propEdgeIntersectionSelf;
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

        [SerializeField]
        private SerializedObject serializedLineSetParams;
        [SerializeField]
        private SerializedObject serializedBrushSettingsVisibleParams;
        [SerializeField]
        private SerializedObject serializedBrushSettingsHiddenParams;
        [SerializeField]
        private SerializedObject serializedBrushDetailVisibleParams;
        [SerializeField]
        private SerializedObject serializedBrushDetailHiddenParams;

        private GUIStyle listBoxStyle;
        private GUIStyle inListBoxStyle;
        private GUIStyle indent1Style;

        private PencilReorderableList _reorderableObjects = null;
        private PencilReorderableList _reorderableMaterials = null;
        private GameObject _selectedLineSet = null;
        int objectPickerID = -1;
        int materialPickerID = -1;

        // Line SetのObjects / Materialsリストの更新するため、定期的な再描画を行う
        public override bool RequiresConstantRepaint() { return true;  }


        private static List<T> GetList<T>(NodeBase node, string varName)
            where T : UnityEngine.Object
        {
            var lineSetNode = node as LineSetNode;

            var newList = new List<T>();

            switch (varName)
            {
                case "Objects":
                    if (lineSetNode != null)
                        newList = lineSetNode.Objects
                            .Select(x => x as T)
                            .ToList();
                    break;

                case "Materials":
                    if (lineSetNode != null)
                        newList = lineSetNode.Materials
                            .Select(x => x as T)
                            .ToList();
                    break;

                default:
                    break;
            }

            return newList;
        }


        /// <summary>
        /// ObjectまたはMaterialの追加、削除を行うリストを作成
        /// </summary>
        /// <typeparam name="T">リストの型</typeparam>
        /// <param name="prop">リストのプロパティ</param>
        /// <param name="reorderableList">リスト</param>
        /// <param name="pickerId">オブジェクトピッカーのID</param>
        /// <param name="style">リストのスタイル</param>
        private void CreateDragDropList<T>(SerializedProperty prop,
                         PencilReorderableList reorderableList,
                         int pickerId,
                         GUIStyle style)
            where T : UnityEngine.Object
        {
            var ev = Event.current;
            var lineNode = target as LineNode;

            var isGameObject = typeof(T) == typeof(GameObject);

            var labelText = _selectedLineSet.name;
            labelText += isGameObject ? " -> Object List" :
                                    " -> " + typeof(T).Name + " List";

            EditorGUILayout.LabelField(labelText);

            serializedLineSetParams.Update();


            using(var verticalLayout = new EditorGUILayout.VerticalScope(style))
            {
                // D&D
                Common.DragAndDropObject<T, LineSetNode>(lineNode,
                                                     lineNode.LineSets,
                                                     prop,
                                                     verticalLayout.rect,
                                                     GetList<T>,
                                                     true);

                // 表示
                reorderableList.HandleInputEventAndLayoutList();
            }

            serializedLineSetParams.ApplyModifiedProperties();

        }

        /// <summary>
        /// BasicParametersのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        private void CreateBasicParametersGui(NodeBase lineNode)
        {
            var ev = Event.current;
            
            foldoutBasicParam =
                EditorGUILayout.Foldout(foldoutBasicParam, "Basic Parameters");
            if (!foldoutBasicParam)
            {
                return;
            }

            ++EditorGUI.indentLevel;


            // Line Set List
            EditorGUILayout.LabelField("Line Set List");

            var style = new GUIStyle {margin = new RectOffset(30, 8, 0, 4)};

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<LineSetNode>(lineNode, propLineSets, verticalLayout.rect);

            using (verticalLayout)
            {
                reorderableSetList.HandleInputEventAndLayoutList();
            }

            // LineSetが選択されていない場合は表示なし
            if (ev.type == EventType.Used)
            {
                return;
            }
            else if (ev.type == EventType.Layout)
            {
                LineSetSelectionChanged();
                
                if (serializedLineSetParams == null ||
                    serializedLineSetParams.targetObject == null ||
                    reorderableSetList.index == -1)
                {
                    _selectedLineSet = null;
                    return;
                }
                
                var propLineSet = propLineSets.GetArrayElementAtIndex(reorderableSetList.index);
                _selectedLineSet = propLineSet.objectReferenceValue as GameObject;
            }
            else
            {
                if (_selectedLineSet == null)
                {
                    --EditorGUI.indentLevel;
                    return;
                }
            }
            
            
            serializedLineSetParams.Update();

            // Objects
            CreateDragDropList<GameObject>(propObjects,
                                 _reorderableObjects,
                                 objectPickerID,
                                 style);


            // Materials
            CreateDragDropList<Material>(propMaterials,
                               _reorderableMaterials,
                               materialPickerID,
                               style);



            serializedLineSetParams.ApplyModifiedProperties();

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// LineSizeのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        private void CreateLineSizeGui(LineNode lineNode)
        {
            foldoutLineSize =
                EditorGUILayout.Foldout(foldoutLineSize, "Line Size");
            if (!foldoutLineSize)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // enumの型を直す
            var lineSize = (LineNode.LineSize)Enum
                           .GetValues(typeof(LineNode.LineSize))
                           .GetValue(propLineSize.enumValueIndex);

            propLineSize.enumValueIndex = (int)(LineNode.LineSize)EditorGUILayout.EnumPopup("Line Size", lineSize);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }


        /// <summary>
        /// OthersのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        private void CreateOthersGui(LineNode lineNode)
        {
            foldoutOthers =
                EditorGUILayout.Foldout(foldoutOthers, "Others");
            if (!foldoutOthers)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Output to Render Elements only
            propOutputToRenderElementsOnly.boolValue =
                EditorGUILayout.ToggleLeft("Output to Render Elements only", propOutputToRenderElementsOnly.boolValue);

            // Over Sampling
            propOverSampling.intValue =
                EditorGUILayout.IntSlider("Over Sampling",
                                          propOverSampling.intValue,
                                          1, 4);

            // Anti-aliasing
            propAntialiasing.floatValue =
                EditorGUILayout.Slider("Antialiasing",
                                       propAntialiasing.floatValue,
                                       0.0f, 2.0f);

            // Off Screen Distance
            propOffScreenDistance.floatValue =
                EditorGUILayout.Slider("Off Screen Distance",
                                       propOffScreenDistance.floatValue,
                                       0.0f, 1000.0f);

            // Random Seed
            propRandomSeed.intValue =
                EditorGUILayout.IntSlider("Random Seed",
                                          propRandomSeed.intValue,
                                          0, 65535);

            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;

        }


        /// <summary>
        /// LineSetのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        private void CreateLineSetGui(LineNode lineNode)
        {
            if (_selectedLineSet == null)
            {
                return;
            }
            
            var lineSetNode = _selectedLineSet.GetComponent<LineSetNode>();
               
            foldoutLineSet =
                EditorGUILayout.Foldout(foldoutLineSet, _selectedLineSet.name);
            if (!foldoutLineSet)
            {
                return;
            }

            
            serializedLineSetParams.Update();

            ++EditorGUI.indentLevel;

            // ID
            propId.intValue = EditorGUILayout.IntSlider("ID", propId.intValue, 1, 8);

            // LineType (Visible or Hidden)
            // Tabの代わりにボタンを置く
            using (new EditorGUILayout.HorizontalScope(indent1Style))
            {
                using (var toggleChange = new EditorGUI.ChangeCheckScope())
                {
                    var isVisible = GUILayout.Toggle(currentLineType == LineSetNode.LineType.Visible,
                        "Visible",
                        BuiltInResources.VisibleToggleButtonStyle);
                    if (toggleChange.changed && isVisible)
                    {
                        currentLineType = LineSetNode.LineType.Visible;
                        Repaint();
                    }
                }

                using (var toggleChange = new EditorGUI.ChangeCheckScope())
                {
                    var isHidden = GUILayout.Toggle(currentLineType == LineSetNode.LineType.Hidden,
                            "Hidden",
                            BuiltInResources.HiddenToggleButtonStyle);
                    if (toggleChange.changed && isHidden)
                    {
                        currentLineType = LineSetNode.LineType.Hidden;
                        Repaint();
                    }
                    
                }
                
                ChangeDisplayingBrushSettings(lineSetNode, currentLineType);

            }

            CreateBrushSectionGui(lineSetNode);
            CreateEdgeGui(lineSetNode);
            CreateEdgeCommonParamsGui();
            CreateReductionGui(lineSetNode);

            --EditorGUI.indentLevel;

            serializedLineSetParams.ApplyModifiedProperties();
        }

        /// <summary>
        /// LineSetのBrush項目のGUIを作成
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
                serializedLineSetParams,
                currentParams.propBrushSettings,
                (nodeObject) =>
                {
                    LineSetSelectionChanged();
                }
              );

            var bsObj = currentParams.propBrushSettings.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bsObj == null))
            {
                CreateBrushSettingsGui((GameObject)currentParams.propBrushSettings.objectReferenceValue);
            }

            EditorGUILayout.Separator();
        }

        /// <summary>
        /// LineSetのBrushSettingsに関する部分のGUIの作成
        /// </summary>
        /// <param name="lineSetObject">LineSetGameObject</param>
        private void CreateBrushSettingsGui(GameObject lineSetObject)
        {
            var brushSettingsNode = lineSetObject ? lineSetObject.GetComponent<BrushSettingsNode>() : null;

            if(lineSetObject == null
               || brushSettingsNode == null)
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

                CreateBrushDetailGui(null);

                --EditorGUI.indentLevel;
                return;
            }

           
          
            if (currentLineType == LineSetNode.LineType.Visible
                && serializedBrushSettingsVisibleParams != null)
            {
                serializedBrushSettingsVisibleParams.Update();
            }
            else if (serializedBrushSettingsHiddenParams != null)
            {
                serializedBrushSettingsHiddenParams.Update();
            }
          

            // Brush Detail
            
            EditorGUICustomLayout.PencilNodeField(
                "Brush Detail",
                typeof(BrushDetailNode),
                currentLineType == LineSetNode.LineType.Visible ? 
                    serializedBrushSettingsVisibleParams : 
                    serializedBrushSettingsHiddenParams,
                currentParams.propBrushDetail,
                (nodeObject) =>
                {
                    // LineNodeEditorが参照しているBrushDetailsNodeの値を再設定
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


            //// BlendMode
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
            currentParams.propMapOpacity.floatValue = EditorGUILayout.Slider(
                "Map Opacity",
                currentParams.propMapOpacity.floatValue,
                0.0f, 1.0f);

            // Size
            currentParams.propSize.floatValue = EditorGUILayout.Slider(
                "Size",
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
                if (serializedBrushSettingsVisibleParams != null)
                    serializedBrushSettingsVisibleParams.ApplyModifiedProperties();
            }
            else
            {
                if (serializedBrushSettingsHiddenParams != null)
                    serializedBrushSettingsHiddenParams.ApplyModifiedProperties();
            }

            // BrushDetailParams
            var bdObj = currentParams.propBrushDetail.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bdObj == null))
            {
                CreateBrushDetailGui((GameObject)currentParams.propBrushDetail.objectReferenceValue);
            }

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// LineSetのBrushDetailに関する部分のGUIの作成
        /// </summary>
        /// <param name="brushDetailObject">BrushDetailコンポーネントを持っているGameObject</param>
        private void CreateBrushDetailGui(GameObject brushDetailObject)
        {
            Action noBrushDetails = () =>
            {
                EditorGUILayout.Slider("Stretch", 0, -1.0f, 1.0f);
                EditorGUILayout.Slider("Angle", 0, -3600.0f, 3600.0f);
            };

            if (brushDetailObject == null)
            {
                noBrushDetails();
                return;
            }

            var brushDetailNode = brushDetailObject.GetComponent<BrushDetailNode>();
            if (brushDetailNode == null)
            {
                noBrushDetails();
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
        /// 単一のエッジの設定に関連するプロパティの集合
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
        /// LineSetのEdge項目のGUIを作成
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

            var beforeSpecificOn = false;
            GameObject beforeObj;

            Action<string, EdgeProps> createEdgeGroupGui =
                (label, props) =>
                {
                    beforeSpecificOn = props.SpecificOn.boolValue;
                    beforeObj = props.BrushSettings.objectReferenceValue as GameObject;

                    CreateIndividualEdgeGui(lineSetNode, label, props);

                    if (beforeSpecificOn != props.SpecificOn.boolValue &&
                        beforeSpecificOn == false &&
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

            propMaskHiddenLines.boolValue = EditorGUILayout.Toggle("On",
                    propMaskHiddenLines.boolValue);
            --EditorGUI.indentLevel;
            
        }
        

        /// <summary>
        /// LineSetのReduction項目のGUIを追加
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
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

            Action<string, SerializedProperty, SerializedProperty> createReductionGroupGui =
                (label, propOn, propReduction) =>
                {
                    before = propOn.boolValue;
                    beforeObj = propReduction.objectReferenceValue as GameObject;
                    CreateIndividualReductionGui(lineSetNode, label,
                                        propOn,
                                        propReduction);

                    if (before != propOn.boolValue &&
                        before == false &&
                        !propReduction.objectReferenceValue)
                    {
                        var newReduction = Pcl4EditorUtilities.CreateNodeObject<ReductionSettingsNode>(lineSetNode.transform, suffixName);
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
            createReductionGroupGui("Size Reduction",
                           currentParams.propSizeReductionOn,
                           currentParams.propSizeReduction);

            // Alpha
            createReductionGroupGui("Alpha Reduction",
                           currentParams.propAlphaReductionOn,
                           currentParams.propAlphaReduction);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;
        }


        /// <summary>
        /// 現在のLineTypeに応じた設定に切替を行う
        /// </summary>
        /// <param name="lineSetNode">LineSetNode</param>
        /// <param name="lineType">現在のLineType</param>
        private void ChangeDisplayingBrushSettings(LineSetNode lineSetNode, LineSetNode.LineType lineType)
        {
            switch (lineType)
            {
                case LineSetNode.LineType.Visible:
                    currentParams = visibleParams;
                    break;

                case LineSetNode.LineType.Hidden:
                    currentParams = hiddenParams;
                    break;
            }

        }

        /// <summary>
        /// EdgeのGUIの作成
        /// </summary>
        /// <param name="lineSetNode">現在のLineSetNode</param>
        /// <param name="label">ラベルの文字列</param>
        /// <param name="edgeProps">GUIの表示に使用するプロパティの集合</param>
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
                        EditorGUILayout.Toggle("Merge Groups", edgeProps.MergeGroups.boolValue);
                }
                
                // Specific On
                edgeProps.SpecificOn.boolValue = EditorGUILayout.Toggle("Specific On", edgeProps.SpecificOn.boolValue);


                using (new EditorGUI.DisabledGroupScope(!edgeProps.SpecificOn.boolValue))
                {
                    // BrushSettings
                    EditorGUICustomLayout.PencilNodeField(
                        "",
                        typeof(BrushSettingsNode),
                        serializedLineSetParams,
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
                    serializedLineSetParams,
                    propReductionSettings,
                    selectedObject => {});

            }

            --EditorGUI.indentLevel;

        }


        private GameObject _oldLineSet = null;

        /// <summary>
        /// LineSetの選択が変更されたら行う処理
        /// </summary>
        public void LineSetSelectionChanged()
        {
            // Set Line Set Properties
            if (reorderableSetList.index < 0 ||
               reorderableSetList.index >= propLineSets.arraySize)
            {
                return;
            }

            var currentLineSet = propLineSets.GetArrayElementAtIndex(reorderableSetList.index);
            if ( currentLineSet == null)
            {
                return;
            }

            var lineSet = currentLineSet.objectReferenceValue as GameObject;

            if (_oldLineSet == lineSet)
            {
                return;
            }

            _oldLineSet = lineSet;

            serializedLineSetParams =
                lineSet ?
                new SerializedObject(lineSet.GetComponent<LineSetNode>()) :
                null;

            if (serializedLineSetParams != null)
            {
                propId = serializedLineSetParams.FindProperty("Id");
                propObjects = serializedLineSetParams.FindProperty("Objects");
                propMaterials = serializedLineSetParams.FindProperty("Materials");

                visibleParams.propBrushSettings =
                    serializedLineSetParams.FindProperty("VBrushSettings");
                visibleParams.propEdgeOutlineOn =
                    serializedLineSetParams.FindProperty("VOutlineOn");
                visibleParams.propEdgeOutlineOpen =
                    serializedLineSetParams.FindProperty("VOutlineOpen");
                visibleParams.propEdgeOutlineMergeGroups =
                    serializedLineSetParams.FindProperty("VOutlineMergeGroups");
                visibleParams.propEdgeOutlineSpecificOn =
                    serializedLineSetParams.FindProperty("VOutlineSpecificOn");
                visibleParams.propEdgeOutline =
                    serializedLineSetParams.FindProperty("VOutline");
                visibleParams.propEdgeObjectOn =
                    serializedLineSetParams.FindProperty("VObjectOn");
                visibleParams.propEdgeObjectOpen =
                    serializedLineSetParams.FindProperty("VObjectOpen");
                visibleParams.propEdgeObjectSpecificOn =
                    serializedLineSetParams.FindProperty("VObjectSpecificOn");
                visibleParams.propEdgeObject =
                    serializedLineSetParams.FindProperty("VObject");
                visibleParams.propEdgeIntersectionOn =
                    serializedLineSetParams.FindProperty("VIntersectionOn");
                visibleParams.propEdgeIntersectionSelf =
                    serializedLineSetParams.FindProperty("VIntersectionSelf");
                visibleParams.propEdgeIntersectionSpecificOn =
                    serializedLineSetParams.FindProperty("VIntersectionSpecificOn");
                visibleParams.propEdgeIntersection =
                    serializedLineSetParams.FindProperty("VIntersection");
                visibleParams.propEdgeSmoothOn =
                    serializedLineSetParams.FindProperty("VSmoothOn");
                visibleParams.propEdgeSmoothSpecificOn =
                    serializedLineSetParams.FindProperty("VSmoothSpecificOn");
                visibleParams.propEdgeSmooth =
                    serializedLineSetParams.FindProperty("VSmooth");
                visibleParams.propEdgeMaterialOn =
                    serializedLineSetParams.FindProperty("VMaterialOn");
                visibleParams.propEdgeMaterialSpecificOn =
                    serializedLineSetParams.FindProperty("VMaterialSpecificOn");
                visibleParams.propEdgeMaterial =
                    serializedLineSetParams.FindProperty("VMaterial");
                visibleParams.propEdgeNormalAngleOn =
                    serializedLineSetParams.FindProperty("VNormalAngleOn");
                visibleParams.propEdgeNormalAngleSpecificOn =
                    serializedLineSetParams.FindProperty("VNormalAngleSpecificOn");
                visibleParams.propEdgeNormalAngle =
                    serializedLineSetParams.FindProperty("VNormalAngle");
                visibleParams.propEdgeNormalAngleMin =
                    serializedLineSetParams.FindProperty("VNormalAngleMin");
                visibleParams.propEdgeNormalAngleMax =
                    serializedLineSetParams.FindProperty("VNormalAngleMax");
                visibleParams.propEdgeWireframeOn =
                    serializedLineSetParams.FindProperty("VWireframeOn");
                visibleParams.propEdgeWireframeSpecificOn =
                    serializedLineSetParams.FindProperty("VWireframeSpecificOn");
                visibleParams.propEdgeWireframe =
                    serializedLineSetParams.FindProperty("VWireframe");
                visibleParams.propSizeReductionOn =
                    serializedLineSetParams.FindProperty("VSizeReductionOn");
                visibleParams.propSizeReduction =
                    serializedLineSetParams.FindProperty("VSizeReduction");
                visibleParams.propAlphaReductionOn =
                    serializedLineSetParams.FindProperty("VAlphaReductionOn");
                visibleParams.propAlphaReduction =
                    serializedLineSetParams.FindProperty("VAlphaReduction");

                hiddenParams.propBrushSettings =
                    serializedLineSetParams.FindProperty("HBrushSettings");
                hiddenParams.propEdgeOutlineOn =
                    serializedLineSetParams.FindProperty("HOutlineOn");
                hiddenParams.propEdgeOutlineOpen =
                    serializedLineSetParams.FindProperty("HOutlineOpen");
                hiddenParams.propEdgeOutlineMergeGroups =
                    serializedLineSetParams.FindProperty("HOutlineMergeGroups");
                hiddenParams.propEdgeOutlineSpecificOn =
                    serializedLineSetParams.FindProperty("HOutlineSpecificOn");
                hiddenParams.propEdgeOutline =
                    serializedLineSetParams.FindProperty("HOutline");
                hiddenParams.propEdgeObjectOn =
                    serializedLineSetParams.FindProperty("HObjectOn");
                hiddenParams.propEdgeObjectOpen =
                    serializedLineSetParams.FindProperty("HObjectOpen");
                hiddenParams.propEdgeObjectSpecificOn =
                    serializedLineSetParams.FindProperty("HObjectSpecificOn");
                hiddenParams.propEdgeObject =
                    serializedLineSetParams.FindProperty("HObject");
                hiddenParams.propEdgeIntersectionOn =
                    serializedLineSetParams.FindProperty("HIntersectionOn");
                hiddenParams.propEdgeIntersectionSelf =
                    serializedLineSetParams.FindProperty("HIntersectionSelf");
                hiddenParams.propEdgeIntersectionSpecificOn =
                    serializedLineSetParams.FindProperty("HIntersectionSpecificOn");
                hiddenParams.propEdgeIntersection =
                    serializedLineSetParams.FindProperty("HIntersection");
                hiddenParams.propEdgeSmoothOn =
                    serializedLineSetParams.FindProperty("HSmoothOn");
                hiddenParams.propEdgeSmoothSpecificOn =
                    serializedLineSetParams.FindProperty("HSmoothSpecificOn");
                hiddenParams.propEdgeSmooth =
                    serializedLineSetParams.FindProperty("HSmooth");
                hiddenParams.propEdgeMaterialOn =
                    serializedLineSetParams.FindProperty("HMaterialOn");
                hiddenParams.propEdgeMaterialSpecificOn =
                    serializedLineSetParams.FindProperty("HMaterialSpecificOn");
                hiddenParams.propEdgeMaterial =
                    serializedLineSetParams.FindProperty("HMaterial");
                hiddenParams.propEdgeNormalAngleOn =
                    serializedLineSetParams.FindProperty("HNormalAngleOn");
                hiddenParams.propEdgeNormalAngleSpecificOn =
                    serializedLineSetParams.FindProperty("HNormalAngleSpecificOn");
                hiddenParams.propEdgeNormalAngle =
                    serializedLineSetParams.FindProperty("HNormalAngle");
                hiddenParams.propEdgeNormalAngleMin =
                    serializedLineSetParams.FindProperty("HNormalAngleMin");
                hiddenParams.propEdgeNormalAngleMax =
                    serializedLineSetParams.FindProperty("HNormalAngleMax");
                hiddenParams.propEdgeWireframeOn =
                    serializedLineSetParams.FindProperty("HWireframeOn");
                hiddenParams.propEdgeWireframeSpecificOn =
                    serializedLineSetParams.FindProperty("HWireframeSpecificOn");
                hiddenParams.propEdgeWireframe =
                    serializedLineSetParams.FindProperty("HWireframe");
                hiddenParams.propSizeReductionOn =
                    serializedLineSetParams.FindProperty("HSizeReductionOn");
                hiddenParams.propSizeReduction =
                    serializedLineSetParams.FindProperty("HSizeReduction");
                hiddenParams.propAlphaReductionOn =
                    serializedLineSetParams.FindProperty("HAlphaReductionOn");
                hiddenParams.propAlphaReduction =
                    serializedLineSetParams.FindProperty("HAlphaReduction");

                propWeldsEdges = serializedLineSetParams.FindProperty("WeldsEdges");
                propMaskHiddenLines = serializedLineSetParams.FindProperty("MaskHiddenLines");


                
                // ---------- Object List ----------
                _reorderableObjects = Common.CreateObjectList(
                    serializedLineSetParams,
                    propObjects,
                    ((LineNode)target).LineSets
                        .Select(x => x.GetComponent<LineSetNode>())
                        .Where(x => x != null)
                        .SelectMany(x => x.Objects),
                    selectedObjects => 
                    {
                        serializedLineSetParams.Update();
                        propObjects.AppendObjects(selectedObjects);
                        serializedLineSetParams.ApplyModifiedProperties();
                        _reorderableObjects.index = _reorderableObjects.count - 1;
                    });


                // ---------- Material List ----------
                _reorderableMaterials = Common.CreateMaterialList(
                    serializedLineSetParams,
                    propMaterials,
                    ((LineNode)target).LineSets
                        .Select(x => x.GetComponent<LineSetNode>())
                        .Where(x => x != null)
                        .SelectMany(x => x.Materials),
                    selectedMaterials => 
                    {
                        serializedLineSetParams.Update();
                        propMaterials.AppendObjects(selectedMaterials);
                        serializedLineSetParams.ApplyModifiedProperties();
                        _reorderableMaterials.index = _reorderableMaterials.count - 1;
                    });

            }

            // Set Brush Settings Properties
            var brushSettings = visibleParams.propBrushSettings != null ?
                visibleParams.propBrushSettings.objectReferenceValue as GameObject :
                null;

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


            brushSettings = hiddenParams.propBrushSettings != null ?
                            hiddenParams.propBrushSettings.objectReferenceValue as GameObject :
                            null;

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

        private void OnDisable()
        {
            var lineNode = target as LineNode;
            if (lineNode != null) lineNode.SelectedLineSet = _selectedLineSet;
        }


        private void OnEnable()
        {
            var lineNode = target as LineNode;

            if (lineNode != null)
            {
                _selectedLineSet = lineNode.SelectedLineSet;

                // Styleの作成
                if (indent1Style == null)
                {
                    indent1Style = new GUIStyle
                    {
                        border = new RectOffset(1, 1, 1, 1),
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(30, 0, 0, 0)
                    };
                }

                if (listBoxStyle == null)
                {
                    listBoxStyle = new GUIStyle("box")
                    {
                        border = new RectOffset(1, 1, 1, 1),
                        padding = new RectOffset(1, 1, 1, 1),
                        margin = new RectOffset(30, 0, 0, 0),
                        stretchWidth = true,
                        stretchHeight = false,
                        fixedHeight = 128
                    };
                }

                if (inListBoxStyle == null)
                {
                    inListBoxStyle = new GUIStyle
                    {
                        border = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                // SerializePropertyをセット
                propLineSets = serializedObject.FindProperty("LineSets");
                propLineSize = serializedObject.FindProperty("LineSizeType");
                propOverSampling = serializedObject.FindProperty("OverSampling");
                propAntialiasing = serializedObject.FindProperty("Antialiasing");
                propOffScreenDistance = serializedObject.FindProperty("OffscreenDistance");
                propRandomSeed = serializedObject.FindProperty("RandomSeed");
                propOutputToRenderElementsOnly = serializedObject.FindProperty("OutputToRenderElementsOnly");


                // Line Set List
                reorderableSetList =
                    Common.CreateReorderableNodeList<LineSetNode>(serializedObject,
                        propLineSets,
                        lineNode);
                
                reorderableSetList.IsSelectionLimitOneOrMore = true;

                // Elementを追加するコールバック
                reorderableSetList.onAddCallback = (list) =>
                {
                    // LineSetの追加
                    var pencilList = (PencilReorderableList)list;
                    var newLineSet = Pcl4EditorUtilities.CreateNodeObject<LineSetNode>(lineNode.gameObject.transform);

                    Undo.RegisterCreatedObjectUndo(newLineSet, "Create Line Set Node");

                    pencilList.index = propLineSets.arraySize++;
                    
                    var propLineSet = propLineSets.GetArrayElementAtIndex(pencilList.index);
                    propLineSet.objectReferenceValue = newLineSet;


                    // BrushSettingsの追加
                    var lineSetNode = newLineSet.GetComponent<LineSetNode>();

                    var newVBrushSettings = Pcl4EditorUtilities.CreateNodeObject<BrushSettingsNode>(newLineSet.transform, " Visible ");

                    var newHBrushSettings = Pcl4EditorUtilities.CreateNodeObject<BrushSettingsNode>(newLineSet.transform, " Hidden ");

                    lineSetNode.VBrushSettings = newVBrushSettings;
                    lineSetNode.HBrushSettings = newHBrushSettings;


                    // BrushDetailの追加
                    var vBrushSettingsNodeComponent = newVBrushSettings.GetComponent<BrushSettingsNode>();
                    var hBrushSettingsNodeComponent = newHBrushSettings.GetComponent<BrushSettingsNode>();

                    var newVBrushDetail = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newVBrushSettings.transform, " Visible ");
                    var newHBrushDetail = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newHBrushSettings.transform, " Hidden ");

                    vBrushSettingsNodeComponent.BrushDetail = newVBrushDetail;
                    hBrushSettingsNodeComponent.BrushDetail = newHBrushDetail;

                    _selectedLineSet = (GameObject)propLineSet.objectReferenceValue;
                    LineSetSelectionChanged();
                    pencilList.GrabKeyboardFocus();
                };
            }


            // Elementを削除するコールバック
            reorderableSetList.onRemoveCallback += (list) =>
            {
                LineSetSelectionChanged();
            };

            // Elementの入れ替えを行った際に呼ばれるコールバック
            reorderableSetList.onReorderCallback += (list) =>
            {
                LineSetSelectionChanged();
            };

            // 選択状態変更
            reorderableSetList.OnSelectionChangeCallback += (list) =>
            {
                LineSetSelectionChanged();
            };


            if (propLineSets.arraySize == 0)
            {
                reorderableSetList.index = 0;
            }

            if (reorderableSetList.index >= propLineSets.arraySize)
            {
                reorderableSetList.index = propLineSets.arraySize - 1;
            }

            // 前回のLineSetの選択を読み込み
            var count = propLineSets.arraySize;
            var isFound = false;
            for (var i = 0; i < count; ++i)
            {
                if (propLineSets.GetArrayElementAtIndex(i).objectReferenceValue != _selectedLineSet) continue;
                isFound = true;
                reorderableSetList.index = i;
                break;
            }
            if(count > 0 && !isFound)
            {
                reorderableSetList.index = 0;
            }

            // ObjectとMaterialで使用するControlIDを取得
            objectPickerID = GUIUtility.GetControlID(FocusType.Passive);
            materialPickerID = GUIUtility.GetControlID(FocusType.Passive);

            LineSetSelectionChanged();
        }


        public override void OnInspectorGUI()
        {   
            var lineNode = target as LineNode;
            serializedObject.Update();
            
            if (Event.current.type == EventType.Repaint)
            {
                reorderableSetList.OnRepaint();
            }

            // Basic Parameters
            CreateBasicParametersGui(lineNode);

            // Line Size
            CreateLineSizeGui(lineNode);

            // Others
            CreateOthersGui(lineNode);

            // Line Set
            CreateLineSetGui(lineNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line Node", priority = 20)]
        public static void OpenLineNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<LineNode>(menuCommand, typeof(LineListNode), "LineList");
        }

    }
}


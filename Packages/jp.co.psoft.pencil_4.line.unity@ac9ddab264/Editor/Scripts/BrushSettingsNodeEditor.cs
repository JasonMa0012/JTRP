using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(BrushSettingsNode))]
    public class BrushSettingsNodeEditor : Editor
    {
        /// <summary>
        /// Foldout
        /// </summary>
        public bool foldoutBrushSettings = true;


        private SerializedProperty propBrushDetail;
        //private SerializedProperty propBlendMode;
        private SerializedProperty propBlendAmount;
        private SerializedProperty propColor;
        private SerializedProperty propColorMap;
        private SerializedProperty propMapOpacity;
        private SerializedProperty propSize;
        private SerializedProperty propSizeMap;
        private SerializedProperty propSizeMapAmount;

        private SerializedObject serializedBrushDetailParams;

        private SerializedProperty propStretch;
        private SerializedProperty propAngle;


        /// <summary>
        /// BrushSettingsのGUI作成
        /// </summary>
        /// <param name="brushSettingsNode">BrushSettingsNode</param>
        void MakeBrushSettings(BrushSettingsNode brushSettingsNode)
        {
            foldoutBrushSettings =
                EditorGUILayout.Foldout(foldoutBrushSettings, "Brush Settings");
            if(!foldoutBrushSettings)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Brush Detail

            EditorGUICustomLayout.PencilNodeField(
                "Brush Detail",
                typeof(BrushDetailNode),
                serializedObject,
                propBrushDetail,
                (nodeObject) => 
                {
                    if (nodeObject == null)
                    {
                        return;
                    }

                    serializedBrushDetailParams =
                        new SerializedObject(nodeObject.GetComponent<BrushDetailNode>());
                    propStretch = serializedBrushDetailParams.FindProperty("Stretch");
                    propAngle = serializedBrushDetailParams.FindProperty("Angle");
                });


            // BlendMode
            //var blendMode = (BrushSettingsNode.BlendModeType)Enum
            //                .GetValues(typeof(BrushSettingsNode.BlendModeType))
            //                .GetValue(propBlendMode.enumValueIndex);

            //propBlendMode.enumValueIndex =
            //    (int)(LineSetNode.LineType)EditorGUILayout.EnumPopup("Blend Mode", blendMode);

            // BlendAmount
            propBlendAmount.floatValue = EditorGUILayout.Slider("Blend Amount",
                                                                propBlendAmount.floatValue,
                                                                0.0f, 1.0f);

            // Color
            propColor.colorValue =
                EditorGUILayout.ColorField("Color", propColor.colorValue);


            // ColorMap
            EditorGUICustomLayout.PencilNodeField(
                "Color Map",
                typeof(TextureMapNode),
                serializedObject,
                propColorMap,
                nodeObject => { },
                () => 
                {
                    var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushSettingsNode.transform);
                    propColorMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });


            // MapOpacity
            propMapOpacity.floatValue = EditorGUILayout.Slider("Map Opacity",
                                                               propMapOpacity.floatValue,
                                                               0.0f, 1.0f);

            // Size
            propSize.floatValue = EditorGUILayout.Slider("Size",
                                                         propSize.floatValue,
                                                         0.1f, 20.0f);

            //// SizeMap
            EditorGUICustomLayout.PencilNodeField(
                "Size Map",
                typeof(TextureMapNode),
                serializedObject,
                propSizeMap,
                nodeObject => { },
                () =>
                {
                    var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushSettingsNode.transform);
                    propSizeMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });

            //// SizeMapAmount
            propSizeMapAmount.floatValue = EditorGUILayout.Slider("Size Map Amount",
                                                                  propSizeMapAmount.floatValue,
                                                                  0.0f, 1.0f);

            // BrushDetailParams
            var bdObj = propBrushDetail.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bdObj == null))
            {
                MakeBrushDetail((GameObject)propBrushDetail.objectReferenceValue);
            }

            --EditorGUI.indentLevel;
        }


        /// <summary>
        /// BrushDetail部分のGUIの作成
        /// </summary>
        /// <param name="brushDetailObject">BrushDetailコンポーネントを持っているGameObject</param>
        void MakeBrushDetail(GameObject brushDetailObject)
        {
            Action NoBrushDetails = () =>
            {
                EditorGUILayout.Slider("Stretch", 0, -1.0f, 1.0f);
                EditorGUILayout.Slider("Angle", 0, -3600.0f, 3600.0f);
            };

            if (brushDetailObject == null)
            {
                NoBrushDetails();
                return;
            }

            BrushDetailNode brushDetailNode = brushDetailObject.GetComponent<BrushDetailNode>();
            if (brushDetailNode == null)
            {
                NoBrushDetails();
                return;
            }

            serializedBrushDetailParams.Update();

            // Stretch
            propStretch.floatValue = EditorGUILayout.Slider("Stretch",
                                                            propStretch.floatValue,
                                                            -1.0f, 1.0f);

            // Angle
            propAngle.floatValue = EditorGUILayout.Slider("Angle",
                                                            propAngle.floatValue,
                                                            -3600.0f, 3600.0f);

            serializedBrushDetailParams.ApplyModifiedProperties();

        }



        void OnEnable()
        {
            propBrushDetail     = serializedObject.FindProperty("BrushDetail");
            //propBlendMode       = serializedObject.FindProperty("BlendMode");
            propBlendAmount     = serializedObject.FindProperty("BlendAmount");
            propColor           = serializedObject.FindProperty("BrushColor");
            propColorMap        = serializedObject.FindProperty("ColorMap");
            propMapOpacity      = serializedObject.FindProperty("MapOpacity");
            propSize            = serializedObject.FindProperty("Size");
            propSizeMap         = serializedObject.FindProperty("SizeMap");
            propSizeMapAmount   = serializedObject.FindProperty("SizeMapAmount");

            // 子のBrushDetailのパラメータを取得
            GameObject brushDetail = propBrushDetail.objectReferenceValue as GameObject;

            serializedBrushDetailParams =
                brushDetail != null ?
                new SerializedObject(brushDetail.GetComponent<BrushDetailNode>()) :
                null;

            if(serializedBrushDetailParams != null)
            {
                propStretch = serializedBrushDetailParams.FindProperty("Stretch");
                propAngle = serializedBrushDetailParams.FindProperty("Angle");
            }

        }

        public override void OnInspectorGUI()
        {
            var brushSettingsNode = target as BrushSettingsNode;
            serializedObject.Update();

            MakeBrushSettings(brushSettingsNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineSetノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Brush Settings Node", priority = 20)]
        public static void OpenBrushSettingsNode(MenuCommand menuCommand)
        {
            var newBrushSettingsObject = EditorCommons.CreateNodeObjectFromMenu<BrushSettingsNode>(menuCommand);

            // BrushDetailの追加
            var brushSettingsNode = newBrushSettingsObject.GetComponent<BrushSettingsNode>();
            var newBrushDetailObject = Pcl4EditorUtilities.CreateNodeObject<BrushDetailNode>(newBrushSettingsObject.transform);

            brushSettingsNode.BrushDetail = newBrushDetailObject;
        }
    }
}

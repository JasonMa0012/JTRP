#pragma warning disable 0414
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{
    using Brush = BrushDetailNode.Brush;
    using Stroke = BrushDetailNode.Stroke;
    using Line = BrushDetailNode.Line;
    using LoopDirection  = BrushDetailNode.LoopDirection;
    using ColorSpace = BrushDetailNode.ColorSpace;
    using Common = EditorCommons;

    [CustomEditor(typeof(BrushDetailNode))]
    public class BrushDetailNodeEditor : Editor
    {
        private SerializedProperty propBrushType;
        private SerializedProperty propBrushMap;
        private SerializedProperty propMapOpacity;
        private SerializedProperty propStretch;
        private SerializedProperty propStretchRandom;
        private SerializedProperty propAngle;
        private SerializedProperty propAngleRandom;
        private SerializedProperty propGroove;
        private SerializedProperty propGrooveNumber;
        private SerializedProperty propSize;
        private SerializedProperty propSizeRandom;
        private SerializedProperty propAntialiasing;
        private SerializedProperty propHorizontalSpace;
        private SerializedProperty propHorizontalSpaceRandom;
        private SerializedProperty propVerticalSpace;
        private SerializedProperty propVerticalSpaceRandom;
        private SerializedProperty propReductionStart;
        private SerializedProperty propReductionEnd;
        private SerializedProperty propStrokeType;
        private SerializedProperty propLineType;
        private SerializedProperty propLength;
        private SerializedProperty propLengthRandom;
        private SerializedProperty propSpace;
        private SerializedProperty propSpaceRandom;
        private SerializedProperty propLengthSizeRandom;
        private SerializedProperty propExtend;
        private SerializedProperty propExtendRandom;
        private SerializedProperty propLineCopy;
        private SerializedProperty propLineCopyRandom;
        private SerializedProperty propNormalOffset;
        private SerializedProperty propNormalOffsetRandom;
        private SerializedProperty propXOffset;
        private SerializedProperty propXOffsetRandom;
        private SerializedProperty propYOffset;
        private SerializedProperty propYOffsetRandom;
        private SerializedProperty propLineSplitAngle;
        private SerializedProperty propMinLineLength;
        private SerializedProperty propLineLinkLength;
        private SerializedProperty propLineDirection;
        private SerializedProperty propLoopDirectionType;
        private SerializedProperty propDistortionEnable;
        private SerializedProperty propDistortionMap;
        private SerializedProperty propMapAmount;
        private SerializedProperty propAmount;
        private SerializedProperty propRandom;
        private SerializedProperty propCycles;
        private SerializedProperty propCyclesRandom;
        private SerializedProperty propPhase;
        private SerializedProperty propPhaseRandom;
        private SerializedProperty propSizeReductionEnable;
        private SerializedProperty propSizeReductionCurve;
        private SerializedProperty propAlphaReductionEnable;
        private SerializedProperty propAlphaReductionCurve;
        private SerializedProperty propColorSpaceType;
        private SerializedProperty propColorRed;
        private SerializedProperty propColorGreen;
        private SerializedProperty propColorBlue;
        private SerializedProperty propColorHue;
        private SerializedProperty propColorSaturation;
        private SerializedProperty propColorValue;

        private Brush currentBrushType;
        private Line currentLineType;
        
        public bool foldoutBrushEditor = true;
        public bool foldoutStroke = true;
        public bool foldoutDistortion = true;
        public bool foldoutSizeReduction = true;
        public bool foldoutAlphaReduction = true;
        public bool foldoutColorRange = true;

        // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
        // エディタでの反映が上手くいかないため、バッファとして用意
        private AnimationCurve sizeCurve;
        private AnimationCurve alphaCurve;

        /// <summary>
        /// BrushEditor項目のGUIを作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeBrushEditor(BrushDetailNode brushDetailNode)
        {
            foldoutBrushEditor = 
                EditorGUILayout.Foldout(foldoutBrushEditor, "Brush Editor");
            if(!foldoutBrushEditor)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Brush Type
            var brushType = (Brush)Enum
                           .GetValues(typeof(Brush))
                           .GetValue(propBrushType.enumValueIndex);

            propBrushType.enumValueIndex =
                (int)(BrushDetailNode.Brush)EditorGUILayout.EnumPopup("Brush Type", brushType);

            currentBrushType = (BrushDetailNode.Brush)propBrushType.enumValueIndex;

            using (new EditorGUI.DisabledGroupScope(currentBrushType == BrushDetailNode.Brush.Simple))
            {
                // Brush Map
                EditorGUICustomLayout.PencilNodeField(
                    "Brush Map",
                    typeof(TextureMapNode),
                    serializedObject,
                    propBrushMap,
                    selectedObject => { },
                    () =>
                    {
                        var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushDetailNode.transform);
                        propBrushMap.objectReferenceValue = textureMap;
                        Selection.activeObject = textureMap;
                        Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                    });

                // Map Opacity
                propMapOpacity.floatValue = EditorGUILayout.Slider("Map Opacity",
                                                                   propMapOpacity.floatValue,
                                                                   0.0f, 1.0f);
            }



            // Stretch
            propStretch.floatValue = EditorGUILayout.Slider("Stretch",
                                                            propStretch.floatValue,
                                                            -1.0f, 1.0f);

            // Stretch Random
            propStretchRandom.floatValue = EditorGUILayout.Slider("Stretch Random",
                                                                  propStretchRandom.floatValue,
                                                                  0.0f, 1.0f);

            // Angle
            propAngle.floatValue = EditorGUILayout.Slider("Angle",
                                                          propAngle.floatValue,
                                                          -3600.0f, 3600.0f);

            // Angle Random
            propAngleRandom.floatValue = EditorGUILayout.Slider("Angle Random",
                                                                propAngleRandom.floatValue,
                                                                0.0f, 360.0f);

            using (new EditorGUI.DisabledGroupScope(currentBrushType == BrushDetailNode.Brush.Simple))
            {
                // Groove
                propGroove.floatValue = EditorGUILayout.Slider("Groove",
                                                               propGroove.floatValue,
                                                               0.0f, 1.0f);

                // Groove Number
                propGrooveNumber.intValue = EditorGUILayout.IntSlider("Groove Number",
                                                                      propGrooveNumber.intValue,
                                                                      3, 20);
            }


            using (new EditorGUI.DisabledGroupScope(currentBrushType != Brush.Multiple))
            {
                // Size
                propSize.floatValue = EditorGUILayout.Slider("Size",
                                                             propSize.floatValue,
                                                             0.1f, 100.0f);

                // Size Random
                propSizeRandom.floatValue = EditorGUILayout.Slider("Size Random",
                                                                   propSizeRandom.floatValue,
                                                                   0.0f, 1.0f);
                // Antialiasing
                propAntialiasing.floatValue = EditorGUILayout.Slider("Antialiasing",
                                                                     propAntialiasing.floatValue,
                                                                     0.0f, 10.0f);

                // Horizontal Space
                propHorizontalSpace.floatValue = EditorGUILayout.Slider("Horizontal Space",
                                                                        propHorizontalSpace.floatValue,
                                                                        0.0f, 1.0f);
                // Horizontal Space Random
                propHorizontalSpaceRandom.floatValue =
                    EditorGUILayout.Slider("Horizontal Space Random",
                                           propHorizontalSpaceRandom.floatValue,
                                           0.0f, 1.0f);

                // Vertical Space
                propVerticalSpace.floatValue = EditorGUILayout.Slider("Vertical Space",
                                                                      propVerticalSpace.floatValue,
                                                                      0.0f, 1.0f);

                // Vertical Space Random
                propVerticalSpaceRandom.floatValue =
                    EditorGUILayout.Slider("Vertical Space Random",
                                           propVerticalSpaceRandom.floatValue,
                                           0.0f, 1.0f);

            }

            using (new EditorGUI.DisabledGroupScope(currentBrushType == BrushDetailNode.Brush.Simple))
            {

                // Reduction Start
                propReductionStart.floatValue = EditorGUILayout.Slider("Reduction Start",
                                                                       propReductionStart.floatValue,
                                                                       0.0f, 1.0f);

                // Reduction End
                propReductionEnd.floatValue = EditorGUILayout.Slider("Reduction End",
                                                           propReductionEnd.floatValue,
                                                           0.0f, 1.0f);

            }

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// Stroke項目のGUIの作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeStroke(BrushDetailNode brushDetailNode)
        {
            foldoutStroke =
                EditorGUILayout.Foldout(foldoutStroke, "Stroke");
            if (!foldoutStroke)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            using (new EditorGUI.DisabledGroupScope(currentBrushType == BrushDetailNode.Brush.Simple))
            {

                // Stroke Type
                var strokeType = (Stroke)Enum
                   .GetValues(typeof(Stroke))
                   .GetValue(propStrokeType.enumValueIndex);

                propStrokeType.enumValueIndex =
                    (int)(Stroke)EditorGUILayout.EnumPopup("Stroke Type", strokeType);

            }

            
            // Line Type
            var lineType = (Line)Enum
               .GetValues(typeof(Line))
               .GetValue(propLineType.enumValueIndex);

            propLineType.enumValueIndex =
                (int)(Line)EditorGUILayout.EnumPopup("Line Type", lineType);

            currentLineType = (Line)propLineType.enumValueIndex;


            using (new EditorGUI.DisabledGroupScope(currentLineType == Line.Full))
            {
                // Length
                propLength.floatValue = EditorGUILayout.Slider("Length",
                                                               propLength.floatValue,
                                                               0.001f, 10000.0f);

                // Length Random
                propLengthRandom.floatValue = EditorGUILayout.Slider("Length Random",
                                                                     propLengthRandom.floatValue,
                                                                     0.0f, 1.0f);

                // Space
                propSpace.floatValue = EditorGUILayout.Slider("Space",
                                                              propSpace.floatValue,
                                                              1.0f, 10000.0f);

                // Space Random
                propSpaceRandom.floatValue = EditorGUILayout.Slider("Space Random",
                                                                    propSpaceRandom.floatValue,
                                                                    0.0f, 1.0f);

            }

            // Length Size Random
            propLengthSizeRandom.floatValue = EditorGUILayout.Slider("Length Size Random",
                                                                     propLengthSizeRandom.floatValue,
                                                                     0.0f, 1.0f);

            // Extend
            propExtend.floatValue = EditorGUILayout.Slider("Extend",
                                                           propExtend.floatValue,
                                                           0.0f, 10000.0f);

            // Extend Random
            propExtendRandom.floatValue = EditorGUILayout.Slider("Extend Random",
                                                                 propExtendRandom.floatValue,
                                                                 0.0f, 1.0f);

            // Line Copy
            propLineCopy.intValue = EditorGUILayout.IntSlider("Line Copy",
                                                              propLineCopy.intValue,
                                                              1, 10);

            // Line Copy Random
            propLineCopyRandom.intValue = EditorGUILayout.IntSlider("Line Copy Random",
                                                                    propLineCopyRandom.intValue,
                                                                    0, 10);

            // Normal Offset
            propNormalOffset.floatValue = EditorGUILayout.Slider("Normal Offset",
                                                                 propNormalOffset.floatValue,
                                                                 -1000.0f, 1000.0f);

            // Normal Offset Random
            propNormalOffsetRandom.floatValue = EditorGUILayout.Slider("Normal Offset Random",
                                                                       propNormalOffsetRandom.floatValue,
                                                                       0.0f, 1000.0f);

            // X Offset
            propXOffset.floatValue = EditorGUILayout.Slider("X Offset",
                                                                 propXOffset.floatValue,
                                                                 -1000.0f, 1000.0f);

            // X Offset Random
            propXOffsetRandom.floatValue = EditorGUILayout.Slider("X Offset Random",
                                                                       propXOffsetRandom.floatValue,
                                                                       0.0f, 1000.0f);
            
            // Y Offset
            propYOffset.floatValue = EditorGUILayout.Slider("Y Offset",
                                                                 propYOffset.floatValue,
                                                                 -1000.0f, 1000.0f);
            
            // Y Offset Random
            propYOffsetRandom.floatValue = EditorGUILayout.Slider("Y Offset Random",
                                                                       propYOffsetRandom.floatValue,
                                                                       0.0f, 1000.0f);

            // Line Split Angle
            propLineSplitAngle.floatValue = EditorGUILayout.Slider("Line Split Angle",
                                                                   propLineSplitAngle.floatValue,
                                                                   0.0f, 180.0f);

            // Min Line Length
            propMinLineLength.floatValue = EditorGUILayout.Slider("Min Line Length",
                                                                  propMinLineLength.floatValue,
                                                                  0.0f, 100.0f);
            
            // Line Link Length
            propLineLinkLength.floatValue = EditorGUILayout.Slider("Line Link Length",
                                                                   propLineLinkLength.floatValue,
                                                                   0.0f, 100.0f);

            // Line Direction
            propLineDirection.floatValue = EditorGUILayout.Slider("Line Direction",
                                                                  propLineDirection.floatValue,
                                                                  -180.0f, 180.0f);

            // Loop Direction
            var loopDirection = (LoopDirection)Enum
                                .GetValues(typeof(LoopDirection))
                                .GetValue(propLoopDirectionType.enumValueIndex);

            propLoopDirectionType.enumValueIndex =
                (int)(LoopDirection)EditorGUILayout.EnumPopup("Loop Direction", loopDirection);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// Distortion項目のGUIの作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeDistortion(BrushDetailNode brushDetailNode)
        {
            foldoutDistortion =
                EditorGUILayout.Foldout(foldoutDistortion, "Distortion");
            if(!foldoutDistortion)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Enable
            propDistortionEnable.boolValue = 
                EditorGUILayout.Toggle("Enable", propDistortionEnable.boolValue);

            
            using (new EditorGUI.DisabledGroupScope(!propDistortionEnable.boolValue))
            {
                // Distortion Map
                EditorGUICustomLayout.PencilNodeField(
                  "Brush Map",
                  typeof(TextureMapNode),
                  serializedObject,
                  propDistortionMap,
                  selectedObject => { },
                  () =>
                  {
//                      var textureMap = Instantiate(Prefabs.TextureMap);
//                      textureMap.name = Common.GetAllGameObject().GetUniqueName(Prefabs.TextureMap);
                      var textureMap = Pcl4EditorUtilities.CreateNodeObject<TextureMapNode>(brushDetailNode.transform);
                      propDistortionMap.objectReferenceValue = textureMap;
                      Selection.activeObject = textureMap;
                      Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                  });



                // Map Amount
                propMapAmount.floatValue = EditorGUILayout.Slider("Map Amount",
                                                                  propMapAmount.floatValue,
                                                                  0.0f, 1000.0f);

                // Amount
                propAmount.floatValue = EditorGUILayout.Slider("Amount",
                                                               propAmount.floatValue,
                                                               0.0f, 1000.0f);

                // Random
                propRandom.floatValue = EditorGUILayout.Slider("Random",
                                                               propRandom.floatValue,
                                                               0.0f, 1.0f);
                // Cycles
                propCycles.floatValue = EditorGUILayout.Slider("Cycles",
                                                               propCycles.floatValue,
                                                               5.0f, 1000.0f);
                // Cycles Random
                propCyclesRandom.floatValue = EditorGUILayout.Slider("Cycles Random",
                                                                     propCyclesRandom.floatValue,
                                                                     0.0f, 1.0f);

                // Phase
                propPhase.floatValue = EditorGUILayout.Slider("Phase",
                                                              propPhase.floatValue,
                                                              -9999.0f, 9999.0f);
                // Phase Random
                propPhaseRandom.floatValue = EditorGUILayout.Slider("Phase Random",
                                                                    propPhaseRandom.floatValue,
                                                                    0.0f, 1.0f);
            }

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }
        
        /// <summary>
        /// SizeReduction項目のGUIの作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeSizeReduction(BrushDetailNode brushDetailNode)
        {
            foldoutSizeReduction =
                EditorGUILayout.Foldout(foldoutSizeReduction, "Size Reduction");
            if (!foldoutSizeReduction)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Enable
            propSizeReductionEnable.boolValue =
                EditorGUILayout.Toggle("Enable", propSizeReductionEnable.boolValue);

            using (new EditorGUI.DisabledGroupScope(!propSizeReductionEnable.boolValue))
            {
                // Curve
                // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
                // エディタでの反映が上手くいかないため、第二引数をバッファ変数から読ませる
                propSizeReductionCurve.animationCurveValue =
                    EditorGUILayout.CurveField("Curve",
                                               sizeCurve, // propSizeReductionCurve.animationCurveValue,
                                               Color.green,
                                               new Rect(0.0f, 0.0f, 1.0f, 1.0f));
            }
            
            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }
        
        /// <summary>
        /// AlphaReduction項目のGUIの作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeAlphaReduction(BrushDetailNode brushDetailNode)
        {
            foldoutAlphaReduction =
            EditorGUILayout.Foldout(foldoutAlphaReduction, "Alpha Reduction");
            if (!foldoutAlphaReduction)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Enable
            propAlphaReductionEnable.boolValue =
                 EditorGUILayout.Toggle("Enable", propAlphaReductionEnable.boolValue);

            using (new EditorGUI.DisabledGroupScope(!propAlphaReductionEnable.boolValue))
            {
                // Curve
                // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
                // エディタでの反映が上手くいかないため、第二引数をバッファ変数から読ませる
                propAlphaReductionCurve.animationCurveValue =
                    EditorGUILayout.CurveField("Curve",
                                               alphaCurve, // propAlphaReductionCurve.animationCurveValue,
                                               Color.green,
                                               new Rect(0.0f, 0.0f, 1.0f, 1.0f));
            }

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// ColorRange項目のGUIの作成
        /// </summary>
        /// <param name="brushDetailNode">BrushDetailNode</param>
        void MakeColorRange(BrushDetailNode brushDetailNode)
        {
            foldoutColorRange =
                EditorGUILayout.Foldout(foldoutColorRange, "Color Range");
            if (!foldoutColorRange)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Color Space
            var colorSpace = (ColorSpace)Enum
                            .GetValues(typeof(ColorSpace))
                            .GetValue(propColorSpaceType.enumValueIndex);

            propColorSpaceType.enumValueIndex =
                (int)(ColorSpace)EditorGUILayout.EnumPopup("Color Space", colorSpace);

            ++EditorGUI.indentLevel;

            // RGB
            if (propColorSpaceType.enumValueIndex == (int)ColorSpace.RGB)
            {
                // Red
                propColorRed.floatValue = EditorGUILayout.Slider("R",
                                                                 propColorRed.floatValue,
                                                                 0.0f, 1.0f);
                
                // Green
                propColorGreen.floatValue = EditorGUILayout.Slider("G",
                                                                 propColorGreen.floatValue,
                                                                 0.0f, 1.0f);

                // Blue
                propColorBlue.floatValue = EditorGUILayout.Slider("B",
                                                                 propColorBlue.floatValue,
                                                                 0.0f, 1.0f);
            }

            // HSV
            else 
            {
                // Hue
                propColorHue.floatValue = EditorGUILayout.Slider("H",
                                                                 propColorHue.floatValue,
                                                                 0.0f, 1.0f);

                // Saturation
                propColorSaturation.floatValue = EditorGUILayout.Slider("S",
                                                                 propColorSaturation.floatValue,
                                                                 0.0f, 1.0f);

                // Value
                propColorValue.floatValue = EditorGUILayout.Slider("V",
                                                                   propColorValue.floatValue,
                                                                   0.0f, 1.0f);            
            }

            --EditorGUI.indentLevel;

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }

        void OnEnable()
        {
            propBrushType = serializedObject.FindProperty("BrushType");
            propBrushMap = serializedObject.FindProperty("BrushMap");
            propMapOpacity = serializedObject.FindProperty("MapOpacity");
            propStretch = serializedObject.FindProperty("Stretch");
            propStretchRandom = serializedObject.FindProperty("StretchRandom");
            propAngle = serializedObject.FindProperty("Angle");
            propAngleRandom = serializedObject.FindProperty("AngleRandom");
            propGroove = serializedObject.FindProperty("Groove");
            propGrooveNumber = serializedObject.FindProperty("GrooveNumber");
            propSize = serializedObject.FindProperty("Size");
            propSizeRandom = serializedObject.FindProperty("SizeRandom");
            propAntialiasing = serializedObject.FindProperty("Antialiasing");
            propHorizontalSpace = serializedObject.FindProperty("HorizontalSpace");
            propHorizontalSpaceRandom = serializedObject.FindProperty("HorizontalSpaceRandom");
            propVerticalSpace = serializedObject.FindProperty("VerticalSpace");
            propVerticalSpaceRandom = serializedObject.FindProperty("VerticalSpaceRandom");
            propReductionStart = serializedObject.FindProperty("ReductionStart");
            propReductionEnd = serializedObject.FindProperty("ReductionEnd");
            propStrokeType = serializedObject.FindProperty("StrokeType");
            propLineType = serializedObject.FindProperty("LineType");
            propLength = serializedObject.FindProperty("Length");
            propLengthRandom = serializedObject.FindProperty("LengthRandom");
            propSpace = serializedObject.FindProperty("Space");
            propSpaceRandom = serializedObject.FindProperty("SpaceRandom");
            propLengthSizeRandom = serializedObject.FindProperty("LengthSizeRandom");
            propExtend = serializedObject.FindProperty("Extend");
            propExtendRandom = serializedObject.FindProperty("ExtendRandom");
            propLineCopy = serializedObject.FindProperty("LineCopy");
            propLineCopyRandom = serializedObject.FindProperty("LineCopyRandom");
            propNormalOffset = serializedObject.FindProperty("NormalOffset");
            propNormalOffsetRandom = serializedObject.FindProperty("NormalOffsetRandom");
            propXOffset = serializedObject.FindProperty("XOffset");
            propXOffsetRandom = serializedObject.FindProperty("XOffsetRandom");
            propYOffset = serializedObject.FindProperty("YOffset");
            propYOffsetRandom = serializedObject.FindProperty("YOffsetRandom");
            propLineSplitAngle = serializedObject.FindProperty("LineSplitAngle");
            propMinLineLength = serializedObject.FindProperty("MinLineLength");
            propLineLinkLength = serializedObject.FindProperty("LineLinkLength");
            propLineDirection = serializedObject.FindProperty("LineDirection");
            propLoopDirectionType = serializedObject.FindProperty("LoopDirectionType");
            propDistortionEnable = serializedObject.FindProperty("DistortionEnable");
            propDistortionMap = serializedObject.FindProperty("DistortionMap");
            propMapAmount = serializedObject.FindProperty("MapAmount");
            propAmount = serializedObject.FindProperty("Amount");
            propRandom = serializedObject.FindProperty("Random");
            propCycles = serializedObject.FindProperty("Cycles");
            propCyclesRandom = serializedObject.FindProperty("CyclesRandom");
            propPhase = serializedObject.FindProperty("Phase");
            propPhaseRandom = serializedObject.FindProperty("PhaseRandom");
            propSizeReductionEnable = serializedObject.FindProperty("SizeReductionEnable");
            propSizeReductionCurve = serializedObject.FindProperty("SizeReductionCurve");
            propAlphaReductionEnable = serializedObject.FindProperty("AlphaReductionEnable");
            propAlphaReductionCurve = serializedObject.FindProperty("AlphaReductionCurve");
            propColorSpaceType = serializedObject.FindProperty("ColorSpaceType");
            propColorRed = serializedObject.FindProperty("ColorRed");
            propColorGreen = serializedObject.FindProperty("ColorGreen");
            propColorBlue = serializedObject.FindProperty("ColorBlue");
            propColorHue = serializedObject.FindProperty("ColorHue");
            propColorSaturation = serializedObject.FindProperty("ColorSaturation");
            propColorValue = serializedObject.FindProperty("ColorValue");

            // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
            // エディタでの反映が上手くいかないため、バッファとして変数を噛ませる
            sizeCurve = propSizeReductionCurve.animationCurveValue;
            alphaCurve = propAlphaReductionCurve.animationCurveValue;

        }

        public override void OnInspectorGUI()
        {
            var brushDetailNode = target as BrushDetailNode;
            serializedObject.Update();

            MakeBrushEditor(brushDetailNode);
            MakeStroke(brushDetailNode);
            MakeDistortion(brushDetailNode);
            MakeSizeReduction(brushDetailNode);
            MakeAlphaReduction(brushDetailNode);
            MakeColorRange(brushDetailNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにBrushDetailノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Brush Detail Node", priority = 20)]
        public static void OpenBrushDetailNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<BrushDetailNode>(menuCommand);
        }
    }
}


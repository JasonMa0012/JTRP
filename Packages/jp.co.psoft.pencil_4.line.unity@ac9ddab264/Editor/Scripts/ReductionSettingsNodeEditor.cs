using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(ReductionSettingsNode))]
    public class ReductionSettingsNodeEditor : Editor
    {
        public bool foldoutStartAndEnd = true;

        private SerializedProperty propStart;
        private SerializedProperty propEnd;
        private SerializedProperty propReferObject;
        private SerializedProperty propObject;
        private SerializedProperty propCurve;

        // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
        // エディタでの反映が上手くいかないため、バッファとして用意
        private AnimationCurve curve;


        /// <summary>
        /// ReductionNodeのGUI作成
        /// </summary>
        /// <param name="reductionSettingsNode">ReductionSettingsNode</param>
        void MakeReductionSettings(ReductionSettingsNode reductionSettingsNode)
        {
            foldoutStartAndEnd =
                EditorGUILayout.Foldout(foldoutStartAndEnd, "Start and End");
            if (!foldoutStartAndEnd)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Start
            propStart.floatValue =
                EditorGUILayout.Slider("Start", propStart.floatValue, 0.01f, 1000.0f);
            
            // End
            propEnd.floatValue =
                EditorGUILayout.Slider("End", propEnd.floatValue, 0.01f, 1000.0f);

            // Refer Object
            propReferObject.boolValue =
                EditorGUILayout.Toggle("Refer Object", propReferObject.boolValue);


            EditorGUI.BeginDisabledGroup(!propReferObject.boolValue); // object

            ++EditorGUI.indentLevel;

            // Object
           var targetObjectName = propObject.objectReferenceValue ? propObject.objectReferenceValue.name : "None (Game Object)";
           EditorGUILayout.LabelField(
               new GUIContent("Object"),
               new GUIContent(
                   targetObjectName,
                   EditorGUIUtility.IconContent("GameObject Icon").image),
               GUI.skin.FindStyle("ObjectField"));

           // フィールド右側の☉ボタンの範囲
           var objectFieldRect = GUILayoutUtility.GetLastRect();
#if UNITY_2019_3_OR_NEWER
            var pickerButtonRect = objectFieldRect;
            pickerButtonRect.x += pickerButtonRect.width - 19;
            pickerButtonRect.width = 19;

            if (Event.current.type == EventType.Repaint)
            {
                var style = GUI.skin.FindStyle("ObjectFieldButton");
                style.Draw(style.margin.Remove(pickerButtonRect), objectFieldRect.Contains(Event.current.mousePosition), false, false, false);
            }
#else
            var pickerButtonRect = objectFieldRect;
            pickerButtonRect.x += pickerButtonRect.width - 15;
            pickerButtonRect.width = 15;
#endif

            if (Event.current.type == EventType.MouseDown
               && Event.current.button == 0) // 左クリック
           {
               if(pickerButtonRect.Contains(Event.current.mousePosition))
               {
                    // ☉ボタンのクリック
                    SingleObjectPickerWindow.OpenAllObjectPicker(x =>
                    {
                        serializedObject.Update();
                        propObject.objectReferenceValue = x;
                        serializedObject.ApplyModifiedProperties();
                    });
               }
               else if(objectFieldRect.Contains(Event.current.mousePosition))
               {
                    // テキストフィールドのクリック
                    EditorGUIUtility.PingObject(propObject.objectReferenceValue);
                    
                    // ↓ On Double Click
                    //Selection.activeObject = propObject.objectReferenceValue;
               }
           }

           

            --EditorGUI.indentLevel;

            EditorGUI.EndDisabledGroup();   // End of Object

            // Curve
            // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
            // エディタでの反映が上手くいかないため、第二引数をバッファ変数から読ませる
            propCurve.animationCurveValue =
                EditorGUILayout.CurveField("Reduction Curve",
                                           curve, // propCurve.animationCurveValue,
                                           Color.green,
                                           new Rect(0.0f, 0.0f, 1.0f, 1.0f));

            --EditorGUI.indentLevel;
        }


        void OnEnable()
        {
            propStart = serializedObject.FindProperty("ReductionStart");
            propEnd = serializedObject.FindProperty("ReductionEnd");
            propReferObject = serializedObject.FindProperty("ReferObject");
            propObject = serializedObject.FindProperty("Object");
            propCurve = serializedObject.FindProperty("Curve");

            // MEMO: 2017/07/14 Unity2017にて、Serializeされた変数から直接呼び出すと
            // エディタでの反映が上手くいかないため、バッファとして変数を噛ませる
            curve = propCurve.animationCurveValue;
        }

        public override void OnInspectorGUI()
        {
            var reductionSettingsNode = target as ReductionSettingsNode;
            serializedObject.Update();

            MakeReductionSettings(reductionSettingsNode);

            serializedObject.ApplyModifiedProperties();
        }


        /// <summary>
        /// MenuにReductionSettingsノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Reduction Settings Node", priority = 20)]
        public static void OpenReductionSettingsNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<ReductionSettingsNode>(menuCommand);
        }
    }

}


using System;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace Pcl4Editor
{
    /// <summary>
    /// ダブルクリック検知用クラス
    /// </summary>
    public static class DoubleClickCounter
    {
        const int doubleClickDurationMs = 500;
        static DateTime lastClickedTime;
        static int lastClickedControlId;

        public static bool IsDoubleClicked(int controlId)
        { 
            var elapsedTime = DateTime.Now - lastClickedTime;
            var clickedControlID = lastClickedControlId;

            lastClickedTime = DateTime.Now;
            lastClickedControlId = controlId;

            return elapsedTime < TimeSpan.FromMilliseconds(999.0) &&
                elapsedTime.Milliseconds < doubleClickDurationMs &&
                clickedControlID == controlId;
        }
    }


    public static class EditorGUICustomLayout
    {
        /// <summary>
        /// 編集不能なテキストフィールドを配置する
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="text">本文</param>
        /// <param name="style">スタイル</param>
        public static void UneditableTextField(string title, string text, GUIStyle style)
        {
            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.margin = new RectOffset(0, 0, 0, 0);
            var textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.margin = new RectOffset(0, 0, 0, 0);
            EditorGUILayout.BeginHorizontal(style);
            EditorGUILayout.PrefixLabel(title, labelStyle);
            EditorGUILayout.SelectableLabel(text, textFieldStyle, GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// NodeBaseを継承しているクラス専用のオブジェクトフィールドを配置する
        /// </summary>
        /// <param name="title">タイトル(空文字列を設定するとレイアウトが左詰めになる)</param>
        /// <param name="nodeType">Nodeの型(NodeBaseを継承しているクラスである必要がある)</param>
        /// <param name="targetSerializedObject">編集対象のオブジェクト</param>
        /// <param name="targetProperty">編集対象のプロパティ</param>
        /// <param name="onPropertyChanged">プロパティの値が変化した時に呼ばれるデリゲート</param>
        /// <param name="onFieldDoubleClicked">フィールドをダブルクリックした時に呼ばれるデリゲート</param>
        public static void PencilNodeField(
            string title, 
            Type nodeType,
            SerializedObject targetSerializedObject,
            SerializedProperty targetProperty,
            Action<GameObject> onPropertyChanged,
            Action onFieldDoubleClicked = null)
        {
            var targetObjectName = targetProperty.objectReferenceValue ?
                targetProperty.objectReferenceValue.name :
                "None (Game Object)";
            EditorGUILayout.LabelField(
                new GUIContent(title),
                new GUIContent(
                    targetObjectName,
                    BuiltInResources.PencilNodeIconResource),
                GUI.skin.FindStyle("ObjectField"));

            
            var objectFieldRect = GUILayoutUtility.GetLastRect();

            // フィールド右側の☉ボタンの範囲
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

            // クリックイベントのハンドル
            if (Event.current.type == EventType.MouseDown
               && Event.current.button == 0) // 左クリック
            {
                if (pickerButtonRect.Contains(Event.current.mousePosition))
                {
                    // ☉ボタンのクリック
                    NodePickerWindow.Open(nodeType, x =>
                    {
                        targetSerializedObject.Update();
                        if (targetProperty.objectReferenceValue != x)
                        {
                            targetProperty.objectReferenceValue = x;
                            onPropertyChanged(targetProperty.objectReferenceValue as GameObject);
                        }
                        targetSerializedObject.ApplyModifiedProperties();
                    });
                }
                else if (objectFieldRect.Contains(Event.current.mousePosition))
                {
                    // テキストフィールドのクリック
                    EditorGUIUtility.PingObject(targetProperty.objectReferenceValue);

                    // テキストフィールドのダブルクリック
                    if (DoubleClickCounter.IsDoubleClicked(GUIUtility.GetControlID(FocusType.Passive)))
                    {
                        if (onFieldDoubleClicked != null 
                            && targetProperty.objectReferenceValue == null)
                        {
                            onFieldDoubleClicked();
                        }
                        else
                        {
                            Selection.activeObject = targetProperty.objectReferenceValue;
                        }
                    }
                }
            }

            // ドラッグイベント
            if (Event.current.type == EventType.DragUpdated &&
                objectFieldRect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.activeControlID = GUIUtility.GetControlID(FocusType.Passive);
            }

            if (Event.current.type == EventType.DragPerform &&
                objectFieldRect.Contains(Event.current.mousePosition))
            { 
                DragAndDrop.AcceptDrag();
                // MEMO: 複数のオブジェクトがD&Dされた場合、最初の物以外は無視する
                var draggedObject = DragAndDrop.objectReferences.FirstOrDefault();

                if(draggedObject != null &&
                    draggedObject is GameObject &&
                    (draggedObject as GameObject).GetComponent(nodeType) != null)
                {
                    targetSerializedObject.Update();
                    if (targetProperty.objectReferenceValue != draggedObject)
                    {
                        targetProperty.objectReferenceValue = draggedObject;
                        onPropertyChanged(targetProperty.objectReferenceValue as GameObject);
                    }
                    targetSerializedObject.ApplyModifiedProperties();
                }

                DragAndDrop.activeControlID = 0;
                Event.current.Use();
            }
        }

    }
}
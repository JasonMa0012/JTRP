using System.Collections;
using System.Collections.Generic;
using Pencil_4;
using UnityEngine;
using UnityEditor;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(PencilLineEffect))]
    public class PencilLineEffectEditor : Editor
    {
        private SerializedProperty _propLineListObject;
        private SerializedProperty _propMode;
        private SerializedProperty _propRenderElements;
        
        private PencilReorderableList _reorderableRenderElementsList;
        
        private void OnEnable()
        {
            _propLineListObject = serializedObject.FindProperty("LineListObject");
            _propMode = serializedObject.FindProperty("Mode");

            var currentComponent = (PencilLineEffect)target;

            _propRenderElements =
                serializedObject.FindProperty("RenderElements");

            _reorderableRenderElementsList =
                Common.CreateReorderableNodeList<RenderElementsNodeBase>(
                    serializedObject,
                    _propRenderElements,
                    currentComponent);

            _reorderableRenderElementsList.onAddCallback = list =>
            {
                var pencilList = (PencilReorderableList)list;
                pencilList.index = _propRenderElements.arraySize++;

                var element = _propRenderElements.GetArrayElementAtIndex(pencilList.index);
                var newNode = Pcl4EditorUtilities.CreateNodeObject<RenderElementsLineNode>(currentComponent.transform);
                Undo.RegisterCreatedObjectUndo(newNode, "Create Node");

                element.objectReferenceValue = newNode;
                list.GrabKeyboardFocus();
            };

//            _reorderableRenderElementsList.onAddDropdownCallback = (rect, list) =>
//            {
//                var menu = new GenericMenu();
//                menu.AddItem(new GUIContent("Line"), false, () =>
//                {
//                        var pencilList = (PencilReorderableList)list;
//                        pencilList.index = _propRenderElements.arraySize++;
//                
//                        var element = _propRenderElements.GetArrayElementAtIndex(pencilList.index);
//                        var newNode = Pcl4EditorUtilities.CreateNodeObject<RenderElementsLineNode>();
//                        Undo.RegisterCreatedObjectUndo(newNode, "Create Node");
//                
//                        newNode.transform.parent = currentComponent.transform;
//                        element.objectReferenceValue = newNode;
//                        list.GrabKeyboardFocus();
//                });
//                
//                menu.AddDisabledItem(new GUIContent("PLD"));
//                
//                menu.DropDown(rect);
//            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (Event.current.type == EventType.Repaint)
            {
                _reorderableRenderElementsList.OnRepaint();
            }
            
            // Line List Object
            EditorGUICustomLayout.PencilNodeField(
                "Line List Object",
                typeof(LineListNode),
                serializedObject,
                _propLineListObject,
                _ => {});
            
            // Mode
            _propMode.enumValueIndex = (int)(EffectMode)EditorGUILayout.EnumPopup(
                "Mode", (EffectMode)_propMode.enumValueIndex);
            
            EditorGUILayout.Space();
            
            // Render Elements
            EditorGUILayout.LabelField("Render Elements");
            var style = new GUIStyle {margin = new RectOffset(4, 8, 0, 4)};

            using (new EditorGUILayout.VerticalScope(style))
            {
                _reorderableRenderElementsList.HandleInputEventAndLayoutList();
            }


            serializedObject.ApplyModifiedProperties();
        }

        [InitializeOnLoadMethod]
        static void InitializeComponentWhenAdded()
        {
            ObjectFactory.componentWasAdded += componet =>
            {
                if (EditorApplication.isPlaying) {return;}

                if (componet.GetType() == typeof(PencilLineEffect))
                {
                    var lineEffect = componet as PencilLineEffect;
                    if (lineEffect.LineListObject == null)
                    {
                        lineEffect.LineListObject = Pcl4EditorUtilities.CreateNodeObject<LineListNode>(null);
                        Undo.RegisterCreatedObjectUndo(lineEffect.LineListObject, "Create Node");
                    }
                }
            };
        }
    }
}

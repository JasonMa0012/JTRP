
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(LineListNode))]
    public class LineListNodeEditor : Editor
    {

        private SerializedProperty _propLineList;
        private SerializedProperty _propLineFunctionsList;
        private SerializedObject _serializedLineFunctionsObject;
        private SerializedProperty _propTargetMaterials;    // MaterialLineFunctionsList.TargetMaterials
        private SerializedProperty _propDoubleSidedMaterials;
        private SerializedProperty _propIgnoreObjectList;
        private SerializedProperty _propLineGroupList;
        private SerializedObject _serializedLineGroupObject;
        private SerializedProperty _propLineGroupObjects;

        private PencilReorderableList _reorderableLineList;
        private PencilReorderableList _reorderableFunctionsList;
        private PencilReorderableList _reorderableTargetMaterialList;
        private PencilReorderableList _reorderableDoubleSidedMaterials;
        private PencilReorderableList _reorderableIgnoreObjectList;
        private PencilReorderableList _reorderableLineGroupList;
        private PencilReorderableList _reorderableLineGroupObjectsList;
        
        private GameObject _selectedTargetMaterial;
        private GameObject _selectedLineGroup;
        
        private GameObject _oldLineFunc = null;
        private GameObject _oldLineGroup = null;

        private MethodInfo _importMethodInfo = null;
        private MethodInfo _exportMethodInfo = null;

        // Material Line FunctionsのMaterialsリストを更新するため、定期的な再描画を行う
        public override bool RequiresConstantRepaint() { return true; }


        private void OnEnable()
        {
            var currentNode = target as LineListNode;

            // ---------- Line List ----------

            _propLineList =
                serializedObject.FindProperty("LineList");


            _reorderableLineList =
                Common.CreateReorderableNodeList<LineNode>(
                    serializedObject,
                    _propLineList,
                    currentNode);


            // ---------- Material Line Functions List ----------

            _propLineFunctionsList =
                serializedObject.FindProperty("LineFunctionsList");

            _reorderableFunctionsList =
                Common.CreateReorderableNodeList<MaterialLineFunctionsNode>(
                    serializedObject,
                    _propLineFunctionsList,
                    currentNode);

            _reorderableFunctionsList.IsSelectionLimitOneOrMore = true;

            // Elementを追加するコールバック
            _reorderableFunctionsList.onAddCallback += (list) =>
            {
                LineFunctionsIndexChanged();
            };

            // Elementを削除するコールバック
            _reorderableFunctionsList.onRemoveCallback += (list) =>
            {
                LineFunctionsIndexChanged();
            };

            // Elementの入れ替えを行った際に呼ばれるコールバック
            _reorderableFunctionsList.onReorderCallback += (list) =>
            {
                LineFunctionsIndexChanged();
            };

            // 選択状態変更
            _reorderableFunctionsList.OnSelectionChangeCallback += (list) =>
            {
                LineFunctionsIndexChanged();
            };


            // ---------- Double Sided Materials ----------

            _propDoubleSidedMaterials =
                serializedObject.FindProperty("DoubleSidedMaterials");

            _reorderableDoubleSidedMaterials =
                Common.CreateMaterialList(
                    serializedObject,
                    _propDoubleSidedMaterials,
                    new List<Material>(),
                    selectedMaterials => 
                    {
                        serializedObject.Update();
                        _propDoubleSidedMaterials.AppendObjects(selectedMaterials);
                        serializedObject.ApplyModifiedProperties();
                        _reorderableDoubleSidedMaterials.index = _reorderableDoubleSidedMaterials.count - 1;

                    });


            // ---------- Ignore Object List ----------

            _propIgnoreObjectList =
                serializedObject.FindProperty("IgnoreObjectList");

            _reorderableIgnoreObjectList =
                Common.CreateObjectList(
                    serializedObject,
                    _propIgnoreObjectList,
                    new List<GameObject>(), 
                    selectedObjects => 
                    {
                        serializedObject.Update();
                        _propIgnoreObjectList.AppendObjects(selectedObjects);
                        serializedObject.ApplyModifiedProperties();
                        _reorderableIgnoreObjectList.index = _reorderableIgnoreObjectList.count - 1;
                    });
            
            // ---------- Line Group List ----------

            _propLineGroupList = serializedObject.FindProperty("LineGroupList");

            _reorderableLineGroupList =
                Common.CreateReorderableNodeList<LineGroupNode>(
                    serializedObject,
                    _propLineGroupList,
                    currentNode);
            
            _reorderableLineGroupList.onAddCallback += (list) =>
            {
                LineGroupsIndexChanged();
            };

            _reorderableLineGroupList.onRemoveCallback += (list) =>
            {
                LineGroupsIndexChanged();
            };

            _reorderableLineGroupList.onReorderCallback += (list) =>
            {
                LineGroupsIndexChanged();
            };

            _reorderableLineGroupList.OnSelectionChangeCallback += (list) =>
            {
                LineGroupsIndexChanged();
            };

            _reorderableLineGroupList.IsSelectionLimitOneOrMore = true;

            //
            var bridgeEditorType = Type.GetType("Pcl4Editor.BridgeEditor, PSOFT.Pencil_4_Bridge.Editor");
            if (bridgeEditorType != null)
            {
                _importMethodInfo = bridgeEditorType.GetMethod("Import", BindingFlags.Public | BindingFlags.Static);
                _exportMethodInfo = bridgeEditorType.GetMethod("Export", BindingFlags.Public | BindingFlags.Static);
            }
        }



        /// <summary>
        /// LineListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateLineListGui(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            EditorGUILayout.LabelField("Line List");

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<LineNode>(lineListNode,
                                             _propLineList,
                                             verticalLayout.rect);

            using (verticalLayout)
            {
                _reorderableLineList.HandleInputEventAndLayoutList();
            }
        }

        /// <summary>
        /// LineFunctionsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateLineFunctionsListGui(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            StaticParameter.instance.IsLineFunctionsFoldoutOpen =
                EditorGUILayout.Foldout(StaticParameter.instance.IsLineFunctionsFoldoutOpen, "Material Line Functions List", true);

            if (!StaticParameter.instance.IsLineFunctionsFoldoutOpen)
            {
                EditorGUILayout.Space();
                return;
            }
            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<MaterialLineFunctionsNode>(lineListNode,
                _propLineFunctionsList,
                verticalLayout.rect);

            using (verticalLayout)
            {
                _reorderableFunctionsList.HandleInputEventAndLayoutList();
            }
        }

        /// <summary>
        /// DoubleSidedMaterialListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateDoubleSidedMaterialListGui(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            StaticParameter.instance.IsDoubleSidedMaterialFoldoutOpen = 
                EditorGUILayout.Foldout(StaticParameter.instance.IsDoubleSidedMaterialFoldoutOpen, "Double Sided Material List", true);

            if (!StaticParameter.instance.IsDoubleSidedMaterialFoldoutOpen)
            {
                EditorGUILayout.Space();
                return;
            }

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, LineListNode>(
                lineListNode,
                null,
                _propDoubleSidedMaterials,
                verticalLayout.rect,
                (node, _) => ((LineListNode)node).DoubleSidedMaterials,
                false);

            using (verticalLayout)
            {
                _reorderableDoubleSidedMaterials.HandleInputEventAndLayoutList();
            }
        }

        /// <summary>
        /// IgnoreObjectListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateIgnoreObjectListGui(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            StaticParameter.instance.IsIgnoreObjectFoldoutOpen =
                EditorGUILayout.Foldout(StaticParameter.instance.IsIgnoreObjectFoldoutOpen, "Ignore Object List", true);

            if (!StaticParameter.instance.IsIgnoreObjectFoldoutOpen)
            {
                EditorGUILayout.Space();
                return;
            }

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<GameObject, LineListNode>(
                lineListNode,
                null,
                _propIgnoreObjectList,
                verticalLayout.rect,
                (node, _) => ((LineListNode)node).IgnoreObjectList,
                false);

            using (verticalLayout)
            {
                _reorderableIgnoreObjectList.HandleInputEventAndLayoutList();
            }

        }

        /// <summary>
        /// LineGroupListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateLineGroupListGui(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            StaticParameter.instance.IsLineGroupListFoldoutOpen = 
                EditorGUILayout.Foldout(StaticParameter.instance.IsLineGroupListFoldoutOpen, "Line Group List", true);

            if (!StaticParameter.instance.IsLineGroupListFoldoutOpen)
            {
                EditorGUILayout.Space();
                return;
            }
        
            var verticalLayout = new EditorGUILayout.VerticalScope(style);
            
            Common.DragAndDropNode<LineGroupNode>(
                lineListNode,
                _propLineGroupList,
                verticalLayout.rect);
            
            using (verticalLayout)
            {
                _reorderableLineGroupList.HandleInputEventAndLayoutList();
            }
        }


        /// <summary>
        /// TargetMaterialListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        private void CreateTargetMaterialListGui(GUIStyle style)
        {
            if (!StaticParameter.instance.IsLineFunctionsFoldoutOpen) return;
            
            var ev = Event.current;
            if (ev.type == EventType.Used)
            {
                return;
            }
            else if (ev.type == EventType.Layout)
            {
                LineFunctionsIndexChanged();
                if (_serializedLineFunctionsObject == null ||
                    _serializedLineFunctionsObject.targetObject == null ||
                    _reorderableFunctionsList.index == -1)
                {
                    _selectedTargetMaterial = null;
                    return;
                }

                var propFunctions = _propLineFunctionsList.GetArrayElementAtIndex(_reorderableFunctionsList.index);

                _selectedTargetMaterial = propFunctions.objectReferenceValue as GameObject;
            }
            else // ev.type == EventType.Repaint, KeyDown, etc.
            {
                if (_selectedTargetMaterial == null)
                {
                    return;
                }
            }

            var lineListNode = target as LineListNode;

            var label = _selectedTargetMaterial.name + " -> Target Material";
            EditorGUILayout.LabelField(label);

            _serializedLineFunctionsObject.Update();

            var verticalLayout = new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, MaterialLineFunctionsNode>(
                lineListNode,
                lineListNode.LineFunctionsList,
                _propTargetMaterials,
                verticalLayout.rect,
                (node, _) =>
                {
                    var functionsNode = node as MaterialLineFunctionsNode;
                    return functionsNode != null
                        ? functionsNode.TargetMaterials
                        : new List<Material>();
                },
                true);

            // 表示
            using (verticalLayout)
            {
                _reorderableTargetMaterialList.HandleInputEventAndLayoutList();
            }

            _serializedLineFunctionsObject.ApplyModifiedProperties();
        }
        

        private void CreateLineGroupObjectsListGui(GUIStyle style)
        {
            if (!StaticParameter.instance.IsLineGroupListFoldoutOpen) return;
            
            var ev = Event.current;
            if (ev.type == EventType.Used)
            {
                return;
            }
            else if (ev.type == EventType.Layout)
            {
                LineGroupsIndexChanged();
                if (_serializedLineGroupObject == null ||
                    _serializedLineGroupObject.targetObject == null ||
                    _reorderableLineGroupList.index == -1)
                {
                    _selectedLineGroup = null;
                    return;
                }

                var propLineGroup = _propLineGroupList.GetArrayElementAtIndex(_reorderableLineGroupList.index);
                _selectedLineGroup = propLineGroup.objectReferenceValue as GameObject;

            }
            else // ev.type == EventType.Repaint, KeyDown, etc.
            {
                if (_selectedLineGroup == null)
                {
                    return;
                }
            }

            var lineListNode = target as LineListNode;
            var labelText = _selectedLineGroup.name + " -> Objects";
            EditorGUILayout.LabelField(labelText);
            
            _serializedLineGroupObject.Update();
            var verticalLayout = new EditorGUILayout.VerticalScope(style);
            
            Common.DragAndDropObject<GameObject, LineGroupNode>(
                lineListNode,
                lineListNode.LineGroupList,
                _propLineGroupObjects,
                verticalLayout.rect,
                (node, _) =>
                {
                    var groupNode = node as LineGroupNode;
                    return groupNode != null
                        ? groupNode.Objects
                        : new List<GameObject>();
                },
                true);

            using (verticalLayout)
            {
                _reorderableLineGroupObjectsList.HandleInputEventAndLayoutList();
            }

            _serializedLineGroupObject.ApplyModifiedProperties();
        }
        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (Event.current.type == EventType.Repaint)
            {
                _reorderableLineList.OnRepaint();
                _reorderableFunctionsList.OnRepaint();
                if (_reorderableTargetMaterialList != null)
                {
                    _reorderableTargetMaterialList.OnRepaint();
                }
                _reorderableDoubleSidedMaterials.OnRepaint();
                _reorderableIgnoreObjectList.OnRepaint();
                _reorderableLineGroupList.OnRepaint();
                if (_reorderableLineGroupObjectsList != null)
                {
                    _reorderableLineGroupObjectsList.OnRepaint();
                }
            }
            
            var style = new GUIStyle {margin = new RectOffset(4, 8, 0, 4)};

            CreateLineListGui(style);

            CreateLineFunctionsListGui(style);

            CreateTargetMaterialListGui(style);

            CreateDoubleSidedMaterialListGui(style);

            CreateIgnoreObjectListGui(style);

            CreateLineGroupListGui(style);
            
            CreateLineGroupObjectsListGui(style);

            serializedObject.ApplyModifiedProperties();
            
            
            GUILayout.Space(10);
           

            using (new EditorGUILayout.VerticalScope(new GUIStyle(GUI.skin.box){ margin = new RectOffset(7, 9, 7, 7)}))
            {
                GUILayout.Label("Pencil+ 4 Bridge");

                using (new EditorGUILayout.HorizontalScope(new GUIStyle() { margin = new RectOffset(3, 3, 3, 3) }))
                {
                    if (_importMethodInfo == null || _exportMethodInfo == null)
                    {
                        EditorGUILayout.LabelField("Please install the Pencil+ 4 Bridge package.");
                    }
                    else
                    {
                        if (GUILayout.Button("Import"))
                        {
                            _importMethodInfo.Invoke(null, new object[] { target });
                        }

                        if (GUILayout.Button("Export"))
                        {
                            _exportMethodInfo.Invoke(null, new object[] { target });
                        }
                    }
                }
            }
        }

        
        
        /// <summary>
        /// LineFunctionIndexを変更したときに呼ぶ
        /// </summary>
        private void LineFunctionsIndexChanged()
        {
            var funcIndex = _reorderableFunctionsList.index;
            if (funcIndex < 0 ||
               funcIndex >= _propLineFunctionsList.arraySize)
            {
                return;
            }

            var propFunc = _propLineFunctionsList.GetArrayElementAtIndex(funcIndex);

            var currentLineFunc = propFunc.objectReferenceValue as GameObject;

            if (_oldLineFunc == currentLineFunc)
            {
                return;
            }
            _oldLineFunc = currentLineFunc;

            _serializedLineFunctionsObject =
                    currentLineFunc != null ?
                    new SerializedObject(currentLineFunc.GetComponent<MaterialLineFunctionsNode>()) :
                    null;
    
            if (_serializedLineFunctionsObject == null ||
                _serializedLineFunctionsObject.targetObject == null)
            {
                return;
            }

            _propTargetMaterials = _serializedLineFunctionsObject.FindProperty("TargetMaterials");

            _reorderableTargetMaterialList =
                Common.CreateMaterialList(
                    _serializedLineFunctionsObject,
                    _propTargetMaterials,
                    new List<Material>(),
                    selectedMaterials => 
                    {
                        _serializedLineFunctionsObject.Update();
                        _propTargetMaterials.AppendObjects(selectedMaterials);
                        _serializedLineFunctionsObject.ApplyModifiedProperties();
                    });

        }


        private void LineGroupsIndexChanged()
        {
            var idx = _reorderableLineGroupList.index;

            if (idx < 0 || _propLineGroupList.arraySize <= idx)
            {
                return;
            }

            var propLineGroup = _propLineGroupList.GetArrayElementAtIndex(idx);

            var currentGroup = propLineGroup.objectReferenceValue as GameObject;

            if (_oldLineGroup == currentGroup)
            {
                return;
            }

            _oldLineGroup = currentGroup;

            _serializedLineGroupObject = currentGroup != null ? 
                new SerializedObject(currentGroup.GetComponent<LineGroupNode>()) :
                null;

            if (_serializedLineGroupObject == null ||
                _serializedLineGroupObject.targetObject == null)
            {
                return;
            }

            _propLineGroupObjects = _serializedLineGroupObject.FindProperty("Objects");

            _reorderableLineGroupObjectsList =
                Common.CreateObjectList(
                    _serializedLineGroupObject,
                    _propLineGroupObjects,
                    ((LineListNode)target).LineGroupList
                        .Select(x => x.GetComponent<LineGroupNode>())
                        .Where(x => x != null)
                        .SelectMany(x => x.Objects),
                    selectedObjects =>
                    {
                        _serializedLineGroupObject.Update();
                        _propLineGroupObjects.AppendObjects(selectedObjects);
                        _serializedLineGroupObject.ApplyModifiedProperties();
                        _reorderableLineGroupObjectsList.index = _reorderableLineGroupObjectsList.count - 1;
                    });

        }

        /// <summary>
        /// MenuにLineListノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line List Node", priority = 20)]
        public static void OpenLineListNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<LineListNode>(menuCommand);
        }
    }
}


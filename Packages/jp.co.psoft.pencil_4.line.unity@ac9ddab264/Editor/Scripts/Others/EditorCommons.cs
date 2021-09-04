﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;
using UnityEditor.SceneManagement;

namespace Pcl4Editor
{

    public static class EditorCommons
    {
        private const string PencilLogoGUID = "805e8e4a471d37f47affd8d21dc9e40a";
        private const string SLSettingsGUID = "eb2ea87efec5de748920c53b4757fc76";
        
        public static Texture FindPencilLogoTexture()
        {
            var path = AssetDatabase.GUIDToAssetPath(EditorCommons.PencilLogoGUID);
            if (path != null)
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            
            var assetGUIDs = AssetDatabase.FindAssets("PencilLogo t:texture2D", null);
            if (assetGUIDs.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static string FindSLSettingsPath()
        {
            return AssetDatabase.GUIDToAssetPath(SLSettingsGUID) ??
                   AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SLSetting_x64", null).FirstOrDefault());
        }

        public static GameObject CreateNodeObjectFromMenu<T>(
            MenuCommand menuCommand,
            Type preferredParentType = null,
            string preferredParentNodeListName = null,
            string suffix = " ") where T : NodeBase
        {
            // 新規生成されたノードの親の設定
            var parent = menuCommand.context as GameObject;
            if (parent == null && preferredParentType != null)
            {
                // 複数のオブジェクトを選択した状態でエディタ上部メニューからノード生成を実行した場合、
                // 単一のノードが新規生成される。
                // 新規生成されたノードの親ノードとして最適なものが選択オブジェクト中にあれば、設定する
                parent = Selection.gameObjects
                    .FirstOrDefault(x => x.GetComponent(preferredParentType) != null);
            }

            //
            GameObject go = Pcl4EditorUtilities.CreateNodeObject<T>(parent ? parent.transform : null, suffix);

            //　親が新規生成されたノードを追加すべきノードリストを持つのならば、追加する
            if (parent != null && preferredParentType != null && preferredParentNodeListName != null)
            {
                var preferredParentComponent = parent.GetComponent(preferredParentType);
                if (preferredParentComponent != null)
                {
                    var nodeList = preferredParentType.GetField(preferredParentNodeListName).GetValue(preferredParentComponent) as List<GameObject>;
                    if (nodeList != null)
                    {
                        nodeList.Add(go);
                    }
                }
            }

            //
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;

            return go;
        }


        /// <summary>
        /// ノードリストを作成する
        /// </summary>
        /// <param name="serializedObj">シリアライズオブジェクト</param>
        /// <param name="propList">シリアライズプロパティ</param>
        /// <param name="currentNodeComponent">現在のノード</param>
        /// <param name="prefab">オブジェクト作成の基になるプレハブ</param>
        /// <param name="doubleClickDiffTime">ダブルクリックの間隔(ms)</param>
        /// <returns>作成されたリスト</returns>
        public static PencilReorderableList CreateReorderableNodeList<T>(
            SerializedObject serializedObj,
            SerializedProperty propList,
            Component currentNodeComponent,
            int doubleClickDiffTime = 500) where T : NodeBase
        {
            var reorderbleList =  new PencilReorderableList(serializedObj, propList)
            {
                headerHeight = 2,

                onAddCallback = (list) =>
                {
                    var pencilList = (PencilReorderableList)list;
                    pencilList.index = propList.arraySize++;

                    var element = propList.GetArrayElementAtIndex(pencilList.index);
                    var newNode = Pcl4EditorUtilities.CreateNodeObject<T>(currentNodeComponent.transform);
                    Undo.RegisterCreatedObjectUndo(newNode, "Create Node");

                    element.objectReferenceValue = newNode;
                    list.GrabKeyboardFocus();
                },
                onRemoveCallback = (list) =>
                {
                    var pencilList = (PencilReorderableList)list;

                    foreach (var index in pencilList.SelectedIndices)
                    {
                        var element = propList.GetArrayElementAtIndex(index);
                        if (element == null) continue;
                        
                        var obj = element.objectReferenceValue as GameObject;
                        if (obj != null)
                        {
                            pencilList.ObjectsToDestroy.Add(obj);
                        }
                    }

                    pencilList.WillDestroyGameObjectCallback = objects =>
                    {
                        if (Event.current.type != EventType.Repaint) return;
                        if (pencilList.ObjectsToDestroy.Count <= 0) return;
                        foreach (var obj in pencilList.ObjectsToDestroy)
                        {
                            if (obj == null || obj.transform.parent != currentNodeComponent.transform) continue;
                            
                            // シーン上の同種のノードで、リストの中に削除されるオブジェクトを含むものを探す
                            var otherReferenceNode = Resources.FindObjectsOfTypeAll<GameObject>()
                                .Select(x => x.GetComponent(currentNodeComponent.GetType()))
                                .FirstOrDefault(x => x != null && x != currentNodeComponent &&
                                                     ((List<GameObject>)x.GetType().GetField(propList.name).GetValue(x))
                                                     .Contains(obj));

                            if (otherReferenceNode == null)
                            {
                                // 他のノードリストから参照されていなければ、オブジェクトを削除する
                                Undo.DestroyObjectImmediate(obj);
                            }
                            else
                            {
                                // 他のノードリストから参照されていれば、親子関係を設定する
                                Undo.SetTransformParent(obj.transform, otherReferenceNode.transform, "");
                            }
                        }
                        pencilList.ObjectsToDestroy.Clear();
                    };
                    
                    foreach (var index in pencilList.SelectedIndices.Reverse())
                    {
                        var elem = propList.GetArrayElementAtIndex(index);
                        elem.objectReferenceValue = null;
                        propList.DeleteArrayElementAtIndex(index);
                    }

                    var lastIndex = pencilList.count - 1;
                    pencilList.index = pencilList.index >= lastIndex ? lastIndex : pencilList.index;
                }
            };

            reorderbleList.OnDoubleClickCallback += index =>
            {
                var propObj = propList.GetArrayElementAtIndex(index);
                Selection.activeObject = propObj.objectReferenceValue;
            };

            return reorderbleList;
        }


        /// <summary>
        /// GameObjectのリスト(追加・削除ボタン付き)を作成する
        /// </summary>
        /// <param name="serializedObject">インスペクタに表示中のオブジェクト</param>
        /// <param name="serializedProperty">リスト対象のプロパティ</param>
        /// <param name="objectsToExclude">除外リスト。このリストに入っているオブジェクトは"+"ボタンを押した時に表示されない</param>
        /// <param name="objectsWillAdd">オブジェクトが追加された時に呼ばれるデリゲート</param>
        /// <returns></returns>
        public static PencilReorderableList CreateObjectList(
            SerializedObject serializedObject,
            SerializedProperty serializedProperty,
            IEnumerable<GameObject> objectsToExclude,
            Action<List<GameObject>> objectsWillAdd)
        {
            return CreateReorderableList<GameObject>(serializedObject, serializedProperty,
                () =>
                {
                    ObjectPickerWindow.Open( 
                        Enumerable.Range(0, serializedProperty.arraySize)
                            .Select(i => (GameObject)serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue)
                            .Concat(objectsToExclude),
                        objectsWillAdd);
                });
        }

        /// <summary>
        /// Materialのリスト(追加・削除ボタン付き)を作成する
        /// </summary>
        /// <param name="serializedObject">インスペクタに表示中のオブジェクト</param>
        /// <param name="serializedProperty">リスト対象のプロパティ</param>
        /// <param name="materialsToExclude">除外リスト。このリストに入っているマテリアルは"+"ボタンを押した時に表示されない</param>
        /// <param name="materialsWillAdd">マテリアルが追加された時に呼ばれるデリゲート</param>
        /// <returns></returns>
        public static PencilReorderableList CreateMaterialList(
            SerializedObject serializedObject,
            SerializedProperty serializedProperty,
            IEnumerable<Material> materialsToExclude,
            Action<List<Material>> materialsWillAdd)
        {
            return CreateReorderableList<Material>(serializedObject, serializedProperty,
                () =>
                {
                    MaterialPickerWindow.Open(
                        Enumerable.Range(0, serializedProperty.arraySize)
                            .Select(i => (Material)serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue)
                            .Concat(materialsToExclude),
                        materialsWillAdd);
                });
        }



        /// <summary>
        /// ObjectList, MaterialListを作成する
        /// (CreateObjectList, CreateMaterialListから呼ばれる事を想定している)
        /// </summary>
        /// <typeparam name="T">列挙するオブジェクトの型</typeparam>
        /// <param name="serializedObj">表示対象のSerializedObject</param>
        /// <param name="propList">表示対象のSerializedProperty</param>
        /// <param name="pickerWillShow">ピッカーが表示される時に呼ばれるデリゲート</param>
        /// <param name="doubleClickDiffTimeMs">ダブルクリックの間隔</param>
        /// <returns></returns>
        private static PencilReorderableList CreateReorderableList<T>(
            SerializedObject serializedObj,
            SerializedProperty propList,
            Action pickerWillShow,
            int doubleClickDiffTimeMs = 500)
            where T : UnityEngine.Object
        {
            var reorderableList = new PencilReorderableList(serializedObj, propList)
                {
                    headerHeight = 2,

                    onRemoveCallback = (list) =>
                    {
                        var pencilList = (PencilReorderableList)list;
                        foreach (var index in pencilList.SelectedIndices.Reverse())
                        {
                            var elem = propList.GetArrayElementAtIndex(index);
                            elem.objectReferenceValue = null;
                            propList.DeleteArrayElementAtIndex(index);
                        }

                        var lastIndex = pencilList.count - 1;
                        pencilList.index = pencilList.index >= lastIndex ? lastIndex : pencilList.index;
                    }
                };
            
            
            reorderableList.onAddCallback += (list) =>
            {
                reorderableList.GrabKeyboardFocus();
                pickerWillShow();
            };
               

            reorderableList.OnDoubleClickCallback += index =>
            {
                var propObj = propList.GetArrayElementAtIndex(index);
                Selection.activeObject = propObj.objectReferenceValue;
            };

            return reorderableList;
        }


        /// <summary>
        /// ノードリストにドラッグアンドドロップ機能を追加
        /// </summary>
        /// <typeparam name="T">ノードリストのタイプ</typeparam>
        /// <param name="parentNode">親ノード</param>
        /// <param name="propList">ノードリスト</param>
        /// <param name="rect">ドラッグアンドドロップ可能範囲</param>
        public static void DragAndDropNode<T>(NodeBase parentNode,
                                              SerializedProperty propList,
                                              Rect rect)
            where T : NodeBase
        {

            var id = GUIUtility.GetControlID(FocusType.Passive);
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = id;

                    if (evt.type != EventType.DragPerform)
                    {
                        break;
                    }

                    DragAndDrop.AcceptDrag();

                    var draggedList =
                        DragAndDrop.objectReferences.ToList();

                    // 条件に合わないノード
                    var exclusionList =
                        draggedList.Where(x => x.GetType() != typeof(GameObject) ||
                                          (x as GameObject).GetComponent<T>() == null)
                                   .ToList();

                    foreach (var item in exclusionList)
                    {
                        Debug.LogWarning("\"" + item.name + "\" is not " + typeof(T).Name);
                    }

                    // 条件に合うノード
                    for (int i = 0; i < propList.arraySize; i++)
                    {
                        // すでにリストに含まれているオブジェクトは除外する
                        var element = propList.GetArrayElementAtIndex(i).objectReferenceValue;
                        draggedList.Remove(element);
                    }

                    var addItemList =
                        draggedList.OfType<GameObject>()
                                   .Where(x => x.GetComponent<T>());

                    // 条件に合ったノードをリストに追加
                    propList.AppendObjects(addItemList.ToList());

                    DragAndDrop.activeControlID = 0;

                    Event.current.Use();
                    break;
            }
        }



        public static void DragAndDropObject<T, U>(NodeBase parentNode,
                                                List<GameObject> parentList,
                                                SerializedProperty propList,
                                                Rect rect,
                                                GetObjectList<T> getListDelegate,
                                                bool checkOtherNode)
            where T : UnityEngine.Object
            where U : NodeBase
        {
            if (!rect.Contains(Event.current.mousePosition)) return;
            
            if (Event.current.type != EventType.DragUpdated && Event.current.type != EventType.DragPerform) return;


            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            DragAndDrop.activeControlID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.AcceptDrag();

            // 追加処理

            // propertyからlistにコピー
            var list = new List<T>();
            for (var i = 0; i < propList.arraySize; i++)
            {
                var propObj = propList.GetArrayElementAtIndex(i);
                list.Add(propObj.objectReferenceValue as T);
            }

            // GameObjectかそれ以外で処理を変更する
            var draggedList = typeof(T) == typeof(GameObject) ?
                new List<T>() :
                DragAndDrop.objectReferences.OfType<T>().ToList();

            var draggedObjList =
                DragAndDrop.objectReferences.OfType<GameObject>().ToList();

            var draggedAllList = new List<T>();
            foreach (var item in draggedObjList)
            {
                var objs = new List<T>();

                if(typeof(T) == typeof(GameObject))
                {
                    objs = item.GetMeshObjectList() as List<T>;
                }
                else if(typeof(T) == typeof(Material))  // FIXME: TがMaterialを継承したクラスでも大丈夫?
                {
                    objs = item.GetMeshMaterialList() as List<T>;
                }
                draggedAllList.AddRange(objs.ToList());
            }

            draggedList.AddRange(draggedAllList.Distinct().ToList());

            // 他ノードとの重複チェック
            if (checkOtherNode)
            {
                draggedList = CheckOtherNode<T, U>(
                    parentList,
                    draggedList.Distinct().ToList(),
                    propList.name,
                    getListDelegate);
            }

            // 追加
            list.AddRange(draggedList);

            if (!checkOtherNode)
            {
                // 現在のリスト内で被ったら警告表示
                var overlappedList = list.CheckOverlapped();
                foreach (var item in overlappedList)
                {
                    Debug.LogWarning("\"" + item.name +
                                     "\" is already added in \"" +
                                     parentNode.name + "\"");
                }
                list = list.Distinct().ToList();
            }

            // Null除去
            list = list.Where(x => x != null).ToList();

            // propertyで反映
            propList.arraySize = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                var propObj = propList.GetArrayElementAtIndex(i);
                propObj.objectReferenceValue = list[i];
            }

            DragAndDrop.activeControlID = 0;

            Event.current.Use();
        }


        public delegate List<T> GetObjectList<T>(NodeBase node, string varName);


        private static List<T> CheckOtherNode<T, U>(IEnumerable<GameObject> ObjList,
                                                   IList<T> checkList,
                                                   string varName,
                                                   GetObjectList<T> getListDelegate)
            where T : UnityEngine.Object
            where U : NodeBase
        {
            foreach (var obj in ObjList)
            {
                var node = obj.GetComponent<U>();
                if (node == null)
                {
                    continue;
                }

                var currentList = getListDelegate(node, varName);

                for (var i = 0; i < checkList.Count; i++)
                {
                    if (checkList[i] == default(T))
                    {
                        continue;
                    }

                    var list = new List<T>(currentList);

                    list = list.Where(x => x == checkList[i]).ToList();
                    if (list.Count != 1)
                    {
                        continue;
                    }

                    Debug.LogWarning("\"" + checkList[i].name +
                                        "\" is already added in \"" +
                                        obj.name + "\"");

                    checkList[i] = default(T);
                }

            }

            return checkList.Where(x => x != null).ToList();
        }
        
    }
}

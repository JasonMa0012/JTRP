﻿using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{
    public static class EditorExtensions
    {

        /// <summary>
        /// 指定したコンポーネントを持っている要素を検索する
        /// </summary>
        /// <param name="T">フィルタリングするコンポーネントの型</param>
        /// <param name="self"></param>
        /// <returns>指定したコンポーネントを持っている要素リスト</returns>
        public static List<GameObject> SearchComponent<T>(this List<GameObject> self)
        {
            List<GameObject> result = self.Where(x => x.GetComponent<T>() != null)
                                          .ToList();

            return result;
        }


        /// <summary>
        /// Suffix付きの唯一の名前を取得
        /// </summary>
        /// <param name="allGameObjs">シーン内すべてのオブジェクト</param>
        /// <param name="prefab">Object名取得用のプレハブ</param>
        /// <param name="suffixName">接尾に付加する文字列</param>
        /// <returns> 唯一の名前 </returns>
        public static string GetUniqueName(this List<GameObject> allGameObjs,
                                           GameObject prefab,
                                           string suffixName = " ")
        {
            string name;
            for (var i = 0; ; ++i)
            {
                name = (prefab.name + suffixName + i);
                var result = allGameObjs.Find(c => c.name == name);
                if (result == null)
                {
                    return name;
                }
            }
        }


        

        /// <summary>
        /// Listから重複している要素を取得
        /// </summary>
        /// <typeparam name="T">リストのタイプ</typeparam>
        /// <returns>重複している要素リスト</returns>
        public static List<T> CheckOverlapped<T>(this List<T> self)
            where T : UnityEngine.Object
        {
            List<T> overlappedList = new List<T>();
            List<T> uniqueList = new List<T>();
            foreach (var item in self)
            {
                if (item != null && uniqueList.Contains(item))
                {
                    overlappedList.Add(item);
                }
                else
                {
                    uniqueList.Add(item);
                }
            }
            return overlappedList;
        }

        /// <summary>
        /// プレハブ名から"Pencil+ "を除いた文字列を取得
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static String GetNodeNameLabel(this String self)
        {
            return self.Replace("Pencil+ ", "");
        }

        /// <summary>
        /// Objectフィールドに適用しないObjectが入った場合元に戻す処理を行うメソッド
        /// </summary>
        /// <typeparam name="T">チェックを行う型</typeparam>
        /// <param name="propObj">チェックする対象となるGameObject</param>
        /// <param name="beforeObject">変更前のGameObject</param>
        public static SerializedProperty UndoObject<T>(this SerializedProperty self,
                                                       GameObject beforeObject)
        {
            // Tに指定された型のコンポーネントが存在しなかったら元に戻す
            GameObject obj = (GameObject)self.objectReferenceValue;
            if (obj != null && obj.GetComponent<T>() == null)
            {
                self.objectReferenceValue = beforeObject;
            }
            return self;
        }


        /// <summary>
        /// MeshRendererかSkinnedMeshRenderを持っているか
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>Meshを持っているオブジェクトリスト</returns>
        public static List<GameObject> GetMeshObjectList(this GameObject self)
        {
            Func<GameObject, List<GameObject>, List<GameObject>> getMeshObjects = null;
            getMeshObjects = (gameObj, list) =>
            {
                if (gameObj == null)
                {
                    return list;
                }

                MeshRenderer mr = gameObj.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    list.Add(gameObj);
                }

                SkinnedMeshRenderer smr = gameObj.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    list.Add(gameObj);
                }

                List<GameObject> children = new List<GameObject>();
                int childCount = gameObj.transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    children.Add(gameObj.transform.GetChild(i).gameObject);
                }

                foreach (var child in children)
                {
                    list = getMeshObjects(child, list);
                }
                return list;
            };

            return getMeshObjects(self, new List<GameObject>());

        }


        /// <summary>
        /// MeshRendererとSkinnedMeshRenderが持っているマテリアルを全て取得
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>MeshのMaterialリスト</returns>
        public static List<Material> GetMeshMaterialList(this GameObject self)
        {

            Func<GameObject, List<Material>, List<Material>> getMeshMaterials = null;
            getMeshMaterials = (gameObj, list) =>
            {
                if (gameObj == null)
                {
                    return list;
                }

                MeshRenderer mr = gameObj.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    list.AddRange(mr.sharedMaterials);
                }

                SkinnedMeshRenderer smr = gameObj.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    list.AddRange(smr.sharedMaterials);
                }

                List<GameObject> children = new List<GameObject>();
                int childCount = gameObj.transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    children.Add(gameObj.transform.GetChild(i).gameObject);
                }

                foreach (var child in children)
                {
                    list = getMeshMaterials(child, list);
                }
                return list;
            };

            return getMeshMaterials(self, new List<Material>());
        }

        /// <summary>
        /// シーン中のGameObjectのフルパスを取得
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>GameObjectのフルパス(区切り文字は"/")</returns>
        public static string FullPath (this GameObject gameObject)
        {
            var currentObject = gameObject;
            var objectsToJoin = new List<GameObject> { currentObject };
            while(currentObject.transform.parent != null)
            {
                currentObject = currentObject.transform.parent.gameObject;
                objectsToJoin.Add(currentObject);
            }
            return string.Join("/", objectsToJoin.Select(x => x.name).Reverse().ToArray());
        }
    }


    public static class SerializePropertyExtensions
    {
        /// <summary>
        /// Array型のSerializedPropertyの末尾にオブジェクトの列を結合する
        /// </summary>
        /// <typeparam name="T">UnityEngine.Objectを継承する任意の型</typeparam>
        /// <param name="property">結合対象のプロパティ</param>
        /// <param name="objects">結合するオブジェクト列</param>
        public static void AppendObjects<T> (this SerializedProperty property, List<T> objects) where T : UnityEngine.Object
        {
            if(!property.isArray)
            {
                throw new InvalidOperationException("SerializeProperty.AddObjects: Property is not array.");
            }

            var oldArraySize = property.arraySize;
            property.arraySize += objects.Count;
            for (var i = 0; i < objects.Count; ++i)
            {
                var propObj = property.GetArrayElementAtIndex(i + oldArraySize);
                propObj.objectReferenceValue = objects[i];
            }
        }
    }
}

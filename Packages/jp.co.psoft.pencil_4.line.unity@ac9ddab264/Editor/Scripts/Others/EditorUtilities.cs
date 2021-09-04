using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pencil_4
{
    public static class Pcl4EditorUtilities
    {
        /// <summary>
        /// プロジェクトから条件に一致するアセットを検索し、検索結果の先頭を返す。
        /// エディタ上でのみ動作する。
        /// </summary>
        /// <typeparam name="T">検索するオブジェクトの型</typeparam>
        /// <param name="filter">検索条件</param>
        /// <param name="name">アセットの名前</param>
        /// <returns>検索結果の先頭（見つからなかった場合はnull）</returns>
        public static UnityEngine.Object FindAssetInProjectOnEditor(string filter, string name)
        {
            UnityEngine.Object ret = null;

            if (!string.IsNullOrEmpty(name))
            {
                var materialList = UnityEditor.AssetDatabase.FindAssets(string.Format("{0} {1}", filter, name));
                if (materialList.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(materialList[0]);
                    ret = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                }
            }

            return ret;
        }

        public static GameObject CreateNodeObject<T>(Transform parent, string suffix = " ") where T : NodeBase
        {
            var ret = NodeBase.CreateNodeObject<T>(suffix);
            Place(ret, parent ? parent.gameObject : null);
            return ret;
        }

        public static GameObject CreateNodeObjectFromType(Transform parent, Type type)
        {
            var ret = NodeBase.CreateNodeObjectFromType(type);
            Place(ret, parent ? parent.gameObject : null);
            return ret;
        }

        public static void Place(GameObject go, GameObject parent, bool keepSelection = true)
        {
            var t = Type.GetType("UnityEditor.GOCreationCommands, UnityEditor");
            if (t != null)
            {
                MethodInfo mi = t.GetMethod("Place", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (mi != null && mi.GetParameters().Length == 2)
                {
                    GameObject activeObject = null;
                    if (keepSelection)
                    {
                        activeObject = Selection.activeGameObject;
                    }

                    mi.Invoke(null, new object[] { go, parent });
                    
                    if (keepSelection)
                    {
                        Selection.activeGameObject = activeObject;
                    }

                    return;
                }
            }

            go.transform.parent = parent ? parent.transform : null;
        }
    }
}
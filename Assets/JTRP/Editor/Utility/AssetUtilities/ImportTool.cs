using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace JTRP.Utility
{
    public class ImportUtility
    {
        [MenuItem("JTRP/Import Scene Model")]
        static void ImportModel()
        {
            var selectGUIDs = Selection.assetGUIDs;
            var selectOBJ = Selection.activeObject;

            string path = AssetDatabase.GUIDToAssetPath(selectGUIDs[0]);
            var targetDir = Path.GetDirectoryName(path) + "/" + selectOBJ.name + "_Import";
            string meshDir = targetDir + "/Mesh", matDir = targetDir + "/Material";
            var s = Directory.GetCurrentDirectory() + "/";
            if (Directory.Exists(s + targetDir))
            {
                Debug.LogError("selectOBJ.name" + "_Import 文件夹已存在，请先删除");
                return;
            }
            else
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(path), selectOBJ.name + "_Import");
                AssetDatabase.CreateFolder(targetDir, "Mesh");
                AssetDatabase.CreateFolder(targetDir, "Material");
            }

            // 复制出材质球、mesh
            var allSubAsset = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            var mats = GetModelAssets<Material>(allSubAsset);
            mats = CopyModelAsset<Material>(mats, matDir, ".mat");
            var meshs = GetModelAssets<Mesh>(allSubAsset);
            meshs = CopyModelAsset<Mesh>(meshs, meshDir, ".asset");

            Debug.Log(meshs.Count);

            // 实例化go，将复制后的材质、mesh赋值给go
            var go = GameObject.Instantiate(selectOBJ) as GameObject;
            go.name = selectOBJ.name;
            foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {

                Debug.Log(renderer.sharedMesh.name);

                renderer.sharedMesh = meshs[renderer.sharedMesh.name];
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.sharedMaterials[i] = mats[renderer.sharedMaterials[i].name];
                }
            }
            foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>())
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.sharedMaterials[i] = mats[renderer.sharedMaterials[i].name];
                }
            }
            foreach (var renderer in go.GetComponentsInChildren<MeshFilter>())
            {
                renderer.sharedMesh = meshs[renderer.sharedMesh.name];
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(go, targetDir + "/" + go.name + ".prefab", InteractionMode.UserAction);
        }

        [MenuItem("JTRP/Import Scene Model", true)]
        static bool ValidateImportModel()
        {
            var selectGUIDs = Selection.assetGUIDs;
            if (selectGUIDs.Length == 0)
            {
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(selectGUIDs[0]);
            var extension = Path.GetExtension(path);
            if (extension != ".fbx" && extension != ".FBX")
            {
                return false;
            }
            return true;
        }


        static Dictionary<string, T> GetModelAssets<T>(Object[] objs) where T : Object
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            foreach (var item in objs)
            {
                if (item is T)
                {
                    var go = Object.Instantiate(item);

                    go.name = item.name;
                    dic.Add(item.name, go as T);
                }
            }
            return dic;
        }

        static Dictionary<string, T> CopyModelAsset<T>(Dictionary<string, T> assets, string targetDir, string extension) where T : Object
        {
            var list = new List<T>();
            foreach (var item in assets)
            {
                var p = targetDir + "/" + item.Value.name + extension;
                AssetDatabase.CreateAsset(item.Value, p);
                AssetDatabase.ImportAsset(p);
                list.Add(AssetDatabase.LoadAssetAtPath<T>(p));
            }
            return GetModelAssets<T>(list.ToArray());
        }
    }
}// namespace JTRP.Utility

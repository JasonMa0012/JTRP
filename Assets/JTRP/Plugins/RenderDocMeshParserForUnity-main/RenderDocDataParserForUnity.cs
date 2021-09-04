#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Windsmoon.Tools
{
    public static class RenderDocDataParserForUnity
    {
        #region methods
        [MenuItem("Windsmoon/Tools/Parse Mesh Data")]
        public static void ParseMeshData()
        {
            StreamReader sr; 
            string path = LoadCSV(out sr);
            sr.ReadLine(); // pass the title
            List<string> stringDataList = new List<string>();
            
            while (!sr.EndOfStream)
            {
                string tempData = sr.ReadLine();
                tempData = tempData.Replace(" ", "");
                tempData.Replace("\r", "");
                tempData.Replace("\n", "");
                stringDataList.Add(tempData);
            }
            
            List<VertexData> vertexDataList = new List<VertexData>();

            // VTX, IDX, POSITION.x, POSITION.y, POSITION.z, NORMAL.x, NORMAL.y, NORMAL.z, TEXCOORD0.x, TEXCOORD0.y
            foreach (var stringData in stringDataList)
            {
                string[] datas = stringData.Split(',');
                VertexData vertexData = new VertexData();
                vertexData.index = int.Parse(datas[1]);
                vertexData.Position = new Vector3(float.Parse(datas[2]), float.Parse(datas[3]), float.Parse(datas[4]));
                vertexData.Normal = new Vector3(float.Parse(datas[5]), float.Parse(datas[6]), float.Parse(datas[7]));
                vertexData.UV = new Vector2(float.Parse(datas[8]), float.Parse(datas[9]));
                vertexDataList.Add(vertexData);
            }

            // construct mesh
            int maxIndex = FindMaxIndex(vertexDataList);
            int vertexArrayCount = maxIndex + 1;
            Vector3[] vertices = new Vector3[vertexArrayCount];
            Vector3[] normals = new Vector3[vertexArrayCount];
            int[] triangles = new int[vertexDataList.Count];
            Vector2[] uvs = new Vector2[vertexArrayCount];
            
            // fill mesh data
            // ?? why hash set has not the capcity property
            Dictionary<int, int> flagDict = new Dictionary<int, int>(vertexArrayCount);;
            
            for (int i = 0; i < vertexDataList.Count; ++i)
            {
                VertexData vertexData = vertexDataList[i];
                int index = vertexData.index;
                triangles[i] = index;
                
                if (flagDict.ContainsKey(index))
                {
                    continue;
                }

                flagDict.Add(index, 1);
                vertices[index] = vertexData.Position;
                normals[index] = vertexData.Normal;
                uvs[index] = vertexData.UV;
            }
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            AssetDatabase.CreateAsset(mesh, "Assets/" + System.IO.Path.GetFileNameWithoutExtension(path) + "_" + System.DateTime.Now.Ticks + ".mesh");
            AssetDatabase.SaveAssets();
        }

        private static int FindMaxIndex(List<VertexData> vertexDataList)
        {
            int maxIndex = 0;
            
            foreach (VertexData vertexData in vertexDataList)
            {
                int currentIndex = vertexData.index;

                if (currentIndex > maxIndex)
                {
                    maxIndex = currentIndex;
                }
            }

            return maxIndex;
        }
        
        private static string LoadCSV(out StreamReader sr)
        {
            string csvPath = EditorUtility.OpenFilePanel("select mesh data in csv", String.Empty, "csv");
            sr = new StreamReader(new FileStream(csvPath, FileMode.Open));
            return csvPath;
        }
        #endregion
        
        
        #region structs
        struct VertexData
        {
            #region fields
            public int index;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
            #endregion
        }
        #endregion
    }
}
#endif
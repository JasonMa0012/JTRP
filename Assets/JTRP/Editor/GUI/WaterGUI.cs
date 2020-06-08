using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JTRP.Editor
{
    [CustomEditor(typeof(Water))]
    public class WaterGUI : UnityEditor.Editor
    {
        class WaterDebug : EditorWindow
        {
            public static Texture texture;
            public static void ShowWindow(Texture tex)
            {
                texture = tex;
                GetWindow<WaterDebug>(true);
            }
            private void OnGUI()
            {
                if (texture)
                    GUILayout.Box(texture);
            }
            private void OnDisable()
            {
                Texture.DestroyImmediate(texture);
            }

        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(20f);
            if (GUILayout.Button("Bake Mesh"))
            {
                DoBake();
            }
        }

        public void DoBake()
        {
            // get obj
            var planeRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();
            var planeScale = planeRenderer.transform.localScale;
            planeRenderer.transform.localScale = Vector3.one;
            var planeBound = planeRenderer.bounds;
            var sceneRenderer = GameObject.FindObjectsOfType<MeshRenderer>();
            List<MeshRenderer> renderersList = new List<MeshRenderer>();
            foreach (var item in sceneRenderer)
            {
                if (item == planeRenderer)
                    continue;
                if (item.bounds.Intersects(planeBound) && !item.GetComponent<ReflectionProbe>())
                {
                    renderersList.Add(item);
                }
            }

            Debug.Log(renderersList.Count);

            // do culling
            ComputeShader cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("b4160d9aa0181db4fa8bd65a5d119bf8"));
            var index = cs.FindKernel("Culling");
            Mesh resultMesh = Object.Instantiate(planeRenderer.GetComponent<MeshFilter>().sharedMesh);
            resultMesh.SetUVs(0, new Vector2[resultMesh.vertexCount]);

            List<CombineInstance> instanceList = new List<CombineInstance>();
            instanceList.Add(new CombineInstance
            {
                mesh = Object.Instantiate(resultMesh),
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            });

            foreach (var item in renderersList)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                var mesh = item.GetComponent<MeshFilter>().sharedMesh;
                instanceList.Add(new CombineInstance
                {
                    mesh = DoCulling(cs, mesh, item.localToWorldMatrix, item.worldToLocalMatrix, planeBound.center.y, index),
                    subMeshIndex = 0,
                    transform = planeRenderer.worldToLocalMatrix
                });
            }
            resultMesh.CombineMeshes(instanceList.ToArray());
            MeshUtility.Optimize(resultMesh);
            resultMesh.RecalculateNormals();
            resultMesh.RecalculateBounds();
            ExportMesh(resultMesh);
            planeRenderer.transform.localScale = planeScale;
        }

        struct Triangle
        {
            public Vector3 v1, v2, v3;
            public Vector2 uv1, uv2, uv3;
        }
        Mesh DoCulling(ComputeShader cs, Mesh mesh, Matrix4x4 M2W, Matrix4x4 W2M, float h, int index = 0)
        {
            int count = mesh.vertexCount;
            int triCount = mesh.triangles.Length / 3;
            // 容量为现有顶点或三角形总量
            ComputeBuffer cbVertices = new ComputeBuffer(count, sizeof(float) * 3);
            ComputeBuffer cbNormals = new ComputeBuffer(count, sizeof(float) * 3);
            ComputeBuffer cbTriangles = new ComputeBuffer(triCount, sizeof(int) * 3);
            // 一个三角形若与平面相交则生成新生成两个三角形，所以最大容量为现有三角形的2倍
            ComputeBuffer cbResultTriangles = new ComputeBuffer(triCount * 2, sizeof(float) * 5 * 3, ComputeBufferType.Append);

            ComputeBuffer trianglesArgBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.Raw);

            cbVertices.SetData(mesh.vertices);
            cbNormals.SetData(mesh.normals);
            cbTriangles.SetData(mesh.triangles);

            cbResultTriangles.SetCounterValue(0);

            cs.SetFloat("h", h);
            cs.SetMatrix("M2W", M2W);
            cs.SetMatrix("W2M", W2M);
            cs.SetBuffer(index, "vertices", cbVertices);
            cs.SetBuffer(index, "normals", cbNormals);
            cs.SetBuffer(index, "triangles", cbTriangles);
            cs.SetBuffer(index, "resultTriangles", cbResultTriangles);

            cs.Dispatch(index, triCount, 1, 1);

            int[] trianglesCount = new int[] { 0 };

            ComputeBuffer.CopyCount(cbResultTriangles, trianglesArgBuffer, 0);

            trianglesArgBuffer.GetData(trianglesCount);

            Triangle[] resultTriangles = new Triangle[trianglesCount[0]];

            cbResultTriangles.GetData(resultTriangles);

            List<Vector3> vertList = new List<Vector3>();
            List<Vector2> uvList = new List<Vector2>();
            List<int> triList = new List<int>();
            Dictionary<Vector3, int> vertDic = new Dictionary<Vector3, int>();


            // per triangle
            for (int i = 0; i < resultTriangles.Length; i++)
            {
                var tri = resultTriangles[i];
                RecombinationTriangle(tri.v1, tri.uv1, triList, vertList, uvList, vertDic);
                RecombinationTriangle(tri.v2, tri.uv2, triList, vertList, uvList, vertDic);
                RecombinationTriangle(tri.v3, tri.uv3, triList, vertList, uvList, vertDic);
            }

            Mesh resultMesh = new Mesh();
            resultMesh.SetVertices(vertList.ToArray());
            resultMesh.SetUVs(0, uvList.ToArray());
            resultMesh.SetTriangles(triList.ToArray(), 0);

            cbVertices.Release();
            cbNormals.Release();
            cbTriangles.Release();
            cbResultTriangles.Release();
            trianglesArgBuffer.Release();


            return resultMesh;
        }

        /*
        若顶点存在于Dic
            从Dic取Index存入tri list
        否则
            将顶点存入vert list
            将顶点和vert index存入Dic
            Index存入tri list
        */
        void RecombinationTriangle(Vector3 point, Vector2 uv, List<int> triList, List<Vector3> vertList, List<Vector2> uvList, Dictionary<Vector3, int> vertDic)
        {
            if (vertDic.ContainsKey(point))
            {
                triList.Add(vertDic[point]);
            }
            else
            {
                vertList.Add(point);
                uvList.Add(uv);
                vertDic.Add(point, vertList.Count - 1);
                triList.Add(vertList.Count - 1);
            }
        }

        void ExportMesh(Mesh mesh)
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Select Export Folder", "NewWaterMesh", "mesh", "Baking finish");

            if (string.IsNullOrEmpty(savePath))
                return;

            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.Refresh();
        }

    }
}

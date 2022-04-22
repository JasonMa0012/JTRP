using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JTRP.Utility
{
    public enum NormalMode
    {
        Normal,
        VertexColor,
        VCOutline
    }

    public class ShowNormal : MonoBehaviour
    {
        [Range(0.0001f, 0.1f)]
        public float tbnLen = 0.01f;
        [Range(0, 100000)]
        public int maxShowNum = 10000;
        public bool showNormal = true;
        public bool showTangent = true;
        public bool showBiTangent = true;
        public NormalMode _mode;

        Mesh sharedMesh;

        Matrix4x4 localToWorld;
        Matrix4x4 localToWorldInverseTranspose;

        Vector3[] vertices;
        Vector3[] normals;
        Vector4[] tangents;
        Vector3[] biTangents;
        Vector3[] tangentsData;

        private void OnValidate()
        {
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                var mesh = GetComponent<SkinnedMeshRenderer>();
                sharedMesh = mesh.sharedMesh;
                localToWorld = mesh.transform.localToWorldMatrix;
            }
            else
            {
                sharedMesh = meshFilter.sharedMesh;
                localToWorld = meshFilter.transform.localToWorldMatrix;
            }
            /*
             * localToWorld 将 顶点位置 从模型坐标系转到世界坐标系矩阵
             * localToWorldInverseTranspose 将 向量 从模型坐标系转到世界坐标系矩阵
             *      1、切向量t和副切向量b 由于方向与纹理坐标系一致 使用localToWorld和localToWorldInverseTranspose矩阵转换到世界坐标系 结果相同
             *      2、normal 由于模型有非等比缩放的情况，缩放后顶点的法向量使用localToWorld矩阵转换的结果不正确
             *      设矩阵M为切向量t的转换矩阵,矩阵G为法向量n的转换矩阵,
             *      转换后的切向量且t2 = M*t， 转换后的法向量n2 = G*n，同时要求 n2 * t2 = 0
             *      所以  (G*n)' * (M*t) = 0  =>  n'*G'*M*t = 0  (n'表示向量n的转置, G'表示矩阵G的转置)
             *      已知 n'*t = 0(法向量和切向量垂直)， 此时如果令 G'*M = I(单位矩阵)
             *      则有 n'*G'*M*t = n'*I*t = n'*t = 0 成立
             *      可得 G'*M = I => G = (inverse(M))'
             */

            localToWorldInverseTranspose = localToWorld.inverse.transpose;

            vertices = sharedMesh.vertices;
            normals = sharedMesh.normals;
            tangents = sharedMesh.tangents;

            int tangentsLen = (tangents != null ? tangents.Length : 0);
            biTangents = new Vector3[tangentsLen];
            tangentsData = new Vector3[tangentsLen];
            var c = sharedMesh.colors;
            for (int i = 0; i < tangentsLen; i++)
            {
                //切向量数据 Vector4 转 Vector3
                tangentsData[i].x = tangents[i].x;
                tangentsData[i].y = tangents[i].y;
                tangentsData[i].z = tangents[i].z;
                //计算副切线 cross(法向量，切向量)*坐标系方向参数
                biTangents[i] = Vector3.Cross(normals[i], tangentsData[i]) * tangents[i].w;

                if (_mode == NormalMode.VertexColor)
                {
                    normals[i] = new Vector3(c[i].r * 2 - 1, c[i].g * 2 - 1, c[i].b * 2 - 1);
                }
                else if (_mode == NormalMode.VCOutline)
                {
                    Vector3 n = new Vector3();
                    n.x = c[i].r * 2 - 1;
                    n.y = c[i].g * 2 - 1;
                    n.z = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(new Vector2(n.x, n.y), new Vector2(n.x, n.y))));
                    n = n.normalized;
                    var m = new Matrix4x4(
                        tangents[i],
                        new Vector4(biTangents[i].x, biTangents[i].y, biTangents[i].z, 0),
                        new Vector4(n.x, n.y, n.z, 0),
                        Vector4.zero
                    );
                    normals[i] = m.MultiplyVector(n).normalized;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (showNormal) DrawVectors(vertices, normals, ref localToWorld, ref localToWorldInverseTranspose, Color.red, tbnLen);
            if (showTangent) DrawVectors(vertices, tangentsData, ref localToWorld, ref localToWorld, Color.green, tbnLen);
            if (showBiTangent) DrawVectors(vertices, biTangents, ref localToWorld, ref localToWorld, Color.blue, tbnLen);
        }

        /*显示向量
         * vertexs 向量初始位置
         * vectors 向量方向
         * vertexMatrix 向量初始位置从模型坐标系转到世界坐标系矩阵
         * vectorMatrix 向量方向从模型坐标系转到世界坐标系矩阵
         * color 向量颜色
         * */
        void DrawVectors(Vector3[] vertexs, Vector3[] vectors, ref Matrix4x4 vertexMatrix, ref Matrix4x4 vectorMatrix, Color color, float vectorLen)
        {
            Gizmos.color = color;
            int len = (vertexs == null || vectors == null ? 0 : vertexs.Length);
            len = Mathf.Min(len, maxShowNum);
            if (vertexs.Length != vectors.Length)
            {
                Debug.LogError("vertexs lenght not equal vectors length!!!");
                return;
            }
            for (int i = 0; i < len; i++)
            {
                Vector3 vertexData = vertexMatrix.MultiplyPoint(vertexs[i]);
                Vector3 vectorData = vectorMatrix.MultiplyVector(vectors[i]);
                vectorData.Normalize();
                Gizmos.DrawLine(vertexData, vertexData + vectorData * vectorLen);
            }
        }
    }
}// namespace JTRP.Utility

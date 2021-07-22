using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using static Unity.Mathematics.math;
using Unity.Mathematics;
using JTRP.Utility;

namespace JTRP.CustomAssetPostprocessor
{
    public class ModelOutlineImporter : AssetPostprocessor
    {
        /// <summary>
        /// 将平滑后的法线烘焙到UV8中
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct BakeNormalJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> vertrx, normals;
            [ReadOnly] public NativeArray<float4> tangents;
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly] public UnsafeHashMap<float3, float3> smoothedNormals;
            [WriteOnly] public NativeArray<float2> uv8;

            public BakeNormalJob(NativeArray<float3> vertrx,
                NativeArray<float3> normals,
                NativeArray<float4> tangents,
                UnsafeHashMap<float3, float3> smoothedNormals,
                NativeArray<float2> uv8)
            {
                this.vertrx = vertrx;
                this.normals = normals;
                this.tangents = tangents;
                this.smoothedNormals = smoothedNormals;
                this.uv8 = uv8;
            }

            void IJobParallelFor.Execute(int index)
            {
                float3 smoothedNormal = normalizesafe(smoothedNormals[vertrx[index]]);

                var bitangent = normalizesafe(cross(normals[index], tangents[index].xyz) * tangents[index].w);

                var tbn = new float3x3(
                    tangents[index].xyz,
                    bitangent,
                    normals[index]
                );

                var bakedNormal = normalizesafe(mul(smoothedNormal, tbn));
                uv8[index] = bakedNormal.xy;
            }
        }

        // 在GameObject生成后调用，对GameObject的修改会影响生成结果，但引用不会保留
        // 所有导入的模型均会做此操作，可以根据需要进行处理
        void OnPostprocessModel(GameObject g)
        {
            Dictionary<string, Mesh> originMeshDic = GetMesh(g);

            foreach (var item in originMeshDic)
            {
                var originMesh = item.Value;
                ComputeSmoothedNormalByJob(originMesh);
            }
        }
        Dictionary<string, Mesh> GetMesh(GameObject go)
        {
            void AddMesh(Dictionary<string, Mesh> dictionary, string name, Mesh mesh)
            {
                if (dictionary.ContainsKey(name))
                    LogWarning($"模型名称'{name}'重复！");
                else
                    dictionary.Add(name, mesh);
            }
            Dictionary<string, Mesh> dic = new Dictionary<string, Mesh>();
            foreach (var item in go.GetComponentsInChildren<MeshFilter>())
                AddMesh(dic, item.name, item.sharedMesh);

            var mf = go.GetComponent<MeshFilter>();
            if (mf != null)
                AddMesh(dic, mf.name.Replace("@", ""), mf.sharedMesh);

            foreach (var item in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                AddMesh(dic, item.name, item.sharedMesh);

            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                AddMesh(dic, smr.name.Replace("@", ""), smr.sharedMesh);

            return dic;
        }

        void ComputeSmoothedNormalByJob(Mesh originalMesh)
        {
            List<Vector3> verts = new List<Vector3>();
            originalMesh.GetVertices(verts);
            List<Vector3> nors = new List<Vector3>();
            originalMesh.GetNormals(nors);

            int ovc = verts.Count;

            float3[] vertF3 = verts.ToArray().ToF3();
            float3[] norsF3 = nors.ToArray().ToF3();

            //CollectNormalJob&SmoothNormal 无并行处理
            UnsafeHashMap<float3, float3> smoothedNormalsMap = new UnsafeHashMap<float3, float3>(ovc, Allocator.Persistent);
            for (int i = 0; i < ovc; i++)
            {
                if (smoothedNormalsMap.ContainsKey(vertF3[i]))
                {
                    smoothedNormalsMap[vertF3[i]] = smoothedNormalsMap[vertF3[i]] + norsF3[i];
                }
                else
                {
                    smoothedNormalsMap.Add(vertF3[i], norsF3[i]);
                }
            }

            NativeArray<float3> normalsNative = new NativeArray<float3>(nors.ToArray().ToF3(), Allocator.Persistent);
            NativeArray<float3> vertrxNative = new NativeArray<float3>(verts.ToArray().ToF3(), Allocator.Persistent);

            var tangents = new NativeArray<float4>(originalMesh.tangents.ToF4(), Allocator.Persistent);
            var uv8 = new NativeArray<float2>(ovc, Allocator.Persistent);

            BakeNormalJob bakeNormalJob = new BakeNormalJob(vertrxNative, normalsNative, tangents, smoothedNormalsMap, uv8);
            bakeNormalJob.Schedule(ovc, 100).Complete();

            var _uv8 = new float2[ovc];
            uv8.CopyTo(_uv8);
            originalMesh.uv8 = _uv8.ToV2();
            smoothedNormalsMap.Dispose();
            normalsNative.Dispose();
            vertrxNative.Dispose();
            tangents.Dispose();
            uv8.Dispose();
        }

    }

}// JTRP.CustomAssetPostprocessor

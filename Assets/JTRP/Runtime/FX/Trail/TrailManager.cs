using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JTRP.FX
{
    public class TrailManager
    {
        TrailManager() { }
        static TrailManager _instance;
        public static TrailManager Instance
        {
            get
            {
                // if (isDispose)
                // {
                //     return null;
                // }
                if (_instance == null)
                {
                    _instance = new TrailManager();
                }
                return _instance;
            }
        }
        // static bool isDispose = false;
        public Dictionary<string, TrailBuilder> builderDic = new Dictionary<string, TrailBuilder>();
        int _frameCount = 0;
        public static void Dispose()
        {
            // isDispose = true;
            _instance = null;
        }

        public void UpdateTrail(int currentFrame)
        {
            if (currentFrame == _frameCount)
                return;
            List<string> needRemove = new List<string>();
            foreach (var kvp in builderDic)
            {
                var builder = kvp.Value;
                if (Time.time - builder.componentTimer < builder.componentSurvivalTime)
                {
                    if (Time.time - builder.generateTimer > builder.intervalTime && builder.openAutoGenerate)
                    {
                        builder.generateTimer = Time.time;
                        //生成残影
                        CreateAfterImage(builder);
                    }
                    if (builder.generate && !builder.lastGenerate)
                    {
                        //生成残影
                        CreateAfterImage(builder);
                    }
                    builder.lastGenerate = builder.generate;
                }
                else if (builder.trailList.Count == 0)
                {
                    needRemove.Add(kvp.Key);
                    continue;
                }
                //刷新残影
                UpdateAfterImage(builder);
            }
            foreach (var builder in needRemove)
            {
                builderDic[builder].enabled = false;
                builderDic.Remove(builder);
            }
            _frameCount = currentFrame;
        }
        /// <summary>
        /// 生成残影
        /// </summary>
        void CreateAfterImage(TrailBuilder builder)
        {
            foreach (var meshRenderer in builder.meshRenderers)
            {
                Mesh mesh = new Mesh();
                meshRenderer.BakeMesh(mesh);
                Material m = new Material(builder.material);
                Color c;
                if (m.HasProperty("_Color"))
                {
                    c = m.GetColor("_Color");
                    c.a = builder.alphaCurve.Evaluate(0);
                }
                else
                {
                    builder.openAutoGenerate = false;
                    Debug.LogError($"Shader:{m.shader.name} 不含 _Color 属性");
                    return;
                }
                builder.trailList.Add(new Trail(
                    mesh,
                    m,
                    meshRenderer.transform.localToWorldMatrix,
                    c,
                    Time.time,
                    builder.survivalTime));
            }
        }
        /// <summary>
        /// 刷新残影
        /// </summary>
        void UpdateAfterImage(TrailBuilder builder)
        {
            List<Trail> needRemove = new List<Trail>();
            foreach (var trail in builder.trailList)
            {
                float _PassingTime = Time.time - trail._StartTime;
                if (_PassingTime > trail._Duration)
                {
                    needRemove.Add(trail);
                    continue;
                }
                var c = trail._Material.GetColor("_Color");
                c.a = builder.alphaCurve.Evaluate(_PassingTime / trail._Duration);
                trail._Material.SetColor("_Color", c);
                trail._Material.SetFloat("_Offset", builder.offsetCurve.Evaluate(_PassingTime / trail._Duration));

                Graphics.DrawMesh(trail._Mesh, trail._Matrix, trail._Material, builder.gameObject.layer);
            }
            for (int i = 0; i < needRemove.Count; i++)
            {
                builder.trailList.Remove(needRemove[i]);
                needRemove[i] = null;
            }
        }
    }

    public class Trail
    {
        //残影网格
        public Mesh _Mesh;
        //残影纹理
        public Material _Material;
        //残影位置
        public Matrix4x4 _Matrix;
        //残影透明度
        public Color _Color;
        //残影启动时间
        public float _StartTime;
        //残影保留时间
        public float _Duration;

        public Trail(Mesh mesh, Material material, Matrix4x4 matrix4x4, Color color, float startTime, float duration)
        {
            _Mesh = mesh;
            _Material = material;
            _Matrix = matrix4x4;
            _Color = color;
            _StartTime = startTime;
            _Duration = duration;
        }
    }
}// namespace JTRP.FX

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//1.11
namespace JTRP.FX
{
    [ExecuteInEditMode]
    public class TrailBuilder : MonoBehaviour
    {
        [Header("自动生成")]
        public bool openAutoGenerate = true;
        [Header("手动生成")]
        public bool generate = false;

        [Header("组件生命周期")]
        public float componentSurvivalTime = 15f;
        [Header("残影生命周期")]
        public float survivalTime = 0.5f;
        [Header("残影生成间隔")]
        public float intervalTime = 0.2f;

        public Material material;
        public SkinnedMeshRenderer[] meshRenderers;
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0.0f, 0.5f, 1.0f, 0.0f);
        public AnimationCurve offsetCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 0.0f);

        public bool lastGenerate { get; set; }
        public float generateTimer { get; set; }
        public float componentTimer { get; set; }

        public List<Trail> trailList = new List<Trail>();

        private void OnEnable()
        {
            if (meshRenderers.Length > 0 && material)
            {
                componentTimer = Time.time;
                generateTimer = Time.time;
                TrailManager.Instance.builderDic[name] = this;
            }
            else
            {
                Debug.LogWarning($"{name}残影组件未正确初始化");
            }
        }

        private void OnValidate()
        {
            if (gameObject.activeSelf)
                OnEnable();
        }

        private void Update()
        {
            TrailManager.Instance.UpdateTrail(Time.frameCount);
        }

        private void OnApplicationQuit()
        {
            // TrailManager.Dispose();
        }
    }
}// namespace JTRP.FX

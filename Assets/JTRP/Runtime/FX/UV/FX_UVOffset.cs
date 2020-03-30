using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP.FX
{
    [ExecuteInEditMode]
    public class FX_UVOffset : MonoBehaviour
    {
        public string textureName = "MainTex";
        public bool enable = true;
        [Range(-5, 5)] public float horizontalSpeed;
        [Range(-5, 5)] public float verticalSpeed;
        Material material;
        string vecName;
        Vector2 vector2;
        private void OnValidate()
        {
            if (enable)
                Start();
        }
        private void Start()
        {
            material = GetComponent<Renderer>().sharedMaterial;
            vecName = "_" + textureName + "_ST";

            if (!material.HasProperty(vecName))
            {
                vecName = "_" + textureName;
                if (!material.HasProperty(vecName))
                {
                    vecName = textureName;
                    if (!material.HasProperty(vecName))
                    {
                        enable = false;
                        Debug.LogError($"不存在{vecName}属性！");
                        return;
                    }
                }
            }
            var v4 = material.GetVector(vecName);
            vector2 = new Vector2(v4.x, v4.y);
        }
        void Update()
        {
            if (!enable)
                return;
            float h = Time.time * horizontalSpeed;
            float v = Time.time * verticalSpeed;
            material.SetVector(vecName, new Vector4(vector2.x, vector2.y, -h, -v));
        }

    }
}// namespace JTRP.FX

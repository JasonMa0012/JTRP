using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP
{
    [ExecuteAlways]
    public class Water : MonoBehaviour
    {
        Material _material;
        MeshRenderer _meshRenderer;

        private Texture2D _rampTexture;
        [SerializeField]
        private Gradient _absorptionRamp;
        [SerializeField]
        private Gradient _scatterRamp;

        private void OnValidate()
        {
            OnEnable();
        }
        private void OnEnable()
        {
            Init();
            GenerateColorRamp();
        }
        private void OnDisable()
        {
            Dispose();
        }
        private void OnRenderObject()
        {

        }
        public void Init()
        {
            if (!_meshRenderer)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
                if (!_meshRenderer) Debug.LogError("Missing MeshRenderer");
            }
            if (!_material)
            {
                _material = _meshRenderer.sharedMaterial;
                if (!_material) Debug.LogError("Missing Material");
            }
        }
        public void Dispose()
        {
            _rampTexture = null;
        }

        public void GenerateColorRamp()
        {
            if (_rampTexture == null)
            {
                _rampTexture = new Texture2D(128, 4, TextureFormat.ARGB32, false, false);
                _rampTexture.wrapMode = TextureWrapMode.Clamp;
            }

            Color[] cols = new Color[512];
            for (int i = 0; i < 128; i++)
            {
                cols[i] = _absorptionRamp.Evaluate((float)i / 128f);
            }
            for (int i = 0; i < 128; i++)
            {
                cols[i + 128] = _scatterRamp.Evaluate((float)i / 128f);
            }
            _rampTexture.SetPixels(cols);
            _rampTexture.Apply();
            Shader.SetGlobalTexture("_RampMap", _rampTexture);
        }
    }
}

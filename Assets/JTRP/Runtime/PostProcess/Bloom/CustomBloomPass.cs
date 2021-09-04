using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace JTRP.PostProcessing
{
    [System.Serializable]
    class CustomBloomPass : CustomPass
    {
        RTHandle bloomDistortionBuffer;
        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            bloomDistortionBuffer = RTHandles.Alloc(// RTHandle是可以自动缩放的RenderTexture，第一个V2表示和相机渲染分辨率的比例
                Vector2.one * 0.5f, TextureXR.slices, dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
                useDynamicScale: true, name: "Bloom Distortion Buffer"
            );
        }

        protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
        {
            CoreUtils.SetRenderTarget(cmd, bloomDistortionBuffer, ClearFlag.Color);

            var resultOpaque = new RendererListDesc(new ShaderTagId("BloomDistortion"), cullingResult, hdCamera.camera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.opaque,
                sortingCriteria = SortingCriteria.CommonOpaque,
                excludeObjectMotionVectors = false,
                overrideMaterialPassIndex = 0,
                // layerMask = layer,
            };
            var resultTransparent = new RendererListDesc(new ShaderTagId("BloomDistortion"), cullingResult, hdCamera.camera)
            {
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.transparent,
                sortingCriteria = SortingCriteria.CommonTransparent,
                excludeObjectMotionVectors = false,
                overrideMaterialPassIndex = 0,
                // layerMask = layer,
            };

            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(resultOpaque));
            HDUtils.DrawRendererList(renderContext, cmd, RendererList.Create(resultTransparent));

            // SetCameraRenderTarget(cmd);
            // HDUtils.BlitTexture(cmd, bloomDistortionBuffer, new Vector4(1, 1, 0, 0), 0, false);
        }

        protected override void Cleanup()
        {
            bloomDistortionBuffer.Release();
        }
    }
}//namespace JTRP.PostProcessing

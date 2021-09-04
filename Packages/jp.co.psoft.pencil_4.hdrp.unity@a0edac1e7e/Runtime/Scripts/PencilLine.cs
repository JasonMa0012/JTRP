using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pencil_4.HDRP
{
    [Serializable, VolumeComponentMenu("Post-processing/Pencil+ 4/Line")]
    public sealed class PencilLine : PencilLineBase
    {
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterOpaqueAndSky;
    }

    public abstract class PencilLineBase : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter alpha = new ClampedFloatParameter(0f, 0f, 1f);

        Material m_Material;

        public bool IsActive() => m_Material != null && alpha.value > 0f;

        public override void Setup()
        {
            if (Shader.Find("Hidden/Pcl4LineHDRP") != null)
            {
                m_Material = new Material(Shader.Find("Hidden/Pcl4LineHDRP"));
            }
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying && RenderMode.GameViewRenderMode == RenderMode.Mode.Off)
            {
                HDUtils.BlitCameraTexture(cmd, source, destination);
                return;
            }
#endif

            bool draw = false;

            foreach (var lineEffect in camera.camera.GetComponents<PencilLineEffect>())
            {
                if (lineEffect.PencilRenderer != null && lineEffect.PencilRenderer.Texture != null && lineEffect.isPostProsessingEnabled)
                {
                    // エフェクトの重ね掛け対応
                    if (draw)
                    {
                        HDUtils.BlitCameraTexture(cmd, destination, source);
                    }

                    // テクスチャ更新設定
                    if (lineEffect.isRendering == true)
                    {
#if UNITY_2018_3_OR_NEWER
                        var callback = NativeFunctions.GetTextureUpdateCallbackV2();
#else
                        var callback = NativeFunctions.GetTextureUpdateCallback();
#endif
                        if (callback == IntPtr.Zero)
                        {
                            continue;
                        }

                        // ハンドルを取得し、ネイティブで確保したバッファが意図せず解放されないようにする
                        // ハンドルはTextureUpdateCallback()のEndで自動的に解除される
                        var textureUpdateHandle = lineEffect.PencilRenderer.RequestTextureUpdate(0);
                        if (textureUpdateHandle == 0xFFFFFFFF)
                        {
                            // PencilLinePostProcessRenderer.Render()の呼び出しがlineEffect.OnPreRender()よりも早いケースが稀にあり、
                            // PostProcessing_RenderingEventモードのときに適切なライン描画が行われない場合がある
                            continue;
                        }
#if UNITY_2018_3_OR_NEWER
                        cmd.IssuePluginCustomTextureUpdateV2(callback, lineEffect.PencilRenderer.Texture, textureUpdateHandle);
#else
                        cmd.IssuePluginCustomTextureUpdate(callback, lineEffect.PencilRenderer.Texture, textureUpdateHandle);
#endif
                        // レンダーエレメント画像出力用のテクスチャ更新
                        for (int renderElementIndex = 0; true; renderElementIndex++)
                        {
                            var renderElementTexture = lineEffect.PencilRenderer.GetRenderElementTexture(renderElementIndex);
                            var renderElementTargetTexture = lineEffect.PencilRenderer.GetRenderElementTargetTexture(renderElementIndex);
                            if (renderElementTexture == null || renderElementTargetTexture == null)
                            {
                                break;
                            }

                            textureUpdateHandle = lineEffect.PencilRenderer.RequestTextureUpdate(1 + renderElementIndex);
                            if (textureUpdateHandle == 0xFFFFFFFF)
                            {
                                break;
                            }

#if UNITY_2018_3_OR_NEWER
                            cmd.IssuePluginCustomTextureUpdateV2(callback, renderElementTexture, textureUpdateHandle);
#else
                            cmd.IssuePluginCustomTextureUpdate(callback, renderElementTexture, textureUpdateHandle);
#endif
                            cmd.Blit(renderElementTexture, renderElementTargetTexture);
                        }
                    }

                    // 描画設定
                    cmd.SetGlobalTexture("_MainTex", source);
                    cmd.SetGlobalTexture("_LineTex", lineEffect.PencilRenderer.Texture);
                    cmd.SetGlobalFloat("_Alpha", alpha.value);
                    HDUtils.DrawFullScreen(cmd, m_Material, destination);

                    //
                    draw = true;
                }
            }

            // 何も描画するものがなかった場合、RenderTargetを転写しておく
            if (!draw)
            {
                HDUtils.BlitCameraTexture(cmd, source, destination);
            }
        }

        public override void Cleanup() => CoreUtils.Destroy(m_Material);

    }
}
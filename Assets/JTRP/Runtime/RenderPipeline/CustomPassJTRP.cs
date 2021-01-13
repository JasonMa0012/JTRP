using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// ReSharper disable InconsistentNaming

namespace JTRP
{
	[System.Serializable]
	// ReSharper disable once InconsistentNaming
	class CustomPassJTRP : CustomPass
	{
		public bool enablePPOutline = true;
		public Material ppOutlineMaterial;
		public bool enableGeometryOutline = true;
		public bool enable3XDepth = false;
		[Range(0, 5)] public float globalGeometryWidthScale = 1f;

		RTHandle _customBuffer; // vertex color
		RTHandle _depthBuffer; // 3X Depth Texture
		RTHandle _postProcessTempBuffer;

		private static readonly int _camera3XDepthTexture = Shader.PropertyToID("_Camera3XDepthTexture");
		private static readonly int _jtrpCameraDepth = Shader.PropertyToID("_JTRP_CameraDepth");
		private static readonly int _jtrpCameraColor = Shader.PropertyToID("_JTRP_CameraColor");

		protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
		{
			// _customBuffer = RTHandles.Alloc(
			// 	scaleFactor: Vector2.one,
			// 	colorFormat: GraphicsFormat.B8G8R8A8_UNorm,
			// 	name: "Vertex Color Buffer",
			// 	autoGenerateMips: false
			// );
		}

		private void SetupPPTempBuffer(CustomPassContext ctx)
		{
			if (_postProcessTempBuffer == null)
				_postProcessTempBuffer = RTHandles.Alloc(
					scaleFactor: Vector2.one,
					colorFormat: ctx.cameraColorBuffer.rt.graphicsFormat,
					filterMode: ctx.cameraColorBuffer.rt.filterMode,
					name: "JTRP PP Temp");
		}

		private void Setup3XDepthBuffer()
		{
			if (_depthBuffer == null)
				_depthBuffer = RTHandles.Alloc(
					Vector2.one * 3,
					TextureXR.slices,
					DepthBits.Depth32,
					dimension: TextureXR.dimension,
					useDynamicScale: true,
					name: "3X CameraDepthStencil"
				);
		}

		//? BUG: HDUtils.BlitCameraTexture can not blit to ctx.cameraColorBuffer
		// Use this function instead
		private void BlitToCameraColorTexture(CustomPassContext ctx, RTHandle src)
		{
			SetRenderTargetAuto(ctx.cmd);
			HDUtils.BlitQuad(ctx.cmd, src,
				new Vector2(src.rtHandleProperties.rtHandleScale.x,
					src.rtHandleProperties.rtHandleScale.y),
				Vector2.one,
				0, false);
		}

		protected override void Execute(CustomPassContext ctx)
		{
			#region Draw Vertex Color

			// use MRT to render JTRP Shader after Opaque / Sky
			// CoreUtils.SetRenderTarget(ctx.cmd, _customBuffer);
			// CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.Color, Color.white);
			//
			// CoreUtils.SetRenderTarget(ctx.cmd, new RenderTargetIdentifier[] {ctx.cameraColorBuffer, _customBuffer},
			// 	ctx.cameraDepthBuffer);

			#endregion

			// render PP to tempBuffer and zhen copy back to cameraColor
			if (enablePPOutline)
			{
				SetupPPTempBuffer(ctx);
				CoreUtils.SetRenderTarget(ctx.cmd, _postProcessTempBuffer, ctx.cameraDepthBuffer);
				CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.Color, Color.blue);
				DoPPOutline(ctx);
				BlitToCameraColorTexture(ctx, _postProcessTempBuffer);
			}

			// draw opaque
			var resultJTRPOpaque =
				new RendererListDesc(new ShaderTagId("JTRPLitToon"), ctx.cullingResults, ctx.hdCamera.camera)
				{
					rendererConfiguration = (PerObjectData) 2047, // all
					renderQueueRange = RenderQueueRange.opaque,
					sortingCriteria = SortingCriteria.CommonOpaque
				};
			SetRenderTargetAuto(ctx.cmd);
			CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(resultJTRPOpaque));

			// Procedural Geometry Outline
			if (enableGeometryOutline)
			{
				if (enable3XDepth)
				{
					Setup3XDepthBuffer();
					CoreUtils.SetRenderTarget(ctx.cmd, _depthBuffer, _depthBuffer);
					CoreUtils.ClearRenderTarget(ctx.cmd, ClearFlag.Depth, Color.black);

					var resultOpaqueDepthOnly =
						new RendererListDesc(new ShaderTagId("DepthOnly"), ctx.cullingResults, ctx.hdCamera.camera)
						{
							rendererConfiguration = (PerObjectData) 2047, // all
							renderQueueRange = RenderQueueRange.opaque,
							sortingCriteria = SortingCriteria.CommonOpaque
						};
					CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, RendererList.Create(resultOpaqueDepthOnly));
					SetRenderTargetAuto(ctx.cmd);
					Shader.SetGlobalTexture(_camera3XDepthTexture, _depthBuffer);
				}

				DoGeometryOutline(ctx);
			}
		}

		protected override void Cleanup()
		{
			_customBuffer?.Release();
			_depthBuffer?.Release();
			_postProcessTempBuffer?.Release();
		}

		private void DoPPOutline(CustomPassContext ctx)
		{
			if (ppOutlineMaterial == null) return;
			ppOutlineMaterial.SetTexture(_jtrpCameraDepth, ctx.cameraDepthBuffer);
			ppOutlineMaterial.SetTexture(_jtrpCameraColor, ctx.cameraColorBuffer);
			CoreUtils.DrawFullScreen(ctx.cmd, ppOutlineMaterial);
		}

		private static bool _needRebake = true;

		[UnityEditor.MenuItem("JTRP/Rebake Geometry Outline")]
		private static void Rebake()
		{
			_needRebake = true;
		}

		private void DoGeometryOutline(CustomPassContext ctx)
		{// TODO:Analyze performance
			var needRebake = _needRebake;
			ctx.cmd.SetGlobalFloat("_globalEdgeWidthScale", globalGeometryWidthScale);
			ctx.cmd.SetGlobalFloat("_enable3XDepth", enable3XDepth ? 1 : 0);
			var outlineObjects = Object.FindObjectsOfType<GeometryOutline>();
			if (outlineObjects == null) return;
			foreach (var obj in outlineObjects)
			{
				if (needRebake) obj.DoRebake();
				if (obj.enable) obj.Draw(ctx.cmd);
			}

			_needRebake = false;
		}
	}
} //namespace JTRP.PostProcessing
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace JTRP
{
	[System.Serializable]
	class CustomPassJTRPRayTracing : CustomPass
	{
		[Header("Ray Tracing Outline")]
		public bool enableRayTracingOutline = false;

		public RayTracingShader rayTracingOutlineShader;

		[Space]
		public Renderer[] outlineRenderers;

		[Range(0, 5)]
		public float globalWidthScale = 1f;

		public bool debugGeometryOutline = false;

		private RayTracingAccelerationStructure _outlineRAS;
		private RTHandle                        _outlineMask;

		protected override void Setup(ScriptableRenderContext ctx, CommandBuffer cmd)
		{
			if (!enableRayTracingOutline) return;
			_outlineRAS = new RayTracingAccelerationStructure();
			if (outlineRenderers != null)
				foreach (var renderer in outlineRenderers)
					if (renderer != null)
						_outlineRAS.AddInstance(renderer);
			_outlineMask = RTHandles.Alloc(Vector2.one, TextureXR.slices, colorFormat: GraphicsFormat.R8_SNorm,
			                               dimension: TextureXR.dimension, enableRandomWrite: true,
			                               useDynamicScale: true, useMipMap: false,
			                               name: "JTRP Ray Tracing Outline Mask");
		}

		protected override void Execute(CustomPassContext ctx)
		{
			if (!enableRayTracingOutline || rayTracingOutlineShader == null)
				return;

			if (debugGeometryOutline)
			{
				DoGeometryOutline(ctx);
				return;
			}

			ctx.cmd.SetRenderTarget(_outlineMask);
			ctx.cmd.ClearRenderTarget(false, true, Color.black);
			DoGeometryOutline(ctx);

			SetRenderTargetAuto(ctx.cmd);
			DoRayTracingOutline(ctx);
		}

		protected override void Cleanup()
		{
			_outlineRAS?.Dispose();
			_outlineMask?.Release();
		}


		private static bool _needRebake = true;

		[UnityEditor.MenuItem("JTRP/Rebake Geometry Outline")]
		private static void Rebake()
		{
			_needRebake = true;
		}

		private void DoGeometryOutline(CustomPassContext ctx)
		{
			// TODO:Analyze performance
			var needRebake = _needRebake;
			ctx.cmd.SetGlobalFloat("_globalEdgeWidthScale", globalWidthScale);
			ctx.cmd.SetGlobalFloat("_enable3XDepth", 0);
			var outlineObjects = Object.FindObjectsOfType<GeometryOutline>();
			if (outlineObjects == null) return;
			foreach (var obj in outlineObjects)
			{
				if (needRebake) obj.DoRebake();
				if (obj.enable) obj.Draw(ctx.cmd);
			}

			_needRebake = false;
		}

		private void DoRayTracingOutline(CustomPassContext ctx)
		{
			ctx.cmd.SetRayTracingShaderPass(rayTracingOutlineShader, "JTRPRayTracingOutline");
			if (ShaderConfig.s_CameraRelativeRendering != 0)
				_outlineRAS.Build(ctx.hdCamera.camera.transform.position);
			else
				_outlineRAS.Build();
			if (outlineRenderers != null)
				foreach (var renderer in outlineRenderers)
					if (renderer != null)
						_outlineRAS.UpdateInstanceTransform(renderer);
			ctx.cmd.SetRayTracingAccelerationStructure(rayTracingOutlineShader, "_RaytracingAccelerationStructure",
			                                           _outlineRAS);
			ctx.cmd.SetRayTracingTextureParam(rayTracingOutlineShader, "RenderTarget", ctx.cameraColorBuffer);
			ctx.cmd.SetRayTracingTextureParam(rayTracingOutlineShader, "_DepthTexture", ctx.cameraDepthBuffer);
			ctx.cmd.SetRayTracingTextureParam(rayTracingOutlineShader, "_OutlineMask", _outlineMask);
			ctx.cmd.DispatchRays(rayTracingOutlineShader, "RayTracingOutlineRaygen",
			                     (uint) ctx.cameraColorBuffer.rt.width,
			                     (uint) ctx.cameraColorBuffer.rt.height, 1);
		}
	}
} //namespace JTRP.PostProcessing
// Material / Surface shader: Hit shaders should be defined as a pass in a shader used for a
// material in the scene.
Shader "JTRP/Ray Tracing Outline"
{
	SubShader
	{
		Pass
		{
			// Pass Name and LightMode must match that specified by SetShaderPass()
			Name "JTRPRayTracingOutline"
			Tags { "LightMode" = "JTRPRayTracingOutline" }
			HLSLPROGRAM

			#pragma only_renderers d3d11
			#pragma raytracing surface_shader

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
			
			
			[shader("closesthit")]
			void JTRPRayTracingOutline(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes)
			{
				IntersectionVertex currentVertex;
				GetCurrentIntersectionVertex(attributeData, currentVertex);


				float4 albedo = float4(currentVertex.normalOS, 1);//float4(0.5, 1, 0.5, 1);
				rayIntersection.color = albedo;
			}
			ENDHLSL

		}
	}
}
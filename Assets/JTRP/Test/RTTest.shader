// Material / Surface shader: Hit shaders should be defined as a pass in a shader used for a
// material in the scene.
Shader "FlatColor"
{
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardOnly" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
			};
			struct v2f
			{
				float4 vertex: SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o = (v2f)0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag(v2f i): SV_Target
			{
				return fixed4(1, 0, 0, 1);
			}
			
			ENDCG

		}
		Pass
		{
			Name "RayTracingPrepass"
			Tags { "LightMode" = "RayTracingPrepass" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
			};
			struct v2f
			{
				float4 vertex: SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o = (v2f)0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag(v2f i): SV_Target
			{
				return fixed4(1, 1, 1, 1);
			}
			
			ENDCG

		}
	}
	
	SubShader
	{
		Pass
		{
			// Pass Name and LightMode must match that specified by SetShaderPass()
			Name "JTRPForwardDXR"
			Tags { "LightMode" = "JTRPForwardDXR" }
			HLSLPROGRAM

			#pragma only_renderers d3d11
			#pragma raytracing surface_shader

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
			
			
			[shader("closesthit")]
			void FullResRayGen(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes)
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
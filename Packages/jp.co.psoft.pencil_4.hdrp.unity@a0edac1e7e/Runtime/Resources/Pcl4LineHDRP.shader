Shader "Hidden/Pcl4LineHDRP"
{
	HLSLINCLUDE
	#pragma target 4.5
	#pragma editor_sync_compilation
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

	TEXTURE2D_X(_MainTex);
	TEXTURE2D(_LineTex);
	SAMPLER(sampler_LineTex);
	float _Alpha;

	struct Attributes
	{
		uint vertexID : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
		float4 positionCS : SV_POSITION;
		float2 texcoord   : TEXCOORD0;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings Vert(Attributes input)
	{
		Varyings output;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		uint2 positionSS = input.texcoord * _ScreenSize.xy;

#if !UNITY_COLORSPACE_GAMMA
		float4 from = LOAD_TEXTURE2D_X(_MainTex, positionSS);
		float4 lineColor = SAMPLE_TEXTURE2D(_LineTex, sampler_LineTex, input.texcoord.xy) * _Alpha;
		return float4(from.rgb * (1.0 - lineColor.a) + lineColor.rgb, 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#else
		float4 lineColor = SAMPLE_TEXTURE2D(_LineTex, sampler_LineTex, input.texcoord.xy);
		if (lineColor.a * _Alpha == 1)
		{
			return float4(lineColor.rgb, 1);
		}

		float4 from = LOAD_TEXTURE2D_X(_MainTex, positionSS);
		if (lineColor.a == 0.0)
		{
			return from;
		}

		from.rgb = SRGBToLinear(from.rgb);

		lineColor.rgb /= lineColor.a;
		lineColor.rgb = SRGBToLinear(lineColor.rgb);
		lineColor.a *= _Alpha;

		return float4(LinearToSRGB(lerp(from.rgb, lineColor.rgb, lineColor.a)), 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#endif
	}
	ENDHLSL

	SubShader
	{
		Tags{ "RenderPipeline" = "HDRenderPipeline" }

		Pass
		{
			ZWrite Off ZTest Always Blend Off Cull Off

			HLSLPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
			ENDHLSL
		}
	}
	Fallback Off

}
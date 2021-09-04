Shader "Hidden/PPOutline"
{
	Properties
	{
		[Header(Common)]
		[Toggle]_AugmentSceneColor ("Augment Scene Color", float) = 1
		_BGColor ("Background Color", color) = (0.5, 0.5, 0.5, 1)
		[Space]
		
		[Header(Silhouette)]
		[Toggle]_SILineEnable ("Silhouette Line Enable", float) = 1
		_SILineThickness ("Silhouette Line Width", float) = 1
		_SILineStrength ("Silhouette Line Strength", float) = 1
		_SILineThreshold ("Silhouette Line Threshold", float) = 0
		_SILineColor ("Silhouette Line Color", color) = (1, 0, 0, 1)
		[Space]
		
		[Header(Crease)]
		[Toggle]_CRLineEnable ("Crease Line Enable", float) = 1
		_CRLineThickness ("Crease Line Width", float) = 1
		_CRLineStrength ("Crease Line Strength", float) = 1
		_CRLineThreshold ("Crease Line Threshold", float) = 0
		_CRLineColor ("Crease Line Color", color) = (0, 0, 1, 1)
		[Space]
		
		[Header(Suggestive Contour)]
		[Toggle]_SCLineEnable ("Suggestive Contour Line Enable", float) = 1
		_SCLineThickness ("Suggestive Contour Line Width", float) = 1
		_SCLineStrength ("Suggestive Contour Line Strength", float) = 100
		[PowerSilder(8)] _SCLineThreshold ("Suggestive Contour Line Threshold", Range(0, 1)) = 0
		_SCLineColor ("Suggestive Contour Line Color", color) = (0, 1, 0, 1)
	}
	
	HLSLINCLUDE
	
	#pragma vertex Vert
	
	#pragma target 4.5
	#pragma only_renderers d3d11 playstation xboxone vulkan metal switch
	#pragma multi_compile SHADOW_HIGH SHADOW_LOW SHADOW_MEDIUM
	
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
	
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
	#define SHADERPASS SHADERPASS_DEFERRED_LIGHTING
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
	
	/*
	The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
	struct PositionInputs
	{
		float3 positionWS;  // World space position (could be camera-relative)
		float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
		uint2 positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
		uint2 tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
		float deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
		float linearDepth; // View space Z coordinate                              : [Near, Far]
	};
	struct BSDFData
	{
		uint materialFeatures;
		real3 diffuseColor;
		real3 fresnel0;
		real ambientOcclusion;
		real specularOcclusion;
		float3 normalWS;
		real perceptualRoughness;
		real coatMask;
		uint diffusionProfileIndex;
		real subsurfaceMask;
		real thickness;
		bool useThickObjectMode;
		real3 transmittance;
		float3 tangentWS;
		float3 bitangentWS;
		real roughnessT;
		real roughnessB;
		real anisotropy;
		real iridescenceThickness;
		real iridescenceMask;
		real coatRoughness;
		real3 geomNormalWS;
		real ior;
		real3 absorptionCoefficient;
		real transmittanceMask;
	};
	
	
	To sample custom buffers, you have access to these functions:
	But be careful, on most platforms you can't sample to the bound color buffer. It means that you
	can't use the SampleCustomColor when the pass color buffer is set to custom(and same for camera the buffer).
	float4 SampleCustomColor(float2 uv);
	float4 LoadCustomColor(uint2 pixelCoords);
	float LoadCustomDepth(uint2 pixelCoords);
	float SampleCustomDepth(float2 uv);
	
	There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
	you can check them out in the source code of the core SRP package.
	*/
	
	// #include "UnityCG.cginc"
	
	
	bool _AugmentSceneColor;
	float3 _BGColor;
	
	bool _SILineEnable;
	float _SILineThickness;
	float _SILineStrength;
	float _SILineThreshold;
	float3 _SILineColor;
	
	bool _CRLineEnable;
	float _CRLineThickness;
	float _CRLineStrength;
	float _CRLineThreshold;
	float3 _CRLineColor;
	
	bool _SCLineEnable;
	float _SCLineThickness;
	float _SCLineStrength;
	float _SCLineThreshold;
	float3 _SCLineColor;
	
	float _LineIntensityOffset = 0;
	
	SamplerState sampler_PointClamp;

	TEXTURE2D_X(_JTRP_CameraColor);
	TEXTURE2D_X(_JTRP_CameraDepth);
	
	#include "JiffycrewPPLineCommon.cginc"
	
	float3 WDirUV(float3 wsNormal, float3 wsCamera)
	{
		return wsCamera - dot(wsNormal, wsCamera) * wsNormal;
	}
	
	float2 WScreenUV(float3 wsPos, float3 wsNormal, float3 wsCamera)
	{
		float3 wsWDir = WDirUV(wsNormal, wsCamera);
		
		float4 wsPosA = float4(wsPos, 1);
		float4 wsPosB = float4(wsPos + wsWDir, 1);
		
		float4 scrPosA = mul(UNITY_MATRIX_VP, wsPosA);
		float4 scrPosB = mul(UNITY_MATRIX_VP, wsPosB);
		
		return scrPosB.xy / scrPosB.w - scrPosA.xy / scrPosB.w;
	}
	
	float Valley(float2 uv, float2 scrW, float3 wsNormal, float3 worldView)
	{
		float2 uvOffset = scrW * _SCLineThickness * _ScreenSize.zw;
		float2 uv1 = uv + uvOffset;
		float2 uv2 = uv - uvOffset;
		
		float3 wsNormal1 = GetNormal(uv1);
		float3 wsNormal2 = GetNormal(uv2);
		
		float3 wsCamera = worldView;
		float3 wsCamera1 = GetWorldView(uv1);
		float3 wsCamera2 = GetWorldView(uv2);
		
		float NdotV = saturate(dot(wsNormal, wsCamera));
		float NdotV1 = saturate(dot(wsNormal1, wsCamera1));
		float NdotV2 = saturate(dot(wsNormal2, wsCamera2));
		
		float lineVal = max(NdotV1, NdotV2) - saturate(NdotV + _SCLineThreshold);
		return saturate(lineVal * _SCLineStrength);
	}
	
	float SuggestiveContour(float2 uv, float3 normal, float3 worldView, float3 worldPos)
	{
		float2 wScr = WScreenUV(worldPos, normal, worldView);
		return Valley(uv, wScr, normal, worldView);
	}
	
	float4 FullScreenPass(Varyings varyings): SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
		float2 uv = varyings.positionCS.xy * _ScreenSize.zw;
		float depth = GetDepth(uv);
		PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
		float3 c = GetColor(varyings.positionCS.xy);
		
		// Add your custom pass code here
		float3 worldNormal = GetNormal(uv);
		float3 worldPos = GetWorldPos(uv, depth);
		float3 worldView = GetWorldSpaceNormalizeViewDir(worldPos);
		
		float si = SilhouetteSobel(_SILineThickness * 0.5, uv, _ScreenSize.zw);
		float cr = CreaseSobel(_CRLineThickness, uv, _ScreenSize.zw);
		float sc = SuggestiveContour(uv, worldNormal, worldView, worldPos);
		
		si = saturate(si * _SILineStrength - _SILineThreshold);
		cr = saturate(cr * _CRLineStrength - _CRLineThreshold);
		sc = saturate(sc);
		
		float3 white = float3(1, 1, 1);
		float3 black = float3(0, 0, 0);
		
		float3 bgColor = _AugmentSceneColor ? c.rgb: _BGColor;
		
		float3 siColor = _SILineEnable ? white - (si * (white - _SILineColor)): white;
		float3 crColor = _CRLineEnable ? white - (cr * (white - _CRLineColor)): white;
		float3 scColor = _SCLineEnable ? white - (sc * (white - _SCLineColor)): white;
		
		float3 lineColor = bgColor * siColor * crColor * scColor;
		
		return float4(lineColor, 1);
	}
	
	ENDHLSL
	
	SubShader
	{
		Pass
		{
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			
			HLSLPROGRAM
			
			#pragma fragment FullScreenPass
			ENDHLSL
			
		}
	}
	Fallback Off
}

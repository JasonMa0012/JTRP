Shader "JTRP/Lit Toon Tessellation"
{
	Properties
	{
		[Title(_, Diffuse)]
		[Tex(_, _Color)] _MainTex ("ColorMap (RGB)", 2D) = "white" { }
		[HideInInspector] _Color ("Color", Color) = (1, 1, 1, 1)
		_Purity ("Purity：纯度", Range(0, 5)) = 1
		
		[Header(Light Setting)][Space(5)]
		[PowerSlider(3)] _LightColorIntensity ("Light Int：平行光强度", Range(0, 1)) = 0.3
		_SkyColorIntensity ("Sky Int：天空盒强度", Range(0, 1)) = 0.5
		_LightColorBlend ("Blend：灯光混合固有色", Range(0, 1)) = 0.5
		_PointLightColorIntensity ("Point Light Int：点光照明强度", Range(0, 1)) = 1
		_PointLightStep ("Point Light Step：阈值", Range(0, 2)) = 0.6
		[PowerSlider(4)] _PointLightFeather ("Point Light Feather：羽化", Range(0.0001, 1)) = 0.001
		
		[Title(_, Normal)]
		[Tex(_, _NormalScale)][Normal] _NormalMap ("NormalMap", 2D) = "bump" { }
		[HideInInspector] _NormalScale ("Normal Scale：深度", Range(0.001, 3.0)) = 1.0
		_DiffuseNormalBlend ("Diffuse Normal Blend", Range(0.0, 1.0)) = 1.0
		
		
		[Main(_Shadow, _, 2)]
		_shadow ("Shadow", float) = 0
		[Tex(_Shadow)] _ShadowMap ("ShadowMap (RGBA)", 2D) = "black" { }
		[Sub(_Shadow)] _ShadowFixedColor ("Color：固有阴影颜色", Color) = (1, 1, 1, 1)
		[SubToggle(_Shadow, _ENABLE_SELFSHADOW)] _enable_selfshadow ("Enable Self Shadow", float) = 0
		
		[Title(_Shadow, 1st Shadow)]
		[Tex(_Shadow, _ShadowMapColor)]_ShadowColorMap ("ShadowColorMap (RGB)", 2D) = "white" { }
		[HideInInspector]_ShadowMapColor ("Color", Color) = (0.5, 0.5, 0.5, 1)
		[SubToggle(_Shadow, _)]_EnableShadowColorRamp ("Enable Shadow Color Ramp", float) = 0
		[Ramp(_Shadow)]_ShadowColorRamp ("Shadow Color Ramp", 2D) = "white" { }
		[Sub(_Shadow)]_ShadowIntensity ("Int：强度", Range(0, 1)) = 1
		[Sub(_Shadow)]_Shadow_Purity ("Purity：纯度", Range(0, 5)) = 1
		[Sub(_Shadow)]_Shadow_Step ("Step：阈值", Range(0, 1)) = 0.55
		[SubPowerSlider(_Shadow, 6)] _Shadow_Feather ("Feather：羽化", Range(0.0001, 1)) = 0.0001
		
		[Title(_Shadow, 2nd Shadow)]
		[Sub(_Shadow)][HDR] _ShadowColor2 ("Color Add", Color) = (0, 0, 0, 1)
		// [Ramp(_Shadow)]_ShadowAddRamp ("Shadow Add Ramp", 2D) = "black" { }
		[Sub(_Shadow)]_ShadowIntensity2 ("Int：强度", Range(0, 1)) = 1
		[Sub(_Shadow)]_ShadowColorBlend2 ("Blend：混合", Range(0, 1)) = 0.8
		[Sub(_Shadow)]_Shadow_Step2 ("Step：阈值", Range(0, 1)) = 0.2
		[SubPowerSlider(_Shadow, 6)] _Shadow_Feather2 ("Feather：羽化", Range(0.0001, 1)) = 0.001
		
		
		[Main(_HL)]
		_Enable_HighLight ("HighLight", float) = 0
		[Tex(_HL)] _LightMap ("LightMap (RGBA)", 2D) = "white" { }
		[KWEnum(_HL, NPR, _HL_NPR, PBR, _HL_PBR)]
		_HighLight_Mode ("HighLight Mode", float) = 0
		
		[Sub(_HL)][HDR] _HighColor1 ("Toon High Color1", Color) = (1, 1, 1, 1)
		[Sub(_HL)] _roughness ("Roughness：粗糙度", Range(0.02, 1)) = 0.5
		[SubPowerSlider(_HL, 2)]_HighColorInt1 ("Int1：强度", Range(0, 1)) = 1
		[SubPowerSlider(_HL, 5)]_HighColorPointInt1 ("PointInt1：点光强度", Range(0, 1)) = 0.005
		[HideInInspector]_HighLightStep1 ("Step1：阈值", Range(0, 3)) = 0.99
		[HideInInspector]_HighLightFeather1 ("Feather1：羽化", Range(0.001, 3)) = 0.001
		// _HL_NPR
		[Sub(_HL_HL_NPR)] _HighColorIntOnShadow1 ("Int On Shadow1：阴影中强度", Range(0, 1)) = 0.3
		[Sub(_HL_HL_NPR)] [HDR] _HighColor2 ("Phong High Color2", Color) = (1, 1, 1, 1)
		[SubPowerSlider(_HL_HL_NPR, 2)]_HighColorInt2 ("Int2：强度", Range(0, 1)) = 1
		[SubPowerSlider(_HL_HL_NPR, 5)]_HighColorPointInt2 ("PointInt2：点光强度", Range(0, 1)) = 0.005
		[SubPowerSlider(_HL_HL_NPR, 2)]_HighLightPower2 ("power：范围", Range(0, 1000)) = 888
		[Sub(_HL_HL_NPR)]_HighColorIntOnShadow2 ("Int On Shadow2：阴影中强度", Range(0, 1)) = 0.3
		
		
		[Main(_MC)]
		_MatCap_Enable ("MatCap", float) = 0
		[Sub(_MC)] _MatCap_Sampler ("MatCap Map (R)", 2D) = "black" { }
		[Sub(_MC)] _BumpScaleMatcap ("Noise Int：扰动强度", Range(-3, 3)) = 1
		[Sub(_MC)] _MatCapNormalMap ("Noise Map", 2D) = "black" { }
		[Sub(_MC)] [HDR] _MatCapColor ("MatCap Color", Color) = (1, 1, 1, 1)
		[SubPowerSlider(_MC, 3)] _MatCap ("Int：强度", Range(0, 1)) = 0.1
		[SubPowerSlider(_MC, 2)] _TweakMatCapOnShadow ("Int On Shadow：阴影中强度", Range(0, 1)) = 0.25
		[SubPowerSlider(_MC, 2)] _BlurLevelMatcap ("Blur Level：模糊", Range(0, 10)) = 0
		
		
		[Main(_Rim)]
		_RimLight_Enable ("RimLight", float) = 0
		[KWEnum(_Rim, Normal, _Rim_Normal, Screen Space, _Rim_SS)]
		_RimLight_Mode ("HighLight Mode", float) = 0
		
		[Title(_Rim_Rim_Normal, Bright Side)]
		[Sub(_Rim)] [HDR] _RimLightColor ("RimLight Color：边缘光", Color) = (1, 1, 1, 1)
		[Sub(_Rim)] _RimLightIntensity ("Int：强度", Range(0, 1)) = 1
		[Sub(_Rim)] _RimLightBlend ("Blend：混合固有色", Range(0, 1)) = 0.5
		[Sub(_Rim)] _RimLightBlendPoint ("Blend Point：混合点光", Range(0, 1)) = 0.35
		[Title(_Rim_Rim_Normal, Dark Side)]
		[Sub(_Rim_Rim_Normal)] [HDR] _RimLightColor2 ("RimLight Color2：边缘光", Color) = (1, 1, 1, 1)
		[Sub(_Rim_Rim_Normal)] _RimLightIntensity2 ("Int2：强度", Range(0, 1)) = 1
		[Sub(_Rim_Rim_Normal)] _RimLightBlend2 ("Blend2：混合固有色", Range(0, 1)) = 0.5
		[Sub(_Rim_Rim_Normal)] _RimLightBlendPoint2 ("Blend Point2：混合点光", Range(0, 1)) = 0.35
		
		[Title(_Rim, Rim Setting)]
		[SubPowerSlider(_Rim, 5)] _RimLightFeather ("Feather：羽化", Range(0.0001, 1)) = 0.005
		[SubPowerSlider(_Rim, 1.5)] _RimLightWidth ("Width：宽度", Range(0, 2)) = 0.3
		[SubPowerSlider(_Rim, 0.35)] _RimLightLength ("Length：长度", Range(0, 10)) = 7
		[Sub(_Rim_Rim_SS)] _RimLightIntInShadow ("Int：暗面强度", Range(0, 1)) = 0.35

		
		[Main(_Tess, _, 2)]
		_TessellationMode ("Tessellation", Float) = 0
		[SubPowerSlider(_Tess, 2)] _TessellationFactor ("Tessellation Factor", Range(0.0, 64.0)) = 0.0
		[Sub(_Tess)] _TessellationFactorMinDistance ("Tessellation start fading distance", Float) = 20.0
		[Sub(_Tess)] _TessellationFactorMaxDistance ("Tessellation end fading distance", Float) = 50.0
		[Sub(_Tess)] _TessellationFactorTriangleSize ("Tessellation triangle size", Float) = 100.0
		[Sub(_Tess)] _TessellationShapeFactor ("Tessellation shape factor", Range(0.0, 1.0)) = 0.75 // Only use with Phong
		[Sub(_Tess)] _TessellationBackFaceCullEpsilon ("Tessellation back face epsilon", Range(-1.0, 0.0)) = -1
		
		
		[Main(_OL)]
		_OutLine_Enable ("OutLine", float) = 0
		[Tex(_OL, _Outline_Color)] _Outline_ColorMap ("Outline Color (RGB)", 2D) = "white" { }
		[HideInInspector] _Outline_Color ("Outline Color", Color) = (0, 0, 0, 1)
		[Sub(_OL)] _Outline_Width ("Width：宽度", float) = 1
		[Ramp(_OL)]_Outline_Width_Ramp ("Outline Width Ramp", 2D) = "white" { }
		[Sub(_OL)] _Outline_Ramp_Max_Distance ("Ramp Max Distance", float) = 10
		[SubToggle(_OL)] _OriginNormal ("Origin Normal：原始法线", float) = 1
		[Sub(_OL)] _Offset_Z ("Offset Z：深度偏移", float) = 0
		[Sub(_OL)] _Outline_Blend ("Blend：颜色混合", Range(0, 1)) = 1
		[SubPowerSlider(_OL, 2)] _Outline_Purity ("Purity：纯度", Range(0, 5)) = 1
		[SubPowerSlider(_OL, 2)] _Outline_Lightness ("Lightness：明度", Range(0, 5)) = 1
		
		
		[Header(State)]
		[Queue] _RenderQueue ( "Queue", int) = 2000
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode：裁剪", int) = 2  //OFF/FRONT/BACK
		[Toggle(_)] _ZWrite ("ZWrite", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4 // Less equal
		_ZOffset ("Z Offset", float) = 0
		[Toggle(_)] _AlphaIsTransparent ("Alpha Is Transparent", Float) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0.0
		[PowerSlider(2)] _BloomFactor ("Bloom Factor", Range(1, 50)) = 1
		_StencilRef ("Stencil Ref", float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompare ("Stencil Compare", float) = 8
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOP ("Stencil Op", float) = 0
		
		
		[HideInInspector]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
		[HideInInspector]_BaseColorMap ("BaseColorMap", 2D) = "white" { }
		// Versioning of material to help for upgrading
		[HideInInspector] _HdrpVersion ("_HdrpVersion", Float) = 2
		[HideInInspector] _SurfaceType ("__surfacetype", Float) = 0.0
		[HideInInspector] _AlphaSrcBlend ("__alphaSrc", Float) = 1.0
		[HideInInspector] _AlphaDstBlend ("__alphaDst", Float) = 0.0
	}
	
	HLSLINCLUDE
	
	#pragma target 5.0
	#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	#pragma require tessellation tessHW
	#pragma enable_d3d11_debug_symbols
	
	#pragma shader_feature_local _ _TESSELLATION_DISPLACEMENT _PIXEL_DISPLACEMENT
	#pragma shader_feature_local _TESSELLATION_PHONG
	
	
	#define ATTRIBUTES_NEED_TEXCOORD2
	#define VARYINGS_NEED_TEXCOORD2
	
	#define TESSELLATION_ON
	#define HAVE_VERTEX_MODIFICATION
	#define HAVE_TESSELLATION_MODIFICATION
	
	
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
	
	#include "LitToonDefine.hlsl"
	
	ENDHLSL

	SubShader
	{
		// Pass
		// {
		// 	Name "GBuffer"
		// 	Tags { "LightMode" = "GBuffer" }// This will be only for opaque object based on the RenderQueue index
			
		// 	Cull [_CullMode]
		// 	ZTest [_ZTest]
			
		// 	Stencil
		// 	{
		// 		Ref [_StencilRef]
		// 		Comp [_StencilCompare]
		// 		Pass [_StencilOP]
		// 	}
			
		// 	HLSLPROGRAM

		// 	#pragma multi_compile _ DEBUG_DISPLAY
		// 	#pragma multi_compile _ LIGHTMAP_ON
		// 	#pragma multi_compile _ DIRLIGHTMAP_COMBINED
		// 	#pragma multi_compile _ DYNAMICLIGHTMAP_ON
		// 	#pragma multi_compile _ SHADOWS_SHADOWMASK
		// 	// Setup DECALS_OFF so the shader stripper can remove variants
		// 	#pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
		// 	#pragma multi_compile _ LIGHT_LAYERS
			
		// 	#ifndef DEBUG_DISPLAY
		// 		// When we have alpha test, we will force a depth prepass so we always bypass the clip instruction in the GBuffer
		// 		// Don't do it with debug display mode as it is possible there is no depth prepass in this case
		// 		#define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
		// 	#endif
			
		// 	#define SHADERPASS SHADERPASS_GBUFFER
		// 	#ifdef DEBUG_DISPLAY
		// 		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
		// 	#endif
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
			
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassGBuffer.hlsl"
			
		// 	#pragma vertex Vert
		// 	#pragma fragment Frag
		// 	#pragma hull Hull
		// 	#pragma domain Domain
			
		// 	ENDHLSL

		// }
		
		// // This tags allow to use the shader replacement features
		// Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDLitShader" /* "Queue" = "Transparent+100"*/ }
		// Pass
		// {
		// 	Name "ShadowCaster"
		// 	Tags { "LightMode" = "ShadowCaster" }
			
		// 	Cull off
			
		// 	ZClip [_ZClip]
		// 	ZWrite On
		// 	ZTest LEqual
			
		// 	ColorMask 0
			
		// 	HLSLPROGRAM

		// 	#define SHADERPASS SHADERPASS_SHADOWS
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitDepthPass.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
			
		// 	#pragma vertex Vert
		// 	#pragma fragment Frag
		// 	#pragma hull Hull
		// 	#pragma domain Domain
			
		// 	ENDHLSL

		// }
		
		// Pass
		// {
		// 	Name "DepthOnly"
		// 	Tags { "LightMode" = "DepthOnly" }
			
		// 	Cull[_CullMode]
		// 	ZWrite [_ZWrite]
		// 	ZTest [_ZTest]
		// 	Offset [_ZOffset], 0
			
		// 	Stencil
		// 	{
		// 		Ref [_StencilRef]
		// 		Comp [_StencilCompare]
		// 		Pass [_StencilOP]
		// 	}
			
		// 	HLSLPROGRAM

		// 	// In deferred, depth only pass don't output anything.
		// 	// In forward it output the normal buffer
		// 	#pragma multi_compile _ WRITE_NORMAL_BUFFER
		// 	#pragma multi_compile _ WRITE_MSAA_DEPTH
			
		// 	#define SHADERPASS SHADERPASS_DEPTH_ONLY
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			
		// 	#ifdef WRITE_NORMAL_BUFFER // If enabled we need all regular interpolator
		// 		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
		// 	#else
		// 		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitDepthPass.hlsl"
		// 	#endif
			
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
		// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
			
		// 	#pragma vertex Vert
		// 	#pragma fragment Frag
		// 	#pragma hull Hull
		// 	#pragma domain Domain
			
		// 	ENDHLSL

		// }
		
		Pass
		{
			Name "JTRPLitToon"
			Tags { "LightMode" = "JTRPLitToon" }// This will be only for transparent object based on the RenderQueue index
			Cull [_CullMode]
			ZWrite [_ZWrite]
			ZTest [_ZTest]
			Offset [_ZOffset], 0
			Blend 0 [_SrcBlend] [_DstBlend]
			Blend 1 one zero
			
			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilCompare]
				Pass [_StencilOP]
			}
			
			HLSLPROGRAM

			// Supported shadow modes per light type
			#pragma multi_compile SHADOW_HIGH SHADOW_LOW SHADOW_MEDIUM
			#pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
			
			#pragma shader_feature_local _ _ENABLE_SELFSHADOW
			#pragma shader_feature_local _ _ENABLE_HIGHLIGHT_ON
			#pragma shader_feature_local _ _HL_PBR
			#pragma shader_feature_local _ _RIMLIGHT_ENABLE_ON
			#pragma shader_feature_local _ _MATCAP_ENABLE_ON
			#pragma shader_feature_local _ _EMISSIVE_ENABLE_ON
			
			
			#include "LitToonForwardPass.hlsl"
			
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma hull Hull
			#pragma domain Domain
			
			ENDHLSL

		}

		Pass
		{
			Name "Outline"
			Cull Front
			ZWrite [_ZWrite]
			
			HLSLPROGRAM

			#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
			
			#pragma shader_feature_local _ _OUTLINE_ENABLE_ON
			#pragma shader_feature_local _ _ORIGINNORMAL_ON
			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
			
			#include "LitToonOutlinePass.hlsl"
			
			// TODO: add tessellation
			#pragma vertex vert
			#pragma fragment frag
			// #pragma hull Hull
			// #pragma domain Domain
			
			ENDHLSL

		}
	}

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

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingFragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/Common/AtmosphericScatteringRayTracing.hlsl"
			
			[shader("closesthit")]
			void JTRPRayTracingOutline(inout RayIntersection rayIntersection: SV_RayPayload, AttributeData attributeData: SV_IntersectionAttributes)
			{
				// The first thing that we should do is grab the intersection vertice
				IntersectionVertex currentVertex;
				GetCurrentIntersectionVertex(attributeData, currentVertex);

				// Build the Frag inputs from the intersection vertice
				FragInputs fragInput;
				BuildFragInputsFromIntersection(currentVertex, rayIntersection.incidentDirection, fragInput);
				float3 normalWS = fragInput.tangentToWorld[2];

				// Compute the view vector
				float3 viewWS = -rayIntersection.incidentDirection;

				// Let's compute the world space position (the non-camera relative one if camera relative rendering is enabled)
				float3 pointWSPos = fragInput.positionRWS;
				
				// Make sure to add the additional travel distance
				float travelDistance = length(fragInput.positionRWS - rayIntersection.origin);
				rayIntersection.t = travelDistance;


				float4 albedo = travelDistance;
				// float4 albedo = float4(normalWS, 1);//float4(0.5, 1, 0.5, 1);
				rayIntersection.color = albedo;
				rayIntersection.rayCount += 1;
			}
			ENDHLSL

		}
	}

	CustomEditor "JTRP.ShaderDrawer.LWGUI"
}

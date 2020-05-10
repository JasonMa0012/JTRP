Shader "JTRP/Lit"
{
    Properties
    {
        [Enum(OFF, 0, FRONT, 1, BACK, 2)] _CullMode ("Cull Mode：裁剪", int) = 2  //OFF/FRONT/BACK
        [Queue] _RenderQueue ( "Queue", int) = 2000
        [Toggle(_)] _ZWrite ("ZWrite", Float) = 1.0
        
        [Title(_, Diffuse)]
        [Tex(_, _Color)] _MainTex ("ColorMap (RGB)", 2D) = "white" { }
        [HideInInspector] _Color ("Color", Color) = (1, 1, 1, 1)
        _AddColorIntensity ("Add Int", Range(0, 1)) = 0
        [HDR]_AddColor ("Add Color", Color) = (0, 0, 0, 1)
        
        [Header(Light Setting)][Space(5)]
        [PowerSlider(3)] _LightColorIntensity ("Light Int：平行光强度", Range(0, 1)) = 0.15
        _SkyColorIntensity ("Sky Int：天空盒强度", Range(0, 1)) = 0.75
        _LightColorBlend ("Blend：灯光混合固有色", Range(0, 1)) = 1
        _PointLightColorIntensity ("Point Light Int：点光照明强度", Range(0, 1)) = 1
        _PointLightStep ("Point Light Step：阈值", Range(0, 2)) = 0.6
        [PowerSlider(4)] _PointLightFeather ("Point Light Feather：羽化", Range(0.0001, 1)) = 0.001
        
        
        [Title(_, Normal)]
        [Tex(_, _NormalScale)][Normal] _NormalMap ("NormalMap", 2D) = "bump" { }
        [HideInInspector] _NormalScale ("Normal Scale：深度", Range(-3.0, 3.0)) = 1.0
        
        [Main(_shadow, _, 2)]
        _shadow ("Shadow", float) = 0
        [Tex(_shadow)] _ShadowMap ("ShadowMap (RGBA)", 2D) = "black" { }
        [Sub(_shadow)] _ShadowFixedColor ("Color：固有阴影颜色", Color) = (0.5, 0.5, 0.5, 1)
        [SubToggle(_shadow, _ENABLE_SELFSHADOW)] _enable_selfshadow ("Enable Self Shadow", float) = 1
        [Title(_shadow, 1st Shadow)]
        [Tex(_shadow, _ShadowMapColor)]_ShadowColorMap ("ShadowColorMap (RGB)", 2D) = "white" { }
        [HideInInspector]_ShadowMapColor ("Color", Color) = (1, 1, 1, 1)
        [Sub(_shadow)]_ShadowIntensity ("Int：强度", Range(0, 1)) = 0.6 // 降低强度提高浓度有助于减少光照探针权重、提高固有色纯度
        [Sub(_shadow)]_Shadow_Power ("Power：浓度", Range(0, 1)) = 0.5
        [Sub(_shadow)]_Shadow_Step ("Step：阈值", Range(0, 1)) = 0.55
        [SubPowerSlider(_shadow, 6)] _Shadow_Feather ("Feather：羽化", Range(0.0001, 1)) = 0.0001
        [Title(_shadow, 2st Shadow)]
        [Sub(_shadow)][HDR] _ShadowColor2 ("Color", Color) = (0, 0, 0, 1)
        [Sub(_shadow)]_ShadowIntensity2 ("Int：强度", Range(-1, 1)) = -0.5
        [Sub(_shadow)]_ShadowColorBlend2 ("Blend：混合", Range(0, 1)) = 0.8
        [Sub(_shadow)]_Shadow_Step2 ("Step：阈值", Range(0, 1)) = 0.2
        [SubPowerSlider(_shadow, 6)] _Shadow_Feather2 ("Feather：羽化", Range(0.0001, 1)) = 0.001
        
        
        [Main(_HL)]
        _Enable_HighLight ("HighLight", float) = 0
        [Tex(_HL)] _LightMap ("LightMap (RGBA)", 2D) = "white" { }
        [KWEnum(_HL, NPR, _HL_NPR, PBR, _HL_PBR)]
        _HighLight_Mode ("HighLight Mode", float) = 0
        
        [SubPowerSlider(_HL, 2)] _HighColorLevel ("Level：强度偏移", Range(-1, 1)) = 0
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
        
        
        [Main(MatCap)]
        _MatCap_Enable ("MatCap", float) = 0
        [Sub(MatCap)] _MatCap_Sampler ("MatCap Map (R)", 2D) = "black" { }
        [Sub(MatCap)] _BumpScaleMatcap ("Noise Int：扰动强度", Range(-3, 3)) = 1
        [Sub(MatCap)] _MatCapNormalMap ("Noise Map", 2D) = "black" { }
        [Sub(MatCap)] [HDR] _MatCapColor ("MatCap Color", Color) = (1, 1, 1, 1)
        [SubPowerSlider(MatCap, 3)] _MatCap ("Int：强度", Range(0, 1)) = 0.1
        [SubPowerSlider(MatCap, 2)] _TweakMatCapOnShadow ("Int On Shadow：阴影中强度", Range(0, 1)) = 0.25
        [SubPowerSlider(MatCap, 2)] _Tweak_MatcapMaskLevel ("Level：强度偏移", Range(-1, 1)) = 0
        [SubPowerSlider(MatCap, 2)] _BlurLevelMatcap ("Blur Level：模糊", Range(0, 10)) = 0
        
        
        [Main(Rim)]
        _RimLight_Enable ("RimLight", float) = 0
        [KWEnum(Rim, Normal, _Rim_Normal, Screen Space, _Rim_SS)]
        _RimLight_Mode ("HighLight Mode", float) = 0
        
        [Title(Rim_Rim_Normal, Bright Side)]
        [Sub(Rim)] [HDR] _RimLightColor ("RimLight Color：边缘光", Color) = (1, 1, 1, 1)
        [Sub(Rim)] _RimLightIntensity ("Int：强度", Range(0, 1)) = 1
        [Sub(Rim)] _RimLightBlend ("Blend：混合固有色", Range(0, 1)) = 0.5
        [Sub(Rim)] _RimLightBlendPoint ("Blend Point：混合点光", Range(0, 1)) = 0.35
        [Title(Rim_Rim_Normal, Dark Side)]
        [Sub(Rim_Rim_Normal)] [HDR] _RimLightColor2 ("RimLight Color2：边缘光", Color) = (1, 1, 1, 1)
        [Sub(Rim_Rim_Normal)] _RimLightIntensity2 ("Int2：强度", Range(0, 1)) = 1
        [Sub(Rim_Rim_Normal)] _RimLightBlend2 ("Blend2：混合固有色", Range(0, 1)) = 0.5
        [Sub(Rim_Rim_Normal)] _RimLightBlendPoint2 ("Blend Point2：混合点光", Range(0, 1)) = 0.35
        
        [Title(Rim, Rim Setting)]
        [SubPowerSlider(Rim, 5)] _RimLightFeather ("Feather：羽化", Range(0.0001, 1)) = 0.005
        [SubPowerSlider(Rim, 1.5)] _RimLightWidth ("Width：宽度", Range(0, 1)) = 0.3
        [SubPowerSlider(Rim, 0.35)] _RimLightLength ("Length：长度", Range(0, 10)) = 7
        [SubPowerSlider(Rim, 2)] _RimLightLevel ("Level：强度偏移", Range(-1, 1)) = 0
        [Sub(Rim_Rim_SS)] _RimLightIntInShadow ("Int：暗面强度", Range(0, 1)) = 0.35
        [Sub(Rim_Rim_SS)] _RimLightSNBlend ("Blend：混合平滑法线", Range(0, 1)) = 0.5
        
        [Title(Rim, Add Rim)]
        [Sub(Rim)] [HDR] _RimLightColor3 ("Color2：额外边缘光", Color) = (0, 0, 0, 1)
        [Sub(Rim)] _RimLightIntensity3 ("Int：强度", Range(0, 1)) = 0
        [Sub(Rim)] _RimLightBlend3 ("Blend：混合固有色", Range(0, 1)) = 0
        [SubPowerSlider(Rim, 8)] _RimLightFeather3 ("Feather：羽化", Range(0, 1)) = 0.005
        [SubPowerSlider(Rim, 1.5)] _RimLightWidth3 ("Width：宽度", Range(0, 1)) = 0.3
        [SubPowerSlider(Rim, 0.35)] _RimLightLength3 ("Length：长度", Range(0, 10)) = 10
        
        
        [Main(Emi)]
        _Emissive_Enable ("Emissive", float) = 0
        [Sub(Emi)] _Emissive_Mask1 ("Mask1：自发光遮罩", 2D) = "white" { }
        [Sub(Emi)] [HDR] _Emissive_ColorA1 ("主色", Color) = (1, 1, 1, 1)
        [Sub(Emi)] [HDR] _Emissive_ColorB1 ("副色", Color) = (1, 1, 1, 1)
        [Sub(Emi)] _Emissive_IntA1 ("Int：主色强度", Range(0, 1)) = 0
        [Sub(Emi)] _Emissive_IntB1 ("Int：副色强度", Range(0, 1)) = 0
        [Sub(Emi)] _Emissive_Level1 ("Level：强度偏移", Range(-1, 1)) = 0
        [SubPowerSlider(Emi, 4)] _Emissive_MoveHor1 ("Move：横向速度", Range(-10, 10)) = 0
        [SubPowerSlider(Emi, 4)] _Emissive_MoveVer1 ("Move：竖向速度", Range(-10, 10)) = 0
        [SubPowerSlider(Emi, 2)] _Emissive_Speed1 ("Speed：切换速度", Range(0, 10)) = 0
        [Sub(Emi)] _Emissive_Mask2 ("Mask2：自发光遮罩", 2D) = "white" { }
        [Sub(Emi)] [HDR] _Emissive_ColorA2 ("主色", Color) = (1, 1, 1, 1)
        [Sub(Emi)] [HDR] _Emissive_ColorB2 ("副色", Color) = (1, 1, 1, 1)
        [Sub(Emi)] _Emissive_IntA2 ("Int：主色强度", Range(0, 1)) = 0
        [Sub(Emi)] _Emissive_IntB2 ("Int：副色强度", Range(0, 1)) = 0
        [Sub(Emi)] _Emissive_Level2 ("Level：强度偏移", Range(-1, 1)) = 0
        [SubPowerSlider(Emi, 4)] _Emissive_MoveHor2 ("Move：横向速度", Range(-10, 10)) = 0
        [SubPowerSlider(Emi, 4)] _Emissive_MoveVer2 ("Move：竖向速度", Range(-10, 10)) = 0
        [SubPowerSlider(Emi, 2)] _Emissive_Speed2 ("Speed：切换速度", Range(0, 10)) = 0
        
        
        [Main(OutLine)]
        _OutLine_Enable ("OutLine", float) = 1
        [Sub(OutLine)] _Outline_Color ("Outline Color", Color) = (0.5, 0.5, 0.5, 1)
        [Sub(OutLine)] _Outline_Width ("Width：宽度", float) = 2
        [SubToggle(OutLine)] _OriginNormal ("Origin Normal：原始法线", float) = 0
        [Sub(OutLine)] _Offset_Z ("Offset Z：深度偏移", float) = 0
        [Sub(OutLine)] _Outline_Blend ("Blend：颜色混合", Range(0, 1)) = 1
        [SubPowerSlider(OutLine, 1.7)] _Outline_Purity ("Purity：纯度", Range(-1, 1)) = 0
        [SubPowerSlider(OutLine, 1.7)] _Outline_Lightness ("Lightness：明度", Range(-1, 1)) = 0
        
        
        [HideInInspector]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
        [HideInInspector]_BaseColorMap ("BaseColorMap", 2D) = "white" { }
        
        // Versioning of material to help for upgrading
        [HideInInspector] _HdrpVersion ("_HdrpVersion", Float) = 2
        
        //#region Descrption
        // Blending state
        [HideInInspector] _SurfaceType ("__surfacetype", Float) = 0.0
        [HideInInspector] _BlendMode ("__blendmode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _AlphaSrcBlend ("__alphaSrc", Float) = 1.0
        [HideInInspector] _AlphaDstBlend ("__alphaDst", Float) = 0.0

        // [HideInInspector] _CullMode ("__cullmode", Float) = 2.0
        [HideInInspector] _CullModeForward ("__cullmodeForward", Float) = 2.0 // This mode is dedicated to Forward to correctly handle backface then front face rendering thin transparent
        // [Enum(UnityEditor.Rendering.HighDefinition.TransparentCullMode)] _TransparentCullMode ("_TransparentCullMode", Int) = 2 // Back culling by default
        [HideInInspector] _ZTestDepthEqualForOpaque ("_ZTestDepthEqualForOpaque", Int) = 4 // Less equal
        [HideInInspector] _ZTestModeDistortion ("_ZTestModeDistortion", Int) = 8
        [HideInInspector] _ZTestGBuffer ("_ZTestGBuffer", Int) = 4
        // [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestTransparent ("Transparent ZTest", Int) = 4 // Less equal
        [HideInInspector][ToggleUI] _EnableFogOnTransparent ("Enable Fog", Float) = 1.0
        // [ToggleUI] _EnableBlendModePreserveSpecularLighting ("Enable Blend Mode Preserve Specular Lighting", Float) = 1.0
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Planar, 4, Triplanar, 5)] _UVBase ("UV Set for base", Float) = 0
        [HideInInspector]_TexWorldScale ("Scale to apply on world coordinate", Float) = 1.0
        [HideInInspector] _InvTilingScale ("Inverse tiling scale = 2 / (abs(_BaseColorMap_ST.x) + abs(_BaseColorMap_ST.y))", Float) = 1
        [HideInInspector] _UVMappingMask ("_UVMappingMask", Color) = (1, 0, 0, 0)
        [HideInInspector][Enum(TangentSpace, 0, ObjectSpace, 1)] _NormalMapSpace ("NormalMap space", Float) = 0
        [HideInInspector] _DiffusionProfile ("Obsolete, kept for migration purpose", Int) = 0
        [HideInInspector] _DiffusionProfileAsset ("Diffusion Profile Asset", Vector) = (0, 0, 0, 0)
        [HideInInspector] _DiffusionProfileHash ("Diffusion Profile Hash", Float) = 0
        //#endregion
    }
    
    HLSLINCLUDE
    
    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    
    #define ATTRIBUTES_NEED_TEXCOORD2
    #define VARYINGS_NEED_TEXCOORD2
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
    
    #include "ShaderLibrary\LitProperties.hlsl"
    
    ENDHLSL
    
    SubShader
    {
        // This tags allow to use the shader replacement features
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDLitShader" /* "Queue" = "Transparent+100"*/ }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            Cull[_CullMode]
            
            ZClip [_ZClip]
            ZWrite On
            ZTest LEqual
            
            ColorMask 0
            
            HLSLPROGRAM
            
            #define SHADERPASS SHADERPASS_SHADOWS
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitDepthPass.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Frag
            
            ENDHLSL
            
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            Cull[_CullMode]
            
            // To be able to tag stencil with disableSSR information for forward
            Stencil
            {
                WriteMask [_StencilWriteMaskDepth]
                Ref [_StencilRefDepth]
                Comp Always
                Pass Replace
            }
            
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            
            // In deferred, depth only pass don't output anything.
            // In forward it output the normal buffer
            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile _ WRITE_MSAA_DEPTH
            
            #define SHADERPASS SHADERPASS_DEPTH_ONLY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            
            #ifdef WRITE_NORMAL_BUFFER // If enabled we need all regular interpolator
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
            #else
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitDepthPass.hlsl"
            #endif
            
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Frag
            
            ENDHLSL
            
        }
        
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardOnly" }// This will be only for transparent object based on the RenderQueue index
            Cull [_CullMode]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            
            // Supported shadow modes per light type
            #pragma multi_compile SHADOW_HIGH SHADOW_LOW SHADOW_MEDIUM
            #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
            
            #pragma shader_feature_local _ _ENABLE_SELFSHADOW
            #pragma shader_feature_local _ _ENABLE_HIGHLIGHT_ON
            #pragma shader_feature_local _HL_NPR _HL_PBR
            #pragma shader_feature_local _ _RIMLIGHT_ENABLE_ON
            #pragma shader_feature_local _ _MATCAP_ENABLE_ON
            #pragma shader_feature_local _ _EMISSIVE_ENABLE_ON
            
            #define ATTRIBUTES_NEED_TEXCOORD7
            #define VARYINGS_NEED_TEXCOORD7

            #include "ShaderLibrary/ShaderPassForward.hlsl"
            
            #pragma vertex Vert
            #pragma fragment Frag
            
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
            
            #include "../ShaderLibrary/ShaderPassOutline.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDHLSL
            
        }
    }
    CustomEditor "JTRP.ShaderDrawer.LWGUI"
}

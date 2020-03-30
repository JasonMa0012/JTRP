Shader "JNGO/FX/Standard Particles"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        _BumpScale ("Scale", Float) = 1.0
        _DistortionMap ("Normal Map", 2D) = "white" { }
        
        _EmissionColor ("Color", Color) = (0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" { }
        
        _DistortionStrength ("Strength", Float) = 1.0
        _DistortionBlend ("Blend", Range(0.0, 1.0)) = 0.5
        
        _SoftParticlesNearFadeDistance ("Soft Particles Near Fade", Float) = 0.0
        _SoftParticlesFarFadeDistance ("Soft Particles Far Fade", Float) = 1.0
        _CameraNearFadeDistance ("Camera Near Fade", Float) = 1.0
        _CameraFarFadeDistance ("Camera Far Fade", Float) = 2.0
        
        // Hidden properties
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _ColorMode ("__colormode", Float) = 0.0
        [HideInInspector] _FlipbookMode ("__flipbookmode", Float) = 0.0
        [HideInInspector] _LightingEnabled ("__lightingenabled", Float) = 0.0
        [HideInInspector] _DistortionEnabled ("__distortionenabled", Float) = 0.0
        [HideInInspector] _EmissionEnabled ("__emissionenabled", Float) = 0.0
        [HideInInspector] _BlendOp ("__blendop", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _Cull ("__cull", Float) = 2.0
        [HideInInspector] _SoftParticlesEnabled ("__softparticlesenabled", Float) = 0.0
        [HideInInspector] _CameraFadingEnabled ("__camerafadingenabled", Float) = 0.0
        [HideInInspector] _SoftParticleFadeParams ("__softparticlefadeparams", Vector) = (0, 0, 0, 0)
        [HideInInspector] _CameraFadeParams ("__camerafadeparams", Vector) = (0, 0, 0, 0)
        [HideInInspector] _ColorAddSubDiff ("__coloraddsubdiff", Vector) = (0, 0, 0, 0)
        [HideInInspector] _DistortionStrengthScaled ("__distortionstrengthscaled", Float) = 0.0
        
        
        [HideInInspector]_EmissionColor ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_RenderQueueType ("Vector1", Float) = 5
        [HideInInspector]_StencilRef ("Vector1", Int) = 2
        [HideInInspector]_StencilWriteMask ("Vector1", Int) = 3
        [HideInInspector]_StencilRefDepth ("Vector1", Int) = 32
        [HideInInspector]_StencilWriteMaskDepth ("Vector1", Int) = 48
        [HideInInspector]_StencilRefMV ("Vector1", Int) = 160
        [HideInInspector]_StencilWriteMaskMV ("Vector1", Int) = 176
        [HideInInspector]_StencilRefDistortionVec ("Vector1", Int) = 64
        [HideInInspector]_StencilWriteMaskDistortionVec ("Vector1", Int) = 64
        [HideInInspector]_StencilWriteMaskGBuffer ("Vector1", Int) = 51
        [HideInInspector]_StencilRefGBuffer ("Vector1", Int) = 34
        [HideInInspector]_ZTestGBuffer ("Vector1", Int) = 4
        [HideInInspector][ToggleUI]_RequireSplitLighting ("Boolean", Float) = 0
        [HideInInspector][ToggleUI]_ReceivesSSR ("Boolean", Float) = 0
        [HideInInspector]_SurfaceType ("Vector1", Float) = 1
        [HideInInspector]_BlendMode ("Vector1", Float) = 0
        [HideInInspector][ToggleUI]_ZWrite ("Boolean", Float) = 0
        [HideInInspector]_TransparentSortPriority ("Vector1", Int) = 0
        [HideInInspector]_CullModeForward ("Vector1", Float) = 2
        [HideInInspector][Enum(Front, 1, Back, 2)]_TransparentCullMode ("Vector1", Float) = 2
        [HideInInspector]_ZTestDepthEqualForOpaque ("Vector1", Int) = 4
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestTransparent ("Vector1", Float) = 4
        [HideInInspector][ToggleUI]_TransparentBackfaceEnable ("Boolean", Float) = 0
        [HideInInspector][ToggleUI]_AlphaCutoffEnable ("Boolean", Float) = 0
        [HideInInspector][ToggleUI]_UseShadowThreshold ("Boolean", Float) = 0
        [HideInInspector][ToggleUI]_DoubleSidedEnable ("Boolean", Float) = 0
        [HideInInspector][Enum(Flip, 0, Mirror, 1, None, 2)]_DoubleSidedNormalMode ("Vector1", Float) = 2
        [HideInInspector]_DoubleSidedConstants ("Vector4", Vector) = (1, 1, -1, 0)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDUnlitShader" "Queue" = "Transparent+0" // "IgnoreProjector" = "True" "PreviewType" = "Plane"
            //"PerformanceChecks" = "False"
        }
        
        Pass
        {
            // based on HDUnlitPass.template
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            
            //-------------------------------------------------------------------------------------
            // Render Modes (Blend, Cull, ZTest, Stencil, etc)
            //-------------------------------------------------------------------------------------
            BlendOp [_BlendOp]
            Blend [_SrcBlend] [_DstBlend]
            ColorMask RGBA
            Cull [_Cull]
            ZTest [_ZTestTransparent]
            ZWrite [_ZWrite]
            
            // Stencil setup
            Stencil
            {
                WriteMask [_StencilWriteMask]
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }
            
            
            //-------------------------------------------------------------------------------------
            // End Render Modes
            //-------------------------------------------------------------------------------------
            
            HLSLPROGRAM
            
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            //#pragma enable_d3d11_debug_symbols
            
            //-------------------------------------------------------------------------------------
            // Variant
            //-------------------------------------------------------------------------------------
            
            // #pragma shader_feature_local _DOUBLESIDED_ON - We have no lighting, so no need to have this combination for shader, the option will just disable backface culling
            
            // Keyword for transparent
            #pragma shader_feature _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
            
            #pragma multi_compile __ SOFTPARTICLES_ON
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _FADING_ON
            #pragma shader_feature _REQUIRE_UV2
            #pragma shader_feature EFFECT_BUMP
            
            
            #define _ENABLE_FOG_ON_TRANSPARENT 1
            // #define _ADD_PRECOMPUTED_VELOCITY
            // #define _ENABLE_SHADOW_MATTE
            
            //enable GPU instancing support
            #pragma multi_compile_instancing
            
            //-------------------------------------------------------------------------------------
            // End Variant Definitions
            //-------------------------------------------------------------------------------------
            
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            
            //-------------------------------------------------------------------------------------
            // Defines
            //-------------------------------------------------------------------------------------
            #define SHADERPASS SHADERPASS_FORWARD_UNLIT
            #pragma multi_compile _ DEBUG_DISPLAY
            #define REQUIRE_DEPTH_TEXTURE
            // ACTIVE FIELDS:
            //   AlphaFog
            //   SurfaceDescriptionInputs.ScreenPosition
            //   SurfaceDescriptionInputs.VertexColor
            //   SurfaceDescriptionInputs.uv0
            //   SurfaceDescriptionInputs.uv1
            //   VertexDescriptionInputs.ObjectSpaceNormal
            //   VertexDescriptionInputs.ObjectSpaceTangent
            //   VertexDescriptionInputs.ObjectSpacePosition
            //   SurfaceDescription.Color
            //   SurfaceDescription.Alpha
            //   SurfaceDescription.AlphaClipThreshold
            //   SurfaceDescription.Emission
            //   SurfaceDescriptionInputs.WorldSpacePosition
            //   FragInputs.color
            //   FragInputs.texCoord0
            //   FragInputs.texCoord1
            //   AttributesMesh.normalOS
            //   AttributesMesh.tangentOS
            //   AttributesMesh.positionOS
            //   FragInputs.positionRWS
            //   VaryingsMeshToPS.color
            //   VaryingsMeshToPS.texCoord0
            //   VaryingsMeshToPS.texCoord1
            //   VaryingsMeshToPS.positionRWS
            //   AttributesMesh.color
            //   AttributesMesh.uv0
            //   AttributesMesh.uv1
            // Shared Graph Keywords
            
            // this translates the new dependency tracker into the old preprocessor definitions for the existing HDRP shader code
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            // #define ATTRIBUTES_NEED_TEXCOORD2
            // #define ATTRIBUTES_NEED_TEXCOORD3
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            // #define VARYINGS_NEED_TANGENT_TO_WORLD
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1
            // #define VARYINGS_NEED_TEXCOORD2
            // #define VARYINGS_NEED_TEXCOORD3
            #define VARYINGS_NEED_COLOR
            // #define VARYINGS_NEED_CULLFACE
            // #define HAVE_MESH_MODIFICATION
            
            #if defined(_ENABLE_SHADOW_MATTE) && SHADERPASS == SHADERPASS_FORWARD_UNLIT
                #define LIGHTLOOP_DISABLE_TILE_AND_CLUSTER
                #define HAS_LIGHTLOOP
                #define SHADOW_OPTIMIZE_REGISTER_USAGE 1
                
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonLighting.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowContext.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadow.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/PunctualLightCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadowLoop.hlsl"
            #endif
            
            //-------------------------------------------------------------------------------------
            // End Defines
            //-------------------------------------------------------------------------------------
            
            
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
            
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
            
            // Used by SceneSelectionPass
            int _ObjectId;
            int _PassValue;
            
            //-------------------------------------------------------------------------------------
            // Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
            //#region 结构体
            
            // Generated Type: AttributesMesh
            struct AttributesMesh
            {
                float3 positionOS: POSITION;
                float3 normalOS: NORMAL; // optional
                float4 tangentOS: TANGENT; // optional
                float4 uv0: TEXCOORD0; // optional
                float4 uv1: TEXCOORD1; // optional
                float4 color: COLOR; // optional
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID: INSTANCEID_SEMANTIC;
                #endif // UNITY_ANY_INSTANCING_ENABLED
            };
            // Generated Type: VaryingsMeshToPS
            struct VaryingsMeshToPS
            {
                float4 positionCS: SV_Position;
                float3 positionRWS; // optional
                float4 texCoord0; // optional
                float4 texCoord1; // optional
                float4 color; // optional
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID: CUSTOM_INSTANCE_ID;
                #endif // UNITY_ANY_INSTANCING_ENABLED
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace: FRONT_FACE_SEMANTIC;
                #endif // defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            };
            
            // Generated Type: PackedVaryingsMeshToPS
            struct PackedVaryingsMeshToPS
            {
                float4 positionCS: SV_Position; // unpacked
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID: CUSTOM_INSTANCE_ID; // unpacked
                #endif // conditional
                float3 interp00: TEXCOORD0; // auto-packed
                float4 interp01: TEXCOORD1; // auto-packed
                float4 interp02: TEXCOORD2; // auto-packed
                float4 interp03: TEXCOORD3; // auto-packed
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace: FRONT_FACE_SEMANTIC; // unpacked
                #endif // conditional
            };
            
            // Packed Type: VaryingsMeshToPS
            PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
            {
                PackedVaryingsMeshToPS output;
                output.positionCS = input.positionCS;
                output.interp00.xyz = input.positionRWS;
                output.interp01.xyzw = input.texCoord0;
                output.interp02.xyzw = input.texCoord1;
                output.interp03.xyzw = input.color;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif // conditional
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif // conditional
                return output;
            }
            
            // Unpacked Type: VaryingsMeshToPS
            VaryingsMeshToPS UnpackVaryingsMeshToPS(PackedVaryingsMeshToPS input)
            {
                VaryingsMeshToPS output;
                output.positionCS = input.positionCS;
                output.positionRWS = input.interp00.xyz;
                output.texCoord0 = input.interp01.xyzw;
                output.texCoord1 = input.interp02.xyzw;
                output.color = input.interp03.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif // conditional
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif // conditional
                return output;
            }
            // Generated Type: VaryingsMeshToDS
            struct VaryingsMeshToDS
            {
                float3 positionRWS;
                float3 normalWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID: CUSTOM_INSTANCE_ID;
                #endif // UNITY_ANY_INSTANCING_ENABLED
            };
            
            // Generated Type: PackedVaryingsMeshToDS
            struct PackedVaryingsMeshToDS
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID: CUSTOM_INSTANCE_ID; // unpacked
                #endif // conditional
                float3 interp00: TEXCOORD0; // auto-packed
                float3 interp01: TEXCOORD1; // auto-packed
            };
            
            // Packed Type: VaryingsMeshToDS
            PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
            {
                PackedVaryingsMeshToDS output;
                output.interp00.xyz = input.positionRWS;
                output.interp01.xyz = input.normalWS;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif // conditional
                return output;
            }
            
            // Unpacked Type: VaryingsMeshToDS
            VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
            {
                VaryingsMeshToDS output;
                output.positionRWS = input.interp00.xyz;
                output.normalWS = input.interp01.xyz;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif // conditional
                return output;
            }
            //#endregion
            //-------------------------------------------------------------------------------------
            // End Interpolator Packing And Struct Declarations
            //-------------------------------------------------------------------------------------
            
            //-------------------------------------------------------------------------------------
            // Graph generated code
            //-------------------------------------------------------------------------------------
            // Shared Graph Properties (uniform inputs)
            
            #if _REQUIRE_UV2
                #define _FLIPBOOK_BLENDING 1
            #endif
            
            #if EFFECT_BUMP
                #define _DISTORTION_ON 1
            #endif
            
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DistortionMap);
            SAMPLER(sampler_DistortionMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);
            
            CBUFFER_START(UnityPerMaterial)
            
            float4 _MainTex_ST;
            float4 _Color;
            float _BumpScale;
            float3 _EmissionColor;
            float _Metallic;
            float _Glossiness;
            // UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _SoftParticleFadeParams;
            float4 _CameraFadeParams;
            float _Cutoff;
            
            #if _DISTORTION_ON
                float _DistortionStrengthScaled;
                float _DistortionBlend;
            #endif
            
            #if defined(_COLORADDSUBDIFF_ON)
                float4 _ColorAddSubDiff;
            #endif
            
            
            // float4 _EmissionColor;
            float _RenderQueueType;
            float _StencilRef;
            float _StencilWriteMask;
            float _StencilRefDepth;
            float _StencilWriteMaskDepth;
            float _StencilRefMV;
            float _StencilWriteMaskMV;
            float _StencilRefDistortionVec;
            float _StencilWriteMaskDistortionVec;
            float _StencilWriteMaskGBuffer;
            float _StencilRefGBuffer;
            float _ZTestGBuffer;
            float _RequireSplitLighting;
            float _ReceivesSSR;
            float _SurfaceType;
            float _BlendMode;
            float _SrcBlend;
            float _DstBlend;
            // float _SrcBlend;
            float _ZWrite;
            float _Cull;
            float _TransparentSortPriority;
            float _CullModeForward;
            float _TransparentCullMode;
            float _ZTestDepthEqualForOpaque;
            float _ZTestTransparent;
            float _TransparentBackfaceEnable;
            float _AlphaCutoffEnable;
            float _UseShadowThreshold;
            float _DoubleSidedEnable;
            float _DoubleSidedNormalMode;
            float4 _DoubleSidedConstants;
            
            CBUFFER_END
            
            // Pixel Graph Inputs
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition; // optional
                float4 ScreenPosition; // optional
                float4 uv0; // optional
                float4 uv1; // optional
                float4 VertexColor; // optional
            };
            // Pixel Graph Outputs
            struct SurfaceDescription
            {
                float3 Color;
                float Alpha;
                float AlphaClipThreshold;
                float3 Emission;
            };
            
            #define SOFT_PARTICLE_NEAR_FADE _SoftParticleFadeParams.x
            #define SOFT_PARTICLE_INV_FADE_DISTANCE _SoftParticleFadeParams.y
            
            #define CAMERA_NEAR_FADE _CameraFadeParams.x
            #define CAMERA_INV_FADE_DISTANCE _CameraFadeParams.y
            
            // Shared Graph Node Functions
            
            void Unity_SceneDepth_Linear01_float(float4 UV, out float Out)
            {
                Out = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
            
            void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
            {
                Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
            }
            
            
            void Unity_SceneColor_float(float4 UV, out float3 Out)
            {
                Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
            }
            
            
            float4 readTexture(Texture2D tex, SamplerState sam, SurfaceDescriptionInputs IN, float4 ST)
            {
                float4 color = SAMPLE_TEXTURE2D(tex, sam, IN.uv0.xy * ST.xy + ST.zw);
                #ifdef _FLIPBOOK_BLENDING
                    float4 color2 = SAMPLE_TEXTURE2D(tex, sam, IN.uv1.xy * ST.xy + ST.zw);
                    color = lerp(color, color2, IN.uv1.z);
                #endif
                return color;
            }
            
            // Color blending fragment function
            
            void fragColorMode(SurfaceDescriptionInputs i, inout float4 albedo)
            {
                #if defined(_COLOROVERLAY_ON)
                    albedo.rgb = lerp(1 - 2 * (1 - albedo.rgb) * (1 - i.VertexColor.rgb), 2 * albedo.rgb * i.VertexColor.rgb, step(albedo.rgb, 0.5));
                    albedo.a *= i.VertexColor.a;
                #elif defined(_COLORCOLOR_ON)
                    float3 aHSL = RgbToHsv(albedo.rgb);
                    float3 bHSL = RgbToHsv(i.VertexColor.rgb);
                    float3 rHSL = float3(bHSL.x, bHSL.y, aHSL.z);
                    albedo = float4(HsvToRgb(rHSL), albedo.a * i.VertexColor.a);
                #elif defined(_COLORADDSUBDIFF_ON)
                    albedo.rgb = albedo.rgb + i.VertexColor.rgb * _ColorAddSubDiff.x;
                    albedo.rgb = lerp(albedo.rgb, abs(albedo.rgb), _ColorAddSubDiff.y);
                    albedo.a *= i.VertexColor.a;
                #else
                    albedo *= i.VertexColor;
                #endif
            }
            
            // Pre-multiplied alpha helper
            #if defined(_ALPHAPREMULTIPLY_ON)
                #define ALBEDO_MUL albedo
            #else
                #define ALBEDO_MUL albedo.a
            #endif
            
            void fragSoftParticles(SurfaceDescriptionInputs i, float objDepth, float sceneDepth, inout float4 albedo, out float softParticlesFade)
            {
                softParticlesFade = 1.0f;
                #if defined(SOFTPARTICLES_ON) || defined(_FADING_ON)
                    if (SOFT_PARTICLE_NEAR_FADE > 0.0 || SOFT_PARTICLE_INV_FADE_DISTANCE > 0.0)
                    {
                        softParticlesFade = saturate(SOFT_PARTICLE_INV_FADE_DISTANCE * ((sceneDepth - SOFT_PARTICLE_NEAR_FADE) - objDepth));
                        ALBEDO_MUL *= softParticlesFade;
                    }
                #endif
            }
            
            
            void fragCameraFading(SurfaceDescriptionInputs i, float objDepth, inout float4 albedo, out float cameraFade)
            {
                cameraFade = 1.0f;
                #if defined(_FADING_ON)
                    cameraFade = saturate((objDepth - CAMERA_NEAR_FADE) * CAMERA_INV_FADE_DISTANCE);
                    ALBEDO_MUL *= cameraFade;
                #endif
            }
            
            
            void fragDistortion(SurfaceDescriptionInputs i, float4 screenUV, float3 normal, inout float4 albedo)
            {
                #if _DISTORTION_ON
                    screenUV.xy += normal.xy * _DistortionStrengthScaled * albedo.a;
                    float3 grabPass = SampleCameraColor(screenUV);
                    albedo.rgb = lerp(grabPass, albedo.rgb, saturate(albedo.a - _DistortionBlend));
                #endif
            }
            
            // 片元
            // Pixel Graph Evaluation
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 _ScreenPosition = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
                float _SceneDepth;// 场景深度
                float _EyeDepth;// 当前物体深度
                float4 defaultST = float4(1, 1, 0, 0);
                // Unity_SceneDepth_Linear01_float(_ScreenPosition, _SceneDepth);
                Unity_SceneDepth_Eye_float(_ScreenPosition, _SceneDepth);
                _EyeDepth = LinearEyeDepth(IN.WorldSpacePosition.xyz, UNITY_MATRIX_V);
                
                surface.Color = (_SceneDepth.xxx);
                surface.AlphaClipThreshold = 0.5;
                surface.Emission = float3(0, 0, 0);
                
                float4 albedo = readTexture(_MainTex, sampler_MainTex, IN, _MainTex_ST);
                albedo *= _Color;
                
                fragColorMode(IN, albedo);
                float softParticlesFade, cameraFade;
                fragSoftParticles(IN, _EyeDepth, _SceneDepth, albedo, softParticlesFade);
                fragCameraFading(IN, _EyeDepth, albedo, cameraFade);
                
                #if defined(_NORMALMAP)
                    float3 normal = readTexture(_DistortionMap, sampler_DistortionMap, IN, defaultST), _BumpScale;
                #else
                    float3 normal = float3(0, 0, 1);
                #endif
                
                #if defined(_EMISSION)
                    float3 emission = readTexture(_EmissionMap, sampler_EmissionMap, IN, defaultST).rgb;
                #else
                    float3 emission = 0;
                #endif
                
                fragDistortion(IN, _ScreenPosition, normal, albedo);
                
                half4 result = albedo;
                
                #if defined(_ALPHAMODULATE_ON)
                    result.rgb = lerp(float3(1.0, 1.0, 1.0), albedo.rgb, albedo.a);
                #endif
                
                result.rgb += emission * _EmissionColor * cameraFade * softParticlesFade;
                
                #if !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAOVERLAY_ON)
                    result.a = 1;
                #endif
                
                #if defined(_ALPHATEST_ON)
                    clip(albedo.a - _Cutoff + 0.0001);
                #endif
                
                surface.Color = result.rgb;
                surface.Alpha = result.a;
                
                return surface;
            }
            
            //#region 数据收集函数
            
            //-------------------------------------------------------------------------------------
            // End graph generated code
            //-------------------------------------------------------------------------------------
            
            // $include("VertexAnimation.template.hlsl")
            
            //-------------------------------------------------------------------------------------
            // TEMPLATE INCLUDE : SharedCode.template.hlsl
            //-------------------------------------------------------------------------------------
            
            FragInputs BuildFragInputs(VaryingsMeshToPS input)
            {
                FragInputs output;
                ZERO_INITIALIZE(FragInputs, output);
                
                // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                // to compute normals which are then passed on elsewhere to compute other values...
                output.tangentToWorld = k_identity3x3;
                output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                output.positionRWS = input.positionRWS;
                // output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                output.texCoord0 = input.texCoord0;
                output.texCoord1 = input.texCoord1;
                // output.texCoord2 = input.texCoord2;
                // output.texCoord3 = input.texCoord3;
                output.color = input.color;
                #if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
                    output.isFrontFace = IS_FRONT_VFACE(input.cullFace, true, false);
                #elif SHADER_STAGE_FRAGMENT
                    // output.isFrontFace = IS_FRONT_VFACE(input.cullFace, true, false);
                #endif // SHADER_STAGE_FRAGMENT
                
                return output;
            }
            
            SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                // output.WorldSpaceNormal =            normalize(input.tangentToWorld[2].xyz);
                // output.ObjectSpaceNormal =           mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_M);           // transposed multiplication by inverse matrix to handle normal scale
                // output.ViewSpaceNormal =             mul(output.WorldSpaceNormal, (float3x3) UNITY_MATRIX_I_V);         // transposed multiplication by inverse matrix to handle normal scale
                // output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                // output.WorldSpaceTangent =           input.tangentToWorld[0].xyz;
                // output.ObjectSpaceTangent =          TransformWorldToObjectDir(output.WorldSpaceTangent);
                // output.ViewSpaceTangent =            TransformWorldToViewDir(output.WorldSpaceTangent);
                // output.TangentSpaceTangent =         float3(1.0f, 0.0f, 0.0f);
                // output.WorldSpaceBiTangent =         input.tangentToWorld[1].xyz;
                // output.ObjectSpaceBiTangent =        TransformWorldToObjectDir(output.WorldSpaceBiTangent);
                // output.ViewSpaceBiTangent =          TransformWorldToViewDir(output.WorldSpaceBiTangent);
                // output.TangentSpaceBiTangent =       float3(0.0f, 1.0f, 0.0f);
                // output.WorldSpaceViewDirection =     normalize(viewWS);
                // output.ObjectSpaceViewDirection =    TransformWorldToObjectDir(output.WorldSpaceViewDirection);
                // output.ViewSpaceViewDirection =      TransformWorldToViewDir(output.WorldSpaceViewDirection);
                // float3x3 tangentSpaceTransform =     float3x3(output.WorldSpaceTangent,output.WorldSpaceBiTangent,output.WorldSpaceNormal);
                // output.TangentSpaceViewDirection =   mul(tangentSpaceTransform, output.WorldSpaceViewDirection);
                output.WorldSpacePosition = input.positionRWS;
                // output.ObjectSpacePosition =         TransformWorldToObject(input.positionRWS);
                // output.ViewSpacePosition =           TransformWorldToView(input.positionRWS);
                // output.TangentSpacePosition =        float3(0.0f, 0.0f, 0.0f);
                // output.AbsoluteWorldSpacePosition =  GetAbsolutePositionWS(input.positionRWS);
                output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionRWS), _ProjectionParams.x);
                output.uv0 = input.texCoord0;
                output.uv1 = input.texCoord1;
                // output.uv2 =                         input.texCoord2;
                // output.uv3 =                         input.texCoord3;
                output.VertexColor = input.color;
                // output.FaceSign =                    input.isFrontFace;
                // output.TimeParameters =              _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
                
                return output;
            }
            
            // existing HDRP code uses the combined function to go directly from packed to frag inputs
            FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                VaryingsMeshToPS unpacked = UnpackVaryingsMeshToPS(input);
                return BuildFragInputs(unpacked);
            }
            
            //-------------------------------------------------------------------------------------
            // END TEMPLATE INCLUDE : SharedCode.template.hlsl
            //-------------------------------------------------------------------------------------
            
            
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData)
            {
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                // copy across graph values, if defined
                surfaceData.color = surfaceDescription.Color;
                
                #if defined(DEBUG_DISPLAY)
                    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                    {
                        // TODO
                    }
                #endif
            }
            
            void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
            {
                SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
                
                // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                // TODO: split graph evaluation to grab just alpha dependencies first? tricky.
                // DoAlphaTest(surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold);
                
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
                
                #if defined(_ENABLE_SHADOW_MATTE) && SHADERPASS == SHADERPASS_FORWARD_UNLIT
                    HDShadowContext shadowContext = InitShadowContext();
                    float shadow;
                    float3 shadow3;
                    posInput = GetPositionInput(fragInputs.positionSS.xy, _ScreenSize.zw, fragInputs.positionSS.z, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
                    float3 normalWS = normalize(fragInputs.tangentToWorld[1]);
                    uint renderingLayers = _EnableLightLayers ? asuint(unity_RenderingLayer.x): DEFAULT_LIGHT_LAYERS;
                    ShadowLoopMin(shadowContext, posInput, normalWS, asuint(_ShadowMatteFilter), renderingLayers, shadow3);
                    shadow = dot(shadow3, float3(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f));
                    
                    float4 shadowColor = (1 - shadow) * surfaceDescription.ShadowTint.rgba;
                    float  localAlpha = saturate(shadowColor.a + surfaceDescription.Alpha);
                    
                    // Keep the nested lerp
                    // With no Color (bsdfData.color.rgb, bsdfData.color.a == 0.0f), just use ShadowColor*Color to avoid a ring of "white" around the shadow
                    // And mix color to consider the Color & ShadowColor alpha (from texture or/and color picker)
                    #ifdef _SURFACE_TYPE_TRANSPARENT
                        surfaceData.color = lerp(shadowColor.rgb * surfaceData.color, lerp(lerp(shadowColor.rgb, surfaceData.color, 1 - surfaceDescription.ShadowTint.a), surfaceData.color, shadow), surfaceDescription.Alpha);
                    #else
                        surfaceData.color = lerp(lerp(shadowColor.rgb, surfaceData.color, 1 - surfaceDescription.ShadowTint.a), surfaceData.color, shadow);
                    #endif
                    localAlpha = ApplyBlendMode(surfaceData.color, localAlpha).a;
                    
                    surfaceDescription.Alpha = localAlpha;
                #endif
                
                // Builtin Data
                ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                builtinData.opacity = surfaceDescription.Alpha;
                
                builtinData.emissiveColor = surfaceDescription.Emission;
                
                #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                #endif
            }
            
            //#endregion
            //-------------------------------------------------------------------------------------
            // Pass Includes
            //-------------------------------------------------------------------------------------
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"
            //-------------------------------------------------------------------------------------
            // End Pass Includes
            //-------------------------------------------------------------------------------------
            
            ENDHLSL
            
        }
    }
    // CustomEditor "UnityEditor.Rendering.HighDefinition.HDUnlitGUI"
    CustomEditor "HDStandardParticlesGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}

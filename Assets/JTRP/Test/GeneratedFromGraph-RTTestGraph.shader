Shader "JTRP/Test/RTTestGraph"
    {
        Properties
        {
            [HideInInspector]_EmissionColor("Color", Color) = (1, 1, 1, 1)
            [HideInInspector]_RenderQueueType("Float", Float) = 1
            [HideInInspector][ToggleUI]_AddPrecomputedVelocity("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_DepthOffsetEnable("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_TransparentWritingMotionVec("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_AlphaCutoffEnable("Boolean", Float) = 0
            [HideInInspector]_TransparentSortPriority("_TransparentSortPriority", Float) = 0
            [HideInInspector][ToggleUI]_UseShadowThreshold("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_DoubleSidedEnable("Boolean", Float) = 0
            [HideInInspector][Enum(Flip, 0, Mirror, 1, None, 2)]_DoubleSidedNormalMode("Float", Float) = 2
            [HideInInspector]_DoubleSidedConstants("Vector4", Vector) = (1, 1, -1, 0)
            [HideInInspector][ToggleUI]_TransparentDepthPrepassEnable("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_TransparentDepthPostpassEnable("Boolean", Float) = 0
            [HideInInspector]_SurfaceType("Float", Float) = 0
            [HideInInspector]_BlendMode("Float", Float) = 0
            [HideInInspector]_SrcBlend("Float", Float) = 1
            [HideInInspector]_DstBlend("Float", Float) = 0
            [HideInInspector]_AlphaSrcBlend("Float", Float) = 1
            [HideInInspector]_AlphaDstBlend("Float", Float) = 0
            [HideInInspector][ToggleUI]_AlphaToMask("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_AlphaToMaskInspectorValue("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_ZWrite("Boolean", Float) = 1
            [HideInInspector][ToggleUI]_TransparentZWrite("Boolean", Float) = 0
            [HideInInspector]_CullMode("Float", Float) = 2
            [HideInInspector][ToggleUI]_EnableFogOnTransparent("Boolean", Float) = 1
            [HideInInspector]_CullModeForward("Float", Float) = 2
            [HideInInspector][Enum(Front, 1, Back, 2)]_TransparentCullMode("Float", Float) = 2
            [HideInInspector][Enum(UnityEditor.Rendering.HighDefinition.OpaqueCullMode)]_OpaqueCullMode("Float", Float) = 2
            [HideInInspector]_ZTestDepthEqualForOpaque("Float", Int) = 4
            [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestTransparent("Float", Float) = 4
            [HideInInspector][ToggleUI]_TransparentBackfaceEnable("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_RequireSplitLighting("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_ReceivesSSR("Boolean", Float) = 1
            [HideInInspector][ToggleUI]_ReceivesSSRTransparent("Boolean", Float) = 0
            [HideInInspector][ToggleUI]_EnableBlendModePreserveSpecularLighting("Boolean", Float) = 1
            [HideInInspector][ToggleUI]_SupportDecals("Boolean", Float) = 1
            [HideInInspector]_StencilRef("Float", Int) = 0
            [HideInInspector]_StencilWriteMask("Float", Int) = 6
            [HideInInspector]_StencilRefDepth("Float", Int) = 8
            [HideInInspector]_StencilWriteMaskDepth("Float", Int) = 8
            [HideInInspector]_StencilRefMV("Float", Int) = 40
            [HideInInspector]_StencilWriteMaskMV("Float", Int) = 40
            [HideInInspector]_StencilRefDistortionVec("Float", Int) = 4
            [HideInInspector]_StencilWriteMaskDistortionVec("Float", Int) = 4
            [HideInInspector]_StencilWriteMaskGBuffer("Float", Int) = 14
            [HideInInspector]_StencilRefGBuffer("Float", Int) = 10
            [HideInInspector]_ZTestGBuffer("Float", Int) = 4
            [HideInInspector][ToggleUI]_RayTracing("Boolean", Float) = 1
            [HideInInspector][Enum(None, 0, Box, 1, Sphere, 2, Thin, 3)]_RefractionModel("Float", Float) = 0
            [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
            [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
            [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        }
        SubShader
        {
            Tags
            {
                "RenderPipeline"="HDRenderPipeline"
                "RenderType"="HDLitShader"
                "Queue"="Geometry+225"
            }
            Pass
            {
                Name "ShadowCaster"
                Tags
                {
                    "LightMode" = "ShadowCaster"
                }
    
                // Render State
                Cull [_CullMode]
                ZWrite On
                ColorMask 0
                ZClip [_ZClip]
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_SHADOWS
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.Alpha = 1;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "META"
                Tags
                {
                    "LightMode" = "META"
                }
    
                // Render State
                Cull Off
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_COLOR
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_LIGHT_TRANSPORT
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassLightTransport.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "ScenePickingPass"
                Tags
                {
                    "LightMode" = "Picking"
                }
    
                // Render State
                Cull [_CullMode]
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma editor_sync_compilation
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_DEPTH_ONLY
                #define SCENEPICKINGPASS
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "SceneSelectionPass"
                Tags
                {
                    "LightMode" = "SceneSelectionPass"
                }
    
                // Render State
                Cull Off
                ColorMask 0
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma editor_sync_compilation
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_DEPTH_ONLY
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
                #define SCENESELECTIONPASS
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "MotionVectors"
                Tags
                {
                    "LightMode" = "MotionVectors"
                }
    
                // Render State
                Cull [_CullMode]
                ZWrite On
                Stencil
                    {
                        WriteMask [_StencilWriteMaskMV]
                        Ref [_StencilRefMV]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma multi_compile _ WRITE_MSAA_DEPTH
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ WRITE_NORMAL_BUFFER
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ WRITE_DECAL_BUFFER
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_MOTION_VECTORS
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    float4 uv3 : TEXCOORD3;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 positionRWS;
                    float3 normalWS;
                    float4 tangentWS;
                    float4 texCoord1;
                    float4 texCoord2;
                    float4 texCoord3;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 interp0 : TEXCOORD0;
                    float3 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    float4 interp3 : TEXCOORD3;
                    float4 interp4 : TEXCOORD4;
                    float4 interp5 : TEXCOORD5;
                    float4 interp6 : TEXCOORD6;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyz =  input.positionRWS;
                    output.interp1.xyz =  input.normalWS;
                    output.interp2.xyzw =  input.tangentWS;
                    output.interp3.xyzw =  input.texCoord1;
                    output.interp4.xyzw =  input.texCoord2;
                    output.interp5.xyzw =  input.texCoord3;
                    output.interp6.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.positionRWS = input.interp0.xyz;
                    output.normalWS = input.interp1.xyz;
                    output.tangentWS = input.interp2.xyzw;
                    output.texCoord1 = input.interp3.xyzw;
                    output.texCoord2 = input.interp4.xyzw;
                    output.texCoord3 = input.interp5.xyzw;
                    output.color = input.interp6.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
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
                    output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                    output.texCoord1 = input.texCoord1;
                    output.texCoord2 = input.texCoord2;
                    output.texCoord3 = input.texCoord3;
                    output.color = input.color;
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassMotionVectors.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "TransparentDepthPrepass"
                Tags
                {
                    "LightMode" = "TransparentDepthPrepass"
                }
    
                // Render State
                Cull [_CullMode]
                Blend One Zero
                ZWrite On
                Stencil
                    {
                        WriteMask [_StencilWriteMaskDepth]
                        Ref [_StencilRefDepth]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    float4 uv3 : TEXCOORD3;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 positionRWS;
                    float3 normalWS;
                    float4 tangentWS;
                    float4 texCoord1;
                    float4 texCoord2;
                    float4 texCoord3;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 interp0 : TEXCOORD0;
                    float3 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    float4 interp3 : TEXCOORD3;
                    float4 interp4 : TEXCOORD4;
                    float4 interp5 : TEXCOORD5;
                    float4 interp6 : TEXCOORD6;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyz =  input.positionRWS;
                    output.interp1.xyz =  input.normalWS;
                    output.interp2.xyzw =  input.tangentWS;
                    output.interp3.xyzw =  input.texCoord1;
                    output.interp4.xyzw =  input.texCoord2;
                    output.interp5.xyzw =  input.texCoord3;
                    output.interp6.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.positionRWS = input.interp0.xyz;
                    output.normalWS = input.interp1.xyz;
                    output.tangentWS = input.interp2.xyzw;
                    output.texCoord1 = input.interp3.xyzw;
                    output.texCoord2 = input.interp4.xyzw;
                    output.texCoord3 = input.interp5.xyzw;
                    output.color = input.interp6.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                    float3 NormalTS;
                    float Smoothness;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.Alpha = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
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
                    output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                    output.texCoord1 = input.texCoord1;
                    output.texCoord2 = input.texCoord2;
                    output.texCoord3 = input.texCoord3;
                    output.color = input.color;
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "FullScreenDebug"
                Tags
                {
                    "LightMode" = "FullScreenDebug"
                }
    
                // Render State
                Cull [_CullMode]
                ZTest LEqual
                ZWrite Off
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_FULL_SCREEN_DEBUG
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassFullScreenDebug.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "DepthOnly"
                Tags
                {
                    "LightMode" = "DepthOnly"
                }
    
                // Render State
                Cull [_CullMode]
                ZWrite On
                Stencil
                    {
                        WriteMask [_StencilWriteMaskDepth]
                        Ref [_StencilRefDepth]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma multi_compile _ WRITE_NORMAL_BUFFER
                #pragma multi_compile _ WRITE_MSAA_DEPTH
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ WRITE_DECAL_BUFFER
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define ATTRIBUTES_NEED_TEXCOORD3
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
                #define VARYINGS_NEED_TEXCOORD3
                #define VARYINGS_NEED_COLOR
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_DEPTH_ONLY
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    float4 uv3 : TEXCOORD3;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 positionRWS;
                    float3 normalWS;
                    float4 tangentWS;
                    float4 texCoord1;
                    float4 texCoord2;
                    float4 texCoord3;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 interp0 : TEXCOORD0;
                    float3 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    float4 interp3 : TEXCOORD3;
                    float4 interp4 : TEXCOORD4;
                    float4 interp5 : TEXCOORD5;
                    float4 interp6 : TEXCOORD6;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyz =  input.positionRWS;
                    output.interp1.xyz =  input.normalWS;
                    output.interp2.xyzw =  input.tangentWS;
                    output.interp3.xyzw =  input.texCoord1;
                    output.interp4.xyzw =  input.texCoord2;
                    output.interp5.xyzw =  input.texCoord3;
                    output.interp6.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.positionRWS = input.interp0.xyz;
                    output.normalWS = input.interp1.xyz;
                    output.tangentWS = input.interp2.xyzw;
                    output.texCoord1 = input.interp3.xyzw;
                    output.texCoord2 = input.interp4.xyzw;
                    output.texCoord3 = input.interp5.xyzw;
                    output.color = input.interp6.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
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
                    output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                    output.texCoord1 = input.texCoord1;
                    output.texCoord2 = input.texCoord2;
                    output.texCoord3 = input.texCoord3;
                    output.color = input.color;
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "GBuffer"
                Tags
                {
                    "LightMode" = "GBuffer"
                }
    
                // Render State
                Cull [_CullMode]
                ZTest [_ZTestGBuffer]
                Stencil
                    {
                        WriteMask [_StencilWriteMaskGBuffer]
                        Ref [_StencilRefGBuffer]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma multi_compile _ LIGHT_LAYERS
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_GBUFFER
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 positionRWS;
                    float3 normalWS;
                    float4 tangentWS;
                    float4 texCoord1;
                    float4 texCoord2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 interp0 : TEXCOORD0;
                    float3 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    float4 interp3 : TEXCOORD3;
                    float4 interp4 : TEXCOORD4;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyz =  input.positionRWS;
                    output.interp1.xyz =  input.normalWS;
                    output.interp2.xyzw =  input.tangentWS;
                    output.interp3.xyzw =  input.texCoord1;
                    output.interp4.xyzw =  input.texCoord2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.positionRWS = input.interp0.xyz;
                    output.normalWS = input.interp1.xyz;
                    output.tangentWS = input.interp2.xyzw;
                    output.texCoord1 = input.interp3.xyzw;
                    output.texCoord2 = input.interp4.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                    float4 VTPackedFeedback;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    {
                        surface.VTPackedFeedback = float4(1.0f,1.0f,1.0f,.0f);
                    }
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
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
                    output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                    output.texCoord1 = input.texCoord1;
                    output.texCoord2 = input.texCoord2;
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassGBuffer.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "Forward"
                Tags
                {
                    "LightMode" = "Forward"
                }
    
                // Render State
                Cull [_CullModeForward]
                Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
                ZTest [_ZTestDepthEqualForOpaque]
                ZWrite [_ZWrite]
                ColorMask [_ColorMaskTransparentVel] 1
                Stencil
                    {
                        WriteMask [_StencilWriteMask]
                        Ref [_StencilRef]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
                #pragma multi_compile_instancing
                #pragma instancing_options renderinglayer
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma multi_compile _ DYNAMICLIGHTMAP_ON
                #pragma multi_compile _ SHADOWS_SHADOWMASK
                #pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
                #pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
                #pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON
                #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_TEXCOORD2
                #define VARYINGS_NEED_POSITION_WS
                #define VARYINGS_NEED_TANGENT_TO_WORLD
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_TEXCOORD2
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_FORWARD
                #define SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
                #define HAS_LIGHTLOOP
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 positionRWS;
                    float3 normalWS;
                    float4 tangentWS;
                    float4 texCoord1;
                    float4 texCoord2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    float3 interp0 : TEXCOORD0;
                    float3 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    float4 interp3 : TEXCOORD3;
                    float4 interp4 : TEXCOORD4;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyz =  input.positionRWS;
                    output.interp1.xyz =  input.normalWS;
                    output.interp2.xyzw =  input.tangentWS;
                    output.interp3.xyzw =  input.texCoord1;
                    output.interp4.xyzw =  input.texCoord2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    output.positionRWS = input.interp0.xyz;
                    output.normalWS = input.interp1.xyz;
                    output.tangentWS = input.interp2.xyzw;
                    output.texCoord1 = input.interp3.xyzw;
                    output.texCoord2 = input.interp4.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                    float4 VTPackedFeedback;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    {
                        surface.VTPackedFeedback = float4(1.0f,1.0f,1.0f,.0f);
                    }
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
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
                    output.tangentToWorld = BuildTangentToWorld(input.tangentWS, input.normalWS);
                    output.texCoord1 = input.texCoord1;
                    output.texCoord2 = input.texCoord2;
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForward.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "RayTracingPrepass"
                Tags
                {
                    "LightMode" = "RayTracingPrepass"
                }
    
                // Render State
                Cull [_CullMode]
                Blend One Zero
                ZWrite On
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag
                #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_CONSTANT
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassConstant.hlsl"
    
                ENDHLSL
            }
        }
        SubShader
        {
            Tags
            {
                "RenderPipeline"="HDRenderPipeline"
                "RenderType"="HDLitShader"
                "Queue"="Geometry+225"
            }
            Pass
            {
                Name "IndirectDXR"
                Tags
                {
                    "LightMode" = "IndirectDXR"
                }
    
                // Render State
                // RenderState: <None>
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 5.0
                #pragma raytracing surface_shader
                #pragma only_renderers d3d11
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_RAYTRACING_INDIRECT
                #define SHADOW_LOW
                #define RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define HAS_LIGHTLOOP
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingMacros.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingIndirect.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "VisibilityDXR"
                Tags
                {
                    "LightMode" = "VisibilityDXR"
                }
    
                // Render State
                // RenderState: <None>
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 5.0
                #pragma raytracing surface_shader
                #pragma only_renderers d3d11
    
                // Keywords
                #pragma multi_compile _ TRANSPARENT_COLOR_SHADOW
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_RAYTRACING_VISIBILITY
                #define RAYTRACING_SHADER_GRAPH_RAYTRACED
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingMacros.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingVisibility.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "ForwardDXR"
                Tags
                {
                    "LightMode" = "ForwardDXR"
                }
    
                // Render State
                // RenderState: <None>
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 5.0
                #pragma raytracing surface_shader
                #pragma only_renderers d3d11
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_RAYTRACING_FORWARD
                #define SHADOW_LOW
                #define RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define HAS_LIGHTLOOP
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingMacros.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassRaytracingForward.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "GBufferDXR"
                Tags
                {
                    "LightMode" = "GBufferDXR"
                }
    
                // Render State
                // RenderState: <None>
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 5.0
                #pragma raytracing surface_shader
                #pragma only_renderers d3d11
    
                // Keywords
                #pragma multi_compile _ MINIMAL_GBUFFER
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma multi_compile _ DEBUG_DISPLAY
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_RAYTRACING_GBUFFER
                #define SHADOW_LOW
                #define RAYTRACING_SHADER_GRAPH_RAYTRACED
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingMacros.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/Deferred/RaytracingIntersectonGBuffer.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/StandardLit/StandardLit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderpassRaytracingGBuffer.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "PathTracingDXR"
                Tags
                {
                    "LightMode" = "PathTracingDXR"
                }
    
                // Render State
                // RenderState: <None>
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 5.0
                #pragma raytracing surface_shader
                #pragma only_renderers d3d11
    
                // Keywords
                #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local _BLENDMODE_OFF _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
                #pragma shader_feature_local _ _DOUBLESIDED_ON
                #pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
                #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC
                #pragma shader_feature_local _ _ENABLE_FOG_ON_TRANSPARENT
                #pragma shader_feature_local _ _DISABLE_DECALS
                #pragma shader_feature_local _ _DISABLE_SSR
                #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
                #pragma shader_feature_local _REFRACTION_OFF _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" // Required to be include before we include properties as it define DECLARE_STACK_CB
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    
                // --------------------------------------------------
                // Defines
    
                // Attribute
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
    
                #define HAVE_MESH_MODIFICATION
    
    
                #define SHADERPASS SHADERPASS_PATH_TRACING
                #define SHADOW_LOW
                #define RAYTRACING_SHADER_GRAPH_DEFAULT
                #define HAS_LIGHTLOOP
    
                // Following two define are a workaround introduce in 10.1.x for RaytracingQualityNode
                // The ShaderGraph don't support correctly migration of this node as it serialize all the node data
                // in the json file making it impossible to uprgrade. Until we get a fix, we do a workaround here
                // to still allow us to rename the field and keyword of this node without breaking existing code.
                #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT 
                #define RAYTRACING_SHADER_GRAPH_HIGH
                #endif
    
                #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
                #endif
                // end
    
                #ifndef SHADER_UNLIT
                // We need isFrontFace when using double sided - it is not required for unlit as in case of unlit double sided only drive the cullmode
                // VARYINGS_NEED_CULLFACE can be define by VaryingsMeshToPS.FaceSign input if a IsFrontFace Node is included in the shader graph.
                #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                    #define VARYINGS_NEED_CULLFACE
                #endif
                #endif
    
                // Specific Material Define
            #define _ENERGY_CONSERVING_SPECULAR 1
                
                // If we use subsurface scattering, enable output split lighting (for forward pass)
                #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
                    #define OUTPUT_SPLIT_LIGHTING
                #endif
                
                // This shader support recursive rendering for raytracing
                #define HAVE_RECURSIVE_RENDERING
                
                // Caution: we can use the define SHADER_UNLIT onlit after the above Material include as it is the Unlit template who define it
    
                // To handle SSR on transparent correctly with a possibility to enable/disable it per framesettings
                // we should have a code like this:
                // if !defined(_DISABLE_SSR_TRANSPARENT)
                // pragma multi_compile _ WRITE_NORMAL_BUFFER
                // endif
                // i.e we enable the multicompile only if we can receive SSR or not, and then C# code drive
                // it based on if SSR transparent in frame settings and not (and stripper can strip it).
                // this is currently not possible with our current preprocessor as _DISABLE_SSR_TRANSPARENT is a keyword not a define
                // so instead we used this and chose to pay the extra cost of normal write even if SSR transaprent is disabled.
                // Ideally the shader graph generator should handle it but condition below can't be handle correctly for now.
                #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                    #define WRITE_NORMAL_BUFFER
                #endif
                #endif
    
                #ifndef DEBUG_DISPLAY
                    // In case of opaque we don't want to perform the alpha test, it is done in depth prepass and we use depth equal for ztest (setup from UI)
                    // Don't do it with debug display mode as it is possible there is no depth prepass in this case
                    #if !defined(_SURFACE_TYPE_TRANSPARENT) && defined(_ALPHATEST)
                        #if SHADERPASS == SHADERPASS_FORWARD
                        #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                        #elif SHADERPASS == SHADERPASS_GBUFFER
                        #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                        #endif
                    #endif
                #endif
    
                // Translate transparent motion vector define
                #if defined(_TRANSPARENT_WRITES_MOTION_VEC) && defined(_SURFACE_TYPE_TRANSPARENT)
                    #define _WRITE_TRANSPARENT_MOTION_VECTOR
                #endif
    
                // Dots Instancing
                // DotsInstancingOptions: <None>
    
                // Various properties
    
                // HybridV1InjectedBuiltinProperties: <None>
    
                // -- Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _EmissionColor;
                float _UseShadowThreshold;
                float4 _DoubleSidedConstants;
                float _BlendMode;
                float _EnableBlendModePreserveSpecularLighting;
                float _RayTracing;
                float _RefractionModel;
                CBUFFER_END
                
                // Object and Global properties
    
                // -- Property used by ScenePickingPass
                #ifdef SCENEPICKINGPASS
                float4 _SelectionID;
                #endif
    
                // -- Properties used by SceneSelectionPass
                #ifdef SCENESELECTIONPASS
                int _ObjectId;
                int _PassValue;
                #endif
    
                // Includes
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingMacros.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/ShaderVariablesRaytracingLightLoop.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingIntersection.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitPathTracing.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/Raytracing/Shaders/RaytracingCommon.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct VaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float3 TangentSpaceNormal;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryingsMeshToPS
                {
                    float4 positionCS : SV_POSITION;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                };
    
                PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
                {
                    PackedVaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
                VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
                {
                    VaryingsMeshToPS output;
                    output.positionCS = input.positionCS;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
    
                // Graph Functions
                // GraphFunctions: <None>
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float3 Emission;
                    float Alpha;
                    float3 BentNormal;
                    float Smoothness;
                    float Occlusion;
                    float3 NormalTS;
                    float Metallic;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    surface.BaseColor = IsGammaSpace() ? float3(0.8867924, 0.8867924, 0.8867924) : SRGBToLinear(float3(0.8867924, 0.8867924, 0.8867924));
                    surface.Emission = float3(0, 0, 0);
                    surface.Alpha = 1;
                    surface.BentNormal = IN.TangentSpaceNormal;
                    surface.Smoothness = 0.5;
                    surface.Occlusion = 1;
                    surface.NormalTS = IN.TangentSpaceNormal;
                    surface.Metallic = 0;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                
                VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS.xyz;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
                {
                    // build graph inputs
                    VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);
                    // Override time paramters with used one (This is required to correctly handle motion vector for vertex animation based on time)
                
                    // evaluate vertex graph
                    VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);
                
                    // copy graph output to the results
                    input.positionOS = vertexDescription.Position;
                    input.normalOS = vertexDescription.Normal;
                    input.tangentOS.xyz = vertexDescription.Tangent;
                
                    return input;
                }
                
                FragInputs BuildFragInputs(VaryingsMeshToPS input)
                {
                    FragInputs output;
                    ZERO_INITIALIZE(FragInputs, output);
                
                    // Init to some default value to make the computer quiet (else it output 'divide by zero' warning even if value is not used).
                    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
                    // to compute normals which are then passed on elsewhere to compute other values...
                    output.tangentToWorld = k_identity3x3;
                    output.positionSS = input.positionCS;       // input.positionCS is SV_Position
                
                
                    return output;
                }
                
                SurfaceDescriptionInputs FragInputsToSurfaceDescriptionInputs(FragInputs input, float3 viewWS)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                    #if defined(SHADER_STAGE_RAY_TRACING)
                    #else
                    #endif
                    output.TangentSpaceNormal =          float3(0.0f, 0.0f, 1.0f);
                
                    return output;
                }
                
                // existing HDRP code uses the combined function to go directly from packed to frag inputs
                FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    VaryingsMeshToPS unpacked= UnpackVaryingsMeshToPS(input);
                    return BuildFragInputs(unpacked);
                }
                
    
                // --------------------------------------------------
                // Build Surface Data (Specific Material)
    
            void BuildSurfaceData(FragInputs fragInputs, inout SurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
                {
                    ZERO_INITIALIZE(SurfaceData, surfaceData);
                
                    // specularOcclusion need to be init ahead of decal to quiet the compiler that modify the SurfaceData struct
                    // however specularOcclusion can come from the graph, so need to be init here so it can be override.
                    surfaceData.specularOcclusion = 1.0;
                
                    surfaceData.baseColor =                 surfaceDescription.BaseColor;
                    surfaceData.perceptualSmoothness =      surfaceDescription.Smoothness;
                    surfaceData.ambientOcclusion =          surfaceDescription.Occlusion;
                    surfaceData.metallic =                  surfaceDescription.Metallic;
                
                    #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                        if (_EnableSSRefraction)
                        {
                
                            surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                            surfaceDescription.Alpha = 1.0;
                        }
                        else
                        {
                            surfaceData.ior = 1.0;
                            surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                            surfaceData.atDistance = 1.0;
                            surfaceData.transmittanceMask = 0.0;
                            surfaceDescription.Alpha = 1.0;
                        }
                    #else
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                    #endif
                
                    // These static material feature allow compile time optimization
                    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;
                    #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_TRANSMISSION
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_ANISOTROPY
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                    #endif
                
                    #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                        surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                    #endif
                
                    #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                        // Require to have setup baseColor
                        // Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
                        surfaceData.baseColor *= (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b));
                    #endif
                
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
                
                    // normal delivered to master node
                    GetNormalWS(fragInputs, surfaceDescription.NormalTS, surfaceData.normalWS, doubleSidedConstants);
                
                    surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                
                    surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz);    // The tangent is not normalize in tangentToWorld for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
                
                
                    #if HAVE_DECALS
                        if (_EnableDecals)
                        {
                            float alpha = 1.0;
                            alpha = surfaceDescription.Alpha;
                
                            // Both uses and modifies 'surfaceData.normalWS'.
                            DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs.tangentToWorld[2], alpha);
                            ApplyDecalToSurfaceData(decalSurfaceData, fragInputs.tangentToWorld[2], surfaceData);
                        }
                    #endif
                
                    bentNormalWS = surfaceData.normalWS;
                
                    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);
                
                    #ifdef DEBUG_DISPLAY
                        if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                        {
                            // TODO: need to update mip info
                            surfaceData.metallic = 0;
                        }
                
                        // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
                        // as it can modify attribute use for static lighting
                        ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                    #endif
                
                    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
                    // If user provide bent normal then we process a better term
                    #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                        // Just use the value passed through via the slot (not active otherwise)
                    #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                        // If we have bent normal and ambient occlusion, process a specular occlusion
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                    #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                    #endif
                
                    #if defined(_ENABLE_GEOMETRIC_SPECULAR_AA) && !defined(SHADER_STAGE_RAY_TRACING)
                        surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                    #endif
                }
                
    
                // --------------------------------------------------
                // Get Surface And BuiltinData
    
                void GetSurfaceAndBuiltinData(FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData RAY_TRACING_OPTIONAL_PARAMETERS)
                {
                    // Don't dither if displaced tessellation (we're fading out the displacement instead to match the next LOD)
                    #if !defined(SHADER_STAGE_RAY_TRACING) && !defined(_TESSELLATION_DISPLACEMENT)
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
                    #endif
                    #endif
    
                    #ifndef SHADER_UNLIT
                    #ifdef _DOUBLESIDED_ON
                        float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                    #else
                        float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                    #endif
    
                    ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants); // Apply double sided flip on the vertex normal
                    #endif // SHADER_UNLIT
    
                    SurfaceDescriptionInputs surfaceDescriptionInputs = FragInputsToSurfaceDescriptionInputs(fragInputs, V);
                    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
    
                    // Perform alpha test very early to save performance (a killed pixel will not sample textures)
                    // TODO: split graph evaluation to grab just alpha dependencies first? tricky..
                    #ifdef _ALPHATEST_ON
                        float alphaCutoff = surfaceDescription.AlphaClipThreshold;
                        #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
                        // The TransparentDepthPrepass is also used with SSR transparent.
                        // If an artists enable transaprent SSR but not the TransparentDepthPrepass itself, then we use AlphaClipThreshold
                        // otherwise if TransparentDepthPrepass is enabled we use AlphaClipThresholdDepthPrepass
                        #elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
                        // DepthPostpass always use its own alpha threshold
                        alphaCutoff = surfaceDescription.AlphaClipThresholdDepthPostpass;
                        #elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
                        // If use shadow threshold isn't enable we don't allow any test
                        #endif
    
                        GENERIC_ALPHA_TEST(surfaceDescription.Alpha, alphaCutoff);
                    #endif
    
                    #if !defined(SHADER_STAGE_RAY_TRACING) && _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
                    #endif
    
                    #ifndef SHADER_UNLIT
                    float3 bentNormalWS;
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
    
                    // Builtin Data
                    // For back lighting we use the oposite vertex normal
                    InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);
    
                    #else
                    BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData);
    
                    ZERO_INITIALIZE(BuiltinData, builtinData); // No call to InitBuiltinData as we don't have any lighting
                    builtinData.opacity = surfaceDescription.Alpha;
    
                    #if defined(DEBUG_DISPLAY)
                        // Light Layers are currently not used for the Unlit shader (because it is not lit)
                        // But Unlit objects do cast shadows according to their rendering layer mask, which is what we want to
                        // display in the light layers visualization mode, therefore we need the renderingLayers
                        builtinData.renderingLayers = GetMeshRenderingLightLayer();
                    #endif
    
                    #endif // SHADER_UNLIT
    
                    #ifdef _ALPHATEST_ON
                        // Used for sharpening by alpha to mask - Alpha to covertage is only used with depth only and forward pass (no shadow pass, no transparent pass)
                        builtinData.alphaClipTreshold = alphaCutoff;
                    #endif
    
                    // override sampleBakedGI - not used by Unlit
    
                    builtinData.emissiveColor = surfaceDescription.Emission;
    
                    // Note this will not fully work on transparent surfaces (can check with _SURFACE_TYPE_TRANSPARENT define)
                    // We will always overwrite vt feeback with the nearest. So behind transparent surfaces vt will not be resolved
                    // This is a limitation of the current MRT approach.
    
                    #if _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
                    #endif
    
                    // TODO: We should generate distortion / distortionBlur for non distortion pass
                    #if (SHADERPASS == SHADERPASS_DISTORTION)
                    builtinData.distortion = surfaceDescription.Distortion;
                    builtinData.distortionBlur = surfaceDescription.DistortionBlur;
                    #endif
    
                    #ifndef SHADER_UNLIT
                    // PostInitBuiltinData call ApplyDebugToBuiltinData
                    PostInitBuiltinData(V, posInput, surfaceData, builtinData);
                    #else
                    ApplyDebugToBuiltinData(builtinData);
                    #endif
    
                    RAY_TRACING_OPTIONAL_ALPHA_TEST_PASS
                }
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassPathTracing.hlsl"
    
                ENDHLSL
            }
        }
        CustomEditor "Rendering.HighDefinition.LitShaderGraphGUI"
        FallBack "Hidden/Shader Graph/FallbackError"
    }

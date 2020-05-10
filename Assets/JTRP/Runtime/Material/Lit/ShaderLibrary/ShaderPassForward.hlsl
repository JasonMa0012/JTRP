
#define SHADERPASS SHADERPASS_FORWARD
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"

#define HAS_LIGHTLOOP

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"

// #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
#include "../ShaderLibrary/LitSharePass.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
#include "../ShaderLibrary/ForwardFunction.hlsl"

PackedVaryingsType Vert(AttributesMesh input)
{
    VaryingsType varyingsType;//VaryingsToPS
    VaryingsMeshType output = (VaryingsMeshType)0;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
    // This return the camera relative position (if enable)
    float3 positionRWS = TransformObjectToWorld(input.positionOS);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    float4 tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
    
    output.positionRWS = positionRWS;
    output.positionCS = TransformWorldToHClip(positionRWS);
    output.normalWS = normalWS;
    output.tangentWS = tangentWS;
    
    output.texCoord0 = input.uv0;
    #ifdef VARYINGS_NEED_TEXCOORD1
        output.texCoord1 = input.uv1;
    #endif
    #ifdef VARYINGS_NEED_TEXCOORD2
        output.texCoord2 = input.uv2;
    #endif
    #ifdef VARYINGS_NEED_TEXCOORD3
        output.texCoord3 = input.uv3;
    #endif
    #ifdef VARYINGS_NEED_TEXCOORD7
        output.texCoord7 = input.uv7;
    #endif
    output.color = input.color;
    
    varyingsType.vmesh = output;
    return PackVaryingsToPS(varyingsType);
}

void Frag(PackedVaryingsToPS packedInput, out float4 outColor: SV_Target0)
{
    FragInputs input;
    PositionInputs posInput;
    BuiltinData builtinData;
    SurfaceData surfaceData;
    LitToonContext context = (LitToonContext)0;
    
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
    input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);
    
    GetUVs(context, input);
    
    float4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, context.uv0);
    float4 _ShadowMap_var = SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, context.uv0);
    float4 _ShadowColorMap_var = SAMPLE_TEXTURE2D(_ShadowColorMap, sampler_ShadowMap, context.uv0);
    float4 _LightMap_var = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, context.uv0);
    float3 normalMap = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D_LOD(_NormalMap, sampler_NormalMap, context.uv0, 0), _NormalScale);
    
    AlphaGammaCorrection(_MainTex_var.a, _ShadowMap_var.a, _LightMap_var.a);
    
    context.roughness = ComputeRoughness((1 - _ShadowMap_var.b) * _roughness);
    PreData(normalMap, _LightColorIntensity, packedInput, input, posInput, builtinData, surfaceData, context);
    
    context.shadowStep = GetShadowStep(context.halfLambert, _Shadow_Step, _Shadow_Feather, _ShadowMap_var.a,
    GetSelfShadow(context, posInput));
    float step2 = GetShadowStep(context.halfLambert, _Shadow_Step2, _Shadow_Feather2);
    
    GetBaseColor(context, _MainTex_var.rgb * _Color.rgb, _SkyColorIntensity, _Shadow_Power * 5 + 1,
    step2 * _ShadowColor2.rgb * _ShadowIntensity2, _ShadowColorBlend2);
    
    PointLightLoop(context, posInput, builtinData, _PointLightColorIntensity * _LightMap_var.a, _LightMap_var.b + _HighColorLevel);
    
    context.diffuse = StdToonDiffuseLightingModel(context, _ShadowIntensity * (1 - _ShadowMap_var.r),
    _ShadowColorMap_var.rgb * _ShadowMapColor.rgb, _ShadowFixedColor, _ShadowMap_var.a);
    
    context.specular = max(context.highLightColor,
    GetHighLight(context.N, context.V, context.L, context.dirLightColor, context.shadowStep, context.roughness, _HighColorInt1, _HighColorInt2)
    * saturate(_LightMap_var.b + _HighColorLevel));
    
    float3 matCapColor = GetMatCap(context.V, context.dirLightColor, context.uv0, context.shadowStep, context.N, _LightMap_var.r);
    context.specular = max(context.specular, matCapColor);
    
    context.emissive += GetEmissive(context.uv0,
    smoothstep(0.55, 1, _ShadowMap_var.g),
    smoothstep(0.45, 0, _ShadowMap_var.g));
    
    float3 rimColor = GetRimLight(context, posInput, input, _LightMap_var.g);
    context.emissive = max(context.emissive, rimColor);
    
    
    float3 finalCol = 0;
    finalCol = context.diffuse + ToonLightColorAddMode(context.brightBaseColor, context.specular)
    + context.emissive + _AddColor.rgb * _AddColorIntensity;
    
    
    outColor = float4(finalCol, 1);
    

    
    
    //#region Descrption
    
    /*struct AttributesMesh
    {
        float3 positionOS   : POSITION;
        #ifdef ATTRIBUTES_NEED_NORMAL
            float3 normalOS     : NORMAL;
        #endif
        #ifdef ATTRIBUTES_NEED_TANGENT
            float4 tangentOS    : TANGENT; // Store sign in w
        #endif
        #ifdef ATTRIBUTES_NEED_TEXCOORD0
            float2 uv0          : TEXCOORD0;
        #endif
        #ifdef ATTRIBUTES_NEED_TEXCOORD1
            float2 uv1          : TEXCOORD1;
        #endif
        #ifdef ATTRIBUTES_NEED_TEXCOORD2
            float2 uv2          : TEXCOORD2;
        #endif
        #ifdef ATTRIBUTES_NEED_TEXCOORD3
            float2 uv3          : TEXCOORD3;
        #endif
        #ifdef ATTRIBUTES_NEED_COLOR
            float4 color        : COLOR;
        #endif
        
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct VaryingsMeshToPS
    {
        float4 positionCS;
        #ifdef VARYINGS_NEED_POSITION_WS
            float3 positionRWS;
        #endif
        #ifdef VARYINGS_NEED_TANGENT_TO_WORLD
            float3 normalWS;
            float4 tangentWS;  // w contain mirror sign
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD0
            float2 texCoord0;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD1
            float2 texCoord1;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD2
            float2 texCoord2;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD3
            float2 texCoord3;
        #endif
        #ifdef VARYINGS_NEED_COLOR
            float4 color;
        #endif
        
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };*/
    //#endregion
    
    
    //#region FragInputs
    /*float4 positionSS;
    float3 positionRWS;       相对摄像机空间位置，w为depth Offset
    float4 texCoord0;
    float4 texCoord1;
    float4 color;
    float3x3 tangentToWorld;  //(TBN)
    bool isFrontFace;*/
    //#endregion
    
    //#region PositionInputs
    /*float3 positionWS;  // World space position (could be camera-relative)
    float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    float  linearDepth; // View space Z coordinate                              : [Near, Far]*/
    //#endregion
    //#region Descrption
    
    // SurfaceData surfaceData;
    /*uint materialFeatures;
    real3 baseColor;
    real specularOcclusion;
    float3 normalWS;
    real perceptualSmoothness;
    real ambientOcclusion;
    real metallic;
    real coatMask;
    real3 specularColor;
    uint diffusionProfileHash;
    real subsurfaceMask;
    real thickness;
    float3 tangentWS;
    real anisotropy;
    real iridescenceThickness;
    real iridescenceMask;
    float3 geomNormalWS;
    real ior;
    real3 transmittanceColor;
    real atDistance;
    real transmittanceMask;*/
    //#endregion
    
    /*struct BuiltinData
    real opacity;
    real3 bakeDiffuseLighting;
    real3 backBakeDiffuseLighting;
    real shadowMask0;
    real shadowMask1;
    real shadowMask2;
    real shadowMask3;
    real3 emissiveColor;
    real2 motionVector;
    real2 distortion;
    real distortionBlur;
    uint renderingLayers;
    float depthOffset;*/
    
    
    /*    内置光照变量？
    float4 _ShadowAtlasSize;
    float4 _CascadeShadowAtlasSize;
    float4 _AreaShadowAtlasSize;
    float4x4 _Env2DCaptureVP[32];
    float _Env2DCaptureForward[96];
    uint _DirectionalLightCount;// 平行光数量
    uint _PunctualLightCount;// 点光源数量
    uint _AreaLightCount;// 区域光数量
    uint _EnvLightCount;
    uint _EnvProxyCount;
    int _EnvLightSkyEnabled;
    int _DirectionalShadowIndex;
    float _MicroShadowOpacity;
    uint _NumTileFtplX;
    uint _NumTileFtplY;
    float g_fClustScale;
    float g_fClustBase;
    float g_fNearPlane;
    float g_fFarPlane;
    int g_iLog2NumClusters;
    uint g_isLogBaseBufferEnabled;
    uint _NumTileClusteredX;
    uint _NumTileClusteredY;
    uint _CascadeShadowCount;
    int _DebugSingleShadowIndex;
    int _EnvSliceSize;
    uint _CookieSizePOT;
    int _RaytracedIndirectDiffuse;
    
    struct DirectionalLightData
    float3 positionRWS;
    uint lightLayers;
    float lightDimmer;
    float volumetricLightDimmer;
    float3 forward;
    int cookieIndex;
    float3 right;
    int tileCookie;
    float3 up;
    int shadowIndex;
    float3 color;
    int contactShadowMask;
    float3 shadowTint;
    float shadowDimmer;
    float volumetricShadowDimmer;
    int nonLightMappedOnly;
    real minRoughness;
    int screenSpaceShadowIndex;
    real4 shadowMaskSelector;
    float diffuseDimmer;
    float specularDimmer;
    float angularDiameter;
    float distanceFromCamera;
    float isRayTracedContactShadow;
    float penumbraTint;*/
    //#endregion
}

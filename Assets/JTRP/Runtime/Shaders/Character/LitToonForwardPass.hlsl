
#define SHADERPASS SHADERPASS_FORWARD
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"

#define HAS_LIGHTLOOP

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
#include "ForwardFunction.hlsl"

PackedVaryingsType Vert(AttributesMesh inputMesh)
{
    VaryingsType varyingsType;
    varyingsType.vmesh = VertMesh(inputMesh);

    return PackVaryingsType(varyingsType);
}

PackedVaryingsToPS VertTesselation(VaryingsToDS input)
{
    VaryingsToPS output;
    output.vmesh = VertMeshTesselation(input.vmesh);

    AntiPerspective(output.vmesh.positionCS);

    return PackVaryingsToPS(output);
}

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/TessellationShare.hlsl"

void Frag(PackedVaryingsToPS packedInput, 
out float4 outColor: SV_Target0,
out float4 customBuffer: SV_TARGET1)
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
    float4 _ShadowMap_var = SAMPLE_TEXTURE2D(_ShadowMap, sampler_MainTex, context.uv0);
    float4 _ShadowColorMap_var = SAMPLE_TEXTURE2D(_ShadowColorMap, sampler_MainTex, context.uv0);
    float4 _LightMap_var = SAMPLE_TEXTURE2D(_LightMap, sampler_MainTex, context.uv0);
    float3 normalMap = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D_LOD(_NormalMap, sampler_MainTex, context.uv0, 0), _NormalScale);
    
    AlphaGammaCorrection(_ShadowMap_var.a, _LightMap_var.a);
    _MainTex_var.rgb = ShiftColorPurity(_MainTex_var.rgb, _Purity);
    
    context.roughness = SetRoughness((1 - 0) * _roughness);
    PreData(normalMap, _LightColorIntensity, packedInput, input, posInput, builtinData, surfaceData, context);

	context.shadowStep = GetShadowStep(context.ONHalfLambert, _Shadow_Step, _Shadow_Feather, GetSelfShadow(context, posInput));
    
    PointLightLoop(context, posInput, builtinData, _PointLightColorIntensity * 1, 1);
    
	float3 _ShadowRamp_Var = SAMPLE_TEXTURE2D(_ShadowColorRamp, s_point_clamp_sampler, float2(1 - context.shadowStep, 0.5)).rgb;
	float3 shadowColor = _ShadowColorMap_var.rgb * _ShadowMapColor.rgb * lerp(1, _ShadowRamp_Var, _EnableShadowColorRamp);
    ToonDiffuseLighting(context, _MainTex_var.rgb * _Color.rgb, _ShadowIntensity * (1 - 0),
    shadowColor, _ShadowFixedColor, _ShadowMap_var.a);
    
    context.specular = max(context.highLightColor,
    GetHighLight(context.N, context.V, context.L, context.dirLightColor, context.shadowStep, context.roughness, _HighColorInt1, _HighColorInt2) * 1);
    
    float3 matCapColor = GetMatCap(context.V, context.dirLightColor, context.uv0, context.shadowStep, context.N, 1);
    context.specular = max(context.specular, matCapColor);
    
    float3 rimColor = GetRimLight(context, posInput, input, 1);
    context.emissive = max(context.emissive, rimColor);
    
    
    float3 finalCol = 0;
    finalCol = context.diffuse + ToonLightColorAddMode(context.baseColor, context.specular)
    + context.emissive;
    
    // finalCol = context.diffuse;
    
    outColor = float4(finalCol, lerp(_BloomFactor, _MainTex_var.a * _Color.a, _AlphaIsTransparent));
	customBuffer = input.color;
}



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


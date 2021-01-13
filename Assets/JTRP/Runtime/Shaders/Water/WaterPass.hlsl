struct VertexInput
{
    float4 vertex: POSITION;
    float4 uv0: TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 clipPos: SV_POSITION;
    float3 viewPos: TEXCOORD0;
    float3 worldPos: TEXCOORD1;
    float4 screenPos: TEXCOORD2;
    float2 uv0: TEXCOORD3;
    float3 viewDir: TEXCOORD4;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "WaterFunc.hlsl"


VertexOutput vert(VertexInput i)
{
    VertexOutput o = (VertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    float3 positionWS = TransformObjectToWorld(i.vertex.xyz);
    o.worldPos = positionWS;
    o.viewPos = mul(UNITY_MATRIX_V, float4(positionWS, 1)).xyz;
    o.clipPos = mul(UNITY_MATRIX_P, float4(o.viewPos, 1));
    o.screenPos = ComputeScreenPos(o.clipPos, _ProjectionParams.x);
    o.uv0 = i.uv0.xy;
    
    o.viewDir = GetWorldSpaceNormalizeViewDir(positionWS);
    
    return o;
}

float4 frag(VertexOutput i): SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float depthFactor = 1 / _depthFactor;
    float2 t = _Time.x * _Speed * _Dir;
    
    // Screen Space
    float2 screenPos = i.screenPos.xy / i.screenPos.w;//0-1
    float depthTex, depthScene, depthWater, depthWaterWorld;
    float3 sceneWorldPos, absolutePositionWS = GetAbsolutePositionWS(i.worldPos);
    
    GetScreenSpaceData(screenPos, i.viewPos, absolutePositionWS, depthTex, depthScene, depthWater, sceneWorldPos, depthWaterWorld);
    
    float originDepthWaterWorld = depthWaterWorld;
    float3 originSceneWorldPos = sceneWorldPos;
    float depthBlend = DepthBlend(depthWaterWorld);
    float3 normal = GetNormal(absolutePositionWS.xz, t);
    float3 sceneNormal = normalize(cross(ddy(originSceneWorldPos), ddx(originSceneWorldPos)));
    float3 screenColor;
    
    // Distortion
    GetDistortionSSData(screenPos, normal.xz * _DistortionScale, i.viewPos,
    absolutePositionWS, depthTex, depthScene, depthWater, sceneWorldPos, depthWaterWorld, screenColor);
    
    
    // Shadow
    DirectionalLightData mainLight = _DirectionalLightDatas[0];
    HDShadowContext sc = InitShadowContext();
    float waterTopShadow = GetShadow(mainLight, sc, screenPos, i.worldPos, normal);
    float waterBottomShadow = GetShadow(mainLight, sc, screenPos, GetCameraRelativePositionWS(sceneWorldPos), sceneNormal);
    
    // caustics
    float3 caustics = GetCaustics(t, sceneWorldPos, sceneNormal);// World space texture sampling
    caustics *= _Color.rgb * (screenColor * 0.9 + 0.1) * saturate(waterBottomShadow + 0.3) * depthBlend;
    
    // Fresnel
    float fresnelTerm = Fresnel(normal, i.viewDir.xyz);
    
    // Highlights
    float3 spec = Highlights(_Roughness, normal, i.viewDir) * waterTopShadow * _SpecularColor.rgb * _SpecularColor.a;
    
    // Reflections
    float3 reflection = SampleReflections(lerp(float3(0, 1, 0), normal, _ReflectionNormalBlend), i.viewDir.xyz, fresnelTerm);
    reflection = reflection * GetCurrentExposureMultiplier() * _ReflectionColor.rgb + spec;
    
    // Foam
    float3 foam = Foam(originDepthWaterWorld, originSceneWorldPos, t);
    
    reflection *= (1 - saturate(foam * 2.2)) * depthBlend;
    
    float3 lighting = lerp(1, mainLight.color * GetCurrentExposureMultiplier() * 0.2, _MainLightBlend);
    
    // combine
    float4 finalColor = float4(0, 0, 0, 1);
    
    float absorptionDepth = depthWaterWorld * lerp(depthWater, 1, _ViewAbsorption) * depthFactor;
    finalColor.rgb += Scattering(absorptionDepth) * lighting * _waterColor;
    finalColor.rgb += reflection;
    finalColor.rgb += foam;
    finalColor.rgb += (screenColor + caustics * lighting) * Absorption(absorptionDepth);
    
    
    return finalColor;
}

///////////////////////////////////////////////////////////////////////////////
//                            Tessellation                                   //
///////////////////////////////////////////////////////////////////////////////

struct TessellationControlPoint
{
    float4 vertex: INTERNALTESSPOS;
    float4 texcoord: TEXCOORD0;	// Geometric UVs stored in xy, and world(pre-waves) in zw
    float3 posWS: TEXCOORD1;	// world position of the vertices
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct HS_ConstantOutput
{
    float TessFactor[3]: SV_TessFactor;
    float InsideTessFactor: SV_InsideTessFactor;
};


float TessellationEdgeFactor(float3 p0, float3 p1)
{
    float edgeLength = distance(p0, p1);
    
    float3 edgeCenter = (p0 + p1) * 0.5;
    float viewDistance = smoothstep(_TessellationStart, _TessellationEnd, length(edgeCenter)) * 100 + 1;
    
    return edgeLength * _ScreenParams.y / (_TessellationEdgeLength * viewDistance);
}

TessellationControlPoint TessellationVertex(VertexInput v)
{
    TessellationControlPoint o;
    o.vertex = v.vertex;
    o.posWS = TransformObjectToWorld(v.vertex.xyz);
    o.texcoord.xy = v.uv0.xy;
    o.texcoord.zw = o.posWS.xz;
    //o.color = v.color;
    return o;
}

HS_ConstantOutput HSConstant(InputPatch < TessellationControlPoint, 3 > Input)
{
    float3 p0 = TransformObjectToWorld(Input[0].vertex.xyz);
    float3 p1 = TransformObjectToWorld(Input[1].vertex.xyz);
    float3 p2 = TransformObjectToWorld(Input[2].vertex.xyz);
    HS_ConstantOutput o = (HS_ConstantOutput)0;
    o.TessFactor[0] = TessellationEdgeFactor(p1, p2);
    o.TessFactor[1] = TessellationEdgeFactor(p2, p0);
    o.TessFactor[2] = TessellationEdgeFactor(p0, p1);
    o.InsideTessFactor = (TessellationEdgeFactor(p1, p2) +
    TessellationEdgeFactor(p2, p0) +
    TessellationEdgeFactor(p0, p1)) * (1 / 3.0);
    return o;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[patchconstantfunc("HSConstant")]
[outputcontrolpoints(3)]
TessellationControlPoint Hull(InputPatch < TessellationControlPoint, 3 > Input, uint uCPID: SV_OutputControlPointID)
{
    return Input[uCPID];
}

// Domain: replaces vert for tessellation version
[domain("tri")]
VertexOutput Domain(HS_ConstantOutput HSConstantData, const OutputPatch < TessellationControlPoint, 3 > Input, float3 BarycentricCoords: SV_DomainLocation)
{
    VertexOutput o = (VertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    /////////////////////Tessellation////////////////////////
    float fU = BarycentricCoords.x;
    float fV = BarycentricCoords.y;
    float fW = BarycentricCoords.z;
    
    // float4 vertex = Input[0].vertex * fU + Input[1].vertex * fV + Input[2].vertex * fW;
    float4 coords = Input[0].texcoord * fU + Input[1].texcoord * fV + Input[2].texcoord * fW;
    o.uv0 = coords.xy;
    // o.screenPos = float4(coords.zw, 0, 1);
    o.worldPos = Input[0].posWS * fU + Input[1].posWS * fV + Input[2].posWS * fW;
    
    float2 worldUV = GetAbsolutePositionWS(o.worldPos).xz;
    o.worldPos.y += (SAMPLE_TEXTURE2D_LOD(_NormalMap, s_linear_repeat_sampler,
    worldUV * half2(0.0025, 0.0025) + _Time.x * 0.075, 0).r * 2 - 1) * 0.5;
    // o.worldPos.y += (SAMPLE_TEXTURE2D_LOD(_NormalMap2, s_linear_repeat_sampler,
    // worldUV * half2(0.02, 0.02) + _Time.x * 0.25, 0).r * 2 - 1) * 0.25 ;
    
    
    o.viewPos = mul(UNITY_MATRIX_V, float4(o.worldPos, 1)).xyz;
    o.clipPos = mul(UNITY_MATRIX_P, float4(o.viewPos, 1));
    o.screenPos = ComputeScreenPos(o.clipPos, _ProjectionParams.x);
    o.viewDir = GetWorldSpaceNormalizeViewDir(o.worldPos);
    
    
    
    return o;
}

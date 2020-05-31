struct VertexInput
{
    float4 vertex: POSITION;
    float3 normal: NORMAL;
    float4 tangent: TANGENT;
    float2 texcoord0: TEXCOORD0;
    float2 texcoord1: TEXCOORD1;
    float2 texcoord7: TEXCOORD7;
    float4 color: COLOR0;
};
struct VertexOutput
{
    float4 pos: SV_POSITION;
    float2 uv0: TEXCOORD0;
    float2 uv1: TEXCOORD1;
    // float3 normalDir: COLOR0;
    // float3 tangentDir: COLOR1;
    // float3 bitangentDir: COLOR2;
};

#include "ForwardFunction.hlsl"

VertexOutput vert(VertexInput v)
{
    #ifndef _OUTLINE_ENABLE_ON
        return(VertexOutput)0;
    #endif
    
    VertexOutput o = (VertexOutput)0;
    if (_Outline_Width == 0)
        return o;
    
    o.uv0 = v.texcoord0;
    o.uv1 = v.texcoord1;
    float3 normalDir = TransformObjectToWorldNormal(v.normal);
    float3 tangentDir = normalize(mul(UNITY_MATRIX_M, float4(v.tangent.xyz, 0.0)).xyz);
    float3 bitangentDir = normalize(cross(normalDir, tangentDir) * v.tangent.w);
    float3x3 tangentTransform = float3x3(tangentDir, bitangentDir, normalDir);
    
    #ifdef _ORIGINNORMAL_ON
        float3 _BakedNormalDir = normalDir;
    #else
        float3 _BakedNormalDir = GetSmoothedWorldNormal(v.texcoord7, tangentTransform);
    #endif
    
    #if defined(UNITY_REVERSED_Z)
        //v.2.0.4.2 (DX)
        _Offset_Z = _Offset_Z * - 0.01;
    #else
        //OpenGL
        _Offset_Z = _Offset_Z * 0.01;
    #endif
    
    float3 posWS = TransformObjectToWorld(v.vertex.xyz);
    
    float distance = length(GetWorldSpaceViewDir(posWS));
    float Set_Outline_Width = _Outline_Width * 0.002 * v.color.g * GetOutLineScale(distance);
    
    float2 whRatio = GetWHRatio();
    
    o.pos = TransformWorldToHClip(posWS);
    o.pos.z = o.pos.z + _Offset_Z * distance * v.color.r;
    o.pos.xy += normalize(mul((float3x3)UNITY_MATRIX_VP, _BakedNormalDir).xy) * Set_Outline_Width * whRatio;
    
    return o;
}
float4 frag(VertexOutput i): SV_Target
{
    #ifndef _OUTLINE_ENABLE_ON
        return(float4)0;
    #endif
    
    float3 lightColor = _DirectionalLightDatas[0].color.rgb * GetCurrentExposureMultiplier() * _LightColorIntensity;
    float2 Set_UV0 = i.uv0;
    float2 Set_UV1 = i.uv1;
    float4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, Set_UV0) * _Color;
    
    float3 Set_BaseColor = _Outline_Color.rgb * lightColor * lerp(1, _MainTex_var.rgb, _Outline_Blend);
    
    Set_BaseColor = ShiftColorPurity(Set_BaseColor, _Outline_Purity) * _Outline_Lightness;
    
    return float4(Set_BaseColor + _AddColor.rgb * _AddColorIntensity, 1.0);
}

Shader "JTRP/Face"
{
    Properties
    {
        _MainTex ("Color Map", 2D) = "white" { }
        _LightMap ("Light Map", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.6, 0.6, 0.6, 1)
        [PowerSlider(6)]_ShadowFeather ("Shadow Feather", Range(0.0001, 1)) = 0.01
        [PowerSlider(2)]_Gamma ("Gamma", Range(0.01, 15)) = 1
    }
    
    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
    
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
    
    CBUFFER_START(UnityPerMaterial)
    float4 _Color;
    float4 _ShadowColor;
    float _ShadowFeather;
    float _Gamma;
    
    CBUFFER_END
    
    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardOnly" }
        LOD 100
        
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            struct GraphVertexInput
            {
                float4 vertex: POSITION;
                float4 texcoord0: TEXCOORD0;
                float4 texcoord1: TEXCOORD1;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct GraphVertexOutput
            {
                float4 position: POSITION;
                half4 uv0: TEXCOORD0;
                float3 positionWS: TEXCOORD1;
                float3 normal: TEXCOORD2;
                float3 forwardNormal: TEXCOORD3;
            };
            
            float3 UnpackNormalRG(float2 packedNormal, real scale = 1.0)
            {
                float3 normal;
                normal.xy = packedNormal.rg * 2.0 - 1.0;
                normal.xy *= scale;
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                return normal;
            }
            
            GraphVertexOutput vert(GraphVertexInput v)
            {
                GraphVertexOutput o;
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.position = TransformWorldToHClip(positionWS);
                o.positionWS = positionWS;
                
                o.normal = TransformObjectToWorldNormal(v.normal);
                float3 tangentDir = normalize(mul(UNITY_MATRIX_M, float4(v.tangent.xyz, 0.0)).xyz);
                float3 bitangentDir = normalize(cross(o.normal, tangentDir) * v.tangent.w);
                float3x3 tangentTransform = float3x3(tangentDir, bitangentDir, o.normal);
                
                float3 forwardNormal = normalize(UnpackNormalRG(v.texcoord1.rg, 1));
                o.forwardNormal = mul(forwardNormal, tangentTransform);
                o.uv0 = v.texcoord0;
                return o;
            }
            
            float4 frag(GraphVertexOutput i): SV_Target0
            {
                float4 uv0 = i.uv0;
                DirectionalLightData light = _DirectionalLightDatas[0];
                float3 L = -light.forward.xyz;// 光源朝向
                // float3 V = GetWorldSpaceNormalizeViewDir(i.positionWS);// 视角
                // float3 N = normalize(i.normal);// 法线
                float3 F = normalize(i.forwardNormal);// 脸部正前方dir
                float3 FH = normalize(float3(F.x, 0, F.z));// F的XZ投影
                float3 LH = normalize(float3(L.x, 0, L.z));// L的XZ投影
                
                float halfLambert = dot(LH, FH) * 0.5 + 0.5;// F L夹角
                halfLambert = pow(halfLambert, 1 / _Gamma);
                float LRFlag = normalize(cross(FH, LH).y);// -1 / 1 表示左右
                
                float lightMap = 1 - SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, float2(uv0.x * - LRFlag, uv0.y)).r;
                float shadowThreshold = clamp(halfLambert, 0.001, 0.999);
                float shadowStep = saturate(1.0 - (lightMap - (shadowThreshold - _ShadowFeather)) / _ShadowFeather);// 从UTS抄的公式
                
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0.xy) * _Color;
                float4 shadowColor = baseColor * _ShadowColor;
                
                float4 finalColor = lerp(shadowColor, baseColor, shadowStep);
                
                return finalColor;
            }
            ENDHLSL
            
        }
    }
}

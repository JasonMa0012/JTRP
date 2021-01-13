Shader "JTRP/Hair"
{
    Properties
    {
        _MainTex ("Color Map", 2D) = "white" { }
        _LightMap ("Light Map", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.6, 0.6, 0.6, 1)
        _DmcShadowIntensity ("Dynamic Shadow Intensity", Range(0, 1)) = 0.5
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        [PowerSlider(6)]_ShadowFeather ("Shadow Feather", Range(0.0001, 1)) = 0.01
        
        _LightColor_H ("Light Color H", Color) = (0.3, 0.3, 0.3, 1)
        _LightColor_L ("Light Color L", Color) = (0.05, 0.05, 0.05, 1)
        _LightWidth ("Light Width", Range(0, 1)) = 0.9
        _LightLength ("Light Length", Range(0.1, 1)) = 0.5
        _LightFeather ("Light Feather H", Range(0, 0.5)) = 0.2
        _LightThreshold ("Light Threshold L", Range(0.01, 0.9)) = 0.1
        _LightIntShadow ("Light Intensity In Shadow", Range(0, 1)) = 0.3
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
    float _DmcShadowIntensity;
    float _ShadowThreshold;
    float _ShadowFeather;
    
    float4 _LightColor_H;
    float4 _LightColor_L;
    float _LightWidth;
    float _LightLength;
    float _LightFeather;
    float _LightThreshold;
    float _LightIntShadow;
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
                float3 normal: NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct GraphVertexOutput
            {
                float4 position: POSITION;
                half4 uv0: TEXCOORD0;
                float3 positionWS: TEXCOORD1;
                float3 normal: TEXCOORD2;
            };
            
            GraphVertexOutput vert(GraphVertexInput v)
            {
                GraphVertexOutput o;
                // 这里的World Space是相对于摄像机的
                //https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@7.1/manual/Camera-Relative-Rendering.html
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.position = TransformWorldToHClip(positionWS);
                o.positionWS = positionWS;
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv0 = v.texcoord0;
                
                return o;
            }
            
            float4 frag(GraphVertexOutput i): SV_Target0
            {
                float4 uv0 = i.uv0;
                //Packages\com.unity.render-pipelines.high-definition@7.1.6\Runtime\Lighting\LightDefinition.cs
                DirectionalLightData light = _DirectionalLightDatas[0];
                float3 L = -light.forward.xyz;
                float3 V = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float3 H = normalize(L + V);
                float3 N = normalize(i.normal);
                
                float halfLambert = dot(N, L) * 0.5 + 0.5;
                float shadowStep = saturate(1.0 - (halfLambert - (_ShadowThreshold - _ShadowFeather)) / _ShadowFeather);
                
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, uv0.xy);// 未使用A通道所以未校正Gamma
                shadowStep = max(shadowStep, lightMap.b);
                
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0.xy) * _Color;
                float4 dmcShadowColor = lerp((float4)1, _ShadowColor, _DmcShadowIntensity);
                float4 shadowColor = baseColor * lerp(dmcShadowColor, _ShadowColor, lightMap.b);
                
                float3 NV = mul(UNITY_MATRIX_V, N);// 顶点去做
                float3 HV = mul(UNITY_MATRIX_V, H);
                
                float NdotH = dot(normalize(NV.xz), normalize(HV.xz));// xz投影后NdotH
                NdotH = pow(NdotH, 6) * _LightWidth;//这里将6改为float或属性会遇到一些奇怪的bug，原因不明
                NdotH = pow(NdotH, 1 / _LightLength);//用gamma校正的公式简单控制长度
                
                float lightFeather = _LightFeather * NdotH;
                float lightStepMax = saturate(1 - NdotH + lightFeather);
                float lightStepMin = saturate(1 - NdotH - lightFeather);
                float3 lightColor_H = smoothstep(lightStepMin, lightStepMax, clamp(lightMap.r, 0, 0.99)) * _LightColor_H.rgb;
                float3 lightColor_L = smoothstep(_LightThreshold, 1, lightMap.r) * _LightColor_L.rgb;
                
                float4 finalColor = lerp(baseColor, shadowColor, shadowStep);
                finalColor.rgb += (lightColor_H + lightColor_L) * (1 - lightMap.b) * lerp(1, _LightIntShadow, shadowStep);
                
                return finalColor;
            }
            ENDHLSL
            
        }
    }
}

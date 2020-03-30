Shader "Jason Ma/Hair Pro"
{
    Properties
    {
        _MainTex ("Color Map", 2D) = "white" { }
        _LightMap ("Light Map", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
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
            
            float GetStep(float map, float width, float soft, float dot)
            {
                float max = saturate(dot + width);
                float min = saturate(dot - width);
                if (map >= min && map <= max)
                    return 1;
                else if(map > max)
                {
                    return smoothstep(max + soft, max, map);
                }
                else
                {
                    return smoothstep(min - soft, min, map);
                }
            }
            
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
                DirectionalLightData light = _DirectionalLightDatas[0];
                float3 L = -light.forward.xyz;
                float3 V = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float3 N = normalize(i.normal);
                float3 Up = float3(0, 1, 0);
                
                float4 lightMap = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, uv0.xy);
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0.xy) * _Color;
                
                float VdotN = dot(normalize(mul(UNITY_MATRIX_V, V).yz), normalize(mul(UNITY_MATRIX_V, N).yz)) * 0.5 + 0.5;
                float VdotUp = dot(V, Up) * 0.5 + 0.5;
                
                float width = 0.05;
                float soft = 0.001;
                
                float lightStep = GetStep(lightMap.r, width, soft, VdotUp);
                
                float4 finalColor = baseColor;
                
                finalColor.rgb = (float3)lightStep;
                
                return finalColor;
            }
            ENDHLSL
            
        }
    }
}

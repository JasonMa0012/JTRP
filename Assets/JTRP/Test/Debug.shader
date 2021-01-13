Shader "JTRP/Debug"
{
    Properties
    {
        _MainTex ("Color Map", 2D) = "white" { }
        
        _Color ("Color", Color) = (1, 1, 1, 1)
            
        
        [Toggle(_)]_ShowOutlineNormal ("Show Outline Normal", float) = 0
        [Toggle(_)]_ShowVertecColorR ("VC R", float) = 0
        [Toggle(_)]_ShowVertecColorG ("VC G", float) = 0
        [Toggle(_)]_ShowVertecColorB ("VC B", float) = 0
        [Toggle(_)]_ShowVertecColorA ("VC A", float) = 0
        [Toggle(_)]_ShowVertecColorRGB ("VC RGB", float) = 0
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
    float _ShowOutlineNormal;
    float _ShowVertecColorR;
    float _ShowVertecColorG;
    float _ShowVertecColorB;
    float _ShowVertecColorA;
    float _ShowVertecColorRGB;
    CBUFFER_END
    
    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    
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
                float4 color: COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct GraphVertexOutput
            {
                float4 position: POSITION;
                half4 uv0: TEXCOORD0;
                float3 positionWS: TEXCOORD1;
                float3 normal: TEXCOORD2;
                float4 color: COLOR;
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
                o.uv0 = v.texcoord0;
                o.color = v.color;
                
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
                
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0.xy) * _Color;
                
                float4 finalColor = baseColor;
                
                float3 outlineNormal = UnpackNormalRG(i.color.rg);
                finalColor.rgb = lerp(finalColor.rgb, float3(i.color.rg, 1), _ShowOutlineNormal);
                finalColor.rgb = lerp(finalColor.rgb, (float3)i.color.r, _ShowVertecColorR);
                finalColor.rgb = lerp(finalColor.rgb, (float3)i.color.g, _ShowVertecColorG);
                finalColor.rgb = lerp(finalColor.rgb, (float3)i.color.b, _ShowVertecColorB);
                finalColor.rgb = lerp(finalColor.rgb, (float3)i.color.a, _ShowVertecColorA);
                finalColor.rgb = lerp(finalColor.rgb, i.color.rgb, _ShowVertecColorRGB);
                
                
                return finalColor;
            }
            ENDHLSL
            
        }
    }
    CustomEditor "JTRP.ShaderDrawer.LWGUI"
}

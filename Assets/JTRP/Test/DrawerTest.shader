Shader "JTRP/DrawerTest"
{
    Properties
    {
        _MainTex ("Color Map", 2D) = "white" { }
        
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        
        
        [Tex(_, _group81Color)]  _group3 ("g3", 2D) = "white" { }
        
        [Main(g1)] _group ("g1", float) = 1
        [Sub(g1)]  _group2 ("g2", float) = 2
        
        [KWEnum(g1, name1, key1, name2, key2, name3, key3)]
        _g1_enum ("enum", float) = 0
        [Sub(g1key1)]  _enum1 ("enum1", float) = 0
        [Sub(g1key2)]  _enum2 ("enum2", float) = 0
        [Sub(g1key3)]  _enum3 ("enum3", float) = 0
        [Sub(g1key3)]  _enum30 ("enum30", float) = 0
        
        [Tex(g1)] [Normal] _group33 ("g33", 2D) = "white" { }
        [Sub(g1)] [HDR] _group3Color ("Color", Color) = (1, 1, 1, 1)
        [Title(g1, SubHeaderDecorator)]
        [SubToggle(g1, _)] _group5 ("g5", float) = 0

        [SubToggle(g1, _HHHHHHHHH)] _group6 ("ghhhh6", float) = 0 
        [Sub(g1_HHHHHHHHH)]  _enum30h ("enum30hh", float) = 0

        [SubPowerSlider(g1)]  _group4 ("g4", Range(0, 10)) = 2
        [SubPowerSlider(g1, 2)] _group7 ("g7", Range(0, 100)) = 0
        [Color(g1, _)] _group8Color ("Color", Color) = (1, 1, 1, 1)
        [Color(g1, _, _group8Color, _group81Color)] _group83Color ("Color4", Color) = (1, 1, 1, 1)
        [HideInInspector] _group81Color ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector] [HDR] _group82Color ("Color", Color) = (1, 1, 1, 1)
        
        
        [Main] _group21 ("_ShowOutlineNormal", float) = 1
        [Sub(_group21)]  _group22 ("g22", float) = 2
        [Sub(_group21)]  _group32 ("g32", 2D) = "white" { }
        [Sub(_group21)]  _group42 ("g42", Range(0, 10)) = 2
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
                return(float4)0;
            }
            ENDHLSL
            
        }
    }
    CustomEditor "JTRP.ShaderDrawer.LWGUI"
}

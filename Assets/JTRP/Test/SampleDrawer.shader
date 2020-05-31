Shader "JTRP/SampleDrawer"
{
    Properties
    {
        [Queue] _Queue ( "Queue", float) = 2000
        
        // use Header on builtin attribute
        [Header(Header)][NoScaleOffset]
        _MainTex ("Color Map", 2D) = "white" { }
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        
        // use Title on LWGUI attribute
        [Title(_, Title)]
        [Tex(_, _mColor2)] _tex ("tex color", 2D) = "white" { }
        
        // Create a folding group with key "g1"
        [Main(g1)] _group ("group", float) = 1
        [Sub(g1)]  _float ("float", float) = 2
        
        [KWEnum(g1, name1, key1, name2, key2, name3, key3)]
        _enum ("enum", float) = 0
        
        // Display when the keyword ("group + keyword") is activated
        [Sub(g1key1)] _enum1("enum1", float) = 0
        [Sub(g1key2)] _enum2 ("enum2", float) = 0
        [Sub(g1key3)] _enum3 ("enum3", float) = 0
        [Sub(g1key3)] _enum3_range ("enum3_range", Range(0, 1)) = 0
        
        [Tex(g1)][Normal] _normal ("normal", 2D) = "bump" { }
        [Sub(g1)][HDR] _hdr ("hdr", Color) = (1, 1, 1, 1)
        [Title(g1, Sample Title)]
        [SubToggle(g1, _)] _toggle ("toggle", float) = 0
        [SubToggle(g1, _KEYWORD)] _toggle_keyword ("toggle_keyword", float) = 0
        [Sub(g1_KEYWORD)]  _float_keyword ("float_keyword", float) = 0
        [SubPowerSlider(g1, 2)] _powerSlider ("powerSlider", Range(0, 100)) = 0

        // Display up to 4 colors in a single line
        [Color(g1, _, _mColor1, _mColor2, _mColor3)]
        _mColor ("multicolor", Color) = (1, 1, 1, 1)
        [HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
        [HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
        [HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)
        
        // Create a drop-down menu that opens by default, without toggle
        [Main(g2, _KEYWORD, 3)] _group2 ("group2 without toggle", float) = 1
        [Sub(g2)]  _float2 ("float2", float) = 2
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

Shader "JTRP/Water"
{
    Properties
    {
        _waterColor ("Water Color", Color) = (0.8, 0.8, 0.8, 1)
        _MainLightBlend ("Main Light Blend", Range(0, 1)) = 0.9
        _ViewAbsorption ("View Absorption", Range(0, 1)) = 0.9
        [PowerSlider(2)] _Speed ("Speed", Range(-10, 10)) = 1.5
        _Dir ("Direction", vector) = (1, 1, 0, 0)
        
        [Space(15)]
        [PowerSlider(3)] _depthFactor ("Depth Factor", Range(0.001, 10)) = 3.0
        [PowerSlider(3)] _depthBlendScale ("Depth Blend Scale", Range(0.001, 1)) = 1.0
        [PowerSlider(3)] _depthBlendPower ("Depth Blend Power", Range(0.001, 10)) = 1.5
        
        [Header(Caustics)]
        _MainTex ("Caustics Map", 2D) = "white" { }
        [HDR] _Color ("Caustics Color", Color) = (0.5, 0.5, 0.5, 1)
        
        [Header(Specular)]
        [HDR] _SpecularColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        [PowerSlider(4)] _Roughness ("Roughness", Range(0.001, 1)) = 0.01
        
        [Header(Normal)]
        _NormalMap ("Normal Map", 2D) = "black" { }
        [PowerSlider(3)] _NormalScale ("Normal Scale", Range(0, 1)) = 0.15
        _NormalMap2 ("Normal Map2", 2D) = "black" { }
        [PowerSlider(3)] _NormalScale2 ("Normal Scale2", Range(0, 1)) = 0.15
        
        [Header(Distortion)]
        [PowerSlider(1.5)] _DistortionScale ("Distortion Scale", Range(0, 1)) = 0.1
        
        [Header(Reflection)]
        _CubemapTexture ("Reflection Map", Cube) = "black" { }
        _ReflectionColor ("Reflection Color", Color) = (0.4, 0.5, 0.7, 1)
        [PowerSlider(2)] _ReflectionNormalBlend ("Normal Blend", Range(0, 1)) = 0.15
        [PowerSlider(2)] _FresnelPower ("Fresnel Power", Range(1, 100)) = 5
        
        [Header(Foam)]
        _FoamNoise ("Noise", 2D) = "black" { }
        _FoamColor ("Foam Color", Color) = (0.5, 0.5, 0.5, 1)
        [PowerSlider(2)] _FoamRange ("Foam Range", Range(0.01, 10)) = 7
        [PowerSlider(2)] _NoisePower ("Noise Power", Range(0.01, 10)) = 3.0
        
        
        [Header(Tessellation)]
        _TessellationEdgeLength ("Tessellation Edge Length", float) = 200
        _TessellationStart ("Start", float) = 5
        _TessellationEnd ("End", float) = 100
    }
    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
    
    CBUFFER_START(UnityPerMaterial)
    uniform float4 _MainTex_ST;
    uniform float4 _NormalMap_ST;
    uniform float4 _NormalMap2_ST;
    uniform float4 _FoamNoise_ST;
    
    uniform float4 _Color;
    uniform float4 _SpecularColor;
    uniform float3 _waterColor;
    uniform float4 _ReflectionColor;
    uniform float4 _FoamColor;
    uniform float _MainLightBlend;
    uniform float _depthFactor;
    uniform float _depthBlendPower;
    uniform float _depthBlendScale;
    uniform float _Speed;
    uniform float2 _Dir;
    uniform float _NormalScale;
    uniform float _NormalScale2;
    uniform float _ViewAbsorption;
    uniform float _DistortionScale;
    uniform float _Roughness;
    uniform float _FresnelPower;
    uniform float _ReflectionNormalBlend;
    uniform float _FoamRange;
    uniform float _NoisePower;
    
    uniform float _TessellationEdgeLength;
    uniform float _TessellationStart;
    uniform float _TessellationEnd;
    
    CBUFFER_END
    
    TEXTURE2D(_MainTex);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_NormalMap2);
    TEXTURE2D(_FoamNoise);
    
    TEXTURE2D(_RampMap);
    TEXTURECUBE(_CubemapTexture);
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType" = "Transparents" "Queue" = "Transparent-100" }
        
        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            
            // Blend SrcAlpha OneMinusSrcAlpha
            ZWrite off
            Cull off
            
            HLSLPROGRAM
            
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            #pragma require tessellation tessHW
            
            // Supported shadow modes per light type
            #pragma multi_compile SHADOW_HIGH SHADOW_LOW SHADOW_MEDIUM
            #pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
            
            #include "WaterPass.hlsl"
            
            #pragma vertex TessellationVertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment frag
            
            ENDHLSL
            
        }
    }
}

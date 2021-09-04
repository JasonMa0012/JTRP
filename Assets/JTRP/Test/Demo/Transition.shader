Shader "JTRP/FX/Transition"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _Step ("step", Range(0, 1)) = 0.5
        [Toggle(_)]_Inverse ("inverse", float) = 0
    }
    
    HLSLINCLUDE
    
    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    
    struct Attributes
    {
        float4 vertex: POSITION;
        float4 texcoord0: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct Varyings
    {
        float4 positionCS: SV_POSITION;
        float2 texcoord: TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
        output.positionCS = TransformWorldToHClip(positionWS);
        output.texcoord = input.texcoord0.xy;
        return output;
    }
    
    CBUFFER_START(UnityPerMaterial)
    uniform float4 _Color;
    uniform float4 _MainTex_ST;
    uniform float _Step;
    uniform float _Inverse;
    CBUFFER_END
    
    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    
    float4 CustomPostProcess(Varyings input): SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        
        float4 outColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord.xy);
        
        outColor.rgb = lerp(_Color.rgb, outColor.rgb, _Inverse? step(Luminance(outColor.rgb), _Step): step(_Step, Luminance(outColor.rgb)));
        
        return outColor;
    }
    
    ENDHLSL
    
    SubShader
    {
        Pass
        {
            Name "Transition"
            
            ZWrite Off
            ZTest Always
            Blend off
            Cull back
            
            HLSLPROGRAM
            
            #pragma fragment CustomPostProcess
            #pragma vertex Vert
            ENDHLSL
            
        }
    }
    Fallback Off
}

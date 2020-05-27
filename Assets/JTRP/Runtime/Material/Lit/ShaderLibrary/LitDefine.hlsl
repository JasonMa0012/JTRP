#ifndef LITTOONDEFINE
    #define LITTOONDEFINE
    
    struct LitToonContext
    {
        float3 V;// view dir world space
        float3 L;// light
        float3 H;// half
        float3 T;// tangent
        float3 B;// binormal
        float3 N;// normal
        float3 SN;// smoothed normal
        float3 ON;// original normal
        
        float2 uv0;
        #ifdef VARYINGS_NEED_TEXCOORD1
            float2 uv1;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD2
            float2 uv2;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD3
            float2 uv3;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD4
            float2 uv4;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD5
            float2 uv5;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD6
            float2 uv6;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD7
            float2 uv7;
        #endif
        
        float exposure;
        float halfLambert;// 0: dark 1: bright
        float ONHalfLambert;// 0: dark 1: bright
        float shadowStep;// 0: bright 1: dark
        float OShadowStep;// original shadowStep (without fixed shadow or self shadow)
        float roughness;
        
        float3 diffuse;
        float3 dirLightColor;
        float3 pointLightColor;// point / spot  light
        float3 brightBaseColor;
        float3 darkBaseColor;
        float3 envColor;
        
        float3 specular;
        float3 highLightColor;// dir + point / spot
        float3 matCapColor;
        
        float3 emissive;
    };
#endif
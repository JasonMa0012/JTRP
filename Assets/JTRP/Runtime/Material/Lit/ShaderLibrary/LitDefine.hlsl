struct LitToonContext
{
    float3 V;// view dir world space
    float3 L;// light
    float3 H;// half
    float3 T;// tangent
    float3 B;// binormal
    float3 N;// normal
    float2 uv0;
    float2 uv1;
    float2 uv2;
    float2 uv3;
    
    float exposure;
    float halfLambert;// 0: dark 1: bright
    float shadowStep;// 0: bright 1: dark
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

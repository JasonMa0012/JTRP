/*
Taken from optimized-ggx.hlsl by John Hable
http://www.filmicworlds.com/2014/04/29/optimizing-ggx-update/
*/

// PixelShader:  GenerateEnvLightingLUT, entry: GenerateEnvLightingLUT
// PixelShader:  GenerateGGXLightingDLUT, entry: GenerateGGXLightingDLUT
// PixelShader:  GenerateGGXLightingFVLUT, entry: GenerateGGXLightingFVLUT
// PixelShader:  PrefilterEnvLighting, entry: PrefilterEnvLighting, Defines: PREFILTER

#include "constants.fx"

Texture2D<float>  g_GGXBRDFDLUT             : register(t40);
Texture2D<float2> g_GGXBRDFFVLUT            : register(t41);
Texture2D<float2> g_EnvBRDFLUT              : register(t42);

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

float LightingFuncGGX_REF(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float alpha = roughness*roughness;

	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));
	float dotNV = saturate(dot(N,V));
	float dotNH = saturate(dot(N,H));
	float dotLH = saturate(dot(L,H));

	float F, D, vis;

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	vis = G1V(dotNL,k)*G1V(dotNV,k);

	float specular = dotNL * D * F * vis;
	return specular;
}

float LightingFuncGGX_OPT1(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float alpha = roughness*roughness;

	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));
	float dotLH = saturate(dot(L,H));
	float dotNH = saturate(dot(N,H));

	float F, D, vis;

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	vis = G1V(dotLH,k)*G1V(dotLH,k);

	float specular = dotNL * D * F * vis;
	return specular;
}


float LightingFuncGGX_OPT2(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float alpha = roughness*roughness;

	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));

	float dotLH = saturate(dot(L,H));
	float dotNH = saturate(dot(N,H));

	float F, D, vis;

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	float k2 = k*k;
	float invK2 = 1.0f-k2;
	vis = rcp(dotLH*dotLH*invK2 + k2);

	float specular = dotNL * D * F * vis;
	return specular;
}

float2 LightingFuncGGX_FV(float dotLH, float roughness)
{
	float alpha = roughness*roughness;

	// F
	float F_a, F_b;
	float dotLH5 = pow(1.0f-dotLH,5);
	F_a = 1.0f;
	F_b = dotLH5;

	// V
	float vis;
	float k = alpha/2.0f;
	float k2 = k*k;
	float invK2 = 1.0f-k2;
	vis = rcp(dotLH*dotLH*invK2 + k2);

	return float2(F_a*vis,F_b*vis);
}

float LightingFuncGGX_D(float dotNH, float roughness)
{
	float alpha = roughness*roughness;
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;

	float D = alphaSqr/(pi * denom * denom);
	return D;
}

float LightingFuncGGX_OPT3(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));
	float dotLH = saturate(dot(L,H));
	float dotNH = saturate(dot(N,H));

	float D = LightingFuncGGX_D(dotNH,roughness);
	float2 FV_helper = LightingFuncGGX_FV(dotLH,roughness);
	float FV = F0*FV_helper.x + (1.0f-F0)*FV_helper.y;
	float specular = dotNL * D * FV;

	return specular;
}

float Pow4(float x)
{
	return x*x*x*x;
}

float Pow1_4(float x)
{
    return sqrt(sqrt(x));
}

float LightingFuncGGX_OPT4(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));
	float dotLH = saturate(dot(L,H));
	float dotNH = saturate(dot(N,H));

    float D = g_GGXBRDFDLUT.Sample(linearSampler, float2(Pow4(dotNH), roughness));
	float2 FV_helper =
        g_GGXBRDFFVLUT.Sample(linearSampler, float2(dotLH, roughness)).xy;

	float FV = F0*FV_helper.x + (1.0f-F0)*FV_helper.y;
	float specular = dotNL * D * FV;

	return specular;
}

// This version includes Stephen Hill's optimization
float LightingFuncGGX_OPT5(float3 N, float3 V, float3 L, float roughness, float F0)
{
	float3 H = normalize(V+L);

	float dotNL = saturate(dot(N,L));
	float dotLH = saturate(dot(L,H));
	float dotNH = saturate(dot(N,H));

    float D = g_GGXBRDFDLUT.Sample(linearSampler, float2(Pow4(dotNH), roughness)).x;
	float2 FV_helper =
        g_GGXBRDFFVLUT.Sample(linearSampler, float2(dotLH, roughness)).xy;

	float FV = F0*FV_helper.x + FV_helper.y;
	float specular = dotNL * D * FV;

	return specular;
}

// Source: Brian Karis Siggraph 2013 talk "Real Shading in Unreal Engine 4"
// http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf 

float3 ImportanceSampleGGX(float2 Xi, float Roughness, float3 N)
{
    float a = Roughness * Roughness;
    float Phi = 2 * PI * Xi.x;
    float CosTheta = sqrt((1 - Xi.y) / (1 + (a*a - 1) * Xi.y));
    float SinTheta = sqrt(1 - CosTheta * CosTheta);
    float3 H;
    H.x = SinTheta * cos(Phi);
    H.y = SinTheta * sin(Phi);
    H.z = CosTheta;
    float3 UpVector = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 TangentX = normalize(cross(UpVector, N));
    float3 TangentY = cross(N, TangentX);

    // Tangent to world space
    return TangentX * H.x + TangentY * H.y + N * H.z;
}

float G1V_Epic(float Roughness, float NoV)
{
    // no hotness remapping for env BRDF as suggested by Brian Karis
    float k = Roughness * Roughness;
    return NoV / (NoV * (1.0f - k) + k);
}

float G_Smith(float Roughness, float NoV, float NoL)
{
    return G1V_Epic(Roughness, NoV) * G1V_Epic(Roughness, NoL);
}

#ifdef PREFILTER
TextureCube<float4>     EnvMap          : register(t0);

float3 PrefilterEnvMap( float Roughness , float3 R )
{
    float3 N = R;
    float3 V = R;
    float3 PrefilteredColor = 0;
    float TotalWeight = 0.0000001f;
    const uint NumSamples = 1024;

    [loop]
    for( uint i = 0; i < NumSamples; i++ )
    {
        float2 Xi = float2(RandomNumBuffer[i * 2 + 0], RandomNumBuffer[i * 2 + 1]);
        float3 H = ImportanceSampleGGX( Xi, Roughness , N );
        float3 L = 2 * dot( V, H ) * H - V;
        float NoL = saturate( dot( N, L ) );

        if( NoL > 0 )
        {
            PrefilteredColor += EnvMap.SampleLevel( linearSampler, L, 0 ).rgb * NoL;
            TotalWeight += NoL;
        }
    }
    return PrefilteredColor / TotalWeight;
}
#endif

float2 IntegrateBRDF(float Roughness, float NoV)
{
    float3 V;

    float3 N = float3(0.0f,0.0f,1.0f);

    V.x = sqrt(1.0f - NoV * NoV); // sin
    V.y = 0;
    V.z = NoV; // cos
    float A = 0;
    float B = 0;
    const uint NumSamples = 1024;

    [loop]
    for (uint i = 0; i < NumSamples; i++)
    {
        float2 Xi = float2(RandomNumBuffer[i * 2 + 0], RandomNumBuffer[i * 2 + 1]);//Hammersley(i, NumSamples);
        float3 H = ImportanceSampleGGX(Xi, Roughness, N);
        float3 L = 2 * dot(V, H) * H - V;
        float NoL = saturate(L.z);
        float NoH = saturate(H.z);
        float VoH = saturate(dot(V, H));
        if (NoL > 0)
        {
            float G = G_Smith(Roughness, NoV, NoL);
            float G_Vis = G * VoH / (NoH * NoV);
            float Fc = pow(1 - VoH, 5);
            A += (1 - Fc) * G_Vis;
            B += Fc * G_Vis;
        }
    }
    return float2(A, B) / NumSamples;
}

float2 EnvLightingBRDF(float Roughness, float NoV)
{
    return g_EnvBRDFLUT.SampleLevel(linearSampler, float2(Roughness, NoV), 0);
}

cbuffer CubemapDownsample : register(b6)
{
    float       g_Mip;
    int         g_Face;
}

float3 GetCubeDirFromUVFace(uint face, float2 uv)
{
    float2 debiased = uv * 2.0f - 1.0f;

    float3 dir = 0;

    switch (face)
    {
        case 0: dir = float3(1, -debiased.y, -debiased.x); break;
        case 1: dir = float3(-1, -debiased.y, debiased.x); break;
        case 2: dir = float3(debiased.x, 1, debiased.y); break;
        case 3: dir = float3(debiased.x, -1, -debiased.y); break;
        case 4: dir = float3(debiased.x, -debiased.y, 1); break;
        case 5: dir = float3(-debiased.x, -debiased.y, -1); break;
    };

    return dir;
}


float4 PrefilterEnvLighting(VS_OUTPUT_POSTFX i) : SV_Target
{
#ifdef PREFILTER
    float3 cubeDir = normalize(GetCubeDirFromUVFace((uint)g_Face, i.uv.xy));
    float r = MipLevelToRoughness(g_Mip);
    return float4(PrefilterEnvMap(r, cubeDir), 1.0f);
#endif
    return 1.0f;
}



float2 GenerateEnvLightingLUT(VS_OUTPUT_POSTFX i) : SV_Target
{
    return IntegrateBRDF(i.uv.x, i.uv.y);
}

float GenerateGGXLightingDLUT(VS_OUTPUT_POSTFX i) : SV_Target
{  
    float dotNH = Pow1_4(i.uv.x);
    float r = i.uv.y;

    float d = LightingFuncGGX_D(dotNH, r);
    return d;
}

float2 GenerateGGXLightingFVLUT(VS_OUTPUT_POSTFX i) : SV_Target
{
    float2 fv = LightingFuncGGX_FV(i.uv.x, i.uv.y);
    return float2(max(fv.x - fv.y, 0.0f), fv.y);
}


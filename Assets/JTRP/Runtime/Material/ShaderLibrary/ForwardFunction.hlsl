




float GetShadowStep(float halfLambert, float step, float feather, float maxValue = 0, float selfShadow = 1)
{
    return //       阴影 -0.5 ~ 0.5 亮面   / 锐利 0.0001 ~ 1 平滑
    saturate(max((step - halfLambert * selfShadow) / feather, maxValue));
}

float StepAntiAliasing(float x, float y)
{
    float v = x - y;
    return saturate(v / fwidth(v));//fwidth(x) = abs(ddx(x) + ddy(x))
}

float2 GetWHRatio()
{
    return float2(_ScreenParams.y / _ScreenParams.x, 1);
}

float GetSSRimScale(float z)
{
    float w = (1.0 / (pow(z, 1.5) + 0.75)) * _ScreenParams.y / 1080;
    return w < 0.01 ? 0: w;
}
float GetOutLineScale(float z, float nPower = 0.8, float fPower = 0.1)
{
    return pow(z, z < 1 ?nPower: fPower);
}

float3 GetSmoothedWorldNormal(float2 uv7, float3x3 tbn)
{
    float3 normal = float3(uv7, 0);
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return mul(normal, tbn);
}

float3 GetHighLight(float3 color, float halfLambert, float step, float intensity, float feather,
float shadowMask, float intOnShadow, float maxValue = 0)
{
    float i = max(1 - GetShadowStep(halfLambert, step, feather), maxValue);
    return lerp(color * i, color * i * intOnShadow, shadowMask) * intensity;
}
float3 GetPowerHighLight(float3 color, float halfLambert, float power, float intensity,
float shadowMask, float intOnShadow)
{
    float i = pow(abs(halfLambert), power);
    return lerp(color * i, color * i * intOnShadow, shadowMask) * intensity;
}

#define G1V(dotNV, k) (1.0f / (dotNV * (1.0f - k) + k))
float PBRSpecular(float3 N, float3 V, float3 L, float roughness, float F0) // GGX
{
    float alpha = roughness * roughness;
    float3 H = normalize(V + L);
    
    float dotNL = saturate(dot(N, L));
    float dotNV = saturate(dot(N, V));
    float dotNH = saturate(dot(N, H));
    float dotLH = saturate(dot(L, H));
    
    float F, D, vis;
    // D
    float alphaSqr = alpha * alpha;
    float pi = 3.14159;
    float denom = dotNH * dotNH * (alphaSqr - 1.0) + 1.0;
    D = alphaSqr / (pi * denom * denom);
    
    // F
    float dotLH5 = pow(max(1.0 - dotLH, 0.0001), 5);
    F = F0 + (1.0 - F0) * dotLH5;
    
    // V
    float k = alpha / 2.0;
    vis = G1V(dotNL, k) * G1V(dotNV, k);
    
    return dotNL * D * F * vis;
}

float3 GetRimLight(float3 viewDirection, float3 normalDirection, float halfLambert,
float rimLightLength, float rimLightWidth, float rimLightFeather, float3 baseColor, float blend)
{
    float rimDot = saturate(1 - dot(viewDirection, normalDirection));
    float rimDot_X_lightDot = rimDot * pow(abs(halfLambert), 10 - rimLightLength);
    float light = smoothstep((1 - rimLightWidth) - rimLightFeather, (1 - rimLightWidth) + rimLightFeather, rimDot_X_lightDot);
    return light * lerp(1, baseColor, blend);
}


float2 RotateUV(float2 _uv, float _radian, float2 _piv, float _time)
{
    float RotateUV_ang = _radian;
    float RotateUV_cos = cos(_time * RotateUV_ang);
    float RotateUV_sin = sin(_time * RotateUV_ang);
    return(mul(_uv - _piv, float2x2(RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
}

float StrandSpecular(float3 T, float3 V, float3 L, float exponent, float strength)
{
    float3 H = normalize(L + V);
    float dotTH = dot(T, H);
    float sinTH = sqrt(1 - dotTH * dotTH);
    float dirAtten = smoothstep(-1, 0, dotTH);
    return dirAtten * pow(sinTH, exponent) * strength;
}

float3 ShiftTangent(float3 T, float3 S, float shift, float step)
{
    float3 shiftedT = T + (shift - step) * S;
    return normalize(shiftedT);
}

void AlphaGammaCorrection(inout float a1, inout float a2, inout float a3, inout float a4)
{
    float4 a = float4(a1, a2, a3, a4);
    a = pow(abs(a), 1 / 1.48);
    a1 = a.x;
    a2 = a.y;
    a3 = a.z;
    a4 = a.w;
}
void AlphaGammaCorrection(inout float alpha)
{
    alpha = pow(abs(alpha), 1 / 1.48);
}
void AlphaGammaCorrection(inout float alpha, inout float alpha2)
{
    AlphaGammaCorrection(alpha, alpha2, alpha, alpha);
}
void AlphaGammaCorrection(inout float alpha, inout float alpha2, inout float alpha3)
{
    AlphaGammaCorrection(alpha, alpha2, alpha3, alpha3);
}

float3 UnpackNormalRG(float2 packedNormal, real scale = 1.0)
{
    float3 normal;
    normal.xy = packedNormal.rg * 2.0 - 1.0;
    normal.xy *= scale;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

float4 ComputeScreenPos(float4 pos, float projectionSign)
{
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y * projectionSign) + o.w;
    o.zw = pos.zw;
    return o;
}

float3 GetShadowColor(float3 color, float shadowPower)
{
    shadowPower = max(1, shadowPower);
    return pow(abs(color), shadowPower);
}

float3 GetMatCap(float3 V, float3 lightColor, float2 uv, float shadowStep, float3 N, float matcapMask)
{
    #ifndef _MATCAP_ENABLE_ON
        return(float3)0;
    #else
        //鏡スクリプト判定：_sign_Mirror = -1 なら、鏡の中と判定.
        // float _sign_Mirror = i.mirrorFlag;
        float3 _Camera_Right = UNITY_MATRIX_V[0].xyz;
        float3 _Camera_Front = UNITY_MATRIX_V[2].xyz;
        float3 _Up_Unit = float3(0, 1, 0);
        // 垂直于世界 +Y 和 摄像机正方向 的轴？？？
        float3 _Right_Axis = cross(_Camera_Front, _Up_Unit);
        //鏡の中なら反転.
        // _Right_Axis *= _sign_Mirror < 0 ? - 1: 1;
        
        float _Camera_Right_Magnitude = length(_Camera_Right);
        float _Right_Axis_Magnitude = length(_Right_Axis);
        float _Camera_Roll_Cos = dot(_Right_Axis, _Camera_Right) / (_Right_Axis_Magnitude * _Camera_Right_Magnitude);
        float _Camera_Roll = acos(clamp(_Camera_Roll_Cos, -1, 1));
        float _Camera_Dir = _Camera_Right.y < 0 ? - 1: 1;
        float _Rot_MatCapUV_var_ang = - (_Camera_Dir * _Camera_Roll);
        
        float3 _NormalMapForMatCap_var = SAMPLE_TEXTURE2D(_MatCapNormalMap, sampler_MatCapNormalMap, TRANSFORM_TEX(uv, _MatCapNormalMap)).rgb * _BumpScaleMatcap;
        //v.2.0.5: MatCap with camera skew correction
        float3 viewNormal = (mul(UNITY_MATRIX_V, float4(normalize(N + _NormalMapForMatCap_var.rgb), 0))).rgb;
        float3 NormalBlend_MatcapUV_Detail = viewNormal.rgb * float3(-1, -1, 1);
        float3 NormalBlend_MatcapUV_Base = (mul(UNITY_MATRIX_V, float4(V, 0)).rgb * float3(-1, -1, 1)) + float3(0, 0, 1);
        // 修正摄像机旋转后
        float3 noSknewViewNormal = NormalBlend_MatcapUV_Base * dot(NormalBlend_MatcapUV_Base, NormalBlend_MatcapUV_Detail) / NormalBlend_MatcapUV_Base.b - NormalBlend_MatcapUV_Detail;
        float2 _ViewNormalAsMatCapUV = (noSknewViewNormal.rg * 0.5) + 0.5;
        float2 _Rot_MatCapUV_var = RotateUV(_ViewNormalAsMatCapUV, _Rot_MatCapUV_var_ang, float2(0.5, 0.5), 1.0);
        
        /*
        //鏡の中ならUV左右反転.
        if (_sign_Mirror < 0)
        {
            _Rot_MatCapUV_var.x = 1 - _Rot_MatCapUV_var.x;
        }*/
        //v.2.0.6 : LOD of Matcap
        float4 _MatCap_Sampler_var = SAMPLE_TEXTURE2D_LOD(_MatCap_Sampler, sampler_MatCap_Sampler, TRANSFORM_TEX(_Rot_MatCapUV_var, _MatCap_Sampler), _BlurLevelMatcap);
        //MatcapMask
        float _Tweak_MatcapMaskLevel_var = saturate(matcapMask + _Tweak_MatcapMaskLevel);
        
        // 高光颜色
        float3 _Is_LightColor_MatCap_var = (_MatCap_Sampler_var.rgb * _MatCapColor.rgb) * lightColor;
        // 调整阴影中强度
        float3 Set_MatCap = _Is_LightColor_MatCap_var * ((1.0 - shadowStep) + (shadowStep * _TweakMatCapOnShadow));
        return Set_MatCap * _Tweak_MatcapMaskLevel_var * _MatCap;
    #endif
}

float3 GetEmissive(float2 uv, float intensity1, float intensity2)
{
    #if defined(_EMISSIVE_ENABLE_ON)
        float time = _TimeParameters.x;
        float4 emiUV = float4(time * _Emissive_MoveHor1, time * _Emissive_MoveVer1, time * _Emissive_MoveHor2, time * _Emissive_MoveVer2);
        float emiMask1 = SAMPLE_TEXTURE2D(_Emissive_Mask1, sampler_Emissive_Mask1, TRANSFORM_TEX(uv, _Emissive_Mask1) + emiUV.xy).r * (intensity1 + _Emissive_Level1);
        float emiMask2 = SAMPLE_TEXTURE2D(_Emissive_Mask2, sampler_Emissive_Mask2, TRANSFORM_TEX(uv, _Emissive_Mask2) + emiUV.zw).r * (intensity2 + _Emissive_Level2);
        float3 emiColor1 = emiMask1 * lerp(_Emissive_ColorA1.rgb * _Emissive_IntA1, _Emissive_ColorB1.rgb * _Emissive_IntB1, sin(_Emissive_Speed1 * time) / 2 + 1);
        float3 emiColor2 = emiMask2 * lerp(_Emissive_ColorA2.rgb * _Emissive_IntA2, _Emissive_ColorB2.rgb * _Emissive_IntB2, sin(_Emissive_Speed2 * time) / 2 + 1);
        return max(emiColor1, emiColor2);
    #else
        return(float3)0;
    #endif
}

#if defined(HAS_LIGHTLOOP)
    #include "../Lit/ShaderLibrary\LitFunction.hlsl"
#endif
// 各向异性高光
// #ifdef _IS_HAIRMODE
//     float _NoiseMap_var = SAMPLE_TEXTURE2D_LOD(_NoiseMap, sampler_NoiseMap, TRANSFORM_TEX(context.uv1, _NoiseMap), _HighLit_LOD).r;
//     float3 HighLit_BT = ShiftTangent(bitangentWS, _HighLit_Scale, _NoiseMap_var, _HighLit_ScaleStep);
//     float3 HighLit_color = _HighLit_Color.rgb * _LightMap_var.r * StrandSpecular(HighLit_BT, V, L, _HighLit_Range, _HighLit_Intensity + _HighLit_Level);
//     float3 LowLit_color = _LowLit_Color.rgb * _LightMap_var.g * StrandSpecular(bitangentWS, V, L, _LowLit_Range, _LowLit_Intensity + _LowLit_Level);

//     matCapColorFinal = (max(HighLit_color, LowLit_color)) * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakMatCapOnShadow));
// #else

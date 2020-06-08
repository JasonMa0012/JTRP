//------------- Function --------------
float4 ComputeScreenPos(float4 pos, float projectionSign)
{
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y * projectionSign) + o.w;
    o.zw = pos.zw;
    return o;
}

// World Position reconstruction from Depth.
float3 ReconstructionWorldPos(float2 screenPos, float depthTex)
{
    // 不明原因使IV矩阵的XZ轴旋转相反，这里进行矫正
    float4x4 IVP = UNITY_MATRIX_I_VP;
    IVP._12_32 *= -1;
    IVP[1].xyz *= -1;
    float4 sceneWorldPos = mul(IVP, float4((screenPos * 2 - 1), depthTex, 1));
    sceneWorldPos /= sceneWorldPos.a;// homogeneous coordinates
    return GetAbsolutePositionWS(sceneWorldPos.xyz);// 从Camera-relative Space还原世界坐标
}

void GetScreenSpaceData(float2 screenPos, float3 viewPos, float3 worldPos, out float depthTex,
out float depthScene, out float depthWater, out float3 sceneWorldPos, out float depthWaterWorld)
{
    depthTex = SampleCameraDepth(screenPos);
    depthScene = LinearEyeDepth(depthTex, _ZBufferParams);
    depthWater = (depthScene + viewPos.z);
    
    sceneWorldPos = ReconstructionWorldPos(screenPos, depthTex);
    
    depthWaterWorld = (worldPos.y - sceneWorldPos.y);
}

void GetDistortionSSData(inout float2 screenPos, float2 distortionUV, float3 viewPos, float3 worldPos, inout float depthTex,
inout float depthScene, inout float depthWater, inout float3 sceneWorldPos, inout float depthWaterWorld, inout float3 screenColor)
{
    // 随距离缩放且防止边缘溢出
    distortionUV /= 1 + abs(viewPos.z);
    distortionUV *= saturate((1 - screenPos) * 20);
    float _depthTex = SampleCameraDepth(screenPos + distortionUV);
    float _depthScene = LinearEyeDepth(_depthTex, _ZBufferParams);
    float _depthWater = (_depthScene + viewPos.z);
    float3 _sceneWorldPos = ReconstructionWorldPos(screenPos + distortionUV, _depthTex);
    float _depthWaterWorld = (worldPos.y - _sceneWorldPos.y);
    
    // 再次采样以随水深变浅减少折射
    distortionUV *= min(saturate(_depthWaterWorld), saturate(depthWaterWorld));
    _depthTex = SampleCameraDepth(screenPos + distortionUV);
    _depthScene = LinearEyeDepth(_depthTex, _ZBufferParams);
    _depthWater = (_depthScene + viewPos.z);
    _sceneWorldPos = ReconstructionWorldPos(screenPos + distortionUV, _depthTex);
    _depthWaterWorld = (worldPos.y - _sceneWorldPos.y);
    
    // 仅在水盖住物体时折射
    if (_depthWater > 0)
    {
        depthTex = _depthTex;
        depthScene = _depthScene;
        depthWater = _depthWater;
        sceneWorldPos = _sceneWorldPos;
        depthWaterWorld = _depthWaterWorld;
        screenPos += distortionUV;
    }
    screenColor = SampleCameraColor(screenPos);
}

float DepthBlend(float waterDepth)
{
    return saturate(pow(abs(waterDepth * _depthBlendScale), _depthBlendPower));
}

float3 GetNormal(float2 uv, float2 t, float depthBlend = 1)
{
    float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, s_linear_repeat_sampler, t + TRANSFORM_TEX(uv, _NormalMap)) * 2 - 1;
    normalMap *= _NormalScale;
    float4 normalMap2 = SAMPLE_TEXTURE2D(_NormalMap2, s_linear_repeat_sampler, t * 0.75 + TRANSFORM_TEX(uv, _NormalMap2)) * 2 - 1;
    normalMap += normalMap2 * _NormalScale2;
    return normalize(float3(normalMap.x * depthBlend, 1.0, normalMap.y * depthBlend));
}

float GetShadow(DirectionalLightData mainLight, HDShadowContext sc, float2 screenPos, float3 worldPos, float3 normal)
{
    return GetDirectionalShadowAttenuation(sc, screenPos * _ScreenParams.xy, worldPos,
    normal, mainLight.shadowIndex, -mainLight.forward);
}

float3 GetCaustics(float2 t, float3 sceneWorldPos, float3 sceneNormal)
{
    // extract shared uv
    // sceneNormal.y系数控制竖直方向采样速度，xz控制水平采样方向
    float2 uv = sceneWorldPos.xz + sceneWorldPos.y * -(sceneNormal.x + sceneNormal.z) * (sceneNormal.y + 0.5) * 0.5;

    // original caustics
    float2 uv1 = t + TRANSFORM_TEX(uv, _MainTex);
    float3 result1 = SAMPLE_TEXTURE2D(_MainTex, s_linear_repeat_sampler, uv1).rgb;// World space texture sampling
    //return result1; //enable this line will return Jason's original caustics result 

    // sample caustics texture with an opposite flow direction uv again, the goal is to make caustics more random, caustics will never flow to a single direction uniformly
    float2 uv2 = t * float2(-1.07,-1.437) * 1.777 + TRANSFORM_TEX(uv, _MainTex);//any opposite direction uv, as long as uv2 is not equals uv1, it should looks good
    uv2 *= 0.777; //make texture bigger
    float3 result2 = SAMPLE_TEXTURE2D(_MainTex, s_linear_repeat_sampler, uv2).rgb;// World space texture sampling

    float intensityFix = 4; //because we will use min() next line, overall result will be darker, use a multiply to fix it
    return min(result1, result2) * intensityFix; //min() is the magic function of rendering fastest fake caustics!
}

float3 Scattering(float depth)
{
    return SAMPLE_TEXTURE2D(_RampMap, s_linear_clamp_sampler, float2(depth, 0.375h)).rgb;
}

float3 Absorption(float depth)
{
    return SAMPLE_TEXTURE2D(_RampMap, s_linear_clamp_sampler, float2(depth, 0.0h)).rgb;
}

float3 Highlights(float roughness, float3 normalWS, float3 viewDirectionWS)
{
    DirectionalLightData mainLight = _DirectionalLightDatas[0];
    
    float roughness2 = roughness * roughness;
    float3 halfDir = SafeNormalize(-mainLight.forward + viewDirectionWS);
    float NoH = saturate(dot(normalize(normalWS), halfDir));
    float LoH = saturate(dot(-mainLight.forward, halfDir));
    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155
    float d = NoH * NoH * (roughness2 - 1.h) + 1.0001h;
    float LoH2 = LoH * LoH;
    float specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * (roughness + 0.5h) * 4);
    // on mobiles (where float actually means something) denominator have risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
    #if defined(SHADER_API_MOBILE)
        specularTerm = specularTerm - HALF_MIN;
        specularTerm = clamp(specularTerm, 0.0, 5.0); // Prevent FP16 overflow on mobiles
    #endif
    return specularTerm * mainLight.color * GetCurrentExposureMultiplier();
}

float Fresnel(float3 normalWS, float3 viewDirectionWS)
{
    return pow(1.0 - saturate(dot(normalWS, viewDirectionWS)), _FresnelPower);
}

float3 SampleReflections(float3 normalWS, float3 viewDirectionWS, float fresnelTerm)
{
    float3 reflectVector = reflect(-viewDirectionWS, normalWS);
    float3 reflection = SAMPLE_TEXTURECUBE(_CubemapTexture, s_linear_repeat_sampler, reflectVector).rgb;
    return reflection * fresnelTerm;
}

float3 Foam(float worldWaterDepth, float3 worldPos, float2 t)
{
    float foamRange = 1 - pow(saturate(worldWaterDepth * _FoamRange), 0.75);
    float foamNoise = SAMPLE_TEXTURE2D(_FoamNoise, s_linear_repeat_sampler, t + TRANSFORM_TEX((worldPos.xz), _FoamNoise)).r;
    foamNoise = PositivePow(foamNoise, _NoisePower);
    float3 foam = step(foamNoise, foamRange);
    foam *= _FoamColor.rgb * _FoamColor.a;
    return foam;
}

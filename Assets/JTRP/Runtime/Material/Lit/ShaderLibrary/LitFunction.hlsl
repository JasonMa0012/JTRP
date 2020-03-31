#include "LitDefine.hlsl"

///////////// data /////////////
float ComputeRoughness(float roughness)
{
    // 粗糙度越大高光范围越大，强度越低
    _HighLightStep1 = lerp(0.9999, 0.95, roughness * roughness * roughness);
    _HighLightPower2 = PositivePow(_HighLightPower2, (1 - roughness * 0.8 + 0.2));
    
    float intensity = lerp(0.05, 1, PositivePow(1 - roughness, 4));
    _HighColorInt1 *= intensity;
    _HighColorPointInt1 *= intensity;
    _HighColorInt2 *= intensity;
    
    // _PointLightColorIntensity *= roughness * roughness ;
    
    return roughness;
}

void PreData(float3 normalMap, float dirLightInt, inout PackedVaryingsToPS packedInput, inout FragInputs input, inout PositionInputs posInput,
inout BuiltinData builtinData, inout SurfaceData surfaceData, inout LitToonContext context)
{
    // We need to readapt the SS position as our screen space positions are for a low res buffer, but we try to access a full res buffer.
    input.positionSS.xy = _OffScreenRendering > 0 ?(input.positionSS.xy * _OffScreenDownsampleFactor): input.positionSS.xy;
    uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize();
    // input.positionSS is SV_Position
    posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex);
    context.V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
    context.T = input.tangentToWorld[0];
    context.B = input.tangentToWorld[1];
    context.N = normalize(TransformTangentToWorld(normalMap, input.tangentToWorld));
    GetSurfaceAndBuiltinData(input, context.V, posInput, surfaceData, builtinData); //TODO:优化掉不需要的计算
    
    DirectionalLightData dirLight = _DirectionalLightDatas[0];
    context.L = -dirLight.forward;
    context.H = normalize(context.V + context.L);
    context.exposure = GetCurrentExposureMultiplier();
    context.dirLightColor = dirLight.color * context.exposure * dirLightInt;
    context.envColor = SampleBakedGI(posInput.positionWS, context.N, input.texCoord1.xy, input.texCoord2.xy) * context.exposure;
    
    
    context.halfLambert = 0.5 * dot(context.N, context.L) + 0.5;
}

void GetBaseColor(inout LitToonContext context, float3 mainTex, float skyIntensity, float shadowPower,
float3 shadowColor2, float shadowColorBlend2)
{
    float3 brightBaseColor = mainTex * context.dirLightColor;
    float3 evn = lerp(1, context.envColor, skyIntensity);
    
    context.brightBaseColor = brightBaseColor + brightBaseColor * evn;
    context.darkBaseColor = GetShadowColor(mainTex, shadowPower) * context.dirLightColor * evn;
    
    context.darkBaseColor += shadowColor2 * lerp(1, context.darkBaseColor, shadowColorBlend2);
}

//////////// lighting ///////////
float3 GetHighLight(float3 N, float3 V, float3 L, float3 lightColor, float shadowStep, float roughness, float intensity1 = 1, float intensity2 = 0)
{
    float3 result = 0;
    #ifdef _ENABLE_HIGHLIGHT_ON
        #ifdef _HL_PBR
            float spec = PBRSpecular(N, V, L, roughness, intensity1);
            result = spec * lightColor * _HighColor1.rgb * intensity1;
        #elif _HL_NPR
            float halfLambert = 0.5 * dot(normalize(V + L), N) + 0.5;
            float3 c1 = GetHighLight(lightColor * _HighColor1.rgb, halfLambert, _HighLightStep1,
            intensity1, _HighLightFeather1, shadowStep, intensity2? _HighColorIntOnShadow1: 0);
            float3 c2 = intensity2? GetPowerHighLight(lightColor * _HighColor2.rgb, halfLambert, _HighLightPower2,
            intensity2, shadowStep, _HighColorIntOnShadow2): (float3)0;
            result = max(c1, c2);
        #endif
    #endif
    
    return result;
}

DirectLighting ShadeSurface_Punctual(LightLoopContext lightLoopContext,
PositionInputs posInput, BuiltinData builtinData, //PreLightData preLightData,
LightData light, float3 N, float3 V, float roughness)
{
    DirectLighting lighting;
    ZERO_INITIALIZE(DirectLighting, lighting);
    
    float3 L;
    float4 distances; // {d, d^2, 1/d, d_proj}
    GetPunctualLightVectors(posInput.positionWS, light, L, distances);
    
    if (light.lightDimmer > 0)
    {
        float4 lightColor = EvaluateLight_Punctual(lightLoopContext, posInput, light, L, distances);
        lightColor.rgb *= lightColor.a; // 衰减
        
        float shadow = EvaluateShadow_Punctual(lightLoopContext, posInput, light, builtinData, N, L, distances);
        lightColor.rgb *= ComputeShadowColor(shadow, light.shadowTint, light.penumbraTint);
        
        float halfLambert = 0.5 * dot(N, L) + 0.5;
        float shadowStep = GetShadowStep(halfLambert, _PointLightStep, _PointLightFeather);
        
        lighting.diffuse = lightColor.rgb * light.diffuseDimmer * (1 - shadowStep);
        lighting.specular = GetHighLight(N, V, L, lighting.diffuse, shadowStep, roughness, _HighColorPointInt1);
    }
    
    return lighting;
}

float PointLightLoop(inout LitToonContext toonContext, PositionInputs posInput, // PreLightData preLightData, BSDFData bsdfData,
BuiltinData builtinData, float pointIntensity, float specularInt)
{
    #ifdef _SURFACE_TYPE_TRANSPARENT
        uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_TRANSPARENT;
    #else
        uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;
    #endif
    LightLoopContext context;
    
    context.shadowContext = InitShadowContext();
    context.shadowValue = 1;
    context.sampleReflection = 0;
    ApplyCameraRelativeXR(posInput.positionWS);
    InitContactShadow(posInput, context);
    
    AggregateLighting aggregateLighting;
    ZERO_INITIALIZE(AggregateLighting, aggregateLighting); // LightLoop is in charge of initializing the struct
    
    if (featureFlags & LIGHTFEATUREFLAGS_PUNCTUAL)
    {
        uint lightCount, lightStart;
        
        #ifndef LIGHTLOOP_DISABLE_TILE_AND_CLUSTER
            GetCountAndStart(posInput, LIGHTCATEGORY_PUNCTUAL, lightStart, lightCount);
        #else   // LIGHTLOOP_DISABLE_TILE_AND_CLUSTER
            lightCount = _PunctualLightCount;
            lightStart = 0;
        #endif
        
        bool fastPath = false;
        #if SCALARIZE_LIGHT_LOOP
            uint lightStartLane0;
            fastPath = IsFastPath(lightStart, lightStartLane0);
            
            if (fastPath)
            {
                lightStart = lightStartLane0;
            }
        #endif
        
        uint v_lightListOffset = 0;
        uint v_lightIdx = lightStart;
        
        while(v_lightListOffset < lightCount)
        {
            v_lightIdx = FetchIndex(lightStart, v_lightListOffset);
            uint s_lightIdx = ScalarizeElementIndex(v_lightIdx, fastPath);
            if(s_lightIdx == -1)
                break;
            
            LightData s_lightData = FetchLight(s_lightIdx);
            
            if(s_lightIdx >= v_lightIdx)
            {
                v_lightListOffset ++ ;
                if(IsMatchingLightLayer(s_lightData.lightLayers, builtinData.renderingLayers))
                {
                    // DirectLighting lighting = EvaluateBSDF_Punctual(context, V, posInput, preLightData, s_lightData, bsdfData, builtinData);
                    DirectLighting lighting = ShadeSurface_Punctual(context, posInput, builtinData, //preLightData,
                    s_lightData, toonContext.N, toonContext.V, toonContext.roughness);
                    
                    AccumulateDirectLighting(lighting, aggregateLighting);
                }
            }
        }
    }
    
    toonContext.pointLightColor = aggregateLighting.direct.diffuse * toonContext.exposure * pointIntensity;
    toonContext.highLightColor += (aggregateLighting.direct.specular + aggregateLighting.indirect.specularReflected) * saturate(specularInt);
    return context.shadowValue;
}
float3 ToonLightColorAddMode(float3 baseColor, float3 addLightColor)
{
    return lerp(1, baseColor, _LightColorBlend) * addLightColor;
}
float3 StdToonDiffuseLightingModel(LitToonContext context, float shadowIntensity, float3 shadowColor)
{
    float3 diffuse = lerp(context.brightBaseColor, context.darkBaseColor, context.shadowStep * shadowIntensity);
    
    diffuse *= lerp(1, shadowColor, context.shadowStep);
    
    return diffuse + ToonLightColorAddMode(context.brightBaseColor, context.pointLightColor);
}


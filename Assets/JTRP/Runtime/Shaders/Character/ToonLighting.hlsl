#ifndef JTRP_TOON_LIGHTING
    #define JTRP_TOON_LIGHTING
    
    //================= Data =================
    void GetUVs(inout LitToonContext context, FragInputs input)
    {
        context.uv0 = input.texCoord0.xy;
        #ifdef VARYINGS_NEED_TEXCOORD1
            context.uv1 = input.texCoord0.zw;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD2
            context.uv2 = input.texCoord1.xy;
        #endif
        #ifdef VARYINGS_NEED_TEXCOORD3
            context.uv3 = input.texCoord1.zw;
        #endif
    }
    
    float SetRoughness(float roughness = 0)
    {
        // 粗糙度越大高光范围越大，强度越低
        _HighLightStep1 = lerp(0.9999, 0.95, roughness * roughness * roughness);
        _HighLightPower2 = PositivePow(_HighLightPower2, (1 - roughness * 0.8 + 0.2));
        
        float intensity = lerp(0.05, 1, PositivePow(1 - roughness, 4));
        _HighColorInt1 *= intensity;
        _HighColorPointInt1 *= intensity;
        _HighColorInt2 *= intensity;
        _HighColorPointInt2 *= intensity;
        
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
        context.ON = input.tangentToWorld[2];
        context.N = normalize(TransformTangentToWorld(normalMap, input.tangentToWorld));
        
        GetSurfaceAndBuiltinData(input, context.V, posInput, surfaceData, builtinData);
        
        DirectionalLightData dirLight = _DirectionalLightDatas[0];
        context.L = -dirLight.forward;
        context.H = normalize(context.V + context.L);
        context.exposure = GetCurrentExposureMultiplier();
        context.dirLightColor = dirLight.color * context.exposure * dirLightInt;
        context.envColor = SampleBakedGI(posInput.positionWS, context.N, input.texCoord1.xy, input.texCoord2.xy) * context.exposure;
        
        context.halfLambert = 0.5 * dot(context.N, context.L) + 0.5;
        context.ONHalfLambert = 0.5 * dot(lerp(context.ON, context.N, _DiffuseNormalBlend), context.L) + 0.5;
    }
    void PreData(float dirLightInt, inout PackedVaryingsToPS packedInput, inout FragInputs input, inout PositionInputs posInput,
    inout BuiltinData builtinData, inout SurfaceData surfaceData, inout LitToonContext context)
    {
        input.positionSS.xy = _OffScreenRendering > 0 ?(input.positionSS.xy * _OffScreenDownsampleFactor): input.positionSS.xy;
        uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize();
        // input.positionSS is SV_Position
        posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex);
        context.V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
        context.T = input.tangentToWorld[0];
        context.B = input.tangentToWorld[1];
        context.N = input.tangentToWorld[2];
        context.ON = input.tangentToWorld[2];
        
        GetSurfaceAndBuiltinData(input, context.V, posInput, surfaceData, builtinData);
        
        DirectionalLightData dirLight = _DirectionalLightDatas[0];
        context.L = -dirLight.forward;
        context.H = normalize(context.V + context.L);
        context.exposure = GetCurrentExposureMultiplier();
        context.dirLightColor = dirLight.color * context.exposure * dirLightInt;
        context.envColor = SampleBakedGI(posInput.positionWS, context.N, input.texCoord1.xy, input.texCoord2.xy) * context.exposure;
        
        context.halfLambert = 0.5 * dot(context.N, context.L) + 0.5;
        context.ONHalfLambert = context.halfLambert;
    }
    
    //================= Shading =================
    float3 ToonLightColorAddMode(float3 baseColor, float3 addLightColor)
    {
        return lerp(1, baseColor, _LightColorBlend) * addLightColor;
    }
    
    void ToonDiffuseLighting(inout LitToonContext context, float3 mainTex, float shadowIntensity, float3 shadowColor,
    float3 fixedShadowColor, float fixedShadowInt)
    {
        context.baseColor = mainTex * context.dirLightColor * lerp(1, context.envColor, _SkyColorIntensity);
        shadowIntensity *= context.shadowStep;
        shadowIntensity = max(shadowIntensity, fixedShadowInt);
        
        float step2 = GetShadowStep(context.ONHalfLambert, _Shadow_Step2, _Shadow_Feather2);
		
        float3 diffuse = context.baseColor * lerp(1, shadowColor, shadowIntensity);
        diffuse *= lerp(1, fixedShadowColor, fixedShadowInt);
        diffuse = lerp(diffuse, ShiftColorPurity(diffuse, _Shadow_Purity), shadowIntensity);
        diffuse += step2 * _ShadowColor2.rgb * _ShadowIntensity2 * lerp(1, context.baseColor, _ShadowColorBlend2);
        
        context.diffuse = diffuse + ToonLightColorAddMode(mainTex, context.pointLightColor);
    }
    
    //================= Lighting =================
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

    float3 GetHighLight(float3 N, float3 V, float3 L, float3 lightColor, float shadowStep, float roughness,
    float intensity1 = 1, float intensity2 = 0, float maxValue = 0)
    {
        float3 result = 0;
        #ifdef _ENABLE_HIGHLIGHT_ON
            #ifdef _HL_PBR
                float spec = PBRSpecular(N, V, L, roughness, intensity1);
                result = spec * lightColor * _HighColor1.rgb * intensity1;
            #else
                float halfLambert = 0.5 * dot(normalize(V + L), N) + 0.5;
                float3 c1 = GetHighLight(lightColor * _HighColor1.rgb, halfLambert, _HighLightStep1,
                intensity1, _HighLightFeather1, shadowStep, intensity2? _HighColorIntOnShadow1: 0, maxValue);
                float3 c2 = intensity2? GetPowerHighLight(lightColor * _HighColor2.rgb, halfLambert, _HighLightPower2,
                intensity2, shadowStep, _HighColorIntOnShadow2): (float3)0;
                result = max(c1, c2);
            #endif
        #endif
        
        return result;
    }
    
    DirectLighting ShadeSurface_Punctual(LightLoopContext lightLoopContext,
    PositionInputs posInput, BuiltinData builtinData, //PreLightData preLightData,
    LightData light, LitToonContext context, float roughness)
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
            
			// if SHADEROPTIONS_COLORED_SHADOW then SHADOW_TYPE == real3
            float shadow = EvaluateShadow_Punctual(lightLoopContext, posInput, light, builtinData, context.N, L, distances).x;
            shadow = step(0.5, shadow);
            lightColor.rgb *= ComputeShadowColor(shadow, light.shadowTint, light.penumbraTint);
            
            float halfLambert = 0.5 * dot(context.ON, L) + 0.5;
            float shadowStep = GetShadowStep(halfLambert, _PointLightStep, _PointLightFeather);
            
            lighting.diffuse = lightColor.rgb * light.diffuseDimmer * (1 - shadowStep);
            lighting.specular = GetHighLight(context.N, context.V, L, lighting.diffuse, shadowStep, roughness, _HighColorPointInt1, _HighColorPointInt2);
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
                        DirectLighting lighting = ShadeSurface_Punctual(context, posInput, builtinData, //preLightData,
                        s_lightData, toonContext, toonContext.roughness);
                        
                        AccumulateDirectLighting(lighting, aggregateLighting);
                    }
                }
            }
        }
        
        toonContext.pointLightColor = aggregateLighting.direct.diffuse * toonContext.exposure * pointIntensity;
        toonContext.highLightColor += (aggregateLighting.direct.specular + aggregateLighting.indirect.specularReflected) * saturate(specularInt);
        return context.shadowValue.x;
    }
    
    float GetSelfShadow(LitToonContext context, PositionInputs posInput)
    {
        #ifndef _ENABLE_SELFSHADOW
            return 1;
        #else
            DirectionalLightData dirLight = _DirectionalLightDatas[0];
            HDShadowContext sc = InitShadowContext();
            float attenuation = GetDirectionalShadowAttenuation(sc, posInput.positionSS, posInput.positionWS,
            context.N, dirLight.shadowIndex, context.L);
            attenuation = StepAntiAliasing(attenuation, 0.5);
            
            // 0:shadow
            float selfShadow = (attenuation * 0.5) + 0.5;
            return clamp(selfShadow, 0.001, 1);
        #endif
    }
    
    float3 GetRimLight(LitToonContext context, PositionInputs posInput, FragInputs input, float mask = 1)
    {
        #ifndef _RIMLIGHT_ENABLE_ON
            return(float3)0;
        #endif
        float3 c = 0;
        
        float luminance = Luminance(context.pointLightColor);
        
        if (_RimLight_Mode == 0)
        {
            float3 rimColor = GetRimLight(context.V, context.N, context.halfLambert, _RimLightLength, _RimLightWidth,
            _RimLightFeather, context.baseColor, _RimLightBlend) * (1 - context.shadowStep) * _RimLightIntensity;// 亮面
            
            float3 rimColor2 = GetRimLight(context.V, context.N, 1 - context.halfLambert, _RimLightLength, _RimLightWidth,
            _RimLightFeather, context.baseColor, _RimLightBlend2) * context.shadowStep * _RimLightIntensity2;// 暗面
            
            rimColor *= lerp(_RimLightColor.rgb, context.pointLightColor, luminance * _RimLightBlendPoint) * mask;
            rimColor2 *= lerp(_RimLightColor2.rgb, context.pointLightColor, luminance * _RimLightBlendPoint2) * mask;
            c = Max3(c, rimColor, rimColor2);
        }
        else
        {
            float2 L_View = normalize(mul((float3x3)UNITY_MATRIX_V, context.L).xy);
            float2 N_View = normalize(mul((float3x3)UNITY_MATRIX_V, context.N).xy);
            float lDotN = saturate(dot(N_View, L_View) + _RimLightLength * 0.1);
            float scale = lDotN * _RimLightWidth * input.color.b * 40 * GetSSRimScale(posInput.linearDepth);
            float2 ssUV1 = clamp(posInput.positionSS + N_View * scale, 0, _ScreenParams.xy - 1);
            
            
            float depthDiff = LinearEyeDepth(LoadCameraDepth(ssUV1), _ZBufferParams) - posInput.linearDepth;
            float intensity = smoothstep(0.24 * _RimLightFeather * posInput.linearDepth, 0.25 * posInput.linearDepth, depthDiff);
            intensity *= lerp(1, _RimLightIntInShadow, context.shadowStep) * _RimLightIntensity * mask;
            
            float3 ssColor = intensity * lerp(1, context.baseColor, _RimLightBlend)
            * lerp(_RimLightColor.rgb, context.pointLightColor, luminance * _RimLightBlendPoint);
            
            c = max(c, ssColor);
        }
        return c;
    }


/*
	// 各向异性高光
	float StrandSpecular(float3 T, float3 V, float3 L, float exponent, float strength)
	{
		float3 H = normalize(L + V);
		float dotTH = dot(T, H);
		float sinTH = sqrt(1 - dotTH * dotTH);
		float dirAtten = smoothstep(-1, 0, dotTH);
		return dirAtten * pow(sinTH, exponent) * strength;
	}
	#ifdef _IS_HAIRMODE
	    float _NoiseMap_var = SAMPLE_TEXTURE2D_LOD(_NoiseMap, sampler_MainTex, TRANSFORM_TEX(context.uv1, _NoiseMap), _HighLit_LOD).r;
	    float3 HighLit_BT = ShiftTangent(bitangentWS, _HighLit_Scale, _NoiseMap_var, _HighLit_ScaleStep);
	    float3 HighLit_color = _HighLit_Color.rgb * _LightMap_var.r * StrandSpecular(HighLit_BT, V, L, _HighLit_Range, _HighLit_Intensity + _HighLit_Level);
	    float3 LowLit_color = _LowLit_Color.rgb * _LightMap_var.g * StrandSpecular(bitangentWS, V, L, _LowLit_Range, _LowLit_Intensity + _LowLit_Level);

	    matCapColorFinal = (max(HighLit_color, LowLit_color)) * ((1.0 - Set_FinalShadowMask) + (Set_FinalShadowMask * _TweakMatCapOnShadow));
	#else*/
    
#endif // JTRP_LITFUNCTION
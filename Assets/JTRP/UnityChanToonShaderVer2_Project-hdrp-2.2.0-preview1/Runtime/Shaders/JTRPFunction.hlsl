#ifndef UTS_JTRP_FUNCTION
#define UTS_JTRP_FUNCTION

//=========================================
// JTRP
//=========================================

TEXTURE2D(_ShadowColorRamp);
uniform float _EnableShadowColorRamp;
uniform float _FaceShadowBias;


uniform float4x4 _SphericalShadowCenter_Matrix_I_M;// form CS: SphericalShadowHelper
uniform float4x4 _SphericalShadowCenter_Matrix_M;
uniform float4 _SphericalShadowNormalScale;
uniform float _SphericalShadowIntensity;

float3 SphericalShadowNormal(float3 positionRWS, float3 normalWS)
{
	float3 sphericalNormal = normalize(mul(_SphericalShadowCenter_Matrix_I_M, float4(GetAbsolutePositionWS(positionRWS), 1)).xyz);
	sphericalNormal *= _SphericalShadowNormalScale.xyz * _SphericalShadowNormalScale.w;
	float3 sphericalNormalWS = normalize(mul((float3x3)_SphericalShadowCenter_Matrix_M, sphericalNormal));

	return lerp(normalWS, sphericalNormalWS, _SphericalShadowIntensity);
}

#ifdef JTRP_FACE_SHADER

	TEXTURE2D(_HairShadowWidthRamp);
	TEXTURE2D(_JTRP_Mask_Map);
	uniform float _HairShadowWidth;
	uniform float _HairShadowRampMaxDistance;

	void GetJTRPHairShadow(inout float shadowValue, float3 screenPos, float3 lightDirWS)
	{
		float2 L = normalize(mul((float3x3)UNITY_MATRIX_V, lightDirWS).xy);
		float hairShadowWidthRamp = SampleRampSignalLine(_HairShadowWidthRamp, LinearEyeDepth(screenPos.z) / _HairShadowRampMaxDistance).r;
		float z = LOAD_TEXTURE2D_X(_JTRP_Mask_Map, screenPos.xy + L * hairShadowWidthRamp * _HairShadowWidth).x;
		shadowValue = min(shadowValue, step(z, screenPos.z));
	}

#endif

//=========================================
// HighLight
//=========================================

uniform half _EnableTangentHighLight;
uniform half4 _HairHighLightHighColor;
uniform half4 _HairHighLightLowColor;
uniform half _HairHighLightIntensityInShadow;

TEXTURE2D(_HairHighLightGradientRamp);
uniform half _GradientRampIntensity;
TEXTURE2D(_HighLightMaskMap);
TEXTURE2D(_HairHighLightMaskRamp);
TEXTURE2D(_HairHighLightOffsetRamp);
uniform float4 _HairHighLightRampST;
uniform half _HighLightMaskMapUV2;
uniform half _HighLightMaskGradientScale;
uniform half _HighLightMaskIntensity;
uniform half _HighLightMaskOffsetIntensity;
uniform half _HighLightMaskWidthIntensity;

uniform float _TangentHighLightWidth;
uniform float _TangentHighLightThreshold;
uniform float _TangentHighLightFeather;
uniform float _TangentHighLightLowWidth;

uniform half _HighLightRimOffset;
uniform half _HighLightRimThreshold;
uniform half _HighLightRimWidth;
uniform half _HighLightRimPower;

TEXTURE2D(_TangentDirMap);
uniform half _TangentDirMapIntensity;
uniform float4 _TangentDirMapScale;

uniform half _SphericalTangentIntensity;
uniform half _SphericalTangentProjectionIntensity;
uniform float3 _SphericalTangentScale;

// form CS: SphericalHairHighLightHelper
uniform float3 _HairCenterWS;
uniform float3 _HeadUpDirWS;
uniform float3 _HeadRightDirWS;


void GetTangentHighLight(inout float3 color, float3x3 TBN, float3 worldPos, float3 V, float2 uv, float2 uv2, float shadowValue)
{
	UNITY_BRANCH
	if (!_EnableTangentHighLight) return;

	float3 normal = TBN[2];

	// spherical tangent
	float3 sphericalNormal = normalize((worldPos - GetCameraRelativePositionWS(_HairCenterWS)) * _SphericalTangentScale.xyz);
	float3 sphericalTangent = normalize(cross(sphericalNormal, cross(sphericalNormal, _HeadUpDirWS)));
	// lerp tangent between sphere and cylinder
	float projectionIntensity = _SphericalTangentIntensity * _SphericalTangentProjectionIntensity;
	float projectedDotValue = dot(sphericalNormal, _HeadUpDirWS);
	float projectedThreshold = (dot(V, _HeadUpDirWS) * 0.5 + 0.5) * projectionIntensity;


	// Widen the highlight on the edge of each hair
	float dotValue_Rim = pow(1 - saturate(abs(dot(sphericalNormal, normal) * 0.5 + 0.5 - _HighLightRimThreshold) / _HighLightRimWidth), _HighLightRimPower) * _HighLightRimOffset;
	

	// 	_TangentDirMap baked form Houdini GameDev FlowMap Tool
	float3 tangentDirMap = saturate(SAMPLE_TEXTURE2D(_TangentDirMap, s_linear_clamp_sampler, uv2));
	tangentDirMap = float3(1 - tangentDirMap.x, tangentDirMap.y, 1 - tangentDirMap.z);
	float3 tangentTS = (tangentDirMap.xzy * 2 - 1) * _TangentDirMapScale.xyz * _TangentDirMapScale.w;
	float3 tangentWS = normalize(mul(tangentTS, TBN));
	float3 newTangent = lerp(TBN[1], normalize(cross(normal, cross(normal, tangentWS))), _TangentDirMapIntensity);
	newTangent = lerp(newTangent, sphericalTangent, _SphericalTangentIntensity);
	float dotValue = lerp(dot(newTangent, V), projectedDotValue, projectionIntensity) * 0.5 + 0.5;
	

	// Mask / Offset Ramp
	float rampU = acos(dot(_HeadRightDirWS, normalize(ProjectOnPlane(sphericalNormal, _HeadUpDirWS)))) * 0.5 + 0.5;
	float maskRamp = SampleRampSignalLine(_HairHighLightMaskRamp, frac(rampU * _HairHighLightRampST.x + _HairHighLightRampST.y)).r;
	float offsetRamp = SampleRampSignalLine(_HairHighLightOffsetRamp, frac(rampU * _HairHighLightRampST.z + _HairHighLightRampST.w)).r * 2 - 1;
	float gradientRamp = (SampleRampSignalLine(_HairHighLightGradientRamp, dot(sphericalNormal, _HeadUpDirWS) * 0.5 + 0.5).r * 2 - 1) * _GradientRampIntensity;

	// R:gradient mark the start to the end of the highlight with 0-1; G Mask B Offset A Width
	float4 maskMap = SAMPLE_TEXTURE2D(_HighLightMaskMap, s_linear_clamp_sampler, lerp(uv, uv2, _HighLightMaskMapUV2));
	float mask = lerp(1, maskMap.g * maskRamp, _HighLightMaskIntensity);
	float offsetMask = (maskMap.b * 2 - 1 + offsetRamp) * _HighLightMaskOffsetIntensity;
	float alphaReduceMask = (1 - mask);
	float widthMask = lerp(1, maskMap.a * 2, _HighLightMaskWidthIntensity);
	float gradientMask = (maskMap.r - 0.5) * _HighLightMaskGradientScale + gradientRamp;
	float threshold = lerp(_TangentHighLightThreshold, projectedThreshold + _TangentHighLightThreshold * 2 - 1, projectionIntensity);
	// x: with offsetMask
	float2 dotValueWithThreshold = abs(dotValue + gradientMask - float2(threshold + offsetMask, threshold));

	float width = (_TangentHighLightWidth + dotValue_Rim) * widthMask;
	float dotClamped = saturate(dotValueWithThreshold.x / width);
	float hardness = saturate(_TangentHighLightFeather) * 0.5;

	float3 highColor = _HairHighLightHighColor.rgb * lerp(1, color, _HairHighLightHighColor.a)
	* smoothstep(0.5 - hardness, 0.5 + hardness, 1 - dotClamped - alphaReduceMask);
	float3 lowColor = _HairHighLightLowColor.rgb * lerp(1, color, _HairHighLightLowColor.a)
	* smoothstep(0.5 - 0.5, 0.5 + 0.5, 1 - saturate(dotValueWithThreshold.y / _TangentHighLightLowWidth) - alphaReduceMask * alphaReduceMask);

	color += max(highColor, lowColor) * lerp(_HairHighLightIntensityInShadow, 1, shadowValue);
}

//=========================================
// SS Rim
//=========================================

uniform half _EnableSSRim;
uniform half _SSRimIntensity;
uniform half4 _SSRimColor;
uniform half _SSRimWidth;
uniform half _SSRimRampMaxDistance;
TEXTURE2D(_SSRimWidthRamp);
uniform half _SSRimLength;
uniform half _SSRimInvertLightDir;
uniform float _SSRimFeather;
uniform half _SSRimInShadow;
TEXTURE2D(_SSRimMask);

void GetSSRimLight(inout float3 color, PositionInputs posInput, float2 uv, float3 lightDirWS, float3 normalDirWS, half shadowValue)
{
	UNITY_BRANCH
	if (!_EnableSSRim) return;

	half4 mask = SAMPLE_TEXTURE2D(_SSRimMask, s_linear_clamp_sampler, uv);
	half widthRamp = SampleRampSignalLine(_SSRimWidthRamp, posInput.linearDepth / _SSRimRampMaxDistance).r;
	
	float2 L_View = normalize(mul((float3x3)UNITY_MATRIX_V, lightDirWS).xy) * (_SSRimInvertLightDir ? - 1: 1);
	float2 N_View = normalize(mul((float3x3)UNITY_MATRIX_V, normalDirWS).xy);
	float lDotN = saturate(dot(N_View, L_View) + _SSRimLength);
	float scale = mask.r * widthRamp * lDotN * _SSRimWidth * GetScaleWithHight();
	float2 ssUV1 = clamp(posInput.positionSS + N_View * scale, 0, _ScreenParams.xy - 1);
	
	float depthDiff = LinearEyeDepth(LoadCameraDepth(ssUV1), _ZBufferParams) - posInput.linearDepth;
	float intensity = smoothstep(0, _SSRimFeather, depthDiff);
	intensity *= mask.a * lerp(1, _SSRimInShadow, shadowValue) * _SSRimIntensity;
	
	color += _SSRimColor * intensity * lerp(1, color, _SSRimColor.a);
}


#endif
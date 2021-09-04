#ifndef JTRP_FORWARD_FUNCTION
	#define JTRP_FORWARD_FUNCTION

	#include "../Common/Common.hlsl"

	float GetShadowStep(float halfLambert, float step, float feather, float selfShadow = 1)
	{
		return //       阴影 -0.5 ~ 0.5 亮面   / 锐利 0.0001 ~ 1 平滑
		saturate((step - halfLambert * selfShadow) / feather);
	}

	float GetScaleWithHight()
	{
		return _ScreenParams.y / 1080;
	}

	float GetSSRimScale(float z)
	{
		float w = (1.0 / (PositivePow(z + saturate(UNITY_MATRIX_P._m00), 1.5) + 0.75)) * GetScaleWithHight();
		w *= lerp(1, UNITY_MATRIX_P._m00, 0.60 * saturate(0.25 * z * z));
		return w < 0.01 ? 0: w;
	}

	float GetOutLineScale(float z, float nPower = 1.05, float fPower = 0.2)
	{
		return SAMPLE_TEXTURE2D_LOD(_Outline_Width_Ramp, s_linear_clamp_sampler, float2(z / _Outline_Ramp_Max_Distance, 0.5), 0).r;
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

	float3 GetRimLight(float3 viewDirection, float3 normalDirection, float halfLambert,
	float rimLightLength, float rimLightWidth, float rimLightFeather, float3 baseColor, float blend)
	{
		float rimDot = saturate(1 - dot(viewDirection, normalDirection));
		float rimDot_X_lightDot = rimDot * pow(abs(halfLambert), 10 - rimLightLength);
		float light = smoothstep((1 - rimLightWidth) - rimLightFeather, (1 - rimLightWidth) + rimLightFeather, rimDot_X_lightDot);
		return light * lerp(1, baseColor, blend);
	}

	float3 ShiftTangent(float3 T, float3 S, float shift, float step)
	{
		float3 shiftedT = T + (shift - step) * S;
		return normalize(shiftedT);
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
			
			float3 _NormalMapForMatCap_var = SAMPLE_TEXTURE2D(_MatCapNormalMap, sampler_MainTex, TRANSFORM_TEX(uv, _MatCapNormalMap)).rgb * _BumpScaleMatcap;
			//v.2.0.5: MatCap with camera skew correction
			float3 viewNormal = (mul(UNITY_MATRIX_V, float4(normalize(N + _NormalMapForMatCap_var.rgb), 0))).rgb;
			float3 NormalBlend_MatcapUV_Detail = viewNormal.rgb * float3(-1, -1, 1);
			float3 NormalBlend_MatcapUV_Base = (mul(UNITY_MATRIX_V, float4(V, 0)).rgb * float3(-1, -1, 1)) + float3(0, 0, 1);
			// 修正摄像机旋转后
			float3 noSknewViewNormal = NormalBlend_MatcapUV_Base * dot(NormalBlend_MatcapUV_Base, NormalBlend_MatcapUV_Detail) / NormalBlend_MatcapUV_Base.b - NormalBlend_MatcapUV_Detail;
			float2 _ViewNormalAsMatCapUV = (noSknewViewNormal.rg * 0.5) + 0.5;
			float2 _Rot_MatCapUV_var = Rotate_UV(_ViewNormalAsMatCapUV, _Rot_MatCapUV_var_ang, float2(0.5, 0.5), 1.0);

			/*
			//鏡の中ならUV左右反転.
			if (_sign_Mirror < 0)
			{
				_Rot_MatCapUV_var.x = 1 - _Rot_MatCapUV_var.x;
			}*/
			//v.2.0.6 : LOD of Matcap
			float4 _MatCap_Sampler_var = SAMPLE_TEXTURE2D_LOD(_MatCap_Sampler, sampler_MainTex, TRANSFORM_TEX(_Rot_MatCapUV_var, _MatCap_Sampler), _BlurLevelMatcap);
			//MatcapMask
			float _Tweak_MatcapMaskLevel_var = matcapMask;
			
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
			float emiMask1 = SAMPLE_TEXTURE2D(_Emissive_Mask1, sampler_MainTex, TRANSFORM_TEX(uv, _Emissive_Mask1) + emiUV.xy).r * (intensity1 + _Emissive_Level1);
			float emiMask2 = SAMPLE_TEXTURE2D(_Emissive_Mask2, sampler_MainTex, TRANSFORM_TEX(uv, _Emissive_Mask2) + emiUV.zw).r * (intensity2 + _Emissive_Level2);
			float3 emiColor1 = emiMask1 * lerp(_Emissive_ColorA1.rgb * _Emissive_IntA1, _Emissive_ColorB1.rgb * _Emissive_IntB1, sin(_Emissive_Speed1 * time) / 2 + 1);
			float3 emiColor2 = emiMask2 * lerp(_Emissive_ColorA2.rgb * _Emissive_IntA2, _Emissive_ColorB2.rgb * _Emissive_IntB2, sin(_Emissive_Speed2 * time) / 2 + 1);
			return max(emiColor1, emiColor2);
		#else
			return(float3)0;
		#endif
	}

	#ifdef HAS_LIGHTLOOP
		#include "ToonLighting.hlsl"
	#endif
#endif
#ifndef JTRP_COMMON
#define JTRP_COMMON

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

#define inverselerp(a, b, x) saturate(((x) - (a)) / ((b) - (a)))

float2 GetWHRatio()
{
	return float2(_ScreenParams.y / _ScreenParams.x, 1);
}

float StepAntiAliasing(float x, float y)
{
	float v = x - y;
	return saturate(v / fwidth(v));//fwidth(x) = abs(ddx(x) + ddy(x))

}

#define SampleRampSignalLine(texture, u) (SAMPLE_TEXTURE2D_LOD(texture, s_linear_clamp_sampler, float2(u, 0.5), 0))

// ----------------------------------------------------------------------------
// Transform
// ----------------------------------------------------------------------------

float3 ProjectOnPlane(float3 vec, float3 normal)
{
	return vec - normal * dot(vec, normal);
}

float2 Rotate_UV(float2 _uv, float _radian, float2 _piv, float _time)
{
	float RotateUV_ang = _radian;
	float RotateUV_cos = cos(_time * RotateUV_ang);
	float RotateUV_sin = sin(_time * RotateUV_ang);
	return(mul(_uv - _piv, float2x2(RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
}

// Anti-perspective form Colin: Counteract perspective effects by replacing regular depth with uniform depth
uniform float _AntiPerspectiveIntensity;
void AntiPerspective(inout float4 clipPos)
{
	float centerVSz = mul(UNITY_MATRIX_V, float4(UNITY_MATRIX_M._m03_m13_m23, 1.0)).z;
	clipPos.xy *= lerp(1.0, abs(clipPos.w) / - centerVSz, _AntiPerspectiveIntensity);
}

// ASE
float2 UnStereo(float2 UV)
{
	#if UNITY_SINGLE_PASS_STEREO
		float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
		UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
	#endif
	return UV;
}

// ----------------------------------------------------------------------------
// Color
// ----------------------------------------------------------------------------
float3 ShiftColorPurity(float3 color, float purity)
{
	return lerp(Luminance(color), color, purity);
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


// ----------------------------------------------------------------------------
// Depth
// ----------------------------------------------------------------------------

float LinearEyeDepth(float z)
{
	return LinearEyeDepth(z, _ZBufferParams);
}

// https://forum.unity.com/threads/what-does-unity-exactly-do-when-we-modify-z-buffer-value-using-sv_depth.526406/
float LinearEyeDepthToOutDepth(float z)
{
	return(1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
}

// Returns the forward (Right) direction of the current view in the world space.
float3 GetViewRightDir()
{
	float4x4 viewMat = GetWorldToViewMatrix();
	return viewMat[0].xyz;
}

// ASE
float3 InvertDepthDirHD(float3 In)
{
	float3 result = In;
	#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301 || ASE_SRP_VERSION == 70503 || ASE_SRP_VERSION >= 80301
		result *= float3(1, 1, -1);
	#endif
	return result;
}

float4x4 unity_CameraProjection;
float4x4 unity_CameraInvProjection;
float4x4 unity_WorldToCamera;
float4x4 unity_CameraToWorld;

// ASE
float3 GetWorldPosFromDepthBuffer(float2 clipPos01, float cameraDepth)
{
	#ifdef UNITY_REVERSED_Z
		float depth = (1.0 - cameraDepth);
	#else
		float depth = cameraDepth;
	#endif
	float3 screenPos_DepthBuffer = (float3(clipPos01, depth));
	float4 clipPos = (float4((screenPos_DepthBuffer * 2.0 - 1.0), 1.0));
	float4 viewPos = mul(unity_CameraInvProjection, clipPos);
	float3 viewPosNorm = viewPos.xyz / viewPos.w;
	float3 localInvertDepthDirHD = InvertDepthDirHD(viewPosNorm);
	
	return mul(unity_CameraToWorld, float4(localInvertDepthDirHD, 1.0)).xyz;
}


#endif // JTRP_C
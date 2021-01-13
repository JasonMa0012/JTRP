#ifndef JTRP_COMMON
#define JTRP_COMMON

float2 GetWHRatio()
{
	return float2(_ScreenParams.y / _ScreenParams.x, 1);
}

float StepAntiAliasing(float x, float y)
{
	float v = x - y;
	return saturate(v / fwidth(v));//fwidth(x) = abs(ddx(x) + ddy(x))
}

// ----------------------------------------------------------------------------
// Transform
// ----------------------------------------------------------------------------

float2 RotateUV(float2 _uv, float _radian, float2 _piv, float _time)
{
	float RotateUV_ang = _radian;
	float RotateUV_cos = cos(_time * RotateUV_ang);
	float RotateUV_sin = sin(_time * RotateUV_ang);
	return(mul(_uv - _piv, float2x2(RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
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
    return (1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
}

// Returns the forward (Right) direction of the current view in the world space.
float3 GetViewRightDir()
{
    float4x4 viewMat = GetWorldToViewMatrix();
    return viewMat[0].xyz;
}


#endif // JTRP_COMMON
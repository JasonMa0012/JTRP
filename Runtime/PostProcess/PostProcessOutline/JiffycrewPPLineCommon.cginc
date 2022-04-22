
// functions begin
#define SUM4 4
#define SUM6 6
#define MULTIPLY_COLOR float4(1, 1, 1, 0.5)
#define SCENE_COLOR float4(0, 0, 0, 0)

struct _3x3UVDatas
{
	float2 LT;
	float2 T;
	float2 RT;
	float2 L;
	float2 C;
	float2 R;
	float2 LB;
	float2 B;
	float2 RB;
};

_3x3UVDatas PP_3x3UVs(float2 uv, float2 dist)
{
	_3x3UVDatas result;
	result.LT = (float2(-1, 1) * dist) + uv;
	result.T = (float2(0, 1) * dist) + uv;
	result.RT = (float2(1, 1) * dist) + uv;
	
	result.L = (float2(-1, 0) * dist) + uv;
	result.C = uv;
	result.R = (float2(1, 0) * dist) + uv;
	
	result.LB = (float2(-1, -1) * dist) + uv;
	result.B = (float2(0, -1) * dist) + uv;
	result.RB = (float2(1, -1) * dist) + uv;
	return result;
}

float WeightedSum6(float _w[SUM6], float _a[SUM6])
{
	float result = 0;
	for (int i = 0; i < SUM6; ++ i)
	result += _w[i] * _a[i];
	
	return result;
}

float WeightedSum4(float _w[SUM4], float _a[SUM4])
{
	float result = 0;
	for (int i = 0; i < SUM4; ++ i)
	result += _w[i] * _a[i];
	
	return result;
}

float3 GetColor(uint2 pixelCoords)
{
	return LOAD_TEXTURE2D_X_LOD(_JTRP_CameraColor, pixelCoords, 0).rgb;
}

float3 GetNormal(float2 uv)
{
	GBufferType1 inGBuffer1 = LOAD_TEXTURE2D_X(_GBufferTexture1, uv * _ScreenSize.xy);
	float2 octNormalWS = Unpack888ToFloat2(inGBuffer1.xyz);
	return UnpackNormalOctQuadEncode(octNormalWS * 2.0 - 1.0);
}

float GetDepth(float2 uv)
{
	return LOAD_TEXTURE2D_X_LOD(_JTRP_CameraDepth, uv * _ScreenSize.xy, 0).x;
}

float3 GetWorldPos(float2 uv, float deviceDepth)
{
	return ComputeWorldSpacePosition(uv, deviceDepth, UNITY_MATRIX_I_VP);
}

float3 GetWorldPos(float2 uv)
{
	return ComputeWorldSpacePosition(uv, GetDepth(uv), UNITY_MATRIX_I_VP);
}

float3 GetWorldView(float2 uv)
{
	return GetWorldSpaceNormalizeViewDir(GetWorldPos(uv));
}

float SilhouetteSobel(float lineWidth, float2 uv, float2 invSize)
{
	float2 dist = lineWidth * invSize;
	
	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float LT = GetDepth(uvDatas.LT);
	float T = GetDepth(uvDatas.T);
	float RT = GetDepth(uvDatas.RT);
	
	float L = GetDepth(uvDatas.L);
	float C = GetDepth(uvDatas.C);
	float R = GetDepth(uvDatas.R);
	
	float LB = GetDepth(uvDatas.LB);
	float B = GetDepth(uvDatas.B);
	float RB = GetDepth(uvDatas.RB);
	
	float w[SUM6] = {
		1, 2, 1, -1, -2, -1
	};
	float h[SUM6] = {
		LT, T, RT, LB, B, RB
	};
	float v[SUM6] = {
		RT, R, RB, LT, L, LB
	};
	
	float2 r;
	r.x = WeightedSum6(w, h);
	r.y = WeightedSum6(w, v);
	return length(r);
}

float SilhouetteSobelDepthAdaptive(sampler2D depthTexture, float lineWidth, float baseDepth, float2 uv, float2 invSize)
{
	float2 dist = lineWidth * invSize / GetDepth(uv) * baseDepth;
	dist = clamp(dist, invSize * 0.5, lineWidth * invSize);
	
	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float LT = GetDepth(uvDatas.LT);
	float T = GetDepth(uvDatas.T);
	float RT = GetDepth(uvDatas.RT);
	
	float L = GetDepth(uvDatas.L);
	float C = GetDepth(uvDatas.C);
	float R = GetDepth(uvDatas.R);
	
	float LB = GetDepth(uvDatas.LB);
	float B = GetDepth(uvDatas.B);
	float RB = GetDepth(uvDatas.RB);
	
	float w[SUM6] = {
		1, 2, 1, -1, -2, -1
	};
	float h[SUM6] = {
		LT, T, RT, LB, B, RB
	};
	float v[SUM6] = {
		RT, R, RB, LT, L, LB
	};
	
	float2 r;
	r.x = WeightedSum6(w, h);
	r.y = WeightedSum6(w, v);
	return length(r) / C;
}

float CreaseSobel(float lineWidth, float2 uv, float2 invSize)
{
	float2 dist = lineWidth * invSize;
	
	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float3 LT = GetNormal(uvDatas.LT);
	float3 T = GetNormal(uvDatas.T);
	float3 RT = GetNormal(uvDatas.RT);
	
	float3 L = GetNormal(uvDatas.L);
	float3 C = GetNormal(uvDatas.C);
	float3 R = GetNormal(uvDatas.R);
	
	float3 LB = GetNormal(uvDatas.LB);
	float3 B = GetNormal(uvDatas.B);
	float3 RB = GetNormal(uvDatas.RB);
	
	float w[SUM4] = {
		2, 2, 1, 1
	};
	float a[SUM4];
	a[0] = dot(R, L);
	a[1] = dot(T, B);
	a[2] = dot(LT, RB);
	a[3] = dot(RT, LB);
	
	return(1 - (WeightedSum4(w, a) / 6.0)) * 2;
}

float CreaseSobelDepthAdaptive(float lineWidth, float baseDepth, float2 uv, float2 invSize)
{
	float depth = GetDepth(uv);
	float2 dist = lineWidth * invSize / depth * baseDepth;
	dist = clamp(dist, invSize * 0.5, lineWidth * invSize);
	
	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float3 LT = GetNormal(uvDatas.LT);
	float3 T = GetNormal(uvDatas.T);
	float3 RT = GetNormal(uvDatas.RT);
	
	float3 L = GetNormal(uvDatas.L);
	float3 C = GetNormal(uvDatas.C);
	float3 R = GetNormal(uvDatas.R);
	
	float3 LB = GetNormal(uvDatas.LB);
	float3 B = GetNormal(uvDatas.B);
	float3 RB = GetNormal(uvDatas.RB);
	
	float w[SUM4] = {
		2, 2, 1, 1
	};
	float a[SUM4];
	a[0] = dot(R, L);
	a[1] = dot(T, B);
	a[2] = dot(LT, RB);
	a[3] = dot(RT, LB);
	
	return(1 - (WeightedSum4(w, a) / 6.0)) / (depth / baseDepth);
}

// functions end
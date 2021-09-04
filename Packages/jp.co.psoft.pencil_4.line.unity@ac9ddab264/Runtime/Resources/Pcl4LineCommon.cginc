#ifndef PCL4LINECOMMON_INCLUDE
#define PCL4LINECOMMON_INCLUDE

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

float2 TransformTriangleVertexToUV(float2 vertex)
{
	float2 uv = (vertex + 1.0) * 0.5;
	return uv;
}

sampler2D _MainTex;
sampler2D _LineTex;

float4 Eval(v2f i, float alpha)
{
#if UNITY_COLORSPACE_GAMMA
	float4 from = tex2D(_MainTex, i.uv);
	float4 lineColor = tex2D(_LineTex, i.uv) * alpha;
	return float4(from.rgb * (1.0 - lineColor.a) + lineColor.rgb, 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#else
	float4 from = tex2D(_MainTex, i.uv);
	float4 lineColor = tex2D(_LineTex, i.uv);

	if (lineColor.a == 0.0)
	{
		return from;
	}

	from.rgb = LinearToGammaSpace(from.rgb);

	lineColor.rgb /= lineColor.a;
	lineColor.rgb = LinearToGammaSpace(lineColor.rgb);
	lineColor.a *= alpha;

	return float4(GammaToLinearSpace(lerp(from.rgb, lineColor.rgb, lineColor.a)), 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#endif
}



#endif //PCL4LINECOMMON_INCLUDE
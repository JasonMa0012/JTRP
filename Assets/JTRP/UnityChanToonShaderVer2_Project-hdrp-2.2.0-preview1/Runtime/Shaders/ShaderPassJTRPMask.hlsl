#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

struct GraphVertexInput
{
	float4 vertex: POSITION;
	// float4 texcoord0: TEXCOORD0;
	// float3 normal: NORMAL;
	float4 color: COLOR;
};
struct GraphVertexOutput
{
	float4 position: POSITION;
	// half4 uv0: TEXCOORD0;
	// float3 positionWS: TEXCOORD1;
	// float3 normal: TEXCOORD2;
	float4 color: COLOR;
};


GraphVertexOutput vert(GraphVertexInput v)
{
	GraphVertexOutput o;
	float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
	o.position = TransformWorldToHClip(positionWS);
	// o.normal = TransformObjectToWorldNormal(v.normal);
	// o.uv0 = v.texcoord0;
	o.color = v.color;
	
	return o;
}

float frag(GraphVertexOutput i): SV_Target0
{
	return i.position.z;
}

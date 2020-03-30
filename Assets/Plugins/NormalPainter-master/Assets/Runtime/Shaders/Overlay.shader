Shader "Hidden/NormalPainter/Overlay" {

CGINCLUDE
#include "UnityCG.cginc"

StructuredBuffer<float3> _BaseNormals;
StructuredBuffer<float4> _BaseTangents;
StructuredBuffer<float3> _Points;
StructuredBuffer<float3> _Normals;
StructuredBuffer<float4> _Tangents;
StructuredBuffer<float> _Selection;



struct ia_out
{
    float4 vertex : POSITION;
    float4 normal : NORMAL;
    float4 uv : TEXCOORD0;
    float4 color : COLOR;
    uint vertexID : SV_VertexID;
    uint instanceID : SV_InstanceID;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 color : TEXCOORD0;
};


vs_out vert_local_space_normals_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color.rgb = v.normal.xyz * 0.5 + 0.5;
    o.color.a = 1.0;
    return o;
}

float3 ToBaseTangentSpace(uint vid, float3 n)
{
    float3 base_normal = _BaseNormals[vid];
    float4 base_tangent = _BaseTangents[vid];
    float3 base_binormal = normalize(cross(base_normal, base_tangent.xyz) * base_tangent.w);
    float3x3 tbn = float3x3(base_tangent.xyz, base_binormal, base_normal);
    return normalize(mul(n, transpose(tbn)));
}

vs_out vert_tangent_space_normals_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color.rgb = ToBaseTangentSpace(v.vertexID, v.normal.xyz) * 0.5 + 0.5;
    o.color.a = 1.0;
    return o;
}

vs_out vert_tangents_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    float4 tangent = _Tangents[v.vertexID];
    o.color.rgb = (tangent.xyz * tangent.w) * 0.5 + 0.5;
    o.color.a = 1.0;
    return o;
}

vs_out vert_binormals_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    float4 tangent = _Tangents[v.vertexID];
    float3 binormal = normalize(cross(v.normal.xyz, tangent.xyz * tangent.w));
    o.color.rgb = binormal * 0.5 + 0.5;
    o.color.a = 1.0;
    return o;
}

vs_out vert_uv_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = float4(v.uv.xy, 0.0, 1.0);
    return o;
}

vs_out vert_color_overlay(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = v.color;
    return o;
}


float4 frag(vs_out v) : SV_Target
{
    return v.color;
}

ENDCG

    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+99" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // pass 0: local space normals overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_local_space_normals_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 1: tangent space normals overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangent_space_normals_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 2: tangents overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangents_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    
        // pass 3: binormals overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_binormals_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 4: uv overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_uv_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 5: vertex color overlay
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_color_overlay
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    }
}

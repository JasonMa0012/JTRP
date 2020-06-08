Shader "Hidden/NormalPainter/Visualizer" {

CGINCLUDE
#include "UnityCG.cginc"

float _VertexSize;
float _NormalSize;
float _TangentSize;
float _BinormalSize;

float4 _VertexColor;
float4 _VertexColor2;
float4 _VertexColor3;
float4 _NormalColor;
float4 _TangentColor;
float4 _BinormalColor;
float4 _BrushPos;
float4 _Direction;
int _OnlySelected = 0;

float4x4 _Transform;
StructuredBuffer<float3> _Points;
StructuredBuffer<float3> _Normals;
StructuredBuffer<float4> _Tangents;
StructuredBuffer<float> _Selection;
sampler2D _BrushSamples;


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


vs_out vert_vertices(ia_out v)
{
    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;

    float s = _Selection[v.instanceID];
    float4 vertex = v.vertex;
    vertex.xyz *= _VertexSize;
    vertex.xyz *= abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z);
    vertex.xyz += pos;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    vs_out o;
    o.vertex = vertex;
    o.color = lerp(_VertexColor, _VertexColor2, s);
    float d = length(pos - _BrushPos.xyz) / _BrushPos.w;
    if (d < 1) {
        o.color.rgb += _VertexColor3.rgb * tex2Dlod(_BrushSamples, float4(1 - d, 0, 0, 0)).r;
    }
    return o;
}

vs_out vert_normals(ia_out v)
{
    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
    float3 dir = normalize((mul(_Transform, float4(_Normals[v.instanceID], 0.0))).xyz);

    float s = _OnlySelected ? _Selection[v.instanceID] : 1.0f;
    s *= abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z);
    float4 vertex = v.vertex;
    vertex.xyz += pos + dir * v.uv.x * _NormalSize * s;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    vs_out o;
    o.vertex = vertex;
    o.color = _NormalColor;
    o.color.a = 1.0 - v.uv.x;
    return o;
}

vs_out vert_tangents(ia_out v)
{
    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
    float4 tangent = _Tangents[v.instanceID];
    float3 dir = normalize((mul(_Transform, float4(tangent.xyz * tangent.w, 0.0))).xyz);

    float s = _OnlySelected ? _Selection[v.instanceID] : 1.0f;
    s *= abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z);
    float4 vertex = v.vertex;
    vertex.xyz += pos + dir * v.uv.x * _TangentSize * s;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    vs_out o;
    o.vertex = vertex;
    o.color = _TangentColor;
    o.color.a = 1.0 - v.uv.x;
    return o;
}

vs_out vert_binormals(ia_out v)
{
    float3 pos = (mul(_Transform, float4(_Points[v.instanceID], 1.0))).xyz;
    float4 tangent = _Tangents[v.instanceID];
    float3 binormal = normalize(cross(_Normals[v.instanceID], tangent.xyz * tangent.w));
    float3 dir = normalize((mul(_Transform, float4(binormal, 0.0))).xyz);

    float s = _OnlySelected ? _Selection[v.instanceID] : 1.0f;
    s *= abs(mul(UNITY_MATRIX_V, float4(pos, 1.0)).z);
    float4 vertex = v.vertex;
    vertex.xyz += pos + dir * v.uv.x * _BinormalSize * s;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    vs_out o;
    o.vertex = vertex;
    o.color = _BinormalColor;
    o.color.a = 1.0 - v.uv.x;
    return o;
}

vs_out vert_lasso(ia_out v)
{
    vs_out o;
    o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    o.vertex.y *= -1;
    o.color = float4(1.0, 0.0, 0.0, 1.0);
    return o;
}

vs_out vert_brush_range(ia_out v)
{
    vs_out o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.color = mul(UNITY_MATRIX_M, v.vertex);
    return o;
}

vs_out vert_ray_position(ia_out v)
{
    float z = abs(mul(UNITY_MATRIX_V, float4(_BrushPos.xyz, 1.0)).z);
    float3 pos = v.vertex.xyz * (0.01 * z) + _BrushPos.xyz;

    vs_out o;
    o.vertex = UnityObjectToClipPos(pos);
    o.color = float4(1, 0, 0, 1);
    return o;
}

vs_out vert_direction(ia_out v)
{
    float3 pos = _BrushPos.xyz;
    float3 dir = _Direction.xyz;
    float z = abs(mul(UNITY_MATRIX_V, float4(_BrushPos.xyz, 1.0)).z);
    float4 vertex = float4(pos + dir * (v.uv.x * 0.25 * z), 1.0);

    vs_out o;
    o.vertex = mul(UNITY_MATRIX_VP, vertex);
    o.color = float4(1, 0, 0, 1-v.uv.x);
    return o;
}


float4 frag(vs_out v) : SV_Target
{
    return v.color;
}

float4 frag_brush_range(vs_out v) : SV_Target
{
    float2 uv = v.color.xy / v.color.w;

    float3 pixel_pos = v.color.xyz;
    float3 brush_pos = _BrushPos.xyz;
    float distance = length(pixel_pos - brush_pos);

    float range = clamp(1.0f - distance / _BrushPos.w, 0, 1);
    float z = abs(mul(UNITY_MATRIX_V, float4(brush_pos, 1.0)).z);

    float border = 0.004 / _BrushPos.w * z;
    if (distance > _BrushPos.w || range > border) { discard; }

    float4 color = float4(1, 0, 0, 1);
    return color;
}

ENDCG

    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // pass 0: visualize vertices
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_vertices
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 1: visualize normals
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_normals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 2: visualize tangents
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_tangents
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 3: visualize binormals
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_binormals
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 4: lasso
        Pass
        {
            ZTest Always

            CGPROGRAM
            #pragma vertex vert_lasso
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 5: brush range
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_brush_range
            #pragma fragment frag_brush_range
            #pragma target 4.5
            ENDCG
        }

        // pass 6: ray position
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_ray_position
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }

        // pass 7: direction
        Pass
        {
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert_direction
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    }
}

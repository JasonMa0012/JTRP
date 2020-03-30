Shader "Hidden/NormalPainter/BrushShape"
{
CGINCLUDE
#include "UnityCG.cginc"


int _NumBrushSamples;
StructuredBuffer<float> _BrushSamples;

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 uv : TEXCOORD0;
};


v2f vert(appdata v)
{
    v2f o;
    o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    o.uv = float4(abs(v.vertex.x * 0.5 + 0.5), 1.0f - (v.vertex.y * 0.5f + 0.5f), 0, 0);
    return o;
}

float4 frag(v2f i) : SV_Target
{
    //StructuredBuffer::GetDimensions() seems not available on non-D3D11..
    int n = _NumBrushSamples;
    
    float u = 1.0 - abs(i.uv.x * 2.0 - 1.0);
    float v = _BrushSamples[(int)(u * (n-1))];

    float dy = 1.0 / (n-1) * 2.0;
    float c = 0.0;
    c += i.uv.y + dy * 2.0 < v ? 0.15 : 0.0; // 
    c += i.uv.y + dy * 1.0 < v ? 0.2  : 0.0; //
    c += i.uv.y < v ? 0.3 : 0.0;
    c += i.uv.y - dy * 1.0 < v ? 0.2  : 0.0; // 
    c += i.uv.y - dy * 2.0 < v ? 0.15 : 0.0; // antialiasing
    return float4(1,1,1,c);
}
ENDCG

    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+1" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            ENDCG
        }
    }
}

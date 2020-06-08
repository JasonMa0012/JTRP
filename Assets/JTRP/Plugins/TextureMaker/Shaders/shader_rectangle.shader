Shader "Unlit/Rectangle"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex ("Texture", 2D) = "white" {}
        _Width ("Width", Range(0.0, 1.0)) = 0.1
        _Height ("Height", Range(0.0, 1.0)) = 0.1
        _Pos ("Position", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "RenderQueue" = "Transparent" "RenderType"="Transparent" }
        BLEND SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float2 _Pos;
            float _Width;
            float _Height;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv = 1 - i.uv;
                i.uv -= _Pos;
                
                // Define the rectangle mask.
                float stepX = step(i.uv.x, _Width  * 0.5)  * step(0.5 * -_Width, i.uv.x);
                float stepY = step(i.uv.y, _Height * 0.5)  * step(0.5 * -_Height, i.uv.y);
                float mask = stepX * stepY;
                
                // Sample the texture.
                fixed4 col = tex2D(_MainTex, float2((i.uv.x + _Width * 0.5) / _Width, (i.uv.y + _Height * 0.5) / _Height));

                // Color based on the mask.
                col = col * mask * _Color;

                return col;
            }
            ENDCG
        }
    }
}

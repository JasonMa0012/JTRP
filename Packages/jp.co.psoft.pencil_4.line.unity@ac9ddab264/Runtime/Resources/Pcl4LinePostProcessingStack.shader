Shader "Hidden/Pcl4LinePostProcessingStack"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Pcl4LineCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = float4(v.vertex.xy, 0.0, 1.0);
				o.uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
				o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
				return o;
			}

			float _Alpha;

			half4 frag(v2f i) : SV_Target
			{
				return Eval(i, _Alpha);
			}
			ENDCG
		}
	}
}
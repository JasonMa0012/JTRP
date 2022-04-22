Shader "Hidden/GeometryOutline"
{
	Properties
	{
		_zOffset ("Z Offset", float) = 0
		_zTest ("Z Test", float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			Cull Back
			Offset [_zOffset], 0
			ZTest [_zTest]
			ZWrite Off
			// Blend SrcAlpha OneMinusSrcAlpha


			HLSLPROGRAM

			#pragma target 5.0
			#pragma require geometry
			#pragma enable_d3d11_debug_symbols

			#pragma vertex vertex_shader
			#pragma geometry geometry_shader
			#pragma fragment fragment_shader

			#define DEPTH_SCALE 3

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
			#define SHADERPASS SHADERPASS_DEFERRED_LIGHTING
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.jasonma.jtrp/Runtime/Shaders/Common/Common.hlsl"

			struct g2f
			{
				float4 position: SV_POSITION;
				float4 spinePos: TEXCOORD0; // 构成退化四边形的两端点中的一个，插值后为脊线
				float4 color: COLOR;
				float2 normal: TEXCOORD1;
				int is_edge: TEXCOORD2;
			};

			struct v2g
			{
				float4 vertex1: TEXCOORD0;
				float4 vertex1_color_width: TEXCOORD1; // xyz: color w:width
				float4 vertex2: TEXCOORD2;
				float4 vertex2_color_width: TEXCOORD3;
				int is_edge: TEXCOORD4;
				uint id: TEXCOORD5;
			};

			struct Line
			{
				int vertex1;
				int vertex2;
				int triangle1_vertex3;
				int triangle2_vertex3;
			};

			// Global
			uniform float _globalEdgeWidthScale = 1;
			uniform float _enable3XDepth = 0;

			// per object
			uniform float _edgeWidth = 0.01;
			uniform float _edgeWidthVC = 0.0;
			uniform float _normalBias;
			uniform float4 _edgeColor;
			// https://www.cnblogs.com/lht666/p/11447199.html
			uniform bool _enableBorderEdge;
			uniform bool _enableSilhouetteEdge;
			uniform bool _enableCreaseEdge;

			StructuredBuffer<float3> _vertices;
			StructuredBuffer<float3> _normals;
			StructuredBuffer<float4> _colors;
			StructuredBuffer<float2> _uvs;
			StructuredBuffer<Line> _degradedRectangles;

			TEXTURE2D_X(_Camera3XDepthTexture);
			SAMPLER(sampler_Camera3XDepthTexture);

			/* 基于线面求交的深度方案，由于效果不好、调试/改进困难弃用
			// https://rosettacode.org/wiki/Find_the_intersection_of_a_line_with_a_plane#C
			float3 LinePlaneIntersection(float3 rayPoint, float3 rayPoint1, float3 planePoint, float3 planePoint1, float3 planePoint2)
			{
				float3 rayVector = normalize(rayPoint1 - rayPoint);
				// float3 planeNormal = normalize(cross(planePoint1 - planePoint, planePoint2 - planePoint));
				float3 planeNormal = normalize(cross(planePoint1 - planePoint, planePoint2 - planePoint));
				
				float3 diff = rayPoint - planePoint;
				float prod1 = dot(diff, planeNormal);
				float prod2 = dot(rayVector, planeNormal);
				float prod3 = prod1 / (prod2 > 0 ? min(0.00001, prod2): prod2);
				return rayPoint - rayVector * prod3;
			}
			
			void GetOutlineVertexPosAndDepth(Line _line, float2 normalCS, float width1, float width2, inout float4 vertexCS1, inout float4 vertexCS2)
			{
				//TODO: 确保边界安全
				float4 opposite = TransformWorldToHClip(TransformObjectToWorld(_vertices[_line.triangle1_vertex3]));
				opposite /= abs(opposite.w);
				float2 n = opposite.xy - ((vertexCS1 + vertexCS2) * 0.5).xy;
				float z1 = 0, z2 = 0;
				bool hasSameSide = false;
				
				float3 planePoint = opposite.xyz, planePoint1 = vertexCS1.xyz, planePoint2 = vertexCS2.xyz;
				vertexCS1.xy += normalCS * width1;
				vertexCS2.xy += normalCS * width2;
				
				// 如果第三点和推出方向在同一边则在三角平面上对直线求交以得出深度
				if (dot(normalize(n), normalCS) > 0)
				{
					hasSameSide = true;
					z1 = LinePlaneIntersection(vertexCS1.xyz, float3(vertexCS1.xy, 1), planePoint, planePoint1, planePoint2).z;
					z2 = LinePlaneIntersection(vertexCS2.xyz, float3(vertexCS2.xy, 1), planePoint, planePoint1, planePoint2).z;
				}
				
				if(_line.triangle2_vertex3 > 0)
				{
					opposite = TransformWorldToHClip(TransformObjectToWorld(_vertices[_line.triangle2_vertex3]));
					opposite /= abs(opposite.w);
					n = opposite.xy - ((vertexCS1 + vertexCS2) * 0.5).xy;
					if(dot(normalize(n), normalCS) > 0)
					{
						hasSameSide = true;
						planePoint = opposite.xyz;
						z1 = max(z1, LinePlaneIntersection(vertexCS1.xyz, float3(vertexCS1.xy, 1), planePoint, planePoint1, planePoint2).z);
						z2 = max(z2, LinePlaneIntersection(vertexCS2.xyz, float3(vertexCS2.xy, 1), planePoint, planePoint1, planePoint2).z);
					}
				}
				
				if(hasSameSide)
				{
					vertexCS1.z = z1;
					vertexCS2.z = z2;
				}
			}*/

			float4 ClipPos2ScreenPos(float4 clipPos, float2 scale = 1.0)
			{
				float4 spineScreenPos = float4(clipPos.xy * 0.5 + 0.5, clipPos.zw);
				#if UNITY_UV_STARTS_AT_TOP
					spineScreenPos.y = 1 - spineScreenPos.y;
				#endif
				spineScreenPos.xy = spineScreenPos.xy * _ScreenParams.xy * scale;
				return spineScreenPos;
			}

			float LoadCamera3XDepth(float2 uv, bool isSS = true)
			{
				UNITY_BRANCH
				if (isSS)
					return LOAD_TEXTURE2D_X_LOD(_Camera3XDepthTexture, uv/* * _RTHandleScale.xy*/, 0).r;
				else
					return SAMPLE_TEXTURE2D_X_LOD(_Camera3XDepthTexture, s_linear_clamp_sampler, uv * _RTHandleScale.xy, 0).r;
			}


			v2g vertex_shader(uint id: SV_VertexID, uint inst: SV_InstanceID)
			{
				v2g o = (v2g)0;
				//获取退化四边形数据，并得到实际的顶点数据。
				o.id = id;
				Line _line = _degradedRectangles[id];
				float4 vertex1 = float4(_vertices[_line.vertex1], 1.0f);
				float4 vertex2 = float4(_vertices[_line.vertex2], 1.0f);
				float3 vertex1_normal = _normals[_line.vertex1];
				float3 vertex2_normal = _normals[_line.vertex2];
				float4 vertex1_color = _colors[_line.vertex1];
				float4 vertex2_color = _colors[_line.vertex2];
				float4 triangle1_vertex3 = float4(_vertices[_line.triangle1_vertex3], 1.0f);
				float4 center_point = (vertex1 + vertex2 + triangle1_vertex3) / 3.0f;

				float3 worldPos1 = TransformObjectToWorld(vertex1.xyz + vertex1_normal * 0.0001 * _normalBias);
				float3 worldPos2 = TransformObjectToWorld(vertex2.xyz + vertex2_normal * 0.0001 * _normalBias);
				o.vertex1 = TransformWorldToHClip(worldPos1);
				o.vertex2 = TransformWorldToHClip(worldPos2);

				// 由于存在Camera-relative rendering，必须小心处理空间转换，_WorldSpaceCameraPos不是relative world pos所以用GetRawUnityWorldToObject()
				// https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@8.2/manual/Camera-Relative-Rendering.html
				float3 view_dir = normalize(
					mul(GetRawUnityWorldToObject(), float4(_WorldSpaceCameraPos, 1)).xyz - center_point.xyz);

					bool is_edge = _enableBorderEdge;
					if (_line.triangle2_vertex3 > 0)
				{
					// 非边界边
					float4 triangle2_vertex3 = float4(_vertices[_line.triangle2_vertex3], 1.0f);

					float3 v1 = normalize(vertex2.xyz - vertex1.xyz);
					float3 v2 = normalize(triangle1_vertex3.xyz - vertex1.xyz);
					float3 v3 = normalize(triangle2_vertex3.xyz - vertex1.xyz);

					float3 face1Normal = normalize(cross(v1, v2));
					float3 face2Normal = normalize(cross(v3, v1));

					bool is_outline = !step(0, (dot(face1Normal, view_dir)) * (dot(face2Normal, view_dir)));
					bool is_crease = step(pow(dot(face1Normal, face2Normal) / cos(1.0472f), 2),
					dot(face1Normal, face1Normal) * dot(face2Normal, face2Normal));

					is_edge = (is_outline & _enableSilhouetteEdge) | (is_crease & _enableCreaseEdge);
				}

				o.vertex1_color_width = float4(vertex1_color.rgb, lerp(1, vertex1_color.g, _edgeWidthVC));
				o.vertex2_color_width = float4(vertex2_color.rgb, lerp(1, vertex2_color.g, _edgeWidthVC));
				o.is_edge = (int)is_edge;

				return o;
			}

			/*
			使用"TriangleStream"画2个三角形, 来组成1个四边形模拟线条粗细。
			use "TriangleStream" to draw two triangle to make a quad to control the width of line.
			*/
			[maxvertexcount(12)]
			void geometry_shader(uint pid: SV_PrimitiveID, point v2g input[1], inout TriangleStream < g2f > stream)
			{
				// 使用几何着色器把退化四边形进化成线条
				// https://zhuanlan.zhihu.com/p/61077186
				g2f o = (g2f)0;
				o.is_edge = input[0].is_edge > 0;

				Line _line = _degradedRectangles[input[0].id];
				float PctExtend = 0.1; // 线段两端延伸的长度

				// 顶点在摄像机和近裁面间时w为负，abs防止此问题
				float4 e1 = input[0].vertex1 / abs(input[0].vertex1.w);
				float4 e2 = input[0].vertex2 / abs(input[0].vertex2.w);

				float2 ext = PctExtend * (e2.xy - e1.xy);
				float2 v = normalize(float3(e2.xy - e1.xy, 0)).xy;
				//TODO: 修复宽高比导致的粗细不一
				// float2 n = float2(-v.y, v.x);
				// float w = 0.5 * _edgeWidth * _globalEdgeWidthScale;
				// float w1 = w * input[0].vertex1_color_width.a;
				// float w2 = w * input[0].vertex2_color_width.a;

				// float4 v0 = e1, v1 = e1, v2 = e2, v3 = e2;
				// GetOutlineVertexPosAndDepth(_line, n, w1, w2, v0, v2);
				// GetOutlineVertexPosAndDepth(_line, -n, w1, w2, v1, v3);


				o.normal = float2(-v.y, v.x);
				float2 n = o.normal * 0.5 * _edgeWidth * _globalEdgeWidthScale;
				float2 n1 = n * input[0].vertex1_color_width.a;
				float2 n2 = n * input[0].vertex2_color_width.a;

				float4 v0 = float4(e1.xy + n1 - ext, e1.zw);
				float4 v1 = float4(e1.xy - n1 - ext, e1.zw);
				float4 v2 = float4(e2.xy + n2 + ext, e2.zw);
				float4 v3 = float4(e2.xy - n2 + ext, e2.zw);

				float4 c01 = float4(input[0].vertex1_color_width.rgb, 1);
				float4 c23 = float4(input[0].vertex2_color_width.rgb, 1);

				// bool ztest = e1.z >= LoadCameraDepth(ClipPos2ScreenPos(e1).xy);
				// ztest = ztest && e2.z >= LoadCameraDepth(ClipPos2ScreenPos(e2).xy);
				// ztest = ztest && v0.z >= LoadCameraDepth(ClipPos2ScreenPos(v0).xy);
				// ztest = ztest && v1.z >= LoadCameraDepth(ClipPos2ScreenPos(v1).xy);
				// ztest = ztest && v2.z >= LoadCameraDepth(ClipPos2ScreenPos(v2).xy);
				// ztest = ztest && v3.z >= LoadCameraDepth(ClipPos2ScreenPos(v3).xy);
				// o.is_edge *= ztest;
				{
					o.position = v0;
					// o.spinePos = e1;
					// o.color = c01;
					stream.Append(o);
					o.position = v1;
					// o.spinePos = e2;
					// o.color = c23;
					stream.Append(o);
					o.position = v2;
					stream.Append(o);
					stream.RestartStrip();

					o.position = v1;
					// o.spinePos = e1;
					// o.color = c01;
					stream.Append(o);
					o.position = v3;
					stream.Append(o);
					o.position = v2;
					// o.spinePos = e2;
					// o.color = c23;
					stream.Append(o);
					stream.RestartStrip();

					// o.position = e1;
					// o.spinePos = e1;
					// o.color = c01;
					// stream.Append(o);
					// o.position = v3;
					// o.spinePos = e2;
					// o.color = c23;
					// stream.Append(o);
					// o.position = e2;
					// stream.Append(o);
					// stream.RestartStrip();

					// o.position = e1;
					// o.spinePos = e1;
					// o.color = c01;
					// stream.Append(o);
					// o.position = v1;
					// stream.Append(o);
					// o.position = v3;
					// o.spinePos = e2;
					// o.color = c23;
					// stream.Append(o);
					// stream.RestartStrip();
				}
			}

			/*
			[maxvertexcount(2)]
			void geometry_shader(
				uint pid: SV_PrimitiveID,
				point v2g input[1],
				inout LineStream < g2f > stream
			)
			{
				// 使用几何着色器把退化四边形进化成线条
				// 直接使用stream.RestartStrip();即可，如有更好的方法请自行实现。
				g2f o = (g2f)0;
				o.is_edge = input[0].is_edge;
				float4 c0 = float4(input[0].vertex1_color_width.rgb, 1);
				float4 c1 = float4(input[0].vertex2_color_width.rgb, 1);
				
				o.color = c0;
				o.position = input[0].vertex1;
				stream.Append(o);
				o.color = c1;
				o.position = input[0].vertex2;
				stream.Append(o);
				stream.RestartStrip();
			}*/

			float4 fragment_shader(g2f i): SV_Target
			{
				clip(i.is_edge - 1);
				return LinearEyeDepth(i.position.z);
				float4 outColor = i.color * _edgeColor;

				float4 spineScreenPos = ClipPos2ScreenPos(i.spinePos / i.spinePos.w, DEPTH_SCALE);

				i.spinePos /= i.spinePos.w;
				float2 spineUV = i.spinePos * 0.5 + 0.5;

				#if UNITY_UV_STARTS_AT_TOP
					i.normal = normalize(float2(i.normal.x, -i.normal.y));
					spineUV.y = 1.0 - spineUV.y;
				#endif

				if (_enable3XDepth)
				{
					float visibility = step(LoadCamera3XDepth(spineUV, false), i.spinePos.z);
					outColor.a *= visibility;

					/*
					const float2 pixelSize = _ScreenParams.zw - 1.0;
					const float2 depthTexelSize = pixelSize / DEPTH_SCALE * 0.1;
					float2 uv = (i.position.xy + 0.5) * pixelSize;


					const uint SAMPLE_COUNT = 9;
					int2 offset[SAMPLE_COUNT] = {
						int2(0, 0),
						int2(0, 1),
						int2(0, -1),
						int2(1, 0),
						int2(1, 1),
						int2(1, -1),
						int2(-1, 0),
						int2(-1, 1),
						int2(-1, -1)
					};

					int diff = SAMPLE_COUNT;
					for (int index = 0; index < SAMPLE_COUNT; index ++)
					{
						float depth = LoadCamera3XDepth(spineUV + offset[index] * depthTexelSize);
						// float depth = LoadCamera3XDepth((spineScreenPos + 0.5) * pixelSize + offset[index] * depthTexelSize);
						diff -= step(i.spinePos.z + 0.00000, depth);
					}
					// clip(diff - 1);
					outColor.a = diff / SAMPLE_COUNT;
					// outColor.rgb = LoadCamera3XDepth(i.spinePos).xxx;
					// outColor.rgb = float3((spineScreenPos.xy + 0.5) * pixelSize, 0);*/
				}
				else
				{
					float depth = LoadCameraDepth(i.position.xy);
					clip(i.position.z - depth + 0.0);
				}
                
				return outColor;
			}
			ENDHLSL

		}
	}
}
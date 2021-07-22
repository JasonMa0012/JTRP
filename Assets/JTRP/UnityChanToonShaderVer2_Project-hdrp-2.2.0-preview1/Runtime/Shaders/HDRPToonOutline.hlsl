﻿//Unitychan Toon Shader ver.8.0
//v.8.0.0
//nobuyuki@unity3d.com
//toshiyuki@unity3d.com (Universal RP/HDRP)
//https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
//(C)Unity Technologies Japan/UCL
// 2018/08/23 N.Kobayashi (Unity Technologies Japan)
// カメラオフセット付きアウトライン（BaseColorライトカラー反映修正版）
// 2017/06/05 PS4対応版
// Ver.2.0.4.3
// 2018/02/05 Outline Tex対応版
// #pragma multi_compile _IS_OUTLINE_CLIPPING_NO _IS_OUTLINE_CLIPPING_YES 
// _IS_OUTLINE_CLIPPING_YESは、Clippigマスクを使用するシェーダーでのみ使用できる. OutlineのブレンドモードにBlend SrcAlpha OneMinusSrcAlphaを追加すること.
//
            uniform float4 _LightColor0;
//            uniform float4 _BaseColor;
            //v.2.0.7.5
            uniform float _Unlit_Intensity;
            uniform fixed _Is_Filter_LightColor;
            uniform fixed _Is_LightColor_Outline;
            //v.2.0.5
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			TEXTURE2D(_Outline_Width_Ramp);
            uniform float _Outline_Width;
            uniform float _Outline_Ramp_Max_Distance;
            uniform float _Farthest_Distance;
            uniform float _Nearest_Distance;
            uniform sampler2D _Outline_Sampler; uniform float4 _Outline_Sampler_ST;
            uniform float4 _Outline_Color;
            uniform fixed _Is_BlendBaseColor;
            uniform float _Offset_Z;
            //v2.0.4
            uniform sampler2D _OutlineTex; uniform float4 _OutlineTex_ST;
            uniform fixed _Is_OutlineTex;
            //Baked Normal Texture for Outline
            uniform sampler2D _BakedNormal; uniform float4 _BakedNormal_ST;
            uniform fixed _Is_BakedNormal;

            uniform float _ZOverDrawMode;
            //

            uniform float _OutlineVisible;
            uniform float _OutlineOverridden;
            uniform float4 _OutlineMaskColor;
            uniform float _ComposerMaskMode;
//v.2.0.4
#ifdef _IS_OUTLINE_CLIPPING_YES
            uniform sampler2D _ClippingMask; uniform float4 _ClippingMask_ST;
            uniform float _Clipping_Level;
            uniform fixed _Inverse_Clipping;
            uniform fixed _IsBaseMapAlphaAsClippingMask;
#endif
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 bakedNormal:TEXCOORD7;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 bitangentDir : TEXCOORD3;
            };

            float3 GetSmoothedWorldNormal(float2 bakedNormal, float3x3 tbn)
            {
                float3 normal = float3(bakedNormal, 0);
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                return mul(normal, tbn);
            }

#undef unity_ObjectToWorld 
#undef unity_WorldToObject 

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                float2 Set_UV0 = o.uv0;
                float4 _Outline_Sampler_var = tex2Dlod(_Outline_Sampler,float4(TRANSFORM_TEX(Set_UV0, _Outline_Sampler),0.0,0));
                //v.2.0.4.3 baked Normal Texture for Outline
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float3x3 tangentTransform = float3x3( o.tangentDir, o.bitangentDir, o.normalDir);
                //UnpackNormal()が使えないので、以下で展開。使うテクスチャはBump指定をしないこと.
                float4 _BakedNormal_var = (tex2Dlod(_BakedNormal,float4(TRANSFORM_TEX(Set_UV0, _BakedNormal),0.0,0)) * 2 - 1);
                //float3 _BakedNormalDir = normalize(mul(_BakedNormal_var.rgb, tangentTransform));
                float3 _BakedNormalDir = GetSmoothedWorldNormal(v.bakedNormal, tangentTransform);

                //ここまで.
				float viewDistance = distance(objPos.rgb,_WorldSpaceCameraPos);
				float outlineWidthRamp = SampleRampSignalLine(_Outline_Width_Ramp, viewDistance / _Outline_Ramp_Max_Distance).r;
                float Set_Outline_Width = (_Outline_Width * 0.001 * outlineWidthRamp * smoothstep( _Farthest_Distance, _Nearest_Distance, viewDistance )*_Outline_Sampler_var.rgb).r;
                Set_Outline_Width *= (1.0f - _ZOverDrawMode);
                //v.2.0.7.5
                float4 _ClipCameraPos = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos.xyz, 1));
                //v.2.0.7
                #if defined(UNITY_REVERSED_Z)
                    //v.2.0.4.2 (DX)
                    _Offset_Z = _Offset_Z * -0.01;
                #else
                    //OpenGL
                    _Offset_Z = _Offset_Z * 0.01;
                #endif
//v2.0.4
#ifdef _OUTLINE_NML
                //v.2.0.4.3 baked Normal Texture for Outline
                o.pos = UnityObjectToClipPos(lerp(float4(v.vertex.xyz + v.normal*Set_Outline_Width,1), float4(v.vertex.xyz + _BakedNormalDir*Set_Outline_Width,1),_Is_BakedNormal));
#elif _OUTLINE_POS
                Set_Outline_Width = Set_Outline_Width*2;
                float signVar = dot(normalize(v.vertex),normalize(v.normal))<0 ? -1 : 1;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + signVar*normalize(v.vertex)*Set_Outline_Width, 1));
#endif
                //v.2.0.7.5
                o.pos.z = o.pos.z + _Offset_Z * _ClipCameraPos.z;

                AntiPerspective(o.pos);

                return o;
            }
            float4 frag(VertexOutput i) : SV_Target{
                //v.2.0.5
                if (_ZOverDrawMode > 0.99f)
                {
                    return float4(1.0f, 1.0f, 1.0f, 1.0f);  // but nothing should be drawn except Z value as colormask is set to 0
                }
                _Color = _BaseColor;
				float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
					//v.2.0.7.5
				float4 unity_AmbientSky = float4(0.1, 0.1, 0.1, 1.0f); //Todo.
                half3 ambientSkyColor = unity_AmbientSky.rgb>0.05 ? unity_AmbientSky.rgb*_Unlit_Intensity : half3(0.05,0.05,0.05)*_Unlit_Intensity;
                float3 lightColor = _LightColor0.rgb >0.05 ? _LightColor0.rgb : ambientSkyColor.rgb;
                float lightColorIntensity = (0.299*lightColor.r + 0.587*lightColor.g + 0.114*lightColor.b);
                lightColor = lightColorIntensity<1 ? lightColor : lightColor/lightColorIntensity;
                lightColor = lerp(half3(1.0,1.0,1.0), lightColor, _Is_LightColor_Outline);
                float2 Set_UV0 = i.uv0;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(Set_UV0, _MainTex));
                float3 Set_BaseColor = _BaseColor.rgb*_MainTex_var.rgb;
                float3 _Is_BlendBaseColor_var = lerp( _Outline_Color.rgb*lightColor, (_Outline_Color.rgb*Set_BaseColor*Set_BaseColor*lightColor), _Is_BlendBaseColor );
                //
                float3 _OutlineTex_var = tex2D(_OutlineTex,TRANSFORM_TEX(Set_UV0, _OutlineTex));

                float4 overridingColor = lerp(_OutlineMaskColor, float4(_OutlineMaskColor.w, 0.0f, 0.0f, 1.0f), _ComposerMaskMode);
                float  maskEnabled = max(_OutlineOverridden, _ComposerMaskMode);

//v.2.0.7.5
#ifdef _IS_OUTLINE_CLIPPING_NO
                float3 Set_Outline_Color = lerp(_Is_BlendBaseColor_var, _OutlineTex_var.rgb*_Outline_Color.rgb*lightColor, _Is_OutlineTex );
                if (_OutlineVisible < 0.1)
                {
                    // Todo. 
                    // without this, something is drawn even if _OutlineVisible = 0, in AngelRing(HDRP)
                    discard; 
                }
                Set_Outline_Color = lerp(Set_Outline_Color, overridingColor.xyz, maskEnabled);
                return float4(Set_Outline_Color, _OutlineVisible );

#elif _IS_OUTLINE_CLIPPING_YES
                float4 _ClippingMask_var = tex2D(_ClippingMask,TRANSFORM_TEX(Set_UV0, _ClippingMask));
                float Set_MainTexAlpha = _MainTex_var.a;
                float _IsBaseMapAlphaAsClippingMask_var = lerp( _ClippingMask_var.r, Set_MainTexAlpha, _IsBaseMapAlphaAsClippingMask );
                float _Inverse_Clipping_var = lerp( _IsBaseMapAlphaAsClippingMask_var, (1.0 - _IsBaseMapAlphaAsClippingMask_var), _Inverse_Clipping );
                float Set_Clipping = saturate((_Inverse_Clipping_var+_Clipping_Level));
                clip(Set_Clipping - 0.5);
                float4 Set_Outline_Color = lerp( float4(_Is_BlendBaseColor_var,Set_Clipping), float4((_OutlineTex_var.rgb*_Outline_Color.rgb*lightColor),Set_Clipping), _Is_OutlineTex );
                Set_Outline_Color = lerp(Set_Outline_Color, overridingColor, maskEnabled);
                Set_Outline_Color.w *= _OutlineVisible;
                return Set_Outline_Color;
#endif
            }
// UCTS_Outline.cginc ここまで.

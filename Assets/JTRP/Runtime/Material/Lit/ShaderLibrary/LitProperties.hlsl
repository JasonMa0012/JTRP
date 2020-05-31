
TEXTURE2D(_BaseColorMap);
SAMPLER(sampler_BaseColorMap);

TEXTURE2D(_HeightMap);
SAMPLER(sampler_HeightMap);


TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
TEXTURE2D(_MatCapNormalMap);
SAMPLER(sampler_MatCapNormalMap);
TEXTURE2D(_ShadowMap);
TEXTURE2D(_ShadowColorMap);
SAMPLER(sampler_ShadowMap);
TEXTURE2D(_LightMap);
SAMPLER(sampler_LightMap);
TEXTURE2D(_MatCap_Sampler);
SAMPLER(sampler_MatCap_Sampler);
TEXTURE2D(_NoiseMap);
SAMPLER(sampler_NoiseMap);
TEXTURE2D(_Emissive_Mask1);
SAMPLER(sampler_Emissive_Mask1);
TEXTURE2D(_Emissive_Mask2);
SAMPLER(sampler_Emissive_Mask2);



CBUFFER_START(UnityPerMaterial)


float3 _EmissiveColor;
float _AlbedoAffectEmissive;
float _EmissiveExposureWeight;

// Set of users variables
float4 _BaseColor;
float4 _BaseColorMap_ST;
// float4 _BaseColorMap_TexelSize;
// float4 _BaseColorMap_MipInfo;

float _Metallic;
float _Smoothness;
float4 _DetailMap_ST;
float _Anisotropy;
float _DiffusionProfileHash;
float _SubsurfaceMask;
float _Thickness;
float4 _SpecularColor;
float _TexWorldScale;
float4 _UVMappingMask;
float4 _UVDetailsMappingMask;
float _LinkDetailsWithBase;
// Following two variables are feeded by the C++ Editor for Scene selection
int _ObjectId;
int _PassValue;



uniform float4 _Color;
uniform float4 _AddColor;
uniform float _Purity;
uniform float _AddColorIntensity;

uniform float _LightColorBlend;
uniform float _SkyColorIntensity;
float _PointLightColorIntensity;
uniform float _PointLightStep;
uniform float _PointLightFeather;
uniform float _LightColorIntensity;

uniform float _NormalScale;
uniform float _DiffuseNormalBlend;

uniform float _ShadowIntensity;
uniform float3 _ShadowFixedColor;
uniform float3 _ShadowMapColor;
uniform float _Shadow_Step;
uniform float _Shadow_Feather;
uniform float _Shadow_Purity;
uniform float4 _ShadowColor2;
uniform float _ShadowIntensity2;
uniform float _ShadowColorBlend2;
uniform float _Shadow_Step2;
uniform float _Shadow_Feather2;

uniform float _HighColorLevel;
uniform float _roughness;
uniform float4 _HighColor1;
float _HighColorInt1;
float _HighColorPointInt1;
float _HighLightStep1;
float _HighLightFeather1;
float _HighColorIntOnShadow1;
uniform float4 _HighColor2;
float _HighColorInt2;
float _HighColorPointInt2;
float _HighLightPower2;
float _HighColorIntOnShadow2;

uniform float4 _MatCap_Sampler_ST;
uniform float4 _MatCapNormalMap_ST;
uniform float4 _MatCapColor;
uniform float _MatCap;
uniform float _MatCap_Enable;
uniform float _TweakMatCapOnShadow;
uniform float _Tweak_MatcapMaskLevel;
uniform float _BumpScaleMatcap;
uniform float _BlurLevelMatcap;

uniform float _RimLight_Enable;
uniform uint _RimLight_Mode;
uniform float4 _RimLightColor;
uniform float _RimLightIntensity;
uniform float _RimLightBlend;
uniform float _RimLightBlendPoint;
uniform float4 _RimLightColor2;
uniform float _RimLightIntensity2;
uniform float _RimLightBlend2;
uniform float _RimLightBlendPoint2;
uniform float _RimLightFeather;
uniform float _RimLightLength;
uniform float _RimLightWidth;
uniform float _RimLightIntInShadow;
uniform float _RimLightSNBlend;
uniform float4 _RimLightColor3;
uniform float _RimLightBlend3;
uniform float _RimLightIntensity3;
uniform float _RimLightFeather3;
uniform float _RimLightLength3;
uniform float _RimLightWidth3;
uniform float _RimLightLevel;

uniform float _Emissive_Enable;
uniform float4 _Emissive_Mask1_ST;
uniform float4 _Emissive_ColorA1;
uniform float4 _Emissive_ColorB1;
uniform float _Emissive_IntA1;
uniform float _Emissive_IntB1;
uniform float _Emissive_Level1;
uniform float _Emissive_MoveHor1;
uniform float _Emissive_MoveVer1;
uniform float _Emissive_Speed1;
uniform float4 _Emissive_Mask2_ST;
uniform float4 _Emissive_ColorA2;
uniform float4 _Emissive_ColorB2;
uniform float _Emissive_IntA2;
uniform float _Emissive_IntB2;
uniform float _Emissive_Level2;
uniform float _Emissive_MoveHor2;
uniform float _Emissive_MoveVer2;
uniform float _Emissive_Speed2;

uniform float _Outline_Width;
uniform float4 _Outline_Color;
uniform float _Offset_Z;
uniform float _Outline_Lightness;
uniform float _Outline_Purity;
uniform float _Outline_Blend;


CBUFFER_END


// TEXTURE2D(_DistortionVectorMap);
// SAMPLER(sampler_DistortionVectorMap);

// TEXTURE2D(_EmissiveColorMap);
// SAMPLER(sampler_EmissiveColorMap);

// TEXTURE2D(_DiffuseLightingMap);
// SAMPLER(sampler_DiffuseLightingMap);

TEXTURE2D(_BaseColorMap);
SAMPLER(sampler_BaseColorMap);

// TEXTURE2D(_MaskMap);
// SAMPLER(sampler_MaskMap);
// TEXTURE2D(_BentNormalMap); // Reuse sampler from normal map
// SAMPLER(sampler_BentNormalMap);

// TEXTURE2D(_NormalMap);
// SAMPLER(sampler_NormalMap);
// TEXTURE2D(_NormalMapOS);
// SAMPLER(sampler_NormalMapOS);

// TEXTURE2D(_DetailMap);
// SAMPLER(sampler_DetailMap);

TEXTURE2D(_HeightMap);
SAMPLER(sampler_HeightMap);

// TEXTURE2D(_TangentMap);
// SAMPLER(sampler_TangentMap);
// TEXTURE2D(_TangentMapOS);
// SAMPLER(sampler_TangentMapOS);

// TEXTURE2D(_AnisotropyMap);
// SAMPLER(sampler_AnisotropyMap);

// TEXTURE2D(_SubsurfaceMaskMap);
// SAMPLER(sampler_SubsurfaceMaskMap);
// TEXTURE2D(_ThicknessMap);
// SAMPLER(sampler_ThicknessMap);

// TEXTURE2D(_IridescenceThicknessMap);
// SAMPLER(sampler_IridescenceThicknessMap);

// TEXTURE2D(_IridescenceMaskMap);
// SAMPLER(sampler_IridescenceMaskMap);

// TEXTURE2D(_SpecularColorMap);
// SAMPLER(sampler_SpecularColorMap);

// TEXTURE2D(_TransmittanceColorMap);
// SAMPLER(sampler_TransmittanceColorMap);

// TEXTURE2D(_CoatMaskMap);
// SAMPLER(sampler_CoatMaskMap);



TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
TEXTURE2D(_MatCapNormalMap);
SAMPLER(sampler_MatCapNormalMap);
TEXTURE2D(_ShadowMap);
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

// shared constant between lit and layered lit
// float _AlphaCutoff;
// float _UseShadowThreshold;
// float _AlphaCutoffShadow;
// float _AlphaCutoffPrepass;
// float _AlphaCutoffPostpass;
// float4 _DoubleSidedConstants;
// float _DistortionScale;
// float _DistortionVectorScale;
// float _DistortionVectorBias;
// float _DistortionBlurScale;
// float _DistortionBlurRemapMin;
// float _DistortionBlurRemapMax;

// float _PPDMaxSamples;
// float _PPDMinSamples;
// float _PPDLodThreshold;

float3 _EmissiveColor;
float _AlbedoAffectEmissive;
float _EmissiveExposureWeight;

// float _EnableSpecularOcclusion;

// Transparency
// float3 _TransmittanceColor;
// float _Ior;
// float _ATDistance;
// float _ThicknessMultiplier;

// Caution: C# code in BaseLitUI.cs call LightmapEmissionFlagsProperty() which assume that there is an existing "_EmissionColor"
// value that exist to identify if the GI emission need to be enabled.
// In our case we don't use such a mechanism but need to keep the code quiet. We declare the value and always enable it.
// TODO: Fix the code in legacy unity so we can customize the beahvior for GI
// float3 _EmissionColor;
// float4 _EmissiveColorMap_ST;
// float _TexWorldScaleEmissive;
// float4 _UVMappingMaskEmissive;

// float4 _InvPrimScale; // Only XY are used

// // Wind
// float _InitialBend;
// float _Stiffness;
// float _Drag;
// float _ShiverDrag;
// float _ShiverDirectionality;

// // Specular AA
// float _EnableGeometricSpecularAA;
// float _SpecularAAScreenSpaceVariance;
// float _SpecularAAThreshold;


// Set of users variables
float4 _BaseColor;
float4 _BaseColorMap_ST;
// float4 _BaseColorMap_TexelSize;
// float4 _BaseColorMap_MipInfo;

float _Metallic;
float _Smoothness;
// float _SmoothnessRemapMin;
// float _SmoothnessRemapMax;
// float _AORemapMin;
// float _AORemapMax;

// float _NormalScale;

float4 _DetailMap_ST;
// float _DetailAlbedoScale;
// float _DetailNormalScale;
// float _DetailSmoothnessScale;

// float4 _HeightMkap_TexelSize; // Unity facility. This will provide the size of the heightmap to the shader

// float _HeightAmplitude;
// float _HeightCenter;

float _Anisotropy;

float _DiffusionProfileHash;
float _SubsurfaceMask;
float _Thickness;
// float4 _ThicknessRemap;


// float _IridescenceThickness;
// float4 _IridescenceThicknessRemap;
// float _IridescenceMask;

// float _CoatMask;

float4 _SpecularColor;
// float _EnergyConservingSpecularColor;

float _TexWorldScale;
// float _InvTilingScale;
float4 _UVMappingMask;
float4 _UVDetailsMappingMask;
float _LinkDetailsWithBase;



// Following two variables are feeded by the C++ Editor for Scene selection
int _ObjectId;
int _PassValue;



uniform float4 _Color;
uniform float4 _AddColor;
uniform float _AddColorIntensity;

uniform float _ColorInt;
uniform float4 _Color1;
uniform float4 _Color2;
uniform float4 _Color3;
uniform float4 _Color4;
uniform float4 _OColor1;
uniform float4 _OColor2;
uniform float4 _OColor3;
uniform float4 _OColor4;

uniform float _LightColorBlend;
uniform float _SkyColorIntensity;
float _PointLightColorIntensity;
uniform float _PointLightStep;
uniform float _PointLightFeather;
uniform float _LightColorIntensity;

uniform float _NormalScale;

uniform float _ShadowIntensity;
uniform float4 _ShadowMapColor;
uniform float _Shadow_Step;
uniform float _Shadow_Feather;
uniform float _Shadow_Power;
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

// #ifdef _IS_HAIRMODE

//     float4 _NoiseMap_ST;
//     float4 _HighLit_Color;
//     float _HighLit_Intensity;
//     float _HighLit_Level;
//     float _HighLit_Range;
//     float _HighLit_LOD;
//     float _HighLit_Scale;
//     float _HighLit_ScaleStep;
//     float4 _LowLit_Color;
//     float _LowLit_Intensity;
//     float _LowLit_Level;
//     float _LowLit_Range;
// #endif

uniform float _RimLight_Enable;
uniform float4 _RimLightColor;
uniform float _RimLightIntensity;
uniform float _RimLightBlend;
uniform float4 _RimLightColor2;
uniform float _RimLightIntensity2;
uniform float _RimLightBlend2;
uniform float _RimLightFeather;
uniform float _RimLightLength;
uniform float _RimLightWidth;
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
uniform float _Outline_Lod;
uniform float _Outline_Blend;


CBUFFER_END

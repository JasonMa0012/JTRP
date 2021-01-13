// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ToonyColorsPro.Utilities;
using ToonyColorsPro.Legacy;

internal class TCP2_MaterialInspector_SurfacePBS_SG : ShaderGUI
{
	private enum WorkflowMode
	{
		Specular,
		Metallic,
		Dielectric
	}
	
	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,		// Old school alpha-blending mode, fresnel does not affect amount of transparency
		Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
	}

	public enum SmoothnessMapChannel
	{
		SpecularMetallicAlpha,
		AlbedoAlpha
	}

	private static class Styles
	{
		//public static GUIStyle optionsButton = "PaneOptions";
		//public static GUIContent uvSetLabel = new GUIContent("UV Set");
		//public static GUIContent[] uvSetOptions = { new GUIContent("UV channel 0"), new GUIContent("UV channel 1") };

		//public static string emptyTootip = "";
		public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
		public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
		public static GUIContent specularMapText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
		public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
		public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness Value");
		public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");
//		public static GUIContent smoothnessMapChannelText = new GUIContent("Source", "Smoothness texture and channel");
//		public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");
//		public static GUIContent reflectionsText = new GUIContent("Reflections", "Glossy Reflections");
		public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
		public static GUIContent heightMapText = new GUIContent("Height Map", "Height Map (G)");
		public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
		public static GUIContent emissionText = new GUIContent("Emission", "Emission (RGB)");
		public static GUIContent detailMaskText = new GUIContent("Detail Mask", "Mask for Secondary Maps (A)");
		public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
		public static GUIContent detailNormalMapText = new GUIContent("Normal Map", "Normal Map");

		//public static string whiteSpaceString = " ";
		public static string primaryMapsText = "Main Maps";
		public static string secondaryMapsText = "Secondary Maps";
//		public static string forwardText = "Forward Rendering Options";
		public static string renderingMode = "Rendering Mode";
		public static GUIContent emissiveWarning = new GUIContent ("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
		//public static GUIContent emissiveColorWarning = new GUIContent ("Ensure emissive color is non-black for emission to have effect.");
		public static readonly string[] blendNames = Enum.GetNames (typeof (BlendMode));

		//public static string tcp2_HeaderText = "Toony Colors Pro 2 - Stylization";
		public static string tcp2_highlightColorText = "Highlight Color";
		public static string tcp2_shadowColorText = "Shadow Color";
		/*
		public static GUIContent tcp2_rampText = new GUIContent("Ramp Texture", "Ramp 1D Texture (R)");
		public static GUIContent tcp2_rampThresholdText = new GUIContent("Threshold", "Threshold for the separation between shadows and highlights");
		public static GUIContent tcp2_rampSmoothText = new GUIContent("Main Light Smoothing", "Main Light smoothing of the separation between shadows and highlights");
		public static GUIContent tcp2_rampSmoothAddText = new GUIContent("Other Lights Smoothing", "Additional Lights smoothing of the separation between shadows and highlights");
		public static GUIContent tcp2_specSmoothText = new GUIContent("Specular Smoothing", "Stylized Specular smoothing");
		public static GUIContent tcp2_SpecBlendText = new GUIContent("Specular Blend", "Stylized Specular contribution over regular Specular");
		public static GUIContent tcp2_rimStrengthText = new GUIContent("Fresnel Strength", "Stylized Fresnel overall strength");
		public static GUIContent tcp2_rimMinText = new GUIContent("Fresnel Min", "Stylized Fresnel min ramp threshold");
		public static GUIContent tcp2_rimMaxText = new GUIContent("Fresnel Max", "Stylized Fresnel max ramp threshold");
		public static GUIContent tcp2_outlineColorText = new GUIContent("Outline Color", "Color of the outline");
		public static GUIContent tcp2_outlineWidthText = new GUIContent("Outline Width", "Width of the outline");

		public static string tcp2_TexLodText = "Outline Texture LOD";
		public static string tcp2_ZSmoothText = "ZSmooth Value";
		public static string tcp2_Offset1Text = "Offset Factor";
		public static string tcp2_Offset2Text = "Offset Units";
		public static string tcp2_srcBlendOutlineText = "Source Factor";
		public static string tcp2_dstBlendOutlineText = "Destination Factor";
		*/
	}

	MaterialProperty blendMode;
	MaterialProperty albedoMap;
	MaterialProperty albedoColor;
	MaterialProperty alphaCutoff;
	MaterialProperty specularMap;
	MaterialProperty specularColor;
	MaterialProperty metallicMap;
	MaterialProperty metallic;
	MaterialProperty smoothness;
	MaterialProperty smoothnessScale;
//	MaterialProperty smoothnessMapChannel = null;
//	MaterialProperty highlights = null;
//	MaterialProperty reflections = null;
	MaterialProperty bumpScale;
	MaterialProperty bumpMap;
	MaterialProperty occlusionStrength;
	MaterialProperty occlusionMap;
	MaterialProperty heigtMapScale;
	MaterialProperty heightMap;
	MaterialProperty emissionColorForRendering;
	MaterialProperty emissionMap;
	MaterialProperty detailMask;
	MaterialProperty detailAlbedoMap;
	MaterialProperty detailNormalMapScale;
	MaterialProperty detailNormalMap;
	//MaterialProperty uvSetSecondary = null;

	//TCP2
	List<MaterialProperty> SGProperties;
	MaterialProperty tcp2_highlightColor;
	MaterialProperty tcp2_shadowColor;

	static bool expandStandardProperties = true;
	static bool expandTCP2Properties = true;
	readonly string[] outlineNormalsKeywords = { "TCP2_NONE", "TCP2_COLORS_AS_NORMALS", "TCP2_TANGENT_AS_NORMALS", "TCP2_UV2_AS_NORMALS" };

	MaterialEditor m_MaterialEditor;
	WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
#if !UNITY_2018_1_OR_NEWER
	readonly ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1/99f, 3f);
#endif

	bool m_FirstTimeApply = true;
	public static bool InspectorNeedsUpdate;

	public void FindProperties (MaterialProperty[] props)
	{
		//Iterate through all properties so that we can extract the 'unknown' ones
		SGProperties = new List<MaterialProperty>();
		for (var i = 0; i < props.Length; i++)
		{
			var property = props[i];
			if(property != null)
			{
				switch(property.name)
				{
					// STANDARD
					case "_Mode": blendMode = property; break;
					case "_MainTex": albedoMap = property; break;
					case "_Color": albedoColor = property; break;
					case "_Cutoff": alphaCutoff = property; break;
					case "_SpecGlossMap": specularMap = property; break;
					case "_SpecColor": specularColor = property; break;
					case "_MetallicGlossMap": metallicMap = property; break;
					case "_Metallic": metallic = property; break;
					case "_Glossiness": smoothness = property; break;
					case "_GlossMapScale": smoothnessScale = property; break;
					//case "_SmoothnessTextureChannel": smoothnessMapChannel = property; break;
					case "_BumpScale": bumpScale = property; break;
					case "_BumpMap": bumpMap = property; break;
					case "_Parallax": heigtMapScale = property; break;
					case "_ParallaxMap": heightMap = property; break;
					case "_OcclusionStrength": occlusionStrength = property; break;
					case "_OcclusionMap": occlusionMap = property; break;
					case "_EmissionColor": emissionColorForRendering = property; break;
					case "_EmissionMap": emissionMap = property; break;
					case "_DetailMask": detailMask = property; break;
					case "_DetailAlbedoMap": detailAlbedoMap = property; break;
					case "_DetailNormalMapScale": detailNormalMapScale = property; break;
					case "_DetailNormalMap": detailNormalMap = property; break;
					//case "_UVSec": uvSetSecondary = property; break;

					// TCP2
					case "_HColor": tcp2_highlightColor = property; break;
					case "_SColor": tcp2_shadowColor = property; break;

					// Add to ShaderGenerator Properties
					default:
						if (!SGProperties.Contains(property))
						{
							if(!property.displayName.StartsWith("__"))
								SGProperties.Add(property);
						}
						else
							Debug.LogWarning("Duplicate property?\n" + property.name);
						break;
				}
			}
		}

		if (specularColor != null)
			m_WorkflowMode = WorkflowMode.Specular;
		else if (metallic != null)
			m_WorkflowMode = WorkflowMode.Metallic;
		else
			m_WorkflowMode = WorkflowMode.Dielectric;
	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
	{
		if(InspectorNeedsUpdate)
		{
			//reset properties
			blendMode = null;
			albedoMap = null;
			albedoColor = null;
			alphaCutoff = null;
			specularMap = null;
			specularColor = null;
			metallicMap = null;
			metallic = null;
			smoothness = null;
			smoothnessScale = null;
			//smoothnessMapChannel = null;
			bumpScale = null;
			bumpMap = null;
			occlusionStrength = null;
			occlusionMap = null;
			heigtMapScale = null;
			heightMap = null;
			emissionColorForRendering = null;
			emissionMap = null;
			detailMask = null;
			detailAlbedoMap = null;
			detailNormalMapScale = null;
			detailNormalMap = null;
			//uvSetSecondary = null;
		}

		FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		m_MaterialEditor = materialEditor;
		var material = materialEditor.target as Material;

		// Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
		// material to a standard shader.
		// Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
		if (m_FirstTimeApply || InspectorNeedsUpdate)
		{
			MaterialChanged(material, m_WorkflowMode);
			m_FirstTimeApply = false;
			InspectorNeedsUpdate = false;
		}

		//TCP2 Header
		EditorGUILayout.BeginHorizontal();
		var label = (Screen.width > 450f) ? "TOONY COLORS PRO 2 - INSPECTOR (Generated Shader)" : (Screen.width > 300f ? "TOONY COLORS PRO 2 - INSPECTOR" : "TOONY COLORS PRO 2");
		TCP2_GUI.HeaderBig(label);
		if (TCP2_GUI.Button(TCP2_GUI.CogIcon, "O", "Open in Shader Generator"))
		{
			if (material.shader != null)
			{
				TCP2_ShaderGenerator.OpenWithShader(material.shader);
			}
		}
		EditorGUILayout.EndHorizontal();
		TCP2_GUI.Separator();

		ShaderPropertiesGUI(material);

#if UNITY_5_5_OR_NEWER
		materialEditor.RenderQueueField();
#endif
#if UNITY_5_6_OR_NEWER
		materialEditor.EnableInstancingField();
#endif
	}

	public void ShaderPropertiesGUI (Material material)
	{
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();
		{
			BlendModePopup();

			GUILayout.Space(8f);
			expandStandardProperties = GUILayout.Toggle(expandStandardProperties, "STANDARD PROPERTIES", EditorStyles.toolbarButton);
			if (expandStandardProperties)
			{
				//Background
				var vertRect = EditorGUILayout.BeginVertical();
				vertRect.xMax += 2;
				vertRect.xMin--;
				GUI.Box(vertRect, "", "RL Background");
				GUILayout.Space(4f);

				// Primary properties
				GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
				DoAlbedoArea(material);
				DoSpecularMetallicArea();

				if(bumpMap != null)
					m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
				if(heightMap != null)
					m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heigtMapScale : null);
				if(occlusionMap != null)
					m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
				if(emissionMap != null || emissionColorForRendering != null)
					DoEmissionArea(material);
				if(detailMask != null)
					m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
				EditorGUI.BeginChangeCheck();
				m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
				if(EditorGUI.EndChangeCheck())
				{
					// Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
					if(emissionMap != null)
						emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset;
				}

				EditorGUILayout.Space();

				// Secondary properties
				if(detailAlbedoMap != null)
				{
					GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
					m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
					if(detailNormalMap != null && detailNormalMapScale != null)
					{
						m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
					}
					m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
					//TCP2: defined through the Shader Generator
					//m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);
				}

				// Third properties
				/*
				if (reflections != null)
					GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
				//if (highlights != null)
					//m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
				if (reflections != null)
					m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);
				*/

				GUILayout.Space(8f);
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.Space();

			//----------------------------------------------------------------
			//    TOONY COLORS PRO 2

			expandTCP2Properties = GUILayout.Toggle(expandTCP2Properties, "TOONY COLORS PRO 2", EditorStyles.toolbarButton);
			if (expandTCP2Properties)
			{
				//Background
				var vertRect = EditorGUILayout.BeginVertical();
				vertRect.xMax += 2;
				vertRect.xMin--;
				GUI.Box(vertRect, "", "RL Background");
				GUILayout.Space(4f);

				GUILayout.Label("Base Properties", EditorStyles.boldLabel);
				m_MaterialEditor.ColorProperty(tcp2_highlightColor, Styles.tcp2_highlightColorText);
				m_MaterialEditor.ColorProperty(tcp2_shadowColor, Styles.tcp2_shadowColorText);
				EditorGUILayout.Space();

				//Shader Generator Properties
				for(var i = 0; i < SGProperties.Count; i++)
				{
					if (SGProperties[i].type == MaterialProperty.PropType.Texture)
					{
						//Compensate margins so that texture slot looks square
						var fw = EditorGUIUtility.fieldWidth;
						EditorGUIUtility.fieldWidth = 64f;
						m_MaterialEditor.ShaderProperty(SGProperties[i], SGProperties[i].displayName);
						EditorGUIUtility.fieldWidth = fw;
					}
					else
						m_MaterialEditor.ShaderProperty(SGProperties[i], SGProperties[i].displayName);
				}

				GUILayout.Space(8f);
				GUILayout.EndVertical();

				// TCP2 End
				//----------------------------------------------------------------
			}

			GUILayout.Space(10f);
		}
		if (EditorGUI.EndChangeCheck())
		{
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj, m_WorkflowMode);
		}
	}

	void UpdateOutlineNormalsKeyword(int index)
	{
		var selectedKeyword = outlineNormalsKeywords[index];

		foreach (var obj in m_MaterialEditor.targets)
		{
			if (obj is Material)
			{
				var m = obj as Material;
				foreach (var kw in outlineNormalsKeywords)
					m.DisableKeyword(kw);
				m.EnableKeyword(selectedKeyword);
			}
		}
	}

	internal void DetermineWorkflow(MaterialProperty[] props)
	{
		if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
			m_WorkflowMode = WorkflowMode.Specular;
		else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
			m_WorkflowMode = WorkflowMode.Metallic;
		else
			m_WorkflowMode = WorkflowMode.Dielectric;
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
	{
		// _Emission property is lost after assigning Standard shader to the material
		// thus transfer it before assigning the new shader
		if (material.HasProperty("_Emission") && material.HasProperty("_EmissionColor"))
		{
			material.SetColor("_EmissionColor", material.GetColor("_Emission"));
		}

		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
		{
			SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
			return;
		}

		var blendMode = BlendMode.Opaque;
		if (oldShader.name.Contains("/Transparent/Cutout/"))
		{
			blendMode = BlendMode.Cutout;
		}
		else if (oldShader.name.Contains("/Transparent/"))
		{
			// NOTE: legacy shaders did not provide physically based transparency
			// therefore Fade mode
			blendMode = BlendMode.Fade;
		}
		material.SetFloat("_Mode", (float)blendMode);

		DetermineWorkflow( MaterialEditor.GetMaterialProperties (new[] { material }) );
		MaterialChanged(material, m_WorkflowMode);
	}

	void BlendModePopup()
	{
		EditorGUI.showMixedValue = blendMode.hasMixedValue;
		var mode = (BlendMode)blendMode.floatValue;

		EditorGUI.BeginChangeCheck();
		mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
		if (EditorGUI.EndChangeCheck())
		{
			m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
			blendMode.floatValue = (float)mode;
		}

		EditorGUI.showMixedValue = false;
	}

	void DoAlbedoArea(Material material)
	{
		m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
		if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
		{
			m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 0);
		}
	}

	void DoEmissionArea(Material material)
	{
#if UNITY_5_6_OR_NEWER
		// Emission for GI?
		if (m_MaterialEditor.EmissionEnabledProperty())
		{
			bool hadEmissionTexture = emissionMap.textureValue != null;

			// Texture and HDR color controls
	#if UNITY_2018_1_OR_NEWER
			m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);
	#else
			m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);
	#endif

			// If texture was assigned and color was black set color to white
			float brightness = emissionColorForRendering.colorValue.maxColorComponent;
			if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
				emissionColorForRendering.colorValue = Color.white;

			// change the GI flag and fix it up with emissive as black if necessary
			m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
		}
#else
		var showHelpBox = !HasValidEmissiveKeyword(material);

		var hadEmissionTexture = emissionMap.textureValue != null;

		// Texture and HDR color controls
		m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);

		// If texture was assigned and color was black set color to white
		var brightness = emissionColorForRendering.colorValue.maxColorComponent;
		if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
			emissionColorForRendering.colorValue = Color.white;

		// Emission for GI?
		m_MaterialEditor.LightmapEmissionProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel + 0);

		if (showHelpBox)
		{
			EditorGUILayout.HelpBox(Styles.emissiveWarning.text, MessageType.Warning);
		}
#endif
	}

	void DoSpecularMetallicArea()
	{
		var hasGlossMap = false;
		if (m_WorkflowMode == WorkflowMode.Specular)
		{
			if(specularMap != null)
			{
				hasGlossMap = true;
				m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap);
			}
			else
			{
				m_MaterialEditor.ShaderProperty(specularColor, Styles.specularMapText, 2);
			}
		}
		else if (m_WorkflowMode == WorkflowMode.Metallic)
		{
			if(metallicMap != null)
			{
				hasGlossMap = true;
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap);
			}
			else
			{
				m_MaterialEditor.ShaderProperty(metallic, Styles.metallicMapText, 2);
			}
		}

		var showSmoothnessScale = hasGlossMap;

		/*
		if (smoothnessMapChannel != null)
		{
			int smoothnessChannel = (int)smoothnessMapChannel.floatValue;
			if (smoothnessChannel == (int)SmoothnessMapChannel.AlbedoAlpha)
				showSmoothnessScale = true;
		}
		*/

		var indentation = 2; // align with labels of texture properties
		m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

		//++indentation;
		//TCP2: this is defined through the Shader Generator
		/*
		if (smoothnessMapChannel != null)
			m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
		*/
	}

	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)RenderQueue.AlphaTest;
				break;
			case BlendMode.Fade:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)RenderQueue.Transparent;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = (int)RenderQueue.Transparent;
				break;
		}
	}

	static SmoothnessMapChannel GetSmoothnessMapChannel( Material material )
	{
		var ch = (int)material.GetFloat("_SmoothnessTextureChannel");
		if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
			return SmoothnessMapChannel.AlbedoAlpha;
		return SmoothnessMapChannel.SpecularMetallicAlpha;
	}

	static bool ShouldEmissionBeEnabled( Material mat, Color color )
	{
		var realtimeEmission = (mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.RealtimeEmissive) > 0;
		return color.maxColorComponent > 0.1f / 255.0f || realtimeEmission;
	}

	static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
	{
		// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
		// (MaterialProperty value might come from renderer material property block)

		//TCP2: no more keywords, these features are toggled in the Shader Generator
		/*
		SetKeyword (material, "_NORMALMAP", material.HasProperty("_BumpMap") || material.HasProperty("_DetailNormalMap"));
		if (workflowMode == WorkflowMode.Specular)
			SetKeyword (material, "_SPECGLOSSMAP", material.GetTexture ("_SpecGlossMap"));
		else if (workflowMode == WorkflowMode.Metallic)
			SetKeyword (material, "_METALLICGLOSSMAP", material.GetTexture ("_MetallicGlossMap"));
		SetKeyword (material, "_PARALLAXMAP", material.GetTexture ("_ParallaxMap"));
		SetKeyword (material, "_DETAIL_MULX2", material.GetTexture ("_DetailAlbedoMap") || material.GetTexture ("_DetailNormalMap"));
		*/

#if UNITY_5_6_OR_NEWER
		// A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
		// or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
		// The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
		if (material.HasProperty("_EmissionColor"))
		{
			MaterialEditor.FixupEmissiveFlag(material);
			bool shouldEmissionBeEnabled = material.HasProperty("_EmissionColor") && (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
			SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
		}
		else if(material.IsKeywordEnabled("_EMISSION"))
		{
			SetKeyword(material, "_EMISSION", false);
		}
#else
		// Setup lightmap emissive flags
		var shouldEmissionBeEnabled = material.HasProperty("_EmissionColor") && ShouldEmissionBeEnabled(material, material.GetColor("_EmissionColor"));
		var flags = material.globalIlluminationFlags;
		if ((flags & (MaterialGlobalIlluminationFlags.BakedEmissive | MaterialGlobalIlluminationFlags.RealtimeEmissive)) != 0)
		{
			flags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			if (!shouldEmissionBeEnabled)
				flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;

			material.globalIlluminationFlags = flags;
		}
#endif
	}

	bool HasValidEmissiveKeyword (Material material)
	{
		// Material animation might be out of sync with the material keyword.
		// So if the emission support is disabled on the material, but the property blocks have a value that requires it, then we need to show a warning.
		// (note: (Renderer MaterialPropertyBlock applies its values to emissionColorForRendering))
		var hasEmissionKeyword = material.IsKeywordEnabled ("_EMISSION");
		if (!hasEmissionKeyword && ShouldEmissionBeEnabled (material, emissionColorForRendering.colorValue))
			return false;
		return true;
	}

	static void MaterialChanged(Material material, WorkflowMode workflowMode)
	{
		SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

		SetMaterialKeywords(material, workflowMode);
	}

	static void SetKeyword(Material m, string keyword, bool state)
	{
		if (state)
			m.EnableKeyword (keyword);
		else
			m.DisableKeyword (keyword);
	}
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Represents a user-created custom material property, that will be generated and injected in the code.
// It will be added as a Material Property in the shader, and can be used by any Shader Property.
//
// Main idea is to reuse channels from a texture between multiple features:
// - RGB = Albedo
// - A = Smoothness
// or
// - R = Smoothness, G = Rim strength, B = Subsurface Mask, A = Outline Width

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		public partial class ShaderProperty
		{
			[Serialization.SerializeAs("ct")]
			public class CustomMaterialProperty : IMaterialPropertyName
			{
				[Serialization.SerializeAs("cimp")]
				public Imp_MaterialProperty implementation;

				//================================================================================================================================
				// Deserialization

				[Serialization.CustomDeserializeCallback]
				static CustomMaterialProperty Deserialize(string data, object[] args)
				{
					ShaderProperty shaderProperty = ShaderGenerator2.CurrentConfig.customMaterialPropertyShaderProperty;

					// find the class name of the implementation, as it is needed to create an instance of CustomMaterialProperty
					string serializedClassName = data.Substring(data.IndexOf("cimp:") + "cimp:".Length);
					serializedClassName = serializedClassName.Substring(0, serializedClassName.IndexOf('('));
					Type implementationType = null;
					var allTypes = typeof(Serialization).Assembly.GetTypes();
					foreach (var t in allTypes)
					{
						var classAttributes = t.GetCustomAttributes(typeof(Serialization.SerializeAsAttribute), false);
						if (classAttributes != null && classAttributes.Length == 1)
						{
							var name = (classAttributes[0] as Serialization.SerializeAsAttribute).serializedName;
							if (name == serializedClassName)
							{
								//match!
								implementationType = t;
							}
						}
					}
					var customMaterialProperty = new CustomMaterialProperty(shaderProperty, implementationType);

					Func<object, string, object> onDeserializeImplementation = (impObj, impData) =>
					{
						// Make sure to deserialize as a new object, so that final Implementation subtype is kept instead of creating base Implementation class
						// Imp should only be an Imp_MaterialProperty
						var imp = Serialization.Deserialize(impData, new object[] { shaderProperty });
						return imp;
					};
					var implementationHandling = new Dictionary<Type, Func<object, string, object>> { { typeof(Imp_MaterialProperty), onDeserializeImplementation } };

					Serialization.DeserializeTo(customMaterialProperty, data, typeof(CustomMaterialProperty), args, implementationHandling);

					return customMaterialProperty;
				}

				//================================================================================================================================

				//Notification when a Custom Material Property is deleted
				public delegate void CustomTextureCallback(CustomMaterialProperty customTexture);
				public static event CustomTextureCallback OnCustomMaterialPropertyRemoved;

				//================================================================================================================================

				[Serialization.SerializeAs("exp")] bool expanded;
				[Serialization.SerializeAs("uv_exp")] bool uvExpanded;
				[Serialization.SerializeAs("imp_lbl")] public string implementationTypeLabel;

				public string Channels
				{
					get
					{
						switch(implementationTypeLabel)
						{
							case "Range":
							case "Float": return "XXXX";
							case "Vector": return "XYZW";
							case "Color":
							case "Texture": return "RGBA";
							default: return "RGBA";
						}
					}
				}

				public string GetChannelsForVariableType(VariableType variableType)
				{
					switch (variableType)
					{
						case VariableType.@float: return Channels.Substring(0, 1);
						case VariableType.float2: return Channels.Substring(0, 2);
						case VariableType.color:
						case VariableType.float3: return Channels.Substring(0, 3);
						case VariableType.color_rgba:
						case VariableType.float4: return Channels.Substring(0, 4);
					}
					return Channels;
				}

				//system to ensure each property name is unique
				public string GetPropertyName() { return implementation.PropertyName; }
				public string PropertyName
				{
					get { return implementation.PropertyName; }
					set { implementation.PropertyName = value; }
				}
				public string Label { get { return implementation.Label; } }
				public bool HasErrors { get { return implementation.HasErrors; } }
				public bool IsGpuInstanced { get { return implementation.IsGpuInstanced; } }

				internal OptionFeatures[] NeededFeatures() { return implementation.NeededFeatures(); }

				public CustomMaterialProperty(ShaderProperty sp, Type implementationType)
				{
					implementation = (Imp_MaterialProperty)Activator.CreateInstance(implementationType, new object[] { sp });
					var menuLabel = implementationType.GetProperty("MenuLabel", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
					string label = (string)menuLabel.GetValue(null, null);
					label = label.Replace("Material Property/", "");
					implementation.PropertyName = string.Format("_{0}", SGUILayout.Utils.RemoveWhitespaces("My " + label));
					implementation.Label = SGUILayout.Utils.VariableNameToReadable(implementation.PropertyName);
					implementation.IsCustomMaterialProperty = true;

					implementationTypeLabel = label;
				}

				public CustomMaterialProperty Clone()
				{
					return (CustomMaterialProperty)MemberwiseClone();
				}

				public void WillBeRemoved()
				{
					implementation.WillBeRemoved();

					if (OnCustomMaterialPropertyRemoved != null)
					{
						OnCustomMaterialPropertyRemoved(this);
					}
				}

				public override string ToString()
				{
					return "[CustomTexture " + PropertyName + ": " + implementation.ToString() + "]";
				}

				//Shader code output that goes in the ShaderLab Properties { } block
				public string PrintProperty(string indent)
				{
					return implementation.PrintProperty(indent);
				}

				//Shader code output that declares the variables, if any
				public string PrintVariablesDeclare(bool gpuInstanced, string indent)
				{
					if (implementation.IsGpuInstanced && !gpuInstanced
						|| !implementation.IsGpuInstanced && gpuInstanced)
					{
						return null;
					}

					return implementation.PrintVariableDeclare(indent);
				}

				public string PrintVariablesDeclareOutsideCBuffer(string indent)
				{
					return implementation.PrintVariableDeclareOutsideCBuffer(indent);
				}

				public string PrintVariableFragment()
				{
					// Only texture properties need sampling, others can use their variable name directly
					if (implementation is Imp_MaterialProperty_Texture)
					{
						return string.Format("value_{0}", PropertyName);
					}

					return PropertyName;
				}

				public string PrintVariableVertex()
				{
					// Only texture properties need sampling, others can use their variable name directly
					if (implementation is Imp_MaterialProperty_Texture)
					{
						return string.Format("value_{0}", PropertyName);
					}

					return PropertyName;
				}

				public string SampleVariableFragment(string inputSource, string outputSource)
				{
					if (implementation is Imp_MaterialProperty_Texture)
					{
						// TODO variable precision option
						return string.Format("half{0} {1} = {2};\n", "4", this.PrintVariableFragment(), implementation.PrintVariableFragment(inputSource, outputSource, null));
					}

					return null;
				}

				public string SampleVariableVertex(string inputSource, string outputSource)
				{
					if (implementation is Imp_MaterialProperty_Texture)
					{
						return string.Format("half{0} {1} = {2};\n", "4", this.PrintVariableVertex(), implementation.PrintVariableVertex(inputSource, outputSource, null));
					}

					return null;
				}

				//================================================================================================================================

				public delegate void ButtonClick(int index);

				public void ShowGUILayout(int index, ButtonClick onAdd, ButtonClick onRemove)
				{
					var guiColor = GUI.color;
					GUI.color *= EditorGUIUtility.isProSkin || HasErrors ? Color.white : new Color(.75f, .75f, .75f, 1f);
					var style = EditorStyles.helpBox;
					if (HasErrors)
						style = expanded ? TCP2_GUI.ErrorPropertyHelpBoxExp : TCP2_GUI.ErrorPropertyHelpBox;
					EditorGUILayout.BeginVertical(style);
					GUI.color = guiColor;

					using (new SGUILayout.IndentedLine(16))
					{
						const float buttonWidth = 20;

						var rect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight));
						var guiContent = new GUIContent(string.Format("{0} ({1})", Label, implementationTypeLabel));
						rect.width -= buttonWidth*2;

						// hover
						TCP2_GUI.DrawHoverRect(rect);

						EditorGUI.BeginChangeCheck();
						expanded = EditorGUI.Foldout(rect, expanded, guiContent, true, TCP2_GUI.HeaderDropDown);
						if (EditorGUI.EndChangeCheck())
						{
							if (Event.current.alt || Event.current.control)
							{
								var state = expanded;
								foreach (var cmp in ShaderGenerator2.CurrentConfig.CustomMaterialProperties)
								{
									cmp.expanded = state;
								}
							}
						}

						rect.x += rect.width;
						rect.width = buttonWidth;
						rect.height = EditorGUIUtility.singleLineHeight;
						if (GUI.Button(rect, "+", EditorStyles.miniButtonLeft))
						{
							onAdd(index);
						}
						rect.x += rect.width;
						if (GUI.Button(rect, "-", EditorStyles.miniButtonRight))
						{
							onRemove(index);
						}

						var labelWidth = TCP2_GUI.HeaderDropDown.CalcSize(guiContent).x;
						rect = GUILayoutUtility.GetLastRect();
						rect.y += 2;
						rect.x += labelWidth;
						rect.width -= labelWidth;
						GUI.Label(rect, ": " + PropertyName, SGUILayout.Styles.GrayMiniLabel);
					}

					if (expanded)
					{
						GUILayout.Space(4);

						implementation.NewLineGUI(false);
					}

					EditorGUILayout.EndVertical();
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

// Implementations that can be used for Shader Properties

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		public partial class ShaderProperty
		{
			//Represents a Shader Property Implementation, e.g. a constant value, material property, vertex color channel...
			public class Implementation
			{
				//Defines the order in which menu item will appear in the menu
				static public Type[] MenuOrders = new Type[]
				{
					typeof(Imp_ConstantValue),
					typeof(Imp_ConstantFloat),
					typeof(Imp_MaterialProperty_Float),
					typeof(Imp_MaterialProperty_Range),
					typeof(Imp_MaterialProperty_Vector),
					typeof(Imp_MaterialProperty_Color),
					typeof(Imp_MaterialProperty_Texture),
					typeof(Imp_VertexColor),
					typeof(Imp_VertexTexcoord),
					typeof(Imp_WorldPosition),
					typeof(Imp_ShaderPropertyReference),
					typeof(Imp_CustomMaterialProperty),
					typeof(Imp_HSV),
					typeof(Imp_CustomCode),
					typeof(Imp_GenericFromTemplate),
				};

				[Serialization.SerializeAs("guid")] public string guid;
				[Serialization.SerializeAs("op")] public Operator @operator = Operator.Multiply;      //How this implementation is calculated compared to the previous one
				[Serialization.SerializeAs("lbl"), ExcludeFromCopy] public string Label = "Property Label";
				[Serialization.SerializeAs("gpu_inst")] public bool IsGpuInstanced = false;
				[Serialization.SerializeAs("locked"), ExcludeFromCopy] public bool IsLocked = false;
				[Serialization.SerializeAs("impl_index"), ExcludeFromCopy] public int DefaultImplementationIndex = -1; // if >= 0, then this is a default implementation

				// Default implementation helpers: system used to check if a variable is different than the default one (highlight labels)
				protected bool IsDefaultImplementation { get { return DefaultImplementationIndex >= 0; } }
				protected T GetDefaultImplementation<T>() where T : Implementation
				{
					return ParentShaderProperty.defaultImplementations[DefaultImplementationIndex] as T;
				}

				public ShaderProperty ParentShaderProperty;

				public virtual void CheckErrors() { }
				public virtual bool HasErrors { get { return false; } }

				public Implementation(ShaderProperty shaderProperty)
				{
					this.guid = Guid.NewGuid().ToString();

					if (shaderProperty != null)
					{
						ParentShaderProperty = shaderProperty;
						Label = shaderProperty.Name;
					}
				}

				// Defines if the implementation can be copied
				internal virtual bool CanBeCopied() { return true; }
				public virtual void WillBeRemoved() { }
				public virtual void OnPasted() { }

				public override string ToString()
				{
					return string.Format("[Implementation: {0}]", this.GetType());
				}

				public virtual string ToHashString()
				{
					var result = new StringBuilder();

					var props = GetType().GetProperties();
					foreach (var prop in props)
					{
						var attributes = prop.GetCustomAttributes(typeof(Serialization.SerializeAsAttribute), true);
						if (attributes == null || attributes.Length == 0)
						{
							continue;
						}

						if (prop.PropertyType == typeof(ShaderProperty))
						{
							var spRef = (ShaderProperty)prop.GetValue(this, null);
							result.Append(spRef != null ? spRef.Name : "EmptyShaderPropertyRef");
						}
						else
						{
							result.Append(prop.GetValue(this, null));
						}
					}

					var fields = GetType().GetFields();
					foreach (var field in fields)
					{
						if (field.Name == "guid") continue;
						result.Append(field.GetValue(this));
					}

					return result.ToString();
				}

				string DebugSerializableProps()
				{
					var result = new StringBuilder();

					var props = GetType().GetProperties();
					foreach (var prop in props)
					{
						var attributes = prop.GetCustomAttributes(typeof(Serialization.SerializeAsAttribute), true);
						if (attributes == null || attributes.Length == 0)
						{
							continue;
						}

						if (prop.PropertyType == typeof(ShaderProperty))
						{
							var spRef = (ShaderProperty)prop.GetValue(this, null);
							result.AppendLine(prop.Name + " = " + spRef != null ? spRef.Name : "EmptyShaderPropertyRef");
						}
						else
						{
							result.AppendLine(prop.Name + " = " + prop.GetValue(this, null));
						}
					}

					var fields = GetType().GetFields();
					foreach (var field in fields)
					{
						if (field.Name == "guid") continue;
						result.AppendLine(field.Name + " = " + field.GetValue(this));
					}

					return result.ToString();
				}

				void CompareToDefaultImplementation<T>() where T:Implementation
				{
					string current = DebugSerializableProps();
					string def = GetDefaultImplementation<T>().DebugSerializableProps();
					Debug.Log("Default:\n" + def);
					Debug.Log("Current:\n" + current);
				}

				virtual public Implementation Clone()
				{
					return (Implementation)MemberwiseClone();
				}

				public void CopyCommonProperties(Implementation from)
				{
					this.@operator = from.@operator;
					this.Label = from.Label;
					this.IsGpuInstanced = from.IsGpuInstanced;

					var from_mp = from as Imp_MaterialProperty;
					var this_mp = this as Imp_MaterialProperty;
					if (this_mp != null && from_mp != null)
					{
						this_mp.PropertyName = from_mp.PropertyName;
					}
				}

				public void SetOperator(object @operator)
				{
					this.@operator = (Operator)(Mathf.Clamp((int)@operator, 0, OperatorSymbols.Length));
				}

				//Label to show on the drop-down button when this implementation is selected
				internal virtual string GUILabel() { return "Error: base Implementation class"; }

				//Shader code output that goes in the ShaderLab Properties { } block
				internal virtual string PrintProperty(string indent) { return null; }

				//Shader code output that declares the variables, if any
				internal virtual string PrintVariableDeclare(string indent) { return null; }

				//Shader code output that declares the variables that are incompatible with CBUFFER blocks
				internal virtual string PrintVariableDeclareOutsideCBuffer(string indent) { return null; }

				internal virtual string PrintVariableDeclare(string indent, bool gpuInstanced)
				{
					// Default behavior for GPU instancing: print declaration only if flags match
					if ( (this.IsGpuInstanced && gpuInstanced) || (!this.IsGpuInstanced && !gpuInstanced) )
					{
						return PrintVariableDeclare(indent);
					}
					else
					{
						return null;
					}
				}

				//Shader code output that represents the variable in the fragment shader
				internal virtual string PrintVariableFragment(string inputSource, string outputSource, string arguments) { return null; }

				//shader code output that represents the variable in the vertex shader
				internal virtual string PrintVariableVertex(string inputSource, string outputSource, string arguments) { return PrintVariableFragment(inputSource, outputSource, arguments); }

				//output the value of a fixed function property: either a constant value, or a material property
				internal virtual string PrintVariableFixedFunction() { throw new InvalidOperationException("This implementation cannot be used with fixed function properties."); }

				//Returns a list of features needed to make this implementation work, such as USE_VERTEX_COLORS (enum)
				internal virtual OptionFeatures[] NeededFeatures() { return new OptionFeatures[0]; }

				//Returns a list of extra features needed to make this implementation work (raw strings)
				internal virtual string[] NeededFeaturesExtra() { return new string[0]; }

				//GUI that goes on the line(s) under the drop-down
				internal virtual void NewLineGUI(bool usedByCustomCode) { }

				internal virtual bool HasOperator() { return true; }

				protected static void BeginHorizontal(float indentOffset = 0f)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(UI.GUI_NEWLINE_INDENT + indentOffset);
				}

				protected static void EndHorizontal()
				{
					GUILayout.Space(4);
					GUILayout.EndHorizontal();
				}

				public string PrintOperator()
				{
					switch (@operator)
					{
						case Operator.Multiply: return " * ";
						case Operator.Divide: return " / ";
						case Operator.Add: return " + ";
						case Operator.Subtract: return " - ";

						default:
							Debug.LogError(ShaderGenerator2.ErrorMsg("Unknown operator: " + @operator));
							return "";
					}
				}
			}

			[Serialization.SerializeAs("imp_hook")]
			public class Imp_Hook : Implementation
			{
				internal override bool CanBeCopied() { return false; }

				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Hook"; } }
				internal override string GUILabel() { return MenuLabel; }

				public Imp_Hook(ShaderProperty shaderProperty) : base(shaderProperty)
				{
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					return this.Label;
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Shader Variable:");
						using (new EditorGUI.DisabledScope(true))
						{
							SGUILayout.TextField(this.Label);
						}
					}
					EndHorizontal();

					BeginHorizontal();
					{
						TCP2_GUI.HelpBoxLayout("Add implementations to this Shader Property to modify the output of this variable in the shader.", MessageType.Info);
					}
					EndHorizontal();
				}
			}

			public abstract class Imp_MaterialProperty : Implementation, IMaterialPropertyName
			{
				//system to ensure each property name is unique
				public string GetPropertyName() { return PropertyName; }

				[Serialization.SerializeAs("prop"), ExcludeFromCopy] protected string _PropertyName = "_ShaderProperty";
				public string PropertyName
				{
					get { return _PropertyName; }
					set { _PropertyName = UniqueMaterialPropertyName.GetUniquePropertyName(value, this); }
				}
				[Serialization.SerializeAs("md")] public string MaterialDrawers = "";
				[Serialization.SerializeAs("custom")] public bool IsCustomMaterialProperty = false;
				[Serialization.SerializeAs("refs")] public string CustomMaterialPropertyReferences = "";

				public Imp_MaterialProperty(ShaderProperty shaderProperty) : base(shaderProperty) { }

				public override Implementation Clone()
				{
					var mp = (Imp_MaterialProperty)base.Clone();
					//special case for material property: this will trigger the unique variable name check
					mp.PropertyName = mp.PropertyName;
					return mp;
				}

				public override void CheckErrors()
				{
					if (IsCustomMaterialProperty)
					{
						IsCustomMaterialPropertyReferenced();
					}

					base.CheckErrors();
				}

				protected bool IsCustomMaterialPropertyReferenced()
				{
					if (!IsCustomMaterialProperty)
					{
						throw new Exception("'IsCustomMaterialPropertyReferenced' shouldn't be used when 'IsCustomMaterialProperty' is false");
					}

					bool isReferenced = false;
					CustomMaterialPropertyReferences = "";
					foreach (var sp in ShaderGenerator2.CurrentConfig.VisibleShaderProperties)
					{
						foreach (var imp in sp.implementations)
						{
							var imp_cmp = imp as Imp_CustomMaterialProperty;
							if (imp_cmp != null && !imp_cmp.willBeRemoved)
							{
								if (imp_cmp.LinkedCustomMaterialProperty != null && imp_cmp.LinkedCustomMaterialProperty.implementation == this)
								{
									isReferenced = true;
									CustomMaterialPropertyReferences += imp_cmp.ParentShaderProperty.Name + ", ";
								}
							}
						}
					}

					if (CustomMaterialPropertyReferences.Length > 0)
					{
						CustomMaterialPropertyReferences = CustomMaterialPropertyReferences.Substring(0, CustomMaterialPropertyReferences.Length-2); // remove trailing ", "
					}

					return isReferenced;
				}

				protected abstract string PropertyTypeName();

				protected string FetchVariable(string variableName)
				{
					return this.IsGpuInstanced ? string.Format("UNITY_ACCESS_INSTANCED_PROP(Props, {0})", variableName) : variableName;
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					{
						ShaderGenerator2.ContextualHelpBox(string.Format("Will create a {0} property that you can tweak in the Material Inspector, or with scripts with the Material or Shader APIs.", PropertyTypeName()));
					}
					EndHorizontal();
					GUILayout.Space(4f);

					if (IsCustomMaterialProperty)
					{
						// Show references
						BeginHorizontal();
						{
							bool isReferenced = !string.IsNullOrEmpty(CustomMaterialPropertyReferences);
							string color = EditorGUIUtility.isProSkin ? "#00927C" : "#087566";
							string label = isReferenced ? "<b><color={0}>Referenced by:</color></b> " + CustomMaterialPropertyReferences :
								"<i>This Material Property isn't referenced in any Shader Property, it won't be included in the generated shader.</i>";

							GUILayout.Label(string.Format(label, color), SGUILayout.Styles.GrayMiniLabelWrap, GUILayout.ExpandWidth(true));
						}
						EndHorizontal();

						GUILayout.Space(5);
						BeginHorizontal();
						{
							GUILayout.Space(2);
							SGUILayout.DrawLine(EditorGUIUtility.isProSkin ? new Color(.3f, .3f, .3f) : new Color(.65f, .65f, .65f));
						}
						EndHorizontal();
						GUILayout.Space(5);
					}

					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Label");
						Label = SGUILayout.TextField(Label);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Variable");

						//Only update if value is effectively changed, because we're calling a setter that loops through all ShaderProperties
						var newName = SGUILayout.TextFieldShaderVariable(PropertyName);
						if (newName != PropertyName)
							PropertyName = newName;
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? !string.IsNullOrEmpty(MaterialDrawers) : MaterialDrawers != GetDefaultImplementation<Imp_MaterialProperty>().MaterialDrawers;
						SGUILayout.InlineLabel("Property Drawers", "Add one or multiple property drawers/decorators to this property\n(e.g. [NoScaleOffset])", highlighted);
						MaterialDrawers = SGUILayout.TextField(MaterialDrawers);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? IsGpuInstanced : IsGpuInstanced != GetDefaultImplementation<Imp_MaterialProperty>().IsGpuInstanced;
						SGUILayout.InlineLabel("GPU Instanced", "Tag this property as a possible variant for GPU instancing", highlighted);
						IsGpuInstanced = SGUILayout.Toggle(IsGpuInstanced);
					}
					EndHorizontal();

					BeginHorizontal();
					GUILayout.Space(2);
					SGUILayout.DrawLine(EditorGUIUtility.isProSkin ? new Color(.3f, .3f, .3f) : new Color(.65f, .65f, .65f));
					EndHorizontal();
					GUILayout.Space(5);
				}

				internal override string PrintProperty(string indent)
				{
					return MaterialDrawers + " ";
				}
			}

			[Serialization.SerializeAs("imp_mp_float")]
			public class Imp_MaterialProperty_Float : Imp_MaterialProperty
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll | VariableType.fixed_function_float; } }
				public static string MenuLabel { get { return "Material Property/Float"; } }
				internal override string GUILabel() { return MenuLabel; }
				protected override string PropertyTypeName() { return "float"; }

				[Serialization.SerializeAs("def")] public float DefaultValue = 1.0f;

				public Imp_MaterialProperty_Float(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					PropertyName = "_" + SGUILayout.Utils.RemoveWhitespaces(Label);
					Label = shaderProperty.Name + " Float";
				}

				internal override string PrintVariableFixedFunction() { return string.Format("[{0}]", PropertyName); }
				internal override string PrintProperty(string indent) { return base.PrintProperty(indent) + string.Format(CultureInfo.InvariantCulture, "{0} (\"{1}\", Float) = {2}", PropertyName, Label, DefaultValue); }
				internal override string PrintVariableDeclare(string indent) { return string.Format("float {0};", PropertyName); }
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments) { return FetchVariable(PropertyName); }

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? DefaultValue != 1.0f : DefaultValue != GetDefaultImplementation<Imp_MaterialProperty_Float>().DefaultValue;
						SGUILayout.InlineLabel("Default Value", highlighted);
						DefaultValue = SGUILayout.FloatField(DefaultValue);
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_mp_range")]
			public class Imp_MaterialProperty_Range : Imp_MaterialProperty
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll | VariableType.fixed_function_float; } }
				public static string MenuLabel { get { return "Material Property/Range"; } }
				internal override string GUILabel() { return MenuLabel; }
				protected override string PropertyTypeName() { return "float range"; }

				[Serialization.SerializeAs("def")] public float DefaultValue = 0.5f;
				[Serialization.SerializeAs("min")] public float Min;
				[Serialization.SerializeAs("max")] public float Max = 1.0f;

				public Imp_MaterialProperty_Range(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					PropertyName = "_" + SGUILayout.Utils.RemoveWhitespaces(Label);
					Label = shaderProperty.Name + " Range";
				}

				internal override string PrintVariableFixedFunction() { return string.Format("[{0}]", PropertyName); }
				internal override string PrintProperty(string indent) { return base.PrintProperty(indent) + string.Format(CultureInfo.InvariantCulture, "{0} (\"{1}\", Range({3},{4})) = {2}", PropertyName, Label, DefaultValue, Min, Max); }
				internal override string PrintVariableDeclare(string indent) { return string.Format("float {0};", PropertyName); }
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments) { return FetchVariable(PropertyName); }

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Min != 0.0f : Min != GetDefaultImplementation<Imp_MaterialProperty_Range>().Min;
						SGUILayout.InlineLabel("Min", highlighted);
						Min = SGUILayout.FloatField(Min);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Max != 1.0f : Max != GetDefaultImplementation<Imp_MaterialProperty_Range>().Max;
						SGUILayout.InlineLabel("Max", highlighted);
						Max = SGUILayout.FloatField(Max);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? DefaultValue != 0.5f : DefaultValue != GetDefaultImplementation<Imp_MaterialProperty_Range>().DefaultValue;
						SGUILayout.InlineLabel("Default Value", highlighted);
						DefaultValue = SGUILayout.FloatField(DefaultValue);
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_mp_vector")]
			public class Imp_MaterialProperty_Vector : Imp_MaterialProperty
			{
				public static VariableType VariableCompatibility { get { return VariableType.float2 | VariableType.float3 | VariableType.float4 | VariableType.color | VariableType.color_rgba; } }
				public static string MenuLabel { get { return "Material Property/Vector"; } }
				internal override string GUILabel() { return MenuLabel; }
				protected override string PropertyTypeName() { return "vector4"; }

				[Serialization.SerializeAs("def")] public Vector4 DefaultValue = Vector4.zero;
				[Serialization.SerializeAs("fp")] public FloatPrecision FloatPrec;
				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "XYZ";
				string DefaultChannels = "RGBA";

				public Imp_MaterialProperty_Vector(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					PropertyName = "_" + SGUILayout.Utils.RemoveWhitespaces(Label);
					Label = shaderProperty.Name + " Vector";

					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.float2: Channels = "XY"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "XYZ"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "XYZW"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				internal override string PrintProperty(string indent) { return base.PrintProperty(indent) + string.Format(CultureInfo.InvariantCulture, "{0} (\"{1}\", Vector) = ({2},{3},{4},{5})", PropertyName, Label, DefaultValue.x, DefaultValue.y, DefaultValue.z, DefaultValue.w); }
				internal override string PrintVariableDeclare(string indent)
				{
					// Always declare a float4, even if all channels aren't necessarily used, as they could still be used for custom code
					//var channels = ChannelsCount > 1 ? ChannelsCount.ToString() : "";
					string channels = "4";
					return string.Format("{0}{1} {2};", FloatPrec, channels, PropertyName);
				}
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}{1}", FetchVariable(PropertyName), channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? DefaultValue != Vector4.zero : DefaultValue != GetDefaultImplementation<Imp_MaterialProperty_Vector>().DefaultValue;
						SGUILayout.InlineLabel("Default Value", highlighted);
						int channelsCount = usedByCustomCode ? 4 : ChannelsCount;
						switch (channelsCount)
						{
							case 4: DefaultValue = SGUILayout.Vector4Field(DefaultValue); break;
							case 3: DefaultValue = SGUILayout.Vector3Field(DefaultValue); break;
							case 2: DefaultValue = SGUILayout.Vector2Field(DefaultValue); break;
						}
					}
					EndHorizontal();

					if (!IsCustomMaterialProperty)
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_MaterialProperty_Vector>().Channels;
							SGUILayout.InlineLabel("Swizzle", highlighted);

							if (usedByCustomCode)
							{
								using (new EditorGUI.DisabledScope(true))
								{
									GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
								}
							}
							else
							{
								if (ChannelsCount == 1)
									Channels = SGUILayout.XYZWSelector(Channels);
								else
									Channels = SGUILayout.XYZWSwizzle(Channels, ChannelsCount);
							}
						}
						EndHorizontal();
					}
				}
			}

			[Serialization.SerializeAs("imp_mp_color")]
			public class Imp_MaterialProperty_Color : Imp_MaterialProperty
			{
				public static VariableType VariableCompatibility { get { return VariableType.float2 | VariableType.float3 | VariableType.float4 | VariableType.color | VariableType.color_rgba; } }
				public static string MenuLabel { get { return "Material Property/Color"; } }
				internal override string GUILabel() { return MenuLabel; }
				protected override string PropertyTypeName() { return "color"; }

				[Serialization.SerializeAs("def")] public Color DefaultValue = Color.white;
				[Serialization.SerializeAs("hdr")] public bool Hdr;
				[Serialization.SerializeAs("cc")] public int ChannelsCount = 4;
				[Serialization.SerializeAs("chan")] public string Channels = "RGB";
				string DefaultChannels = "RGB";

				public Imp_MaterialProperty_Color(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					PropertyName = "_" + SGUILayout.Utils.RemoveWhitespaces(Label);
					Label = shaderProperty.Name + " Color";

					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.float2: Channels = "RG"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "RGB"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "RGBA"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				internal override string PrintProperty(string indent) { return base.PrintProperty(indent) + string.Format(CultureInfo.InvariantCulture, "{7}{6}{0} (\"{1}\", Color) = ({2},{3},{4},{5})", PropertyName, Label, DefaultValue.r, DefaultValue.g, DefaultValue.b, DefaultValue.a, Hdr ? "[HDR] " : "", ChannelsCount < 4 ? "[TCP2ColorNoAlpha] " : ""); }
				internal override string PrintVariableDeclare(string indent)
				{
					// Always declare a float4, even if all channels aren't necessarily used, as they could still be used for custom code
					//var channels = ChannelsCount > 1 ? ChannelsCount.ToString() : "";
					string channels = "4";
					return string.Format("{0}{1} {2};", Hdr ? FloatPrecision.half : FloatPrecision.@fixed, channels, PropertyName);
				}
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}{1}", FetchVariable(PropertyName), channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? DefaultValue != Color.white : DefaultValue != GetDefaultImplementation<Imp_MaterialProperty_Color>().DefaultValue;
						SGUILayout.InlineLabel("Default Value", highlighted);
						var showAlpha = ChannelsCount >= 4 || usedByCustomCode;
						DefaultValue = SGUILayout.ColorField(DefaultValue, showAlpha, Hdr);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Hdr : Hdr != GetDefaultImplementation<Imp_MaterialProperty_Color>().Hdr;
						SGUILayout.InlineLabel("HDR Color", highlighted);
						Hdr = SGUILayout.Toggle(Hdr);
					}
					EndHorizontal();

					if (!IsCustomMaterialProperty)
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_MaterialProperty_Color>().Channels;
							SGUILayout.InlineLabel("Swizzle", highlighted);

							if (usedByCustomCode)
							{
								using (new EditorGUI.DisabledScope(true))
								{
									GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
								}
							}
							else
							{
								if (ChannelsCount == 1)
									Channels = SGUILayout.RGBASelector(Channels);
								else
									Channels = SGUILayout.RGBASwizzle(Channels, ChannelsCount);
							}
						}
						EndHorizontal();
					}
				}
			}

			[Serialization.SerializeAs("imp_mp_texture")]
			public class Imp_MaterialProperty_Texture : Imp_MaterialProperty
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Material Property/Texture"; } }
				internal override string GUILabel() { return MenuLabel; }
				protected override string PropertyTypeName() { return "texture"; }

				public override bool HasErrors
				{
					get
					{
						bool linkedSpErrors = UvSource == UvSourceType.OtherShaderProperty &&
							( _linkedShaderProperty == null || (_linkedShaderProperty != null && !_linkedShaderProperty.IsVisible()) );

						return base.HasErrors | linkedSpErrors | (UseTilingOffset && invalidTilingOffsetVariable) | (UseScrolling && invalidScrollingVariable);
					}
				}

				public override void CheckErrors()
				{
					base.CheckErrors();

					VerifyReferencedValuesValidity();
				}

				internal override OptionFeatures[] NeededFeatures()
				{
					List<OptionFeatures> list = new List<OptionFeatures>();

					list.AddRange(base.NeededFeatures());

					if (NoTile && program == ProgramType.Fragment)
					{
						list.Add(OptionFeatures.NoTile_Sampling);
					}

					if (RandomOffset)
					{
						list.Add(OptionFeatures.UV_Anim_Random_Offset);
					}

					if (UvSource == UvSourceType.ScreenSpace)
					{
						list.Add(ScreenSpaceUVVertex ? OptionFeatures.Screen_Space_UV_Vertex : OptionFeatures.Screen_Space_UV_Fragment);

						if (ScreenSpaceUVObjectOffset && !ScreenSpaceUVVertex)
						{
							list.Add(OptionFeatures.Screen_Space_UV_Object_Offset);
						}
					}

					if (UvSource == UvSourceType.WorldPosition)
					{
						list.Add((program == ProgramType.Vertex) ? OptionFeatures.World_Pos_UV_Vertex : OptionFeatures.World_Pos_UV_Fragment);
					}

					return list.ToArray();
				}

				public enum UvSourceType
				{
					Texcoord,
					ScreenSpace,
					WorldPosition,
					OtherShaderProperty
				}

				[Serialization.SerializeAs("uto")] public bool UseTilingOffset;
				[Serialization.SerializeAs("tov")] public string TilingOffsetVariable = "";
				[Serialization.SerializeAs("tov_lbl")] public string TilingOffsetVariableLabel = "";
				[Serialization.SerializeAs("gto")] public bool GlobalTilingOffset;
				[Serialization.SerializeAs("sbt")] public bool ScaleByTexelSize;
				[Serialization.SerializeAs("scr")] public bool UseScrolling;
				[Serialization.SerializeAs("scv")] public string ScrollingVariable = "";
				[Serialization.SerializeAs("scv_lbl")] public string ScrollingVariableLabel = "";
				[Serialization.SerializeAs("gsc")] public bool GlobalScrolling;
				[Serialization.SerializeAs("roff")] public bool RandomOffset;
				[Serialization.SerializeAs("goff")] public bool GlobalRandomOffset;
				[Serialization.SerializeAs("notile")] public bool NoTile;
				[Serialization.SerializeAs("def")] public string DefaultValue = SGUILayout.Constants.DefaultTextureValues[0];
				[Serialization.SerializeAs("locked_uv"), ExcludeFromCopy] public bool IsUvLocked;
				[Serialization.SerializeAs("uv")] public int UvChannel;
				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "RGB";
				[Serialization.SerializeAs("mip")] public int MipLevel = -1;
				[Serialization.SerializeAs("mipprop")] public bool MipProperty;
				//[Serialization.SerializeAs("ssuv")] public bool UseScreenSpaceUV;
				[Serialization.SerializeAs("ssuv_vert")] public bool ScreenSpaceUVVertex;
				[Serialization.SerializeAs("ssuv_obj")] public bool ScreenSpaceUVObjectOffset;
				//[Serialization.SerializeAs("wpuv")] public bool UseWorldPosUV;
				[Serialization.SerializeAs("uv_type")] public UvSourceType UvSource = UvSourceType.Texcoord;
				[Serialization.SerializeAs("uv_chan")] public string UVChannels = "XZ";
				[Serialization.SerializeAs("uv_shaderproperty")] public string LinkedShaderPropertyName;
				string UvChannelsOptions = "XYZ";

				// ------------------------------------------------------------------------------------------------
				// UV Other Shader Property mode

				ShaderProperty _linkedShaderProperty;
				public ShaderProperty LinkedShaderProperty
				{
					get { return _linkedShaderProperty; }
					set
					{
						SetLinkedShaderProperty(value);
					}
				}
				public List<ShaderProperty> Dependencies = new List<ShaderProperty>();

				public void TryToFindLinkedShaderProperty()
				{
					if (string.IsNullOrEmpty(LinkedShaderPropertyName))
					{
						return;
					}

					if (ShaderGenerator2.CurrentConfig == null)
					{
						return;
					}

					var match = Array.Find(ShaderGenerator2.CurrentConfig.VisibleShaderProperties, sp => sp.Name == LinkedShaderPropertyName);
					if (match != null)
					{
						SetLinkedShaderProperty(match);
					}
				}

				void SetLinkedShaderProperty(ShaderProperty shaderProperty)
				{
					if (shaderProperty == LinkedShaderProperty)
						return;

					if (shaderProperty == ParentShaderProperty)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg("Shader Property Referenced implementation tried to reference its parent: '" + shaderProperty.Name + "'"));
						return;
					}

					//build dependencies list to check cyclic references
					Dependencies.Clear();
					foreach (var imp in shaderProperty.implementations)
					{
						var impSpRef = imp as Imp_ShaderPropertyReference;
						if (impSpRef != null)
							Dependencies.AddRange(impSpRef.Dependencies);
					}
					if (Dependencies.Contains(shaderProperty))
					{
						//cyclic reference: can happen if a template has incorrect values
						Debug.LogError(ShaderGenerator2.ErrorMsg("Cyclic reference between '" + this.ParentShaderProperty.Name + "' and '" + shaderProperty.Name + "'"));
						return;
					}
					Dependencies.Add(shaderProperty);

					//assign as new linked shader property
					_linkedShaderProperty = shaderProperty;
					LinkedShaderPropertyName = _linkedShaderProperty == null ? "" : _linkedShaderProperty.Name;

					if (shaderProperty == null)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg("Referenced ShaderProperty is null"));
						return;
					}

					//determine default swizzle value based on channels count & linked shader property available channels
					bool sourceIsColor = shaderProperty.Type == VariableType.color || shaderProperty.Type == VariableType.color_rgba;
					string options = sourceIsColor ? "RGBA" : "XYZW";
					switch (shaderProperty.Type)
					{
						case VariableType.@float: UvChannelsOptions = "X"; break;
						case VariableType.float2: UvChannelsOptions = "XY"; break;
						case VariableType.float3: UvChannelsOptions = "XYZ"; break;
						case VariableType.float4: UvChannelsOptions = "XYZW"; break;
						case VariableType.color: UvChannelsOptions = "RGB"; break;
						case VariableType.color_rgba: UvChannelsOptions = "RGBA"; break;
					}

					// set default channels, or preserve existing ones as far as possible (the implementation could have just been deserialized)
					var prevChannels = UVChannels;
					UVChannels = "";
					for (int i = 0; i < 2; i++)
					{
						if (i < prevChannels.Length && options.Contains(prevChannels[i].ToString()))
							UVChannels += prevChannels[i];
						else
							UVChannels += options[i % options.Length];
					}
				}

				void OnSelectShaderProperty(object sp)
				{
					LinkedShaderProperty = sp as ShaderProperty;
					ParentShaderProperty.CheckHash();
					ShaderGenerator2.NeedsHashUpdate = true;
				}

				//Force updating the Shader Property hash once we've retrieved the correct Linked Shader Property
				public void ForceUpdateParentDefaultHash()
				{
					ParentShaderProperty.ForceUpdateDefaultHash();
				}

				// ------------------------------------------------------------------------------------------------

				string DefaultChannels = "RGB";

				ProgramType program = ProgramType.Undefined;
				public bool invalidTilingOffsetVariable = false;
				public bool invalidScrollingVariable = false;

				bool? _uvExpandedCache;
				bool uvExpandedCache
				{
					get
					{
						if(_uvExpandedCache == null)
						{
							_uvExpandedCache = ParentShaderProperty.implementationsExpandedStates.Contains(this.guid.GetHashCode());
						}
						return _uvExpandedCache.Value;
					}
				}

				public Imp_MaterialProperty_Texture(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					program = shaderProperty != null ? shaderProperty.Program : ProgramType.Undefined;
					PropertyName = "_" + SGUILayout.Utils.RemoveWhitespaces(Label);
					Label = shaderProperty.Name + " Texture";

					InitChannelsCount();
					InitChannelsSwizzle();

					//make mip level accessible if vertex program
					if (shaderProperty != null && shaderProperty.Program == ProgramType.Vertex)
					{
						MipLevel = 0;
					}
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: Channels = "R"; break;
						case VariableType.float2: Channels = "RG"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "RGB"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "RGBA"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
					TryToFindLinkedShaderProperty();
				}

				string GetMipValue()
				{
					return MipProperty ? FetchVariable(PropertyName + "_Mip") : MipLevel.ToString();
				}

				public void SetScreenSpaceUV()
				{
					UvSource = UvSourceType.ScreenSpace;
					var uvLabelArray = program == ProgramType.Vertex ? SGUILayout.Constants.UvChannelOptionsVertex : SGUILayout.Constants.UvChannelOptions;
					UvChannel = Array.IndexOf(uvLabelArray, SGUILayout.Constants.screenSpaceUVLabel);
				}

				public void SetWorldPositionUV()
				{
					UvSource = UvSourceType.WorldPosition;
					UvChannelsOptions = "XYZ";
					var uvLabelArray = program == ProgramType.Vertex ? SGUILayout.Constants.UvChannelOptionsVertex : SGUILayout.Constants.UvChannelOptions;
					UvChannel = Array.IndexOf(uvLabelArray, SGUILayout.Constants.worldPosUVLabel);
				}

				public void SetShaderPropertyUV()
				{
					UvSource = UvSourceType.OtherShaderProperty;
					var uvLabelArray = program == ProgramType.Vertex ? SGUILayout.Constants.UvChannelOptionsVertex : SGUILayout.Constants.UvChannelOptions;
					UvChannel = Array.IndexOf(uvLabelArray, SGUILayout.Constants.shaderPropertyUVLabel);
				}

				string GetUV(string input, string output, ProgramType programType)
				{
					if (UvSource == UvSourceType.ScreenSpace)
					{
						return "screenUV";
					}
					else if(UvSource == UvSourceType.WorldPosition)
					{
						bool isURP = ShaderGenerator2.TemplateID == "TEMPLATE_URP" || ShaderGenerator2.TemplateID == "TEMPLATE_LWRP";
						if (program == ProgramType.Vertex)
						{
							return string.Format("{0}.{1}", isURP ? "worldPosUv" : "worldPosUv", UVChannels.ToLowerInvariant());
						}
						else
						{
							// assume Fragment
							return string.Format("{0}.{1}", isURP ? "positionWS" : input + ".worldPos", UVChannels.ToLowerInvariant());
						}
					}
					else if (UvSource == UvSourceType.OtherShaderProperty)
					{
						if (LinkedShaderProperty.IsUsedInLightingFunction && ShaderGenerator2.CurrentPassHasLightingFunction)
							return string.Format("{0}.{1}.{2}", output, LinkedShaderProperty.GetVariableName(), UVChannels.ToLowerInvariant());
						else
							return string.Format("{0}.{1}", LinkedShaderProperty.GetVariableName(), UVChannels.ToLowerInvariant());
					}
					else
					{
						string coord = ShaderGenerator2.VariablesManager.GetVariable("texcoord" + UvChannel);
						if (string.IsNullOrEmpty(coord))
						{
							if (programType == ProgramType.Vertex)
							{
								// no packed variable and in vertex program, so it must be a texcoord only used in the vertex function
								return string.Format("{0}.{1}.xy", input, "texcoord" + UvChannel);
							}
							else
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg("Can't find UV coordinates for shader property: " + ParentShaderProperty.Name));
								return null;
							}
						}
						else
						{
							return string.Format("{0}.{1}.xy", programType == ProgramType.Vertex ? output : input, coord);
						}
					}
				}

				#region Tiling/Offset & Scrolling Variables


				public string GetDefaultTilingOffsetVariable()
				{
					return FetchVariable(GetTilingOffsetVariableName());
				}

				public string GetTilingOffsetVariableName()
				{
					return string.Format("{0}_ST", PropertyName);
				}

				// Uses a tiling/offset variable from another property
				public bool UseCustomTilingOffsetVariable()
				{
					return !string.IsNullOrEmpty(TilingOffsetVariable);
				}

				// Returns true if this property's tiling/offset variable can be referenced
				bool HasValidTilingOffsetVariable()
				{
					return this.UseTilingOffset && !this.GlobalTilingOffset && !this.UseCustomTilingOffsetVariable();
				}


				public string GetDefaultScrollingVariable()
				{
					return FetchVariable(GetScrollingVariableName());
				}

				public string GetScrollingVariableName()
				{
					return string.Format("{0}_SC", PropertyName);
				}

				// Uses a tiling/offset variable from another property
				public bool UseCustomScrollingVariable()
				{
					return !string.IsNullOrEmpty(ScrollingVariable);
				}

				// Returns true if this property's tiling/offset variable can be referenced
				public bool HasValidScrollingVariable()
				{
					return this.UseScrolling && !this.GlobalScrolling && !this.UseCustomScrollingVariable();
				}


				public string GetDefaultOffsetSpeedVariable()
				{
					return FetchVariable(string.Format("{0}_OffsetSpeed", PropertyName));
				}


				/// <summary>
				/// Verify that the tiling/offset & scrolling values are correct if they reference another implementation
				/// </summary>
				void VerifyReferencedValuesValidity()
				{
					invalidTilingOffsetVariable = false;
					if (UseTilingOffset && !string.IsNullOrEmpty(TilingOffsetVariable))
					{
						var availableValues = FetchValidTilingOffsetValues();
						if (!availableValues.Exists(av => av.value == TilingOffsetVariable && string.IsNullOrEmpty(av.disabled)))
						{
							invalidTilingOffsetVariable = true;
						}
					}

					invalidScrollingVariable = false;
					if (UseScrolling && !string.IsNullOrEmpty(ScrollingVariable))
					{
						var availableValues = FetchValidScrollingValues();
						if (!availableValues.Exists(av => av.value == ScrollingVariable && string.IsNullOrEmpty(av.disabled)))
						{
							invalidScrollingVariable = true;
						}
					}
				}

				struct AvailableValue
				{
					public string value;
					public string label;
					public string valueLabel;
					public string disabled;

					public override string ToString()
					{
						return string.Format("[AvailableValue value: {0}, label: {1}, valueLabel: {2}, disabled: {3}]", value, label, valueLabel, disabled);
					}
				}

				/// <summary>
				/// Returns the currently available tiling/offset values
				/// </summary>
				List<AvailableValue> FetchValidTilingOffsetValues()
				{
					return FetchValidValuesGeneric(imp => imp.HasValidTilingOffsetVariable(), imp => imp.GetDefaultTilingOffsetVariable(), imp => imp.GetTilingOffsetVariableName());
				}

				/// <summary>
				/// Returns the currently available tiling/offset values
				/// </summary>
				List<AvailableValue> FetchValidScrollingValues()
				{
					return FetchValidValuesGeneric(imp => imp.HasValidScrollingVariable(), imp => imp.GetDefaultScrollingVariable(), imp => imp.GetScrollingVariableName());
				}

				// Generic function to return available tiling/offset or scrolling variables
				List<AvailableValue> FetchValidValuesGeneric(Func<Imp_MaterialProperty_Texture, bool> checkFunction, Func<Imp_MaterialProperty_Texture, string> valueFunction, Func<Imp_MaterialProperty_Texture, string> valueLabelFunction)
				{
					var list = new List<AvailableValue>();

					if (ShaderGenerator2.CurrentConfig == null || ShaderGenerator2.CurrentConfig.VisibleShaderProperties == null)
					{
						return list;
					}

					foreach (var sp in ShaderGenerator2.CurrentConfig.VisibleShaderProperties)
					{
						foreach (var imp in sp.implementations)
						{
							if (imp == this)
							{
								continue;
							}

							// Check regular texture implementations
							var imp_mp_text = imp as Imp_MaterialProperty_Texture;
							if (imp_mp_text != null)
							{
								if (checkFunction(imp_mp_text))
								{
									list.Add(new AvailableValue()
									{
										value = valueFunction(imp_mp_text),
										label = imp_mp_text.Label,
										valueLabel = valueLabelFunction(imp_mp_text),
										disabled = null
									});
								}
							}
						}
					}

					// Check Custom Material Properties with texture implementation
					foreach (var cmp in ShaderGenerator2.CurrentConfig.CustomMaterialProperties)
					{
						var imp_mp_ct = cmp.implementation as Imp_MaterialProperty_Texture;
						if (imp_mp_ct != null)
						{
							if (checkFunction(imp_mp_ct))
							{
								list.Add(new AvailableValue()
								{
									value = valueFunction(imp_mp_ct),
									label = imp_mp_ct.Label,
									valueLabel = valueLabelFunction(imp_mp_ct),
									disabled = imp_mp_ct.IsCustomMaterialPropertyReferenced() ? null : "(unused Custom Material Property)"
								});
							}
						}
					}

					return list;
				}

				#endregion

				internal override string PrintProperty(string indent)
				{
					var prop = base.PrintProperty(indent) + string.Format("{3}{0} (\"{1}\", 2D) = \"{2}\" {{}}", PropertyName, Label, DefaultValue, UseTilingOffset && !UseCustomTilingOffsetVariable() ? "" : "[NoScaleOffset] ");
					if (UseScrolling && !UseCustomScrollingVariable())
						prop += string.Format("\n{0}[TCP2UVScrolling] {1}_SC (\"{2} UV Scrolling\", Vector) = (1,1,0,0)", indent, PropertyName, Label);
					if (RandomOffset)
						prop += string.Format("\n{0}{1} (\"{2} UV Offset Speed\", Float) = 120", indent, GetDefaultOffsetSpeedVariable(), Label);
					if (MipProperty)
						prop += string.Format("\n{0}{1}_Mip (\"{2} Mip Level\", Range(0,10)) = 0", indent, PropertyName, Label);
					return prop;
				}
				internal override string PrintVariableDeclareOutsideCBuffer(string indent)
				{
					return string.Format("sampler2D {0};", PropertyName);
				}
				internal override string PrintVariableDeclare(string indent)
				{
					string properties = "";
					if (UseTilingOffset && !UseCustomTilingOffsetVariable())
						properties += string.Format("{0}float4 {1}_ST;\n", indent, PropertyName);
					if (ScaleByTexelSize)
						properties += string.Format("{0}float4 {1}_TexelSize;\n", indent, PropertyName);
					if (UseScrolling && !UseCustomScrollingVariable())
						properties += string.Format("{0}float4 {1}_SC;\n", indent, PropertyName);
					if (RandomOffset)
						properties += string.Format("{0}float {1};\n", indent, GetDefaultOffsetSpeedVariable());
					if (MipProperty)
						properties += string.Format("{0}fixed {1}_Mip;\n", indent, PropertyName);
					properties = properties.TrimEnd('\n');
					return string.IsNullOrEmpty(properties) ? null : properties;
				}
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var tilingOffsetVariable = UseCustomTilingOffsetVariable() ? TilingOffsetVariable : GetDefaultTilingOffsetVariable();
					var tilingMod = ScaleByTexelSize ? string.Format(" * {0}_TexelSize.xy", PropertyName) : "";
					if (UvSource == UvSourceType.ScreenSpace)
					{
						tilingMod += ScaleByTexelSize ? " * _ScreenParams.xy" : " * _ScreenParams.zw";
					}
					tilingMod += (UseTilingOffset && (!GlobalTilingOffset || UvSource != UvSourceType.Texcoord)) ? string.Format(" * {0}.xy", tilingOffsetVariable) : "";
					var offsetMod = (UseTilingOffset && (!GlobalTilingOffset || UvSource != UvSourceType.Texcoord)) ? string.Format(" + {0}.zw", tilingOffsetVariable) : "";
					var scrollingVariable = UseCustomScrollingVariable() ? ScrollingVariable : GetDefaultScrollingVariable();
					var scrollingMod = (UseScrolling && !GlobalScrolling) ? string.Format(" + {1}(_Time.yy * {0}.xy)", scrollingVariable, NoTile ? "" : "frac") : "";
					var randomOffsetMod = (RandomOffset && !GlobalRandomOffset) ? string.Format(" + hash22(floor(_Time.xx * {0}.xx) / {0}.xx)", GetDefaultOffsetSpeedVariable()) : "";

					// uv coordinates
					string coords = null;
					if (!string.IsNullOrEmpty(arguments))
					{
						var uv = TryGetArgument("uv", arguments);
						if (uv != null)
						{
							coords = uv;
						}
					}
					if (coords == null)
					{
						coords = GetUV(inputSource, outputSource, ProgramType.Fragment);
					}

					// function
					var function = NoTile ? "tex2D_noTile" : "tex2D";

					// channels
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";

					return string.Format("{0}({1}, {2}{3}{4}{5}{6}){7}", function, PropertyName, coords, tilingMod, scrollingMod, offsetMod, randomOffsetMod, channels);
				}
				internal override string PrintVariableVertex(string inputSource, string outputSource, string arguments)
				{
					var tilingOffsetVariable = UseCustomTilingOffsetVariable() ? TilingOffsetVariable : GetDefaultTilingOffsetVariable();
					var tilingMod = ScaleByTexelSize ? string.Format(" * {0}_TexelSize.xy", PropertyName) : "";
					if (UvSource == UvSourceType.ScreenSpace)
					{
						tilingMod += ScaleByTexelSize ? " * _ScreenParams.xy" : " * _ScreenParams.zw";
					}
					tilingMod += (UseTilingOffset && (!GlobalTilingOffset || UvSource != UvSourceType.Texcoord)) ? string.Format(" * {0}.xy", tilingOffsetVariable) : "";
					var offsetMod = (UseTilingOffset && (!GlobalTilingOffset || UvSource != UvSourceType.Texcoord)) ? string.Format(" + {0}.zw", tilingOffsetVariable) : "";
					var scrollingVariable = UseCustomScrollingVariable() ? ScrollingVariable : GetDefaultScrollingVariable();
					var scrollingMod = (UseScrolling && !GlobalScrolling) ? string.Format(" + {1}(_Time.yy * {0}.xy)", scrollingVariable, NoTile ? "" : "frac") : "";
					var randomOffsetMod = (RandomOffset && !GlobalRandomOffset) ? string.Format(" + hash22(floor(_Time.xx * {0}.xx) / {0}.xx)", GetDefaultOffsetSpeedVariable()) : "";

					// uv coordinates
					string coords = null;
					if (!string.IsNullOrEmpty(arguments))
					{
						var uv = TryGetArgument("uv", arguments);
						if (uv != null)
						{
							coords = uv;
						}
					}
					if (coords == null)
					{
						coords = GetUV(inputSource, outputSource, ProgramType.Vertex);
					}
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("tex2Dlod({0}, float4({1}{2}{3}{4}{5}, 0, {6})){7}", PropertyName, coords, tilingMod, scrollingMod, offsetMod, randomOffsetMod, GetMipValue(), channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						var index = Array.IndexOf(SGUILayout.Constants.DefaultTextureValues, DefaultValue);
						var newIndex = index;
						if (newIndex < 0) newIndex = 0;

						bool highlighted = !IsDefaultImplementation ? DefaultValue != SGUILayout.Constants.DefaultTextureValues[0] : DefaultValue != GetDefaultImplementation<Imp_MaterialProperty_Texture>().DefaultValue;
						SGUILayout.InlineLabel("Default Value", highlighted);
						newIndex = SGUILayout.Popup(newIndex, SGUILayout.Constants.DefaultTextureValues);

						if (newIndex != index)
							DefaultValue = SGUILayout.Constants.DefaultTextureValues[newIndex];
					}
					EndHorizontal();

					if (!IsCustomMaterialProperty)
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_MaterialProperty_Texture>().Channels;
							SGUILayout.InlineLabel("Swizzle", highlighted);

							if (usedByCustomCode)
							{
								using (new EditorGUI.DisabledScope(true))
								{
									GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
								}
							}
							else
							{
								if (ChannelsCount == 1)
									Channels = SGUILayout.RGBASelector(Channels);
								else
									Channels = SGUILayout.RGBASwizzle(Channels, ChannelsCount);
							}
						}
						EndHorizontal();
					}

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? UvChannel > 0 : UvChannel != GetDefaultImplementation<Imp_MaterialProperty_Texture>().UvChannel;
						bool uvExpanded = SGUILayout.Foldout(uvExpandedCache, "UV", highlighted);
						if (uvExpanded != uvExpandedCache)
						{
							if (uvExpanded)
							{
								ParentShaderProperty.implementationsExpandedStates.Add(this.guid.GetHashCode());
							}
							else
							{
								ParentShaderProperty.implementationsExpandedStates.Remove(this.guid.GetHashCode());
							}
							_uvExpandedCache = null;
						}

						if (IsUvLocked)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								SGUILayout.Popup(0, SGUILayout.Constants.LockedUvChannelOptions);
							}
						}
						else
						{
							var uvLabelArray = program == ProgramType.Vertex ? SGUILayout.Constants.UvChannelOptionsVertex : SGUILayout.Constants.UvChannelOptions;
							UvChannel = SGUILayout.Popup(UvChannel, uvLabelArray);
							if (GUI.changed)
							{
								if (Array.IndexOf(uvLabelArray, SGUILayout.Constants.screenSpaceUVLabel) == UvChannel && !IsUvLocked)
								{
									UvSource = UvSourceType.ScreenSpace;
								}
								else if (Array.IndexOf(uvLabelArray, SGUILayout.Constants.worldPosUVLabel) == UvChannel && !IsUvLocked)
								{
									UvSource = UvSourceType.WorldPosition;
									UvChannelsOptions = "XYZ";
								}
								else if (Array.IndexOf(uvLabelArray, SGUILayout.Constants.shaderPropertyUVLabel) == UvChannel && !IsUvLocked)
								{
									UvSource = UvSourceType.OtherShaderProperty;
									UvChannelsOptions = "XXX";
								}
								else
								{
									UvSource = UvSourceType.Texcoord;
								}
							}
						}

						if (UvSource == UvSourceType.WorldPosition || UvSource == UvSourceType.OtherShaderProperty)
						{
							var gc = TCP2_GUI.TempContent(".");
							var rect = GUILayoutUtility.GetRect(gc, EditorStyles.label, GUILayout.ExpandWidth(false));
							rect.y -= 2;
							GUI.Label(rect, gc);
							UVChannels = SGUILayout.GenericSwizzle(UVChannels, 2, UvChannelsOptions, 30, showAvailableChannels: false);
						}
					}
					EndHorizontal();

					if (uvExpandedCache)
					{
						//SGUILayout.Indent += 10;

						bool showScreenSpaceUVOptions = UvSource == UvSourceType.ScreenSpace;
						if (GlobalOptions.data.ShowDisabledFeatures || showScreenSpaceUVOptions)
						{
							using (new EditorGUI.DisabledGroupScope(!showScreenSpaceUVOptions))
							{
								BeginHorizontal();
								{
									bool highlighted = !IsDefaultImplementation ? ScreenSpaceUVVertex : ScreenSpaceUVVertex != GetDefaultImplementation<Imp_MaterialProperty_Texture>().ScreenSpaceUVVertex;
									SGUILayout.InlineLabel("└   Vertex SSUV", "Calculate the screen space UV in the vertex shader, faster but can appear distorted", highlighted);
									ScreenSpaceUVVertex = SGUILayout.Toggle(ScreenSpaceUVVertex);
								}
								EndHorizontal();

								using (new EditorGUI.DisabledGroupScope(ScreenSpaceUVVertex))
								{
									BeginHorizontal();
									{
										bool highlighted = !IsDefaultImplementation ? ScreenSpaceUVObjectOffset : ScreenSpaceUVObjectOffset != GetDefaultImplementation<Imp_MaterialProperty_Texture>().ScreenSpaceUVObjectOffset;
										SGUILayout.InlineLabel("└   Obj Offset SSUV", "Align the UV with the object's pivot, so that the texture doesn't appear fixed on the screen (remove the 'shower door' effect)", highlighted);
										ScreenSpaceUVObjectOffset = SGUILayout.Toggle(ScreenSpaceUVObjectOffset);
									}
									EndHorizontal();
								}
							}
						}

						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? UseTilingOffset : UseTilingOffset != GetDefaultImplementation<Imp_MaterialProperty_Texture>().UseTilingOffset;
							SGUILayout.InlineLabel("Tiling/Offset", highlighted);
							UseTilingOffset = SGUILayout.Toggle(UseTilingOffset);
						}
						EndHorizontal();

						bool showTilingOptions = UseTilingOffset && UvSource == UvSourceType.Texcoord;
						if ((GlobalOptions.data.ShowDisabledFeatures || showTilingOptions) && !IsUvLocked)
						{
							using (new EditorGUI.DisabledGroupScope(!showTilingOptions))
							{
								BeginHorizontal();
								{
									bool highlighted = !IsDefaultImplementation ? GlobalTilingOffset : GlobalTilingOffset != GetDefaultImplementation<Imp_MaterialProperty_Texture>().GlobalTilingOffset;
									SGUILayout.InlineLabel("└   Global", "Makes the tiling/offset values global to the selected UV coordinates: all textures using the same UV coordinates will inherit the tiling/offset values defined for this texture.\nIt also means that they will be calculated in the vertex shader (faster but uses one interpolator).\n\nDoes not apply to screen space UV coordinates.", highlighted);
									GlobalTilingOffset = SGUILayout.Toggle(GlobalTilingOffset);
								}
								EndHorizontal();
							}
						}

						showTilingOptions = UseTilingOffset && !(UvSource == UvSourceType.Texcoord && GlobalTilingOffset);
						if ((GlobalOptions.data.ShowDisabledFeatures || showTilingOptions))
						{
							using (new EditorGUI.DisabledGroupScope(!showTilingOptions))
							{
								BeginHorizontal();
								{
									bool highlighted = !IsDefaultImplementation ? UseCustomTilingOffsetVariable() : TilingOffsetVariable != GetDefaultImplementation<Imp_MaterialProperty_Texture>().TilingOffsetVariable;
									SGUILayout.InlineLabel("└   Variable", "Defines the tiling/offset uniform variable.\nBy default, a new property will be created for this texture, however you can use another texture's tiling/offset variable so that this texture will be linked with it. You would typically do that if you have a normal map coupled with an albedo map, for example.", highlighted);
									var tilingOffsetVar = UseCustomTilingOffsetVariable() ? TilingOffsetVariableLabel : GetTilingOffsetVariableName();
									if (SGUILayout.ButtonPopup(tilingOffsetVar))
									{
										var menu = new GenericMenu();
										string label = string.Format("{0}: {1}", ParentShaderProperty.Name, GetTilingOffsetVariableName()); // note: has non-breaking space character
										if (ParentShaderProperty.Name == "_CustomMaterialPropertyDummy") // TODO get rid of the dummy shader property for custom material properties?
										{
											label = GetTilingOffsetVariableName();
										}

										menu.AddItem(new GUIContent(label), !UseCustomTilingOffsetVariable(), () =>
										{
											TilingOffsetVariable = "";
											TilingOffsetVariableLabel = "";
											invalidTilingOffsetVariable = false;
										});

										// fetch available tiling/offset values and add them to the menu
										var itemList = new List<MenuItem>();
										var availableValues = FetchValidTilingOffsetValues();
										foreach(var availableValue in availableValues)
										{
											if (availableValue.label == this.Label)
											{
												continue;
											}

											if (string.IsNullOrEmpty(availableValue.disabled))
											{
												itemList.Add(new MenuItem()
												{
													guiContent = new GUIContent(string.Format("{0}: {1}", availableValue.label, availableValue.valueLabel)), // note: has non-breaking space character
													on = this.TilingOffsetVariable == availableValue.value,
													menuFunction = () =>
													{
														TilingOffsetVariable = availableValue.value;
														TilingOffsetVariableLabel = availableValue.valueLabel;
														invalidTilingOffsetVariable = false;
													}
												});
											}
											else
											{
												itemList.Add(new MenuItem()
												{
													guiContent = new GUIContent(string.Format("{0}: {1} {2}", availableValue.label, availableValue.valueLabel, availableValue.disabled)), // note: has non-breaking space character
													on = this.TilingOffsetVariable == availableValue.value,
													disabled = true
												});
											}
										}

										if (itemList.Count > 0)
										{
											menu.AddSeparator("");
											foreach (var item in itemList)
											{
												if (item.disabled)
												{
													menu.AddDisabledItem(item.guiContent);
												}
												else
												{
													menu.AddItem(item.guiContent, item.on, item.menuFunction);
												}
											}
										}

										menu.ShowAsContext();
									}
								}
								EndHorizontal();

								BeginHorizontal();
								{
									bool highlighted = !IsDefaultImplementation ? ScaleByTexelSize : ScaleByTexelSize != GetDefaultImplementation<Imp_MaterialProperty_Texture>().ScaleByTexelSize;
									SGUILayout.InlineLabel("└   Scale by Texel Size", "Will scale the UV by the texture's texel size. Usually useful to get pixel-perfect screen space UV mapping on the screen.", highlighted);
									ScaleByTexelSize = SGUILayout.Toggle(ScaleByTexelSize);
								}
								EndHorizontal();
							}
						}

						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? UseScrolling || RandomOffset : (UseScrolling != GetDefaultImplementation<Imp_MaterialProperty_Texture>().UseScrolling || RandomOffset != GetDefaultImplementation<Imp_MaterialProperty_Texture>().RandomOffset);
							SGUILayout.InlineLabel("UV Animation", highlighted);
							int choice = UseScrolling ? 1 : (RandomOffset ? 2 : 0);
							int new_choice = SGUILayout.Popup(choice, SGUILayout.Constants.UvAnimationOptions);
							if (new_choice != choice)
							{
								UseScrolling = false;
								RandomOffset = false;

								switch (new_choice)
								{
									case 1: UseScrolling = true; break;
									case 2: RandomOffset = true; break;
								}
							}
						}
						EndHorizontal();

						bool showScrollingOptions = (UseScrolling || RandomOffset) && UvSource == UvSourceType.Texcoord;
						if ((GlobalOptions.data.ShowDisabledFeatures || showScrollingOptions) && !IsUvLocked)
						{
							using (new EditorGUI.DisabledGroupScope(!showScrollingOptions))
							{
								if (UseScrolling)
								{
									BeginHorizontal();
									{
										bool highlighted = !IsDefaultImplementation ? GlobalScrolling : GlobalScrolling != GetDefaultImplementation<Imp_MaterialProperty_Texture>().GlobalScrolling;
										SGUILayout.InlineLabel("└   Global", "Makes the scrolling global to the selected UV coordinates: all textures using the same UV coordinates will inherit the scrolling animation and values defined for this texture.\nIt also means that they will be calculated in the vertex shader (faster but uses one interpolator).", highlighted);
										GlobalScrolling = SGUILayout.Toggle(GlobalScrolling);
										GlobalRandomOffset = GlobalScrolling;
									}
									EndHorizontal();

									bool showScrollingVariable = UseScrolling && !(UvSource == UvSourceType.Texcoord && GlobalScrolling);
									if ((GlobalOptions.data.ShowDisabledFeatures || showScrollingVariable))
									{
										using (new EditorGUI.DisabledGroupScope(!showScrollingVariable))
										{
											BeginHorizontal();
											{
												bool highlighted = !IsDefaultImplementation ? UseCustomScrollingVariable() : ScrollingVariable != GetDefaultImplementation<Imp_MaterialProperty_Texture>().ScrollingVariable;
												SGUILayout.InlineLabel("└   Variable", "Defines the scrolling uniform variable.\nBy default, a new property will be created for this texture, however you can use another texture's scrolling variable so that this texture will be linked with it.", highlighted);
												var scrollingVar = UseCustomScrollingVariable() ? ScrollingVariableLabel : GetScrollingVariableName();
												if (SGUILayout.ButtonPopup(scrollingVar))
												{
													var menu = new GenericMenu();
													string label = string.Format("{0}: {1}", ParentShaderProperty.Name, GetScrollingVariableName());
													if (ParentShaderProperty.Name == "_CustomMaterialPropertyDummy") // TODO get rid of the dummy shader property for custom material properties
													{
														label = GetScrollingVariableName();
													}

													menu.AddItem(new GUIContent(label), !UseCustomScrollingVariable(), () =>
													{
														ScrollingVariable = "";
														ScrollingVariableLabel = "";
														invalidScrollingVariable = false;
													});

													// fetch available scrolling values and add them to the menu
													var itemList = new List<MenuItem>();
													var availableValues = FetchValidScrollingValues();
													foreach (var availableValue in availableValues)
													{
														if (availableValue.label == this.Label)
														{
															continue;
														}

														if (string.IsNullOrEmpty(availableValue.disabled))
														{
															itemList.Add(new MenuItem()
															{
																guiContent = new GUIContent(string.Format("{0}: {1}", availableValue.label, availableValue.valueLabel)), // note: has non-breaking space character
																on = this.ScrollingVariable == availableValue.value,
																menuFunction = () =>
																{
																	ScrollingVariable = availableValue.value;
																	ScrollingVariableLabel = availableValue.valueLabel;
																	invalidScrollingVariable = false;
																}
															});
														}
														else
														{
															itemList.Add(new MenuItem()
															{
																guiContent = new GUIContent(string.Format("{0}: {1} {2}", availableValue.label, availableValue.valueLabel, availableValue.disabled)), // note: has non-breaking space character
																on = this.ScrollingVariable == availableValue.value,
																disabled = true
															});
														}
													}

													if (itemList.Count > 0)
													{
														menu.AddSeparator("");
														foreach (var item in itemList)
														{
															if (item.disabled)
															{
																menu.AddDisabledItem(item.guiContent);
															}
															else
															{
																menu.AddItem(item.guiContent, item.on, item.menuFunction);
															}
														}
													}

													menu.ShowAsContext();
												}
											}
											EndHorizontal();
										}
									}
								}
								else if (RandomOffset)
								{
									BeginHorizontal();
									{
										bool highlighted = !IsDefaultImplementation ? GlobalRandomOffset : GlobalRandomOffset != GetDefaultImplementation<Imp_MaterialProperty_Texture>().GlobalRandomOffset;
										SGUILayout.InlineLabel("└   Global", "Makes the random offset global to the selected UV coordinates: all textures using the same UV coordinates will inherit the random offset animation and values defined for this texture.\nIt also means that they will be calculated in the vertex shader (faster but uses one interpolator).", highlighted);
										GlobalRandomOffset = SGUILayout.Toggle(GlobalRandomOffset);
										GlobalScrolling = GlobalRandomOffset;
									}
									EndHorizontal();
								}
							}
						}

						using (new EditorGUI.DisabledGroupScope(program != ProgramType.Fragment && !IsCustomMaterialProperty))
						{
							BeginHorizontal();
							{
								bool highlighted = !IsDefaultImplementation ? NoTile : NoTile != GetDefaultImplementation<Imp_MaterialProperty_Texture>().NoTile;
								SGUILayout.InlineLabel("No Tile", "Use a special algorithm to prevent tile repetition", highlighted);
								NoTile = SGUILayout.Toggle(NoTile);
							}
							EndHorizontal();
						}

						if (NoTile && UseScrolling && GlobalScrolling)
						{
							TCP2_GUI.HelpBoxLayout("'Global Scrolling' and 'No Tile' don't work properly together: expect to see textures popping in their animation.", MessageType.Warning);
						}

						if (MipLevel >= 0 || IsCustomMaterialProperty)
						{
							BeginHorizontal();
							{
								bool highlighted = !IsDefaultImplementation ? MipLevel > 0 : MipLevel != GetDefaultImplementation<Imp_MaterialProperty_Texture>().MipLevel;
								SGUILayout.InlineLabel("Vertex Sampling Mip Level", highlighted);
								using (new EditorGUI.DisabledScope(MipProperty))
									MipLevel = SGUILayout.IntField(MipLevel, 0, 10);
							}
							EndHorizontal();

							BeginHorizontal();
							{
								bool highlighted = !IsDefaultImplementation ? MipProperty : MipProperty != GetDefaultImplementation<Imp_MaterialProperty_Texture>().MipProperty;
								SGUILayout.InlineLabel("└   Material Property", "Create a material property to control the mip level for sampling this texture in the vertex shader", highlighted);
								MipProperty = SGUILayout.Toggle(MipProperty);
							}
							EndHorizontal();
						}

						//SGUILayout.Indent -= 10;
					} // if ( uvExpanded )

					if (UvSource == UvSourceType.OtherShaderProperty)
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? false : LinkedShaderPropertyName != GetDefaultImplementation<Imp_MaterialProperty_Texture>().LinkedShaderPropertyName;
							SGUILayout.InlineLabel("UV Shader Property", highlighted);

							if (GUILayout.Button((LinkedShaderProperty != null) ? LinkedShaderProperty.Name : "None", SGUILayout.Styles.ShurikenPopup))
							{
								var menu = ShaderProperty.Imp_ShaderPropertyReference.CreateShaderPropertiesMenu(this.ParentShaderProperty, this.LinkedShaderProperty, OnSelectShaderProperty);
								if (menu != null)
								{
									menu.ShowAsContext();
								}
							}
						}
						EndHorizontal();
						GUILayout.Space(3);

						// linked shader property errors
						if (_linkedShaderProperty == null)
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("No Shader Property defined.", MessageType.Error);
							}
							EndHorizontal();
						}
						else if (!_linkedShaderProperty.IsVisible())
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("Invalid Shader Property defined.", MessageType.Error);
							}
							EndHorizontal();
						}
					}

					// errors

					if (UseTilingOffset && invalidTilingOffsetVariable)
					{
						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout("The UV Tiling/Offset Variable is invalid.\nMaybe the original source has been removed or can't be used anymore?", MessageType.Error);
						}
						EndHorizontal();
					}

					if (UseScrolling && invalidScrollingVariable)
					{
						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout("The UV Scrolling Variable is invalid.\nMaybe the original source has been removed or can't be used anymore?", MessageType.Error);
						}
						EndHorizontal();
					}
				}
			}

			[Serialization.SerializeAs("imp_constant")]
			public class Imp_ConstantValue : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll | VariableType.fixed_function_float; } }
				public static string MenuLabel { get { return "Constant Value"; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("type"), ExcludeFromCopy] VariableType type;
				[Serialization.SerializeAs("fprc")] FloatPrecision floatPrec;

				[Serialization.SerializeAs("fv")] public float FloatValue = 1.0f;
				[Serialization.SerializeAs("f2v")] public Vector2 Float2Value = Vector2.one;
				[Serialization.SerializeAs("f3v")] public Vector3 Float3Value = Vector3.one;
				[Serialization.SerializeAs("f4v")] public Vector4 Float4Value = Vector4.one;
				[Serialization.SerializeAs("cv")] public Color ColorValue = Color.white;

				public Imp_ConstantValue(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					type = shaderProperty.Type;
					floatPrec = FloatPrecision.@float;
				}

				internal override string PrintVariableFixedFunction()
				{
					return FloatValue.ToString();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					switch (type)
					{
						case VariableType.@float: return FloatValue.ToString("#.0###############", CultureInfo.InvariantCulture);
						case VariableType.float2: return string.Format(CultureInfo.InvariantCulture, "{0}2({1},{2})", floatPrec, Float2Value.x, Float2Value.y);
						case VariableType.float3: return string.Format(CultureInfo.InvariantCulture, "{0}3({1},{2},{3})", floatPrec, Float3Value.x, Float3Value.y, Float3Value.z);
						case VariableType.float4: return string.Format(CultureInfo.InvariantCulture, "{0}4({1},{2},{3},{4})", floatPrec, Float4Value.x, Float4Value.y, Float4Value.z, Float4Value.w);
						case VariableType.color: return string.Format(CultureInfo.InvariantCulture, "{0}3({1},{2},{3})", floatPrec, ColorValue.r, ColorValue.g, ColorValue.b);
						case VariableType.color_rgba: return string.Format(CultureInfo.InvariantCulture, "{0}4({1},{2},{3},{4})", floatPrec, ColorValue.r, ColorValue.g, ColorValue.b, ColorValue.a);
					}

					return null;
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Uses a constant value in the shader.\nIf your shader property will keep the same value, this will be faster than using a Material Property.");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = false;
						switch(type)
						{
							case VariableType.@float:
							case VariableType.fixed_function_float:
								highlighted = !IsDefaultImplementation ? FloatValue != 1.0f : FloatValue != GetDefaultImplementation<Imp_ConstantValue>().FloatValue;
								break;

							case VariableType.float2:
								highlighted = !IsDefaultImplementation ? FloatValue != 1.0f : FloatValue != GetDefaultImplementation<Imp_ConstantValue>().FloatValue;
								break;
							case VariableType.float3:
								highlighted = !IsDefaultImplementation ? Float2Value != Vector2.one : Float2Value != GetDefaultImplementation<Imp_ConstantValue>().Float2Value;
								break;
							case VariableType.float4:
								highlighted = !IsDefaultImplementation ? Float3Value != Vector3.one : Float3Value != GetDefaultImplementation<Imp_ConstantValue>().Float3Value;
								break;
							case VariableType.color:
							case VariableType.color_rgba:
								highlighted = !IsDefaultImplementation ? ColorValue != Color.white : ColorValue != GetDefaultImplementation<Imp_ConstantValue>().ColorValue;
								break;
						}

						SGUILayout.InlineLabel("Value", highlighted);

						switch (type)
						{
							case VariableType.@float:
							case VariableType.fixed_function_float:
								FloatValue = SGUILayout.FloatField(FloatValue);
								break;
							case VariableType.float2:
								Float2Value = SGUILayout.Vector2Field(Float2Value);
								break;
							case VariableType.float3:
								Float3Value = SGUILayout.Vector3Field(Float3Value);
								break;
							case VariableType.float4:
								Float4Value = SGUILayout.Vector4Field(Float4Value);
								break;
							case VariableType.color:
								ColorValue = SGUILayout.ColorField(ColorValue, false, floatPrec != FloatPrecision.@fixed);
								break;
							case VariableType.color_rgba:
								ColorValue = SGUILayout.ColorField(ColorValue, true, floatPrec != FloatPrecision.@fixed);
								break;
						}
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? floatPrec != default(FloatPrecision) : floatPrec != GetDefaultImplementation<Imp_ConstantValue>().floatPrec;
						SGUILayout.InlineLabel("Precision", highlighted);
						floatPrec = (FloatPrecision)SGUILayout.EnumPopup(floatPrec);
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_constant_float")]
			public class Imp_ConstantFloat : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Constant Float"; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("fprc")] FloatPrecision floatPrec;

				[Serialization.SerializeAs("fv")] public float FloatValue = 1.0f;

				public Imp_ConstantFloat(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					floatPrec = FloatPrecision.@float;
				}

				internal override string PrintVariableFixedFunction()
				{
					return FloatValue.ToString();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					return FloatValue.ToString("#.0###############", CultureInfo.InvariantCulture);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Uses a constant value in the shader.\nIf your shader property will keep the same value, this will be faster than using a Material Property.");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? FloatValue != 1.0f : FloatValue != GetDefaultImplementation<Imp_ConstantFloat>().FloatValue;
						SGUILayout.InlineLabel("Value", highlighted);
						FloatValue = SGUILayout.FloatField(FloatValue);
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? floatPrec != default(FloatPrecision) : floatPrec != GetDefaultImplementation<Imp_ConstantFloat>().floatPrec;
						SGUILayout.InlineLabel("Precision", highlighted);
						floatPrec = (FloatPrecision)SGUILayout.EnumPopup(floatPrec);
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_vcolors")]
			public class Imp_VertexColor : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Vertex Color"; } }
				internal override string GUILabel() { return MenuLabel; }
				internal override OptionFeatures[] NeededFeatures() { return new[] { OptionFeatures.VertexColors }; }

				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "RGB";
				string DefaultChannels = "RGB";

				public Imp_VertexColor(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: Channels = "R"; break;
						case VariableType.float2: Channels = "RG"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "RGB"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "RGBA"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}.vertexColor{1}", inputSource, channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Fetch the mesh's vertex colors.");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_VertexColor>().Channels;
						SGUILayout.InlineLabel("Swizzle", highlighted);

						if (usedByCustomCode)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
							}
						}
						else
						{
							if (ChannelsCount == 1)
								Channels = SGUILayout.RGBASelector(Channels);
							else
								Channels = SGUILayout.RGBASwizzle(Channels, ChannelsCount);
						}
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_texcoord")]
			public class Imp_VertexTexcoord : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Vertex UV"; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("tex")] public int TexcoordChannel = 0;
				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "XYZ";
				string DefaultChannels = "XYZ";

				public Imp_VertexTexcoord(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: Channels = "X"; break;
						case VariableType.float2: Channels = "XY"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "XYZ"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "XYZW"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}.texcoord{1}{2}", inputSource, TexcoordChannel, channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Fetch the mesh's specified UV coordinates.");
					EndHorizontal();

					BeginHorizontal();
					{
						EditorGUI.BeginChangeCheck();
						bool highlighted = !IsDefaultImplementation ? TexcoordChannel  > 0 : TexcoordChannel != GetDefaultImplementation<Imp_VertexTexcoord>().TexcoordChannel;
						SGUILayout.InlineLabel("UV Channel", highlighted);
						char newTecoordChannel = SGUILayout.GenericSelector("0123", (char)(TexcoordChannel + '0'));
						if (EditorGUI.EndChangeCheck())
						{
							TexcoordChannel = newTecoordChannel - '0';
						}
					}
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_VertexTexcoord>().Channels;
						SGUILayout.InlineLabel("Swizzle", highlighted);

						if (usedByCustomCode)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
							}
						}
						else
						{
							if (ChannelsCount == 1)
								Channels = SGUILayout.XYZWSelector(Channels);
							else
								Channels = SGUILayout.XYZWSwizzle(Channels, ChannelsCount);
						}
					}
					EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_worldpos")]
			public class Imp_WorldPosition : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "World Position"; } }
				internal override string GUILabel() { return MenuLabel; }
				internal override OptionFeatures[] NeededFeatures() { return new[] { ParentShaderProperty.Program == ProgramType.Vertex ? OptionFeatures.World_Pos_UV_Vertex : OptionFeatures.World_Pos_UV_Fragment }; }

				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "XYZ";
				string DefaultChannels = "XYZ";

				public Imp_WorldPosition(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				void InitChannelsSwizzle()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: Channels = "X"; break;
						case VariableType.float2: Channels = "XY"; break;
						case VariableType.color:
						case VariableType.float3: Channels = "XYZ"; break;
						case VariableType.color_rgba:
						case VariableType.float4: Channels = "XYZW"; break;
					}
					DefaultChannels = Channels;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("worldPosUv{1}", inputSource, channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("The world space position for the current vertex or fragment.");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_WorldPosition>().Channels;
						SGUILayout.InlineLabel("Swizzle", highlighted);

						if (usedByCustomCode)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
							}
						}
						else
						{
							if (ChannelsCount == 1)
								Channels = SGUILayout.XYZSelector(Channels);
							else
								Channels = SGUILayout.XYZSwizzle(Channels, ChannelsCount);
						}
					}
					EndHorizontal();
				}
			}

			// Generic Implementation that is generated inside the Templates.
			// Originally made to add support for NDL, NDV implementations.
			[Serialization.SerializeAs("imp_generic")]
			public class Imp_GenericFromTemplate : Implementation
			{
				/// <summary>
				/// Represents a Generic Implementation model defined from a template
				/// </summary>
				public struct GenericImplementation
				{
					public bool valid; // replacement for null checks
					public string identifier;
					public bool available;
					public int pass;
					public string MenuLabel;
					public string HelpMessage;
					public VariableType Compatibility;
					public string VariableName;
					public string ChannelsOptions;
					public string NeededFeatures;
					public string Options;
					public List<ShaderProperty> compatibleShaderProperties;
					public bool WorksWithCustomCode;

					public Imp_GenericFromTemplate CreateImplementation(ShaderProperty shaderProperty)
					{
						var imp = new Imp_GenericFromTemplate(shaderProperty);

						// copy properties
						imp.ChannelsOptions = this.ChannelsOptions;
						imp.MenuLabel = this.MenuLabel;
						imp.HelpMessage = this.HelpMessage;
						imp.Compatibility = this.Compatibility;
						imp.VariableName = this.VariableName;
						imp.NeededFeaturesStr = this.NeededFeatures;
						imp.WorksWithCustomCode = this.WorksWithCustomCode;
						imp.OptionsString = this.Options;
						imp.ParseOptions();

						// identification based on available Generic Implementations
						imp.sourceIdentifier = this.identifier;
						imp.sourceIsAvailable = true;
						imp.Register();

						imp.DeduceChannelsSettings(shaderProperty);

						return imp;
					}
				}

				// List of currently available generic implementations parsed from the current template
				public static List<GenericImplementation> AvailableGenericImplementations;

				public static void InitList()
				{
					AvailableGenericImplementations = new List<GenericImplementation>();
				}

				public static void EnableFromLine(string line, int pass, string program)
				{
					// format example: #ENABLE_IMPL: float ndl, "Special/N·L (diffuse lighting)", all
					string lineWithoutHeader = line.Substring(line.IndexOf(':')+1).Trim();
					string[] data = Serialization.SplitExcludingBlocks(lineWithoutHeader, ',', true);

					string id = data[0] + pass + program;

					// enable existing
					var existing = AvailableGenericImplementations.Find(x => x.identifier == id);
					if (existing.valid)
					{
						existing.available = true;
						return;
					}

					// create and enable new

					// - first data is "type name"
					int space = data[0].IndexOf(' ');
					string type = data[0].Substring(0, space);
					string name = data[0].Substring(space+1);

					string label = "No Label";
					string compatibility = "all";
					string help = null;
					string neededFeatures = "";
					string options = "";
					bool customCodeCompatible = false;

					// - remaining data is "key = value" pairs
					for (int i = 1; i < data.Length; i++)
					{
						var subdata = data[i].Split('=');
						var subdata1 = subdata[1].Trim(' ', '"');
						switch (subdata[0].Trim())
						{
							case "lbl": label = subdata1; break;
							case "compat": compatibility = subdata1; break;
							case "help": help = subdata1; break;
							case "toggles": neededFeatures = subdata1; break;
							case "options": options = subdata1; break;
							case "custom_code_compatible": customCodeCompatible = bool.Parse(subdata1); break;
						}
					}

					var imp = new GenericImplementation()
					{
						valid = true,
						identifier = data[0] + pass + program,
						available = true,
						pass = pass,
						MenuLabel = label,
						HelpMessage = help,
						Compatibility = (compatibility == "all") ? VariableTypeAll : (VariableType)Enum.Parse(typeof(VariableType), compatibility),
						VariableName = name,
						ChannelsOptions = GetChannelsOption(type),
						NeededFeatures = neededFeatures,
						Options = options,
						compatibleShaderProperties = new List<ShaderProperty>(),
						WorksWithCustomCode = customCodeCompatible

					};
					AvailableGenericImplementations.Add(imp);
				}

				static string GetChannelsOption(string type)
				{
					switch (type)
					{
						case "float": return "X";
						case "float2": return "XY";
						case "float3": return "XYZ";
						case "float4": return "XYZW";
						case "color": return "RGB";
						case "color_rgba": return "RGBA";
					}
					return null;
				}

				public static void DisableFromLine(string line, int pass, string program)
				{
					bool found = false;
					var id = line.Substring(line.IndexOf(':')+1).Trim() + pass + program;

					for (int i = 0; i < AvailableGenericImplementations.Count; i++)
					{
						var imp = AvailableGenericImplementations[i];

						if (imp.identifier == id)
						{
							imp.available = false;
							AvailableGenericImplementations[i] = imp;
							found = true;
						}
					}

					if (!found)
					{
						Debug.LogWarning(ShaderGenerator2.ErrorMsg("Can't disable Generic Implementation with this identifier: " + id));
					}
				}

				public static void DisableAll()
				{
					for (int i = 0; i < AvailableGenericImplementations.Count; i++)
					{
						var imp = AvailableGenericImplementations[i];
						imp.available = false;
						AvailableGenericImplementations[i] = imp;
					}
				}

				public delegate void OnGenericImplementationsChanged();
				static public OnGenericImplementationsChanged onGenericImplementationsChanged;

				/// <summary>
				/// Triggers a warning if some generic implementations weren't disabled in the template,
				/// and sends event that the available generic implementations may have changed
				/// </summary>
				public static void ListCompleted()
				{
					if (onGenericImplementationsChanged != null)
					{
						onGenericImplementationsChanged();
					}

					// check not disabled in the template
					string notDisabled = "";
					foreach (var imp in AvailableGenericImplementations)
					{
						if (imp.available)
						{
							notDisabled += imp + ", ";
						}
					}
					if (notDisabled.Length > 0)
					{
						notDisabled = notDisabled.Substring(0, notDisabled.Length - 2);
						Debug.LogWarning(ShaderGenerator2.ErrorMsg("Some Generic Implementations were not properly disabled in the template: " + notDisabled));
					}
				}

				/// <summary>
				/// Adds the Shader Property as compatible with all currently available Generic Implementations
				/// </summary>
				public static void AddCompatibleShaderProperty(ShaderProperty shaderProperty)
				{
					foreach (var imp in AvailableGenericImplementations)
					{
						if (!imp.available)
						{
							continue;
						}

						if ((imp.Compatibility & shaderProperty.Type) == shaderProperty.Type)
						{
							imp.compatibleShaderProperties.Add(shaderProperty);
						}
					}
				}

				//--------------------------------------------------------------------------------------------------------------------------------

				public VariableType VariableCompatibility { get { return Compatibility; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("cc")] public int ChannelsCount = 1;
				[Serialization.SerializeAs("chan")] public string Channels = "X";
				[Serialization.SerializeAs("source_id")] public string sourceIdentifier;
				[Serialization.SerializeAs("needed_features")] public string NeededFeaturesStr = "";
				[Serialization.SerializeAs("custom_code_compatible")] public bool WorksWithCustomCode = false;
				public string OptionsString = "";
				[Serialization.SerializeAs("options_v")] public Dictionary<string, bool> OptionsEnabled = new Dictionary<string, bool>();
				
				string DefaultChannels = "X";

				static Dictionary<string, List<Imp_GenericFromTemplate>> AllGenericImplementations = new Dictionary<string, List<Imp_GenericFromTemplate>>();

				List<Option> options;
				struct Option
				{
					public string label;
					public string feature;
					public bool affectConfig;

					public void UpdateConfigIfNeeded(bool enabled)
					{
						if (this.affectConfig)
						{
							if (enabled)
							{
								Utils.AddIfMissing(ShaderGenerator2.CurrentConfig.ExtraTempFeatures, this.feature);
							}
							else
							{
								Utils.RemoveIfExists(ShaderGenerator2.CurrentConfig.ExtraTempFeatures, this.feature);
							}
						}
					}
				}

				// Generic Implementations that have the same identifier should have their options synchronized
				void SynchronizeOptions()
				{
					if (options == null)
					{
						return;
					}

					// the list does exist since this should be in there
					var list = AllGenericImplementations[sourceIdentifier];
					foreach (var imp in list)
					{
						if (imp != this)
						{
							SynchronizeOptions(this, imp);
						}
					}
				}

				static void SynchronizeOptions(Imp_GenericFromTemplate source, Imp_GenericFromTemplate destination)
				{
					destination.OptionsEnabled = new Dictionary<string, bool>();
					foreach (var kvp in source.OptionsEnabled)
					{
						destination.OptionsEnabled.Add(kvp.Key, kvp.Value);
					}
				}

				// Register in the static dictionary of Generic Implementations to synchronize their options
				void Register()
				{
					if (options == null)
					{
						return;
					}

					if (!AllGenericImplementations.ContainsKey(sourceIdentifier))
					{
						AllGenericImplementations.Add(sourceIdentifier, new List<Imp_GenericFromTemplate>());
					}
					else
					{
						// if there's at least one, copy its setting to sync this new instance to the other ones
						if (AllGenericImplementations[sourceIdentifier].Count > 0)
						{
							SynchronizeOptions(AllGenericImplementations[sourceIdentifier][0], this);
						}
					}

					AllGenericImplementations[sourceIdentifier].Add(this);
				}

				bool sourceIsAvailable;
				bool isTheOnlyImplementation;
				bool isNotTheLastImplementation;

				// These are determined from the template, and are not serialized in case they are updated in the template:
				public string MenuLabel;
				public string HelpMessage;
				public VariableType Compatibility;
				public string VariableName;
				public string ChannelsOptions = "XYZW";

				internal override string[] NeededFeaturesExtra()
				{
					var list = new List<string>();

					if (!string.IsNullOrEmpty(NeededFeaturesStr))
					{
						list.AddRange(NeededFeaturesStr.Split(','));
					}

					if (options != null)
					{
						foreach (var option in options)
						{
							if (OptionsEnabled.ContainsKey(option.label) && OptionsEnabled[option.label])
							{
								list.Add(option.feature);
							}
						}
					}

					return list.ToArray();
				}

				public override bool HasErrors { get { return base.HasErrors || !sourceIsAvailable || isNotTheLastImplementation; } }

				public Imp_GenericFromTemplate(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					onGenericImplementationsChanged += CheckSourceValidity;
					shaderProperty.onImplementationsChanged += CheckImplementationValidity;
				}

				public override void WillBeRemoved()
				{
					base.WillBeRemoved();
					onGenericImplementationsChanged -= CheckSourceValidity;
					ParentShaderProperty.onImplementationsChanged -= CheckImplementationValidity;

					if (options != null)
					{
						foreach (var option in options)
						{
							option.UpdateConfigIfNeeded(false);
						}
					}

					if (options != null)
					{
						AllGenericImplementations[sourceIdentifier].Remove(this);
					}
				}

				[Serialization.OnDeserializeCallback]
				void OnDeserialized()
				{
					// get the options from the template and not from serialization, in case options are added in the future
					var match = Imp_GenericFromTemplate.AvailableGenericImplementations.Find(gi => gi.identifier == this.sourceIdentifier);
					if (match.valid)
					{
						OptionsString = match.Options;
					}

					ParseOptions();
					Register();
					SynchronizeOptions();
					CheckSourceValidity();
				}

				void ParseOptions()
				{
					if (string.IsNullOrEmpty(OptionsString))
					{
						return;
					}

					options = new List<Option>();

					var data = Serialization.SplitExcludingBlocks(OptionsString, ',', "()");
					foreach (var d in data)
					{
						var subdata = d.Substring(1, d.Length-2).Split(',');
						var option = new Option()
						{
							label = subdata[0],
							feature = subdata[1],
							affectConfig = subdata.Length > 2 && subdata[2] == "config"
						};
						options.Add(option);

						if (!OptionsEnabled.ContainsKey(option.label))
						{
							OptionsEnabled.Add(option.label, false);
						}

						option.UpdateConfigIfNeeded(OptionsEnabled[option.label]);
					}
				}

				void CheckSourceValidity()
				{
					// check whether the source implementation is still available
					var source = AvailableGenericImplementations.Find(gi => gi.identifier == sourceIdentifier);

					sourceIsAvailable = source.valid;

					if (source.valid)
					{
						this.MenuLabel = source.MenuLabel;
						this.HelpMessage = source.HelpMessage;
						this.ChannelsOptions = source.ChannelsOptions;
						this.Compatibility = source.Compatibility;
						this.VariableName = source.VariableName;

						this.DeduceChannelsSettings(ParentShaderProperty);
					}
				}

				void DeduceChannelsSettings(ShaderProperty shaderProperty)
				{
					// deduce the channels/count based on the shader property
					switch (shaderProperty.Type)
					{
						case VariableType.@float: this.ChannelsCount = 1; break;
						case VariableType.float2: this.ChannelsCount = 2; break;
						case VariableType.float3:
						case VariableType.color: this.ChannelsCount = 3; break;
						case VariableType.float4:
						case VariableType.color_rgba: this.ChannelsCount = 4; break;
					}

					string defaultChannels = "";
					for (int i = 0; i < this.ChannelsCount; i++)
					{
						defaultChannels += this.ChannelsOptions[i % this.ChannelsOptions.Length];
					}
					this.DefaultChannels = defaultChannels;

					// set Channels, or preserve existing ones as far as possible
					var prevChannels = Channels;
					Channels = "";
					for (int i = 0; i < ChannelsCount; i++)
					{
						if (prevChannels != null && i < prevChannels.Length && this.ChannelsOptions.Contains(prevChannels[i].ToString()))
							Channels += prevChannels[i];
						else
							Channels += this.ChannelsOptions[i % this.ChannelsOptions.Length];
					}
				}

				/// <summary>
				/// Verifies that this generic implementation isn't the only one, and is at the end of the implementations list
				/// </summary>
				void CheckImplementationValidity()
				{
					isTheOnlyImplementation = ParentShaderProperty.implementations.Count == 1;

					// iterate through the implementations, and see if any implementation past this one is not a Generic one
					isNotTheLastImplementation = false;
					bool reachedThis = false;
					foreach (var imp in ParentShaderProperty.implementations)
					{
						if (imp == this)
						{
							reachedThis = true;
						}

						if (!reachedThis)
						{
							continue;
						}

						if (!(imp is Imp_GenericFromTemplate))
						{
							isNotTheLastImplementation = true;
							break;
						}
					}
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					// Don't sample at shader property declaration, but at shader property usage,
					// except if there's no other implementations: use a 1 constant that will be multiplied

					if (isTheOnlyImplementation)
					{
						switch (ParentShaderProperty.Type)
						{
							case VariableType.@float: return "1";
							case VariableType.float2: return "float2(1,1)";
							case VariableType.color:
							case VariableType.float3: return "float3(1,1,1)";
							case VariableType.color_rgba:
							case VariableType.float4: return "float4(1,1,1,1)";
						}
					}

					return null;
				}

				public string Print()
				{
					string op = isTheOnlyImplementation ? " * " : PrintOperator();
					return string.Format("{0}{1}.{2}", op, VariableName, Channels.ToLowerInvariant());
				}

				public string PrintCustomCode()
				{
					return VariableName;
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Special implementation defined in the template" + (HelpMessage != null ? ":\n" + HelpMessage : "."));
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_GenericFromTemplate>().Channels;
						SGUILayout.InlineLabel("Swizzle", highlighted);

						if (usedByCustomCode)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
							}
						}
						else
						{
							if (ChannelsCount == 1)
							{
								Channels = SGUILayout.GenericSelector(ChannelsOptions, Channels);
							}
							else
							{
								Channels = SGUILayout.GenericSwizzle(Channels, ChannelsCount, ChannelsOptions);
							}
						}
					}
					EndHorizontal();

					if (options != null)
					{
						for (int i = 0; i < options.Count; i++)
						{
							if (!OptionsEnabled.ContainsKey(options[i].label))
							{
								OptionsEnabled.Add(options[i].label, false);
							}

							BeginHorizontal();
							{
								EditorGUI.BeginChangeCheck();
								bool highlighted = !IsDefaultImplementation ? OptionsEnabled[options[i].label] : OptionsEnabled[options[i].label] != GetDefaultImplementation<Imp_GenericFromTemplate>().OptionsEnabled[options[i].label];
								SGUILayout.InlineLabel(options[i].label, highlighted);
								OptionsEnabled[options[i].label] = SGUILayout.Toggle(OptionsEnabled[options[i].label]);
								if (EditorGUI.EndChangeCheck())
								{
									options[i].UpdateConfigIfNeeded(OptionsEnabled[options[i].label]);
									SynchronizeOptions();
								}
							}
							EndHorizontal();
						}

						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout("The options for this Special Implementation are global across all the Properties.", MessageType.Info);
						}
						EndHorizontal();
					}

					// errors
					if (!sourceIsAvailable)
					{
						GUILayout.Space(4);
						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout("This implementation is not available anymore, based on the selected features and options.", MessageType.Error);
						}
						EndHorizontal();
					}

					if (isNotTheLastImplementation)
					{
						GUILayout.Space(4);
						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout("This special implementation depends on the shader context and has to be the last implementation in the list.\nPlease drag its handle on the left and move it last.", MessageType.Error);
						}
						EndHorizontal();
					}
				}
			}

			[Serialization.SerializeAs("imp_enum")]
			public class Imp_Enum : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableType.fixed_function_enum; } }
				public static string MenuLabel { get { return "Enum (Fixed Function)"; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("value_type")] public int ValueType;
				[Serialization.SerializeAs("value")] public int EnumValue;
				[Serialization.SerializeAs("enum_type")] public string EnumType;

				Enums.OrderedEnum[] enumValues;
				string[] enumDisplayNames;

				string[] options = new string[]
				{
					"Constant",
					"Material Property"
				};

				public void SetValueTypeFromString(string valueTypeStr)
				{
					int index = Array.IndexOf(options, valueTypeStr);
					ValueType = index;
				}

				public Imp_Enum(ShaderProperty shaderProperty) : base(shaderProperty)
				{
				}

				[Serialization.OnDeserializeCallback]
				public void SetEnumType()
				{
					var type = typeof(GameObject).Assembly.GetType(EnumType, false);

					if (type == null)
					{
						var assemblies = AppDomain.CurrentDomain.GetAssemblies();
						foreach (var assembly in assemblies)
						{
							type = assembly.GetType(EnumType);
							if (type != null)
							{
								break;
							}
						}
					}

					if (type == null)
					{
						throw new ArgumentException("Can't find Enum Type: " + EnumType);
					}

					if (!type.IsEnum)
					{
						throw new ArgumentException("Found Type is not an Enum: " + EnumType);
					}

					enumValues = Enums.GetOrderedEnumValues(type);
					enumDisplayNames = Array.ConvertAll(enumValues, ev => ev.displayName);
				}

				public void Parse(string strValue)
				{
					int index = Array.FindIndex(enumValues, ev => ev.value.ToString() == strValue);
					if (index < 0)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't parse value '{0}' for type '{1}'.", strValue, EnumType)));
						return;
					}

					EnumValue = index;
				}

				bool IsConstant()
				{
					return ValueType == 0;
				}

				string PropertyName()
				{
					return string.Format("_{0}", ToLowerCamelCase(ParentShaderProperty.Name));
				}

				internal override string PrintVariableFixedFunction()
				{
					if (IsConstant())
					{
						return enumValues[EnumValue].value.ToString();
					}
					else
					{
						return string.Format("[{0}]", PropertyName());
					}
				}

				internal override string PrintProperty(string indent)
				{
					if (!IsConstant())
					{
						return base.PrintProperty(indent) + string.Format("[Enum({0})] {1} (\"{2}\", Float) = {3}", EnumType.Replace("+", "."), PropertyName(), Label, Convert.ChangeType(enumValues[EnumValue].value, TypeCode.Int32));
					}
					else
					{
						return null;
					}
				}

				/*
				internal override string PrintVariableDeclare(string indent) { return string.Format("float {0};", PropertyName); }
				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments) { return PropertyName; }
				*/

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					base.NewLineGUI(usedByCustomCode);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? ValueType > 0 : ValueType != GetDefaultImplementation<Imp_Enum>().ValueType;
						SGUILayout.InlineLabel(TCP2_GUI.TempContent("Type"), highlighted);
						ValueType = SGUILayout.Popup(ValueType, options);
					}
					EndHorizontal();

					if (enumValues == null)
					{
						BeginHorizontal();
						{
							SGUILayout.InlineLabel(TCP2_GUI.TempContent("Couldn't find enum type: '" + EnumType + "'"));
						}
						EndHorizontal();
					}
					else
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? EnumValue > 0 : EnumValue != GetDefaultImplementation<Imp_Enum>().EnumValue;
							SGUILayout.InlineLabel(TCP2_GUI.TempContent(IsConstant() ? "Value" : "Default Value"), highlighted);
							EnumValue = SGUILayout.Popup(EnumValue, enumDisplayNames);
						}
						EndHorizontal();
					}
				}

				// Used to show the properties in the Features tab directy
				internal void EmbeddedGUI(float indent = 0, float labelWidth = 130)
				{
					// Embedded through the "mult_fs" UIFeature
					/*
					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(indent);
						TCP2_GUI.SubHeader("Type", null, true, labelWidth);
						ValueType = EditorGUILayout.Popup(ValueType, options);
					}
					GUILayout.EndHorizontal();
					*/

					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(indent);
						bool highlighted = EnumValue != GetDefaultImplementation<Imp_Enum>().EnumValue;
						TCP2_GUI.SubHeader(IsConstant() ? "Value" : "Default Value", null, highlighted, labelWidth + 4);
						GUILayout.Space(-4); // hack to align the highlighted part with the regular UIFeatures
						EnumValue = EditorGUILayout.Popup(EnumValue, enumDisplayNames);
					}
					GUILayout.EndHorizontal();
				}
			}

			[Serialization.SerializeAs("imp_customcode")]
			public class Imp_CustomCode : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Special/Custom Code"; } }
				internal override string GUILabel() { return MenuLabel; }
				internal override bool HasOperator() { return false; }

				public enum PrependType
				{
					Disabled,
					Embedded,
					ExternalFile
				}

				[Serialization.SerializeAs("prepend_type")] public PrependType prependType = PrependType.Disabled;
				[Serialization.SerializeAs("prepend_code")] public string prependCode = "";
				[Serialization.SerializeAs("prepend_file")] public string prependFileGuid = "";
				[Serialization.SerializeAs("prepend_file_block")] public string prependFileBlock = "";
				[Serialization.SerializeAs("preprend_params")] public Dictionary<string, string> prependParametersValues = new Dictionary<string, string>(); // values for the parameters of the defined block in the prepend file

				TextAsset prependFile;
				bool prependFileBlockFound;
				string[] prependBlocks;

				struct PrependReference
				{
					public ShaderProperty.VariableType variableType;
					public string variableName;
					public string defaultValueOrComment;
					public bool isComment;

					public PrependReference(ShaderProperty.VariableType type, string name, string value, bool comment)
					{
						variableType = type;
						variableName = name;
						defaultValueOrComment = value;
						isComment = comment;

						label = string.Format("{0} ({1})", name, type);
					}

					public string label { get; private set; }
				}
				List<PrependReference> prependReferences;
				List<string> prependLines;

				[Serialization.SerializeAs("code")] public string code = "";
				public bool usesReplacementTags = false;
				Dictionary<string, List<string>> replacementParts = new Dictionary<string, List<string>>();
				List<int> usedImplementations = new List<int>();
				public string tagError = null;

				public override bool HasErrors { get { return base.HasErrors | !string.IsNullOrEmpty(tagError) | (prependType == PrependType.ExternalFile && prependFile != null && !prependFileBlockFound); } }

				public Imp_CustomCode(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					ParentShaderProperty.onImplementationsChanged += onImplementationsChanged;
					ShaderGenerator2.onProjectChange += onProjectChanged;
				}

				public override void WillBeRemoved()
				{
					ParentShaderProperty.onImplementationsChanged -= onImplementationsChanged;
					ShaderGenerator2.onProjectChange -= onProjectChanged;
				}

				void onImplementationsChanged()
				{
					CheckReplacementTags();
				}

				void onProjectChanged()
				{
					TryToFindPrependCodeBlock();
					CheckReplacementTags();
				}

				[Serialization.OnDeserializeCallback]
				void OnDeserialized()
				{
					TryFindPrependFileFromGuid();
					CheckReplacementTags();
				}

				public override void OnPasted()
				{
					TryFindPrependFileFromGuid();
					CheckReplacementTags();
				}
				
				void TryFindPrependFileFromGuid()
				{
					if (!string.IsNullOrEmpty(prependFileGuid))
					{
						var file = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(prependFileGuid));
						if (file != null)
						{
							prependFile = file;
							TryToFindPrependCodeBlock();
						}
						else
						{
							prependFileGuid = "";
							prependFile = null;
						}
					}
				}

				Dictionary<string, string> shaderUniqueVariableNamesMapping;

				void PrintPrependCodeIfNeeded()
				{
					PrintPrependCodeIfNeeded(null, null, null, null);
				}
				Dictionary<string, string> PrintPrependCodeIfNeeded(Dictionary<Implementation, string> cachedVariables, string inputSource, string outputSource, string arguments)
				{
					if (prependType == PrependType.Disabled)
					{
						return null;
					}

					if (prependType == PrependType.Embedded && !string.IsNullOrEmpty(prependCode))
					{
						string pCode = prependCode;
						if (replacementParts.ContainsKey("prependCode"))
						{
							var list = replacementParts["prependCode"];
							pCode = ParseReplacementParts(list, cachedVariables, inputSource, outputSource, arguments);
						}

						var lines = pCode.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
						foreach (var l in lines)
						{
							ShaderGenerator2.AppendLineBefore(l);
						}
					}
					else if (prependType == PrependType.ExternalFile && prependFile != null && prependFileBlockFound)
					{
						if (shaderUniqueVariableNamesMapping == null && prependReferences != null)
						{
							shaderUniqueVariableNamesMapping = new Dictionary<string, string>();
							foreach (var reference in prependReferences)
							{
								if (reference.isComment) continue;
								shaderUniqueVariableNamesMapping.Add(reference.variableName, string.Format("{0}_{1}", reference.variableName, ShaderGenerator2.GlobalUniqueId));
							}
						}

						string header = string.Format("// {0} : {1}", prependFile.name, prependFileBlock);
						string commentLine = "//" + new string('-', header.Length - 2);
						ShaderGenerator2.AppendLineBefore(commentLine);
						ShaderGenerator2.AppendLineBefore(header);

						// generate declaration of each parameter with its value
						for (int i = 0; i < prependReferences.Count; i++)
						{
							var reference = prependReferences[i];
							if (reference.isComment)
							{
								continue;
							}

							var value = prependParametersValues[reference.variableName];

							if (replacementParts.ContainsKey(reference.variableName))
							{
								var list = replacementParts[reference.variableName];
								value = ParseReplacementParts(list, cachedVariables, inputSource, outputSource, arguments);
							}

							ShaderGenerator2.AppendLineBefore(string.Format("{0} {1} = {2};", 
								ShaderProperty.VariableTypeToShaderCode(reference.variableType),
								shaderUniqueVariableNamesMapping[reference.variableName],
								value));
						}

						// process and append the block lines
						var variableRegex = new Regex(@"[^\w](_(\w+)_)[^\w]+?", RegexOptions.ECMAScript);
						Dictionary<string, string> uniqueVariableReplacements = new Dictionary<string, string>();
						foreach (var l in prependLines)
						{
							string line = l;

							// replace the variable names with their unique id counterpart to avoid duplicate declarations
							foreach (var reference in prependReferences)
							{
								if (reference.isComment) continue;
								line = line.Replace(reference.variableName, shaderUniqueVariableNamesMapping[reference.variableName]);
							}

							// find and replace variables with the _name_ format, to avoid duplicate declarations
							var matches = variableRegex.Matches(line);
							foreach (Match match in matches)
							{
								string toReplace = match.Groups[1].Value;
								if (!uniqueVariableReplacements.ContainsKey(toReplace))
								{
									uniqueVariableReplacements.Add(toReplace, match.Groups[2].Value + "_" + ShaderGenerator2.GlobalUniqueId);
								}

								line = line.Replace(toReplace, uniqueVariableReplacements[toReplace]);
							}

							ShaderGenerator2.AppendLineBefore(line);
						}
						ShaderGenerator2.AppendLineBefore(commentLine);

						return uniqueVariableReplacements;
					}

					return null;
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					PrintPrependCodeIfNeeded();
					return code.Length > 0 && !char.IsWhiteSpace(code[0]) ? " " + code : code;
				}

				//called if the custom code (or prepend code) use {n} tags, to directly use implementations within the custom code
				public string PrintVariableReplacement(ref HashSet<Implementation> usedImplementations, string inputSource, string outputSource, string arguments)
				{
					if (!string.IsNullOrEmpty(tagError))
					{
						// This shouldn't happen because this error is checked beforehand (disables 'Generate' button)
						Debug.LogError(ShaderGenerator2.ErrorMsg("Custom Code error: " + tagError));
						return null;
					}

					if (replacementParts.Count == 0)
					{
						// This shouldn't happen because this error is checked beforehand (disables 'Generate' button)
						Debug.LogError(ShaderGenerator2.ErrorMsg("Custom Code error: 'replacementParts' is null or empty"));
						return null;
					}

					int customCodeIndex = ParentShaderProperty.implementations.IndexOf(this);
					string output = "";

					// First pass: see which implementations are sampled and how many times (to possibly cache them)
					var usedImpsMultipleTimes = new List<Implementation>();
					foreach (var partsList in replacementParts.Values)
					{
						foreach (var part in partsList)
						{
							if (part.StartsWith("tag:"))
							{
								var intStr = part.Substring("tag:".Length);
								int impIndex = int.Parse(intStr) - 1;

								if (impIndex == customCodeIndex)
								{
									// This shouldn't happen because this error is checked beforehand (disables 'Generate' button)
									Debug.LogError(ShaderGenerator2.ErrorMsg("Custom Code error: the Custom Code implementation cannot reference itself!\nCustom Code index = " + customCodeIndex + ", Reference = {" + impIndex + "}"));
									return null;
								}

								if (impIndex < customCodeIndex)
								{
									// This shouldn't happen because this error is checked beforehand (disables 'Generate' button)
									Debug.LogError(ShaderGenerator2.ErrorMsg("Custom Code error: the Custom Code implementation cannot reference previous implementations!\nCustom Code index = " + customCodeIndex + ", Reference = {" + impIndex + "}"));
									return null;
								}

								var imp = ParentShaderProperty.implementations[impIndex];

								if (usedImplementations.Contains(imp) && !usedImpsMultipleTimes.Contains(imp))
								{
									usedImpsMultipleTimes.Add(imp);
								}

								usedImplementations.Add(imp);
							}
						}
					}

					// Sample the implementations that are used multiple times beforehand
					var cachedVariables = new Dictionary<Implementation, string>();
					if (usedImpsMultipleTimes.Count > 0)
					{
						ShaderGenerator2.AppendLineBefore("// Sampled in Custom Code");
						for (int i = 0; i < usedImpsMultipleTimes.Count; i++)
						{
							var imp = usedImpsMultipleTimes[i];

							// unique variable name based on the implementation
							string variableName = string.Format("imp_{0}", ShaderGenerator2.GlobalUniqueId);
							cachedVariables.Add(imp, variableName);

							string variableType = "float";
							var compatibility = (VariableType)imp.GetType().GetProperty("VariableCompatibility", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null, null);
							if (CheckVariableType(compatibility, VariableType.float4)
								|| CheckVariableType(compatibility, VariableType.color_rgba))
							{
								variableType = "float4";
							}
							else if (CheckVariableType(compatibility, VariableType.float3)
								|| CheckVariableType(compatibility, VariableType.color))
							{
								variableType = "float3";
							}
							else if (CheckVariableType(compatibility, VariableType.float2))
							{
								variableType = "float2";
							}
							string format = string.Format("{0} {1} = {{0}};", variableType, variableName);

							string argumentsHideChannels = AddArgument("hide_channels", "true", arguments);

							// special case: when using deferred sampling, allow referencing special implementations because everything is sampled at the same time
							if (ParentShaderProperty.deferredSampling && imp is Imp_GenericFromTemplate)
							{
								ShaderGenerator2.AppendLineBefore(string.Format(format, (imp as Imp_GenericFromTemplate).PrintCustomCode()));
							}
							else if (ParentShaderProperty.Program == ProgramType.Vertex)
							{
								ShaderGenerator2.AppendLineBefore(string.Format(format, imp.PrintVariableVertex(inputSource, outputSource, argumentsHideChannels)));
							}
							else
							{
								ShaderGenerator2.AppendLineBefore(string.Format(format, imp.PrintVariableFragment(inputSource, outputSource, argumentsHideChannels)));
							}
						}
					}

					// Prepend code if any
					var replacementDict = PrintPrependCodeIfNeeded(cachedVariables, inputSource, outputSource, arguments);

					// Print the custom code with cached variables
					arguments = AddArgument("hide_channels", "true", arguments);
					if (replacementParts.ContainsKey("code"))
					{
						var list = replacementParts["code"];
						output += ParseReplacementParts(list, cachedVariables, inputSource, outputSource, arguments);
					}

					// Replace unique variables (format _name_) from the external file, if any
					if (replacementDict != null)
					{
						foreach (var kvp in replacementDict)
						{
							output = output.Replace(kvp.Key, kvp.Value);
						}
					}

					if (output.Length > 0 && !char.IsWhiteSpace(output[0]))
					{
						output = " " + output;
					}

					return output;
				}

				string ParseReplacementParts(List<string> replacementPartsList, Dictionary<Implementation, string> cachedVariables, string inputSource, string outputSource, string arguments)
				{
					string output = "";
					foreach (var part in replacementPartsList)
					{
						if (part.StartsWith("tag:"))
						{
							var intStr = part.Substring("tag:".Length);
							int impIndex = int.Parse(intStr) - 1;
							var imp = ParentShaderProperty.implementations[impIndex];

							if (cachedVariables.ContainsKey(imp))
							{
								output += cachedVariables[imp];
							}
							else
							{
								// special case: when using deferred sampling, allow referencing special implementations because everything is sampled at the same time
								if (imp is Imp_GenericFromTemplate)
								{
									output += (imp as Imp_GenericFromTemplate).PrintCustomCode();
								}
								else if (ParentShaderProperty.Program == ProgramType.Vertex)
								{
									output += imp.PrintVariableVertex(inputSource, outputSource, arguments);
								}
								else
								{
									output += imp.PrintVariableFragment(inputSource, outputSource, arguments);
								}
							}
						}
						else
						{
							output += part;
						}
					}
					return output;
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Insert arbitrary custom shader code.");
					EndHorizontal();

					// Prepend system
					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Prepend Type");
						EditorGUI.BeginChangeCheck();
						prependType = (PrependType)SGUILayout.EnumPopup(prependType);
						if (EditorGUI.EndChangeCheck())
						{
							CheckReplacementTags();
							TryToFindPrependCodeBlock();
						}
					}
					EndHorizontal();

					if (prependType == PrependType.Embedded)
					{
						BeginHorizontal();
						{
							SGUILayout.InlineLabel("Prepend Code");
							EditorGUI.BeginChangeCheck();
							prependCode = SGUILayout.TextArea(prependCode, 90);
							if (EditorGUI.EndChangeCheck())
							{
								CheckReplacementTags();
							}
						}
						EndHorizontal();
						GUILayout.Space(3);
					}
					else if (prependType == PrependType.ExternalFile)
					{
						BeginHorizontal();
						{
							SGUILayout.InlineLabel("Prepend File");
							EditorGUI.BeginChangeCheck();
							prependFile = SGUILayout.ObjectField<TextAsset>(prependFile);
							if (EditorGUI.EndChangeCheck())
							{
								prependFileGuid = prependFile != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prependFile)) : "";
								TryToFindPrependCodeBlock();
							}
						}
						EndHorizontal();
						GUILayout.Space(4);

						if (prependBlocks == null || prependBlocks.Length == 0)
						{
							BeginHorizontal();
							{
								EditorGUILayout.HelpBox("Please select a valid prepend file.", MessageType.Info);
							}
							EndHorizontal();
						}
						else
						{
							BeginHorizontal();
							{
								SGUILayout.InlineLabel("Block Name");
								EditorGUI.BeginChangeCheck();

								//prependFileBlock = SGUILayout.TextField(prependFileBlock, true);

								int index = -1;
								index = Array.IndexOf(prependBlocks, prependFileBlock);
								index = SGUILayout.Popup(index, prependBlocks);

								if (EditorGUI.EndChangeCheck())
								{
									prependFileBlock = prependBlocks[index];
									TryToFindPrependCodeBlock();
								}
							}
							EndHorizontal();

							if (prependFileBlockFound)
							{
								for (int i = 0; i < prependReferences.Count; i++)
								{
									var reference = prependReferences[i];

									if (reference.isComment)
									{
										BeginHorizontal(12);
										{
											EditorGUILayout.HelpBox(reference.defaultValueOrComment, MessageType.None);
										}
										EndHorizontal();
									}
									else
									{
										BeginHorizontal(12);
										{
											SGUILayout.InlineLabel(reference.label);

											EditorGUI.BeginChangeCheck();
											prependParametersValues[reference.variableName] = SGUILayout.TextField(prependParametersValues[reference.variableName], false);
											if (EditorGUI.EndChangeCheck())
											{
												CheckReplacementTags();
											}
										}
										EndHorizontal();
									}
								}
							}

							if (!prependFileBlockFound)
							{
								BeginHorizontal();
								{
									EditorGUILayout.HelpBox("Could not find the specified code block in the linked prepend file.", MessageType.Error);
								}
								EndHorizontal();
							}
						}

						GUILayout.Space(8f);
					}

					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Code");
						EditorGUI.BeginChangeCheck();
						code = SGUILayout.TextField(code);
						if (EditorGUI.EndChangeCheck())
						{
							CheckReplacementTags();
						}
					}
					EndHorizontal();

					if (tagError != null)
					{
						BeginHorizontal();
						{
							GUILayout.Space(4);
							TCP2_GUI.HelpBoxLayout(tagError, MessageType.Error);
						}
						EndHorizontal();
					}
					else
					{
						BeginHorizontal();
						{
							GUILayout.Space(4);
							TCP2_GUI.HelpBoxLayout("You can reference other implementations using <b>{n}</b> notation where <b>n</b> is the index of another implementation for this property, e.g.:\n<i>dot({1}, {2})</i>\n\nNote: the <b>operator</b> and <b>channels</b> for referenced implementations will be ignored!", MessageType.Info);
						}
						EndHorizontal();
					}
				}

				public void TryToFindPrependCodeBlock()
				{
					if (prependType != PrependType.ExternalFile || string.IsNullOrEmpty(this.prependFileGuid))
					{
						return;
					}

					prependFileBlockFound = false;
					string[] lines = System.IO.File.ReadAllLines(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + AssetDatabase.GetAssetPath(prependFile));


					// find all prepend blocks (not the best place to do it though)
					var blockList = new List<string>();
					for (int i = 0; i < lines.Length; i++)
					{
						var line = lines[i].Trim();
						if (line.StartsWith("//#") && line.EndsWith(":"))
						{
							blockList.Add(line.Substring("//#".Length, line.Length - "//#:".Length).Trim());
						}
					}
					prependBlocks = blockList.ToArray();

					// find matching prepend block
					for (int i = 0; i < lines.Length; i++)
					{
						var line = lines[i];
						if (line.StartsWith("///"))
						{
							continue;
						}

						if (line.StartsWith("//#"))
						{
							if (prependFileBlockFound)
							{
								if (line.Contains(":"))
								{
									// marks the end of the current block
									break;
								}
							}
							else
							{
								// found matching block
								var trimmed = line.Substring("//#".Length).Trim().TrimEnd(':');
								if (trimmed == prependFileBlock)
								{
									prependFileBlockFound = true;
									ParsePrependBlock(ref lines, i + 1);
									return;
								}
							}
						}
					}
				}

				void ParsePrependBlock(ref string[] lines, int startIndex)
				{
					prependReferences = new List<PrependReference>();
					prependLines = new List<string>();

					for (int i = startIndex; i < lines.Length; i++)
					{
						string line = lines[i];

						// end of block
						if (line.StartsWith("//#"))
						{
							// prepend comment
							if (line.StartsWith("//# !"))
							{
								string comment = line.Substring("//# !".Length).Trim();
								prependReferences.Add(new PrependReference(VariableType.@float, "comment", comment, true));
							}
							// new block = end of this block
							else if (line.Contains(":"))
							{
								break;
							}
							// prepend reference
							else
							{
								// inputs description in the form:
								// '//# type variableName [defaultValue]'
								// will be translated into an UI where user can type the value they want, including {n} notation

								string prependRefStr = line.Substring("//#".Length).Trim();
								string[] parts = prependRefStr.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
								if (parts.Length < 2)
								{
									Debug.LogError(ShaderGenerator2.ErrorMsg("Invalid prepend code reference, it should be in the following format:\n\"//# type name [defaultValue]\" (e.g. \"//# float4 myVariable (1.1, 2.0, 0.0, 4.0)\")\nParsed line:\n" + line));
								}
								else
								{
									var vType = (ShaderProperty.VariableType)System.Enum.Parse(typeof(ShaderProperty.VariableType), parts[0]);
									if (!System.Enum.IsDefined(typeof(ShaderProperty.VariableType), vType))
									{
										Debug.LogError(ShaderGenerator2.ErrorMsg("Invalid variable type defined for prepend code reference:\n" + line));
									}
									else
									{
										string name = parts[1];
										string defaultValue = parts.Length > 2 ? parts[2] : "";
										prependReferences.Add(new PrependReference(vType, name, defaultValue, false));
									}
								}
							}
						}
						else if (line.StartsWith("///"))
						{
							// ignore comments for the prepend file only
							continue;
						}
						else
						{
							// add the line to the ones to be printed
							prependLines.Add(line);
						}
					}

					// trim all empty lines at the end of the list
					for (int i = prependLines.Count-1; i >= 0; i--)
					{
						if (!string.IsNullOrEmpty(prependLines[i]))
						{
							break;
						}
						prependLines.RemoveAt(i);
					}

					// Initialize the prepend code references
					if (prependReferences.Count > 0)
					{
						// new list
						if (prependParametersValues == null)
						{
							prependParametersValues = new Dictionary<string, string>();
						}

						foreach (var reference in prependReferences)
						{
							if (reference.isComment) continue;
							if (!prependParametersValues.ContainsKey(reference.variableName))
							{
								string defaultValue = string.IsNullOrEmpty(reference.defaultValueOrComment) ? getDefaultValueForType(reference.variableType) : reference.defaultValueOrComment;
								prependParametersValues.Add(reference.variableName, defaultValue);
							}
						}

						List<string> keysToRemove = new List<string>();
						foreach (var kvp in prependParametersValues)
						{
							if (!prependReferences.Exists(reference => reference.variableName == kvp.Key))
							{
								keysToRemove.Add(kvp.Key);
							}
						}
						foreach (var key in keysToRemove)
						{
							prependParametersValues.Remove(key);
						}
					}
				}

				string getDefaultValueForType(ShaderProperty.VariableType variableType)
				{
					switch(variableType)
					{
						case VariableType.@float:		return "0.0";
						case VariableType.float2:		return "float2(0.0, 0.0)";
						case VariableType.float3:
						case VariableType.color: return "float3(0.0, 0.0, 0.0)";
						case VariableType.float4:
						case VariableType.color_rgba: return "float4(0.0, 0.0, 0.0, 0.0)";
						default: return "";
					}
				}

				string[] LoadPrependBlock()
				{
					bool inBlock = false;
					var prepandBlockLines = new List<string>();
					string[] lines = System.IO.File.ReadAllLines(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + AssetDatabase.GetAssetPath(prependFile));
					for (int i = 0; i < lines.Length; i++)
					{
						var line = lines[i];

						if (line.StartsWith("//#"))
						{
							if (inBlock)
							{
								if (line.Contains(":"))
								{
									// marks the end of the current block
									break;
								}
								else
								{
									// inputs description in the form:
									// "type variableName"
									// will be translated into an UI where user can link an implementation to each input
									// TODO
								}
							}
							else
							{
								var trimmed = line.Substring("//#".Length).Trim().TrimEnd(':');
								if (trimmed == prependFileBlock)
								{
									inBlock = true;
								}
							}
						}
						else if (inBlock)
						{
							prepandBlockLines.Add(line);
						}
					}

					// trim all empty lines at the end of the list
					for (int i = prepandBlockLines.Count-1; i >= 0; i--)
					{
						if (!string.IsNullOrEmpty(prepandBlockLines[i]))
						{
							break;
						}
						prepandBlockLines.RemoveAt(i);
					}

					return prepandBlockLines.ToArray();
				}

				public void CheckReplacementTags()
				{
					if (usedImplementations != null)
					{
						foreach (int value in usedImplementations)
						{
							ParentShaderProperty.usedImplementationsForCustomCode.Remove(value);
						}
					}

					usesReplacementTags = false;
					replacementParts.Clear();
					usedImplementations.Clear();
					tagError = null;
					int customCodeIndex = ParentShaderProperty.implementations.IndexOf(this);
					int maxIndex = ParentShaderProperty.implementations.Count - 1;

					// parse code
					var codeReplacements = ReplaceNNotationWithReplacementTags(code, customCodeIndex, maxIndex);
					if (codeReplacements != null)
					{
						replacementParts.Add("code", new List<string>());
						replacementParts["code"].AddRange(codeReplacements.Value.parts);
						usedImplementations.AddRange(codeReplacements.Value.usedImplementations);
					}

					// parse prepend code (embedded mode)
					if (prependType == PrependType.Embedded)
					{
						var cr = ReplaceNNotationWithReplacementTags(prependCode, customCodeIndex, maxIndex);
						if (cr != null)
						{
							replacementParts.Add("prependCode", new List<string>());
							replacementParts["prependCode"].AddRange(cr.Value.parts);
							usedImplementations.AddRange(cr.Value.usedImplementations);
						}
					}
					// parse prepend code (external file mode)
					else if (prependType == PrependType.ExternalFile && prependReferences != null && prependParametersValues != null)
					{
						for (int i = 0; i < prependReferences.Count; i++)
						{
							var reference = prependReferences[i];

							if (reference.isComment)
							{
								continue;
							}

							string value = prependParametersValues[reference.variableName];
							if (value.Contains("{"))
							{
								var cr = ReplaceNNotationWithReplacementTags(value, customCodeIndex, maxIndex);
								if (cr != null)
								{
									string key = prependReferences[i].variableName;
									replacementParts.Add(key, new List<string>());
									replacementParts[key].AddRange(cr.Value.parts);
									usedImplementations.AddRange(cr.Value.usedImplementations);
								}
							}
						}
					}

					usedImplementations = usedImplementations.Distinct().ToList();
					ParentShaderProperty.usedImplementationsForCustomCode.AddRange(usedImplementations);
				}

				struct ReplacementTagsResult
				{
					public List<string> parts;
					public List<int> usedImplementations;
				}

				ReplacementTagsResult? ReplaceNNotationWithReplacementTags(string input, int customCodeIndex, int maxIndex)
				{
					//explore the string and find all '{n}' where n = number, and construct list of parts
					bool tag = false;
					string currentPart = null;
					var parts = new List<string>();
					var usedImps = new List<int>();
					for (int i = 0; i < input.Length; i++)
					{
						char c = input[i];

						//inside tag (maybe)
						if (tag)
						{
							//closing tag
							if (c == '}')
							{
								if (string.IsNullOrEmpty(currentPart))
								{
									tagError = "Invalid code: empty replacement tag";
									return null;
								}

								tag = false;
								parts.Add("tag:" + currentPart);

								int usedImpIndex;
								if (int.TryParse(currentPart, out usedImpIndex))
								{
									usedImpIndex -= 1;
									if (!usedImps.Contains(usedImpIndex))
									{
										usedImps.Add(usedImpIndex);

										if (usedImpIndex == customCodeIndex)
										{
											tagError = "Invalid code: the Custom Code implementation cannot reference itself";
											return null;
										}
										else if (usedImpIndex < customCodeIndex)
										{
											tagError = "Invalid code: the Custom Code implementation cannot reference previous implementations";
											return null;
										}
										else if (usedImpIndex > maxIndex)
										{
											tagError = "Invalid code: can't find implementation for index '" + (usedImpIndex+1) + "'";
											return null;
										}
										else
										{
											// Custom Code can't reference special implementations
											var imp = ParentShaderProperty.implementations[usedImpIndex];
											if (!ImplementationCanBeReferenced(imp))
											{
												tagError = "Invalid code: the Custom Code implementation cannot reference certain Special implementations";
												return null;
											}
										}
									}
								}
								else
								{
									Debug.LogWarning(ShaderGenerator2.ErrorMsg("Couldn't parse custom code tag content: \"" + currentPart + "\""));
								}

								currentPart = "";
							}
							else if (char.IsDigit(c))
							{
								currentPart += c;
							}
							else
							{
								tagError = "Invalid replacement tag: it should only contains digits";
								return null;
							}
						}
						//outside tag
						else
						{
							if (c == '{')
							{
								usesReplacementTags = true;
								tag = true;
								if (!string.IsNullOrEmpty(currentPart))
									parts.Add(currentPart);
								currentPart = "";
							}
							else
							{
								currentPart += c;
							}
						}
					}

					//tag not closed
					if (tag)
					{
						tagError = "Invalid code: replacement tag isn't closed";
						return null;
					}

					//add last part if any
					if (!string.IsNullOrEmpty(currentPart))
						parts.Add(currentPart);

					return new ReplacementTagsResult()
					{
						parts = parts,
						usedImplementations = usedImps
					};
				}

				bool ImplementationCanBeReferenced(Implementation imp)
				{
					// Custom Code can't reference special implementations
					if (!ParentShaderProperty.deferredSampling && imp is Imp_GenericFromTemplate)
					{
						if (!((Imp_GenericFromTemplate)imp).WorksWithCustomCode)
						{
							return false;
						}
					}
					else if (!ParentShaderProperty.deferredSampling
						&& (imp is Imp_HSV || imp is Imp_CustomCode))
					{
						return false;
					}

					return true;
				}
			}

			[Serialization.SerializeAs("imp_hsv")]
			public class Imp_HSV : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableType.color | VariableType.color_rgba; } }
				public static string MenuLabel { get { return "Special/HSV"; } }
				internal override string GUILabel() { return MenuLabel; }
				internal override bool HasOperator() { return false; }
				internal override OptionFeatures[] NeededFeatures() { return new[] { hsvType == HsvType.FullOffset ? OptionFeatures.HSV_Full : (hsvType == HsvType.Colorize ? OptionFeatures.HSV_Colorize : OptionFeatures.HSV_Grayscale) }; }
				internal override string[] NeededFeaturesExtra() { return new[] { string.Format("HSV_COLORIZE_{0}", GetColorizeChannels()) }; }

				public enum HsvType
				{
					FullOffset,
					SaturationOffset,
					Colorize
				}

				public override bool HasErrors { get { return base.HasErrors | isFirstImplementation | noColorizeChannels; } }

				[Serialization.SerializeAs("type")] HsvType hsvType;
				[Serialization.SerializeAs("chue")] bool colorizeHue;
				[Serialization.SerializeAs("csat")] bool colorizeSat;
				[Serialization.SerializeAs("cval")] bool colorizeVal;

				string hueVariable;
				string saturationVariable;
				string valueVariable;
				VariableType variableType;
				bool isFirstImplementation;
				bool noColorizeChannels { get { return (hsvType == HsvType.Colorize && !colorizeHue && !colorizeSat && !colorizeVal); } }

				public Imp_HSV(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					bool hasHue = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeHue);
					bool hasSat = hsvType != HsvType.Colorize || (hsvType == HsvType.Colorize && colorizeSat);
					bool hasVal = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeVal);

					if (hasHue)
						hueVariable = string.Format("_{0}_hue", ToLowerCamelCase(shaderProperty.Name));
					if (hasSat)
						saturationVariable = string.Format("_{0}_sat", ToLowerCamelCase(shaderProperty.Name));
					if (hasVal)
						valueVariable = string.Format("_{0}_val", ToLowerCamelCase(shaderProperty.Name));

					variableType = shaderProperty.Type;

					shaderProperty.onImplementationsChanged += onImplementationsChanged;
					CheckValidity();
				}

				public override void WillBeRemoved()
				{
					base.WillBeRemoved();
					ParentShaderProperty.onImplementationsChanged -= onImplementationsChanged;
				}

				void onImplementationsChanged()
				{
					CheckValidity();
				}

				void CheckValidity()
				{
					if (ParentShaderProperty.implementations.Count < 2 || ParentShaderProperty.implementations[0] == this)
					{
						isFirstImplementation = true;
					}
					else
					{
						isFirstImplementation = false;
					}

					ParentShaderProperty.CheckErrors();
				}

				internal override string PrintProperty(string indent)
				{
					var prop = base.PrintProperty(indent);

					bool hasHue = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeHue);
					bool hasSat = hsvType != HsvType.Colorize || (hsvType == HsvType.Colorize && colorizeSat);
					bool hasVal = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeVal);
					bool group = (hasHue && hasSat) || (hasHue && hasVal) || (hasSat && hasVal);

					string propName = ParentShaderProperty.Name;

					if (group)
					{
						prop += string.Format("\n{0}[HideInInspector] __BeginGroup_HSV_{1} (\"{2} HSV\", Float) = 0", indent, ToLowerCamelCase(ParentShaderProperty.Name), ParentShaderProperty.Name);
						propName = "";
					}
					else
					{
						propName = propName + " ";
					}

					if (hasHue)
						prop += string.Format("\n{0}{1} (\"{2}Hue\", Range(-180,180)) = 0", indent, hueVariable, propName);
					if (hasSat)
						prop += string.Format("\n{0}{1} (\"{2}Saturation\", Range(-2,2)) = {3}", indent, saturationVariable, propName, hsvType == HsvType.SaturationOffset ? "1.0" : "0.0");
					if (hasVal)
						prop += string.Format("\n{0}{1} (\"{2}Value\", Range(-2,2)) = 0", indent, valueVariable, propName);

					if (group)
						prop += string.Format("\n{0}[HideInInspector] __EndGroup (\"{1} HSV\", Float) = 0", indent, ParentShaderProperty.Name);

					return prop;
				}

				internal override string PrintVariableDeclare(string indent)
				{
					bool hasHue = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeHue);
					bool hasSat = hsvType != HsvType.Colorize || (hsvType == HsvType.Colorize && colorizeSat);
					bool hasVal = hsvType == HsvType.FullOffset || (hsvType == HsvType.Colorize && colorizeVal);

					var variables = base.PrintVariableDeclare(indent);
					if (hasHue)
						variables += string.Format("\n{0}float {1};", indent, hueVariable);
					if (hasSat)
						variables += string.Format("\n{0}float {1};", indent, saturationVariable);
					if (hasVal)
						variables += string.Format("\n{0}float {1};", indent, valueVariable);
					return variables;
				}

				public string PrintVariableHSV(string currentReplacement)
				{
					if (hsvType == HsvType.FullOffset)
					{
						return string.Format("ApplyHSV_{0}({1}, {2}, {3}, {4})", VariableTypeToChannelsCount(variableType), currentReplacement, hueVariable, saturationVariable, valueVariable);
					}
					else if (hsvType == HsvType.Colorize)
					{
						var colorizeArguments = "";
						if (colorizeHue)
							colorizeArguments += hueVariable + ",";
						if (colorizeSat)
							colorizeArguments += " " + saturationVariable + ",";
						if (colorizeVal)
							colorizeArguments += " " + valueVariable;

						return string.Format("Colorize{0}({1}, {2})", GetColorizeChannels(), currentReplacement, colorizeArguments.TrimEnd(',').TrimStart());
					}
					else
						return string.Format("ApplyHSVGrayscale({0}, {1})", currentReplacement, saturationVariable);
				}

				string GetColorizeChannels()
				{
					return string.Format("{0}{1}{2}", colorizeHue ? "H" : "", colorizeSat ? "S" : "", colorizeVal ? "V" : "");
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Applies hue, saturation, value correction to this Shader Property.\nThe HSV modifier will be applied to all implementations that preceed it.\nThe corresponding material properties to adjust each HSV value will be automatically created.");
					EndHorizontal();

					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Modes:\n<b>Full Offset:</b> apply an offset to all H,S,V values\n<b>Saturation Offset:</b> apply an offset to the saturation only (faster code)\n<b>Colorize:</b> set the absolute value of any H,S,V value");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? hsvType != default(HsvType) : hsvType != GetDefaultImplementation<Imp_HSV>().hsvType;
						SGUILayout.InlineLabel("HSV Mode", highlighted);
						hsvType = (HsvType)SGUILayout.EnumPopup(hsvType);
					}
					EndHorizontal();

					if (hsvType == HsvType.Colorize)
					{
						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? colorizeHue : colorizeHue != GetDefaultImplementation<Imp_HSV>().colorizeHue;
							SGUILayout.InlineLabel("Hue", highlighted);
							colorizeHue = SGUILayout.Toggle(colorizeHue);
						}
						EndHorizontal();

						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? colorizeSat : colorizeSat != GetDefaultImplementation<Imp_HSV>().colorizeSat;
							SGUILayout.InlineLabel("Saturation", highlighted);
							colorizeSat = SGUILayout.Toggle(colorizeSat);
						}
						EndHorizontal();

						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? colorizeVal : colorizeVal != GetDefaultImplementation<Imp_HSV>().colorizeVal;
							SGUILayout.InlineLabel("Value", highlighted);
							colorizeVal = SGUILayout.Toggle(colorizeVal);
						}
						EndHorizontal();
					}

					if (HasErrors)
					{
						if (isFirstImplementation)
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("HSV can't be the first implementation, because it applies to all the previous implementations before it.", MessageType.Error);
							}
							EndHorizontal();
						}

						if (noColorizeChannels)
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("You need to select the HSV channel(s) to colorize", MessageType.Error);
							}
							EndHorizontal();
						}
					}
				}
			}

			[Serialization.SerializeAs("imp_spref")]
			public class Imp_ShaderPropertyReference : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Other Shader Property"; } }
				internal override string GUILabel() { return MenuLabel; }

				[Serialization.SerializeAs("cc")] public int ChannelsCount = 3;
				[Serialization.SerializeAs("chan")] public string Channels = "RGB";
				[Serialization.SerializeAs("lsp")] public string LinkedShaderPropertyName;
				string DefaultChannels = "RGB";

				public List<ShaderProperty> Dependencies = new List<ShaderProperty>();

				ShaderProperty _linkedShaderProperty;
				public ShaderProperty LinkedShaderProperty
				{
					get { return _linkedShaderProperty; }
					set
					{
						SetLinkedShaderProperty(value);
					}
				}

				public override string ToHashString()
				{
					var result = new StringBuilder();

					var props = GetType().GetProperties();
					foreach (var prop in props)
					{
						var attributes = prop.GetCustomAttributes(typeof(Serialization.SerializeAsAttribute), true);
						if (attributes == null || attributes.Length == 0)
						{
							continue;
						}

						if (prop.PropertyType == typeof(ShaderProperty))
						{
							var spRef = (ShaderProperty)prop.GetValue(this, null);
							result.Append(spRef != null ? spRef.Name : "EmptyShaderPropertyRef");
						}
						else
						{
							result.Append(prop.GetValue(this, null));
						}
					}

					var fields = GetType().GetFields();
					foreach (var field in fields)
					{
						if (field.Name == "guid") continue;
						result.Append(field.GetValue(this));
					}

					return result.ToString();
				}

				public override bool HasErrors
				{
					get
					{
						return base.HasErrors | _linkedShaderProperty == null | (_linkedShaderProperty != null && !_linkedShaderProperty.IsVisible());
					}
				}

				public Imp_ShaderPropertyReference(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					InitChannelsCount();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				public override Implementation Clone()
				{
					var mp = (Imp_ShaderPropertyReference)base.Clone();
					return mp;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
					TryToFindLinkedShaderProperty();
				}

				public void TryToFindLinkedShaderProperty()
				{
					if (string.IsNullOrEmpty(LinkedShaderPropertyName))
					{
						return;
					}

					if (ShaderGenerator2.CurrentConfig == null)
					{
						return;
					}

					var match = Array.Find(ShaderGenerator2.CurrentConfig.VisibleShaderProperties, sp => sp.Name == LinkedShaderPropertyName);
					if (match != null)
					{
						SetLinkedShaderProperty(match);
					}
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";

					if (LinkedShaderProperty.IsUsedInLightingFunction && ShaderGenerator2.CurrentPassHasLightingFunction)
						return string.Format("{0}.{1}{2}", outputSource, LinkedShaderProperty.GetVariableName(), channels);
					else
						return string.Format("{0}{1}", LinkedShaderProperty.GetVariableName(), channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Reference another Shader Property as a source for this one.\nFor example, you could reference the Albedo's alpha channel as a source mask for another property like specular.");
					EndHorizontal();

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? false : LinkedShaderPropertyName != GetDefaultImplementation<Imp_ShaderPropertyReference>().LinkedShaderPropertyName;
						SGUILayout.InlineLabel("Shader Property", highlighted);

						if (GUILayout.Button((LinkedShaderProperty != null) ? LinkedShaderProperty.Name : "None", SGUILayout.Styles.ShurikenPopup))
						{
							var menu = CreateShaderPropertiesMenu(this.ParentShaderProperty, this.LinkedShaderProperty, OnSelectShaderProperty);
							if (menu != null)
							{
								menu.ShowAsContext();
							}
						}
					}
					EndHorizontal();

					GUILayout.Space(3);

					if (LinkedShaderProperty != null)
					{
						int SourceChannelsCount = 0;
						bool sourceIsColor = false;
						switch (LinkedShaderProperty.Type)
						{
							case VariableType.@float:
								SourceChannelsCount = 1;
								break;

							case VariableType.float2:
								SourceChannelsCount = 2;
								break;

							case VariableType.color:
								sourceIsColor = true;
								SourceChannelsCount = 3;
								break;

							case VariableType.float3:
								SourceChannelsCount = 3;
								break;

							case VariableType.float4:
								SourceChannelsCount = 4;
								break;

							case VariableType.color_rgba:
								sourceIsColor = true;
								SourceChannelsCount = 4;
								break;
						}

						BeginHorizontal();
						{
							bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_ShaderPropertyReference>().Channels;
							SGUILayout.InlineLabel("Swizzle", highlighted);

							if (usedByCustomCode)
							{
								using (new EditorGUI.DisabledScope(true))
								{
									GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
								}
							}
							else
							{

								string optionsStr = sourceIsColor ? "RGBA" : "XYZW";
								optionsStr = optionsStr.Substring(0, SourceChannelsCount);
								if (ChannelsCount == 1)
									Channels = SGUILayout.GenericSelector(optionsStr, Channels);
								else
									Channels = SGUILayout.GenericSwizzle(Channels, ChannelsCount, optionsStr);
							}
						}
						EndHorizontal();

						// errors
						if (_linkedShaderProperty == null)
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("No Shader Property defined.", MessageType.Error);
							}
							EndHorizontal();
						}
						else if (!_linkedShaderProperty.IsVisible())
						{
							BeginHorizontal();
							{
								TCP2_GUI.HelpBoxLayout("Invalid Shader Property defined.", MessageType.Error);
							}
							EndHorizontal();
						}
					}
				}

				public static GenericMenu CreateShaderPropertiesMenu(ShaderProperty parent, ShaderProperty selected, GenericMenu.MenuFunction2 selectCallback)
				{
					var menu = new GenericMenu();
					var shaderProperties = new List<ShaderProperty>(ShaderGenerator2.CurrentConfig.VisibleShaderProperties);
					shaderProperties.Sort((x, y) => string.Compare(x.Name, y.Name));
					if (shaderProperties != null && shaderProperties.Count > 0)
					{
						foreach (var sp in shaderProperties)
						{
							if (sp == parent)
								continue;

							string referenceError = IsReferencePossible(parent, sp);

							if (referenceError != "")
							{
								if (referenceError != null)
									menu.AddDisabledItem(new GUIContent(sp.Name + " " + referenceError));
								else
									menu.AddItem(new GUIContent(sp.Name), selected == sp, selectCallback, sp);
							}
						}
						return menu;
					}

					return null;
				}

				static bool CheckCyclicReferences(ShaderProperty parent, ShaderProperty reference)
				{
					//check cyclic references
					bool cyclic = false;
					foreach (var imp in reference.implementations)
					{
						var impSpRef = imp as Imp_ShaderPropertyReference;
						if (impSpRef != null)
						{
							if (impSpRef.Dependencies.Contains(parent))
							{
								return true;
							}
							else
							{
								foreach (var dependency in impSpRef.Dependencies)
								{
									cyclic |= CheckCyclicReferences(parent, dependency);
								}
							}
						}

						var impMpTex = imp as Imp_MaterialProperty_Texture;
						if (impMpTex != null && impMpTex.UvSource == Imp_MaterialProperty_Texture.UvSourceType.OtherShaderProperty)
						{
							if (impMpTex.Dependencies.Contains(parent))
							{
								return true;
							}
							else
							{
								foreach (var dependency in impMpTex.Dependencies)
								{
									cyclic |= CheckCyclicReferences(parent, dependency);
								}
							}
						}
					}
					return cyclic;
				}

				/// <summary>
				/// Verify that 'parent' can reference 'reference'
				/// </summary>
				/// <returns>null if the reference is allowed, an error message if not, an empty string if the reference should be hidden in the menus</returns>
				public static string IsReferencePossible(ShaderProperty parent, ShaderProperty reference)
				{
					//can't reference (from) a hook
					if (parent.isHook || reference.isHook)
						return "";
					//can't reference a fixed function value
					if (reference.Program == ProgramType.FixedFunction)
						return "";
					//disable properties that have a different bitmask (used in a different pass, so can't cross reference)
					if (parent.passBitmask != reference.passBitmask)
						return "(different pass)";
					//can't reference between vertex & fragment shaders
					if (parent.Program != reference.Program)
						return string.Format("({0} shader)", reference.Program.ToString().ToLowerInvariant());
					//cyclic reference
					if (CheckCyclicReferences(parent, reference))
						return "(cyclic reference)";
					// deferred sampling
					if (!string.IsNullOrEmpty(reference.preventReference))
						return reference.preventReference;

					return null;
				}

				void OnSelectShaderProperty(object sp)
				{
					LinkedShaderProperty = sp as ShaderProperty;
					ParentShaderProperty.CheckHash();
					ShaderGenerator2.NeedsHashUpdate = true;
				}

				void SetLinkedShaderProperty(ShaderProperty shaderProperty)
				{
					if (shaderProperty == LinkedShaderProperty)
						return;

					if (shaderProperty == ParentShaderProperty)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg("Shader Property Referenced implementation tried to reference its parent: '" + shaderProperty.Name + "'"));
						return;
					}

					//build dependencies list to check cyclic references
					Dependencies.Clear();
					foreach (var imp in shaderProperty.implementations)
					{
						var impSpRef = imp as Imp_ShaderPropertyReference;
						if (impSpRef != null)
							Dependencies.AddRange(impSpRef.Dependencies);
					}
					if (Dependencies.Contains(shaderProperty))
					{
						//cyclic reference: can happen if a template has incorrect values
						Debug.LogError(ShaderGenerator2.ErrorMsg("Cyclic reference between '" + this.ParentShaderProperty.Name + "' and '" + shaderProperty.Name + "'"));
						return;
					}
					Dependencies.Add(shaderProperty);

					//assign as new linked shader property
					_linkedShaderProperty = shaderProperty;
					LinkedShaderPropertyName = _linkedShaderProperty == null ? "" : _linkedShaderProperty.Name;

					if (shaderProperty == null)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg("Referenced ShaderProperty is null"));
						return;
					}

					//determine default swizzle value based on channels count & linked shader property available channels
					bool sourceIsColor = shaderProperty.Type == VariableType.color || shaderProperty.Type == VariableType.color_rgba;
					string options = sourceIsColor ? "RGBA" : "XYZW";
					switch (shaderProperty.Type)
					{
						case VariableType.@float: options = "X"; break;
						case VariableType.float2: options = "XY"; break;
						case VariableType.float3: options = "XYZ"; break;
						case VariableType.float4: options = "XYZW"; break;
						case VariableType.color: options = "RGB"; break;
						case VariableType.color_rgba: options = "RGBA"; break;
					}

					// set default channels, or preserve existing ones as far as possible (the implementation could have just been deserialized)
					var prevChannels = Channels;
					Channels = "";
					for (int i = 0; i < ChannelsCount; i++)
					{
						if (prevChannels != null && i < prevChannels.Length && options.Contains(prevChannels[i].ToString()))
							Channels += prevChannels[i];
						else
							Channels += options[i % options.Length];
					}
					DefaultChannels = Channels;
				}

				//Force updating the Shader Property hash once we've retrieved the correct Linked Shader Property
				public void ForceUpdateParentDefaultHash()
				{
					ParentShaderProperty.ForceUpdateDefaultHash();
				}
			}

			[Serialization.SerializeAs("imp_ct")]
			public class Imp_CustomMaterialProperty : Implementation
			{
				public static VariableType VariableCompatibility { get { return VariableTypeAll; } }
				public static string MenuLabel { get { return "Custom Material Property"; } }
				internal override string GUILabel() { return MenuLabel; }

				internal override OptionFeatures[] NeededFeatures()
				{
					if (LinkedCustomMaterialProperty != null)
					{
						return LinkedCustomMaterialProperty.NeededFeatures();
					}
					else
					{
						return base.NeededFeatures();
					}
				}

				CustomMaterialProperty _linkedCustomMaterialProperty;
				public CustomMaterialProperty LinkedCustomMaterialProperty
				{
					get { return _linkedCustomMaterialProperty; }
					set
					{
						_linkedCustomMaterialProperty = value;
						LinkedCustomMaterialPropertyName = _linkedCustomMaterialProperty == null ? "" : _linkedCustomMaterialProperty.PropertyName;
					}
				}
				[Serialization.SerializeAs("lct")] public string LinkedCustomMaterialPropertyName;
				[Serialization.SerializeAs("cc")] public int ChannelsCount = 4;
				[Serialization.SerializeAs("chan")] public string Channels = "RGBA";
				[Serialization.SerializeAs("avchan")] string AvailableChannels = "RGBA";
				string DefaultChannels = "RGBA";

				public override bool HasErrors { get { return base.HasErrors | LinkedCustomMaterialProperty == null | errorMessage != null; } }
				string errorMessage = null;
				public override void CheckErrors()
				{
					base.CheckErrors();

					// Specific combinations errors
					errorMessage = null;
					if (this.LinkedCustomMaterialProperty != null)
					{
						var imp_texture = this.LinkedCustomMaterialProperty.implementation as Imp_MaterialProperty_Texture;

						if (this.ParentShaderProperty.Program == ProgramType.Vertex
							&& imp_texture != null
							&& imp_texture.UvSource == Imp_MaterialProperty_Texture.UvSourceType.ScreenSpace)
						{
							// TODO is that stills true?
							errorMessage = "You can't use a texture with screen-space UV on a vertex Shader Property.";
						}

						/*
						if (this.ParentShaderProperty.Program == ProgramType.Vertex
							&& imp_texture != null
							&& imp_texture.UseWorldPosUV)
						{
							errorMessage = "You can't use a texture with world position UV on a vertex Shader Property.";
						}
						*/
					}
				}

				public Imp_CustomMaterialProperty(ShaderProperty shaderProperty) : base(shaderProperty)
				{
					InitChannelsCount();
					InitChannelsSwizzle();
				}

				void InitChannelsCount()
				{
					switch (ParentShaderProperty.Type)
					{
						case VariableType.@float: ChannelsCount = 1; break;
						case VariableType.float2: ChannelsCount = 2; break;
						case VariableType.color:
						case VariableType.float3: ChannelsCount = 3; break;
						case VariableType.color_rgba:
						case VariableType.float4: ChannelsCount = 4; break;
					}
				}

				public void InitChannelsSwizzle()
				{
					Channels = LinkedCustomMaterialProperty != null ? LinkedCustomMaterialProperty.GetChannelsForVariableType(ParentShaderProperty.Type) : "-";
					DefaultChannels = Channels;
					UpdateAvailableChannels();
				}

				void UpdateAvailableChannels()
				{
					if (LinkedCustomMaterialProperty == null)
					{
						AvailableChannels = "-";
						return;
					}

					string channels = LinkedCustomMaterialProperty.Channels;
					// hacky way to extract unique characters only
					string tmp = "";
					for (int i = 0; i < channels.Length; i++)
					{
						if (!tmp.Contains(channels[i].ToString()))
						{
							tmp += channels[i];
						}
					}
					AvailableChannels = tmp;
				}

				public override void OnPasted()
				{
					InitChannelsCount();
				}

				public bool willBeRemoved { get; private set; }
				public override void WillBeRemoved()
				{
					this.willBeRemoved = true;
					if (LinkedCustomMaterialProperty != null)
					{
						LinkedCustomMaterialProperty.implementation.WillBeRemoved();
					}
					base.WillBeRemoved();
				}

				internal override string PrintVariableFragment(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}{1}", LinkedCustomMaterialProperty.PrintVariableFragment(), channels);
				}

				internal override string PrintVariableVertex(string inputSource, string outputSource, string arguments)
				{
					var hideChannels = TryGetArgument("hide_channels", arguments);
					var channels = string.IsNullOrEmpty(hideChannels) ? "." + Channels.ToLowerInvariant() : "";
					return string.Format("{0}{1}", LinkedCustomMaterialProperty.PrintVariableVertex(), channels);
				}

				internal override void NewLineGUI(bool usedByCustomCode)
				{
					BeginHorizontal();
					ShaderGenerator2.ContextualHelpBox("Reference a Custom Material Property for this Shader Property. This is an easy way to define material properties that can be reused across the shader.\nFor example, you can embed 4 different masks into one texture, each mask being mapped to the R,G,B,A channels.");
					EndHorizontal();

					BeginHorizontal();
					{
						SGUILayout.InlineLabel("Custom Property");

						if (GUILayout.Button((LinkedCustomMaterialProperty != null) ? LinkedCustomMaterialProperty.Label : "None", SGUILayout.Styles.ShurikenPopup))
						{
							var menu = CreateCustomMaterialPropertiesMenu();
							menu.ShowAsContext();
						}
					}
					EndHorizontal();

					GUILayout.Space(3);

					BeginHorizontal();
					{
						bool highlighted = !IsDefaultImplementation ? Channels != DefaultChannels : Channels != GetDefaultImplementation<Imp_CustomMaterialProperty>().Channels;
						SGUILayout.InlineLabel("Swizzle", highlighted);

						if (usedByCustomCode)
						{
							using (new EditorGUI.DisabledScope(true))
							{
								GUILayout.Label(TCP2_GUI.TempContent("Defined in Custom Code"), SGUILayout.Styles.ShurikenValue, GUILayout.Height(16), GUILayout.ExpandWidth(false));
							}
						}
						else
						{
							if (ChannelsCount == 1)
								Channels = SGUILayout.GenericSelector(AvailableChannels, Channels);
							else
								Channels = SGUILayout.GenericSwizzle(Channels, ChannelsCount, AvailableChannels);
						}
					}
					EndHorizontal();

					if (LinkedCustomMaterialProperty == null)
					{
						BeginHorizontal();
						TCP2_GUI.HelpBoxLayout("No Custom Material Property defined!", MessageType.Error);
						EndHorizontal();
					}

					if (errorMessage != null)
					{
						BeginHorizontal();
						{
							TCP2_GUI.HelpBoxLayout(errorMessage, MessageType.Error);
						}
						EndHorizontal();
					}
				}

				GenericMenu CreateCustomMaterialPropertiesMenu()
				{
					var customTextures = ShaderGenerator2.CurrentConfig.CustomMaterialProperties;
					var menu = new GenericMenu();

					if (customTextures != null && customTextures.Length > 0)
					{
						foreach (var ct in customTextures)
						{
							menu.AddItem(new GUIContent(string.Format("{0} ({1})", ct.Label, ct.PropertyName)), LinkedCustomMaterialProperty == ct, OnSelectCustomTexture, ct);
						}
						return menu;
					}

					menu.AddDisabledItem(new GUIContent("No Custom Material Property defined"));
					return menu;
				}

				void OnSelectCustomTexture(object ct)
				{
					var customTexture = ct as CustomMaterialProperty;
					LinkedCustomMaterialProperty = customTexture;
					UpdateChannels();
					ShaderGenerator2.PushUndoState();
				}

				public void UpdateChannels()
				{
					UpdateAvailableChannels();

					// check that the current Channels only contains characters from the new available channels
					foreach (char c in Channels)
					{
						bool ok = false;
						foreach(var c2 in AvailableChannels)
						{
							if (c == c2)
							{
								ok = true;
								break;
							}
						}

						if (!ok)
						{
							InitChannelsSwizzle();
							return;
						}
					}
				}
			}
		}
	}
}
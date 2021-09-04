// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		// Utility to generate custom Toony Colors Pro 2 shaders with specific features

		//--------------------------------------------------------------------------------------------------
		// UI from Template System

		internal class UIFeature
		{
			protected const float LABEL_WIDTH = 210f;
			static Rect LastPositionInline;
			static float LastLowerBoundY;
			static float LastIndentY;
			static int LastIndent;
			static bool LastVisible;

			static GUIContent tempContent = new GUIContent();
			protected static GUIContent TempContent(string label, string tooltip = null)
			{
				tempContent.text = label;
				tempContent.tooltip = tooltip;
				return tempContent;
			}

			protected string label;
			protected string tooltip;
			protected string[] requires;    //features required for this feature to be enabled (AND)
			protected string[] requiresOr;  //features required for this feature to be enabled (OR)
			protected string[] excludes;   //features required to be OFF for this feature to be enabled
			protected string[] excludesAll;   //features required to be OFF for this feature to be enabled
			protected string[] visibleIf;   //features required to be ON for this feature to be visible
			protected bool showHelp = false;
			protected int indentLevel;
			protected string helpTopic;
			protected bool customGUI;   //complete custom GUI that overrides the default behaviors (e.g. separator)
			protected bool ignoreVisibility;   //ignore the current visible state and force the UI element to be drawn
			bool wasEnabled;    //track when the Enabled flag changes
			bool inline;        //draw next to previous position
			bool halfWidth;     //draw in half space of the position (for inline)

			UIFeature parent; // simple hierarchy system to handle visibility and vertical/horizontal line hierarchy drawing

			protected static Stack<bool> FoldoutStack = new Stack<bool>();
			internal static void ClearFoldoutStack()
			{
				UIFeature_DropDownStart.ClearDropDownsList();
				FoldoutStack.Clear();
			}

			//Initialize a UIFeature given a list of arbitrary properties
			internal UIFeature(List<KeyValuePair<string, string>> list)
			{
				if(list != null)
				{
					foreach(var kvp in list)
					{
						ProcessProperty(kvp.Key, kvp.Value);
					}
				}
			}

			//Process a property from the Template in the form key=value
			protected virtual void ProcessProperty(string key, string value)
			{
				//Direct inline properties, no need for a value
				if(string.IsNullOrEmpty(value))
				{
					switch(key)
					{
						case "nohelp": showHelp = false; break;
						case "indent": indentLevel = 1; break;
						case "inline": inline = true; break;
						case "half": halfWidth = true; break;
						case "help": showHelp = true; break;
					}
				}
				else
				{
					//Common properties to all UIFeature classes
					switch(key)
					{
						case "lbl": label = value.Replace("  ", "\n").Trim('"'); break;
						case "tt": tooltip = value.Replace(@"\n", "\n").Replace("  ", "\n").Trim('"'); break;
						case "help": showHelp = true; helpTopic = value; break;
						case "indent": indentLevel = int.Parse(value); break;
						case "needs": requires = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
						case "needsOr": requiresOr = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
						case "excl": excludes = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
						case "exclAll": excludesAll = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
						case "visibleIf": visibleIf = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
						case "inline": inline = bool.Parse(value); break;
						case "half": halfWidth = bool.Parse(value); break;
					}
				}
			}

			static Rect HeaderRect(ref Rect lineRect, float width)
			{
				var rect = lineRect;
				rect.width = width;

				lineRect.x += rect.width;
				lineRect.width -= rect.width;

				return rect;
			}

			// temp state between each DrawGUI, so that children don't have to
			// re-fetch them with the Enabled() and Highlighted() methods
			bool enabled;
			bool highlighted;
			internal void DrawGUI(Config config)
			{
				bool guiEnabled = GUI.enabled;

				// update states
				this.enabled = Enabled(config);
				this.highlighted = Highlighted(config);

				GUI.enabled = enabled;

				// by default, only show if top-level
				bool visible = indentLevel == 0;
				// if set, show all
				if (GlobalOptions.data.ShowDisabledFeatures)
				{
					visible = true;
				}
				// else, show only if parent is enabled & highlighted
				else if (indentLevel > 0 && parent != null)
				{
					if (visibleIf != null && visibleIf.Length > 0)
					{
						visible = config.HasFeaturesAll(visibleIf);
					}
					else
					{
						visible = parent.enabled && parent.highlighted;
					}
				}

				if(inline)
					visible = LastVisible;

				visible &= (FoldoutStack.Count > 0) ? FoldoutStack.Peek() : true;

				ForceValue(config);

				if(customGUI)
				{
					if(visible || ignoreVisibility)
					{
						DrawGUI(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 0), config, false);
						return;
					}
				}
				else if(visible)
				{
					//Total line rect
					Rect position;
					position = inline ? LastPositionInline : EditorGUILayout.GetControlRect();

					if(halfWidth)
					{
						position.width = (position.width/2f) - 8f;
					}

					//LastPosition is already halved
					if(inline)
					{
						position.x += position.width + 16f;
					}

					//Last Position for inlined properties
					LastPositionInline = position;

					if(!inline)
					{
						//Help
						if(showHelp)
						{
							var helpRect = HeaderRect(ref position, 20f);
							TCP2_GUI.HelpButtonSG2(helpRect, label, string.IsNullOrEmpty(helpTopic) ? label : helpTopic);
						}
						else
						{
							HeaderRect(ref position, 20f);
						}

						const float barIndent = 2;	// pixels for vertical bar indent
						const float uiIndent = 8;	// pixels per indent level for UI

						var horizontalRect = position;
						var lineColor = Color.black * (EditorGUIUtility.isProSkin ? 0.3f : 0.2f);
						for (int i = 1; i <= indentLevel; i++)
						{
							// vertical bar to the left of indented lines
							horizontalRect = position;
							if (indentLevel > 0 && Event.current.type == EventType.Repaint)
							{
								var verticalRect = position;
								verticalRect.width = 1;
								verticalRect.x += barIndent;
								verticalRect.yMax -= 7;
								verticalRect.yMin = (indentLevel <= 0 || i > LastIndent) ? LastLowerBoundY : LastIndentY;
								EditorGUI.DrawRect(verticalRect, lineColor);
							}

							// indent
							HeaderRect(ref position, uiIndent);

							// horizontal bar
							horizontalRect.width = horizontalRect.width - position.width;
							horizontalRect.height = 1;
							horizontalRect.xMin += barIndent + 1;
							horizontalRect.y += position.height/2;
							if (indentLevel > 0 && i == indentLevel && Event.current.type == EventType.Repaint)
							{
								EditorGUI.DrawRect(horizontalRect, lineColor);
							}
						}

						LastLowerBoundY = position.yMax;
						LastIndentY = horizontalRect.y;
						LastIndent = indentLevel;
					}

					//Label
					var guiContent = TempContent(label, tooltip);
					var labelPosition = HeaderRect(ref position, inline ? (EditorStyles.label.CalcSize(guiContent)).x + 8f : LABEL_WIDTH - position.x);
					TCP2_GUI.SubHeader(labelPosition, guiContent, this.highlighted && this.enabled);

					//Actual property
					bool labelClicked = Event.current.type == EventType.MouseUp && Event.current.button == 0 && labelPosition.Contains(Event.current.mousePosition);
					if (labelClicked)
					{
						Event.current.Use();
					}
					DrawGUI(position, config, labelClicked);

					LastVisible = visible;
				}

				GUI.enabled = guiEnabled;
			}

			//Internal DrawGUI: actually draws the feature
			protected virtual void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				GUI.Label(position, "Unknown feature type for: " + label);
			}

			//Defines if the feature is selected/toggle/etc. or not
			internal virtual bool Highlighted(Config config)
			{
				return false;
			}

			//Called when processing this UIFeature, in case any forced value needs to be set even if the UI component isn't visible
			internal virtual void ForceValue(Config config)
			{

			}

			//Called when Enabled(config) has changed state
			//Originally used to force Multiple UI to enable the default feature, if any
			protected virtual void OnEnabledChangedState(Config config, bool newState)
			{

			}

			internal bool Enabled(Config config)
			{
				var enabled = true;
				if(requiresOr != null)
				{
					enabled = false;
					enabled |= config.HasFeaturesAny(requiresOr);
				}
				if(excludesAll != null)
					enabled &= !config.HasFeaturesAll(excludesAll);
				if(requires != null)
					enabled &= config.HasFeaturesAll(requires);
				if(excludes != null)
					enabled &= !config.HasFeaturesAny(excludes);

				if(wasEnabled != enabled)
				{
					OnEnabledChangedState(config, enabled);
				}
				wasEnabled = enabled;

				return enabled;
			}

			//Parses a #FEATURES text block
			internal static UIFeature[] GetUIFeatures(string[] lines, ref int i, Template template)
			{
				var uiFeaturesList = new List<UIFeature>();
				string subline;
				do
				{
					subline = lines[i];
					i++;

					//Empty line
					if(string.IsNullOrEmpty(subline))
						continue;

					var data = subline.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

					//Skip empty or comment # lines
					if(data == null || data.Length == 0 || (data.Length > 0 && data[0].StartsWith("#")))
						continue;

					var kvpList = new List<KeyValuePair<string, string>>();
					for(var j = 1; j < data.Length; j++)
					{
						var sdata = data[j].Split('=');
						if(sdata.Length == 2)
							kvpList.Add(new KeyValuePair<string, string>(sdata[0], sdata[1]));
						else if(sdata.Length == 1)
							kvpList.Add(new KeyValuePair<string, string>(sdata[0], null));
						else
							Debug.LogError("Couldn't parse UI property from Template:\n" + data[j]);
					}

					// Discard the UIFeature if not for this template:
					var templateCompatibility = kvpList.Find(kvp => kvp.Key == "templates");
					if (templateCompatibility.Key == "templates")
					{
						if (!templateCompatibility.Value.Contains(template.id))
						{
							continue;
						}
					}

					UIFeature feature = null;
					switch(data[0])
					{
						case "---": feature = new UIFeature_Separator(); break;
						case "space": feature = new UIFeature_Space(kvpList); break;
						case "flag": feature = new UIFeature_Flag(kvpList, false); break;
						case "nflag": feature = new UIFeature_Flag(kvpList, true); break;
						case "float": feature = new UIFeature_Float(kvpList); break;
						case "int": feature = new UIFeature_Int(kvpList); break;
						case "subh": feature = new UIFeature_SubHeader(kvpList); break;
						case "header": feature = new UIFeature_Header(kvpList); break;
						case "warning": feature = new UIFeature_Warning(kvpList); break;
						case "sngl": feature = new UIFeature_Single(kvpList); break;
						case "gpu_inst_opt": feature = new UIFeature_Single(kvpList); break;
						case "mult": feature = new UIFeature_Multiple(kvpList); break;
						case "keyword": feature = new UIFeature_Keyword(kvpList); break;
						case "keyword_str": feature = new UIFeature_KeywordString(kvpList); break;
						case "dd_start": feature = new UIFeature_DropDownStart(kvpList); break;
						case "dd_end": feature = new UIFeature_DropDownEnd(); break;
						case "mult_fs": feature = new UIFeature_MultipleFixedFunction(kvpList); break;

						default: feature = new UIFeature(kvpList); break;
					}

					uiFeaturesList.Add(feature);
				}
				while(subline != "#END" && i < lines.Length);

				var uiFeaturesArray = uiFeaturesList.ToArray();

				// Build hierarchy from the parsed UIFeatures
				// note: simple hierarchy, where only a top-level element can be parent (one level only)
				UIFeature lastParent = null;
				for (int j = 0; j < uiFeaturesArray.Length; j++)
				{
					var uiFeature = uiFeaturesArray[j];
					if (uiFeature.indentLevel == 0 && !(uiFeature is UIFeature_Header) && !(uiFeature is UIFeature_Warning) && !uiFeature.inline)
					{
						lastParent = uiFeature;
					}
					else if (uiFeature.indentLevel > 0)
					{
						uiFeature.parent = lastParent;
					}
				}

				return uiFeaturesList.ToArray();
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// SINGLE FEATURE TOGGLE

		internal class UIFeature_Single : UIFeature
		{
			string keyword;
			string[] toggles;    //features forced to be toggled when this feature is enabled

			internal UIFeature_Single(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
					keyword = value;
				else if(key == "toggles")
					toggles = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var feature = Highlighted(config);
				EditorGUI.BeginChangeCheck();
				feature = EditorGUI.Toggle(position, feature);
				if (labelClicked)
				{
					feature = !feature;
					GUI.changed = true;
				}
				if(EditorGUI.EndChangeCheck())
				{
					config.ToggleFeature(keyword, feature);

					if(toggles != null)
					{
						foreach(var t in toggles)
							config.ToggleFeature(t, feature);
					}
				}
			}

			internal override bool Highlighted(Config config)
			{
				return config.HasFeature(keyword);
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// FEATURES COMBOBOX

		internal class UIFeature_Multiple : UIFeature
		{
			string[] labels;
			string[] features;
			string[] toggles;    //features forced to be toggled when this feature is enabled

			internal UIFeature_Multiple(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
				{
					var data = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					labels = new string[data.Length];
					features = new string[data.Length];

					for(var i = 0; i < data.Length; i++)
					{
						var lbl_feat = data[i].Split('|');
						if(lbl_feat.Length != 2)
						{
							Debug.LogWarning("[UIFeature_Multiple] Invalid data:" + data[i]);
							continue;
						}

						labels[i] = lbl_feat[0];
						features[i] = lbl_feat[1];
					}
				}
				else if(key == "toggles")
					toggles = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var feature = GetSelectedFeature(config);
				if(feature < 0) feature = 0;

				EditorGUI.BeginChangeCheck();
				feature = EditorGUI.Popup(position, feature, labels);
				if(EditorGUI.EndChangeCheck())
				{
					ToggleSelectedFeature(config, feature);
				}
			}

			int GetSelectedFeature(Config config)
			{
				for(var i = 0; i < features.Length; i++)
				{
					if(config.HasFeature(features[i]))
						return i;
				}

				return -1;
			}

			internal override bool Highlighted(Config config)
			{
				var feature = GetSelectedFeature(config);
				return feature > 0;
			}

			protected override void OnEnabledChangedState(Config config, bool newState)
			{
				var feature = -1;
				if(newState)
				{
					feature = GetSelectedFeature(config);
					if(feature < 0) feature = 0;
				}

				ToggleSelectedFeature(config, feature);
			}

			void ToggleSelectedFeature(Config config, int selectedFeature)
			{
				for(var i = 0; i < features.Length; i++)
				{
					var enable = (i == selectedFeature);
					config.ToggleFeature(features[i], enable);
				}

				if(toggles != null)
				{
					foreach(var t in toggles)
						config.ToggleFeature(t, selectedFeature > 0);
				}
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// FEATURES COMBOBOX for FIXED FUNCTION STATES
		// Embeds some UI from the corresponding Shader Property to easily change the states in the Features tab

		internal class UIFeature_MultipleFixedFunction : UIFeature
		{
			string keyword;
			string[] labels;
			string[] fixedFunctionValues;
			string shaderPropertyName;
			ShaderProperty shaderProperty;

			internal UIFeature_MultipleFixedFunction(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if (key == "kw")
				{
					keyword = value;
				}
				else if (key == "options")
				{
					var data = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					labels = new string[data.Length];
					fixedFunctionValues = new string[data.Length];

					for (var i = 0; i < data.Length; i++)
					{
						var lbl_feat = data[i].Split('|');
						if (lbl_feat.Length != 2)
						{
							Debug.LogWarning("[UIFeature_MultipleFixedFunction] Invalid data:" + data[i]);
							continue;
						}

						labels[i] = lbl_feat[0];
						fixedFunctionValues[i] = lbl_feat[1];
					}
				}
				else if (key == "shader_property")
				{
					shaderPropertyName = value.Replace("\"", "");
				}
				else
				{
					base.ProcessProperty(key, value);
				}
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				// Fetch embedded Shader Property
				bool highlighted = Highlighted(config);
				if (shaderProperty == null && highlighted) // the SP only exists if the feature is enabled
				{
					var match = Array.Find(config.AllShaderProperties, sp => sp.Name == shaderPropertyName);
					if (match == null)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg("Can't find matching embedded Shader Property with name: '" + shaderPropertyName + "'"));
					}
					shaderProperty = match;
				}

				int feature = highlighted ? (shaderProperty.implementations[0] as ShaderProperty.Imp_Enum).ValueType + 1 : 0;
				if (feature < 0) feature = 0;

				EditorGUI.BeginChangeCheck();
				feature = EditorGUI.Popup(position, feature, labels);
				if (EditorGUI.EndChangeCheck())
				{
					config.ToggleFeature(keyword, feature > 0);

					// Update Fixed Function value type
					var ffv = fixedFunctionValues[feature];
					if (feature > 0 && !string.IsNullOrEmpty(ffv) && shaderProperty != null)
					{
						(shaderProperty.implementations[0] as ShaderProperty.Imp_Enum).SetValueTypeFromString(ffv);
						shaderProperty.CheckHash();
						shaderProperty.CheckErrors();
					}
				}

				// Show embedded Shader Property UI
				if (highlighted && shaderProperty != null)
				{
					if (shaderProperty.Type != ShaderProperty.VariableType.fixed_function_enum)
					{
						EditorGUILayout.HelpBox("Embedded Shader Property should be a Fixed Function enum.", MessageType.Error);
					}
					else
					{
						var imp = shaderProperty.implementations[0] as ShaderProperty.Imp_Enum;
						if (imp == null)
						{
							EditorGUILayout.HelpBox("First implementation of enum Shader Property isn't an Imp_Enum.", MessageType.Error);
						}
						else
						{
							EditorGUI.BeginChangeCheck();
							{
								imp.EmbeddedGUI(28, 170);
							}
							if (EditorGUI.EndChangeCheck())
							{
								shaderProperty.CheckHash();
								shaderProperty.CheckErrors();
							}
						}
					}
				}
			}

			internal override bool Highlighted(Config config)
			{
				return config.HasFeature(keyword);
			}

			/*
			protected override void OnEnabledChangedState(Config config, bool newState)
			{
				var feature = -1;
				if (newState)
				{
					feature = GetSelectedFeature(config);
					if (feature < 0) feature = 0;
				}

				ToggleSelectedFeature(config, feature);
			}
			*/
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// KEYWORD COMBOBOX

		internal class UIFeature_Keyword : UIFeature
		{
			string keyword;
			string[] labels;
			string[] values;
			int defaultValue;
			bool forceValue;

			internal UIFeature_Keyword(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
					keyword = value;
				else if(key == "default")
					defaultValue = int.Parse(value, CultureInfo.InvariantCulture);
				else if(key == "forceKeyword")
					forceValue = bool.Parse(value);
				else if(key == "values")
				{
					var data = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					labels = new string[data.Length];
					values = new string[data.Length];

					for(var i = 0; i < data.Length; i++)
					{
						var lbl_feat = data[i].Split('|');
						if(lbl_feat.Length != 2)
						{
							Debug.LogWarning("[UIFeature_Keyword] Invalid data:" + data[i]);
							continue;
						}

						labels[i] = lbl_feat[0];
						values[i] = lbl_feat[1];
					}
				}
				else
					base.ProcessProperty(key, value);
			}

			internal override void ForceValue(Config config)
			{
				var selectedValue = GetSelectedValue(config);
				if(selectedValue < 0)
					selectedValue = defaultValue;

				if(forceValue && Enabled(config) && !config.HasKeyword(keyword))
				{
					config.SetKeyword(keyword, values[selectedValue]);
				}
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var selectedValue = GetSelectedValue(config);
				if(selectedValue < 0)
				{
					selectedValue = defaultValue;
					if(forceValue && Enabled(config))
					{
						config.SetKeyword(keyword, values[defaultValue]);
					}
				}

				EditorGUI.BeginChangeCheck();
				selectedValue = EditorGUI.Popup(position, selectedValue, labels);
				if(EditorGUI.EndChangeCheck())
				{
					if(string.IsNullOrEmpty(values[selectedValue]))
						config.RemoveKeyword(keyword);
					else
						config.SetKeyword(keyword, values[selectedValue]);
				}
			}

			int GetSelectedValue(Config config)
			{
				var currentValue = config.GetKeyword(keyword);
				for(var i = 0; i < values.Length; i++)
				{
					if(currentValue == values[i])
						return i;
				}

				return -1;
			}

			internal override bool Highlighted(Config config)
			{
				var feature = GetSelectedValue(config);
				return feature != defaultValue;
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// KEYWORD STRING

		internal class UIFeature_KeywordString : UIFeature
		{
			string keyword;
			string defaultValue;
			bool forceValue;

			internal UIFeature_KeywordString(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				switch(key)
				{
					case "kw": keyword = value; break;
					case "default": defaultValue = value.Trim('"'); break;
					case "forceKeyword": forceValue = bool.Parse(value); break;
					default: base.ProcessProperty(key, value); break;
				}
			}

			internal override void ForceValue(Config config)
			{
				if (forceValue && Enabled(config) && !config.HasKeyword(keyword))
				{
					config.SetKeyword(keyword, defaultValue);
				}
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				EditorGUI.BeginChangeCheck();
				string value = config.GetKeyword(keyword);
				if (string.IsNullOrEmpty(value))
				{
					value = defaultValue;
				}
				string newValue = EditorGUI.TextField(position, GUIContent.none, value);
				if (EditorGUI.EndChangeCheck())
				{
					if (newValue != value)
					{
						config.SetKeyword(keyword, newValue);
					}
				}
			}

			internal override bool Highlighted(Config config)
			{
				var value = config.GetKeyword(this.keyword);
				return !string.IsNullOrEmpty(value) && value != defaultValue;
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// SURFACE SHADER / GENERIC FLAG

		internal class UIFeature_Flag : UIFeature
		{
			bool negative;
			string keyword;
			string block = "pragma_surface_shader";
			string[] toggles;    //features forced to be toggled when this flag is enabled

			internal UIFeature_Flag(List<KeyValuePair<string, string>> list, bool negative) : base(list)
			{
				this.negative = negative;
				showHelp = false;
			}

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
					keyword = value;
				else if(key == "block")
					block = value.Trim('"');
				else if(key == "toggles")
					toggles = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var flag = Highlighted(config);
				EditorGUI.BeginChangeCheck();
				flag = EditorGUI.Toggle(position, flag);
				if (labelClicked)
				{
					flag = !flag;
					GUI.changed = true;
				}

				if(EditorGUI.EndChangeCheck())
				{
					UpdateConfig(config, flag);
				}
			}

			internal override bool Highlighted(Config config)
			{
				bool hasFlag = config.HasFlag(block, keyword);
				return negative ? !hasFlag : hasFlag;
			}

			void UpdateConfig(Config config, bool flag)
			{
				config.ToggleFlag(block, keyword, negative ? !flag : flag);

				if (toggles != null)
				{
					foreach (var t in toggles)
					{
						config.ToggleFeature(t, negative ? !flag : flag);
					}
				}
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// FIXED FLOAT

		internal class UIFeature_Float : UIFeature
		{
			string keyword;
			float defaultValue;
			float min = float.MinValue;
			float max = float.MaxValue;

			internal UIFeature_Float(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
					keyword = value;
				else if(key == "default")
					defaultValue = float.Parse(value, CultureInfo.InvariantCulture);
				else if(key == "min")
					min = float.Parse(value, CultureInfo.InvariantCulture);
				else if(key == "max")
					max = float.Parse(value, CultureInfo.InvariantCulture);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var currentValueStr = config.GetKeyword(keyword);
				var currentValue = defaultValue;
				if(!float.TryParse(currentValueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out currentValue))
				{
					currentValue = defaultValue;

					//Only enforce keyword if feature is enabled
					if (Enabled(config))
					{
						config.SetKeyword(keyword, currentValue.ToString("0.0###############", CultureInfo.InvariantCulture));
					}
				}

				EditorGUI.BeginChangeCheck();
				var newValue = currentValue;
				newValue = Mathf.Clamp(EditorGUI.FloatField(position, currentValue), min, max);
				if(EditorGUI.EndChangeCheck())
				{
					if(newValue != currentValue)
					{
						config.SetKeyword(keyword, newValue.ToString("0.0###############", CultureInfo.InvariantCulture));
					}
				}
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// FIXED INTEGER

		internal class UIFeature_Int : UIFeature
		{
			string keyword;
			int defaultValue;
			int min = int.MinValue;
			int max = int.MaxValue;

			internal UIFeature_Int(List<KeyValuePair<string, string>> list) : base(list) { }

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "kw")
					keyword = value;
				else if(key == "default")
					defaultValue = int.Parse(value, CultureInfo.InvariantCulture);
				else if(key == "min")
					min = int.Parse(value, CultureInfo.InvariantCulture);
				else if(key == "max")
					max = int.Parse(value, CultureInfo.InvariantCulture);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				var currentValueStr = config.GetKeyword(keyword);
				var currentValue = defaultValue;
				if(!int.TryParse(currentValueStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out currentValue))
				{
					currentValue = defaultValue;

					//Only enforce keyword if feature is enabled
					if (Enabled(config))
					{
						config.SetKeyword(keyword, currentValue.ToString(CultureInfo.InvariantCulture));
					}
				}

				EditorGUI.BeginChangeCheck();
				var newValue = currentValue;
				newValue = Mathf.Clamp(EditorGUI.IntField(position, currentValue), min, max);
				if(EditorGUI.EndChangeCheck())
				{
					if(newValue != currentValue)
					{
						config.SetKeyword(keyword, newValue.ToString(CultureInfo.InvariantCulture));
					}
				}
			}
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		// DECORATORS

		internal class UIFeature_Separator : UIFeature
		{
			internal UIFeature_Separator() : base(null)
			{
				customGUI = true;
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				TCP2_GUI.SeparatorSimple();
			}
		}

		internal class UIFeature_Space : UIFeature
		{
			float space = 8f;

			internal UIFeature_Space(List<KeyValuePair<string, string>> list) : base(list)
			{
				customGUI = true;
			}

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "space")
					space = float.Parse(value, CultureInfo.InvariantCulture);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				if(Enabled(config))
					GUILayout.Space(space);
			}
		}

		internal class UIFeature_SubHeader : UIFeature
		{
			internal UIFeature_SubHeader(List<KeyValuePair<string, string>> list) : base(list)
			{
				customGUI = true;
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				if (this.helpTopic != null)
				{
					EditorGUILayout.BeginHorizontal();
					{
						TCP2_GUI.HelpButtonSG2(this.helpTopic);
						TCP2_GUI.SubHeaderGray(label);
					}
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					TCP2_GUI.SubHeaderGray(label);
				}
			}
		}

		internal class UIFeature_Header : UIFeature
		{
			internal UIFeature_Header(List<KeyValuePair<string, string>> list) : base(list)
			{
				customGUI = true;
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				TCP2_GUI.Header(label);
			}
		}

		internal class UIFeature_Warning : UIFeature
		{
			MessageType msgType = MessageType.Warning;

			internal UIFeature_Warning(List<KeyValuePair<string, string>> list) : base(list)
			{
				customGUI = true;
			}

			protected override void ProcessProperty(string key, string value)
			{
				if(key == "msgType")
					msgType = (MessageType)Enum.Parse(typeof(MessageType), value, true);
				else
					base.ProcessProperty(key, value);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				if(Enabled(config))
				{
					//EditorGUILayout.HelpBox(this.label, msgType);
					TCP2_GUI.HelpBoxLayout(label, msgType);
				}
			}
		}

		internal class UIFeature_DropDownStart : UIFeature
		{
			static List<UIFeature_DropDownStart> AllDropDowns = new List<UIFeature_DropDownStart>();
			internal static void ClearDropDownsList()
			{
				AllDropDowns.Clear();
			}

			public bool foldout;
			public GUIContent guiContent = GUIContent.none;

			internal UIFeature_DropDownStart(List<KeyValuePair<string, string>> list) : base(list)
			{
				customGUI = true;
				ignoreVisibility = true;

				if(list != null)
				{
					foreach(var kvp in list)
					{
						if(kvp.Key == "lbl")
						{
							guiContent = new GUIContent(kvp.Value.Trim('"'));
						}
					}
				}

				foldout = ProjectOptions.data.OpenedFoldouts.Contains(guiContent.text);

				AllDropDowns.Add(this);
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				//Check if any feature within that Foldout are enabled, and show different color if so
				var hasToggledFeatures = false;
				var i = Array.IndexOf(Template.CurrentTemplate.uiFeatures, this);
				if(i >= 0)
				{
					for(i++; i < Template.CurrentTemplate.uiFeatures.Length; i++)
					{
						var uiFeature = Template.CurrentTemplate.uiFeatures[i];
						if(uiFeature is UIFeature_DropDownEnd)
							break;

						hasToggledFeatures |= uiFeature.Highlighted(config) && uiFeature.Enabled(config);
					}
				}

				var color = GUI.color;
				GUI.color *= EditorGUIUtility.isProSkin ? Color.white : new Color(.95f, .95f, .95f, 1f);
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				GUI.color = color;
				EditorGUI.BeginChangeCheck();
				{
					var rect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, TCP2_GUI.HeaderDropDownBold);

					// hover
					TCP2_GUI.DrawHoverRect(rect);

					foldout = TCP2_GUI.HeaderFoldoutHighlight(rect, foldout, guiContent, hasToggledFeatures);
					FoldoutStack.Push(foldout);
				}
				if(EditorGUI.EndChangeCheck())
				{
					UpdatePersistentState();

					if(Event.current.alt || Event.current.control)
					{
						var state = foldout;
						foreach(var dd in AllDropDowns)
						{
							dd.foldout = state;
							dd.UpdatePersistentState();
						}
					}
				}
			}

			public void UpdatePersistentState()
			{
				if(foldout && !ProjectOptions.data.OpenedFoldouts.Contains(guiContent.text))
					ProjectOptions.data.OpenedFoldouts.Add(guiContent.text);
				else if(!foldout && ProjectOptions.data.OpenedFoldouts.Contains(guiContent.text))
					ProjectOptions.data.OpenedFoldouts.Remove(guiContent.text);
			}
		}

		internal class UIFeature_DropDownEnd : UIFeature
		{
			internal UIFeature_DropDownEnd() : base(null)
			{
				customGUI = true;
				ignoreVisibility = true;
			}

			protected override void DrawGUI(Rect position, Config config, bool labelClicked)
			{
				FoldoutStack.Pop();

				EditorGUILayout.EndVertical();
			}
		}
	}
}
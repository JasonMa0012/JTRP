// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Represents a shader Template for the Shader Generator

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		internal class Template
		{
			internal static Template CurrentTemplate;

			internal TextAsset textAsset { get; private set; }
			internal bool valid { get; private set; }
			internal string[] originalTextLines;    //text lines with the MODULES keywords
			internal string[] textLines;            //text lines after being processed for the MODULES
			internal string templateInfo;
			internal string templateWarning;
			internal string templateType;
			internal string[] templateKeywords;
			internal string id;
			internal UIFeature[] uiFeatures;
			internal ShaderProperty[] shaderProperties;

			internal Template()
			{
				TryLoadTextAsset();
			}

			internal void SetTextAsset(TextAsset templateAsset)
			{
				valid = false;
				textAsset = templateAsset;
				if (templateAsset != null)
				{
					var assetPath = AssetDatabase.GetAssetPath(templateAsset);
					var osPath = Application.dataPath + "/" + assetPath.Substring("Assets/".Length);

					// verify that it's a valid SG2 template
					var lines = File.ReadAllLines(osPath);
					for (int i = 0; i < lines.Length; i++)
					{
						if (lines[i].StartsWith("#SG2"))
						{
							valid = true;
							break;
						}

						if (lines[i].StartsWith("#FEATURES"))
						{
							break;
						}
					}

					if (valid)
					{
						originalTextLines = lines;
					}

					UpdateTemplateMeta();
				}
			}

			internal void Reload()
			{
				UpdateTemplateMeta();
			}

			internal void ApplyForcedValues(Config config)
			{
				foreach (var uiFeature in uiFeatures)
				{
					uiFeature.ForceValue(config);
				}
			}

			internal void ApplyKeywords(Config config)
			{
				// clear previous keywords
				for (int i = config.Features.Count-1; i >= 0; i--)
				{
					if (config.Features[i].StartsWith("TEMPLATE_"))
					{
						config.Features.RemoveAt(i);
					}
				}

				if (templateKeywords == null)
				{
					return;
				}

				// add new keywords if any
				foreach (var kw in templateKeywords)
				{
					Utils.AddIfMissing(config.Features, kw);
				}
			}

			internal void FeaturesGUI(Config config)
			{
				if (uiFeatures == null)
				{
					EditorGUILayout.HelpBox("Couldn't parse the features from the Template.", MessageType.Error);
					return;
				}

				//Make the template accessible to UIFeatures (so that DropDown can iterate and know if any features inside are modified)
				CurrentTemplate = this;
				var length = uiFeatures.Length;
				for (var i = 0; i < length; i++)
				{
					uiFeatures[i].DrawGUI(config);
				}
			}

			//Try to load a Template according to a config type and/or file
			internal void TryLoadTextAsset(Config config = null)
			{
				var configFile = config != null ? config.templateFile : null;

				//Append file extension if necessary
				if (!string.IsNullOrEmpty(configFile) && !configFile.EndsWith(".txt"))
				{
					configFile = configFile + ".txt";
				}

				TextAsset loadedTextAsset = null;

				if (!string.IsNullOrEmpty(configFile))
				{
					var conf = LoadTextAsset(configFile);
					if (conf != null)
					{
						loadedTextAsset = conf;
						if (loadedTextAsset != null)
						{
							SetTextAsset(loadedTextAsset);
							return;
						}
					}
				}

				string defaultTemplate = "SG2_Template_Default.txt";
#if UNITY_2019_3_OR_NEWER
				if (Shader.globalRenderPipeline.Contains("UniversalPipeline"))
				{
					defaultTemplate = "SG2_Template_URP.txt";
				}
				else if (Shader.globalRenderPipeline == "LightweightPipeline")
				{
					defaultTemplate = "SG2_Template_LWRP.txt";
				}
#elif UNITY_5_6_OR_NEWER

				if (Shader.globalRenderPipeline == "LightweightPipeline")
				{
					defaultTemplate = "SG2_Template_LWRP.txt";
				}
#endif
				loadedTextAsset = LoadTextAsset(defaultTemplate);
				if (loadedTextAsset != null)
				{
					SetTextAsset(loadedTextAsset);
				}
			}

			//Returns an array of parsed lines based on the current features enabled, with their corresponding original line number (for error reporting)
			//Only keeps the lines necessary to generate the shader source, e.g. #FEATURES will be skipped
			//Conditions are now only processed in this function, all the other code should ignore them
			internal ParsedLine[] GetParsedLinesFromConditions(Config config, List<string> flags, Dictionary<string, List<string>> extraFlags)
			{
				var list = new List<ParsedLine>();

				int depth = -1;
				var stack = new List<bool>();
				var done = new List<bool>();
				var features = new List<string>(config.Features);
				int passIndex = -1;

				//clear optional features from shader properties options
				config.ClearShaderPropertiesFeatures();

				//make sure to use all needed features as config features for conditions
				var conditionFeatures = new List<string>(config.GetShaderPropertiesNeededFeaturesAll());
				conditionFeatures.AddRange(config.Features);
				conditionFeatures.AddRange(config.ExtraTempFeatures);

				//make sure keywords have been processed
				var keywordsFeatures = new List<string>();
				ProcessKeywordsBlock(config, conditionFeatures, keywordsFeatures, flags, extraFlags);
				features.AddRange(keywordsFeatures);

				//before first #PASS tag: use needed features from _all_ passes:
				//this is to make sure that the CGINCLUDE block with needed #VARIABLES:MODULES gets processed correctly
				features.AddRange(config.GetShaderPropertiesNeededFeaturesAll());
				features.AddRange(config.GetHooksNeededFeatures());

				//parse lines and strip based on conditions
				for (var i = 0; i < textLines.Length; i++)
				{
					var line = textLines[i];

					if (line.Length > 0 && line[0] == '#')
					{
						if (line.StartsWith("#PASS"))
						{
							//new pass: get the specific features for this pass
							passIndex++;
							features = new List<string>(config.Features);
							features.AddRange(config.GetHooksNeededFeatures());
							features.AddRange(config.GetShaderPropertiesNeededFeaturesForPass(passIndex));

							var passKeywordsFeatures = new List<string>();
							ProcessKeywordsBlock(config, features, passKeywordsFeatures, flags, extraFlags);
							features.AddRange(passKeywordsFeatures);
						}

						//Skip #FEATURES block
						if (line.StartsWith("#FEATURES"))
						{
							while (i < textLines.Length)
							{
								i++;
								line = textLines[i];
								if (line == "#END")
									break;
							}
						}
					}

					//Conditions
					if (line.Contains("///"))
					{
						var error = ExpressionParser.ProcessCondition(line, features, ref depth, ref stack, ref done);
						if (!string.IsNullOrEmpty(error))
						{
							Debug.LogError(ShaderGenerator2.ErrorMsg(error + "\n@ line " + i));
						}
					}
					//Regular line
					else
					{
						//Append line if inside valid condition block
						if ((depth >= 0 && stack[depth]) || depth < 0)
						{
							list.Add(new ParsedLine { line = line, lineNumber = i + 1 });
						}
					}
				}

				//error?
				if (depth >= 0)
				{
					//Analyze and try to find where the issue is
					var st = new Stack<ParsedLine>();
					for (var i = 0; i < textLines.Length; i++)
					{
						var tline = textLines[i].TrimStart();

						if (tline == "///")
							st.Pop();
						else if (tline.StartsWith("/// IF"))
							st.Push(new ParsedLine { line = textLines[i], lineNumber = i + 1 });
					}

					if (st.Count > 0)
					{
						var pl = st.Pop();
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Missing {0} ending '///' tag{1} at line {2}:\n{3}", depth + 1, depth > 0 ? "s" : "", pl.lineNumber, pl.line)));
					}
					else
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Missing {0} ending '///' tag{1}", depth + 1, depth > 0 ? "s" : "")));
				}

				return list.ToArray();
			}

			internal struct ParsedLine
			{
				internal string line;
				internal int lineNumber;

				public override string ToString()
				{
					return line;
				}
			}

			//--------

			private static TextAsset LoadTextAsset(string filename)
			{
				string rootPath = Utils.FindReadmePath(true);
				var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("{0}/Editor/Shader Templates/{1}", rootPath, filename));

				if (asset == null)
				{
					var filenameNoExtension = Path.GetFileNameWithoutExtension(filename);
					var guids = AssetDatabase.FindAssets(string.Format("{0} t:TextAsset", filenameNoExtension));
					if (guids.Length >= 1)
					{
						var path = AssetDatabase.GUIDToAssetPath(guids[0]);
						asset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
					}
					else
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find template using Unity's search system. Make sure that the file '{0}' is in the project!", filename)));
					}
				}

				return asset;
			}

			static void AddRangeWithIndent(List<string> list, string[] lines, string indent)
			{
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].StartsWith("#") && lines[i].Contains("_IMPL"))
					{
						// make sure #ENABLE_IMPL & #DISABLE_IMPL don't get indented, else they will end up in shader source
						list.Add(lines[i]);
					}
					else
					{
						list.Add(indent + lines[i]);
					}
				}
			}

			private void UpdateTemplateMeta()
			{
				uiFeatures = null;
				templateInfo = null;
				templateWarning = null;
				templateType = null;
				templateKeywords = null;
				id = null;

				UIFeature.ClearFoldoutStack();

				if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
				{
					//First pass: parse #MODULES and replace related keywords
					var newTemplateLines = new List<string>();
					Dictionary<string, Module> modules = new Dictionary<string, Module>();
					var usedModulesVariables = new HashSet<Module>();
					var usedModulesFunctions = new HashSet<Module>();
					var usedModulesInput = new HashSet<Module>();
					for (int i = 0; i < originalTextLines.Length; i++)
					{
						string line = originalTextLines[i];

						//Parse #MODULES
						if (line.StartsWith("#MODULES"))
						{
							//Iterate module names and try to find matching TextAssets
							while (line != "#END" && i < originalTextLines.Length)
							{
								line = originalTextLines[i];
								i++;

								if (line == "#END")
									break;

								if (line.StartsWith("//") || line.StartsWith("#") || string.IsNullOrEmpty(line))
									continue;

								try
								{
									var moduleName = line.Trim();
									var module = Module.CreateFromName(moduleName);
									if (module != null)
										modules.Add(moduleName, module);
								}
								catch (Exception e)
								{
									Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Parsing error in <b>#MODULES</b> block:\nLine: '{0}'\n'{1}'\n{2}", line, e.Message, e.StackTrace)));
								}
							}
						}

						//Replace module keywords
						if (line.Trim().StartsWith("[[MODULE") && i < originalTextLines.Length)
						{
							//extract indentation
							var indent = "";
							foreach (var c in line)
							{
								if (char.IsWhiteSpace(c))
									indent += c;
								else
									break;
							}

							var start = line.IndexOf("[[MODULE:");
							var end = line.LastIndexOf("]]");
							var tag = line.Substring(start + "[[MODULE:".Length, end - start - "[[MODULE:".Length);

							var moduleName = "";
							var key = "";
							if (tag.IndexOf(':') > 0)
							{
								moduleName = tag.Substring(tag.IndexOf(':') + 1);

								//remove arguments if any
								if (moduleName.Contains("("))
								{
									moduleName = moduleName.Substring(0, moduleName.IndexOf("("));
								}

								//extract key, if any
								int keyStart = moduleName.IndexOf(':');
								if (keyStart > 0)
								{
									key = moduleName.Substring(keyStart+1);
									moduleName = moduleName.Substring(0, keyStart);
								}
							}

							if (!string.IsNullOrEmpty(moduleName) && !modules.ContainsKey(moduleName))
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find module: '{0}' for '{1}'", moduleName, line.Trim())));
								continue;
							}

							if (tag.StartsWith("INPUT:"))
							{
								//Print Input block from specific module
								foreach (var module in modules.Values)
								{
									if (module.name == moduleName)
									{
										AddRangeWithIndent(newTemplateLines, module.InputStruct, indent);
										usedModulesInput.Add(module);
									}
								}

							}
							if (tag == "INPUT")
							{
								//Print all Input lines from all modules
								foreach (var module in modules.Values)
								{
									if (!usedModulesInput.Contains(module))
									{
										AddRangeWithIndent(newTemplateLines, module.InputStruct, indent);
									}
								}
							}
							else if (tag.StartsWith("FUNCTIONS:"))
							{
								//Print Functions line from specific module
								foreach (var module in modules.Values)
								{
									if (module.name == moduleName)
									{
										AddRangeWithIndent(newTemplateLines, module.Functions, indent);
										usedModulesFunctions.Add(module);
									}
								}
							}
							else if (tag == "FUNCTIONS")
							{
								//Print all Variables lines from all modules
								foreach (var module in modules.Values)
								{
									if (!usedModulesFunctions.Contains(module))
									{
										AddRangeWithIndent(newTemplateLines, module.Functions, indent);
									}
								}
							}
							else if (tag.StartsWith("VARIABLES:"))
							{
								//Print Variables line from specific module
								foreach (var module in modules.Values)
								{
									if (module.name == moduleName)
									{
										AddRangeWithIndent(newTemplateLines, module.Variables, indent);
										usedModulesVariables.Add(module);
									}
								}
							}
							else if (tag == "VARIABLES")
							{
								//Print all Variables lines from all modules
								foreach (var module in modules.Values)
								{
									if (!usedModulesVariables.Contains(module))
									{
										AddRangeWithIndent(newTemplateLines, module.Variables, indent);
									}
								}
							}
							else if (tag == "KEYWORDS")
							{
								//Print all Keywords lines from all modules
								foreach (var module in modules.Values)
								{
									AddRangeWithIndent(newTemplateLines, module.Keywords, indent);
								}
							}
							else if (tag.StartsWith("FEATURES:"))
							{
								AddRangeWithIndent(newTemplateLines, modules[moduleName].Features, indent);
							}
							else if (tag.StartsWith("PROPERTIES_NEW:"))
							{
								AddRangeWithIndent(newTemplateLines, modules[moduleName].PropertiesNew, indent);
							}
							else if (tag.StartsWith("PROPERTIES_BLOCK:"))
							{
								AddRangeWithIndent(newTemplateLines, modules[moduleName].PropertiesBlock, indent);
							}
							else if (tag.StartsWith("SHADER_FEATURES_BLOCK"))
							{
								AddRangeWithIndent(newTemplateLines, modules[moduleName].ShaderFeaturesBlock, indent);
							}
							else if (tag.StartsWith("VERTEX:"))
							{
								//Get arguments if any
								var args = new List<string>();
								int argStart = tag.IndexOf("(") + 1;
								int argEnd = tag.IndexOf(")");
								if (argStart > 0 && argEnd > 0)
								{
									string arguments = tag.Substring(argStart, argEnd - argStart);
									var argumentsSplit = arguments.Split(',');
									foreach (var a in argumentsSplit)
										args.Add(a.Trim());
								}

								AddRangeWithIndent(newTemplateLines, modules[moduleName].VertexLines(args, key), indent);
							}
							else if (tag.StartsWith("FRAGMENT:"))
							{
								//Get arguments if any
								var args = new List<string>();
								int argStart = tag.IndexOf("(") + 1;
								int argEnd = tag.IndexOf(")");
								if (argStart > 0 && argEnd > 0)
								{
									string arguments = tag.Substring(argStart, argEnd - argStart);
									var argumentsSplit = arguments.Split(',');
									foreach (var a in argumentsSplit)
										args.Add(a.Trim());
								}

								AddRangeWithIndent(newTemplateLines, modules[moduleName].FragmentLines(args, key), indent);
							}
						}
						else
							newTemplateLines.Add(line);
					}

					//Apply to textLines
					this.textLines = newTemplateLines.ToArray();

					//Second pass: parse other blocks
					for (int i = 0; i < textLines.Length; i++)
					{
						var line = textLines[i];
						if (line.StartsWith("#INFO="))
						{
							templateInfo = line.Substring("#INFO=".Length).TrimEnd().Replace("  ", "\n");
						}

						else if (line.StartsWith("#WARNING="))
						{
							templateWarning = line.Substring("#WARNING=".Length).TrimEnd().Replace("  ", "\n");
						}

						else if (line.StartsWith("#CONFIG="))
						{
							templateType = line.Substring("#CONFIG=".Length).TrimEnd().ToLower();
						}

						else if (line.StartsWith("#TEMPLATE_KEYWORDS="))
						{
							templateKeywords = line.Substring("#TEMPLATE_KEYWORDS=".Length).TrimEnd().Split(',');
						}

						else if (line.StartsWith("#ID="))
						{
							id = line.Substring("#ID=".Length).TrimEnd();
						}

						else if (line.StartsWith("#FEATURES"))
						{
							uiFeatures = UIFeature.GetUIFeatures(textLines, ref i, this);
						}

						else if (line.StartsWith("#PROPERTIES_NEW"))
						{
							shaderProperties = GetShaderProperties(textLines, i);
							return;
						}

						//Config meta should appear before the Shader name line
						else if (line.StartsWith("Shader"))
						{
							return;
						}
					}

					if (id == null)
					{
						Debug.LogWarning(ShaderGenerator2.ErrorMsg("Missing ID in template metadata!"));
					}
				}
			}

			//Get all Shader Properties regardless of conditions, only their visibility will be affected by the Config
			//This ensures that they are always in the correct order
			//Also link the pending Imp_ShaderPropertyReferences at this time, if any
			//and assign the correct pass bitmask based on usage
			static ShaderProperty[] GetShaderProperties(string[] lines, int i)
			{
				var shaderPropertiesList = new List<ShaderProperty>();
				string subline;
				do
				{
					subline = lines[i];
					i++;

					if (subline == "#END")
						break;

					if (subline.Trim().StartsWith("//") || subline.StartsWith("#") || string.IsNullOrEmpty(subline))
						continue;

					if (subline.Trim().StartsWith("header"))
						continue;

					try
					{
						var shaderProperty = ShaderProperty.CreateFromTemplateData(subline);
						shaderPropertiesList.Add(shaderProperty);
					}
					catch (Exception e)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Parsing error in <b>#PROPERTIES_NEW</b> block:\n\nError: '{0}'\n\nLine: '{1}'", e.ToString(), subline)));
					}
				}
				while (subline != "#END" && subline != null);

				//link shader property references
				foreach (var shaderProperty in shaderPropertiesList)
				{
					if (shaderProperty.implementations != null && shaderProperty.implementations.Count > 0)
					{
						foreach (var imp in shaderProperty.implementations)
						{
							var impSpRef = imp as ShaderProperty.Imp_ShaderPropertyReference;
							if (impSpRef != null && !string.IsNullOrEmpty(impSpRef.LinkedShaderPropertyName))
							{
								var match = shaderPropertiesList.Find(sp => sp.Name == impSpRef.LinkedShaderPropertyName);
								if (match != null)
								{
									var channels = impSpRef.Channels;
									impSpRef.LinkedShaderProperty = match;
									//restore channels from template data, it's up to the template to match the referenced shader property
									if (!string.IsNullOrEmpty(channels))
									{
										impSpRef.Channels = channels.ToUpperInvariant();
									}
									impSpRef.ForceUpdateParentDefaultHash();
								}
								else
								{
									Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find referenced Shader Property in template.\n'{0}' tried to reference '{1}'", shaderProperty.Name, impSpRef.LinkedShaderPropertyName)));
								}
							}

							var impMpTex = imp as ShaderProperty.Imp_MaterialProperty_Texture;
							if (impMpTex != null && impMpTex.UvSource == ShaderProperty.Imp_MaterialProperty_Texture.UvSourceType.OtherShaderProperty && !string.IsNullOrEmpty(impMpTex.LinkedShaderPropertyName))
							{
								// NOTE: same code as above, with variables changes for materialproperty_tex
								var match = shaderPropertiesList.Find(sp => sp.Name == impMpTex.LinkedShaderPropertyName);
								if (match != null)
								{
									var channels = impMpTex.UVChannels;
									impMpTex.LinkedShaderProperty = match;
									//restore channels from template data, it's up to the template to match the referenced shader property
									if (!string.IsNullOrEmpty(channels))
									{
										impMpTex.UVChannels = channels.ToUpperInvariant();
									}
									impMpTex.ForceUpdateParentDefaultHash();
								}
								else
								{
									Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find referenced Shader Property in template.\n'{0}' tried to reference '{1}'", shaderProperty.Name, impMpTex.LinkedShaderPropertyName)));
								}
							}
						}
					}
				}

				//iterate rest of template to check usage of each shader property per pass

				int currentPass = -1;
				for (; i < lines.Length; i++)
				{
					var line = lines[i].Trim();

					// update pass
					if (line.StartsWith("#PASS"))
					{
						currentPass++;
						continue;
					}

					// check value usage: used in which pass(es), and which generic implementation they can use
					var end = 0;
					while (line.IndexOf("[[", end) >= 0)
					{
						var start = line.IndexOf("[[", end);
						end = line.IndexOf("]]", end + 1);
						var tag = line.Substring(start + 2, end - start - 2);
						if (tag.StartsWith("VALUE:") || tag.StartsWith("SAMPLE_VALUE_SHADER_PROPERTY:"))
						{
							var propName = tag.Substring(tag.IndexOf(':') + 1);
							int argsStart = propName.IndexOf('(');
							if (argsStart > 0)
							{
								propName = propName.Substring(0, argsStart);
							}

							var sp = shaderPropertiesList.Find(x => x.Name == propName);
							if (sp != null)
							{
								// found used Shader Property
								sp.AddPassUsage(currentPass);
							}
							else
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("No match for used Shader Property in code: '<b>{0}</b>'", tag)));
							}
						}
					}
				}

				return shaderPropertiesList.ToArray();
			}

			internal ShaderProperty[] GetConditionalShaderProperties(ParsedLine[] parsedLines, out Dictionary<int, GUIContent> headers)
			{
				headers = new Dictionary<int, GUIContent>();

				var shaderPropertiesList = new List<ShaderProperty>();
				for (var i = 0; i < parsedLines.Length; i++)
				{
					var line = parsedLines[i].line;

					if (line.StartsWith("#PROPERTIES_NEW"))
					{
						while (i < parsedLines.Length)
						{
							line = parsedLines[i].line;
							i++;

							if (line.StartsWith("#END"))
								return shaderPropertiesList.ToArray();

							if (line.StartsWith("//") || line.StartsWith("#") || string.IsNullOrEmpty(line))
								continue;

							if (line.Trim().StartsWith("header"))
							{
								var data = line.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);
								var gc = new GUIContent(data[1], data.Length > 2 ? data[2].Trim('\"') : null);
								if (!headers.ContainsKey(shaderPropertiesList.Count))
								{
									headers.Add(shaderPropertiesList.Count, null);
								}
								headers[shaderPropertiesList.Count] = gc;	// only take the last one into account, so that empty headers will be ignored
								continue;
							}

							try
							{
								var shaderProperty = ShaderProperty.CreateFromTemplateData(line);
								var match = GetShaderPropertyByName(shaderProperty.Name);
								if (match == null)
									Debug.LogError(ShaderGenerator2.ErrorMsg("Can't find Shader Property in Template, yet it was found for Config"));
								else
									shaderPropertiesList.Add(match);
							}
							catch (Exception e)
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Parsing error in <b>#PROPERTIES_NEW</b> block:\n'{0}'\n{1}", e.Message, e.StackTrace)));
							}

						}
					}
				}

				return shaderPropertiesList.ToArray();
			}

			internal List<List<ShaderProperty>> FindUsedShaderPropertiesPerPass(ParsedLine[] parsedLines)
			{
				// Find used shader properties depending on the current pass, to extract used features per pass
				var shaderPropertiesPerPass = new List<List<ShaderProperty>>();
				// Find available Generic Implementations based on the current features
				ShaderProperty.Imp_GenericFromTemplate.InitList();
				int passIndex = -1;
				string program = "undefined";

				for (var i = 0; i < parsedLines.Length; i++)
				{
					var line = parsedLines[i].line.Trim();

					if (line.StartsWith("#PASS"))
					{
						passIndex++;
						shaderPropertiesPerPass.Add(new List<ShaderProperty>());
						continue;
					}

					if (line.StartsWith("#VERTEX"))
					{
						program = "vertex";
						continue;
					}

					if (line.StartsWith("#FRAGMENT"))
					{
						program = "fragment";
						continue;
					}

					if (line.StartsWith("#LIGHTING"))
					{
						program = "lighting";
						continue;
					}

					if (passIndex < 0)
					{
						continue;
					}

					// enabled generic implementation
					if (line.StartsWith("#ENABLE_IMPL"))
					{
						ShaderProperty.Imp_GenericFromTemplate.EnableFromLine(line, passIndex, program);
						continue;
					}
					// disabled generic implementation
					if (line.StartsWith("#DISABLE_IMPL"))
					{
						if (line.Contains("DISABLE_IMPL_ALL"))
						{
							ShaderProperty.Imp_GenericFromTemplate.DisableAll();
						}
						else
						{
							ShaderProperty.Imp_GenericFromTemplate.DisableFromLine(line, passIndex, program);
						}
						continue;
					}

					var end = 0;
					while (line.IndexOf("[[", end) >= 0)
					{
						var start = line.IndexOf("[[", end);
						end = line.IndexOf("]]", end + 1);
						var tag = line.Substring(start + 2, end - start - 2);
						if (tag.StartsWith("VALUE:") || tag.StartsWith("SAMPLE_VALUE_SHADER_PROPERTY:"))
						{
							var propName = tag.Substring(tag.IndexOf(':') + 1);
							int argsStart = propName.IndexOf('(');
							if (argsStart > 0)
							{
								propName = propName.Substring(0, argsStart);
							}

							var sp = GetShaderPropertyByName(propName);
							if (sp != null)
							{
								//add to used Shader Properties for current parsed pass
								if (!shaderPropertiesPerPass[passIndex].Contains(sp))
								{
									shaderPropertiesPerPass[passIndex].Add(sp);
								}

								ShaderProperty.Imp_GenericFromTemplate.AddCompatibleShaderProperty(sp);
							}
							else
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("No match for used Shader Property in code: '<b>{0}</b>'", tag)));
							}
						}
					}
				}

				ShaderProperty.Imp_GenericFromTemplate.ListCompleted();

				// Iterate through properties, and take into account referenced ones
				Action<ShaderProperty, List<ShaderProperty>> findAndAddLinkedShaderProperties = null;
				findAndAddLinkedShaderProperties = (sp, list) =>
				{
					foreach (var imp in sp.implementations)
					{
						var impSpRef = imp as ShaderProperty.Imp_ShaderPropertyReference;
						if (impSpRef != null)
						{
							// linked shader property can't be null during compilation, or something went wrong
							if (!list.Contains(impSpRef.LinkedShaderProperty))
							{
								list.Add(impSpRef.LinkedShaderProperty);

								// recursive
								findAndAddLinkedShaderProperties(impSpRef.LinkedShaderProperty, list);
							}
						}
						var impMpTex = imp as ShaderProperty.Imp_MaterialProperty_Texture;
						if (impMpTex != null && impMpTex.UvSource == ShaderProperty.Imp_MaterialProperty_Texture.UvSourceType.OtherShaderProperty)
						{
							if (impMpTex.LinkedShaderProperty == null)
							{
								continue;
							}

							if (!list.Contains(impMpTex.LinkedShaderProperty))
							{
								list.Add(impMpTex.LinkedShaderProperty);

								// recursive
								findAndAddLinkedShaderProperties(impMpTex.LinkedShaderProperty, list);
							}
						}
					}
				};
				for (int i = 0; i < shaderPropertiesPerPass.Count; i++)
				{
					var list = shaderPropertiesPerPass[i];
					foreach (var sp in list.ToArray())
					{
						findAndAddLinkedShaderProperties(sp, list);
					}
				}

				return shaderPropertiesPerPass;
			}

			ShaderProperty GetShaderPropertyByName(string name)
			{
				return Array.Find(shaderProperties, sp => sp.Name == name);
			}

			public void ResetShaderProperties()
			{
				foreach (var sp in shaderProperties)
				{
					sp.ResetDefaultImplementation();
				}
			}

			//Process the #KEYWORDS block for this config
			internal void ProcessKeywordsBlock(Config config, List<string> conditionalFeatures, List<string> tempFeatures, List<string> tempFlags, Dictionary<string, List<string>> tempExtraFlags)
			{
				var depth = -1;
				var stack = new List<bool>();
				var done = new List<bool>();

				for (var i = 0; i < textLines.Length; i++)
				{
					var line = textLines[i];

					if (line.Length <= 0 || line[0] != '#')
					{
						continue;
					}

					if (line.StartsWith("#KEYWORDS"))
					{
						int keywordsStartIndex = i+1;

						while (i < textLines.Length)
						{
							line = textLines[i];
							i++;

							if (line.Length > 0 && line[0] == '#' && line.StartsWith("#END"))
							{
								return;
							}

							//Conditions
							if (line.Contains("///"))
							{
								var error = ExpressionParser.ProcessCondition(line, conditionalFeatures, ref depth, ref stack, ref done);
								if (!string.IsNullOrEmpty(error))
									Debug.LogError(ShaderGenerator2.ErrorMsg(error));
							}
							//Regular line
							else
							{
								//Process line if inside valid condition block
								if ((depth >= 0 && stack[depth]) || depth < 0)
								{
									if (config.ProcessKeywords(line, tempFeatures, tempFlags, tempExtraFlags))
									{
										// add the new toggled features, if any
										foreach (var f in tempFeatures)
										{
											Utils.AddIfMissing(conditionalFeatures, f);
										}

										// reset the loop, so that the #keywords order doesn't matter in the template
										i = keywordsStartIndex;
										continue;
									}
								}
							}
						}
					}
				}
			}

			//Find out if current pass has a lighting function, to know if we need to generate surface output variables
			internal bool PassIsSurfaceShader(ParsedLine[] parsedLines, int pass)
			{
				int passIndex = -1;

				for (var i = 0; i < parsedLines.Length; i++)
				{
					var line = parsedLines[i].line.Trim();

					if(line.Length == 0 || line[0] != '#')
					{
						continue;
					}

					if (line.StartsWith("#PASS"))
					{
						passIndex++;
						if (passIndex > pass)
						{
							return false;
						}
					}

					if (passIndex == pass && line.Contains("#pragma surface"))
					{
						return true;
					}
				}
				return false;
			}

			//Process the #INPUT block: retrieve all necessary variables
			//for Input struct (surface shader) or v2f struct (vert/frag shader)
			internal string[] GetInputBlock(ParsedLine[] parsedLines, int pass)
			{
				var variablesList = new List<string>();
				int currentPass = -1;

				for (var i = 0; i < parsedLines.Length; i++)
				{
					var line = parsedLines[i].line;

					if (line.StartsWith("#PASS"))
						currentPass++;

					if (line.StartsWith("#INPUT_VARIABLES") && currentPass == pass)
					{
						i++;
						while (i < parsedLines.Length)
						{
							line = parsedLines[i].line;
							i++;

							if (line.StartsWith("#END"))
								return variablesList.ToArray();

							if (line.StartsWith("#") || string.IsNullOrEmpty(line.Trim()))
								continue;

							//Conditions
							if (line.Contains("///"))
							{
								Debug.LogError(ShaderGenerator2.ErrorMsg("GetInputBlock: template lines should already have been parsed and cleared of conditions"));
							}
							//Regular line
							else
							{
								variablesList.Add(line.Trim());
							}
						}
					}
				}

				return null;
			}
		}
	}
}
// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ToonyColorsPro.Utilities;
using System.Globalization;

// Helper functions related to shader generation and file saving

namespace ToonyColorsPro
{
	namespace Legacy
	{
		public static class TCP2_ShaderGeneratorUtils
		{
			private const string TCP2_PATH = "/JMO Assets/Toony Colors Pro/";
			public const string OUTPUT_PATH = TCP2_PATH + "Shaders Generated/";
			private const string INCLUDE_REL_PATH = "../Shaders/Include/";

			//--------------------------------------------------------------------------------------------------

			public static bool SelectGeneratedShader;
			public static bool CustomOutputDir;
			public static string _OutputPath;
			public static string OutputPath
			{
				get
				{
					if (!CustomOutputDir)
					{
						//TCP2 folder has been moved? Try to find new location
						if (!Directory.Exists(OUTPUT_PATH))
						{
							var rootPath = Utils.FindReadmePath(true).Substring("Assets".Length);
							if (!string.IsNullOrEmpty(rootPath))
							{
								return rootPath + "/Shaders Generated/";
							}
						}

						return OUTPUT_PATH;
					}

					//Custom output directory
					if (_OutputPath == null)
					{
						//Default output
						_OutputPath = EditorPrefs.GetString("TCP2_OutputPath", OUTPUT_PATH);
					}

					return _OutputPath;
				}
				set
				{
					if (_OutputPath != value)
					{
						if (!value.EndsWith("/"))
							value = value + "/";
						if (!value.StartsWith("/"))
							value = "/" + value;

						//try to get safe path name
						foreach (var c in Path.GetInvalidPathChars())
						{
							value = value.Replace(c.ToString(), "");
						}
						foreach (var c in Path.GetInvalidFileNameChars())
						{
							if (c == '/' || c == '\\')
								continue;
							value = value.Replace(c.ToString(), "");
						}

						_OutputPath = value;
						EditorPrefs.SetString("TCP2_OutputPath", _OutputPath);
					}
				}
			}

			private static string MakeRelativePath(string fromPath, string toPath)
			{
				var fromUri = new Uri(fromPath);
				var toUri = new Uri(toPath);

				// Path can't be made relative (shouldn't happen though!)
				if (fromUri.Scheme != toUri.Scheme)
					return INCLUDE_REL_PATH;

				var relativeUri = fromUri.MakeRelativeUri(toUri);
				var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

				return relativePath;
			}

			//--------------------------------------------------------------------------------------------------
			// GENERATION

			public static Shader Compile(TCP2_Config config, Shader existingShader, TCP2_ShaderGenerator.ShaderGeneratorTemplate template, bool showProgressBar = true, bool overwritePrompt = true)
			{
				return Compile(config, existingShader, template, showProgressBar ? 0f : -1f, overwritePrompt);
			}
			public static Shader Compile(TCP2_Config config, Shader existingShader, TCP2_ShaderGenerator.ShaderGeneratorTemplate template, float progress, bool overwritePrompt)
			{
				//UI
				if (progress >= 0f)
					EditorUtility.DisplayProgressBar("Hold On", "Generating Shader: " + config.ShaderName, progress);

				//Generate source
				string source = config.GenerateShaderSource(template, existingShader);
				if (string.IsNullOrEmpty(source))
				{
					Debug.LogError("[TCP2 Shader Generator] Can't save Shader: source is null or empty!");
					return null;
				}

				//Save to disk
				var shader = SaveShader(config, existingShader, source, overwritePrompt, config.isModifiedExternally && false);

				if (config.configType == "terrain")
				{
					//Generate Base shader
					var baseConfig = config.Copy();
					baseConfig.Filename = baseConfig.Filename + "_Base";
					baseConfig.ShaderName = "Hidden/" + baseConfig.ShaderName + "-Base";
					baseConfig.Features.Add("TERRAIN_BASE");

					source = baseConfig.GenerateShaderSource(template, existingShader);
					if (string.IsNullOrEmpty(source))
						Debug.LogError("[TCP2 Shader Generator] Can't save Terrain Base Shader: source is null or empty!");
					else
						SaveShader(baseConfig, existingShader, source, false, false);

					//Generate AddPass shader
					var addPassConfig = config.Copy();
					addPassConfig.Filename = addPassConfig.Filename + "_AddPass";
					addPassConfig.ShaderName = "Hidden/" + addPassConfig.ShaderName + "-AddPass";
					addPassConfig.Features.Add("TERRAIN_ADDPASS");
					addPassConfig.Flags.Add("decal:add");

					source = addPassConfig.GenerateShaderSource(template, existingShader);
					if (string.IsNullOrEmpty(source))
						Debug.LogError("[TCP2 Shader Generator] Can't save Terrain AddPass Shader: source is null or empty!");
					else
						SaveShader(addPassConfig, existingShader, source, false, false);
				}

				//UI
				if (progress >= 0f)
					EditorUtility.ClearProgressBar();

				return shader;
			}

			//Generate the source code for the shader as a string
			private static string GenerateShaderSource(this TCP2_Config config, TCP2_ShaderGenerator.ShaderGeneratorTemplate template, Shader existingShader = null)
			{
				if (config == null)
				{
					var error = "[TCP2 Shader Generator] Config file is null";
					Debug.LogError(error);
					return error;
				}

				if (template == null)
				{
					var error = "[TCP2 Shader Generator] Template is null";
					Debug.LogError(error);
					return error;
				}

				if (template.textAsset == null || string.IsNullOrEmpty(template.textAsset.text))
				{
					var error = "[TCP2 Shader Generator] Template string is null or empty";
					Debug.LogError(error);
					return error;
				}

				//------------------------------------------------
				// SHADER PARAMETERS

				//Masks
				bool mask1 = false, mask2 = false, mask3 = false, vcolors_mask = false, mainTex_mask = false;
				var mask1features = "";
				var mask2features = "";
				var mask3features = "";

				//Enable Masks according to their dependencies (new system using Template)
				foreach (var kvp in config.Keywords)
				{
					if (kvp.Value == "mask1")
					{
						var maskEnabled = template.GetMaskDependency(kvp.Key, config);
						mask1 |= maskEnabled;
						if (maskEnabled)
							mask1features += template.GetMaskDisplayName(kvp.Key) + ",";
					}
					else if (kvp.Value == "mask2")
					{
						var maskEnabled = template.GetMaskDependency(kvp.Key, config);
						mask2 |= maskEnabled;
						if (maskEnabled)
							mask2features += template.GetMaskDisplayName(kvp.Key) + ",";
					}
					else if (kvp.Value == "mask3")
					{
						var maskEnabled = template.GetMaskDependency(kvp.Key, config);
						mask3 |= maskEnabled;
						if (maskEnabled)
							mask3features += template.GetMaskDisplayName(kvp.Key) + ",";
					}
					else if (kvp.Value == "IN.color" || kvp.Value == "vcolors")
					{
						vcolors_mask |= template.GetMaskDependency(kvp.Key, config);
					}
					else if (kvp.Value == "mainTex")
					{
						mainTex_mask |= template.GetMaskDependency(kvp.Key, config);
					}
				}

				//Only enable Independent UVs if relevant Mask is actually enabled
				foreach (var kvp in config.Keywords)
				{
					if (kvp.Key == "UV_mask1")
					{
						config.ToggleFeature("UVMASK1", (kvp.Value == "Independent UV" || kvp.Value == "Independent UV0") && mask1);
						config.ToggleFeature("UVMASK1_UV2", kvp.Value == "Independent UV1" && mask1);
					}
					else if (kvp.Key == "UV_mask2")
					{
						config.ToggleFeature("UVMASK2", (kvp.Value == "Independent UV" || kvp.Value == "Independent UV0") && mask2);
						config.ToggleFeature("UVMASK2_UV2", kvp.Value == "Independent UV1" && mask2);
					}
					else if (kvp.Key == "UV_mask3")
					{
						config.ToggleFeature("UVMASK3", (kvp.Value == "Independent UV" || kvp.Value == "Independent UV0") && mask3);
						config.ToggleFeature("UVMASK3_UV2", kvp.Value == "Independent UV1" && mask3);
					}
				}
				mask1features = mask1features.TrimEnd(',');
				mask2features = mask2features.TrimEnd(',');
				mask3features = mask3features.TrimEnd(',');

				config.ToggleFeature("MASK1", mask1);
				config.ToggleFeature("MASK2", mask2);
				config.ToggleFeature("MASK3", mask3);
				config.ToggleFeature("VCOLORS_MASK", vcolors_mask);
				config.ToggleFeature("MASK_MAINTEX", mainTex_mask);

				//---

				var keywords = new Dictionary<string, string>(config.Keywords);
				var flags = new List<string>(config.Flags);
				var features = new List<string>(config.Features);

				//Unity version
#if UNITY_5_4_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_5_4");
#endif
#if UNITY_5_5_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_5_5");
#endif
#if UNITY_5_6_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_5_6");
#endif
#if UNITY_2017_1_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_2017_1");
#endif
#if UNITY_2018_1_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_2018_1");
#endif
#if UNITY_2018_2_OR_NEWER
				Utils.AddIfMissing(features, "UNITY_2018_2");
#endif

				//Masks
				keywords.Add("MASK1", mask1features);
				keywords.Add("MASK2", mask2features);
				keywords.Add("MASK3", mask3features);

				//Shader name
				keywords.Add("SHADER_NAME", config.ShaderName);

				//Include path
				var include = GetIncludePrefix(config) + GetIncludeRelativePath(config, existingShader).TrimEnd('/');
				keywords.Add("INCLUDE_PATH", include);

				//Shader Model target (old templates)
				if (!keywords.ContainsKey("SHADER_TARGET"))
				{
					var target = GetShaderTarget(config);
					keywords.Add("SHADER_TARGET", target);
					if (config.shaderTarget == 20)
					{
						Utils.AddIfMissing(features, "FORCE_SM2");
					}
				}

				//Generate Surface parameters
				var strFlags = ArrayToString(flags.ToArray(), " ");
				keywords.Add("SURF_PARAMS", strFlags);

				//------------------------------------------------
				// PARSING & GENERATION

				var sb = new StringBuilder();
				var templateLines = template.textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

				var depth = -1;
				var stack = new List<bool>();
				var done = new List<bool>();

				//Parse template file
				string line = null;
				for (var i = 0; i < templateLines.Length; i++)
				{
					line = templateLines[i];

					//Comment
					if (line.StartsWith("#"))
					{
						//Meta
						if (line.StartsWith("#CONFIG="))
						{
							config.configType = line.Substring(8).TrimEnd().ToLower();
						}

						//Features UI
						if (line.StartsWith("#FEATURES"))
						{
							while (i < templateLines.Length)
							{
								i++;
								if (templateLines[i] == "#END")
									break;
							}
							continue;
						}

						//Keywords
						if (line.StartsWith("#KEYWORDS"))
						{
							while (i < templateLines.Length)
							{
								i++;
								if (templateLines[i] == "#END")
									break;

								var error = ProcessKeywords(templateLines[i], config, ref features, ref flags, ref keywords, ref i, ref depth, ref stack, ref done);
								if (!string.IsNullOrEmpty(error))
								{
									return error;
								}
							}

							//Update Surface parameters
							strFlags = ArrayToString(flags.ToArray(), " ");
							if (keywords.ContainsKey("SURF_PARAMS"))
								keywords["SURF_PARAMS"] = strFlags;
							else
								keywords.Add("SURF_PARAMS", strFlags);
						}

						//Debugging
						if (line.StartsWith("#break"))
						{
							Debug.Log("[TCP2] Parse Break @ " + i);
						}

						continue;
					}

					//Line break
					if (string.IsNullOrEmpty(line) && ((depth >= 0 && stack[depth]) || depth < 0))
					{
						sb.AppendLine(line);
						continue;
					}

					//Conditions
					if (line.Contains("///"))
					{
						var error = ProcessCondition(line, ref features, ref i, ref depth, ref stack, ref done);
						if (!string.IsNullOrEmpty(error))
						{
							return error;
						}
					}
					//Regular line
					else
					{
						//Replace keywords
						line = ReplaceKeywords(line, keywords);

						//Append line if inside valid condition block
						if ((depth >= 0 && stack[depth]) || depth < 0)
						{
							sb.AppendLine(line);
						}
					}
				}

				if (depth >= 0)
				{
					Debug.LogWarning("[TCP2 Shader Generator] Missing " + (depth+1) + " ending '///' tags");
				}

				var sourceCode = sb.ToString();

				//Normalize line endings
				sourceCode = sourceCode.Replace("\r\n", "\n");

				return sourceCode;
			}

			//Process #KEYWORDS section from Template
			private static string ProcessKeywords(string line, TCP2_Config config, ref List<string> features, ref List<string> flags, ref Dictionary<string, string> keywords, ref int i, ref int depth, ref List<bool> stack, ref List<bool> done)
			{
				if (line.Contains("///"))
				{
					ProcessCondition(line, ref features, ref i, ref depth, ref stack, ref done);
				}
				//Regular line
				else
				{
					if (string.IsNullOrEmpty(line))
						return null;

					//Inside valid block
					if ((depth >= 0 && stack[depth]) || depth < 0)
					{
						var parts = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
						switch (parts[0])
						{
							case "set":
								if (keywords.ContainsKey(parts[1]))
									keywords[parts[1]] = parts[2];
								else
									keywords.Add(parts[1], parts[2]);
								break;

							case "enable_kw": Utils.AddIfMissing(features, parts[1]); break;
							case "disable_kw": Utils.RemoveIfExists(features, parts[1]); break;
							case "enable_flag": Utils.AddIfMissing(flags, parts[1]); break;
							case "disable_flag": Utils.RemoveIfExists(flags, parts[1]); break;

							//Keywords that will also be modified in the Config
							case "enable_kw_config":
								Utils.AddIfMissing(features, parts[1]);
								Utils.AddIfMissing(config.Features, parts[1]);
								break;

							case "disable_kw_config":
								Utils.RemoveIfExists(features, parts[1]);
								Utils.RemoveIfExists(config.Features, parts[1]);
								break;
						}
					}
				}

				return null;
			}

			private static string ProcessCondition(string line, ref List<string> features, ref int i, ref int depth, ref List<bool> stack, ref List<bool> done)
			{
				//Remove leading white spaces
				line = line.TrimStart();

				var parts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 1)  //END TAG
				{
					if (depth < 0)
					{
						var error = "[TCP2 Shader Generator] Found end tag /// without any beginning! Aborting shader generation.\n@ line: " + i;
						Debug.LogError(error);
						return error;
					}

					stack.RemoveAt(depth);
					done.RemoveAt(depth);
					depth--;
				}
				else if (parts.Length >= 2)
				{
					if (parts[1] == "IF")
					{
						var cond = EvaluateExpression(i, features, parts);

						depth++;
						stack.Add(cond && ((depth <= 0) ? true : stack[depth - 1]));
						done.Add(cond);
					}
					else if (parts[1] == "ELIF")
					{
						if (done[depth])
						{
							stack[depth] = false;
							return null;
						}

						var cond = EvaluateExpression(i, features, parts);

						stack[depth] = cond && ((depth <= 0) ? true : stack[depth - 1]);
						done[depth] = cond;
					}
					else if (parts[1] == "ELSE")
					{
						if (done[depth])
						{
							stack[depth] = false;
							return null;
						}

						stack[depth] = ((depth <= 0) ? true : stack[depth - 1]);
						done[depth] = true;
					}
				}

				return null;
			}

			//New evaluation system with parenthesis and complex expressions support
			private static bool EvaluateExpression(int lineNumber, List<string> features, params string[] conditions)
			{
				if (conditions.Length <= 2)
				{
					Debug.LogError("[TCP2 Shader Generator] Invalid condition block\n@ line " + lineNumber);
					return false;
				}

				var expression = "";
				for (var n = 2; n < conditions.Length; n++)
				{
					expression += conditions[n];
				}

				var result = false;
				try
				{
					ExpressionParser.ExpressionLeaf.EvaluateFunction evalFunc = s => HasEntry(features, s);
					result = ExpressionParser.EvaluateExpression(expression, evalFunc);
				}
				catch (Exception e)
				{
					Debug.LogError("[TCP2 Shader Generator] Incorrect condition in template file\n@ line " + lineNumber + "\n\nError returned:\n" + e.Message + "\n");
				}

				return result;
			}

			private static string ArrayToString(string[] array, string separator)
			{
				var str = "";
				foreach (var s in array)
				{
					str += s + separator;
				}

				if (str.Length > 0)
				{
					str = str.Substring(0, str.Length - separator.Length);
				}

				return str;
			}

			private static string ReplaceKeywords(string line, Dictionary<string, string> searchAndReplace)
			{
				if (line.IndexOf("@%") < 0)
				{
					return line;
				}

				foreach (var kv in searchAndReplace)
				{
					line = line.Replace("@%" + kv.Key + "%@", kv.Value);
				}

				return line;
			}

			//--------------------------------------------------------------------------------------------------
			// IO

			//Save .shader file
			private static Shader SaveShader(TCP2_Config config, Shader existingShader, string sourceCode, bool overwritePrompt, bool modifiedPrompt)
			{
				if (string.IsNullOrEmpty(config.Filename))
				{
					Debug.LogError("[TCP2 Shader Generator] Can't save Shader: filename is null or empty!");
					return null;
				}

				//Save file
				var outputPath = OutputPath;
				var filename = config.Filename;

				//Get existing shader exact path
				if (existingShader != null)
				{
					outputPath = GetExistingShaderPath(config, existingShader);
					/*
					if(config.Filename.Contains("/"))
					{
						filename = config.Filename.Substring(config.Filename.LastIndexOf('/')+1);
					}
					*/
				}

				var systemPath = Application.dataPath + outputPath;
				if (!Directory.Exists(systemPath))
				{
					Directory.CreateDirectory(systemPath);
				}

				var fullPath = systemPath + filename + ".shader";
				var overwrite = true;
				if (overwritePrompt && File.Exists(fullPath))
				{
					overwrite = EditorUtility.DisplayDialog("TCP2 : Shader Generation", "The following shader already exists:\n\n" + fullPath + "\n\nOverwrite?", "Yes", "No");
				}

				if (modifiedPrompt)
				{
					overwrite = EditorUtility.DisplayDialog("TCP2 : Shader Generation", "The following shader seems to have been modified externally or manually:\n\n" + fullPath + "\n\nOverwrite anyway?", "Yes", "No");
				}

				if (overwrite)
				{
					var directory = Path.GetDirectoryName(fullPath);
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					//Write file to disk
					File.WriteAllText(fullPath, sourceCode, Encoding.UTF8);
					AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

					//Import (to compile shader)
					var assetPath = fullPath.Replace(Application.dataPath, "Assets");

					var shader = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader)) as Shader;
					if (SelectGeneratedShader)
					{
						Selection.objects = new Object[] { shader };
					}

					//Set ShaderImporter userData
					var shaderImporter = ShaderImporter.GetAtPath(assetPath) as ShaderImporter;
					if (shaderImporter != null)
					{
						//Set default textures
						string[] names = new string[]
						{
					"_NoTileNoiseTex",
					"_Ramp"
						};
						Texture[] textures = new Texture[]
						{
					AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("af5515bfe14f1af4a9b8b3bf306b9261")),
					AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("ccad9b0732473ee4e95de81e50e9050f"))
						};
						shaderImporter.SetDefaultTextures(names, textures);

						//Get file hash to verify if it has been manually altered afterwards
						var shaderHash = GetShaderContentHash(shaderImporter);

						//Use hash if available, else use timestamp
						var customDataList = new List<string>();
						customDataList.Add(!string.IsNullOrEmpty(shaderHash) ? shaderHash : shaderImporter.assetTimeStamp.ToString());
						customDataList.Add(config.GetShaderTargetCustomData());
						var configTypeCustomData = config.GetConfigTypeCustomData();
						if (configTypeCustomData!= null)
							customDataList.Add(configTypeCustomData);
						customDataList.Add(config.GetConfigFileCustomData());

						var userData = config.ToUserData(customDataList.ToArray());
						shaderImporter.userData = userData;

						//Needed to save userData in .meta file
						AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
					}
					else
					{
						Debug.LogWarning("[TCP2 Shader Generator] Couldn't find ShaderImporter.\nMetadatas will be missing from the shader file.");
					}

					return shader;
				}

				return null;
			}

			//Returns hash of file content to check for manual modifications (with 'h' prefix)
			public static string GetShaderContentHash(ShaderImporter importer)
			{
				string shaderHash = null;
				var shaderFilePath = Application.dataPath.Replace("Assets", "") + importer.assetPath;
				if (File.Exists(shaderFilePath))
				{
					var shaderContent = File.ReadAllText(shaderFilePath);
					shaderHash = (shaderContent != null) ? string.Format("h{0}", shaderContent.GetHashCode().ToString("X")) : "";
				}

				return shaderHash;
			}

			//--------------------------------------------------------------------------------------------------
			// UTILS

			public static bool HasEntry(List<string> list, string entry)
			{
				return list.Contains(entry);
			}
			public static bool HasAnyEntries(List<string> list, params string[] entries)
			{
				foreach (var str in entries)
				{
					if (list.Contains(str))
						return true;
				}
				return false;
			}
			public static bool HasAllEntries(List<string> list, params string[] entries)
			{
				var hasAll = true;
				foreach (var str in entries)
					hasAll &= list.Contains(str);
				return hasAll;
			}

			public static void ToggleEntry(List<string> list, string entry, bool toggle)
			{
				if (toggle && !list.Contains(entry))
					list.Add(entry);
				else if (!toggle && list.Contains(entry))
					list.Remove(entry);
			}

			//-------------------------------------------------

			//Get Features array from ShaderImporter
			public static void ParseUserData(ShaderImporter importer, out List<string> Features)
			{
				string[] array;
				string[] dummy;
				Dictionary<string, string> dummyDict;
				ParseUserData(importer, out array, out dummy, out dummyDict, out dummy);
				Features = new List<string>(array);
			}
			public static void ParseUserData(ShaderImporter importer, out string[] Features, out string[] Flags, out Dictionary<string, string> Keywords, out string[] CustomData)
			{
				var featuresList = new List<string>();
				var flagsList = new List<string>();
				var customDataList = new List<string>();
				var keywordsDict = new Dictionary<string, string>();

				var data = importer.userData.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var d in data)
				{
					if (string.IsNullOrEmpty(d)) continue;

					switch (d[0])
					{
						//Features
						case 'F':
							if (d == "F") break;    //Prevent getting "empty" feature
							featuresList.Add(d.Substring(1));
							break;
						//Flags
						case 'f': flagsList.Add(d.Substring(1)); break;
						//Keywords
						case 'K':
							var kw = d.Substring(1).Split(':');
							if (kw.Length != 2)
							{
								Debug.LogError("[TCP2 Shader Generator] Error while parsing userData: invalid Keywords format.");
								Features = null; Flags = null; Keywords = null; CustomData = null;
								return;
							}
							else
							{
								keywordsDict.Add(kw[0], kw[1]);
							}
							break;
						//Custom Data
						case 'c': customDataList.Add(d.Substring(1)); break;
						//old format
						default: featuresList.Add(d); break;
					}
				}

				Features = featuresList.ToArray();
				Flags = flagsList.ToArray();
				Keywords = keywordsDict;
				CustomData = customDataList.ToArray();
			}

			//--------------------------------------------------------------------------------------------------
			// SHADER GENERATION

			private static string GetIncludePrefix(TCP2_Config config)
			{
				//Folder
				if (!config.Filename.Contains("/"))
					return "";

				var prefix = "";
				foreach (var c in config.Filename) if (c == '/') prefix += "../";
				return prefix;
			}

			private static string GetExistingShaderPath(TCP2_Config config, Shader existingShader)
			{
				//Override OutputPath if Shader already exists, to make sure we replace the original shader file
				var unityPath = AssetDatabase.GetAssetPath(existingShader);
				unityPath = unityPath.Replace(".shader", "");       //remove extension
				unityPath = Path.GetDirectoryName(unityPath);
				if (config.Filename.Contains("/"))
				{
					var filenamePath = Path.GetDirectoryName(config.Filename);
					if (unityPath.EndsWith(filenamePath))
						unityPath = unityPath.Substring(0, unityPath.LastIndexOf(filenamePath));  //remove subdirectories
				}
				unityPath = unityPath.Substring(6);                 //get only directory without leading "Assets"
				if (!unityPath.EndsWith("/"))
					unityPath = unityPath + "/";
				return unityPath;
			}

			private static string GetIncludeRelativePath(TCP2_Config config, Shader existingShader)
			{
				var outputPath = OutputPath;
				if (existingShader != null)
				{
					outputPath = GetExistingShaderPath(config, existingShader);
				}

				if (outputPath == OUTPUT_PATH)
					return INCLUDE_REL_PATH;
				var tcp2includeFile = Utils.GetFileSafe(Application.dataPath, "TCP2_Include.cginc");
				if (tcp2includeFile != null)
				{
					var absoluteTcp2IncludeDir = Path.GetDirectoryName(tcp2includeFile) + "/";
					var absoluteShaderPath = Application.dataPath + outputPath;
					var relativePath = MakeRelativePath(absoluteShaderPath, absoluteTcp2IncludeDir);
					if (outputPath != "/")
						relativePath = "../" + relativePath;
					return relativePath;
				}

				EditorApplication.Beep();
				Debug.LogError("[TCP2 Shader Generator] Can't find file 'TCP2_Include.cginc' in project!\nCan't figure out the relative include path to the generated shader.");
				return INCLUDE_REL_PATH;
			}

			private static string GetShaderTarget(TCP2_Config config)
			{
				return (config.shaderTarget/10f).ToString("0.0", CultureInfo.InvariantCulture);
			}
		}
	}
}
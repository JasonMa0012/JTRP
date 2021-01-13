// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

#define WRITE_UNCOMPRESSED_SERIALIZED_DATA

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Represents a Toony Colors Pro 2 configuration to generate the corresponding shader
// (new version for Shader Generator 2)

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		internal interface IMaterialPropertyName { string GetPropertyName(); }

		internal static class UniqueMaterialPropertyName
		{
			internal delegate bool CheckUniqueVariableName(string variableName, IMaterialPropertyName materialPropertyName);
			internal static event CheckUniqueVariableName checkUniqueVariableName;

			internal static string GetUniquePropertyName(string baseName, IMaterialPropertyName materialPropertyName)
			{
				if (checkUniqueVariableName == null)
				{
					return baseName;
				}

				//name doesn't exist: all good
				if (checkUniqueVariableName(baseName, materialPropertyName))
					return baseName;

				//extract the last digits of the name, if any
				for (var i = baseName.Length - 1; i >= 0; i--)
				{
					if (baseName[i] >= '0' && baseName[i] <= '9')
						continue;
					baseName = baseName.Substring(0, i + 1);
					break;
				}

				//check if name is unique: requires a class that registers to the event and supply its own checks
				var newName = baseName;
				var count = 1;
				while (!checkUniqueVariableName(newName, materialPropertyName))
				{
					newName = string.Format("{0}{1}", baseName, count);
					count++;
				}

				return newName;
			}
		}

		[Serialization.SerializeAs("config")]
		internal class Config
		{
#pragma warning disable 414
			[Serialization.SerializeAs("ver")] string tcp2version = ShaderGenerator2.TCP2_VERSION;
			[Serialization.SerializeAs("unity")] string unityVersion { get { return Application.unityVersion; } }
#pragma warning restore 414

			internal const string kSerializationPrefix = "/* TCP_DATA ";
			internal const string kSerializationPrefixUncompressed = "/* TCP_DATA u ";
			internal const string kSerializationSuffix = " */";

			internal const string kHashPrefix = "/* TCP_HASH ";
			internal const string kHashSuffix = " */";

			internal string Filename = "My TCP2 Shader";
			internal string ShaderName = "Toony Colors Pro 2/User/My TCP2 Shader";
			[Serialization.SerializeAs("tmplt")] internal string templateFile = "TCP2_ShaderTemplate_Default";
			[Serialization.SerializeAs("features")] internal List<string> Features = new List<string>();
			internal List<string> ExtraTempFeatures = new List<string>();
			[Serialization.SerializeAs("flags")] internal List<string> Flags = new List<string>();
			[Serialization.SerializeAs("flags_extra")] internal Dictionary<string, List<string>> FlagsExtra = new Dictionary<string, List<string>>();
			[Serialization.SerializeAs("keywords")] internal Dictionary<string, string> Keywords = new Dictionary<string, string>();
			internal bool isModifiedExternally = false;

			// UI list of Shader Properties
			struct ShaderPropertyGroup
			{
				public GUIContent header;
				public bool hasModifiedShaderProperties;
				public bool hasErrors;
				public List<ShaderProperty> shaderProperties;
			}
			List<ShaderPropertyGroup> shaderPropertiesUIGroups = new List<ShaderPropertyGroup>();
			Dictionary<string, bool> headersExpanded = new Dictionary<string, bool>(); // the struct array above is always recreated, so we can't track expanded state there
			List<ShaderProperty> visibleShaderProperties = new List<ShaderProperty>();
			//Serialize all cached Shader Properties so that their custom implementation is saved, even if they are not used in the shader
			[Serialization.SerializeAs("shaderProperties")] List<ShaderProperty> cachedShaderProperties = new List<ShaderProperty>();
			List<List<ShaderProperty>> shaderPropertiesPerPass;
			[Serialization.SerializeAs("customTextures")] List<ShaderProperty.CustomMaterialProperty> customMaterialPropertiesList = new List<ShaderProperty.CustomMaterialProperty>();
			ReorderableLayoutList customTexturesLayoutList = new ReorderableLayoutList();

			public ShaderProperty customMaterialPropertyShaderProperty = new ShaderProperty("_CustomMaterialPropertyDummy", ShaderProperty.VariableType.color_rgba);

			internal ShaderProperty.CustomMaterialProperty[] CustomMaterialProperties { get { return customMaterialPropertiesList.ToArray(); } }
			internal ShaderProperty[] VisibleShaderProperties { get { return visibleShaderProperties.ToArray(); } }
			internal ShaderProperty[] AllShaderProperties { get { return cachedShaderProperties.ToArray(); } }

			internal string[] GetShaderPropertiesNeededFeaturesForPass(int passIndex)
			{
				if (shaderPropertiesPerPass == null || shaderPropertiesPerPass.Count == 0)
					return new string[0];

				if (passIndex >= shaderPropertiesPerPass.Count)
					return new string[0];

				if (shaderPropertiesPerPass[passIndex] == null || shaderPropertiesPerPass[passIndex].Count == 0)
					return new string[0];

				var features = new List<string>();
				foreach (var sp in shaderPropertiesPerPass[passIndex])
					features.AddRange(sp.NeededFeatures());

				return features.Distinct().ToArray();
			}

			internal string[] GetShaderPropertiesNeededFeaturesAll()
			{
				if (shaderPropertiesPerPass == null || shaderPropertiesPerPass.Count == 0)
					return new string[0];

				// iterate through used Shader Properties for all passes and toggle needed features
				var features = new List<string>();
				foreach (var list in shaderPropertiesPerPass)
				{
					foreach (var sp in list)
					{
						features.AddRange(sp.NeededFeatures());
					}
				}

				return features.Distinct().ToArray();
			}

			internal string[] GetHooksNeededFeatures()
			{
				// iterate through Hook Shader Properties and toggle features if needed
				var features = new List<string>();
				foreach (var sp in visibleShaderProperties)
				{
					if (sp.isHook && !string.IsNullOrEmpty(sp.toggleFeatures))
					{
						if (sp.manuallyModified)
						{
							features.AddRange(sp.toggleFeatures.Split(','));
						}
					}
				}
				return features.ToArray();
			}

			/// <summary>
			/// Remove all features associated with specific Shader Property options,
			/// so that they don't stay when toggling an option on, compile, then off
			/// </summary>
			internal void ClearShaderPropertiesFeatures()
			{
				foreach (var f in ShaderProperty.AllOptionFeatures())
				{
					Utils.RemoveIfExists(this.Features, f);
				}
			}

			//--------------------------------------------------------------------------------------------------

			private enum ParseBlock
			{
				None,
				Features,
				Flags
			}

			internal static Config CreateFromFile(TextAsset asset)
			{
				return CreateFromFile(asset.text);
			}
			internal static Config CreateFromFile(string text)
			{
				var lines = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				var config = new Config();

				//Flags
				var currentBlock = ParseBlock.None;
				for (var i = 0; i < lines.Length; i++)
				{
					var line = lines[i];

					if (line.StartsWith("//")) continue;

					var data = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
					if (line.StartsWith("#"))
					{
						currentBlock = ParseBlock.None;

						switch (data[0])
						{
							case "#filename": config.Filename = data[1]; break;
							case "#shadername": config.ShaderName = data[1]; break;
							case "#features": currentBlock = ParseBlock.Features; break;
							case "#flags": currentBlock = ParseBlock.Flags; break;

							default: Debug.LogWarning("[TCP2 Shader Config] Unrecognized tag: " + data[0] + "\nline " + (i + 1)); break;
						}
					}
					else
					{
						if (data.Length > 1)
						{
							var enabled = false;
							bool.TryParse(data[1], out enabled);

							if (enabled)
							{
								if (currentBlock == ParseBlock.Features)
									config.Features.Add(data[0]);
								else if (currentBlock == ParseBlock.Flags)
									config.Flags.Add(data[0]);
								else
									Debug.LogWarning("[TCP2 Shader Config] Unrecognized line while parsing : " + line + "\nline " + (i + 1));
							}
						}
					}
				}

				return config;
			}

			internal static Config CreateFromShader(Shader shader)
			{
				var shaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;

				var config = new Config
				{
					ShaderName = shader.name,
					Filename = Path.GetFileName(AssetDatabase.GetAssetPath(shader)).Replace(".shader", "")
				};

				var valid = config.ParseUserData(shaderImporter);
				valid |= config.ParseSerializedDataAndHash(shaderImporter, null, false);    //first run (see method comment)

				if (valid)
					return config;
				return null;
			}

			internal Config Copy()
			{
				var config = new Config
				{
					Filename = Filename,
					ShaderName = ShaderName
				};

				foreach (var feature in Features)
					config.Features.Add(feature);

				foreach (var flag in Flags)
					config.Flags.Add(flag);

				foreach (var kvp in FlagsExtra)
					config.FlagsExtra.Add(kvp.Key, new List<string>(kvp.Value));

				foreach (var kvp in Keywords)
					config.Keywords.Add(kvp.Key, kvp.Value);

				config.templateFile = templateFile;

				return config;
			}

			//Copy implementations from this config to another
			public void CopyImplementationsTo(Config otherConfig)
			{
				for (int i = 0; i < this.cachedShaderProperties.Count; i++)
				{
					for (int j = 0; j < otherConfig.cachedShaderProperties.Count; j++)
					{
						if (this.cachedShaderProperties[i].Name == otherConfig.cachedShaderProperties[j].Name)
						{
							otherConfig.cachedShaderProperties[j].implementations = this.cachedShaderProperties[i].implementations;
							otherConfig.cachedShaderProperties[j].CheckHash();
							otherConfig.cachedShaderProperties[j].CheckErrors();
							break;
						}
					}
				}

				for (int i = 0; i < otherConfig.cachedShaderProperties.Count; i++)
				{
					otherConfig.cachedShaderProperties[i].ResolveShaderPropertyReferences();
				}
			}

			public void CopyCustomTexturesTo(Config otherConfig)
			{
				otherConfig.customMaterialPropertiesList = this.customMaterialPropertiesList;
				for (int i = 0; i < otherConfig.cachedShaderProperties.Count; i++)
				{
					otherConfig.cachedShaderProperties[i].ResolveShaderPropertyReferences();
				}
			}

			internal bool HasErrors()
			{
				foreach (var shaderProperty in visibleShaderProperties)
				{
					if (shaderProperty.error)
						return true;
				}

				foreach (var customTexture in CustomMaterialProperties)
				{
					if (customTexture.HasErrors)
						return true;
				}

				return false;
			}

			internal string GetConfigFileCustomData()
			{
				return string.Format("CF:{0}", templateFile);
			}

			internal int ToHash()
			{
				var sb = new StringBuilder();
				/*
				sb.Append(Filename);
				sb.Append(ShaderName);
				*/
				var orderedFeatures = new List<string>(Features);
				orderedFeatures.Sort();
				var orderedFlags = new List<string>(Flags);
				orderedFlags.Sort();
				var orderedFlagsExtra = new List<string>();
				foreach (var kvp in FlagsExtra)
					foreach (var flag in kvp.Value)
						orderedFlagsExtra.Add(flag);
				orderedFlagsExtra.Sort();
				var sortedKeywordsKeys = new List<string>(Keywords.Keys);
				sortedKeywordsKeys.Sort();
				var sortedKeywordsValues = new List<string>(Keywords.Values);
				sortedKeywordsValues.Sort();

				foreach (var f in orderedFeatures)
					sb.Append(f);
				foreach (var f in orderedFlags)
					sb.Append(f);
				foreach (var f in sortedKeywordsKeys)
					sb.Append(f);
				foreach (var f in sortedKeywordsValues)
					sb.Append(f);

				foreach (var sp in visibleShaderProperties)
					sb.Append(sp);
				foreach (var ct in customMaterialPropertiesList)
					sb.Append(ct);

				return sb.ToString().GetHashCode();
			}

			bool ParseUserData(ShaderImporter importer)
			{
				if (string.IsNullOrEmpty(importer.userData))
					return false;

				var data = importer.userData.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				var customDataList = new List<string>();

				foreach (var d in data)
				{
					if (string.IsNullOrEmpty(d)) continue;

					switch (d[0])
					{
						//Features
						case 'F':
							if (d == "F") break; //Prevent getting "empty" feature
							Features.Add(d.Substring(1));
							break;

						//Flags
						case 'f': Flags.Add(d.Substring(1)); break;

						//Keywords
						case 'K':
							var kw = d.Substring(1).Split(':');
							if (kw.Length != 2)
							{
								Debug.LogError("[TCP2 Shader Generator] Error while parsing userData: invalid Keywords format.");
								return false;
							}
							else
							{
								Keywords.Add(kw[0], kw[1]);
							}
							break;

						//Custom Data
						case 'c': customDataList.Add(d.Substring(1)); break;
						//old format
						default: Features.Add(d); break;
					}
				}

				foreach (var customData in customDataList)
				{
					//Configuration File
					if (customData.StartsWith("CF:"))
					{
						templateFile = customData.Substring(3);
					}
				}

				return true;
			}

			private static string CompressString(string uncompressed)
			{
				var bytes = Encoding.UTF8.GetBytes(uncompressed);
				using (var compressedStream = new MemoryStream())
				{
					using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress))
					{
						gZipStream.Write(bytes, 0, bytes.Length);
					}
					bytes = compressedStream.ToArray();
				}
				return Convert.ToBase64String(bytes);
			}

			private static string UncompressString(string compressed)
			{
				var bytes = Convert.FromBase64String(compressed);
				var buffer = new byte[4096];
				var uncompressedStream = new MemoryStream();
				using (var compressedStream = new MemoryStream(bytes))
				{
					using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
					{
						var length = 0;
						do
						{
							length = gZipStream.Read(buffer, 0, 4096);
							if (length > 0)
								uncompressedStream.Write(buffer, 0, length);
						}
						while (length > 0);
					}
				}

				return Encoding.UTF8.GetString(uncompressedStream.ToArray());
			}

			//New serialization format, embedded into the shader source in a comment
			internal string GetSerializedData()
			{
				var serialized = Serialization.Serialize(this);
#if WRITE_UNCOMPRESSED_SERIALIZED_DATA
				return kSerializationPrefixUncompressed + serialized + kSerializationSuffix;
#else
				return kSerializationPrefix + CompressString(serialized) + kSerializationSuffix;
#endif
			}

			//This method is executed twice because of an ordering problem:
			// - first run: it needs to fetch the template used from TCP_DATA
			// - then it loads that template and generate the serialized properties
			// - second run: now that the serialized properties exist, replace their implementations with the ones in TPC2_DATA
			internal bool ParseSerializedDataAndHash(ShaderImporter importer, Template template, bool dontRebuildCustomTextures)
			{
				//try to find serialized TCP2 data
				var unityPath = importer.assetPath;
				var osPath = Application.dataPath + "/" + unityPath.Substring("Assets/".Length);
				if (File.Exists(osPath))
				{
					var code = File.ReadAllLines(osPath);
					for (var i = code.Length - 1; i >= 0; i--)
					{
						var line = code[i].Trim();
						const string serializedPrefix = kSerializationPrefix;
						const string serializedPrefixU = kSerializationPrefixUncompressed;
						const string serializedSuffix = kSerializationSuffix;

						const string hashPrefix = kHashPrefix;
						const string hashSuffix = kHashSuffix;

						//hash is always inserted after serialized data, so the function shouldn't return without it being checked
						if (line.StartsWith(hashPrefix))
						{
							var hash = line.Substring(hashPrefix.Length, line.Length - hashPrefix.Length - hashSuffix.Length);

							//list of all lines, remove them from the end until the serialized prefix is found
							var codeLines = new List<string>(code);
							for (int j = codeLines.Count - 1; j >= 0; j--)
							{
								bool @break = codeLines[j].StartsWith(hashPrefix);
								codeLines.RemoveAt(j);
								if (@break)
									break;
							}

							var sb = new StringBuilder();
							foreach (var l in codeLines)
							{
								sb.AppendLine(l);
							}
							string normalizedLineEndings = sb.ToString().Replace("\r\n", "\n");
							var fileHash = ShaderGenerator2.GetHash(normalizedLineEndings);

							this.isModifiedExternally = string.Compare(fileHash, hash, StringComparison.Ordinal) != 0;
						}

						if (line.StartsWith(serializedPrefix) || line.StartsWith(serializedPrefixU))
						{
							string extractedData = line;
							int j = i;
							while (!extractedData.Contains(" */") && j < code.Length)
							{
								j++;
								if (j < code.Length)
								{
									line = code[j].Trim();
									extractedData += "\n" + line;
								}
								else
								{
									Debug.LogError(ShaderGenerator2.ErrorMsg("Incomplete serialized data in shader file."));
									return false;
								}
							}

							var serializedData = "";
							if (extractedData.StartsWith(serializedPrefixU))
							{
								serializedData = extractedData.Substring(serializedPrefixU.Length, extractedData.Length - serializedPrefixU.Length - serializedSuffix.Length);
							}
							else
							{
								serializedData = extractedData.Substring(serializedPrefix.Length, extractedData.Length - serializedPrefix.Length - serializedSuffix.Length);
								serializedData = UncompressString(serializedData);
							}

							return ParseSerializedData(serializedData, template, dontRebuildCustomTextures);
						}
					}
				}

				return false;
			}

			public bool ParseSerializedData(string serializedData, Template template, bool dontRebuildCustomTextures, bool resetEmptyImplementations = false)
			{
				Func<object, string, object> onDeserializeShaderPropertyList = (obj, data) =>
				{
					//called with data in format 'list[sp(field:value;field:value...),sp(field:value;...)]'

					// - make a new list, and pull matching sp from it
					// - reset the implementations of the remaining sp for the undo/redo system
					var shaderPropertiesTempList = new List<ShaderProperty>(cachedShaderProperties);

					var split = Serialization.SplitExcludingBlocks(data.Substring(5, data.Length - 6), ',', true, true, "()", "[]");
					foreach (var spData in split)
					{
						//try to match existing Shader Property by its name
						string name = null;

						//exclude 'sp(' and ')' and extract fields
						var vars = Serialization.SplitExcludingBlocks(spData.Substring(3, spData.Length - 4), ';', true, true, "()", "[]");
						foreach (var v in vars)
						{
							//find 'name' and remove 'name:' and quotes to extract value
							if (v.StartsWith("name:"))
								name = v.Substring(6, v.Length - 7);
						}

						if (name != null)
						{
							//find corresponding shader property, if it exists
							var matchedSp = shaderPropertiesTempList.Find(sp => sp.Name == name);

							//if no match, try to find it in the template's shader properties
							if (matchedSp == null && template != null)
							{
								matchedSp = Array.Find(template.shaderProperties, sp => sp.Name == name);
								if (matchedSp != null)
								{
									cachedShaderProperties.Add(matchedSp);
									shaderPropertiesTempList.Add(matchedSp);
								}
							}

							if (matchedSp != null)
							{
								shaderPropertiesTempList.Remove(matchedSp);

								Func<object, string, object> onDeserializeImplementation = (impObj, impData) =>
								{
									//make sure to deserialize as a new object, so that final Implementation subtype is kept instead of creating base Implementation class
									var imp = Serialization.Deserialize(impData, new object[] { matchedSp });

									//if custom material property, find the one with the matching serialized name
									if (imp is ShaderProperty.Imp_CustomMaterialProperty)
									{
										var ict = (imp as ShaderProperty.Imp_CustomMaterialProperty);
										var matchedCt = customMaterialPropertiesList.Find(ct => ct.PropertyName == ict.LinkedCustomMaterialPropertyName);
										//will be the match, or null if nothing found
										ict.LinkedCustomMaterialProperty = matchedCt;
										ict.UpdateChannels();
									}
									else if (imp is ShaderProperty.Imp_ShaderPropertyReference)
									{
										//find existing shader property and link it here
										//TODO: what if the shader property hasn't been deserialized yet?
										var ispr = (imp as ShaderProperty.Imp_ShaderPropertyReference);
										var channels = ispr.Channels;
										var matchedLinkedSp = visibleShaderProperties.Find(sp => sp.Name == ispr.LinkedShaderPropertyName);
										ispr.LinkedShaderProperty = matchedLinkedSp;
										//restore channels from serialized data (it is reset when assigning a new linked shader property)
										if (!string.IsNullOrEmpty(channels))
											ispr.Channels = channels;
									}
									else if (imp is ShaderProperty.Imp_MaterialProperty_Texture)
									{
										// find existing shader property for uv if that option is enabled, and link it
										var impt = (imp as ShaderProperty.Imp_MaterialProperty_Texture);
										var channels = impt.UVChannels;
										var matchedLinkedSp = visibleShaderProperties.Find(sp => sp.Name == impt.LinkedShaderPropertyName);
										impt.LinkedShaderProperty = matchedLinkedSp;
										//restore channels from serialized data (it is reset when assigning a new linked shader property)
										if (!string.IsNullOrEmpty(channels))
											impt.UVChannels = channels;
									}

									return imp;
								};

								var implementationHandling = new Dictionary<Type, Func<object, string, object>> { { typeof(ShaderProperty.Implementation), onDeserializeImplementation } };

								Serialization.DeserializeTo(matchedSp, spData, typeof(ShaderProperty), null, implementationHandling);

								matchedSp.CheckHash();
								matchedSp.CheckErrors();
							}
						}
					}

					if (resetEmptyImplementations)
					{
						foreach (var remainingShaderProperty in shaderPropertiesTempList)
						{
							remainingShaderProperty.ResetDefaultImplementation();
						}
					}

					return null;
				};

				try
				{
					var shaderPropertyHandling = new Dictionary<Type, Func<object, string, object>> { { typeof(List<ShaderProperty>), onDeserializeShaderPropertyList } };

					if (dontRebuildCustomTextures)
					{
						// if not building the custom material properties list, just skip its deserialization, else use the custom handling
						shaderPropertyHandling.Add(typeof(List<ShaderProperty.CustomMaterialProperty>), (obj, data) => { return null; });
					}
					Serialization.DeserializeTo(this, serializedData, GetType(), null, shaderPropertyHandling);

					return true;
				}
				catch (Exception e)
				{
					Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Deserialization error:\n'{0}'\n{1}", e.Message, e.StackTrace.Replace(Application.dataPath, ""))));
					return false;
				}
			}

			internal void AutoNames()
			{
				var rawName = ShaderName.Replace("Toony Colors Pro 2/", "");

				if (!ProjectOptions.data.SubFolders)
				{
					rawName = Path.GetFileName(rawName);
				}

				Filename = rawName;
			}

			//--------------------------------------------------------------------------------------------------
			// FEATURES

			internal bool HasFeature(string feature)
			{
				return Features.Contains(feature);
			}

			internal bool HasFeaturesAny(params string[] features)
			{
				foreach (var f in features)
				{
					if (Features.Contains(f))
					{
						return true;
					}
				}

				return false;
			}

			internal bool HasFeaturesAll(params string[] features)
			{
				foreach (var f in features)
				{
					if (f[0] == '!')
					{
						if (Features.Contains(f.Substring(1)))
						{
							return false;
						}
					}
					else
					{
						if (!Features.Contains(f))
						{
							return false;
						}
					}
				}

				return true;
			}

			internal void ToggleFeature(string feature, bool enable)
			{
				if (string.IsNullOrEmpty(feature))
					return;

				if (!Features.Contains(feature) && enable)
					Features.Add(feature);

				else if (Features.Contains(feature) && !enable)
					Features.Remove(feature);
			}

			//--------------------------------------------------------------------------------------------------
			// FLAGS

			internal bool HasFlag(string block, string flag)
			{
				if (block == "pragma_surface_shader")
				{
					return Flags.Contains(flag);
				}
				else
				{
					return FlagsExtra.ContainsKey(block) && FlagsExtra[block].Contains(flag);
				}
			}

			internal void ToggleFlag(string block, string flag, bool enable)
			{
				List<string> flagList = null;
				if (block == "pragma_surface_shader")
				{
					flagList = Flags;
				}
				else
				{
					if (!FlagsExtra.ContainsKey(block))
					{
						FlagsExtra.Add(block, new List<string>());
					}
					flagList = FlagsExtra[block];
				}

				if (!flagList.Contains(flag) && enable)			flagList.Add(flag);
				else if (flagList.Contains(flag) && !enable)	flagList.Remove(flag);
			}

			//--------------------------------------------------------------------------------------------------
			// KEYWORDS

			internal bool HasKeyword(string key)
			{
				return GetKeyword(key) != null;
			}

			internal string GetKeyword(string key)
			{
				if (key == null)
					return null;

				if (!Keywords.ContainsKey(key))
					return null;

				return Keywords[key];
			}

			internal void SetKeyword(string key, string value)
			{
				if (string.IsNullOrEmpty(value))
				{
					if (Keywords.ContainsKey(key))
						Keywords.Remove(key);
				}
				else
				{
					if (Keywords.ContainsKey(key))
						Keywords[key] = value;
					else
						Keywords.Add(key, value);
				}
			}

			internal void RemoveKeyword(string key)
			{
				if (Keywords.ContainsKey(key))
					Keywords.Remove(key);
			}

			//--------------------------------------------------------------------------------------------------
			// SHADER PROPERTIES / CUSTOM MATERIAL PROPERTIES

			void ExpandAllGroups()
			{
				var keys = headersExpanded.Keys.ToArray();
				foreach (var key in keys)
				{
					headersExpanded[key] = true;
				}
			}

			void FoldAllGroups()
			{
				var keys = headersExpanded.Keys.ToArray();
				foreach (var key in keys)
				{
					headersExpanded[key] = false;
				}
			}

			public string getHeadersExpanded()
			{
				string headersFoldout = "";
				foreach (var kvp in headersExpanded)
				{
					if (kvp.Value)
					{
						headersFoldout += kvp.Key + ",";
					}
				}
				return headersFoldout.TrimEnd(',');
			}

			public void setHeadersExpanded(string expandedHeaders)
			{
				var array = expandedHeaders.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				var keys = headersExpanded.Keys.ToArray();
				foreach (var key in keys)
				{
					headersExpanded[key] = Array.Exists(array, str => str == key);
				}
			}

			public string getShaderPropertiesExpanded()
			{
				string spExpanded = "";
				foreach (var sp in cachedShaderProperties)
				{
					if (sp.expanded)
					{
						spExpanded += sp.Name + ",";
					}
				}
				return spExpanded.TrimEnd(',');
			}

			public void setShaderPropertiesExpanded(string spExpanded)
			{
				var array = spExpanded.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var sp in cachedShaderProperties)
				{
					sp.expanded = Array.Exists(array, str => str == sp.Name);
				}
			}

			internal void ShaderPropertiesGUI()
			{
				GUILayout.Space(6);

				GUILayout.BeginHorizontal();

				// Expand / Fold All
				if (GUILayout.Button(TCP2_GUI.TempContent(" Expand All "), EditorStyles.miniButtonLeft))
				{
					ExpandAllGroups();
				}

				if (GUILayout.Button(TCP2_GUI.TempContent(" Fold All "), EditorStyles.miniButtonRight))
				{
					FoldAllGroups();
				}

				GUILayout.FlexibleSpace();

				// Reset All
				bool canReset = false;
				foreach (var sp in cachedShaderProperties)
				{
					if (sp.manuallyModified)
					{
						canReset = true;
						break;
					}
				}
				using (new EditorGUI.DisabledScope(!canReset))
				{
					if (GUILayout.Button(TCP2_GUI.TempContent(" Reset All "), EditorStyles.miniButton))
					{
						if (EditorUtility.DisplayDialog("Reset All Shader Properties", "All Custom Shader Properties will be cleared!\nThis can't be undone!\nProceed?", "Yes", "No"))
						{
							foreach (var sp in cachedShaderProperties)
							{
								sp.ResetDefaultImplementation();
							}
						}
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(4);
				if (ShaderGenerator2.ContextualHelpBox(
					"This section allows you to modify some shader properties that will be used in the shader, based on the features enabled in the corresponding tab.\nClick here to open the documentation and see some examples.",
					"shaderproperties"))
					GUILayout.Space(4);

				if (visibleShaderProperties.Count == 0)
				{
					EditorGUILayout.HelpBox("There are no shader properties for this template.", MessageType.Info);
				}
				else
				{
					for (int i = 0; i < shaderPropertiesUIGroups.Count; i++)
					{
						var group = shaderPropertiesUIGroups[i];

						if (group.header != null)
						{
							EditorGUI.BeginChangeCheck();

							// hover rect as in 2019.3 UI
							var rect = GUILayoutUtility.GetRect(group.header, EditorStyles.foldout, GUILayout.ExpandWidth(true));
							TCP2_GUI.DrawHoverRect(rect);
							rect.xMin += 4; // small left padding
							headersExpanded[group.header.text] = TCP2_GUI.HeaderFoldoutHighlightErrorGrayPosition(rect, headersExpanded[group.header.text], group.header, group.hasErrors, group.hasModifiedShaderProperties);

							if (EditorGUI.EndChangeCheck())
							{
								// expand/fold all when alt/control is held
								if (Event.current.alt || Event.current.control)
								{
									if (headersExpanded[group.header.text])
									{
										ExpandAllGroups();
									}
									else
									{
										FoldAllGroups();
									}
								}
							}
						}

						if (group.header == null || headersExpanded[group.header.text])
						{
							foreach (var sp in group.shaderProperties)
							{
								sp.ShowGUILayout(14);
							}
						}
					}
				}

				// Custom Material Properties
				if (visibleShaderProperties.Count > 0)
				{
					GUILayout.Space(4);
					TCP2_GUI.SeparatorSimple();
					GUILayout.Label("Custom Material Properties", EditorStyles.boldLabel);
					GUILayout.Space(2);
					if (ShaderGenerator2.ContextualHelpBox(
						"You can define your own material properties here, that can then be shared between multiple Shader Properties. For example, this can allow you to pack textures however you want, having a mask for each R,G,B,A channel.",
						"custommaterialproperties"))
					{
						GUILayout.Space(4);
					}

					if (customMaterialPropertiesList == null || customMaterialPropertiesList.Count == 0)
					{
						EditorGUILayout.HelpBox("No custom material properties defined.", MessageType.Info);
						var rect = GUILayoutUtility.GetLastRect();

						const float buttonWidth = 48;
						rect.width -= buttonWidth;
						rect.x += rect.width;
						rect.width = buttonWidth - 4;
						rect.yMin += 4;
						rect.yMax -= 4;
						if (GUI.Button(rect, "Add"))
						{
							ShowCustomMaterialPropertyMenu(0);
						}
					}
					else
					{
						//button callbacks
						ShaderProperty.CustomMaterialProperty.ButtonClick onAdd = index => ShowCustomMaterialPropertyMenu(index);
						ShaderProperty.CustomMaterialProperty.ButtonClick onRemove = index =>
						{
							customMaterialPropertiesList[index].WillBeRemoved();
							customMaterialPropertiesList.RemoveAt(index);
						};

						//draw element callback
						Action<int, float> DrawCustomTextureItem = (index, margin) =>
						{
							customMaterialPropertiesList[index].ShowGUILayout(index, onAdd, onRemove);
						};

						customTexturesLayoutList.DoLayoutList(DrawCustomTextureItem, customMaterialPropertiesList, new RectOffset(2, 0, 0, 2));
					}
				}
			}

			void ShowCustomMaterialPropertyMenu(int index)
			{
				var menu = new GenericMenu();
				var impType = typeof(ShaderProperty.Imp_MaterialProperty);
				var subTypes = impType.Assembly.GetTypes().Where(type => type.IsSubclassOf(impType));
				foreach (var type in subTypes)
				{
					var menuLabel = type.GetProperty("MenuLabel", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
					string label = (string)menuLabel.GetValue(null, null);
					label = label.Replace("Material Property/", "");
					menu.AddItem(new GUIContent(label), false, OnAddCustomMaterialProperty, new object[] { index, type });
				}
				menu.ShowAsContext();
			}

			void OnAddCustomMaterialProperty(object data)
			{
				var array = (object[])data;
				int index = (int)array[0];
				var type = (Type)array[1];

				if (customMaterialPropertiesList.Count == 0)
				{
					customMaterialPropertiesList.Add(CreateUniqueCustomTexture(type));
				}
				else
				{
					customMaterialPropertiesList.Insert(index + 1, CreateUniqueCustomTexture(type));
				}

				ShaderGenerator2.PushUndoState();
			}

			//Get a Shader Property from the list by its name
			internal ShaderProperty GetShaderPropertyByName(string name)
			{
				foreach (var sp in visibleShaderProperties)
					if (sp.Name == name)
						return sp;

				return null;
			}

			//Check if the supplied property name is unique
			internal bool IsUniquePropertyName(string name, IMaterialPropertyName propertyName)
			{
				//check existing Shader Properties of Material Property type
				foreach (var sp in visibleShaderProperties)
				{
					foreach (var imp in sp.implementations)
					{
						var mp = imp as ShaderProperty.Imp_MaterialProperty;
						if (mp != null && mp is IMaterialPropertyName && mp != propertyName && mp.PropertyName == name)
							return false;
					}
				}

				//check Custom Material Properties
				foreach (var ct in customMaterialPropertiesList)
					if (ct != propertyName && ct.PropertyName == name)
						return false;

				return true;
			}

			ShaderProperty.CustomMaterialProperty CreateUniqueCustomTexture(Type impType)
			{
				return new ShaderProperty.CustomMaterialProperty(this.customMaterialPropertyShaderProperty, impType);
			}

			internal void ClearShaderProperties()
			{
				this.cachedShaderProperties.Clear();
				this.visibleShaderProperties.Clear();
			}

			//Update available Shader Properties based on conditions
			internal void UpdateShaderProperties(Template template)
			{
				//Add Unity versions to features
#if UNITY_5_4_OR_NEWER
				Utils.AddIfMissing(Features, "UNITY_5_4");
#endif
#if UNITY_5_5_OR_NEWER
				Utils.AddIfMissing(Features, "UNITY_5_5");
#endif
#if UNITY_5_6_OR_NEWER
				Utils.AddIfMissing(Features, "UNITY_5_6");
#endif
#if UNITY_2017_1_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2017_1");
#endif
#if UNITY_2018_1_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2018_1");
#endif
#if UNITY_2018_2_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2018_2");
#endif
#if UNITY_2018_3_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2018_3");
#endif
#if UNITY_2019_1_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2019_1");
#endif
#if UNITY_2019_2_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2019_2");
#endif
#if UNITY_2019_3_OR_NEWER
				Utils.AddIfMissing(this.Features, "UNITY_2019_3");
#endif
				var parsedLines = template.GetParsedLinesFromConditions(this, null, null);

				//Clear arrays: will be refilled with the template's shader properties
				visibleShaderProperties.Clear();
				Dictionary<int, GUIContent> shaderPropertiesHeaders;
				visibleShaderProperties.AddRange(template.GetConditionalShaderProperties(parsedLines, out shaderPropertiesHeaders));
				foreach (var sp in visibleShaderProperties)
				{
					//add to the cached properties, to be found back if needed (in case of features change)
					if (!cachedShaderProperties.Contains(sp))
					{
						cachedShaderProperties.Add(sp);
					}

					// resolve linked shader property references now that all visible shader properties are known
					sp.ResolveShaderPropertyReferences();

					sp.onImplementationsChanged -= onShaderPropertyImplementationsChanged; // lazy way to make sure we don't subscribe more than once
					sp.onImplementationsChanged += onShaderPropertyImplementationsChanged;
				}

				//Find used shader properties per pass, to extract used features for each
				shaderPropertiesPerPass = template.FindUsedShaderPropertiesPerPass(parsedLines);

				// Build list of shader properties and headers for the UI
				shaderPropertiesUIGroups.Clear();
				ShaderPropertyGroup currentGroup = new ShaderPropertyGroup()
				{
					shaderProperties = new List<ShaderProperty>(),
					hasModifiedShaderProperties = false,
					hasErrors = false,
					header = null
				};


				Action addCurrentGroup = () =>
				{
					if (currentGroup.shaderProperties.Count > 0)
					{
						shaderPropertiesUIGroups.Add(currentGroup);

						if (!headersExpanded.ContainsKey(currentGroup.header.text))
						{
							headersExpanded.Add(currentGroup.header.text, false);
						}
					}
				};

				for (int i = 0; i < visibleShaderProperties.Count; i++)
				{
					if (shaderPropertiesHeaders.ContainsKey(i))
					{
						addCurrentGroup();

						currentGroup = new ShaderPropertyGroup()
						{
							shaderProperties = new List<ShaderProperty>(),
							hasModifiedShaderProperties = false,
							hasErrors = false,
							header = shaderPropertiesHeaders[i]
						};
					}

					currentGroup.shaderProperties.Add(visibleShaderProperties[i]);
					currentGroup.hasModifiedShaderProperties |= visibleShaderProperties[i].manuallyModified;
					currentGroup.hasErrors |= visibleShaderProperties[i].error;
				}
				addCurrentGroup();
			}

			public void UpdateCustomMaterialProperties()
			{
				foreach(var cmp in customMaterialPropertiesList)
				{
					cmp.implementation.CheckErrors();
				}
			}

			private void onShaderPropertyImplementationsChanged()
			{
				ShaderGenerator2.NeedsShaderPropertiesUpdate = true;
				ShaderGenerator2.PushUndoState();
			}

			//Process #KEYWORDS line from Template
			//Use temp features & flags to avoid permanent toggles (e.g. NOTILE_SAMPLING)
			//As long as the original features are there, they should be triggered each time anyway
			/// <returns>'true' if a new feature/flag has been added/removed, so that we can reprocess the whole keywords block</returns>
			internal bool ProcessKeywords(string line, List<string> tempFeatures, List<string> tempFlags, Dictionary<string, List<string>> tempExtraFlags)
			{
				if (string.IsNullOrEmpty(line))
				{
					return false;
				}

				//Inside valid block
				var parts = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

				// Dynamic
				if (parts[0].StartsWith("flag_on:"))
				{
					if (tempExtraFlags == null)
					{
						return false;
					}

					string block = parts[0].Substring("flag_on:".Length);
					if (!tempExtraFlags.ContainsKey(block)) tempExtraFlags.Add(block, new List<string>());

					if (Utils.AddIfMissing(tempExtraFlags[block], parts[1]))
					{
						return true;
					}
				}
				else if (parts[0].StartsWith("flag_off:"))
				{
					if (tempExtraFlags == null)
					{
						return false;
					}

					string block = parts[0].Substring("flag_on:".Length);
					if (!tempExtraFlags.ContainsKey(block))
					{
						return false;
					}

					if (Utils.RemoveIfExists(tempExtraFlags[block], parts[1]))
					{
						if (tempExtraFlags[block].Count == 0)
						{
							tempExtraFlags.Remove(block);
						}

						return true;
					}
				}
				else
				{
					// Fixed
					switch (parts[0])
					{
						case "set": //legacy
						case "set_keyword":
						{
							var keywordValue = parts.Length > 2 ? parts[2] : "";
							if (Keywords.ContainsKey(parts[1]))
								Keywords[parts[1]] = keywordValue;
							else
								Keywords.Add(parts[1], keywordValue);
							break;
						}

						case "enable_kw": //legacy
						case "feature_on":
						{
							if (Utils.AddIfMissing(tempFeatures, parts[1]))
							{
								return true;
							}

							break;
						}
						case "disable_kw": //legacy
						case "feature_off":
						{
							if (Utils.RemoveIfExists(tempFeatures, parts[1]))
							{
								return true;
							}

							break;
						}

						case "enable_flag": //legacy
						case "flag_on":
							if (tempFlags != null)
							{
								if (Utils.AddIfMissing(tempFlags, parts[1]))
								{
									return true;
								}
							}
							break;
						case "disable_flag": //legacy
						case "flag_off":
							if (tempFlags != null)
							{
								if (Utils.RemoveIfExists(tempFlags, parts[1]))
								{
									return true;
								}
							}
							break;
					}
				}

				return false;
			}

			// Cache the expanded state of the visible shader properties, to restore them after shader generation/update
			static HashSet<string> expandedCache;
			static Dictionary<string, bool> headersExpandedCache;
			void UI_CacheExpandedState()
			{
				headersExpandedCache = new Dictionary<string, bool>();
				foreach (var kvp in headersExpanded)
				{
					headersExpandedCache.Add(kvp.Key, kvp.Value);
				}

				expandedCache = new HashSet<string>();
				foreach (var shaderProperty in visibleShaderProperties)
				{
					if (shaderProperty.expanded)
					{
						expandedCache.Add(shaderProperty.Name);
					}
				}
			}

			void UI_RestoreExpandedState()
			{
				if (expandedCache == null && headersExpandedCache == null)
				{
					return;
				}

				foreach (var kvp in headersExpandedCache)
				{
					if (headersExpanded.ContainsKey(kvp.Key))
					{
						headersExpanded[kvp.Key] = kvp.Value;
					}
					else
					{
						headersExpanded.Add(kvp.Key, kvp.Value);
					}
				}

				foreach (var shaderProperty in visibleShaderProperties)
				{
					if (expandedCache.Contains(shaderProperty.Name))
					{
						shaderProperty.expanded = true;
					}
				}

				expandedCache = null;
				headersExpandedCache = null;
			}

			// Useful callbacks
			public void OnBeforeGenerateShader()
			{
				UI_CacheExpandedState();
			}

			public void OnAfterGenerateShader()
			{
				UI_RestoreExpandedState();
			}
		}
	}
}
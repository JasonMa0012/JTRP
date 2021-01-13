// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Represents a Toony Colors Pro 2 configuration to generate the corresponding shader

namespace ToonyColorsPro
{
	namespace Legacy
	{
		public class TCP2_Config
		{
			//--------------------------------------------------------------------------------------------------

			public string Filename = "TCP2 Custom";
			public string ShaderName = "Toony Colors Pro 2/User/My TCP2 Shader";
			public string configType = "Normal";
			public string templateFile = "TCP2_ShaderTemplate_Default";
			public int shaderTarget = 30;
			public List<string> Features = new List<string>();
			public List<string> Flags = new List<string>();
			public Dictionary<string, string> Keywords = new Dictionary<string, string>();
			public bool isModifiedExternally;

			//--------------------------------------------------------------------------------------------------

			private enum ParseBlock
			{
				None,
				Features,
				Flags
			}

			public static TCP2_Config CreateFromFile(TextAsset asset)
			{
				return CreateFromFile(asset.text);
			}
			public static TCP2_Config CreateFromFile(string text)
			{
				var lines = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				var config = new TCP2_Config();

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

							default: Debug.LogWarning("[TCP2 Shader Config] Unrecognized tag: " + data[0] + "\nline " + (i+1)); break;
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
									Debug.LogWarning("[TCP2 Shader Config] Unrecognized line while parsing : " + line + "\nline " + (i+1));
							}
						}
					}
				}

				return config;
			}

			public static TCP2_Config CreateFromShader(Shader shader)
			{
				var shaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;

				var config = new TCP2_Config();
				config.ShaderName = shader.name;
				config.Filename = Path.GetFileName(AssetDatabase.GetAssetPath(shader)).Replace(".shader", "");
				config.isModifiedExternally = false;
				var valid = config.ParseUserData(shaderImporter);

				if (valid)
					return config;
				return null;
			}

			public TCP2_Config Copy()
			{
				var config = new TCP2_Config();

				config.Filename = Filename;
				config.ShaderName = ShaderName;

				foreach (var feature in Features)
					config.Features.Add(feature);

				foreach (var flag in Flags)
					config.Flags.Add(flag);

				foreach (var kvp in Keywords)
					config.Keywords.Add(kvp.Key, kvp.Value);

				config.shaderTarget = shaderTarget;
				config.configType = configType;
				config.templateFile = templateFile;

				return config;
			}

			public string GetShaderTargetCustomData()
			{
				return string.Format("SM:{0}", shaderTarget);
			}

			public string GetConfigTypeCustomData()
			{
				if (configType != "Normal")
				{
					return string.Format("CT:{0}", configType);
				}

				return null;
			}

			public string GetConfigFileCustomData()
			{
				return string.Format("CF:{0}", templateFile);
			}

			public int ToHash()
			{
				var sb = new StringBuilder();
				sb.Append(Filename);
				sb.Append(ShaderName);
				var orderedFeatures = new List<string>(Features);
				orderedFeatures.Sort();
				var orderedFlags = new List<string>(Flags);
				orderedFlags.Sort();
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

				sb.Append(shaderTarget.ToString());

				return sb.ToString().GetHashCode();
			}

			//Convert Config to ShaderImporter UserData
			public string ToUserData(string[] customData)
			{
				var userData = "";
				if (!Features.Contains("USER"))
					userData = "USER,";

				foreach (var feature in Features)
					if (feature.Contains("USER"))
						userData += string.Format("{0},", feature);
					else
						userData += string.Format("F{0},", feature);
				foreach (var flag in Flags)
					userData += string.Format("f{0},", flag);
				foreach (var kvp in Keywords)
					userData += string.Format("K{0}:{1},", kvp.Key, kvp.Value);
				foreach (var custom in customData)
					userData += string.Format("c{0},", custom);
				userData = userData.TrimEnd(',');

				return userData;
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
					//Hash
					if (customData.Length > 0 && customData[0] == 'h')
					{
						var dataHash = customData;
						var fileHash = TCP2_ShaderGeneratorUtils.GetShaderContentHash(importer);

						if (!string.IsNullOrEmpty(fileHash) && dataHash != fileHash)
						{
							isModifiedExternally = true;
						}
					}
					//Timestamp
					else
					{
						ulong timestamp;
						if (ulong.TryParse(customData, out timestamp))
						{
							if (importer.assetTimeStamp != timestamp)
							{
								isModifiedExternally = true;
							}
						}
					}

					//Shader Model target
					if (customData.StartsWith("SM:"))
					{
						shaderTarget = int.Parse(customData.Substring(3));
					}

					//Configuration Type
					if (customData.StartsWith("CT:"))
					{
						configType = customData.Substring(3);
					}

					//Configuration File
					if (customData.StartsWith("CF:"))
					{
						templateFile = customData.Substring(3);
					}
				}

				return true;
			}

			public void AutoNames()
			{
				var rawName = ShaderName.Replace("Toony Colors Pro 2/", "");
				Filename = rawName;
			}

			//--------------------------------------------------------------------------------------------------
			// FEATURES

			public bool HasFeature(string feature)
			{
				return TCP2_ShaderGeneratorUtils.HasEntry(Features, feature);
			}

			public bool HasFeaturesAny(params string[] features)
			{
				return TCP2_ShaderGeneratorUtils.HasAnyEntries(Features, features);
			}

			public bool HasFeaturesAll(params string[] features)
			{
				return TCP2_ShaderGeneratorUtils.HasAllEntries(Features, features);
			}

			public void ToggleFeature(string feature, bool enable)
			{
				if (string.IsNullOrEmpty(feature))
					return;

				TCP2_ShaderGeneratorUtils.ToggleEntry(Features, feature, enable);
			}

			//--------------------------------------------------------------------------------------------------
			// FLAGS

			public bool HasFlag(string flag)
			{
				return TCP2_ShaderGeneratorUtils.HasEntry(Flags, flag);
			}

			public bool HasFlagsAny(params string[] flags)
			{
				return TCP2_ShaderGeneratorUtils.HasAnyEntries(Flags, flags);
			}

			public bool HasFlagsAll(params string[] flags)
			{
				return TCP2_ShaderGeneratorUtils.HasAllEntries(Flags, flags);
			}

			public void ToggleFlag(string flag, bool enable)
			{
				TCP2_ShaderGeneratorUtils.ToggleEntry(Flags, flag, enable);
			}

			//--------------------------------------------------------------------------------------------------
			// KEYWORDS

			public bool HasKeyword(string key)
			{
				return GetKeyword(key) != null;
			}

			public string GetKeyword(string key)
			{
				if (key == null)
					return null;

				if (!Keywords.ContainsKey(key))
					return null;

				return Keywords[key];
			}

			public void SetKeyword(string key, string value)
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

			public void RemoveKeyword(string key)
			{
				if (Keywords.ContainsKey(key))
					Keywords.Remove(key);
			}
		}
	}
}
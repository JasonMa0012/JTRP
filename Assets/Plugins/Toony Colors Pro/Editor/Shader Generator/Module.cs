// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Represents a Shader Generator 2 module: external file that has specific code for a feature, that can be reused among templates

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		public class Module
		{
			public class Argument
			{
				public string name;

				//Variable type is parsed but we actually don't care about it in the code, it's just an indication in the Module file for proper integration into the Template
				public string variable;

				public override string ToString()
				{
					return string.Format("{0} : {1}", name, variable);
				}
			}

			public string name;
			public string[] Features = new string[0];
			public string[] PropertiesNew = new string[0];
			public string[] Keywords = new string[0];
			public string[] ShaderFeaturesBlock = new string[0];
			public string[] PropertiesBlock = new string[0];
			public string[] Functions = new string[0];
			public string[] Variables = new string[0];
			public string[] InputStruct = new string[0];
			Dictionary<string, string[]> Vertices = new Dictionary<string, string[]>();
			Dictionary<string, string[]> Fragments = new Dictionary<string, string[]>();

			Dictionary<string, Argument[]> VerticesArgs = new Dictionary<string, Argument[]>();
			Dictionary<string, Argument[]> FragmentsArgs = new Dictionary<string, Argument[]>();

			static public Module CreateFromName(string moduleName)
			{
				string moduleFile = string.Format("Module_{0}.txt", moduleName);
				string rootPath = Utils.FindReadmePath(true);
				string modulePath = string.Format("{0}/Shader Templates 2/Modules/{1}", rootPath, moduleFile);

				var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(modulePath);

				//Can't find through default path, try to search for the file using AssetDatabase
				if(textAsset == null)
				{
					var matches = AssetDatabase.FindAssets(string.Format("Module_{0} t:textasset", moduleName));
					if (matches.Length > 0)
					{
						// Get the first result
						textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(matches[0]));
					}
					else
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find module using Unity's search system. Make sure that the file 'Module_{0}' is in the project!", moduleName)));
					}
				}

				if(textAsset == null)
				{
					Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't load module: '{0}'", moduleName)));
					return null;
				}

				var lines = textAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

				List<string> features = new List<string>();
				List<string> propertiesNew = new List<string>();
				List<string> keywords = new List<string>();
				List<string> shaderFeaturesBlock = new List<string>();
				List<string> propertiesBlock = new List<string>();
				List<string> variables = new List<string>();
				List<string> functions = new List<string>();
				List<string> inputStruct = new List<string>();

				Dictionary<string, List<Argument>> verticesArgs = new Dictionary<string, List<Argument>>();
				Dictionary<string, List<Argument>> fragmentsArgs = new Dictionary<string, List<Argument>>();
				Dictionary<string, List<string>> vertices = new Dictionary<string, List<string>>();
				Dictionary<string, List<string>> fragments = new Dictionary<string, List<string>>();

				List<string> currentList = null;

				foreach (var line in lines)
				{
					if(line.StartsWith("#") && !line.Contains("_IMPL"))
					{
						var lineTrim = line.Trim();

						//fragment can have arguments, so check the start of the line instead of exact word
						if(lineTrim.StartsWith("#VERTEX"))
						{
							var key = "";
							if(lineTrim.Contains(":"))
							{
								int start = "#VERTEX:".Length;
								int end = lineTrim.IndexOf('(');
								key = lineTrim.Substring(start, end - start);
							}

							currentList = new List<string>();
							vertices.Add(key, currentList);

							if (lineTrim.Contains("(") && lineTrim.Contains(")"))
							{
								//parse arguments
								var vertexArgs = ParseArguments(lineTrim);
								verticesArgs.Add(key, vertexArgs);
							}
						}
						//#LIGHTING is an alias for fragment here, just to differentiate in the template code
						else if(lineTrim.StartsWith("#FRAGMENT") || lineTrim.StartsWith("#LIGHTING"))
						{
							var key = "";
							if (lineTrim.Contains(":"))
							{
								int start = "#FRAGMENT:".Length;
								int end = lineTrim.IndexOf('(');
								if(end >= 0)
									key = lineTrim.Substring(start, end - start);
								else
									key = lineTrim.Substring(start);
							}

							currentList = new List<string>();
							fragments.Add(key, currentList);

							if(lineTrim.Contains("(") && lineTrim.Contains(")"))
							{
								//parse arguments
								var fragmentArgs = ParseArguments(lineTrim);
								fragmentsArgs.Add(key, fragmentArgs);
							}
						}
						else
						{
							switch(lineTrim)
							{
								case "#FEATURES": currentList = features; break;
								case "#PROPERTIES_NEW": currentList = propertiesNew; break;
								case "#KEYWORDS": currentList = keywords; break;
								case "#PROPERTIES_BLOCK": currentList = propertiesBlock; break;
								case "#SHADER_FEATURES_BLOCK": currentList = shaderFeaturesBlock; break;
								case "#FUNCTIONS": currentList = functions; break;
								case "#VARIABLES": currentList = variables; break;
								case "#INPUT": currentList = inputStruct; break;
								case "#END": currentList = null; break;
							}
						}
					}
					else
					{
						if(currentList != null)
						{
							currentList.Add(line);
						}
					}
				}

				Module module = new Module();
				module.name = moduleName;
				module.Features = features.ToArray();
				module.PropertiesNew = propertiesNew.ToArray();
				module.Keywords = keywords.ToArray();
				module.ShaderFeaturesBlock = shaderFeaturesBlock.ToArray();
				module.PropertiesBlock = propertiesBlock.ToArray();
				module.Functions = functions.ToArray();
				module.Variables = variables.ToArray();
				module.InputStruct = inputStruct.ToArray();

				// #VERTEX
				if (vertices.Count == 0)
				{
					vertices.Add("", new List<string>());
					verticesArgs.Add("", new List<Argument>());
				}

				foreach (var vertexPair in vertices)
				{
					var key = vertexPair.Key;
					module.Vertices.Add(key, vertexPair.Value.ToArray());
					if (verticesArgs.ContainsKey(key))
					{
						module.VerticesArgs.Add(key, verticesArgs[key].ToArray());
					}
				}

				// #FRAGMENT
				if (fragments.Count == 0)
				{
					fragments.Add("", new List<string>());
					fragmentsArgs.Add("", new List<Argument>());
				}
				
				foreach (var fragmentPair in fragments)
				{
					var key = fragmentPair.Key;
					module.Fragments.Add(key, fragmentPair.Value.ToArray());
					if (fragmentsArgs.ContainsKey(key))
					{
						module.FragmentsArgs.Add(key, fragmentsArgs[key].ToArray());
					}
				}

				module.ProcessIndentation();

				return module;
			}

			static List<Argument> ParseArguments(string line)
			{
				var list = new List<Argument>();

				//parse arguments
				int start = line.IndexOf("(")+1;
				int end = line.IndexOf(")");
				var content = line.Substring(start, end-start);
				var args = content.Split(',');
				for(int i = 0; i < args.Length; i++)
				{
					var arg = args[i].Trim();
					int spaceIndex = arg.IndexOf(arg.Substring(arg.IndexOf(' ')));
					var type = arg.Substring(0, spaceIndex);
					var name = arg.Substring(spaceIndex+1);
					var argument = new Argument()
					{
						variable = type,
						name = name
					};
					list.Add(argument);
				}
				return list;
			}

			//Find minimum indentation and remove for every line for each block
			void ProcessIndentation()
			{
				RemoveMinimumIndentation(this.Features);
				RemoveMinimumIndentation(this.PropertiesNew);
				RemoveMinimumIndentation(this.Keywords);
				RemoveMinimumIndentation(this.ShaderFeaturesBlock);
				RemoveMinimumIndentation(this.PropertiesBlock);
				RemoveMinimumIndentation(this.Functions);
				RemoveMinimumIndentation(this.Variables);
				RemoveMinimumIndentation(this.InputStruct);
				RemoveMinimumIndentation(this.Vertices);
				RemoveMinimumIndentation(this.Fragments);
			}

			void RemoveMinimumIndentation(Dictionary<string, string[]> dict)
			{
				foreach (var key in dict.Keys)
				{
					RemoveMinimumIndentation(dict[key]);
				}
			}

			void RemoveMinimumIndentation(string[] block)
			{
				if(block == null)
					return;

				//Find minimum number of leading tabs across all lines
				int minIndent = 999;
				for (int i = 0; i < block.Length; i++)
				{
					string trimmedBlock = block[i].Trim();
					if (trimmedBlock.StartsWith("///") || block[i].StartsWith("#") || string.IsNullOrEmpty(trimmedBlock))
					{
						continue;
					}

					// special cases to ignore, as they won't be part of the shader code
					if (trimmedBlock[0] == '#' && trimmedBlock.Contains("not_empty"))
					{
						continue;
					}

					int j = 0;
					while(j < block[i].Length && block[i][j] == '\t')
					{
						j++;
					}
					minIndent = Mathf.Min(minIndent, j);
				}

				//Remove that minimum value for all lines (excluding /// and ENABLE_IMPL and DISABLE_IMPL)
				for(int i = 0; i < block.Length; i++)
				{
					string trim = block[i].Trim();
					if (trim.StartsWith("///") || (trim.StartsWith("#") && trim.Contains("_IMPL")))
						continue;

					if (trim.StartsWith("#") && trim.Contains("not_empty"))
						continue;

					if (block[i].Length > minIndent)
						block[i] = block[i].Substring(minIndent);
				}
			}

			//Return the Vertex Lines with the arguments replaced with their proper names
			public string[] VertexLines(List<string> arguments, string key = "")
			{
				Argument[] args;
				VerticesArgs.TryGetValue(key, out args);
				return ArgumentLines(Vertices[key], args, arguments);
			}

			//Return the Fragment Lines with the arguments replaced with their proper names
			public string[] FragmentLines(List<string> arguments, string key = "")
			{
				Argument[] args;
				string[] lines;
				FragmentsArgs.TryGetValue(key, out args);
				Fragments.TryGetValue(key, out lines);

				if (lines == null)
				{
					Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("Can't find #FRAGMENT/#LIGHTING for Module '{0}{1}'", this.name, string.IsNullOrEmpty(key) ? "" : ":" + key)));
					return null;
				}

				return ArgumentLines(lines, args, arguments);
			}

			string[] ArgumentLines(string[] array, Argument[] arguments, List<string> suppliedArguments)
			{
				if(arguments == null || arguments.Length == 0)
					return array;
				else
				{
					if(suppliedArguments.Count != arguments.Length)
					{
						Debug.LogError(ShaderGenerator2.ErrorMsg(string.Format("[Module {4}] Invalid number of arguments provided: got <b>{0}</b>, expected <b>{1}</b>:\nExpected: {2}\nSupplied: {3}",
							suppliedArguments.Count,
							arguments.Length,
							string.Join(", ", System.Array.ConvertAll(arguments, a => a.ToString())),
							string.Join(", ", suppliedArguments.ToArray()),
							this.name)));
					}

					var list = new List<string>();
					foreach(var line in array)
					{
						string lineWithArgs = line;
						for(int i = 0; i < arguments.Length; i++)
						{
							lineWithArgs = System.Text.RegularExpressions.Regex.Replace(lineWithArgs, @"\b" + arguments[i].name + @"\b", suppliedArguments[i]);
						}
						list.Add(lineWithArgs);
					}

					return list.ToArray();
				}
			}
		}
	}
}
// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// General helper functions for TCP2

namespace ToonyColorsPro
{
	namespace Utilities
	{
		public static class Utils
		{
			//--------------------------------------------------------------------------------------------------------------------------------

			public enum TextureChannel
			{
				Alpha, Red, Green, Blue
			}

			public static string ToShader(this TextureChannel channel)
			{
				switch (channel)
				{
					case TextureChannel.Alpha: return ".a";
					case TextureChannel.Red: return ".r";
					case TextureChannel.Green: return ".g";
					case TextureChannel.Blue: return ".b";
					default: Debug.LogError("[Utils] Unrecognized texture channel: " + channel.ToShader()); return null;
				}
			}

			public static TextureChannel FromShader(string str)
			{
				if (string.IsNullOrEmpty(str))
					return TextureChannel.Alpha;

				switch (str)
				{
					case ".a": return TextureChannel.Alpha;
					case ".r": return TextureChannel.Red;
					case ".g": return TextureChannel.Green;
					case ".b": return TextureChannel.Blue;
					default: Debug.LogError("[Utils] Unrecognized texture channel from shader: " + str + "\nDefaulting to Alpha"); return TextureChannel.Alpha;
				}
			}

			//Fix for retina displays
#if UNITY_5_4_OR_NEWER
			public static float ScreenWidthRetina { get { return Screen.width/EditorGUIUtility.pixelsPerPoint; } }
#else
	static public float ScreenWidthRetina { get { return Screen.width; } }
#endif

			//--------------------------------------------------------------------------------------------------------------------------------
			// CUSTOM INSPECTOR UTILS

			public static bool HasKeywords(List<string> list, params string[] keywords)
			{
				var v = false;
				foreach (var kw in keywords)
					v |= list.Contains(kw);

				return v;
			}

			public static bool ShaderKeywordToggle(string keyword, string label, string tooltip, List<string> list, ref bool update, string helpTopic = null)
			{
				var w = EditorGUIUtility.labelWidth;
				if (!string.IsNullOrEmpty(helpTopic))
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = w - 16;
					TCP2_GUI.HelpButton(helpTopic);
				}

				var boolValue = list.Contains(keyword);
				EditorGUI.BeginChangeCheck();
				boolValue = EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), boolValue, boolValue ? EditorStyles.boldLabel : EditorStyles.label);
				if (EditorGUI.EndChangeCheck())
				{
					if (boolValue)
						list.Add(keyword);
					else
						list.Remove(keyword);

					update = true;
				}

				if (!string.IsNullOrEmpty(helpTopic))
				{
					EditorGUIUtility.labelWidth = w;
					EditorGUILayout.EndHorizontal();
				}

				return boolValue;
			}

			public static bool ShaderKeywordRadio(string header, string[] keywords, GUIContent[] labels, List<string> list, ref bool update)
			{
				var index = 0;
				for (var i = 1; i < keywords.Length; i++)
				{
					if (list.Contains(keywords[i]))
					{
						index = i;
						break;
					}
				}

				EditorGUI.BeginChangeCheck();

				//Header and rect calculations
				var hasHeader = (!string.IsNullOrEmpty(header));
				var headerRect = GUILayoutUtility.GetRect(120f, 16f, GUILayout.ExpandWidth(false));
				var r = headerRect;
				if (hasHeader)
				{
					var helpRect = headerRect;
					helpRect.width = 16;
					headerRect.width -= 16;
					headerRect.x += 16;
					var helpTopic = header.ToLowerInvariant();
					helpTopic = char.ToUpperInvariant(helpTopic[0]) + helpTopic.Substring(1);
					TCP2_GUI.HelpButton(helpRect, helpTopic);
					GUI.Label(headerRect, header, index > 0 ? EditorStyles.boldLabel : EditorStyles.label);
					r.width = ScreenWidthRetina - headerRect.width - 34f;
					r.x += headerRect.width;
				}
				else
				{
					r.width = ScreenWidthRetina - 34f;
				}

				for (var i = 0; i < keywords.Length; i++)
				{
					var rI = r;
					rI.width /= keywords.Length;
					rI.x += i * rI.width;
					if (GUI.Toggle(rI, index == i, labels[i], (i == 0) ? EditorStyles.miniButtonLeft : (i == keywords.Length-1) ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid))
					{
						index = i;
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					//Remove all other keywords and add selected
					for (var i = 0; i < keywords.Length; i++)
					{
						if (list.Contains(keywords[i]))
							list.Remove(keywords[i]);
					}

					if (index > 0)
					{
						list.Add(keywords[index]);
					}

					update = true;
				}

				return (index > 0);
			}

			public static int ShaderKeywordRadioGeneric(string header, int index, GUIContent[] labels)
			{
				//Header and rect calculations
				var hasHeader = (!string.IsNullOrEmpty(header));
				var controlRect = EditorGUILayout.GetControlRect();
				var headerRect = EditorGUI.IndentedRect(controlRect);
				var r = headerRect;
				if (hasHeader)
				{
					headerRect.width = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel*15f);
					GUI.Label(headerRect, header, EditorStyles.label);
					r.width -= headerRect.width;
					r.x += headerRect.width;
				}
				else
				{
					r.width = ScreenWidthRetina - 20f;
				}

				for (var i = 0; i < labels.Length; i++)
				{
					var rI = r;
					rI.width /= labels.Length;
					rI.x += i * rI.width;
					if (GUI.Toggle(rI, index == i, labels[i], (i == 0) ? EditorStyles.miniButtonLeft : (i == labels.Length - 1) ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid))
					{
						index = i;
					}
				}

				return index;
			}

			// Enable/Disable a feature on the shader and mark it for update
			public static void ShaderVariantUpdate(string feature, List<string> featuresList, List<bool> featuresEnabled, bool enable, ref bool update)
			{
				var featureIndex = featuresList.IndexOf(feature);
				if (featureIndex < 0)
				{
					EditorGUILayout.HelpBox("Couldn't find shader feature in list: " + feature, MessageType.Error);
					return;
				}

				if (featuresEnabled[featureIndex] != enable)
				{
					featuresEnabled[featureIndex] = enable;
					update = true;
				}
			}

			public static bool AddIfMissing(List<string> list, string item)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
					return true;
				}
				return false;
			}

			public static bool RemoveIfExists(List<string> list, string item)
			{
				if (list.Contains(item))
				{
					list.Remove(item);
					return true;
				}
				return false;
			}


			//--------------------------------------------------------------------------------------------------------------------------------

			static string cachedReadmePath;
			public static string FindReadmePath(bool relativeToAssets = false)
			{
				string readmePathFull = null;

				// check cached path
				if (!string.IsNullOrEmpty(cachedReadmePath))
				{
					if (File.Exists(cachedReadmePath))
					{
						readmePathFull = cachedReadmePath;
					}
				}

				if (readmePathFull == null)
				{
					// try to find by GUID
					const string readmeGuid = "d6d278f2c506dde44ab56fd1555fb4d4";
					string guidPath = AssetDatabase.GUIDToAssetPath(readmeGuid);
					if (!string.IsNullOrEmpty(guidPath))
					{
						readmePathFull = Application.dataPath + guidPath.Substring("Assets".Length);
						readmePathFull = ToSystemSlashPath(Path.GetDirectoryName(readmePathFull));
					}
				}

				if (readmePathFull == null)
				{
					// GUID has been changed, try to find through the file system
					string readmePath = GetFileSafe(Application.dataPath, "!ToonyColorsPro Readme.txt");
					if (!string.IsNullOrEmpty(readmePath))
					{
						readmePath = ToSystemSlashPath(Path.GetDirectoryName(readmePath));
					}
				}

				if (readmePathFull == null)
				{
					Debug.LogError("Couldn't find the path to '!ToonyColorsPro Readme.txt', you might have to reimport Toony Colors Pro.\nThis file is necessary to figure out the root directory of Toony Colors Pro.");
					return null;
				}

				// Cache for future use
				cachedReadmePath = readmePathFull;

				if (relativeToAssets)
				{
#if UNITY_EDITOR_WIN
					return readmePathFull.Replace(ToSystemSlashPath(Application.dataPath), "Assets").Replace(@"\", "/");
#else
			return readmePathFull.Replace(ToSystemSlashPath(Application.dataPath), "Assets");
#endif
				}
				return readmePathFull;
			}

			/// <summary>
			/// Similar to Directory.GetFiles, but won't raise exceptions for wrong read permissions
			/// </summary>
			public static string GetFileSafe(string path, string pattern)
			{
				var list = new List<string>();
				GetFilesSafeInternal(list, path, pattern, true);
				if (list.Count > 0)
				{
					return list[0];
				}

				return null;
			}

			/// <summary>
			/// Similar to Directory.GetFiles, but won't raise exceptions for wrong read permissions
			/// </summary>
			public static string[] GetFilesSafe(string path, string pattern)
			{
				var list = new List<string>();
				GetFilesSafeInternal(list, path, pattern, false);
				return list.ToArray();
			}

			// Need to use this recursive version, because the regular GetFiles with subdirectories could raise exceptions depending on read permissions
			static void GetFilesSafeInternal(List<string> list, string path, string pattern, bool stopAtFirstMatch)
			{
				var results = Directory.GetFiles(path, pattern);
				if (results != null && results.Length > 0)
				{
					list.AddRange(results);
					if (stopAtFirstMatch)
					{
						return;
					}
				}

				var dirs = Directory.GetDirectories(path);
				foreach (var dir in dirs)
				{
					if (Directory.Exists(dir))
					{
						GetFilesSafeInternal(list, dir, pattern, stopAtFirstMatch);
					}
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------

			public enum SmoothedNormalsChannel
			{
				VertexColors,
				Tangents,
				UV1,
				UV2,
				UV3,
				UV4
			}

			public enum SmoothedNormalsUVType
			{
				FullXYZ,
				CompressedXY,
				CompressedZW
			}

			public static Mesh CreateSmoothedMesh(Mesh originalMesh, string format, SmoothedNormalsChannel smoothedNormalsChannel, SmoothedNormalsUVType uvType, bool overwriteMesh)
			{
				if (originalMesh == null)
				{
					Debug.LogWarning("[TCP2 : Smoothed Mesh] Supplied OriginalMesh is null!\nCan't create smooth normals version.");
					return null;
				}

				//Create new mesh
				var newMesh = overwriteMesh ? originalMesh : new Mesh();
				if (!overwriteMesh)
				{
					//			EditorUtility.CopySerialized(originalMesh, newMesh);
					newMesh.vertices = originalMesh.vertices;
					newMesh.normals = originalMesh.normals;
					newMesh.tangents = originalMesh.tangents;
					newMesh.uv = originalMesh.uv;
					newMesh.uv2 = originalMesh.uv2;
					newMesh.uv3 = originalMesh.uv3;
					newMesh.uv4 = originalMesh.uv4;
					newMesh.colors32 = originalMesh.colors32;
					newMesh.triangles = originalMesh.triangles;
					newMesh.bindposes = originalMesh.bindposes;
					newMesh.boneWeights = originalMesh.boneWeights;

					//Only available from Unity 5.3 onward
					if (originalMesh.blendShapeCount > 0)
						CopyBlendShapes(originalMesh, newMesh);

					newMesh.subMeshCount = originalMesh.subMeshCount;
					if (newMesh.subMeshCount > 1)
						for (var i = 0; i < newMesh.subMeshCount; i++)
							newMesh.SetTriangles(originalMesh.GetTriangles(i), i);
				}

				//--------------------------------
				// Format

				var chSign = Vector3.one;
				if (string.IsNullOrEmpty(format)) format = "xyz";
				format = format.ToLowerInvariant();
				var channels = new[] { 0, 1, 2 };
				var skipFormat = (format == "xyz");
				var charIndex = 0;
				var ch = 0;
				while (charIndex < format.Length)
				{
					switch (format[charIndex])
					{
						case '-': chSign[ch] = -1; break;
						case 'x': channels[ch] = 0; ch++; break;
						case 'y': channels[ch] = 1; ch++; break;
						case 'z': channels[ch] = 2; ch++; break;
						default: break;
					}
					if (ch > 2) break;
					charIndex++;
				}

				//--------------------------------
				//Calculate smoothed normals

				//Iterate, find same-position vertices and calculate averaged values as we go
				var averageNormalsHash = new Dictionary<Vector3, Vector3>();
				for (var i = 0; i < newMesh.vertexCount; i++)
				{
					if (!averageNormalsHash.ContainsKey(newMesh.vertices[i]))
						averageNormalsHash.Add(newMesh.vertices[i], newMesh.normals[i]);
					else
						averageNormalsHash[newMesh.vertices[i]] = (averageNormalsHash[newMesh.vertices[i]] + newMesh.normals[i]).normalized;
				}

				//Convert to Array
				var averageNormals = new Vector3[newMesh.vertexCount];
				for (var i = 0; i < newMesh.vertexCount; i++)
				{
					averageNormals[i] = averageNormalsHash[newMesh.vertices[i]];
					if (!skipFormat)
						averageNormals[i] = Vector3.Scale(new Vector3(averageNormals[i][channels[0]], averageNormals[i][channels[1]], averageNormals[i][channels[2]]), chSign);
				}

#if DONT_ALTER_NORMALS
		//Debug: don't alter normals to see if converting into colors/tangents/uv2 works correctly
		for(int i = 0; i < newMesh.vertexCount; i++)
		{
			averageNormals[i] = newMesh.normals[i];
		}
#endif

				//--------------------------------
				// Store in Vertex Colors

				if (smoothedNormalsChannel == SmoothedNormalsChannel.VertexColors)
				{
					//Assign averaged normals to colors
					var colors = new Color32[newMesh.vertexCount];
					for (var i = 0; i < newMesh.vertexCount; i++)
					{
						var r = (byte)(((averageNormals[i].x * 0.5f) + 0.5f)*255);
						var g = (byte)(((averageNormals[i].y * 0.5f) + 0.5f)*255);
						var b = (byte)(((averageNormals[i].z * 0.5f) + 0.5f)*255);

						colors[i] = new Color32(r, g, b, 255);
					}
					newMesh.colors32 = colors;
				}

				//--------------------------------
				// Store in Tangents

				if (smoothedNormalsChannel == SmoothedNormalsChannel.Tangents)
				{
					//Assign averaged normals to tangent
					var tangents = new Vector4[newMesh.vertexCount];
					for (var i = 0; i < newMesh.vertexCount; i++)
					{
						tangents[i] = new Vector4(averageNormals[i].x, averageNormals[i].y, averageNormals[i].z, 0f);
					}
					newMesh.tangents = tangents;
				}

				//--------------------------------
				// Store in UVs

				if (smoothedNormalsChannel == SmoothedNormalsChannel.UV1 || smoothedNormalsChannel == SmoothedNormalsChannel.UV2 || smoothedNormalsChannel == SmoothedNormalsChannel.UV3 || smoothedNormalsChannel == SmoothedNormalsChannel.UV4)
				{
					int uvIndex = -1;

					switch (smoothedNormalsChannel)
					{
						case SmoothedNormalsChannel.UV1: uvIndex = 0; break;
						case SmoothedNormalsChannel.UV2: uvIndex = 1; break;
						case SmoothedNormalsChannel.UV3: uvIndex = 2; break;
						case SmoothedNormalsChannel.UV4: uvIndex = 3; break;
						default: Debug.LogError("Invalid smoothed normals UV channel: " + smoothedNormalsChannel); break;
					}

					if (uvType == SmoothedNormalsUVType.FullXYZ)
					{
						//Assign averaged normals directly to UV fully (xyz)
						newMesh.SetUVs(uvIndex, new List<Vector3>(averageNormals));
					}
					else
					{
						if (uvType == SmoothedNormalsUVType.CompressedXY)
						{
							//Assign averaged normals to UV compressed (x,y to uv.x and z to uv.y)
							var uvs = new List<Vector2>(newMesh.vertexCount);
							for (var i = 0; i < newMesh.vertexCount; i++)
							{
								float x, y;
								GetCompressedSmoothedNormals(averageNormals[i], out x, out y);
								var v2 = new Vector2(x, y);
								uvs.Add(v2);
							}
							newMesh.SetUVs(uvIndex, uvs);
						}
						else if (uvType == SmoothedNormalsUVType.CompressedZW)
						{
							//Assign averaged normals to UV compressed (x,y to uv.z and z to uv.w)
							List<Vector4> existingUvs = new List<Vector4>();
							newMesh.GetUVs(uvIndex, existingUvs);
							if (existingUvs.Count == 0)
							{
								existingUvs.AddRange(new Vector4[newMesh.vertexCount]);
							}
							var uvs = new List<Vector4>(newMesh.vertexCount);
							for (var i = 0; i < newMesh.vertexCount; i++)
							{
								float x, y;
								GetCompressedSmoothedNormals(averageNormals[i], out x, out y);
								var v4 = existingUvs[i];
								v4.z = x;
								v4.w = y;
								uvs.Add(v4);
							}
							newMesh.SetUVs(uvIndex, uvs);
						}
					}
				}

				return newMesh;
			}

			static void GetCompressedSmoothedNormals(Vector3 smoothedNormal, out float x, out float y)
			{
				var _x = smoothedNormal.x * 0.5f + 0.5f;
				var _y = smoothedNormal.y * 0.5f + 0.5f;
				var _z = smoothedNormal.z * 0.5f + 0.5f;

				//pack x,y to uv2.x
				_x = Mathf.Round(_x*15);
				_y = Mathf.Round(_y*15);
				var packed = Vector2.Dot(new Vector2(_x, _y), new Vector2((float)(1.0/(255.0/16.0)), (float)(1.0/255.0)));

				x = packed;
				y = _z;
			}

			//Only available from Unity 5.3 onward
			private static void CopyBlendShapes(Mesh originalMesh, Mesh newMesh)
			{
				for (var i = 0; i < originalMesh.blendShapeCount; i++)
				{
					var shapeName = originalMesh.GetBlendShapeName(i);
					var frameCount = originalMesh.GetBlendShapeFrameCount(i);
					for (var j = 0; j < frameCount; j++)
					{
						var dv = new Vector3[originalMesh.vertexCount];
						var dn = new Vector3[originalMesh.vertexCount];
						var dt = new Vector3[originalMesh.vertexCount];

						var frameWeight = originalMesh.GetBlendShapeFrameWeight(i, j);
						originalMesh.GetBlendShapeFrameVertices(i, j, dv, dn, dt);
						newMesh.AddBlendShapeFrame(shapeName, frameWeight, dv, dn, dt);
					}
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			// SHADER PACKING/UNPACKING

			public class PackedFile
			{
				public PackedFile(string _path, string _content)
				{
					mPath = _path;
					content = _content;
				}

				private string mPath;
				public string path
				{
					get
					{
#if UNITY_EDITOR_WIN
						return mPath;
#else
				return this.mPath.Replace(@"\","/");
#endif
					}
				}
				public string content { get; private set; }
			}

			//Get a PackedFile from a system file path
			public static PackedFile PackFile(string windowsPath)
			{
				if (!File.Exists(windowsPath))
				{
					EditorApplication.Beep();
					Debug.LogError("[TCP2 PackFile] File doesn't exist:" + windowsPath);
					return null;
				}

				//Get properties
				// Content
				var content = File.ReadAllText(windowsPath, Encoding.UTF8);
				content = content.Replace("\r\n", "\n");
				// File relative path
				var tcpRoot = FindReadmePath();
				if (tcpRoot == null)
				{
					EditorApplication.Beep();
					Debug.LogError("[TCP2 PackFile] Can't find TCP2 Readme file!\nCan't determine root folder to pack/unpack files.");
					return null;
				}
				tcpRoot = ToSystemSlashPath(tcpRoot);
				var relativePath = windowsPath.Replace(tcpRoot, "");

				var pf = new PackedFile(relativePath, content);
				return pf;
			}

			//Create an archive of PackedFile
			public static void CreateArchive(PackedFile[] packedFiles, string outputFile)
			{
				if (packedFiles == null || packedFiles.Length == 0)
				{
					EditorApplication.Beep();
					Debug.LogError("[TCP2 PackFile] No file to pack!");
					return;
				}

				var sbIndex = new StringBuilder();
				var sbContent = new StringBuilder();

				sbIndex.AppendLine("# TCP2 PACKED SHADERS");
				var lineCursor = 0;
				foreach (var pf in packedFiles)
				{
					sbContent.Append(pf.content);
					sbIndex.AppendLine(pf.path + ";" + lineCursor + ";" + pf.content.Length);   // PATH ; START ; LENGTH
					lineCursor += pf.content.Length;
				}

				var archiveContent = sbIndex.ToString().Replace("\r\n", "\n") + "###\n" + sbContent.ToString().Replace("\r\n", "\n");

				var fullPath = Application.dataPath + "/" + outputFile;
				var directory = Path.GetDirectoryName(fullPath);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				File.WriteAllText(fullPath, archiveContent);
				AssetDatabase.Refresh();
				Debug.Log("[TCP2 CreateArchive] Created archive:\n" + fullPath);
			}

			//Extract an archive into an array of PackedFile
			public static PackedFile[] ExtractArchive(string archivePath, string filter = null)
			{
				var archive = File.ReadAllText(archivePath);
				archive = archive.Replace("\r\n", "\n");
				var archiveLines = File.ReadAllLines(archivePath);

				if (archiveLines[0] != "# TCP2 PACKED SHADERS")
				{
					EditorApplication.Beep();
					Debug.LogError("[TCP2 ExtractArchive] Invalid TCP2 archive:\n" + archivePath);
					return null;
				}

				//Find offset
				var offset = archive.IndexOf("###") + 4;
				if (offset < 20)
				{
					Debug.LogError("[TCP2 ExtractArchive] Invalid TCP2 archive:\n" + archivePath);
					return null;
				}

				var tcpRoot = FindReadmePath();
				var packedFilesList = new List<PackedFile>();
				for (var line = 1; line < archiveLines.Length; line++)
				{
					//Index end, start content parsing
					if (archiveLines[line].StartsWith("#"))
					{
						break;
					}

					var shaderIndex = archiveLines[line].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
					if (shaderIndex.Length != 3)
					{
						EditorApplication.Beep();
						Debug.LogError("[TCP2 ExtractArchive] Invalid format in TCP2 archive, at line " + line + ":\n" + archivePath);
						return null;
					}

					//Get data
					var relativePath = shaderIndex[0];
					var start = int.Parse(shaderIndex[1]);
					var length = int.Parse(shaderIndex[2]);
					//Get content
					var content = archive.Substring(offset + start, length);

					//Skip if file already extracted
					if (File.Exists(tcpRoot + relativePath))
					{
						continue;
					}

					//Filter?
					if (!string.IsNullOrEmpty(filter))
					{
						var filters = filter.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
						var skip = false;
						foreach (var f in filters)
						{
							if (!relativePath.ToLower().Contains(f.ToLower()))
							{
								skip = true;
								break;
							}
						}
						if (skip)
							continue;
					}

					//Add File
					packedFilesList.Add(new PackedFile(relativePath, content));
				}

				return packedFilesList.ToArray();
			}

			public static string UnityRelativeToSystemPath(string path)
			{
				var sysPath = path;
#if UNITY_EDITOR_WIN
				sysPath = path.Replace("/", @"\");
#endif
				var appPath = ToSystemSlashPath(Application.dataPath);
				appPath = appPath.Substring(0, appPath.Length - 6); // Remove 'Assets'
				sysPath = appPath + sysPath;
				return sysPath;
			}
			public static string ToSystemSlashPath(string path)
			{
#if UNITY_EDITOR_WIN
				return path.Replace("/", @"\");
#else
		return path;
#endif
			}

			public static bool SystemToUnityPath(ref string sysPath)
			{
				if (sysPath.IndexOf(Application.dataPath) < 0)
				{
					return false;
				}

				sysPath = string.Format("Assets{0}", sysPath.Replace(Application.dataPath, ""));
				return true;
			}

			public static string OpenFolderPanel_ProjectPath(string label, string startDir)
			{
				string output = null;

				if (startDir.Length > 0 && startDir[0] != '/')
				{
					startDir = "/" + startDir;
				}

				string startPath = Application.dataPath.Replace(@"\", "/") + startDir;
				if (!Directory.Exists(startPath))
				{
					startPath = Application.dataPath;
				}

				var path = EditorUtility.OpenFolderPanel(label, startPath, "");
				if (!string.IsNullOrEmpty(path))
				{
					var validPath = SystemToUnityPath(ref path);
					if (validPath)
					{
						if (path == "Assets")
							output = "/";
						else
							output = path.Substring("Assets/".Length);
					}
					else
					{
						EditorApplication.Beep();
						EditorUtility.DisplayDialog("Invalid Path", "The selected path is invalid.\n\nPlease select a folder inside the \"Assets\" folder of your project!", "Ok");
					}
				}

				return output;
			}
		}
	}
}
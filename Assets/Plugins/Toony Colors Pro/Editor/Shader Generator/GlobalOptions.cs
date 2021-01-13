// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Represents the global options for the Shader Generator, using the EditorPrefs API

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		// Global Options shared across all Unity projects
		public static class GlobalOptions
		{
			[System.Serializable]
			public class Data
			{
				public bool ShowOptions = true;
				public bool ShowDisabledFeatures = true;
				public bool SelectGeneratedShader = true;
				public bool ShowContextualHelp = true;
				public bool DockableWindow = false;
			}
			static Data _data;
			public static Data data
			{
				get
				{
					if (_data == null)
					{
						LoadUserPrefs();
					}
					return _data;
				}
			}

			public static void LoadUserPrefs()
			{
				string dataStr = EditorPrefs.GetString("TCP2_GlobalOptions", null);
				_data = new Data();
				if (!string.IsNullOrEmpty(dataStr))
				{
					EditorJsonUtility.FromJsonOverwrite(dataStr, _data);
				}
			}

			public static void SaveUserPrefs()
			{
				EditorPrefs.SetString("TCP2_GlobalOptions", EditorJsonUtility.ToJson(data));
			}
		}

		// Project Options only saved for this Unity project
		public static class ProjectOptions
		{
			[System.Serializable]
			public class Data
			{
				public bool AutoNames = true;
				public bool SubFolders = true;
				public bool OverwriteConfig = false;
				public bool LoadAllShaders = false;
				public string CustomOutputPath = ShaderGenerator2.OUTPUT_PATH;
				public string LastImplementationExportImportPath = Application.dataPath;
				public List<string> OpenedFoldouts = new List<string>();
				public bool UseCustomFont = false;
				public Font CustomFont = null;

				public bool CustomFontInitialized = false;
			}
			static Data _data;
			public static Data data
			{
				get
				{
					if (_data == null)
					{
						LoadProjectOptions();
					}
					return _data;
				}
			}

			static string GetPath()
			{
				return Application.dataPath.Replace(@"\","/") + "/../ProjectSettings/ToonyColorsPro.json";
			}

			public static void LoadProjectOptions()
			{
				_data = new Data();
				string path = GetPath();
				if (File.Exists(path))
				{
					string json = File.ReadAllText(path);
					EditorJsonUtility.FromJsonOverwrite(json, _data);
				}
			}

			public static void SaveProjectOptions()
			{
				string path = GetPath();
				string json = EditorJsonUtility.ToJson(_data, true);
				File.WriteAllText(path, json);
			}
		}
	}
}
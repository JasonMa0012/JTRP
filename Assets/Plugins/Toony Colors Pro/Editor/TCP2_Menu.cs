// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Menu Options for Toony Colors Pro 2

namespace ToonyColorsPro
{
	public static class Menu
	{
		//Change this path if you want the Toony Colors Pro menu to appear elsewhere in the menu bar
		public const string MENU_PATH = @"Tools/Toony Colors Pro/";

		//--------------------------------------------------------------------------------------------------
		// DOCUMENTATION

		[MenuItem(MENU_PATH + "Documentation", false, 100)]
		static void OpenDocumentation()
		{
			TCP2_GUI.OpenHelp();
		}

		//--------------------------------------------------------------------------------------------------
		// UNPACK SHADERS

		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/Rim (Desktop)", false, 800)]
		static void UnpackRim() { UnpackShaders("rim desktop"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/Rim (Mobile)", false, 800)]
		static void UnpackRimMobile() { UnpackShaders("rim mobile"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/Reflection (Desktop)", false, 800)]
		static void UnpackReflectionDesktop() { UnpackShaders("reflection desktop"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/Matcap (Mobile)", false, 800)]
		static void UnpackMatcapMobile() { UnpackShaders("matcap mobile"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/All Shaders (Mobile)", false, 800)]
		static void UnpackAllMobile() { UnpackShaders("mobile"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/All Shaders (Desktop)", false, 800)]
		static void UnpackAllDesktop() { UnpackShaders("desktop"); }
		[MenuItem(MENU_PATH + "Unpack Shaders (legacy)/All Shaders", false, 800)]
		static void UnpackAll() { UnpackShaders(""); }

		private static void UnpackShaders(string filter)
		{
			const string packedShadersGuid = "552f9a41dd13c0c44a9bb1aad0ec3598";
			var path = AssetDatabase.GUIDToAssetPath(packedShadersGuid);
			if (string.IsNullOrEmpty(path))
			{
				EditorApplication.Beep();
				Debug.LogError("[TCP2 Unpack Shaders] Couldn't find file: \"TCP2 Packed Shaders.tcp2data\"\nPlease reimport Toony Colors Pro.");
				return;
			}

			var fullPath = Application.dataPath + path.Substring("Assets".Length);
			if (File.Exists(fullPath))
			{
				var files = Utils.ExtractArchive(fullPath, filter);

				var @continue = 0;
				if (files.Length > 8)
				{
					do
					{
						@continue = EditorUtility.DisplayDialogComplex("TCP2 : Unpack Shaders", "You are about to import " + files.Length + " shaders in Unity.\nIt could take a few minutes!\nContinue?", "Yes", "No", "Help");
						if (@continue == 2)
						{
							TCP2_GUI.OpenHelpFor("Unpack Shaders");
						}
					}
					while (@continue == 2);
				}

				if (@continue == 0 && files.Length > 0)
				{
					var tcpRoot = Utils.FindReadmePath();
					foreach (var f in files)
					{
						var filePath = tcpRoot + f.path;
						var fileDir = Path.GetDirectoryName(filePath);
						if (!Directory.Exists(fileDir))
						{
							Directory.CreateDirectory(fileDir);
						}
						File.WriteAllText(filePath, f.content);
					}

					Debug.Log("Toony Colors Pro - Unpack Shaders:\n" + files.Length + (files.Length > 1 ? " shaders extracted." : " shader extracted."));
					AssetDatabase.Refresh();
				}

				if (files.Length == 0)
				{
					Debug.Log("Toony Colors Pro - Unpack Shaders:\nNothing to unpack. Shaders are probably already unpacked!");
				}
			}
		}

		//--------------------------------------------------------------------------------------------------
		// RESET MATERIAL

		[MenuItem(MENU_PATH + "Reset Selected Material(s)", false, 900)]
		static void ResetSelectedMaterials()
		{
			foreach (var o in Selection.objects)
			{
				if (o is Material)
				{
					var user = false;
					var keywordsList = new List<string>((o as Material).shaderKeywords);
					if (keywordsList.Contains("USER"))
						user = true;
					(o as Material).shaderKeywords = user ? new[] { "USER" } : new string[0];
					if ((o as Material).shader != null && (o as Material).shader.name.Contains("Mobile"))
						(o as Material).shader = Shader.Find("Toony Colors Pro 2/Legacy/Mobile");
					else
						(o as Material).shader = Shader.Find("Toony Colors Pro 2/Legacy/Desktop");
					Debug.Log("[TCP2] Keywords reset for " + o.name);
				}
			}
		}

		[MenuItem(MENU_PATH + "Reset Selected Material(s)", true, 900)]
		static bool ResetSelectedMaterials_Validate()
		{
			foreach (var o in Selection.objects)
			{
				if (o is Material)
				{
					return true;
				}
			}

			return false;
		}
	}
}
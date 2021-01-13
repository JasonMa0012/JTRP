// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Utility to generate meshes with encoded smoothed normals, to fix hard-edged broken outline
// TODO Fully use UV2 now that we can use float4, plus UV3/4 options

namespace ToonyColorsPro
{
	public class TCP2_SmoothedNormalsUtility : EditorWindow
	{
		[MenuItem(Menu.MENU_PATH + "Smoothed Normals Utility", false, 600)]
		static void OpenTool()
		{
			GetWindowTCP2();
		}

		private static TCP2_SmoothedNormalsUtility GetWindowTCP2()
		{
			var window = GetWindow<TCP2_SmoothedNormalsUtility>(true, "TCP2 : Smoothed Normals Utility", true);
			window.minSize = new Vector2(352f, 400f);
			window.maxSize = new Vector2(352f, 5000f);
			return window;
		}

		//--------------------------------------------------------------------------------------------------
		// INTERFACE

		private const string MESH_SUFFIX_DEFAULT = "[TCP2 Smoothed]";
		private string mFilenameSuffix = MESH_SUFFIX_DEFAULT;
#if UNITY_EDITOR_WIN
		private const string OUTPUT_FOLDER = "\\Smoothed Meshes\\";
#else
	private const string OUTPUT_FOLDER = "/Smoothed Meshes/";
#endif

		private class SelectedMesh
		{
			public SelectedMesh(Mesh _mesh, string _name, bool _isAsset, Object _assoObj = null, bool _skinned = false)
			{
				mesh = _mesh;
				name = _name;
				isAsset = _isAsset;
				AddAssociatedObject(_assoObj);

				isSkinned = _skinned;
				if (_assoObj != null && _assoObj is SkinnedMeshRenderer)
					isSkinned = true;
				else if (mesh != null && mesh.boneWeights != null && mesh.boneWeights.Length > 0)
					isSkinned = true;
			}

			public void AddAssociatedObject(Object _assoObj)
			{
				if (_assoObj != null)
				{
					_associatedObjects.Add(_assoObj);
				}
			}

			public Mesh mesh;
			public string name;
			public bool isAsset;
			public Object[] associatedObjects
			{
				get
				{
					if (_associatedObjects.Count == 0) return null;
					return _associatedObjects.ToArray();
				}
			}   //can be SkinnedMeshRenderer or MeshFilter
			public bool isSkinned;

			private List<Object> _associatedObjects = new List<Object>();
		}

		private Dictionary<Mesh, SelectedMesh> mMeshes;
		private string mFormat = "XYZ";
		private Utils.SmoothedNormalsChannel smoothedNormalChannel;
		private Utils.SmoothedNormalsUVType smoothedNormalUVType;
		private Vector2 mScroll;

		private bool mAlwaysOverwrite;
		private bool mCustomDirectory;
		private string mCustomDirectoryPath = "";

		//--------------------------------------------------------------------------------------------------

		private void LoadUserPrefs()
		{
			mAlwaysOverwrite = EditorPrefs.GetBool("TCP2SMU_mAlwaysOverwrite", false);
			mCustomDirectory = EditorPrefs.GetBool("TCP2SMU_mCustomDirectory", false);
			mCustomDirectoryPath = EditorPrefs.GetString("TCP2SMU_mCustomDirectoryPath", "/");
			mFilenameSuffix = EditorPrefs.GetString("TCP2SMU_mFilenameSuffix", MESH_SUFFIX_DEFAULT);
		}

		private void SaveUserPrefs()
		{
			EditorPrefs.SetBool("TCP2SMU_mAlwaysOverwrite", mAlwaysOverwrite);
			EditorPrefs.SetBool("TCP2SMU_mCustomDirectory", mCustomDirectory);
			EditorPrefs.SetString("TCP2SMU_mCustomDirectoryPath", mCustomDirectoryPath);
			EditorPrefs.SetString("TCP2SMU_mFilenameSuffix", mFilenameSuffix);
		}

		void OnEnable() { LoadUserPrefs(); }
		void OnDisable() { SaveUserPrefs(); }

		void OnFocus()
		{
			mMeshes = GetSelectedMeshes();
		}

		void OnSelectionChange()
		{
			mMeshes = GetSelectedMeshes();
			Repaint();
		}

		void OnGUI()
		{
			TCP2_GUI.UseNewHelpIcon = true;

			EditorGUILayout.BeginHorizontal();
			TCP2_GUI.HeaderBig("TCP 2 - SMOOTHED NORMALS UTILITY");
			TCP2_GUI.HelpButton("Smoothed Normals Utility");
			EditorGUILayout.EndHorizontal();
			TCP2_GUI.Separator();

			TCP2_GUI.UseNewHelpIcon = false;

			/*
			mFormat = EditorGUILayout.TextField(new GUIContent("Axis format", "Normals axis may need to be swapped before being packed into vertex colors/tangent/uv2 data. See documentation for more information."), mFormat);
			mFormat = Regex.Replace(mFormat, @"[^xyzXYZ-]", "");
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Known formats:");
			if(GUILayout.Button("XYZ", EditorStyles.miniButtonLeft)) { mFormat = "XYZ"; GUI.FocusControl(null); }
			if(GUILayout.Button("-YZ-X", EditorStyles.miniButtonMid)) { mFormat = "-YZ-X"; GUI.FocusControl(null); }
			if(GUILayout.Button("-Z-Y-X", EditorStyles.miniButtonRight)) { mFormat = "-Z-Y-X"; GUI.FocusControl(null); }
			EditorGUILayout.EndHorizontal();
			*/

			if (mMeshes != null && mMeshes.Count > 0)
			{
				GUILayout.Space(4);
				TCP2_GUI.Header("Meshes ready to be processed:", null, true);
				mScroll = EditorGUILayout.BeginScrollView(mScroll);
				TCP2_GUI.SeparatorSimple();
				bool hasSkinnedMeshes = false;
				foreach (var sm in mMeshes.Values)
				{
					GUILayout.Space(2);
					GUILayout.BeginHorizontal();
					var label = sm.name;
					if (label.Contains(mFilenameSuffix))
					{
						label = label.Replace(mFilenameSuffix, "\n" + mFilenameSuffix);
					}
					GUILayout.Label(label, EditorStyles.wordWrappedMiniLabel, GUILayout.Width(260));
					sm.isSkinned = GUILayout.Toggle(sm.isSkinned, new GUIContent(" Skinned", "Should be checked if the mesh will be used on a SkinnedMeshRenderer"));
					hasSkinnedMeshes |= sm.isSkinned;
					GUILayout.Space(6);
					GUILayout.EndHorizontal();
					GUILayout.Space(2);
					TCP2_GUI.SeparatorSimple();
				}
				EditorGUILayout.EndScrollView();
				GUILayout.FlexibleSpace();

				if (hasSkinnedMeshes)
				{
					EditorGUILayout.HelpBox("Smoothed Normals for Skinned meshes will be stored in Tangents only. See Help to know why.", MessageType.Warning);
				}

				if (GUILayout.Button(mMeshes.Count == 1 ? "Generate Smoothed Mesh" : "Generate Smoothed Meshes", GUILayout.Height(30)))
				{
					try
					{
						var selection = new List<Object>();
						float progress = 1;
						float total = mMeshes.Count;
						foreach (var sm in mMeshes.Values)
						{
							if (sm == null)
								continue;

							EditorUtility.DisplayProgressBar("Hold On", (mMeshes.Count > 1 ? "Generating Smoothed Meshes:\n" : "Generating Smoothed Mesh:\n") + sm.name, progress/total);
							progress++;
							Object o = CreateSmoothedMeshAsset(sm);
							if (o != null)
								selection.Add(o);
						}
						Selection.objects = selection.ToArray();
					}
					finally
					{
						EditorUtility.ClearProgressBar();
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Select one or multiple meshes to create a smoothed normals version.\n\nYou can also select models directly in the Scene, the new mesh will automatically be assigned.", MessageType.Info);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(true))
					GUILayout.Button("Generate Smoothed Mesh", GUILayout.Height(30));
			}

			TCP2_GUI.Separator();

			smoothedNormalChannel = (Utils.SmoothedNormalsChannel)EditorGUILayout.EnumPopup(TCP2_GUI.TempContent("Vertex Data Target", "Defines where to store the smoothed normals in the mesh; use a target where there isn't any data already."), smoothedNormalChannel);
			EditorGUI.BeginDisabledGroup(smoothedNormalChannel == Utils.SmoothedNormalsChannel.Tangents || smoothedNormalChannel == Utils.SmoothedNormalsChannel.VertexColors);
			smoothedNormalUVType = (Utils.SmoothedNormalsUVType)EditorGUILayout.EnumPopup(TCP2_GUI.TempContent("UV Data Type", "Defines where and how to store the smoothed normals in the target vertex UV channel."), smoothedNormalUVType);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.HelpBox("You will need to select the proper option in the Material Inspector depending on the selected target/format!", MessageType.Info);

			/*
			if (smoothedNormalChannel == Utils.SmoothedNormalsChannel.UV1 || smoothedNormalChannel == Utils.SmoothedNormalsChannel.UV3 || smoothedNormalChannel == Utils.SmoothedNormalsChannel.UV4 ||
				(smoothedNormalChannel == Utils.SmoothedNormalsChannel.UV2 && smoothedNormalUVType != Utils.SmoothedNormalsUVType.CompressedXY))
			{
				EditorGUILayout.HelpBox("Only shaders made with the Shader Generator 2 support all texture coordinates.\nOther shaders only support UV2 with 'Compressed XY' option. UV1, UV3, UV4 won't work with them, as well as 'Full XYZ' and 'Compressed ZW' data types.", MessageType.Warning);
			}
			*/

			TCP2_GUI.Separator();

			TCP2_GUI.Header("Options", null, true);
			mFilenameSuffix = EditorGUILayout.TextField(TCP2_GUI.TempContent("File name suffix"), mFilenameSuffix);
			mAlwaysOverwrite = EditorGUILayout.Toggle(new GUIContent("Always Overwrite", "Will always overwrite existing [TCP2 Smoothed] meshes"), mAlwaysOverwrite);
			mCustomDirectory = EditorGUILayout.Toggle(new GUIContent("Custom Output Directory", "Save the generated smoothed meshes in a custom directory"), mCustomDirectory);
			using (new EditorGUI.DisabledScope(!mCustomDirectory))
			{
				EditorGUILayout.BeginHorizontal();
				mCustomDirectoryPath = EditorGUILayout.TextField(GUIContent.none, mCustomDirectoryPath);
				if (GUILayout.Button("Select...", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					var outputPath = Utils.OpenFolderPanel_ProjectPath("Choose custom output directory for generated smoothed meshes", mCustomDirectoryPath);
					if (!string.IsNullOrEmpty(outputPath))
					{
						mCustomDirectoryPath = outputPath;
					}
				}
				EditorGUILayout.EndHorizontal();
			};

			GUILayout.Space(10);
		}

		//--------------------------------------------------------------------------------------------------

		private string GetSafeFilename(string name)
		{
			var invalidChars = new List<char>(Path.GetInvalidFileNameChars());
			var newName = new List<char>(name.Length);
			foreach (var c in name)
			{
				if (!invalidChars.Contains(c))
					newName.Add(c);
			}

			return new string(newName.ToArray());
		}

		private Mesh CreateSmoothedMeshAsset(SelectedMesh originalMesh)
		{
			//Check if we are ok to overwrite
			var overwrite = true;

			var rootPath = mCustomDirectory ? Application.dataPath + "/" + mCustomDirectoryPath + "/" : Utils.FindReadmePath() + OUTPUT_FOLDER;

			if (!Directory.Exists(rootPath))
				Directory.CreateDirectory(rootPath);

#if UNITY_EDITOR_WIN
			rootPath = rootPath.Replace(mCustomDirectory ? Application.dataPath : Utils.ToSystemSlashPath(Application.dataPath), "").Replace(@"\", "/");
#else
		rootPath = rootPath.Replace(Application.dataPath, "");
#endif

			var originalMeshName = GetSafeFilename(originalMesh.name);
			var assetPath = "Assets" + rootPath;
			var newAssetName = string.Format("{0}{1}.asset", originalMeshName, string.IsNullOrEmpty(mFilenameSuffix) ? "" : " " + mFilenameSuffix);
			if (originalMeshName.Contains(mFilenameSuffix))
			{
				newAssetName = originalMeshName + ".asset";
			}
			assetPath += newAssetName;
			var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
			var assetExists = (existingAsset != null) && originalMesh.isAsset;
			if (assetExists)
			{
				if (!mAlwaysOverwrite)
					overwrite = EditorUtility.DisplayDialog("TCP2 : Smoothed Mesh", "The following smoothed mesh already exists:\n\n" + newAssetName + "\n\nOverwrite?", "Yes", "No");

				if (!overwrite)
				{
					return null;
				}

				originalMesh.mesh = existingAsset;
				originalMesh.name = existingAsset.name;
			}

			var channel = originalMesh.isSkinned ? Utils.SmoothedNormalsChannel.Tangents : smoothedNormalChannel;
			Mesh newMesh = Utils.CreateSmoothedMesh(originalMesh.mesh, mFormat, channel, smoothedNormalUVType, !originalMesh.isAsset || (originalMesh.isAsset && assetExists));

			if (newMesh == null)
			{
				ShowNotification(new GUIContent("Couldn't generate the mesh for:\n" + originalMesh.name));
			}
			else
			{
				if (originalMesh.associatedObjects != null)
				{
					Undo.RecordObjects(originalMesh.associatedObjects, "Assign TCP2 Smoothed Mesh to Selection");

					foreach (var o in originalMesh.associatedObjects)
					{
						if (o is SkinnedMeshRenderer)
						{
							(o as SkinnedMeshRenderer).sharedMesh = newMesh;
						}
						else if (o is MeshFilter)
						{
							(o as MeshFilter).sharedMesh = newMesh;
						}
						else
						{
							Debug.LogWarning("[TCP2 Smoothed Normals Utility] Unrecognized AssociatedObject: " + o + "\nType: " + o.GetType());
						}
						EditorUtility.SetDirty(o);
					}
				}

				if (originalMesh.isAsset)
				{
					if (overwrite && !assetExists)
					{
						AssetDatabase.CreateAsset(newMesh, assetPath);
					}
				}
				else
					return null;
			}

			return newMesh;
		}

		private Dictionary<Mesh, SelectedMesh> GetSelectedMeshes()
		{
			var meshDict = new Dictionary<Mesh, SelectedMesh>();
			foreach (var o in Selection.objects)
			{
				var isProjectAsset = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o));

				//Assets from Project
				if (o is Mesh && !meshDict.ContainsKey(o as Mesh))
				{
					if ((o as Mesh) != null)
					{
						var sm = GetMeshToAdd(o as Mesh, isProjectAsset);
						if (sm != null)
							meshDict.Add(o as Mesh, sm);
					}
				}
				else if (o is GameObject && isProjectAsset)
				{
					var path = AssetDatabase.GetAssetPath(o);
					var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
					foreach (var asset in allAssets)
					{
						if (asset is Mesh && !meshDict.ContainsKey(asset as Mesh))
						{
							if ((asset as Mesh) != null)
							{
								var sm = GetMeshToAdd(asset as Mesh, isProjectAsset);
								if (sm.mesh != null)
									meshDict.Add(asset as Mesh, sm);
							}
						}
					}
				}
				//Assets from Hierarchy
				else if (o is GameObject && !isProjectAsset)
				{
					var renderers = (o as GameObject).GetComponentsInChildren<SkinnedMeshRenderer>();
					foreach (var r in renderers)
					{
						if (r.sharedMesh != null)
						{
							if (meshDict.ContainsKey(r.sharedMesh))
							{
								var sm = meshDict[r.sharedMesh];
								sm.AddAssociatedObject(r);
							}
							else
							{
								if (r.sharedMesh.name.Contains(mFilenameSuffix))
								{
									meshDict.Add(r.sharedMesh, new SelectedMesh(r.sharedMesh, r.sharedMesh.name, false));
								}
								else
								{
									if (r.sharedMesh != null)
									{
										var sm = GetMeshToAdd(r.sharedMesh, true, r);
										if (sm.mesh != null)
											meshDict.Add(r.sharedMesh, sm);
									}
								}
							}
						}
					}

					var mfilters = (o as GameObject).GetComponentsInChildren<MeshFilter>();
					foreach (var mf in mfilters)
					{
						if (mf.sharedMesh != null)
						{
							if (meshDict.ContainsKey(mf.sharedMesh))
							{
								var sm = meshDict[mf.sharedMesh];
								sm.AddAssociatedObject(mf);
							}
							else
							{
								if (mf.sharedMesh.name.Contains(mFilenameSuffix))
								{
									meshDict.Add(mf.sharedMesh, new SelectedMesh(mf.sharedMesh, mf.sharedMesh.name, false));
								}
								else
								{
									if (mf.sharedMesh != null)
									{
										var sm = GetMeshToAdd(mf.sharedMesh, true, mf);
										if (sm.mesh != null)
											meshDict.Add(mf.sharedMesh, sm);
									}
								}
							}
						}
					}
				}
			}

			return meshDict;
		}

		private SelectedMesh GetMeshToAdd(Mesh mesh, bool isProjectAsset, Object _assoObj = null)
		{
			var meshPath = AssetDatabase.GetAssetPath(mesh);
			var meshAsset = AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) as Mesh;
			//If null, it can be a built-in Unity mesh
			if (meshAsset == null)
			{
				return new SelectedMesh(mesh, mesh.name, isProjectAsset, _assoObj);
			}
			var meshName = mesh.name;
			if (!AssetDatabase.IsMainAsset(meshAsset))
			{
				var main = AssetDatabase.LoadMainAssetAtPath(meshPath);
				meshName = main.name + " - " + meshName + "_" + mesh.GetInstanceID();
			}

			var sm = new SelectedMesh(mesh, meshName, isProjectAsset, _assoObj);
			return sm;
		}

		private bool SelectedMeshListContains(List<SelectedMesh> list, Mesh m)
		{
			foreach (var sm in list)
				if (sm.mesh == m)
					return true;

			return false;
		}
	}
}
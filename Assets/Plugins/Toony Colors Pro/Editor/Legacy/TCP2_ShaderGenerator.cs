// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

//#define DEBUG_MODE

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;

// Utility to generate custom Toony Colors Pro 2 shaders with specific features

namespace ToonyColorsPro
{
	namespace Legacy
	{
		public partial class TCP2_ShaderGenerator : EditorWindow
		{
			//--------------------------------------------------------------------------------------------------

			[MenuItem(Menu.MENU_PATH + "Shader Generator", false, 500)]
			static void OpenTool()
			{
				GetWindowTCP2();
			}

			public static TCP2_ShaderGenerator OpenWithShader(Shader shader)
			{
				var shaderGenerator = GetWindowTCP2();
				shaderGenerator.LoadCurrentConfigFromShader(shader);
				return shaderGenerator;
			}

			private static TCP2_ShaderGenerator GetWindowTCP2()
			{
				var window = GetWindow<TCP2_ShaderGenerator>(true, "TCP2 : Shader Generator", true);
				window.minSize = new Vector2(375f, 400f);
				window.maxSize = new Vector2(500f, 900f);
				return window;
			}

			//--------------------------------------------------------------------------------------------------
			// UI

			//Represents a template
			public class ShaderGeneratorTemplate
			{
				public static ShaderGeneratorTemplate CurrentTemplate;

				public TextAsset textAsset { get; private set; }
				public string templateInfo;
				public string templateWarning;
				public string templateType;
				public bool newSystem;          //if false, use the hard-coded GUI and dependencies/conditions
				public bool sg2;
				public bool valid;
				public UIFeature[] uiFeatures;

				public ShaderGeneratorTemplate()
				{
					TryLoadTextAsset();
				}

				public void SetTextAsset(TextAsset templateAsset)
				{
					if (textAsset != templateAsset)
					{
						textAsset = templateAsset;
						UpdateTemplateMeta();
					}
				}

				public void Reload()
				{
					UpdateTemplateMeta();
				}

				public void FeaturesGUI(TCP2_Config config)
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

				public string GetMaskDisplayName(string maskFeature)
				{
					foreach (var uiFeature in uiFeatures)
					{
						if (uiFeature is UIFeature_Mask && (uiFeature as UIFeature_Mask).MaskKeyword == maskFeature)
						{
							return (uiFeature as UIFeature_Mask).DisplayName;
						}
					}

					return "Unknown Mask";
				}

				public bool GetMaskDependency(string maskFeature, TCP2_Config config)
				{
					foreach (var uiFeature in uiFeatures)
					{
						if (uiFeature is UIFeature_Mask && (uiFeature as UIFeature_Mask).Keyword == maskFeature)
						{
							return uiFeature.Enabled(config);
						}
					}

					return true;
				}

				//Try to load a Template according to a config type and/or file
				public void TryLoadTextAsset(TCP2_Config config = null)
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

					//New name as of 2.3
					loadedTextAsset = LoadTextAsset("TCP2_ShaderTemplate_Default.txt");
					if (loadedTextAsset != null)
					{
						SetTextAsset(loadedTextAsset);
						return;
					}

					//Old legacy name
					loadedTextAsset = LoadTextAsset("TCP2_User_Unity5.txt");
					if (loadedTextAsset != null)
					{
						SetTextAsset(loadedTextAsset);
					}
				}

				//--------

				private TextAsset LoadTextAsset(string filename)
				{
					string rootPath = Utils.FindReadmePath(true);
					var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(string.Format("{0}/Shader Templates/{1}", rootPath, filename));
					if (asset == null)
					{
						var filenameNoExtension = Path.GetFileNameWithoutExtension(filename);
						var guids = AssetDatabase.FindAssets(string.Format("{0} t:TextAsset", filenameNoExtension));
						if (guids.Length >= 1)
						{
							var path = AssetDatabase.GUIDToAssetPath(guids[0]);
							asset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
						}
					}

					return asset;
				}


				private void UpdateTemplateMeta()
				{
					uiFeatures = null;
					newSystem = false;
					templateInfo = null;
					templateWarning = null;
					templateType = null;
					valid = false;
					sg2 = false;

					UIFeature.ClearFoldoutStack();

					if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
					{
						using (var reader = new StringReader(textAsset.text))
						{
							string line;
							while ((line = reader.ReadLine()) != null)
							{
								if (line.StartsWith("#INFO="))
								{
									templateInfo = line.Substring(6).TrimEnd().Replace("  ", "\n");
								}

								else if (line.StartsWith("#WARNING="))
								{
									templateWarning = line.Substring(9).TrimEnd().Replace("  ", "\n");
								}

								else if (line.StartsWith("#CONFIG="))
								{
									templateType = line.Substring(8).TrimEnd().ToLower();
								}

								else if (line.StartsWith("#FEATURES"))
								{
									newSystem = true;
									uiFeatures = UIFeature.GetUIFeatures(reader);
								}
								
								else if (line.StartsWith("#SG2"))
								{
									sg2 = true;
									return;
								}

								//Config meta should appear before the Shader name line
								else if (line.StartsWith("Shader"))
								{
									valid = uiFeatures != null;
									return;
								}
							}
						}
					}
				}
			}

			private ShaderGeneratorTemplate _Template;
			private ShaderGeneratorTemplate Template
			{
				get
				{
					if (_Template == null)
						_Template = new ShaderGeneratorTemplate();
					return _Template;
				}
			}

			private TextAsset[] LoadAllTemplates()
			{
				var list = new List<TextAsset>();

				var systemPath = Application.dataPath + @"/JMO Assets/Toony Colors Pro/Shader Templates/";
				if (!Directory.Exists(systemPath))
				{
					var rootDir = Utils.FindReadmePath();
					systemPath = rootDir.Replace(@"\", "/") + "/Shader Templates/";
				}

				if (Directory.Exists(systemPath))
				{
					var txtFiles = Utils.GetFilesSafe(systemPath, "*.txt");

					foreach (var sysPath in txtFiles)
					{
						var unityPath = sysPath;
						if (Utils.SystemToUnityPath(ref unityPath))
						{
							var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(unityPath);
							if (textAsset != null && !list.Contains(textAsset))
							{
								list.Add(textAsset);
							}
						}
					}

					list.Sort((x, y) => x.name.CompareTo(y.name));
					return list.ToArray();
				}

				return null;
			}

			//--------------------------------------------------------------------------------------------------
			// INTERFACE

			private Shader mCurrentShader;
			private TCP2_Config mCurrentConfig;
			private int mCurrentHash;
			private bool mFirstHashPass;
			private Shader[] mUserShaders;
			private GenericMenu loadTemplateMenu;
			private List<string> mUserShadersLabels;
			private GenericMenu loadShadersMenu;
			private Vector2 mScrollPosition;
			private int mConfigChoice;
			private bool mDirtyConfig;
			private Color unsavedChangesColor = new Color(1f, 1f, 0.7f);

			//Static
			private static bool sHideDisabled;
			private static bool sAutoNames;
			private static bool sOverwriteConfigs;
			private static bool sLoadAllShaders;
			private static bool sSelectGeneratedShader;
			private static bool sGUIEnabled;
			private static List<string> sOpenedFoldouts = new List<string>();

#if DEBUG_MODE
	private string mDebugText;
	private bool mDebugExpandUserData;
	private ShaderImporter mCurrentShaderImporter;
#endif

			void OnEnable()
			{
				LoadUserPrefs();
				ReloadUserShaders();
				NewShader();

				var allTemplates = LoadAllTemplates();
				if (allTemplates != null && allTemplates.Length > 0)
				{
					var menuItems = new List<object[]>();

					loadTemplateMenu = new GenericMenu();
					foreach (var textAsset in allTemplates)
					{
						//Exceptions
						if (textAsset.name.Contains("TCP2_User_Unity5_Old"))
							continue;

						//Find name
						var name = textAsset.name;
						var sr = new StringReader(textAsset.text);
						string line;
						while ((line = sr.ReadLine()) != null)
						{
							if (line.StartsWith("#NAME"))
							{
								name = line.Substring(6);
								break;
							}
							if (line.StartsWith("#FEATURES"))
							{
								break;
							}
						}

						menuItems.Add(new object[] { name, textAsset });
					}

					//Put submenus at the end of list
					var menuItemsSorted = new List<object[]>();
					for (var i = menuItems.Count-1; i >= 0; i--)
					{
						if ((menuItems[i][0] as string).Contains("/"))
							menuItemsSorted.Add(menuItems[i]);
						else
							menuItemsSorted.Insert(0, menuItems[i]);
					}
					foreach (var item in menuItemsSorted)
						loadTemplateMenu.AddItem(new GUIContent(item[0] as string), false, OnLoadTemplate, item[1] as TextAsset);
				}
			}

			void OnLoadTemplate(object textAsset)
			{
				Template.SetTextAsset(textAsset as TextAsset);
			}

			void OnDisable()
			{
				SaveUserPrefs();
			}

			void OnGUI()
			{
				sGUIEnabled = GUI.enabled;

				EditorGUILayout.BeginHorizontal();
				TCP2_GUI.HeaderBig("TOONY COLORS PRO 2 - SHADER GENERATOR");
				TCP2_GUI.HelpButton("Shader Generator");
				EditorGUILayout.EndHorizontal();
				TCP2_GUI.Separator();

				var lW = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 105f;

				//Avoid refreshing Template meta at every Repaint
				EditorGUILayout.BeginHorizontal();
				var _tmpTemplate = EditorGUILayout.ObjectField("Template:", Template.textAsset, typeof(TextAsset), false) as TextAsset;
				if (_tmpTemplate != Template.textAsset)
				{
					Template.SetTextAsset(_tmpTemplate);
				}
				//Load template
				if (loadTemplateMenu != null)
				{
					if (GUILayout.Button("Load ▼", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
					{
						loadTemplateMenu.ShowAsContext();
					}
				}
				/*
				if(GUILayout.Button("Reload", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					Template.Reload();
				}
				*/
				EditorGUILayout.EndHorizontal();

				//Template not found
				if (Template == null || Template.textAsset == null)
				{
					EditorGUILayout.HelpBox("Couldn't find template file!\n\nVerify that the file 'TCP2_ShaderTemplate_Default.txt' is in your project.\nPlease reimport the pack if you can't find it!", MessageType.Error);
					return;
				}

				//Infobox for custom templates
				if (!string.IsNullOrEmpty(Template.templateInfo))
				{
					TCP2_GUI.HelpBoxLayout(Template.templateInfo, MessageType.Info);
				}
				if (!string.IsNullOrEmpty(Template.templateWarning))
				{
					TCP2_GUI.HelpBoxLayout(Template.templateWarning, MessageType.Warning);
				}

				TCP2_GUI.Separator();

				//If current shader is unsaved, show yellow color
				var gColor = GUI.color;
				GUI.color = mDirtyConfig ? gColor * unsavedChangesColor : GUI.color;

				EditorGUI.BeginChangeCheck();
				mCurrentShader = EditorGUILayout.ObjectField("Current Shader:", mCurrentShader, typeof(Shader), false) as Shader;
				if (EditorGUI.EndChangeCheck())
				{
					if (mCurrentShader != null)
					{
						LoadCurrentConfigFromShader(mCurrentShader);
					}
				}
				EditorGUILayout.BeginHorizontal();

				GUILayout.Space(EditorGUIUtility.labelWidth + 4);
				if (mDirtyConfig)
				{
					var guiContent = new GUIContent("Unsaved changes");
					var rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.helpBox, GUILayout.Height(16));
					rect.y -= 2;
					GUI.Label(rect, guiContent, EditorStyles.helpBox);
				}

				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(mCurrentShader == null))
				{
					if (GUILayout.Button("Copy", EditorStyles.miniButtonLeft, GUILayout.Width(60f), GUILayout.Height(16)))
					{
						CopyShader();
					}
				}
				if (GUILayout.Button("Load ▼", EditorStyles.miniButtonMid, GUILayout.Width(60f), GUILayout.Height(16)))
				{
					loadShadersMenu.ShowAsContext();
				}
				if (GUILayout.Button("New", EditorStyles.miniButtonRight, GUILayout.Width(60f), GUILayout.Height(16)))
				{
					NewShader();
				}
				GUILayout.Space(18);    //leave space to align with the Object Field box
				EditorGUILayout.EndHorizontal();
				GUI.color = gColor;

				if (mCurrentConfig == null)
				{
					NewShader();
				}

				if (mCurrentConfig.isModifiedExternally)
				{
					EditorGUILayout.HelpBox("It looks like this shader has been modified externally/manually. Updating it will overwrite the changes.", MessageType.Warning);
				}

				EditorGUIUtility.labelWidth = lW;

				//Name & Filename
				TCP2_GUI.Separator();
				GUI.enabled = (mCurrentShader == null);
				EditorGUI.BeginChangeCheck();
				mCurrentConfig.ShaderName = EditorGUILayout.TextField(new GUIContent("Shader Name", "Path will indicate how to find the Shader in Unity's drop-down list"), mCurrentConfig.ShaderName);
				mCurrentConfig.ShaderName = Regex.Replace(mCurrentConfig.ShaderName, @"[^a-zA-Z0-9 _!/]", "");
				if (EditorGUI.EndChangeCheck() && sAutoNames)
				{
					mCurrentConfig.AutoNames();
				}
				GUI.enabled &= !sAutoNames;
				EditorGUILayout.BeginHorizontal();
				mCurrentConfig.Filename = EditorGUILayout.TextField("File Name", mCurrentConfig.Filename);
				mCurrentConfig.Filename = Regex.Replace(mCurrentConfig.Filename, @"[^a-zA-Z0-9 _!/]", "");
				GUILayout.Label(".shader", GUILayout.Width(50f));
				EditorGUILayout.EndHorizontal();
				GUI.enabled = sGUIEnabled;

				TCP2_GUI.Separator();

				//########################################################################################################
				// FEATURES

				TCP2_GUI.Header("FEATURES");

				//Scroll view
				mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
				EditorGUI.BeginChangeCheck();

				if (Template.sg2)
				{
					EditorGUILayout.HelpBox("This template is for the Shader Generator 2 only.\nThis is the Shader Generator 1.", MessageType.Error);
				}
				else if (!Template.valid)
				{
					EditorGUILayout.HelpBox("This doesn't seem to be a valid template file.", MessageType.Error);
				}
				else if (!Template.newSystem)
				{
					EditorGUILayout.HelpBox("Old template versions aren't supported anymore.", MessageType.Warning);
				}
				else
				{
					//New UI embedded into Template
					Template.FeaturesGUI(mCurrentConfig);

					if (mFirstHashPass)
					{
						mCurrentHash = mCurrentConfig.ToHash();
						mFirstHashPass = false;
					}
				}

#if DEBUG_MODE
		TCP2_GUI.SeparatorBig();

		TCP2_GUI.SubHeaderGray("DEBUG MODE");

		GUILayout.BeginHorizontal();
		mDebugText = EditorGUILayout.TextField("Custom", mDebugText);
		if(GUILayout.Button("Add Feature", EditorStyles.miniButtonLeft, GUILayout.Width(80f)))
			mCurrentConfig.Features.Add(mDebugText);
		if(GUILayout.Button("Add Flag", EditorStyles.miniButtonRight, GUILayout.Width(80f)))
			mCurrentConfig.Flags.Add(mDebugText);

		GUILayout.EndHorizontal();
		GUILayout.Label("Features:");
		GUILayout.BeginHorizontal();
		int count = 0;
		for(int i = 0; i < mCurrentConfig.Features.Count; i++)
		{
			if(count >= 3)
			{
				count = 0;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			count++;
			if(GUILayout.Button(mCurrentConfig.Features[i], EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				mCurrentConfig.Features.RemoveAt(i);
				break;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Flags:");
		GUILayout.BeginHorizontal();
		count = 0;
		for(int i = 0; i < mCurrentConfig.Flags.Count; i++)
		{
			if(count >= 3)
			{
				count = 0;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			count++;
			if(GUILayout.Button(mCurrentConfig.Flags[i], EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				mCurrentConfig.Flags.RemoveAt(i);
				break;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Keywords:");
		GUILayout.BeginHorizontal();
		count = 0;
		foreach(KeyValuePair<string,string> kvp in mCurrentConfig.Keywords)
		{
			if(count >= 3)
			{
				count = 0;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			count++;
			if(GUILayout.Button(kvp.Key + ":" + kvp.Value, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				mCurrentConfig.Keywords.Remove(kvp.Key);
				break;
			}
		}
		GUILayout.EndHorizontal();

		//----------------------------------------------------------------

		Space();
		if(mCurrentShader != null)
		{
			if(mCurrentShaderImporter == null)
			{
				mCurrentShaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(mCurrentShader)) as ShaderImporter;
			}

			if (mCurrentShaderImporter != null && mCurrentShaderImporter.GetShader() == mCurrentShader)
			{
				mDebugExpandUserData = EditorGUILayout.Foldout(mDebugExpandUserData, "Shader UserData");
				if(mDebugExpandUserData)
				{
					string[] userData = mCurrentShaderImporter.userData.Split(',');
					foreach(var str in userData)
					{
						GUILayout.Label(str);
					}
				}
			}
		}
#endif

				//Update config
				if (EditorGUI.EndChangeCheck())
				{
					var newHash = mCurrentConfig.ToHash();
					if (newHash != mCurrentHash)
					{
						mDirtyConfig = true;
					}
					else
					{
						mDirtyConfig = false;
					}
				}

				//Scroll view
				EditorGUILayout.EndScrollView();

				Space();

				//GENERATE

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUI.color = mDirtyConfig ? gColor * unsavedChangesColor : GUI.color;
				if (GUILayout.Button(mCurrentShader == null ? "Generate Shader" : "Update Shader", GUILayout.Width(120f), GUILayout.Height(30f)))
				{
					if (Template == null)
					{
						EditorUtility.DisplayDialog("TCP2 : Shader Generation", "Can't generate shader: no Template file defined!\n\nYou most likely want to link the TCP2_User.txt file to the Template field in the Shader Generator.", "Ok");
						return;
					}

					//Set config type
					if (Template.templateType != null)
					{
						mCurrentConfig.configType = Template.templateType;
					}

					//Set config file
					mCurrentConfig.templateFile = Template.textAsset.name;

					var generatedShader = TCP2_ShaderGeneratorUtils.Compile(mCurrentConfig, mCurrentShader, Template, true, !sOverwriteConfigs);
					ReloadUserShaders();
					if (generatedShader != null)
					{
						mDirtyConfig = false;
						LoadCurrentConfigFromShader(generatedShader);
					}

					//Workaround to force the inspector to refresh, so that state is reset.
					//Needed in case of switching between specular/metallic and related
					//options, while the inspector is opened, so that it shows/hides the
					//relevant properties according to the changes.
					TCP2_MaterialInspector_SurfacePBS_SG.InspectorNeedsUpdate = true;
				}
				GUI.color = gColor;
				EditorGUILayout.EndHorizontal();
				TCP2_GUI.Separator();

				// OPTIONS
				TCP2_GUI.Header("OPTIONS");

				GUILayout.BeginHorizontal();
				sSelectGeneratedShader = GUILayout.Toggle(sSelectGeneratedShader, new GUIContent("Select Generated Shader", "Will select the generated file in the Project view"), GUILayout.Width(180f));
				sAutoNames = GUILayout.Toggle(sAutoNames, new GUIContent("Automatic Name", "Will automatically generate the shader filename based on its UI name"), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				sOverwriteConfigs = GUILayout.Toggle(sOverwriteConfigs, new GUIContent("Always overwrite shaders", "Overwrite shaders when generating/updating (no prompt)"), GUILayout.Width(180f));
				sHideDisabled = GUILayout.Toggle(sHideDisabled, new GUIContent("Hide disabled fields", "Hide properties settings when they cannot be accessed"), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				TCP2_ShaderGeneratorUtils.CustomOutputDir = GUILayout.Toggle(TCP2_ShaderGeneratorUtils.CustomOutputDir, new GUIContent("Custom Output Directory:", "Will save the generated shaders in a custom directory within the Project"), GUILayout.Width(165f));
				GUI.enabled &= TCP2_ShaderGeneratorUtils.CustomOutputDir;
				if (TCP2_ShaderGeneratorUtils.CustomOutputDir)
				{
					TCP2_ShaderGeneratorUtils.OutputPath = EditorGUILayout.TextField("", TCP2_ShaderGeneratorUtils.OutputPath);
					if (GUILayout.Button("Select...", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
					{
						var outputPath = Utils.OpenFolderPanel_ProjectPath("Choose custom output directory for TCP2 generated shaders", TCP2_ShaderGeneratorUtils.OutputPath);
						if (!string.IsNullOrEmpty(outputPath))
						{
							TCP2_ShaderGeneratorUtils.OutputPath = outputPath;
						}
					}
				}
				else
					EditorGUILayout.TextField("", TCP2_ShaderGeneratorUtils.OUTPUT_PATH);
				if (EditorGUI.EndChangeCheck())
				{
					ReloadUserShaders();
				}

				GUI.enabled = sGUIEnabled;
				EditorGUILayout.EndHorizontal();

				EditorGUI.BeginChangeCheck();
				sLoadAllShaders = GUILayout.Toggle(sLoadAllShaders, new GUIContent("Reload Shaders from all Project", "Load shaders from all your Project folders instead of just Toony Colors Pro 2.\nEnable it if you move your generated shader files outside of the default TCP2 Generated folder."), GUILayout.ExpandWidth(false));
				if (EditorGUI.EndChangeCheck())
				{
					ReloadUserShaders();
				}

				TCP2_ShaderGeneratorUtils.SelectGeneratedShader = sSelectGeneratedShader;
			}

			void OnProjectChange()
			{
				ReloadUserShaders();
				if (mCurrentShader == null && mConfigChoice != 0)
				{
					NewShader();
				}
			}

			//--------------------------------------------------------------------------------------------------
			// MISC

			private void LoadUserPrefs()
			{
				sAutoNames = EditorPrefs.GetBool("TCP2_mAutoNames", true);
				sOverwriteConfigs = EditorPrefs.GetBool("TCP2_mOverwriteConfigs", true);
				sHideDisabled = EditorPrefs.GetBool("TCP2_mHideDisabled", true);
				sSelectGeneratedShader = EditorPrefs.GetBool("TCP2_mSelectGeneratedShader", true);
				sLoadAllShaders = EditorPrefs.GetBool("TCP2_mLoadAllShaders", false);
				mConfigChoice = EditorPrefs.GetInt("TCP2_mConfigChoice", 0);
				TCP2_ShaderGeneratorUtils.CustomOutputDir = EditorPrefs.GetBool("TCP2_TCP2_ShaderGeneratorUtils.CustomOutputDir", false);

				sOpenedFoldouts.Clear();
				sOpenedFoldouts.AddRange(EditorPrefs.GetString("TCP2_sOpenedFoldouts", "").Split(','));
			}

			private void SaveUserPrefs()
			{
				EditorPrefs.SetBool("TCP2_mAutoNames", sAutoNames);
				EditorPrefs.SetBool("TCP2_mOverwriteConfigs", sOverwriteConfigs);
				EditorPrefs.SetBool("TCP2_mHideDisabled", sHideDisabled);
				EditorPrefs.SetBool("TCP2_mSelectGeneratedShader", sSelectGeneratedShader);
				EditorPrefs.SetBool("TCP2_mLoadAllShaders", sLoadAllShaders);
				EditorPrefs.SetInt("TCP2_mConfigChoice", mConfigChoice);
				EditorPrefs.SetBool("TCP2_TCP2_ShaderGeneratorUtils.CustomOutputDir", TCP2_ShaderGeneratorUtils.CustomOutputDir);

				var openedFoldouts = string.Join(",", sOpenedFoldouts.ToArray());
				EditorPrefs.SetString("TCP2_sOpenedFoldouts", openedFoldouts);
			}

			private void LoadCurrentConfig(TCP2_Config config, bool loadConfigTemplate = true)
			{
				mCurrentConfig = config;
				mDirtyConfig = false;
				if (sAutoNames)
				{
					mCurrentConfig.AutoNames();
				}
				mCurrentHash = mCurrentConfig.ToHash();
				mFirstHashPass = false;

				if (loadConfigTemplate)
					Template.TryLoadTextAsset(mCurrentConfig);
			}

			private void NewShader()
			{
				mCurrentShader = null;
				mConfigChoice = 0;
				LoadCurrentConfig(new TCP2_Config(), false);
				mFirstHashPass = true;
			}

			private void CopyShader()
			{
				mCurrentShader = null;
				mConfigChoice = 0;
				var newConfig = mCurrentConfig.Copy();
				newConfig.ShaderName += " Copy";
				newConfig.Filename += " Copy";
				LoadCurrentConfig(newConfig);
				mFirstHashPass = false;
			}

			private void LoadCurrentConfigFromShader(Shader shader)
			{
				mCurrentConfig = TCP2_Config.CreateFromShader(shader);
				if (mCurrentConfig != null)
				{
					mCurrentShader = shader;
					mConfigChoice = mUserShadersLabels.IndexOf(shader.name);
					mDirtyConfig = false;
					mCurrentHash = mCurrentConfig.ToHash();
					mFirstHashPass = false;

					//Load appropriate template
					Template.TryLoadTextAsset(mCurrentConfig);
				}
				else
				{
					EditorApplication.Beep();
					ShowNotification(new GUIContent("Invalid shader loaded: it doesn't seem to have been generated by the TCP2 Shader Generator!"));
					mCurrentShader = null;
					NewShader();
				}
			}

			private void ReloadUserShaders()
			{
				mUserShaders = GetUserShaders();
				mUserShadersLabels = new List<string>(GetShaderLabels(mUserShaders));

				if (mCurrentShader != null)
				{
					mConfigChoice = mUserShadersLabels.IndexOf(mCurrentShader.name);
				}

				//Load shader menu
				loadShadersMenu = new GenericMenu();
				loadShadersMenu.AddItem(new GUIContent("New Shader"), false, NewShader);
				loadShadersMenu.AddSeparator("");
				for (var i = 0; i < mUserShaders.Length; i++)
				{
					if (mUserShaders[i] != null)
						loadShadersMenu.AddItem(new GUIContent(mUserShaders[i].name), false, OnLoadShaderFromMenu, mUserShaders[i]);
				}
			}

			void OnLoadShaderFromMenu(object shaderObj)
			{
				var shader = shaderObj as Shader;
				if (shader != null)
					LoadCurrentConfigFromShader(shader);
			}

			private Shader[] GetUserShaders()
			{
				var rootPath = Application.dataPath + (sLoadAllShaders ? "" : TCP2_ShaderGeneratorUtils.OutputPath);

				if (Directory.Exists(rootPath))
				{
					var paths = Utils.GetFilesSafe(rootPath, "*.shader");
					var shaderList = new List<Shader>();

					foreach (var path in paths)
					{
#if UNITY_EDITOR_WIN
						var assetPath = "Assets" + path.Replace(@"\", @"/").Replace(Application.dataPath, "");
#else
				string assetPath = "Assets" + path.Replace(Application.dataPath, "");
#endif
						var shaderImporter = ShaderImporter.GetAtPath(assetPath) as ShaderImporter;
						if (shaderImporter != null)
						{
							if (shaderImporter.userData.Contains("USER"))
							{
								var shader = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader)) as Shader;
								if (shader != null && !shaderList.Contains(shader))
									shaderList.Add(shader);
							}
						}
					}

					return shaderList.ToArray();
				}

				return new Shader[0];
			}

			private string[] GetShaderLabels(Shader[] array, string firstOption = "New Shader")
			{
				if (array == null)
				{
					return new string[0];
				}

				var labelsList = new List<string>();
				if (!string.IsNullOrEmpty(firstOption))
					labelsList.Add(firstOption);
				foreach (var shader in array)
				{
					labelsList.Add(shader.name);
				}
				return labelsList.ToArray();
			}

			private static void Space()
			{
				var color = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);
				TCP2_GUI.GUILine(color, 1);
				GUILayout.Space(1);
			}
		}
	}
}
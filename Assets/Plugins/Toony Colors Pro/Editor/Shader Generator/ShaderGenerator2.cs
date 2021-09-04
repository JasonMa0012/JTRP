// Toony Colors Pro 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ToonyColorsPro.Utilities;
using Object = UnityEngine.Object;

// Utility to generate custom Toony Colors Pro 2 shaders with specific features

namespace ToonyColorsPro
{
	namespace ShaderGenerator
	{
		public class ShaderGenerator2 : EditorWindow
		{
			public static bool DebugMode = false;

			internal const string TCP2_VERSION = "2.5.2";
			internal const string DOCUMENTATION_URL = "https://jeanmoreno.com/unity/toonycolorspro/doc/shader_generator_2";
			internal const string OUTPUT_PATH = "/JMO Assets/Toony Colors Pro/Shaders Generated/";

			[MenuItem(Menu.MENU_PATH + "Shader Generator 2 (beta)", false, 500)]
			static void OpenTool()
			{
				GetWindowTCP2();
			}

			internal static ShaderGenerator2 OpenWithShader(Shader shader)
			{
				var shaderGenerator = GetWindowTCP2();
				shaderGenerator.LoadCurrentConfigFromShader(shader);
				return shaderGenerator;
			}

			static ShaderGenerator2 GetWindowTCP2()
			{
				var window = GetWindow<ShaderGenerator2>(!GlobalOptions.data.DockableWindow, GlobalOptions.data.DockableWindow ? "Shader Generator" : "Shader Generator 2 (beta)", true);
				window.minSize = new Vector2(375f, 400f);
				window.maxSize = new Vector2(500f, 4000f);
				return window;
			}

			//Only one window at a time, so this should always be the correct value.
			//Used to create communication between Shader Properties and Custom Material Properties
			internal static Config CurrentConfig { get; private set; }
			internal static string TemplateID { get; private set; }
			internal static VertexToFragmentVariablesManager VariablesManager { get; private set; }
			internal static ShaderProperty.ProgramType CurrentProgram = ShaderProperty.ProgramType.Undefined;
			internal static bool IsInLightingFunction = false;
			internal static bool CurrentPassHasLightingFunction = false;
			internal static bool NeedsHashUpdate = false;
			internal static bool NeedsShaderPropertiesUpdate = false;

			internal static bool showDynamicTooltip;
			internal static string dynamicTooltip;

			internal static int _GlobalUniqueId = 100;
			internal static int GlobalUniqueId
			{
				get
				{
					int id = _GlobalUniqueId;
					_GlobalUniqueId++;
					return id;
				}
			}

			static ShaderGenerator2 instance;

			//--------------------------------------------------------------------------------------------------

			public static bool UpdateShader(Shader shader, bool progressBar, bool overwrite)
			{
				var config = Config.CreateFromShader(shader);
				if (config != null)
				{
					var template = new Template();
					template.TryLoadTextAsset(config);
					if (template.valid)
					{
						Compile(config, shader, template, progressBar, !overwrite);
						return true;
					}
				}
				return false;
			}

			//--------------------------------------------------------------------------------------------------

			//Represents a template
			Template _template;
			Template template { get { return _template ?? (_template = new Template()); } }

			static TextAsset[] LoadAllTemplates()
			{
				var list = new List<TextAsset>();

				var systemPath = Application.dataPath + @"/JMO Assets/Toony Colors Pro/Shader Templates 2/";
				if (!Directory.Exists(systemPath))
				{
					var rootDir = Utils.FindReadmePath();
					systemPath = rootDir.Replace(@"\", "/") + "/Shader Templates 2/";
				}

				if (Directory.Exists(systemPath))
				{
					var txtFiles = Utils.GetFilesSafe(systemPath, "*.txt");

					foreach (var sysPath in txtFiles)
					{
						var unityPath = sysPath;
						if (Utils.SystemToUnityPath(ref unityPath))
						{
							// Hard-coded filtering, might need a generic system eventually:
#if UNITY_2019_3_OR_NEWER
							if (unityPath.EndsWith("_LWRP.txt")) continue;
#else
							if (unityPath.EndsWith("_URP.txt")) continue;
#endif

							var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(unityPath);
							if (textAsset != null && !list.Contains(textAsset))
								list.Add(textAsset);
						}
					}

					list.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
					return list.ToArray();
				}

				return null;
			}

			void LoadNewTemplate(TextAsset newTemplate = null)
			{
				// Fetch Shader Properties from previous Template
				List<ShaderProperty> oldShaderProperties = null;
				if (this.template != null)
				{
					oldShaderProperties = new List<ShaderProperty>(this.template.shaderProperties);
				}

				// Load new Template
				if (newTemplate != null)
				{
					currentConfig.ClearShaderProperties();
					this.template.SetTextAsset(newTemplate);
				}

				// Copy old Shader Properties to new Template, if any, to retain user changes even when loading a new template
				if (oldShaderProperties != null)
				{
					for (int i = 0; i < this.template.shaderProperties.Length; i++)
					{
						var match = oldShaderProperties.Find(oldSp => oldSp.Name == this.template.shaderProperties[i].Name);
						if (match != null)
						{
							// ----------------------------------------------------------------
							// Very specific cases for Albedo/Main Color because of URP new variable names...
							bool restorePropertyName = false;
							string newPropertyName = null;
							if (match.Name == "Albedo")
							{
								var defImp = match.implementations.Count >= 1 ? match.implementations[0] as ShaderProperty.Imp_MaterialProperty_Texture : null;
								if (defImp != null && defImp.PropertyName == "_MainTex" || defImp.PropertyName == "_BaseMap")
								{
									newPropertyName = (this.template.shaderProperties[i].implementations[0] as ShaderProperty.Imp_MaterialProperty).PropertyName;
									restorePropertyName = true;
								}
							}
							if (match.Name == "Main Color")
							{
								var defImp = match.implementations.Count >= 1 ? match.implementations[0] as ShaderProperty.Imp_MaterialProperty_Color : null;
								if (defImp != null && defImp.PropertyName == "_Color" || defImp.PropertyName == "_BaseColor")
								{
									newPropertyName = (this.template.shaderProperties[i].implementations[0] as ShaderProperty.Imp_MaterialProperty).PropertyName;
									restorePropertyName = true;
								}
							}

							// ----------------------------------------------------------------

							var originalImplementations = this.template.shaderProperties[i].implementations;
							this.template.shaderProperties[i].implementations = match.implementations;

							// Hook implementations shouldn't change
							for (int j = 0; j < this.template.shaderProperties[i].implementations.Count; j++)
							{
								if (this.template.shaderProperties[i].implementations[j] is ShaderProperty.Imp_Hook)
								{
									this.template.shaderProperties[i].implementations[j] = originalImplementations[j];
								}
							}

							if (restorePropertyName)
							{
								(this.template.shaderProperties[i].implementations[0] as ShaderProperty.Imp_MaterialProperty).PropertyName = newPropertyName;
							}

							this.template.shaderProperties[i].CheckHash();
							this.template.shaderProperties[i].CheckErrors();
						}
					}

					for (int i = 0; i < this.template.shaderProperties.Length; i++)
					{
						this.template.shaderProperties[i].ResolveShaderPropertyReferences();
					}
				}

				// Apply keywords, update Shader Properties
				if (currentConfig != null)
				{
					this.template.ApplyKeywords(currentConfig);
					currentConfig.UpdateShaderProperties(this.template);
				}

				currentHash = currentConfig.ToHash();
			}

			//--------------------------------------------------------------------------------------------------

			Shader currentShader;
			Config _currentConfig;
			Config currentConfig
			{
				get { return _currentConfig; }
				set { _currentConfig = value; CurrentConfig = _currentConfig; }
			}
			int currentHash;
			bool unsavedChanges;

			TextAsset[] allTemplates;

			int tabIndex;
			readonly Vector2[] scrollPositions = new Vector2[2];
			readonly Color unsavedChangesColor = new Color(1f, 1f, 0.7f);

			//--------------------------------------------------------------------------------------------------
			// Undo/Redo system
			//
			// We use a SerializedProperty for the `undoRedoAction` string to push changes to the undo stack.
			// The string holds the previous state of the current config, and restores it when undo/redo is
			// called.
			// We use a GUID to make sure that the undo/redo action is tied to the current SG2 session.

			const string undoRedoPrefix = "TCP2_SG2_UNDO_REDO ";
			string undoRedoGuid;    // GUID to identify the SG2 session for the undo/redo system
			SerializedProperty undoRedoProperty;
#pragma warning disable 0649 // 'never assigned to, and will always have its default value null'
			[SerializeField] string undoRedoAction;
#pragma warning restore 0649
			int lashUndoHash;
			bool ignoreUndoPushes;

			struct UndoRedoState
			{
				public string guid;
				public int tab;
				public string serializedConfig;
				public string uiFeaturesFoldouts;
				public string shaderPropertiesHeadersFoldouts;
				public string shaderPropertiesFoldouts;
				public int hash;

				public UndoRedoState(string guid, int tab, string serializedConfig, string uiFeaturesFoldouts, string shaderPropertiesHeadersFoldouts, string shaderPropertiesFoldouts)
				{
					this.guid = guid;
					this.tab = tab;
					this.serializedConfig = serializedConfig;
					this.uiFeaturesFoldouts = uiFeaturesFoldouts;
					this.shaderPropertiesHeadersFoldouts = shaderPropertiesHeadersFoldouts;
					this.shaderPropertiesFoldouts = shaderPropertiesFoldouts;
					this.hash = (guid + tab + serializedConfig + uiFeaturesFoldouts + shaderPropertiesHeadersFoldouts + shaderPropertiesFoldouts).GetHashCode();
				}

				public override string ToString()
				{
					return string.Format("[UndoRedoState {0} tab: {6}\nserializedConfig: {1}\nuiFeaturesFoldouts: {2}\n\nheadersFoldouts: {3}\nspFoldouts: {4}\nguid: {5}]", hash, serializedConfig, uiFeaturesFoldouts, shaderPropertiesHeadersFoldouts, shaderPropertiesFoldouts, guid, tab);
				}
			}

			static internal void PushUndoState()
			{
				if (instance != null)
				{
					instance.pushUndoState();
				}
			}

			void pushUndoState()
			{
				if (ignoreUndoPushes)
				{
					return;
				}

				// create new state
				string serializedConfig = Serialization.Serialize(currentConfig);
				string uiFeaturesFoldouts = getUIFeaturesFoldoutStates();
				string shaderPropertiesHeadersFoldouts = currentConfig.getHeadersExpanded();
				string shaderPropertiesFoldouts = currentConfig.getShaderPropertiesExpanded();
				var undoState = new UndoRedoState(undoRedoGuid, tabIndex, serializedConfig, uiFeaturesFoldouts, shaderPropertiesHeadersFoldouts, shaderPropertiesFoldouts);

				// only push state if there was a change with the last config state
				if (undoState.hash != lashUndoHash)
				{
					// push last state to undo/redo
					undoRedoProperty.stringValue = undoRedoPrefix + EditorJsonUtility.ToJson(undoState);
					undoRedoProperty.serializedObject.ApplyModifiedProperties();

					// set current state as previous
					lashUndoHash = undoState.hash;

					//Debug.Log("<color=#6080FF>Pushed new state</color>:\n" + undoState);
				}
				else
				{
					//Debug.Log("<color=#FF8060>Same state</color>:\n" + undoState);
				}
			}

			void OnUndoRedo()
			{
				// when processing an undo/redo action, deserialize the config state to find the new state
				if (undoRedoAction != null && undoRedoAction.StartsWith(undoRedoPrefix))
				{
					string json = undoRedoAction.Substring(undoRedoPrefix.Length);

					UndoRedoState state = new UndoRedoState();
					object boxedState = state;
					EditorJsonUtility.FromJsonOverwrite(json, boxedState);
					state = (UndoRedoState)boxedState;

					// same SG2 session?
					if (state.guid == undoRedoGuid)
					{
						ignoreUndoPushes = true;

						// tab
						tabIndex = state.tab;

						// parse config
						currentConfig.ParseSerializedData(state.serializedConfig, template, false, true);
						currentConfig.UpdateShaderProperties(template);
						template.ApplyKeywords(currentConfig);
						NeedsHashUpdate = true;
						this.Repaint();

						// parse foldout states
						setUIFeaturesFoldoutStates(state.uiFeaturesFoldouts);
						currentConfig.setHeadersExpanded(state.shaderPropertiesHeadersFoldouts);
						currentConfig.setShaderPropertiesExpanded(state.shaderPropertiesFoldouts);

						ignoreUndoPushes = false;
					}
				}
			}

			// Special case: the UI foldout states are not all serialized, so we need to gather and restore them for each undo/redo step
			string getUIFeaturesFoldoutStates()
			{
				string uiFeaturesFoldout = "";
				foreach (var uiFeature in template.uiFeatures)
				{
					var uiDropDownStart = uiFeature as UIFeature_DropDownStart;
					if (uiDropDownStart != null && uiDropDownStart.foldout)
					{
						uiFeaturesFoldout += uiDropDownStart.guiContent.text + ",";
					}
				}
				return uiFeaturesFoldout.TrimEnd(',');
			}

			void setUIFeaturesFoldoutStates(string foldouts)
			{
				var array = foldouts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var uiFeature in template.uiFeatures)
				{
					var uiDropDownStart = uiFeature as UIFeature_DropDownStart;
					if (uiDropDownStart != null)
					{
						uiDropDownStart.foldout = Array.Exists(array, str => str == uiDropDownStart.guiContent.text);
						uiDropDownStart.UpdatePersistentState();
					}
				}
			}

			//--------------------------------------------------------------------------------------------------

			void OnEnable()
			{
				instance = this;

				// undo/redo system
				Undo.undoRedoPerformed += OnUndoRedo;
				var so = new SerializedObject(this);
				undoRedoProperty = so.FindProperty("undoRedoAction");
				undoRedoGuid = Guid.NewGuid().ToString();

				ReorderableLayoutList.OnNeedRepaint += Repaint;

				ProjectOptions.LoadProjectOptions();
				GlobalOptions.LoadUserPrefs();
				this.wantsMouseMove = true; // needed for contextual help boxes, and dynamic tooltip
				shouldReloadUserShaders = true;
				NewShader();

				//Check unique variable names for shader properties names for Custom Material Properties & Shader Properties
				UniqueMaterialPropertyName.checkUniqueVariableName += CheckUniqueVariableName;

				// push initial state
				lashUndoHash = 0;
				pushUndoState();

				// initialize custom font to use Roboto
				if (!ProjectOptions.data.CustomFontInitialized)
				{
					ProjectOptions.data.CustomFontInitialized = true;
					var roboto = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath("8a406076b7de34849be1f35585875a66"));
					if (roboto != null)
					{
#if UNITY_2019_3_OR_NEWER
						ProjectOptions.data.UseCustomFont = true;
#endif
						ProjectOptions.data.CustomFont = roboto;
					}
				}
			}

			void OnDisable()
			{
				Undo.undoRedoPerformed -= OnUndoRedo;

				ReorderableLayoutList.OnNeedRepaint -= Repaint;
				Repaint();

				GlobalOptions.SaveUserPrefs();
				ProjectOptions.SaveProjectOptions();

				UniqueMaterialPropertyName.checkUniqueVariableName -= CheckUniqueVariableName;

				// clear delegates
				ShaderProperty.Imp_GenericFromTemplate.onGenericImplementationsChanged = null;
			}

			bool CheckUniqueVariableName(string name, IMaterialPropertyName materialPropertyName)
			{
				return currentConfig == null || currentConfig.IsUniquePropertyName(name, materialPropertyName);
			}

			virtual protected void OnGUI()
			{
				var font = GUI.skin.font;
				if (ProjectOptions.data.UseCustomFont && ProjectOptions.data.CustomFont != null)
				{
					GUI.skin.font = ProjectOptions.data.CustomFont;
				}

				OnGUI_Internal();

				GUI.skin.font = font;
			}

			void OnGUI_Internal()
			{
				TCP2_GUI.UseNewHelpIcon = true;

				var guiEnabled = GUI.enabled;
				var guiColor = GUI.color;

				EditorGUILayout.BeginHorizontal();
				TCP2_GUI.HeaderBig("Toony Colors Pro " + TCP2_VERSION + " - Shader Generator 2");
				var helpRect = GUILayoutUtility.GetRect(GUIContent.none, TCP2_GUI.HelpIcon);
				helpRect.y += 2;
				if (GUI.Button(helpRect, TCP2_GUI.TempContent("", "Open documentation"), TCP2_GUI.HelpIcon))
				{
					Application.OpenURL(DOCUMENTATION_URL);
				}
				EditorGUILayout.EndHorizontal();
				TCP2_GUI.Separator();

				var lW = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 105f;

				EditorGUILayout.BeginHorizontal();

				//Avoid refreshing Template meta at every Repaint
				if (template != null && !template.valid)
				{
					GUI.color *= new Color(1.0f, 0.6f, 0.6f);
				}
				var newTemplate = EditorGUILayout.ObjectField(TCP2_GUI.TempContent("Template:"), template.textAsset, typeof(TextAsset), false) as TextAsset;
				GUI.color = guiColor;
				if (newTemplate != template.textAsset)
				{
					LoadNewTemplate(newTemplate);
				}

				//Load template button
				if (GUILayout.Button(TCP2_GUI.TempContent("Load", TCP2_GUI.GetCustomTexture("TCP2_DropDownArrow")), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					ShowTemplatesMenu();
				}

				if (GUILayout.Button(TCP2_GUI.TempContent("Reload"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				{
					//Twice to prevent bug with used variable names
					LoadNewTemplate(template.textAsset);
					LoadNewTemplate(template.textAsset);
				}

				EditorGUILayout.EndHorizontal();

				//Template not found
				if (template == null || template.textAsset == null)
				{
					EditorGUILayout.HelpBox("Couldn't find template file!\n\nVerify that the file 'TCP2_ShaderTemplate_Default.txt' is in your project.\nPlease reimport the pack if you can't find it!", MessageType.Error);
					return;
				}

				//Template Info
				if (!string.IsNullOrEmpty(template.templateInfo))
				{
					TCP2_GUI.HelpBoxLayout(template.templateInfo, MessageType.Info);
				}

				//Template Warning
				if (!string.IsNullOrEmpty(template.templateWarning))
				{
					TCP2_GUI.HelpBoxLayout(template.templateWarning, MessageType.Warning);
				}

				TCP2_GUI.Separator();

				//If current shader is unsaved, show yellow color
				GUI.color = unsavedChanges ? guiColor * unsavedChangesColor : GUI.color;

				//Current Shader object field
				EditorGUI.BeginChangeCheck();
				var newShader = EditorGUILayout.ObjectField("Current Shader:", currentShader, typeof(Shader), false) as Shader;
				if (EditorGUI.EndChangeCheck() && newShader != null)
				{
					LoadCurrentConfigFromShader(newShader);
				}

				//Copy/Load/New buttons
				EditorGUILayout.BeginHorizontal();
				{
					//Small yellow label if unsaved changes
					if (unsavedChanges)
					{
						GUILayout.Space(EditorGUIUtility.labelWidth + 4);
						var guiContent = TCP2_GUI.TempContent("Unsaved changes");
						var rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.helpBox, GUILayout.Height(EditorGUIUtility.singleLineHeight));
#if !UNITY_2019_3_OR_NEWER
						rect.y -= 1;
#endif
						GUI.Label(rect, guiContent, EditorStyles.helpBox);
					}
					else
					{
						GUILayout.FlexibleSpace();
					}

					using (new EditorGUI.DisabledScope(currentShader == null))
					{
						if (GUILayout.Button(TCP2_GUI.TempContent("Copy"), EditorStyles.miniButtonLeft, GUILayout.Width(60f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
						{
							CopyShader();
						}
					}
					if (GUILayout.Button(TCP2_GUI.TempContent("Load", TCP2_GUI.GetCustomTexture("TCP2_DropDownArrow")), EditorStyles.miniButtonMid, GUILayout.Width(60f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
					{
						ShowShadersMenu();
					}
					if (GUILayout.Button("New", EditorStyles.miniButtonRight, GUILayout.Width(60f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
					{
						NewShader();
					}

					GUILayout.Space(18);    //leave space to align with the Object Field box
				}
				EditorGUILayout.EndHorizontal();
				GUI.color = guiColor;

				if (currentConfig == null)
				{
					NewShader();
				}

				if (currentConfig.isModifiedExternally)
				{
					TCP2_GUI.HelpBoxLayout("It looks like this shader has been modified externally or manually.\nUpdating it <b>will overwrite</b> the changes!", MessageType.Warning);
				}

				EditorGUIUtility.labelWidth = lW;

				// Shader name
				TCP2_GUI.Separator();
				GUI.enabled = (currentShader == null);
				EditorGUI.BeginChangeCheck();
				currentConfig.ShaderName = EditorGUILayout.TextField(TCP2_GUI.TempContent("Shader Name", "Path will indicate how to find the Shader in Unity's drop-down list"), currentConfig.ShaderName);
				currentConfig.ShaderName = Regex.Replace(currentConfig.ShaderName, @"[^a-zA-Z0-9 _!/]", "");
				if (EditorGUI.EndChangeCheck() && ProjectOptions.data.AutoNames)
				{
					currentConfig.AutoNames();
				}

				// Filename
				GUI.enabled &= !ProjectOptions.data.AutoNames;
				{
					EditorGUILayout.BeginHorizontal();
					currentConfig.Filename = EditorGUILayout.TextField(TCP2_GUI.TempContent("Filename", "The filename for the generated shader." + (ProjectOptions.data.AutoNames ? "" : "\nYou can input your own by disabling the auto-filename option in the options below.")), currentConfig.Filename);
					currentConfig.Filename = Regex.Replace(currentConfig.Filename, @"[^a-zA-Z0-9 _!/]", "");
					GUILayout.Label(".shader", GUILayout.Width(50f));
					EditorGUILayout.EndHorizontal();
				}
				GUI.enabled = guiEnabled;

				// Directory
				EditorGUILayout.BeginHorizontal();
				{
					if (currentShader == null)
					{
						ProjectOptions.data.CustomOutputPath = EditorGUILayout.TextField(TCP2_GUI.TempContent("Output Directory", "The output directory for the generated shader."), ProjectOptions.data.CustomOutputPath);
						if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
						{
							var newOutputPath = Utils.OpenFolderPanel_ProjectPath("Choose custom output directory for TCP2 generated shaders", ProjectOptions.data.CustomOutputPath);
							if (!string.IsNullOrEmpty(newOutputPath))
							{
								ProjectOptions.data.CustomOutputPath = newOutputPath;
							}
						}
					}
					else
					{
						using (new EditorGUI.DisabledScope(true))
						{
							EditorGUILayout.TextField(TCP2_GUI.TempContent("Output Directory", "The output directory for the generated shader."), "-");
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				TCP2_GUI.Separator();

				if (!template.valid)
				{
					EditorGUILayout.HelpBox("Invalid template: it might be an incorrect file, or one for the old Shader Generator.", MessageType.Error);
					GUILayout.FlexibleSpace();
				}
				else
				{

					//########################################################################################################
					// Tabs: Features, Properties/Custom Material Properties

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Toggle(tabIndex == 0, TCP2_GUI.TempContent("FEATURES"), TCP2_GUI.Tab))
						tabIndex = 0;
					if (GUILayout.Toggle(tabIndex == 1, TCP2_GUI.TempContent("SHADER PROPERTIES"), TCP2_GUI.Tab))
						tabIndex = 1;
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					TCP2_GUI.SeparatorSimple();

					var changed = false;

					//########################################################################################################
					// FEATURES

					if (tabIndex == 0)
					{
						ShaderGenerator2.ContextualHelpBox(
							"Select the features you want for your shader here.\nLeave the cursor over a label to get a help tooltip, or click here to open the documentation to see details and screenshots.",
							"featuresreference");

						scrollPositions[tabIndex] = EditorGUILayout.BeginScrollView(scrollPositions[tabIndex]);

						EditorGUI.BeginChangeCheck();

						//New UI embedded into Template
						template.FeaturesGUI(currentConfig);

						if (EditorGUI.EndChangeCheck())
						{
							//reload shader properties
							currentConfig.UpdateShaderProperties(template);
							changed = true;
						}

						EditorGUILayout.EndScrollView();
					}

					//########################################################################################################
					// SHADER PROPERTIES/CUSTOM MATERIAL PROPERTIES

					else if (tabIndex == 1)
					{
						scrollPositions[tabIndex] = EditorGUILayout.BeginScrollView(scrollPositions[tabIndex]);

						currentConfig.ShaderPropertiesGUI();

						if (NeedsShaderPropertiesUpdate)
						{
							//reload shader properties if needed
							NeedsShaderPropertiesUpdate = false;
							currentConfig.UpdateShaderProperties(template);
							changed = true;

							// update Custom Material Properties
							currentConfig.UpdateCustomMaterialProperties();
						}

						//changed |= EditorGUI.EndChangeCheck();

						EditorGUILayout.EndScrollView();
					}

					TCP2_GUI.SeparatorSimple();

					//########################################################################################################
					//GENERATE

					GUI.enabled = guiEnabled;

					EditorGUILayout.BeginHorizontal();
					bool configHasErrors = currentConfig == null || currentConfig.HasErrors();
					if (configHasErrors)
					{
						var gc = TCP2_GUI.TempContent("There are errors in the Shader Properties");
						var rect = GUILayoutUtility.GetRect(gc, EditorStyles.helpBox, GUILayout.Height(38f));
						rect.y -= 1;
						EditorGUI.HelpBox(rect, gc.text, MessageType.Error);
					}
					else
						GUILayout.FlexibleSpace();

					GUI.color = unsavedChanges ? guiColor * unsavedChangesColor : GUI.color;
					using (new EditorGUI.DisabledScope(configHasErrors))
					{
						if (GUILayout.Button(currentShader == null ? "Generate Shader" : "Update Shader", GUILayout.Width(120f), GUILayout.Height(38f)))
						{
							if (template == null)
							{
								EditorUtility.DisplayDialog("TCP2 : Shader Generation", "Can't generate shader: no Template file defined!\n\nYou most likely want to link the TCP2_User.txt file to the Template field in the Shader Generator.", "Ok");
								return;
							}

							currentConfig.templateFile = template.textAsset.name;
							currentConfig.OnBeforeGenerateShader();
							_GlobalUniqueId = 100;

							Shader generatedShader = null;
							try
							{
								generatedShader = Compile(currentConfig, currentShader, template, true, !ProjectOptions.data.OverwriteConfig);
							}
							catch (Exception e)
							{
								Debug.LogError(ErrorMsg("Error generating the shader:\n" + e));
							}
							finally
							{
								EditorUtility.ClearProgressBar();
							}

							if (generatedShader != null)
							{
								unsavedChanges = false;
								LoadCurrentConfigFromShader(generatedShader);
							}

							shouldReloadUserShaders = true;
							currentConfig.OnAfterGenerateShader();

							//Workaround to force the inspector to refresh, so that state is reset.
							//Needed in case of switching between specular/metallic and related
							//options, while the inspector is opened, so that it shows/hides the
							//relevant properties according to the changes.
							TCP2_MaterialInspector_SurfacePBS_SG.InspectorNeedsUpdate = true;
						}
					}
					GUI.color = guiColor;
					EditorGUILayout.EndHorizontal();

					//Update config hash
					if (changed)
					{
						NeedsHashUpdate = true;
					}

					if (NeedsHashUpdate && Event.current.type == EventType.Repaint)
					{
						//check for errors
						foreach (var sp in currentConfig.VisibleShaderProperties)
						{
							sp.CheckErrors();
						}

						//update hash
						var newHash = currentConfig.ToHash();
						unsavedChanges = (newHash != currentHash);
						NeedsHashUpdate = false;
						this.Repaint();
					}
				}

				TCP2_GUI.Separator();

				//########################################################################################################
				// OPTIONS

				GlobalOptions.data.ShowOptions = TCP2_GUI.HeaderFoldout(GlobalOptions.data.ShowOptions, TCP2_GUI.TempContent("OPTIONS"), true);

				if (GlobalOptions.data.ShowOptions)
				{
					GlobalOptions.data.SelectGeneratedShader = GUILayout.Toggle(GlobalOptions.data.SelectGeneratedShader, TCP2_GUI.TempContent("Select Generated Shader", "Will select the generated file in the Project view"), GUILayout.Width(180f));
					ProjectOptions.data.OverwriteConfig = GUILayout.Toggle(ProjectOptions.data.OverwriteConfig, TCP2_GUI.TempContent("Always overwrite shaders", "Overwrite shaders when generating/updating (no prompt)"), GUILayout.Width(180f));
					EditorGUI.BeginChangeCheck();
					ProjectOptions.data.LoadAllShaders = GUILayout.Toggle(ProjectOptions.data.LoadAllShaders, TCP2_GUI.TempContent("Reload Shaders from all Project", "Load shaders from all your Project folders instead of just Toony Colors Pro 2.\nEnable it if you move your generated shader files outside of the default TCP2 Generated folder."), GUILayout.ExpandWidth(false));
					if (EditorGUI.EndChangeCheck())
					{
						shouldReloadUserShaders = true;
					}

					GUILayout.Space(4);
					TCP2_GUI.SeparatorSimple();

					EditorGUI.BeginChangeCheck();

					// Auto filename
					ProjectOptions.data.AutoNames = GUILayout.Toggle(ProjectOptions.data.AutoNames, TCP2_GUI.TempContent("Automatic filename", "Will automatically generate the shader filename based on its UI name"), GUILayout.ExpandWidth(false));

					// Auto sub-folders
					GUI.enabled &= ProjectOptions.data.AutoNames;
					{
						ProjectOptions.data.SubFolders = GUILayout.Toggle(ProjectOptions.data.SubFolders, TCP2_GUI.TempContent("Automatic sub-directories", "Will automatically create sub-directories based on the shader's UI categories"), GUILayout.ExpandWidth(false));
					}
					GUI.enabled = guiEnabled;

					if (EditorGUI.EndChangeCheck())
					{
						currentConfig.AutoNames();
					}

					GUILayout.Space(4);
					TCP2_GUI.SeparatorSimple();

					GlobalOptions.data.ShowDisabledFeatures = GUILayout.Toggle(GlobalOptions.data.ShowDisabledFeatures, TCP2_GUI.TempContent("Show disabled fields", "Show all settings, including disabled ones. Allows you to view all options available."), GUILayout.ExpandWidth(false));

					EditorGUI.BeginChangeCheck();
					GlobalOptions.data.ShowContextualHelp = GUILayout.Toggle(GlobalOptions.data.ShowContextualHelp, TCP2_GUI.TempContent("Show contextual help", "Will show help boxes throughout the UI regarding the usage of the Shader Generator"), GUILayout.Width(180f));
					if (EditorGUI.EndChangeCheck())
					{
						this.wantsMouseMove = GlobalOptions.data.ShowContextualHelp;
					}

					GlobalOptions.data.DockableWindow = GUILayout.Toggle(GlobalOptions.data.DockableWindow, TCP2_GUI.TempContent("Dockable Window", "Makes the Shader Generator 2 window dockable in the Editor UI (close and reopen the tool to apply)"), GUILayout.ExpandWidth(false));

					EditorGUILayout.BeginHorizontal();
					{
						ProjectOptions.data.UseCustomFont = GUILayout.Toggle(ProjectOptions.data.UseCustomFont, TCP2_GUI.TempContent("Use Custom Font", "Use a custom font for the Shader Geneator 2"), GUILayout.ExpandWidth(false));
						GUILayout.Space(10f);
						EditorGUI.BeginDisabledGroup(!ProjectOptions.data.UseCustomFont);
						{
							ProjectOptions.data.CustomFont = (Font)EditorGUILayout.ObjectField(ProjectOptions.data.CustomFont, typeof(Font), false);
						}
						EditorGUI.EndDisabledGroup();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(4f);
				}

				TCP2_GUI.UseNewHelpIcon = false;

				//needed for hover to work correctly
				if (Event.current.type == EventType.MouseMove)
				{
					this.Repaint();
				}

				if (showDynamicTooltip)
				{
					var realMousePosition = Event.current.mousePosition + new Vector2(this.position.x, this.position.y) + new Vector2(10, 14);
					Tooltip.Show(realMousePosition, dynamicTooltip);

					/*
					var gc = TCP2_GUI.TempContent(helpMessage);
					float width = Mathf.Min(EditorGUIUtility.currentViewWidth - Event.current.mousePosition.x - 10, 200);
					float height = EditorStyles.helpBox.CalcHeight(gc, width);
					var rectHelp = new Rect(Event.current.mousePosition.x + 10, Event.current.mousePosition.y + 10, width, height);
					GUI.Label(rectHelp, gc, EditorStyles.helpBox);
					*/

					showDynamicTooltip = false;
				}
				else if (Event.current.type != EventType.Layout)
				{
					Tooltip.Hide();
				}

				if (GUI.changed)
				{
					pushUndoState();
				}
			}

			public delegate void OnProjectChangeCallback();
			static public OnProjectChangeCallback onProjectChange;

			void OnProjectChange()
			{
				shouldReloadUserShaders = true;
				if (onProjectChange != null)
				{
					onProjectChange();
				}
			}

			public void GenerateOrUpdateShader()
			{
				currentConfig.templateFile = template.textAsset.name;
				currentConfig.OnBeforeGenerateShader();

				Shader generatedShader = null;
				try
				{
					generatedShader = Compile(currentConfig, currentShader, template, true, !ProjectOptions.data.OverwriteConfig);
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}

				if (generatedShader != null)
				{
					unsavedChanges = false;
					LoadCurrentConfigFromShader(generatedShader);
				}

				shouldReloadUserShaders = true;
				currentConfig.OnAfterGenerateShader();
			}

			//--------------------------------------------------------------------------------------------------
			// MISC

			void LoadConfig(Config config, bool loadConfigTemplate = true)
			{
				currentConfig = config;
				unsavedChanges = false;
				if (ProjectOptions.data.AutoNames)
				{
					currentConfig.AutoNames();
				}
				if (loadConfigTemplate)
				{
					template.TryLoadTextAsset(currentConfig);
					LoadNewTemplate();
				}

				if (template.valid)
				{
					currentConfig.UpdateShaderProperties(template);

					//apply default features/keywords according to template
					template.ApplyForcedValues(currentConfig);
					template.ApplyKeywords(currentConfig);
				}

				currentHash = currentConfig.ToHash();
			}

			void NewShader()
			{
				currentShader = null;
				template.ResetShaderProperties();
				LoadConfig(new Config(), false);
			}

			void CopyShader()
			{
				currentShader = null;
				var oldConfig = currentConfig;
				var newConfig = currentConfig.Copy();
				newConfig.ShaderName += " Copy";
				newConfig.Filename += " Copy";
				LoadConfig(newConfig);
				oldConfig.CopyCustomTexturesTo(newConfig);
				oldConfig.CopyImplementationsTo(newConfig);
			}

			public void LoadCurrentConfigFromShader(Shader shader)
			{
				var newConfig = Config.CreateFromShader(shader);
				if (newConfig != null)
				{
					currentConfig = newConfig;
					currentShader = shader;
					unsavedChanges = false;

					//Load appropriate template
					template.TryLoadTextAsset(currentConfig);
					LoadNewTemplate();
					currentConfig.ParseSerializedDataAndHash(ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter, template, true);   //second run (see method comment)
					currentConfig.UpdateShaderProperties(template);
					template.ApplyKeywords(currentConfig);

					currentHash = currentConfig.ToHash();
				}
				else
				{
					EditorApplication.Beep();
					ShowNotification(TCP2_GUI.TempContent("Invalid shader loaded: it doesn't seem to have been generated by Toony Colors Pro 2's Shader Generator 2!"));
				}
			}

			bool shouldReloadUserShaders;
			Shader[] projectShaders;
			void ShowShadersMenu()
			{
				// load shaders from the project
				if (projectShaders == null || shouldReloadUserShaders)
				{
					projectShaders = GetUserShaders();
				}

				// build shader menu
				var loadShadersMenu = new GenericMenu();
				loadShadersMenu.AddItem(new GUIContent("New Shader"), false, NewShader);
				loadShadersMenu.AddSeparator("");

				GenericMenu.MenuFunction2 onLoadShaderFromMenu = shaderObj =>
				{
					var shader = shaderObj as Shader;
					if (shader != null)
						LoadCurrentConfigFromShader(shader);
				};

				if (projectShaders != null && projectShaders.Length > 0)
				{
					for (var i = 0; i < projectShaders.Length; i++)
					{
						if (projectShaders[i] != null)
						{
							loadShadersMenu.AddItem(new GUIContent(projectShaders[i].name), currentShader == projectShaders[i], onLoadShaderFromMenu, projectShaders[i]);
						}
					}
				}
				else
				{
					loadShadersMenu.AddDisabledItem(new GUIContent("No shaders made with the Shader Generator 2 found"));
				}

				shouldReloadUserShaders = false;

				loadShadersMenu.ShowAsContext();
			}

			static Shader[] GetUserShaders()
			{
				Shader[] returnArray = null;
				EditorUtility.DisplayProgressBar("Toony Colors Pro", "Fetching shaders...", 0.5f);

				try
				{
					var rootPath = (ProjectOptions.data.LoadAllShaders ? Application.dataPath : GetOutputPath());
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
								// Shader Generator 1 shaders: skip
								if (shaderImporter.userData.Contains("USER"))
								{
									continue;
								}
								else
								{
									string osPath = Utils.UnityRelativeToSystemPath(shaderImporter.assetPath);
									var shaderContent = File.ReadAllLines(osPath);

									// only check the last 10 lines
									for (int i = 1; i <= 10; i++)
									{
										int index = shaderContent.Length - i;
										if (index >= shaderContent.Length || index < 0)
										{
											break;
										}

										// Shader Generator 2 shaders have a hash at the end of the file
										// If it exists, it means this is an SG2 shader.
										string line = shaderContent[shaderContent.Length - i];
										if (line.StartsWith(Config.kHashPrefix))
										{
											var shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
											if (shader != null && !shaderList.Contains(shader))
											{
												shaderList.Add(shader);
											}
										}
									}
								}
							}
						}

						returnArray = shaderList.ToArray();
					}
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}

				return returnArray;
			}

			void ShowTemplatesMenu()
			{
				if (allTemplates == null)
				{
					allTemplates = LoadAllTemplates();
				}

				// construct menu
				var loadTemplateMenu = new GenericMenu();
				if (allTemplates != null && allTemplates.Length > 0)
				{
					var menuItems = new List<object[]>();

					foreach (var textAsset in allTemplates)
					{
						//Exceptions
						if (textAsset.name.Contains("TCP2_User_Unity5_Old"))
							continue;

						//Find name and SG2 compatibility
						var name = textAsset.name;
						bool validTemplate = false;
						var sr = new StringReader(textAsset.text);
						string line;
						while ((line = sr.ReadLine()) != null)
						{
							if (line.StartsWith("#NAME"))
							{
								name = line.Substring(6);
							}
							if (line.StartsWith("#SG2"))
							{
								validTemplate = true;
							}
							if (line.StartsWith("#FEATURES"))
							{
								break;
							}
						}

						if (validTemplate)
						{
							menuItems.Add(new object[] { name, textAsset });
						}
					}

					//put submenus at the end of list
					var menuItemsSorted = new List<object[]>();
					for (var i = menuItems.Count-1; i >= 0; i--)
					{
						if ((menuItems[i][0] as string).Contains("/"))
							menuItemsSorted.Add(menuItems[i]);
						else
							menuItemsSorted.Insert(0, menuItems[i]);
					}

					//create load templates menu
					GenericMenu.MenuFunction2 onLoadTemplate = textAssetObj => { LoadNewTemplate(textAssetObj as TextAsset); };
					foreach (var item in menuItemsSorted)
					{
						var textAsset = item[1] as TextAsset;
						loadTemplateMenu.AddItem(new GUIContent(item[0] as string), template.textAsset == textAsset, onLoadTemplate, textAsset);
					}
				}
				else
				{
					loadTemplateMenu.AddDisabledItem(new GUIContent("Couldn't load templates"));
				}

				loadTemplateMenu.ShowAsContext();
			}

			//================================================================================================================================================================
			// COMPILATION
			// Code from ShaderGeneratorUtils

			/// <summary>
			/// Returns the absolute output path
			/// </summary>
			static string GetOutputPath()
			{
				// if (ProjectOptions.data.CustomOutputDir)
				{
					var trimmed = ProjectOptions.data.CustomOutputPath.Trim('/', ' ');
					return Application.dataPath + (string.IsNullOrEmpty(trimmed) ? "/" : string.Format("/{0}/", trimmed));
				}

				//TCP2 folder has been moved? Try to find new location
				/*
				if (!Directory.Exists(Application.dataPath + OUTPUT_PATH))
				{
					var rootPath = Utils.FindReadmePath(false);
					if (!string.IsNullOrEmpty(rootPath))
					{
						return rootPath + "/Shaders Generated/";
					}
				}

				return Application.dataPath + OUTPUT_PATH;
				*/
			}

			static string SafePath(string path)
			{
				if (!path.EndsWith("/"))
					path = path + "/";
				if (!path.StartsWith("/"))
					path = "/" + path;

				//try to get safe path name
				foreach (var c in Path.GetInvalidPathChars())
				{
					path = path.Replace(c.ToString(), "");
				}
				foreach (var c in Path.GetInvalidFileNameChars())
				{
					if (c == '/' || c == '\\')
						continue;
					path = path.Replace(c.ToString(), "");
				}

				return path;
			}

			static Shader Compile(Config config, Shader existingShader, Template template, bool showProgressBar = true, bool overwritePrompt = true, bool externallyModifiedPrompt = true)
			{
				return Compile(config, existingShader, template, showProgressBar ? 0f : -1f, overwritePrompt, externallyModifiedPrompt);
			}
			static Shader Compile(Config config, Shader existingShader, Template template, float progress, bool overwritePrompt, bool externallyModifiedPrompt)
			{
				//UI
				if (progress >= 0f)
					EditorUtility.DisplayProgressBar("Hold On", "Generating Shader: " + config.ShaderName, progress);

				// Set up statics
				ShaderGenerator2.CurrentConfig = config;
				ShaderGenerator2.TemplateID = template.id;

				//Generate source
				var source = GenerateShaderSource(config, template, existingShader);
				if (string.IsNullOrEmpty(source))
				{
					Debug.LogError(ErrorMsg("Can't save Shader: source is null or empty!"));
					EditorUtility.ClearProgressBar();
					return null;
				}

				//Save to disk
				var shader = SaveShader(config, existingShader, source, overwritePrompt, externallyModifiedPrompt && config.isModifiedExternally);

				//Special configs
				if (template.templateType == "terrain")
				{
					//Generate Base shader
					var baseConfig = config.Copy();
					baseConfig.Filename = baseConfig.Filename + "_Base";
					baseConfig.ShaderName = "Hidden/" + baseConfig.ShaderName + "-Base";
					baseConfig.Features.Add("TERRAIN_BASE");

					source = GenerateShaderSource(baseConfig, template, existingShader);
					if (string.IsNullOrEmpty(source))
						Debug.LogError(ErrorMsg("Can't save Terrain Base Shader: source is null or empty!"));
					else
						SaveShader(baseConfig, existingShader, source, false, false);

					//Generate AddPass shader
					var addPassConfig = config.Copy();
					addPassConfig.Filename = addPassConfig.Filename + "_AddPass";
					addPassConfig.ShaderName = "Hidden/" + addPassConfig.ShaderName + "-AddPass";
					addPassConfig.Features.Add("TERRAIN_ADDPASS");
					addPassConfig.Flags.Add("decal:add");

					source = GenerateShaderSource(addPassConfig, template, existingShader);
					if (string.IsNullOrEmpty(source))
						Debug.LogError(ErrorMsg("Can't save Terrain AddPass Shader: source is null or empty!"));
					else
						SaveShader(addPassConfig, existingShader, source, false, false);
				}

				//UI
				if (progress >= 0f)
					EditorUtility.ClearProgressBar();

				return shader;
			}

			struct CustomMaterialPropertyUsage
			{
				internal ShaderProperty.CustomMaterialProperty customMaterialProperty;
				internal ShaderProperty.ProgramType program;
			}

			class NotEmptyBlock
			{
				public StringBuilder stringBuilderToPrintBefore = new StringBuilder();
				public StringBuilder stringBuilderNotEmptyBlock = new StringBuilder();
				public StringBuilder stringBuilderToPrintAfter = new StringBuilder();

				public NotEmptyBlock()
				{
					stringBuilderToPrintBefore = new StringBuilder();
					stringBuilderNotEmptyBlock = new StringBuilder();
					stringBuilderToPrintAfter = new StringBuilder();
				}

				public bool ShouldBePrinted()
				{
					bool empty = stringBuilderNotEmptyBlock.Length == 0;
					if (empty)
					{
						return false;
					}

					for (int c = 0; c < stringBuilderNotEmptyBlock.Length; c++)
					{
						if (!char.IsWhiteSpace(stringBuilderNotEmptyBlock[c]))
						{
							return true;
						}
					}

					return false;
				}

				public string Print()
				{
					return stringBuilderToPrintBefore.ToString()
						+ stringBuilderNotEmptyBlock.ToString()
						+ stringBuilderToPrintAfter.ToString();
				}
			}

			// System to add lines before the currently examined line (e.g. for custom code to cache sampled implementations)
			static StringBuilder currentStringBuilder;
			static string currentIndent;
			internal static void AppendLineBefore(string line)
			{
				currentStringBuilder.AppendLine(currentIndent + line);
			}

			//Generate the source code for the shader as a string
			//Very long method that will do the following steps:
			// 1. Add specific keywords (shader name, unity version)
			// 2. Get stripped template lines based on conditions
			// 3. Find used ShadeProperties for each pass
			// 4. Generate the output code
			static string GenerateShaderSource(Config config, Template template, Shader existingShader = null)
			{
				if (config == null)
				{
					Debug.LogError(ErrorMsg("Config file is null"));
					return null;
				}

				if (template == null)
				{
					Debug.LogError(ErrorMsg("Template is null"));
					return null;
				}

				if (template.textAsset == null || string.IsNullOrEmpty(template.textAsset.text))
				{
					Debug.LogError(ErrorMsg("Template string is null or empty"));
					return null;
				}

				//------------------------------------------------
				// UPDATE CONFIG
				// Make sure that all needed Shader Properties exist
				// (sometimes some can be missing if the Shader Properties tab isn't viewed and some features are toggled)
				config.UpdateShaderProperties(template);

				//------------------------------------------------
				// SHADER PARAMETERS

				var keywords = new Dictionary<string, string>(config.Keywords);

				//Shader name
				keywords.Add("SHADER_NAME", config.ShaderName);

				//------------------------------------------------
				// PARSING & GENERATION

				var stringBuilder = new StringBuilder();
				currentStringBuilder = stringBuilder;
				var stackNotEmptyBlocks = new Stack<NotEmptyBlock>();

				//Get the template lines without the ones not matching selected features (and without unneeded blocks at this point, like #FEATURES)
				var flags = new List<string>(config.Flags);
				var extraFlags = new Dictionary<string, List<string>>();
				// we need a deep copy of the dictionary:
				foreach (var kvp in config.FlagsExtra)
				{
					extraFlags.Add(kvp.Key, new List<string>(kvp.Value));
				}

				var templateLines = template.GetParsedLinesFromConditions(config, flags, extraFlags);

				// Generate flag parameters (has to happen after #KEYWORD block in GetParsedLinesFromCondition)

				// surface shader flags
				var strFlags = string.Join(" ", flags.ToArray());
				keywords.Add("FLAGS:pragma_surface_shader", strFlags);

				// extra flags
				foreach (var kvp in extraFlags)
				{
					if (kvp.Value.Count > 0)
					{
						keywords.Add("FLAGS:" + kvp.Key, string.Join(" ", kvp.Value.ToArray()));
					}
				}

				//--------------------------------
				// GLOBAL Params

				//Keep track of manually printed Shader Properties ( e.g. [[PROP:Albedo]] ), so that we can print the remaining ones with the [[PROPERTIES]] block
				var printedShaderProperties = new HashSet<ShaderProperty>();
				var allPassesUsedShaderProperties = new List<ShaderProperty>();
				var allPassesUsedCustomMaterialProperty = new List<ShaderProperty.CustomMaterialProperty>();
				CurrentProgram = ShaderProperty.ProgramType.Undefined;

				//--------------------------------
				// PASS Params

				List<CustomMaterialPropertyUsage> currentPassUsedCustomMaterialProperties = null;
				List<ShaderProperty> currentPassUsedShaderProperties = null;
				VertexToFragmentVariablesManager variablesManager = null;
				List<int> usedUvChannelsVertex = null;
				List<int> usedUvChannelsFragment = null;
				Dictionary<int, int> uvChannelsDimensions = null; // dimensions (float2, float3, float4) needed for each uv channel in the fragment shader
				Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>> uvChannelGlobalTilingOffset = null;
				Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>> uvChannelGlobalScrolling = null;
				Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>> uvChannelGlobalRandomOffset = null;
				string inputSource = "no_input";
				string outputSource = "no_output";
				bool newPass = false;
				bool isSurfacePass = false;

				// Add a used UV channel and specify its dimensions for the fragment shader
				Action<List<int>, int, int> AddUvChannelUsage = (List<int> uvList, int uvChannel, int dimensions) =>
				{
					if (!uvList.Contains(uvChannel))
					{
						uvList.Add(uvChannel);
					}

					if (!uvChannelsDimensions.ContainsKey(uvChannel))
					{
						uvChannelsDimensions.Add(uvChannel, dimensions);
					}
					else if (uvChannelsDimensions[uvChannel] < dimensions)
					{
						uvChannelsDimensions[uvChannel] = dimensions;
					}
				};

				//Used Shader Properties per pass
				var usedShaderPropertiesPerPass = template.FindUsedShaderPropertiesPerPass(templateLines);
				var usedCustomMaterialProperties = new List<List<CustomMaterialPropertyUsage>>();

				//Find used Custom Material Properties per pass and globally
				for (var i = 0; i < usedShaderPropertiesPerPass.Count; i++)
				{
					usedCustomMaterialProperties.Add(new List<CustomMaterialPropertyUsage>());

					foreach (var sp in usedShaderPropertiesPerPass[i])
					{
						//All used Shader Properties so that we can print them in [[PROPERTIES]]
						if (!allPassesUsedShaderProperties.Contains(sp))
							allPassesUsedShaderProperties.Add(sp);

						//All used Custom Material Properties so that we can print them in [[PROPERTIES]] + get the ones used per pass
						foreach (var imp in sp.implementations)
						{
							var ctImp = imp as ShaderProperty.Imp_CustomMaterialProperty;
							if (ctImp != null)
							{
								var cmp = ctImp.LinkedCustomMaterialProperty;

								if (cmp == null)
								{
									Debug.LogError(ErrorMsg(string.Format("No Custom Material Property defined for property '{0}'", sp.Name)));
									return null;
								}

								if (!allPassesUsedCustomMaterialProperty.Contains(cmp))
								{
									allPassesUsedCustomMaterialProperty.Add(cmp);
								}

								// add to used Custom Material Properties per pass if not added already, or if the program is different
								// (a custom material property can be sampled in both vertex and fragment shader)
								bool alreadyAdded = usedCustomMaterialProperties[i].Exists(ctUsage => ctUsage.program == sp.Program && ctUsage.customMaterialProperty == cmp);
								if (!alreadyAdded)
								{
									usedCustomMaterialProperties[i].Add(new CustomMaterialPropertyUsage() { customMaterialProperty = cmp, program = sp.Program });
								}
							}
						}
					}
				}

				// System to mark some shader properties/custom material properties as already declared in
				// a CG/HLSLINCLUDE block, and thus they shouldn't be declared again anywhere else.
				bool isInIncludeBlock = false;
				bool isOutsideCBuffer = false;
				var cgIncludeShaderProperties = new HashSet<ShaderProperty>();
				var cgIncludeCustomMaterialProperties = new HashSet<ShaderProperty.CustomMaterialProperty>();

				//Parse template file and output generated shader
				var passIndex = -1;
				for (var i = 0; i < templateLines.Length; i++)
				{
					var line = templateLines[i].line;
					var skipLine = false;

					//extract indentation
					var indent = "";
					foreach (var c in line)
					{
						if (char.IsWhiteSpace(c))
							indent += c;
						else
							break;
					}
					currentIndent = indent;

					//Comment or special commands
					if (line.StartsWith("#"))
					{
						//Skip these blocks
						if (line.StartsWith("#FEATURES") || line.StartsWith("#PROPERTIES_NEW") || line.StartsWith("#KEYWORDS") || line.StartsWith("#INPUT_VARIABLES"))
						{
							while (i < templateLines.Length)
							{
								i++;
								if (templateLines[i].line.Trim() == "#END")
									break;
							}

							continue;
						}

						//Special tags
						var tags = line.Substring(1).Split(',');
						foreach (var tag in tags)
						{
							var trimmedTag = tag.Trim().Replace(" ", "");

							if (trimmedTag == "VERTEX")
							{
								CurrentProgram = ShaderProperty.ProgramType.Vertex;
								IsInLightingFunction = false;
								continue;
							}

							if (trimmedTag == "FRAGMENT")
							{
								CurrentProgram = ShaderProperty.ProgramType.Fragment;
								IsInLightingFunction = false;
								continue;
							}

							if (trimmedTag == "SURFACE")
							{
								CurrentProgram = ShaderProperty.ProgramType.Fragment;
								IsInLightingFunction = false;
								continue;
							}

							if (trimmedTag == "LIGHTING")
							{
								CurrentProgram = ShaderProperty.ProgramType.Fragment;
								IsInLightingFunction = true;
								continue;
							}

							if (trimmedTag == "PASS")
							{
								newPass = true;
								passIndex++;
								continue;
							}

							if (trimmedTag.StartsWith("INPUT"))
							{
								//remove white spaces and "#INPUT="
								inputSource = trimmedTag.Substring("INPUT=".Length);
								continue;
							}

							if (trimmedTag.StartsWith("OUTPUT"))
							{
								//remove white spaces and "#OUTPUT="
								outputSource = trimmedTag.Substring("OUTPUT=".Length);
								continue;
							}
						}

						if (line.Contains("not_empty"))
						{
							Debug.LogWarning(ErrorMsg("'not_empty' line without any space at the beginning!"));
						}

						continue;
					}

					// System to only print some information if there has been anything printed in a block
					// e.g. only print a [Header()] if there are actual properties printed after it
					var trimmedLine = line.Trim();
					if (trimmedLine == "#if_not_empty")
					{
						var notEmptyBlock = new NotEmptyBlock();
						stackNotEmptyBlocks.Push(notEmptyBlock);
						currentStringBuilder = notEmptyBlock.stringBuilderToPrintBefore;
						continue;
					}
					else if (trimmedLine == "#start_not_empty_block")
					{
						var notEmptyBlock = stackNotEmptyBlocks.Peek();
						currentStringBuilder = notEmptyBlock.stringBuilderNotEmptyBlock;
						continue;
					}
					else if (trimmedLine == "#end_not_empty_block")
					{
						var notEmptyBlock = stackNotEmptyBlocks.Peek();
						currentStringBuilder = notEmptyBlock.stringBuilderToPrintAfter;
						continue;
					}
					else if (trimmedLine == "#end_not_empty")
					{
						var notEmptyBlock = stackNotEmptyBlocks.Pop();
						if (notEmptyBlock.ShouldBePrinted())
						{
							stringBuilder.Append(notEmptyBlock.Print());
						}
						currentStringBuilder = stringBuilder;
						continue;
					}

					//------------------------------------------------------------------------------------------------
					// New pass initialization

					if (newPass)
					{
						newPass = false;

						//Find out if current pass has a lighting function
						isSurfacePass = template.PassIsSurfaceShader(templateLines, passIndex);
						CurrentPassHasLightingFunction = isSurfacePass;

						//------------------------------------------------------------------------------------------------
						// Used Custom Material Properties for this pass

						currentPassUsedCustomMaterialProperties = usedCustomMaterialProperties[passIndex];
						currentPassUsedShaderProperties = usedShaderPropertiesPerPass[passIndex];

						//------------------------------------------------------------------------------------------------
						// UV Channels: usage and global tiling/offset & scrolling for this pass

						//Find used uv channels in (Custom) Material Properties
						usedUvChannelsFragment = new List<int>();
						usedUvChannelsVertex = new List<int>();
						uvChannelsDimensions = new Dictionary<int, int>();

						if (isSurfacePass)
						{
							if (!config.Flags.Contains("nolightmap"))
							{
								// lightmaps require TEXCOORD1 and TEXCOORD2 from vertex input
								AddUvChannelUsage(usedUvChannelsVertex, 1, 2);
								AddUvChannelUsage(usedUvChannelsVertex, 2, 2);
							}
						}

						uvChannelGlobalTilingOffset = new Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>>();
						uvChannelGlobalScrolling = new Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>>();
						uvChannelGlobalRandomOffset = new Dictionary<int, List<ShaderProperty.Imp_MaterialProperty_Texture>>();

						//Find used uv channels in Shader Properties
						foreach (var sp in usedShaderPropertiesPerPass[passIndex])
						{
							foreach (var imp in sp.implementations)
							{
								var vertexUvImp = imp as ShaderProperty.Imp_VertexTexcoord;
								if (vertexUvImp != null)
								{
									AddUvChannelUsage(usedUvChannelsVertex, vertexUvImp.TexcoordChannel, 2);

									if (sp.Program == ShaderProperty.ProgramType.Fragment)
									{
										int dimensions = 2;
										if (vertexUvImp.Channels.Contains("W"))
										{
											dimensions = 4;
										}
										else if (vertexUvImp.Channels.Contains("Z"))
										{
											dimensions = 3;
										}
										AddUvChannelUsage(usedUvChannelsFragment, vertexUvImp.TexcoordChannel, dimensions);
									}
								}

								var textureImp = imp as ShaderProperty.Imp_MaterialProperty_Texture;

								// find MaterialProperty_Texture implementations from Custom Material Properties
								if (textureImp == null)
								{
									var imp_ct = imp as ShaderProperty.Imp_CustomMaterialProperty;
									if (imp_ct != null && imp_ct.LinkedCustomMaterialProperty != null)
									{
										textureImp = imp_ct.LinkedCustomMaterialProperty.implementation as ShaderProperty.Imp_MaterialProperty_Texture;
									}
								}

								if (textureImp != null && textureImp.UvSource == ShaderProperty.Imp_MaterialProperty_Texture.UvSourceType.Texcoord)
								{
									AddUvChannelUsage(usedUvChannelsVertex, textureImp.UvChannel, 2);

									if (sp.Program == ShaderProperty.ProgramType.Fragment)
									{
										AddUvChannelUsage(usedUvChannelsFragment, textureImp.UvChannel, 2);
									}

									//Find global tiling/offset flags in Shader Properties to apply them in the vertex shader
									if (textureImp.UseTilingOffset && textureImp.GlobalTilingOffset)
									{
										if (!uvChannelGlobalTilingOffset.ContainsKey(textureImp.UvChannel))
										{
											uvChannelGlobalTilingOffset.Add(textureImp.UvChannel, new List<ShaderProperty.Imp_MaterialProperty_Texture>());
										}

										uvChannelGlobalTilingOffset[textureImp.UvChannel].Add(textureImp);
									}

									//Find global scrolling flags in Shader Properties to apply them in the vertex shader
									if (textureImp.UseScrolling && textureImp.GlobalScrolling)
									{
										if (!uvChannelGlobalScrolling.ContainsKey(textureImp.UvChannel))
										{
											uvChannelGlobalScrolling.Add(textureImp.UvChannel, new List<ShaderProperty.Imp_MaterialProperty_Texture>());
										}

										uvChannelGlobalScrolling[textureImp.UvChannel].Add(textureImp);
									}

									//Find global random offset flags in Shader Properties to apply them in the vertex shader
									if (textureImp.RandomOffset && textureImp.GlobalRandomOffset)
									{
										if (!uvChannelGlobalRandomOffset.ContainsKey(textureImp.UvChannel))
										{
											uvChannelGlobalRandomOffset.Add(textureImp.UvChannel, new List<ShaderProperty.Imp_MaterialProperty_Texture>());
										}

										uvChannelGlobalRandomOffset[textureImp.UvChannel].Add(textureImp);
									}
								}
							}
						}

						//------------------------------------------------------------------------------------------------
						// UV Channels & vertex-to-fragment variables: packing into float4 to reduce interpolators

						//input variables
						var listToPack = new List<string>();

						//texture coordinates
						usedUvChannelsVertex.Sort();
						usedUvChannelsFragment.Sort();
						foreach (int uv in usedUvChannelsFragment)
						{
							listToPack.Add(string.Format("float{0} texcoord{1}", uvChannelsDimensions[uv], uv));
						}

						//get needed input variables
						var inputVariables = template.GetInputBlock(templateLines, passIndex);
						if (inputVariables != null)
						{
							listToPack.AddRange(inputVariables);
						}

						//add to variables manager: if inputVariables is null, it means we are dealing with a surface shader, so no need for packing
						variablesManager = new VertexToFragmentVariablesManager(listToPack.ToArray(), inputVariables != null);
					}

					//detect shader code // comment
					var trimLine = line.TrimStart();
					var isComment = trimLine.StartsWith("//") && !trimLine.StartsWith("///");

					//Line break
					if (string.IsNullOrEmpty(line))
					{
						currentStringBuilder.AppendLine(line);
						continue;
					}

					//Replace @% keywords %@
					line = ReplaceKeywords(line, keywords);

					//New [[KEYWORD]] replacements
					while (!isComment && line.IndexOf("[[") >= 0)
					{
						var start = line.IndexOf("[[");
						var end = line.IndexOf("]]");
						var tag = line.Substring(start+2, end-start-2);

						var replacement = "";

						//Speficic variable
						if (tag.StartsWith("VALUE:"))
						{
							var varName = tag.Substring(tag.IndexOf(':')+1);
							//string varArgs = null;
							int argsStart = varName.IndexOf('(');
							if (argsStart > 0)
							{
								//varArgs = varName.Substring(argsStart + 1, varName.Length - argsStart - 2);
								varName = varName.Substring(0, argsStart);
							}

							var sp = config.GetShaderPropertyByName(varName);
							if (sp != null)
							{
								replacement = sp.PrintVariableName(inputSource);
							}
							else
							{
								Debug.LogError(ErrorMsg("No match for '<b>VALUE:" + varName + "</b>'"));
							}
						}
						//Specific property
						else if (tag.StartsWith("PROP:"))
						{
							var propName = tag.Substring(tag.IndexOf(':')+1);
							var sp = config.GetShaderPropertyByName(propName);
							if (sp != null)
							{
								replacement = sp.PrintProperties(indent);
								printedShaderProperties.Add(sp);
							}
							else
							{
								Debug.LogError(ErrorMsg("No match for '<b>PROP:" + propName + "'</b>"));
							}
						}
						//output code to declare texture coordinates and necessary vertex-to-fragment variables, packed as float4 (for v2f struct)
						else if (tag.StartsWith("INPUT_STRUCT_SEMANTICS"))
						{
							//get last available TEXCOORD number
							int availableTexcoord = int.Parse(tag.Substring(tag.IndexOf(':')+1));
							string packedVariables = variablesManager.PrintPackedVariables(availableTexcoord, indent);
							if (!string.IsNullOrEmpty(packedVariables))
							{
								replacement += packedVariables;
							}
						}
						//output variable that has to be handled by the variable manager
						else if (tag.StartsWith("INPUT_VALUE:"))
						{
							string varName = tag.Substring(tag.IndexOf(':')+1);
							string variable = variablesManager.GetVariable(varName);
							replacement += variable ?? varName;
						}
						else if (tag.StartsWith("SAMPLE_SHADER_PROPERTY:") || tag.StartsWith("SAMPLE_VALUE_SHADER_PROPERTY:"))
						{
							bool declareVariable = tag.StartsWith("SAMPLE_SHADER_PROPERTY:");
							var heading = declareVariable ? "SAMPLE_SHADER_PROPERTY:" : "SAMPLE_VALUE_SHADER_PROPERTY:";
							var property = tag.Substring(heading.Length);
							string args = null;
							int argsStart = property.IndexOf('(');
							if (argsStart > 0)
							{
								args = property.Substring(argsStart + 1, property.Length - argsStart - 2);
								property = property.Substring(0, argsStart);
							}

							var shaderProperty = currentPassUsedShaderProperties.Find(sp => sp.Name == property);
							if (shaderProperty != null)
							{
								if (shaderProperty.Program == CurrentProgram)
								{
									replacement += string.Format("{0}{1}", declareVariable ? indent : "", shaderProperty.PrintVariableSampleDeferred(inputSource, outputSource, CurrentProgram, args, declareVariable));
								}
							}
							else
							{
								Debug.LogError(ErrorMsg(string.Format("Can't find property '{0}' for manual sampling", property)));
							}
						}
						//Generic [[tags]]
						else
						{
							switch (tag)
							{
								case "SURFACE_FLAGS":
									break;

								case "PROPERTIES":
									//shader properties
									if (allPassesUsedShaderProperties != null && allPassesUsedShaderProperties.Count > 0)
									{
										foreach (var sp in allPassesUsedShaderProperties)
										{
											if (!printedShaderProperties.Contains(sp))
											{
												var prop = sp.PrintProperties(indent);
												if (!string.IsNullOrEmpty(prop))
													replacement += prop + "\n" + indent;
											}
										}

										if (!string.IsNullOrEmpty(replacement))
											replacement = "\n" + indent + replacement;
									}

									//custom material properties
									string tempString = "";
									if (allPassesUsedCustomMaterialProperty != null && allPassesUsedCustomMaterialProperty.Count > 0)
										foreach (var ct in allPassesUsedCustomMaterialProperty)
											tempString += ct.PrintProperty(indent) + "\n" + indent;

									if (!string.IsNullOrEmpty(tempString))
									{
										if (string.IsNullOrEmpty(replacement))
											replacement = "// Custom Material Properties\n" + indent + tempString;
										else
											replacement += "\n" + indent + "// Custom Material Properties\n" + indent + tempString;
									}

									replacement = replacement.TrimEnd();
									break;

								case "VARIABLES_SURFACE_OUTPUT":
									//shader properties that will be sampled in lighting function
									if (usedShaderPropertiesPerPass[passIndex] != null && usedShaderPropertiesPerPass[passIndex].Count > 0)
									{
										foreach (var sp in usedShaderPropertiesPerPass[passIndex])
										{
											var variable = sp.PrintVariableSurfaceOutput();
											if (!string.IsNullOrEmpty(variable))
												replacement += indent + variable + "\n";
										}
									}
									if (!string.IsNullOrEmpty(replacement))
										replacement = "\n" + indent + "// Shader Properties\n" + replacement;

									replacement = replacement.TrimEnd();
									break;

								case "VARIABLES_INCLUDE":
									isInIncludeBlock = true;
									goto case "VARIABLES";

								case "VARIABLES_OUTSIDE_CBUFFER_INCLUDE":
									isInIncludeBlock = true;
									isOutsideCBuffer = true;
									goto case "VARIABLES";

								case "VARIABLES":
									// Custom Material Properties
									// If in CGINCLUDE block, print *all* Custom Material Properties
									if (isInIncludeBlock)
									{
										var allCustomMaterialProperties = new List<CustomMaterialPropertyUsage>();
										foreach (var list in usedCustomMaterialProperties)
										{
											foreach (var cmp in list)
											{
												if (!allCustomMaterialProperties.Contains(cmp))
												{
													allCustomMaterialProperties.Add(cmp);
												}
											}
										}

										var uniqueMaterialPropertiesList = new List<ShaderProperty.CustomMaterialProperty>();
										foreach (var ctUsage in allCustomMaterialProperties)
										{
											if (!uniqueMaterialPropertiesList.Contains(ctUsage.customMaterialProperty))
											{
												uniqueMaterialPropertiesList.Add(ctUsage.customMaterialProperty);
											}
										}

										foreach (var customMaterialProperty in uniqueMaterialPropertiesList)
										{
											if (isOutsideCBuffer)
											{
												replacement += indent + customMaterialProperty.PrintVariablesDeclareOutsideCBuffer(indent) + "\n";
											}
											else
											{
												if (!customMaterialProperty.IsGpuInstanced && !cgIncludeCustomMaterialProperties.Contains(customMaterialProperty))
												{
													replacement += indent + customMaterialProperty.PrintVariablesDeclare(false, indent) + "\n";
													cgIncludeCustomMaterialProperties.Add(customMaterialProperty);
												}
											}
										}
									}
									else
									{
										if (currentPassUsedCustomMaterialProperties != null && currentPassUsedCustomMaterialProperties.Count > 0)
										{
											var hashset = new HashSet<ShaderProperty.CustomMaterialProperty>();
											foreach (var ctUsage in currentPassUsedCustomMaterialProperties)
											{
												if (!ctUsage.customMaterialProperty.IsGpuInstanced && !hashset.Contains(ctUsage.customMaterialProperty) && !cgIncludeCustomMaterialProperties.Contains(ctUsage.customMaterialProperty))
												{
													replacement += indent + ctUsage.customMaterialProperty.PrintVariablesDeclare(false, indent) + "\n";

													hashset.Add(ctUsage.customMaterialProperty);
												}
											}
										}
									}

									if (!string.IsNullOrEmpty(replacement))
									{
										replacement = "\n" + indent + "// Custom Material Properties\n" + replacement;
									}

									// Shader Properties
									tempString = "";
									// If in CGINCLUDE block, print *all* Shader Properties
									if (isInIncludeBlock)
									{
										var allUsedShaderProperties = new List<ShaderProperty>();
										foreach (var list in usedShaderPropertiesPerPass)
										{
											foreach (var sp in list)
											{
												if (!allUsedShaderProperties.Contains(sp))
												{
													allUsedShaderProperties.Add(sp);
												}
											}
										}

										foreach (var sp in allUsedShaderProperties)
										{
											if (isOutsideCBuffer)
											{
												string declarations = sp.PrintVariableDeclareOutsideCBuffer(indent);
												if (!string.IsNullOrEmpty(declarations))
												{
													tempString += indent + declarations + "\n";
												}
											}
											else
											{
												string declarations = sp.PrintVariableDeclare(false, indent);
												if (!string.IsNullOrEmpty(declarations) && !cgIncludeShaderProperties.Contains(sp))
												{
													tempString += indent + declarations + "\n";
													cgIncludeShaderProperties.Add(sp);
												}
											}
										}
									}
									else
									{
										// Regular pass, print this pass's Shader Properties
										if (usedShaderPropertiesPerPass[passIndex] != null && usedShaderPropertiesPerPass[passIndex].Count > 0)
										{
											foreach (var sp in usedShaderPropertiesPerPass[passIndex])
											{
												if (cgIncludeShaderProperties.Contains(sp))
												{
													continue;
												}

												var prop = sp.PrintVariableDeclare(false, indent);
												if (!string.IsNullOrEmpty(prop))
												{
													tempString += indent + prop + "\n";
												}
											}
										}
									}

									if (!string.IsNullOrEmpty(tempString))
									{
										replacement += "\n" + indent + "// Shader Properties\n" + tempString;
									}

									replacement = replacement.TrimEnd();

									isInIncludeBlock = false;
									isOutsideCBuffer = false;
									break;

								case "VARIABLES_GPU_INSTANCING_INCLUDE":
									isInIncludeBlock = true;
									goto case "VARIABLES_GPU_INSTANCING";

								case "VARIABLES_GPU_INSTANCING":

									var indentPlusOne = indent + "\t";


									// Custom Material Properties
									// If in CGINCLUDE block, print *all* Custom Material Properties
									if (isInIncludeBlock)
									{
										var allCustomMaterialProperties = new List<CustomMaterialPropertyUsage>();
										foreach (var list in usedCustomMaterialProperties)
										{
											foreach (var cmp in list)
											{
												if (!allCustomMaterialProperties.Contains(cmp))
												{
													allCustomMaterialProperties.Add(cmp);
												}
											}
										}

										var hashset = new HashSet<ShaderProperty.CustomMaterialProperty>();
										foreach (var ctUsage in allCustomMaterialProperties)
										{
											if (ctUsage.customMaterialProperty.IsGpuInstanced
												&& !hashset.Contains(ctUsage.customMaterialProperty)
												&& !cgIncludeCustomMaterialProperties.Contains(ctUsage.customMaterialProperty))
											{
												replacement += indentPlusOne + ctUsage.customMaterialProperty.PrintVariablesDeclare(true, indentPlusOne) + "\n";

												hashset.Add(ctUsage.customMaterialProperty);
												cgIncludeCustomMaterialProperties.Add(ctUsage.customMaterialProperty);
											}
										}
									}
									else
									{
										if (currentPassUsedCustomMaterialProperties != null && currentPassUsedCustomMaterialProperties.Count > 0)
										{
											var hashset = new HashSet<ShaderProperty.CustomMaterialProperty>();
											foreach (var ctUsage in currentPassUsedCustomMaterialProperties)
											{
												if (ctUsage.customMaterialProperty.IsGpuInstanced && !hashset.Contains(ctUsage.customMaterialProperty) && !cgIncludeCustomMaterialProperties.Contains(ctUsage.customMaterialProperty))
												{
													replacement += indentPlusOne + ctUsage.customMaterialProperty.PrintVariablesDeclare(true, indentPlusOne) + "\n";

													hashset.Add(ctUsage.customMaterialProperty);
												}
											}
										}
									}


									if (!string.IsNullOrEmpty(replacement))
									{
										replacement = indentPlusOne + "// Custom Material Properties\n" + replacement;
									}

									// Shader Properties
									tempString = "";
									// If in CGINCLUDE block, print *all* Shader Properties
									if (isInIncludeBlock)
									{
										var allUsedShaderProperties = new List<ShaderProperty>();
										foreach (var list in usedShaderPropertiesPerPass)
										{
											foreach (var sp in list)
											{
												if (!allUsedShaderProperties.Contains(sp))
												{
													allUsedShaderProperties.Add(sp);
												}
											}
										}

										foreach (var sp in allUsedShaderProperties)
										{
											var prop = sp.PrintVariableDeclare(true, indentPlusOne);
											if (!string.IsNullOrEmpty(prop) && !cgIncludeShaderProperties.Contains(sp))
											{
												tempString += indentPlusOne + prop + "\n";
												cgIncludeShaderProperties.Add(sp);
											}
										}
									}
									else
									{
										// Regular pass, print this pass's Shader Properties
										if (usedShaderPropertiesPerPass[passIndex] != null && usedShaderPropertiesPerPass[passIndex].Count > 0)
										{
											foreach (var sp in usedShaderPropertiesPerPass[passIndex])
											{
												if (cgIncludeShaderProperties.Contains(sp))
												{
													continue;
												}

												var prop = sp.PrintVariableDeclare(true, indentPlusOne);
												if (!string.IsNullOrEmpty(prop))
												{
													tempString += indentPlusOne + prop + "\n";
												}
											}
										}
									}

									if (!string.IsNullOrEmpty(tempString))
									{
										replacement += indentPlusOne + "// Shader Properties\n" + tempString;
									}

									replacement = replacement.TrimEnd();

									if (!string.IsNullOrEmpty(replacement))
									{
										var sb = new StringBuilder();
										sb.AppendLine();
										sb.AppendLine(indent + "// Instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.");
										sb.AppendLine(indent + "// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.");
										sb.AppendLine(indent + "// #pragma instancing_options assumeuniformscaling");
#if UNITY_2017_3_OR_NEWER
										sb.AppendLine(indent + "UNITY_INSTANCING_BUFFER_START(Props)");
										sb.AppendLine("{0}");
										sb.AppendLine(indent + "UNITY_INSTANCING_BUFFER_END(Props)");
#else
										sb.AppendLine(indent + "UNITY_INSTANCING_CBUFFER_START(Props)");
										sb.AppendLine("{0}");
										sb.AppendLine(indent + "UNITY_INSTANCING_CBUFFER_END");
#endif

										replacement = string.Format(sb.ToString(), replacement);
									}

									isInIncludeBlock = false;
									break;

								case "GPU_INSTANCING_OPTIONS":
									if (keywords.ContainsKey("FLAGS:pragma_gpu_instancing"))
									{
										replacement = string.Format("#pragma instancing_options {0}", keywords["FLAGS:pragma_gpu_instancing"]);

										// Special case for GPU Instancing (force)maxcount
										if (replacement.Contains("maxcount"))
										{
											replacement = replacement.Replace("maxcount", string.Format("maxcount:{0}", keywords["GPU_INSTANCING_MAX_COUNT_VALUE"]));
										}
									}
									break;

								case "VERTEX_INPUT_TEXCOORDS":
									//print needed texcoords from vertex input
									if (usedUvChannelsVertex.Count > 0)
									{
										replacement = "";
										foreach (int uv in usedUvChannelsVertex)
										{
											replacement += string.Format("{0}float4 texcoord{1} : TEXCOORD{1};\n", indent, uv);
										}
										replacement = replacement.TrimEnd('\n');
									}
									break;

								case "VERTEX_INPUT_OUTLINE":
									//print texcoords that could be needed for outline normals; only if they haven't been printed by VERTEX_INPUT_TEXCOORDS first
									replacement = "";
									string indentMinus1 = indent.Length > 0 ? indent.Substring(0, indent.Length-1) : indent;
									bool first = true;
									for (int uv = 0; uv < 4; uv++)
									{
										if (usedUvChannelsVertex.Contains(uv))
										{
											continue;
										}
										replacement += string.Format("{0}{1} TCP2_UV{2}_AS_NORMALS\n", first ? "" : indentMinus1, first ? "#if" : "#elif", uv+1);
										replacement += string.Format("{0}float4 texcoord{1} : TEXCOORD{1};\n", indent, uv);
										first = false;
									}
									if (!string.IsNullOrEmpty(replacement))
									{
										replacement += indentMinus1 + "#endif";
									}
									break;

								//output code to declare texture coordinates and necessary vertex-to-fragment variables in Input struct (surface shader)
								case "INPUT_STRUCT":
									//print used texcoords
									if (usedUvChannelsFragment.Count > 0)
									{
										replacement = "";
										foreach (int uv in usedUvChannelsFragment)
										{
											replacement += string.Format("{0}float{1} texcoord{2};\n", indent, uvChannelsDimensions[uv], uv);
										}
										replacement = replacement.TrimEnd('\n');
									}
									break;

								//output code to generate texture coordinates in the vertex shader
								case "VERTEX_TEXCOORDS":
									bool hasTexcoords = false;

									Action<int, bool> printWithGlobalTilingOffset = (int uv, bool isVertex) =>
									{
										//generate global tiling/offset calculations
										var globalTiling = "";
										var globalOffset = "";
										var globalScrolling = "";
										var globalRandomOffset = "";
										if (uvChannelGlobalTilingOffset.ContainsKey(uv))
										{
											foreach (var textureImplementation in uvChannelGlobalTilingOffset[uv])
											{
												globalTiling += string.Format(" * {0}.xy", textureImplementation.GetDefaultTilingOffsetVariable());
												globalOffset += string.Format(" + {0}.zw", textureImplementation.GetDefaultTilingOffsetVariable());
											}
										}

										if (uvChannelGlobalScrolling.ContainsKey(uv))
										{
											foreach (var textureImplementation in uvChannelGlobalScrolling[uv])
											{
												globalScrolling += string.Format(" + frac(_Time.yy * {0}.xy)", textureImplementation.GetDefaultScrollingVariable());
											}
										}

										if (uvChannelGlobalRandomOffset.ContainsKey(uv))
										{
											foreach (var textureImplementation in uvChannelGlobalRandomOffset[uv])
											{
												globalRandomOffset += string.Format(" + hash22(floor(_Time.xx * {0}.xx) / {0}.xx)", textureImplementation.GetDefaultOffsetSpeedVariable());
											}
										}

										bool hasModifiers = globalTiling != "" || globalOffset != "" || globalScrolling != "" || globalRandomOffset != "";

										if (isVertex)
										{
											replacement += string.Format("{0}.texcoord{1}.xy = {0}.texcoord{1}.xy{2}{3}{4}{5};\n{6}", inputSource, uv, globalTiling, globalScrolling, globalOffset, globalRandomOffset, indent);
										}
										else
										{
											// if necessary, first print without the modifiers to copy all channels
											if (uvChannelsDimensions[uv] > 2 || !hasModifiers)
											{
												replacement += string.Format("{0}.{1} = {2}.texcoord{3}.xy;\n{4}", outputSource, variablesManager.GetVariable("texcoord" + uv), inputSource, uv, indent);
											}

											// then handle the .xy with modifiers, if any
											if (hasModifiers)
											{
												replacement += string.Format("{0}.{1}.xy = {2}.texcoord{3}.xy{4}{5}{6}{7};\n{8}", outputSource, variablesManager.GetVariable("texcoord" + uv), inputSource, uv, globalTiling, globalScrolling, globalOffset, globalRandomOffset, indent);
											}
										}
									};

									if (usedUvChannelsFragment.Count > 0)
									{
										hasTexcoords = true;

										foreach (int uv in usedUvChannelsFragment)
										{
											if (uvChannelsDimensions[uv] > 2)
											{

											}

											printWithGlobalTilingOffset(uv, false);
										}
										replacement = replacement.TrimEnd();
									}

									// No need to print "v.texcoord0 = v.texcoord0"... unless it has a modifier that stays throughout the vertex program?
									/*
									if (usedUvChannelsVertex.Count > 0)
									{
										hasTexcoords = true;

										foreach (int uv in usedUvChannelsVertex)
										{
											if (usedUvChannelsFragment.Contains(uv))
											{
												continue;
											}

											printWithGlobalTilingOffset(uv, true);
										}
									}
									*/

									if (hasTexcoords)
									{
										replacement = indent + "// Texture Coordinates\n" + indent + replacement;
									}

									break;

								//output code to sample all the Custom Material Properties so that their values are ready to be used
								case "SAMPLE_CUSTOM_TEXTURES":
								case "SAMPLE_CUSTOM_PROPERTIES":
									if (currentPassUsedCustomMaterialProperties != null && currentPassUsedCustomMaterialProperties.Count > 0)
									{
										if (CurrentProgram == ShaderProperty.ProgramType.Fragment)
										{
											foreach (var ctUsage in currentPassUsedCustomMaterialProperties)
											{
												if (ctUsage.program == ShaderProperty.ProgramType.Fragment)
												{
													string sampled = ctUsage.customMaterialProperty.SampleVariableFragment(inputSource, outputSource);
													if (sampled != null)
													{
														replacement += indent + sampled;
													}
												}
											}
										}
										else if (CurrentProgram == ShaderProperty.ProgramType.Vertex)
										{
											foreach (var ctUsage in currentPassUsedCustomMaterialProperties)
											{
												if (ctUsage.program == ShaderProperty.ProgramType.Vertex)
												{
													string sampled = ctUsage.customMaterialProperty.SampleVariableVertex(inputSource, outputSource);
													if (sampled != null)
													{
														replacement += indent + sampled;
													}
												}
											}
										}

										if (!string.IsNullOrEmpty(replacement))
										{
											replacement = indent + "// Custom Material Properties Sampling\n" + replacement;
										}
									}
									break;

								default:
									Debug.LogError(ErrorMsg("Unknown tag: <b>[[" + tag + "]]</b>"));
									replacement = "/* UNKNOWN_TAG:" + tag + " */";
									break;

								//output code to sample all the Shader Properties so that their values are ready to be used
								case "SAMPLE_SHADER_PROPERTIES":
									if (currentPassUsedShaderProperties != null && currentPassUsedShaderProperties.Count > 0)
									{
										//Topological sorting based on dependencies
										var sortedList = new List<ShaderProperty>();
										{
											const int DEAD = 0;
											const int ALIVE = 1;
											const int UNDEAD = 2;

											Dictionary<ShaderProperty, int> states = new Dictionary<ShaderProperty, int>();
											foreach (var p in currentPassUsedShaderProperties)
												states.Add(p, ALIVE);

											Action<ShaderProperty, List<ShaderProperty>> visit = null;
											visit = (ShaderProperty prop, List<ShaderProperty> list) =>
											{
												if (!states.ContainsKey(prop))
												{
													Debug.LogError(ErrorMsg("Can't find property: " + prop.Name));
													return;
												}

												if (states[prop] == DEAD)
													return;
												if (states[prop] == UNDEAD)
												{
													Debug.LogError(ErrorMsg("Cyclic reference!"));
													return;
												}

												states[prop] = UNDEAD;
												foreach (var imp in prop.implementations)
												{
													var impSpRef = imp as ShaderProperty.Imp_ShaderPropertyReference;
													if (impSpRef != null)
													{
														foreach (var d in impSpRef.Dependencies)
														{
															visit(d, list);
														}
													}

													var impMpTex = imp as ShaderProperty.Imp_MaterialProperty_Texture;
													if (impMpTex != null && impMpTex.UvSource == ShaderProperty.Imp_MaterialProperty_Texture.UvSourceType.OtherShaderProperty)
													{
														foreach (var d in impMpTex.Dependencies)
														{
															visit(d, list);
														}
													}
												}
												states[prop] = DEAD;
												list.Add(prop);
											};

											foreach (var prop in currentPassUsedShaderProperties)
											{
												visit(prop, sortedList);
											}
										};

										foreach (var sp in sortedList)
										{
											if (!sp.deferredSampling && !ShaderProperty.VariableTypeIsFixedFunction(sp.Type) && sp.Program == CurrentProgram)
											{
												replacement += string.Format("{0}{1}\n", indent, sp.PrintVariableSample(inputSource, outputSource, CurrentProgram, null));
											}
										}

										if (!string.IsNullOrEmpty(replacement))
										{
											replacement = indent + "// Shader Properties Sampling\n" + replacement;
										}
									}
									break;
							}
						}

						line = line.Replace("[["+tag+"]]", replacement);

						//restore indentation
						line = indent + line.TrimStart(' ', '\t');

						//skip line if resulting line is empty
						skipLine = string.IsNullOrEmpty(line.Trim());
					}

					//Append line
					if (!skipLine)
					{
						currentStringBuilder.AppendLine(line);
					}
				}
				currentStringBuilder = null;
				currentIndent = null;

				//Post pass to:
				// - remove multiple successive line breaks
				// - remove TCP2Header & TCP2Separator without any filler (e.g. if all properties are constants)
				// - add dummy variable if 'struct Input' is empty
				var outputStr = stringBuilder.Replace("\r\n", "\n").ToString();
				stringBuilder.Length = 0;
				var newLines = outputStr.Split('\n');
				bool lastLineWasEmpty = false;
				bool isInsideInputStruct = false;
				bool isInputStructEmpty = false;
				var inputIndent = "";
				for (var i = 0; i < newLines.Length; i++)
				{
					if (newLines[i].Contains("TCP2Header"))
					{
						for (int j = i+1; j < newLines.Length; j++)
						{
							if (string.IsNullOrEmpty(newLines[j]))
								continue;
							else if (newLines[j].Trim().StartsWith("//"))
								continue;
							else if (newLines[j].Contains("TCP2Separator"))
							{
								//skip the lines if not "real" content was found between TCP2Header & TCP2Separator
								i = j+1;
								break;
							}
							else
								break;
						}
					}

					//append a dummy variable if the Input struct is empty, to prevent compilation error
					if (isInsideInputStruct)
					{
						if (newLines[i].Contains("}"))
						{
							isInsideInputStruct = false;
							if (isInputStructEmpty)
							{
								stringBuilder.AppendLine(inputIndent + "\tfloat input_is_not_empty;");
							}
						}
						else if (!newLines[i].Contains("{"))
						{
							isInputStructEmpty = false;
						}
					}
					else if (newLines[i].Trim().StartsWith("struct Input"))
					{
						isInsideInputStruct = true;
						isInputStructEmpty = true;

						inputIndent = "";
						foreach (var c in newLines[i])
						{
							if (char.IsWhiteSpace(c))
								inputIndent += c;
							else
								break;
						}
					}

					var empty = string.IsNullOrEmpty(newLines[i].Trim());

					if (!(empty && lastLineWasEmpty))
						stringBuilder.AppendLine(newLines[i]);

					lastLineWasEmpty = empty;
				}

				//Add serialized data
				stringBuilder.AppendLine(config.GetSerializedData());

				//Calculate hash
				string normalizedLineEndings = stringBuilder.ToString().Replace("\r\n", "\n");
				var hash = GetHash(normalizedLineEndings);
				stringBuilder.AppendLine(string.Format(Config.kHashPrefix + hash + Config.kHashSuffix));

				//Convert line endings to current OS format
				stringBuilder = stringBuilder.Replace("\r\n", "\n");
				stringBuilder = stringBuilder.Replace("\n", Environment.NewLine);

				var sourceCode = stringBuilder.ToString();

				return sourceCode;
			}

			internal static string GetHash(string input)
			{
				using (var md5 = System.Security.Cryptography.MD5.Create())
				{
					var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
					var hashStringBuilder = new StringBuilder();
					for (int i = 0; i < hash.Length; i++)
					{
						hashStringBuilder.Append(hash[i].ToString("x2"));
					}

					return hashStringBuilder.ToString();
				}
			}

			static string ReplaceKeywords(string line, Dictionary<string, string> searchAndReplace)
			{
				if (line.IndexOf("@%") < 0)
					return line;

				foreach (var kv in searchAndReplace)
					line = line.Replace("@%" + kv.Key + "%@", kv.Value);

				return line;
			}

			//Save .shader file
			static Shader SaveShader(Config config, Shader existingShader, string sourceCode, bool overwritePrompt, bool modifiedPrompt)
			{
				if (string.IsNullOrEmpty(config.Filename))
				{
					Debug.LogError(ErrorMsg("Can't save Shader: filename is null or empty!"));
					return null;
				}

				//Save file
				var outputPath = GetOutputPath();
				var filename = config.Filename;

				//Get existing shader exact path
				if (existingShader != null)
				{
					outputPath = GetExistingShaderPath(config, existingShader);
				}

				if (!Directory.Exists(outputPath))
				{
					Directory.CreateDirectory(outputPath);
				}

				var fullPath = outputPath + filename + ".shader";
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
					var assetPath = fullPath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

					var shader = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader)) as Shader;
					if (GlobalOptions.data.SelectGeneratedShader)
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

						//Needed to save userData in .meta file
						AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
					}
					else
					{
						Debug.LogWarning("[TCP2 Shader Generator] Couldn't find ShaderImporter.\nDefault textures won't be set for the generated shader.");
					}

					return shader;
				}

				return null;
			}

			static string GetExistingShaderPath(Config config, Shader existingShader)
			{
				//Override OutputPath if Shader already exists, to make sure we replace the original shader file
				var unityPath = AssetDatabase.GetAssetPath(existingShader);
				unityPath = Path.GetDirectoryName(unityPath);
				if (config.Filename.Contains("/"))
				{
					var filenamePath = Path.GetDirectoryName(config.Filename);
					if (unityPath.EndsWith(filenamePath))
						unityPath = unityPath.Substring(0, unityPath.LastIndexOf(filenamePath)); //remove subdirectories
				}
				if (!unityPath.EndsWith("/"))
					unityPath = unityPath + "/";
				return Utils.UnityRelativeToSystemPath(unityPath);
			}

			//Returns hash of file content to check for manual modifications (with 'h' prefix)
			static string GetShaderContentHash(ShaderImporter importer)
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

			//Format an error message
			internal static string ErrorMsg(string message)
			{
				return string.Format("[Shader Generator] {0}\n", message);
			}

			//Show a contextual help box message if the option is enabled
			internal static bool ContextualHelpBox(string message, string helpTopic = null)
			{
				bool hasLink = !string.IsNullOrEmpty(helpTopic);

				if (GlobalOptions.data.ShowContextualHelp)
				{
					TCP2_GUI.ContextualHelpBoxLayout(message, hasLink);
				}

				if (hasLink)
				{
					var rect = GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

					if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 1 && rect.Contains(Event.current.mousePosition))
					{
						Application.OpenURL(ToonyColorsPro.ShaderGenerator.ShaderGenerator2.DOCUMENTATION_URL + "#" + helpTopic);
					}
				}

				return GlobalOptions.data.ShowContextualHelp;
			}

			//Handles the vertex-to-fragment variables that need to be packed into float4 (for vert/frag shaders),
			//or just be used as-is (for surface shaders),
			//as well as the needed texture coordinates (for both)
			internal class VertexToFragmentVariablesManager
			{
				class Variable
				{
					public string name;
					public int dimensions = 1; //1-4
					public string vectorType = ""; //fixed, half, float; stored but not used currently

					public override string ToString()
					{
						return string.Format("[Variable {0}{1} {2}]", vectorType, dimensions, name);
					}
				}

				Float4Packer float4packer;
				Dictionary<string, Variable> rawVariables;
				bool isUsingPacking;

				public VertexToFragmentVariablesManager(string[] variables, bool needsPacking)
				{
					//static ref
					VariablesManager = this;

					isUsingPacking = needsPacking;

					if (isUsingPacking)
					{
						//pack remaining variables in float4, because we are dealing with a vert/frag shader
						float4packer = new Float4Packer(variables);
					}
					else
					{
						//get raw variable without any packing, because the surface shaders already packs them
						rawVariables = new Dictionary<string, Variable>();
						foreach (var v in variables)
						{
							var var = StringToVariable(v);
							rawVariables.Add(var.name, var);
						}
					}
				}

				//Convert a line with format "float3 variable;"
				Variable StringToVariable(string line)
				{
					line = line.Trim();
					Variable var = new Variable();
					foreach (char c in line)
					{
						bool isDigit = char.IsDigit(c);
						if (isDigit || char.IsWhiteSpace(c))
						{
							if (isDigit)
								var.dimensions = int.Parse(c.ToString());
							else
								var.dimensions = 1;
							break;
						}
						else
							var.vectorType += c;
					}

					var.name = line.Substring(line.IndexOf(' ')+1).TrimEnd(';');
					return var;
				}

				public string GetVariable(string name)
				{
					if (isUsingPacking)
						return float4packer.GetVariable(name);
					else
					{
						if (rawVariables.ContainsKey(name))
							return rawVariables[name].name;
					}

					return null;
				}

				public string PrintPackedVariables(int texcoord, string indent)
				{
					if (isUsingPacking)
					{
						var output = "";

						for (int i = 0; i < float4packer.packs.Length; i++)
						{
							var pack = float4packer.packs[i];
							string comment = "";
							foreach (var m in pack.mapping)
							{
								comment += string.Format("{0} = {1}  ", m.Value, m.Key);
							}
							comment = string.Format(" /* {0} */", comment.Trim());
							string packedName = pack.keepName ? pack.variables[0].name : string.Format("pack{0}", i);
							output += string.Format("{0}float{1} {2} : TEXCOORD{3};{4}\n", indent, pack.dimensions > 1 ? pack.dimensions.ToString() : "", packedName, texcoord + i, pack.keepName ? "" : comment);
						}
						return output.TrimEnd('\n');
					}
					else
					{
						return null;
					}
				}

				public string PrintRawVariables(string indent)
				{
					var output = "";
					foreach (var v in rawVariables)
					{
						output += string.Format("{0}{1}{2} {3};\n", indent, v.Value.vectorType, v.Value.dimensions > 1 ? v.Value.dimensions.ToString() : "", v.Value.name);
					}
					return output.TrimEnd('\n');
				}

				class Float4Packer
				{
					//Given a list of float1-4 parameters, pack them as float4 and map them through a dictionary for later use

					public class Pack
					{
						public List<Variable> variables;
						public int dimensions;
						public Dictionary<string, string> mapping;
						// don't use packN as a name, keep the original name instead (only for float4 variables)
						public bool keepName;

						public void CreateMapping(int packIndex)
						{
							dimensions = 0;
							mapping = new Dictionary<string, string>();
							var channels = new Queue<string>(new[] { "x", "y", "z", "w" });
							for (int i = 0; i < variables.Count; i++)
							{
								dimensions += variables[i].dimensions;

								string swizzle = "";
								for (int j = 0; j < variables[i].dimensions; j++)
									swizzle += channels.Dequeue();

								string packedName = keepName ? variables[i].name : string.Format("pack{0}", packIndex);
								mapping.Add(variables[i].name, string.Format("{0}.{1}", packedName, swizzle));
							}
						}
					}

					public Pack[] packs;

					public Float4Packer(string[] variables)
					{
						ParseVariables(variables);
					}

					void ParseVariables(string[] variables)
					{
						var variablesList = new List<Variable>();

						//each line = a variable in format "vector_n variableName;", e.g. "half3 myVariable;"
						foreach (var v in variables)
						{
							Variable var = new Variable();
							foreach (char c in v)
							{
								bool isDigit = char.IsDigit(c);
								if (isDigit || char.IsWhiteSpace(c))
								{
									if (isDigit)
										var.dimensions = int.Parse(c.ToString());
									else
										var.dimensions = 1;
									break;
								}
								else
									var.vectorType += c;
							}

							var.name = v.Substring(v.IndexOf(' ')+1).TrimEnd(';');
							variablesList.Add(var);
						}

						//sort from higher to lower dimensions
						variablesList.Sort((v1, v2) => v2.dimensions.CompareTo(v1.dimensions));

						var usedVariables = new HashSet<Variable>();
						var float4list = new List<Pack>();

						//get n dimension(s) variable from the list
						Func<int, Variable> FindNDimensionsVariable = (int d) =>
						{
							foreach (var v in variablesList)
								if (v.dimensions == d && !usedVariables.Contains(v))
									return v;
							return null;
						};

						for (int i = 0; i < variablesList.Count; i++)
						{
							var v = variablesList[i];

							if (usedVariables.Contains(v))
								continue;

							switch (v.dimensions)
							{
								case 4:
								{
									float4list.Add(new Pack() { variables = new List<Variable>() { v }, keepName = true });
									usedVariables.Add(v);
									break;
								}

								case 3:
								{
									var pack = new Pack() { variables = new List<Variable>() { v } };
									usedVariables.Add(v);

									//look for a float1 variable to fill the float4 pack
									var v1 = FindNDimensionsVariable(1);
									if (v1 != null)
									{
										pack.variables.Add(v1);
										usedVariables.Add(v1);
									}

									float4list.Add(pack);
									break;
								}

								case 2:
								{
									var pack = new Pack() { variables = new List<Variable>() { v } };
									usedVariables.Add(v);

									//look for a float2 variable to fill the float4 pack
									var v2 = FindNDimensionsVariable(2);
									if (v2 != null)
									{
										pack.variables.Add(v2);
										usedVariables.Add(v2);
									}

									float4list.Add(pack);
									break;
								}

								case 1:
								{
									var pack = new Pack() { variables = new List<Variable>() { v } };
									usedVariables.Add(v);
									int toFill = 3;

									//look for multiple float1 variables to fill the float4 pack
									Variable v1;
									do
									{
										v1 = FindNDimensionsVariable(1);
										if (v1 != null)
										{
											pack.variables.Add(v1);
											usedVariables.Add(v1);
											toFill--;
										}
									}
									while (toFill > 0 && v1 != null);

									float4list.Add(pack);
									break;
								}
							}
						}

						//create mapping => variable name = packN.swizzle
						for (int i = 0; i < float4list.Count; i++)
							float4list[i].CreateMapping(i);

						packs = float4list.ToArray();
					}

					public string GetVariable(string name)
					{
						foreach (var p in packs)
						{
							if (p.mapping.ContainsKey(name))
							{
								return p.mapping[name];
							}
						}
						return null;
					}
				}
			}
		}
	}
}
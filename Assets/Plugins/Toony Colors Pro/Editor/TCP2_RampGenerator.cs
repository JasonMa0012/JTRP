// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ToonyColorsPro.Utilities;
using Gradient = UnityEngine.Gradient;

// Utility to generate ramp textures

namespace ToonyColorsPro
{
	public class TCP2_RampGenerator : EditorWindow
	{
		const float WINDOW_WIDTH = 352f;
		const float WINDOW_HEIGHT = 200f;
		//const float WINDOW_HEIGHT = 256f;

		[MenuItem(Menu.MENU_PATH + "Ramp Generator", false, 600)]
		static void OpenTool()
		{
			GetWindowTCP2();
		}

		private static TCP2_RampGenerator GetWindowTCP2()
		{
			var window = GetWindow<TCP2_RampGenerator>(true, "TCP2 : Ramp Generator", true);
			window.editMode = false;
			window.linkedTexture = null;
			window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
			window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
			return window;
		}

		public static void OpenForEditing(Texture2D texture, Object[] materials, bool openedFromMaterial, bool isNewTexture)
		{
			var window = GetWindow<TCP2_RampGenerator>(true, "TCP2 : Ramp Generator", true);
			window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
			window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
			var matList = new List<Material>();
			if (materials != null)
			{
				foreach (var o in materials)
					if (o is Material)
						matList.Add(o as Material);
			}
			window.editModeFromMaterial = openedFromMaterial;
			window.InitEditMode(texture, matList.ToArray());
			window.isNewTexture = isNewTexture;
		}

		//--------------------------------------------------------------------------------------------------
		// INTERFACE

#if UNITY_EDITOR_WIN
		private const string OUTPUT_FOLDER = "\\Textures\\Custom Ramps\\";
#else
	private const string OUTPUT_FOLDER = "/Textures/Custom Ramps/";
#endif

		[SerializeField]
		private Gradient mGradient;
		[SerializeField]
		private Gradient[] m2dGradients;
		private int textureWidth = 256;
		private int textureHeight = 256;
		private bool editMode;
		private bool isNewTexture;
		private bool textureIsDirty;
		private bool editedTextureIs2d;
		private Texture2D linkedTexture;
		private AssetImporter linkedImporter;
		private Material[] linkedMaterials;
		private bool editModeFromMaterial;

		//--------------------------------------------------------------------------------------------------

		void OnEnable()
		{
			Init();
		}

		void Init()
		{
			mGradient = new Gradient();
			mGradient.colorKeys = new[] { new GradientColorKey(Color.black, 0.49f), new GradientColorKey(Color.white, 0.51f) };
			mGradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

			m2dGradients = new Gradient[]
			{
				new Gradient()
				{
					colorKeys = new[] { new GradientColorKey(Color.black, 0.49f), new GradientColorKey(Color.white, 0.51f) },
					alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
				},
				new Gradient()
				{
					colorKeys = new[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f) },
					alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
				},
			};

			serializedObject = new SerializedObject(this);
			gradientProperty = serializedObject.FindProperty("mGradient");

			var sp2d = serializedObject.FindProperty("m2dGradients");
			gradients2dList = new ReorderableList(serializedObject, sp2d);
			gradients2dList.elementHeight = 18;
			gradients2dList.draggable = true;
			gradients2dList.drawHeaderCallback = (Rect rect) => { GUI.Label(rect, "Gradients"); };
			gradients2dList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
				var sp = sp2d.GetArrayElementAtIndex(index);
				EditorGUI.PropertyField(rect, sp, GUIContent.none);
			};
			gradients2dList.onReorderCallback = (ReorderableList list) => UpdateGradientPreview();
			gradients2dList.onChangedCallback = (ReorderableList list) => UpdateGradientPreview();
		}

		void InitEditMode(Texture2D texture, Material[] materials)
		{
			textureIsDirty = false;
			editMode = true;
			linkedTexture = texture;
			linkedImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			linkedMaterials = materials;
			var gradientList = GradientManager.GetGradientsFromUserData(linkedImporter.userData);

			if (gradientList.Count == 1)
			{
				mGradient = gradientList[0];
				editedTextureIs2d = false;
			}
			else
			{
				m2dGradients = gradientList.ToArray();
				editedTextureIs2d = true;
			}

			UpdateGradientPreview();
		}

		void OnDestroy()
		{
			if (textureIsDirty)
			{
				if (EditorUtility.DisplayDialog("Edited Ramp Texture", "There are pending edits on the following ramp texture:\n\n" + linkedTexture.name + "\n\nSave them?", "Yes", "Discard"))
				{
					SaveEditedTexture();
				}
				else
				{
					DiscardEditedTexture();
				}
			}
		}

		int tabIndex = 0;
		Vector2 scrollPosition;
		SerializedObject serializedObject;
		ReorderableList gradients2dList;
		SerializedProperty gradientProperty;

		bool isRamp1d { get { return !isRamp2d; } }
		bool isRamp2d { get { return (editMode && editedTextureIs2d) || (!editMode && tabIndex == 1); } }

		void OnGUI()
		{
			TCP2_GUI.UseNewHelpIcon = true;

			EditorGUILayout.BeginHorizontal();
			{
				TCP2_GUI.HeaderBig(editMode ? "TCP 2 - RAMP EDITOR" : "TCP 2 - RAMP GENERATOR");
				TCP2_GUI.HelpButton("Ramp Generator");
			}
			EditorGUILayout.EndHorizontal();
			TCP2_GUI.Separator();

			serializedObject.Update();

			if (editMode)
			{
				if (!isNewTexture)
				{
					var msg = "This will affect <b>all materials</b> that use this texture!" +
						(editModeFromMaterial ? "\n\nSave as a new texture first if you want to affect this material only." : "\n\nSave as a new texture if you want to keep the original ramp.");
					EditorGUILayout.LabelField(GUIContent.none, new GUIContent(msg, TCP2_GUI.GetHelpBoxIcon(MessageType.Warning)), TCP2_GUI.HelpBoxRichTextStyle);
				}

				var rect = EditorGUILayout.GetControlRect(GUILayout.Height(16f));
				var lw = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 50f;
				var enabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUI.ObjectField(rect, "Editing: ", linkedTexture, typeof(Texture2D), false);
				EditorGUIUtility.labelWidth = lw;
				GUI.enabled = enabled;
			}
			else
			{
				/*
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Toggle(tabIndex == 0, "1D RAMP", TCP2_GUI.Tab))
					tabIndex = 0;
				if (GUILayout.Toggle(tabIndex == 1, "2D RAMP", TCP2_GUI.Tab))
					tabIndex = 1;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				TCP2_GUI.SeparatorSimple();
				*/
			}

			if (isRamp1d)
			{
				GUILayout.Label("Click on the gradient to edit it:");
				EditorGUILayout.PropertyField(gradientProperty, GUIContent.none);
			}
			else
			{
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
				gradients2dList.DoLayoutList();
				EditorGUILayout.EndScrollView();
			}

			if (!editMode)
			{
				if (isRamp1d)
				{
					textureWidth = EditorGUILayout.IntField("TEXTURE SIZE:", textureWidth);
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("64", EditorStyles.miniButtonLeft)) textureWidth = 64;
					if (GUILayout.Button("128", EditorStyles.miniButtonMid)) textureWidth = 128;
					if (GUILayout.Button("256", EditorStyles.miniButtonMid)) textureWidth = 256;
					if (GUILayout.Button("512", EditorStyles.miniButtonMid)) textureWidth = 512;
					if (GUILayout.Button("1024", EditorStyles.miniButtonRight)) textureWidth = 1024;
					EditorGUILayout.EndHorizontal();
				}
				else if (isRamp2d)
				{
					GUILayout.BeginHorizontal();
					textureWidth = EditorGUILayout.IntField("TEXTURE SIZE:", textureWidth);
					GUILayout.Label("x");
					textureHeight = EditorGUILayout.IntField(GUIContent.none, textureHeight);
					GUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("64", EditorStyles.miniButtonLeft)) textureWidth = 64;
					if (GUILayout.Button("128", EditorStyles.miniButtonMid)) textureWidth = 128;
					if (GUILayout.Button("256", EditorStyles.miniButtonMid)) textureWidth = 256;
					if (GUILayout.Button("512", EditorStyles.miniButtonMid)) textureWidth = 512;
					if (GUILayout.Button("1024", EditorStyles.miniButtonRight)) textureWidth = 1024;
					GUILayout.Space(8);
					if (GUILayout.Button("64", EditorStyles.miniButtonLeft)) textureHeight = 64;
					if (GUILayout.Button("128", EditorStyles.miniButtonMid)) textureHeight = 128;
					if (GUILayout.Button("256", EditorStyles.miniButtonMid)) textureHeight = 256;
					if (GUILayout.Button("512", EditorStyles.miniButtonMid)) textureHeight = 512;
					if (GUILayout.Button("1024", EditorStyles.miniButtonRight)) textureHeight = 1024;
					EditorGUILayout.EndHorizontal();
				}
			}

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();

				mGradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

				if (editMode)
				{
					textureIsDirty = true;

					//Update linked texture
					if (editedTextureIs2d)
					{
						GradientManager.SetPixelsFromGradients(linkedTexture, m2dGradients, linkedTexture.width, linkedTexture.height);
					}
					else
					{
						var pixels = GradientManager.GetPixelsFromGradient(mGradient, linkedTexture.width, linkedTexture.height);
						linkedTexture.SetPixels(pixels);
						linkedTexture.Apply(true, false);
					}
				}
			}

			GUILayout.Space(8f);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (editMode)
			{
				if (GUILayout.Button("Discard", GUILayout.Width(90f), GUILayout.Height(20f)))
				{
					DiscardEditedTexture();
					if (editModeFromMaterial)
						Close();
					else
						OpenTool();
				}
				if (GUILayout.Button("Apply", GUILayout.Width(90f), GUILayout.Height(20f)))
				{
					SaveEditedTexture();
					if (editModeFromMaterial)
						Close();
					else
						OpenTool();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
			}

			var saveButton = false;
			EditorGUI.BeginDisabledGroup(isRamp2d && (m2dGradients == null || m2dGradients.Length < 2));
			if (editMode)
				saveButton = GUILayout.Button("Save as...", EditorStyles.miniButton, GUILayout.Width(120f), GUILayout.Height(16f));
			else
				saveButton = GUILayout.Button("GENERATE", GUILayout.Width(120f), GUILayout.Height(34f));
			EditorGUI.EndDisabledGroup();
			if (saveButton)
			{
				var path = EditorUtility.SaveFilePanel("Save Generated Ramp", GradientManager.LAST_SAVE_PATH, editMode ? linkedTexture.name : "TCP2_CustomRamp", "png");
				if (!string.IsNullOrEmpty(path))
				{
					GradientManager.LAST_SAVE_PATH = Path.GetDirectoryName(path);
					var projectPath = path.Replace(Application.dataPath, "Assets");
					GenerateAndSaveTexture(projectPath, isRamp2d);

					if (editMode)
					{
						var newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(projectPath);
						if (newTexture != null)
						{
							foreach (var mat in linkedMaterials)
							{
								mat.SetTexture("_Ramp", newTexture);
								EditorUtility.SetDirty(mat);
							}
						}

						//Reinitialize edit mode
						InitEditMode(newTexture, linkedMaterials);
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if (!editMode)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Load Texture", EditorStyles.miniButton, GUILayout.Width(120f)))
				{
					LoadTexture();
				}
				EditorGUILayout.EndHorizontal();
			}

			TCP2_GUI.UseNewHelpIcon = false;
		}

		//--------------------------------------------------------------------------------------------------

		//Update Gradient preview through Reflection
		MethodInfo _ClearCacheMethod;
		MethodInfo ClearCacheMethod
		{
			get
			{
				if (_ClearCacheMethod == null)
				{
					var gpc = typeof(MonoScripts).Assembly.GetType("UnityEditorInternal.GradientPreviewCache");
					if (gpc != null)
						_ClearCacheMethod = gpc.GetMethod("ClearCache");
				}
				return _ClearCacheMethod;
			}
		}
		private void UpdateGradientPreview()
		{
			if (ClearCacheMethod != null)
				ClearCacheMethod.Invoke(null, null);
		}

		private void LoadTexture()
		{
			var path = EditorUtility.OpenFilePanel("TCP2 Gradient Texture", GradientManager.LAST_SAVE_PATH, "png");
			if (!string.IsNullOrEmpty(path))
			{
				GradientManager.LAST_SAVE_PATH = Path.GetDirectoryName(path);
				var assetPath = path.Replace(Application.dataPath, "Assets");
				var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
				if (texture != null)
				{
					OpenForEditing(texture, null, false, false);
				}
			}
		}

		private void GenerateAndSaveTexture(string path, bool is2dRamp)
		{
			if (string.IsNullOrEmpty(path))
				return;

			if (is2dRamp)
			{
				GradientManager.SaveGradientTexture2D(m2dGradients, textureWidth, textureHeight, path);
			}
			else
			{
				GradientManager.SaveGradientTexture(mGradient, textureWidth, path);
			}
		}

		private void SaveEditedTexture()
		{
			if (textureIsDirty)
			{
				//Save data to file
				File.WriteAllBytes(Application.dataPath + AssetDatabase.GetAssetPath(linkedTexture).Substring(6), linkedTexture.EncodeToPNG());

				//Update linked texture userData
				if (editedTextureIs2d)
				{
					linkedImporter.userData = GradientManager.GradientToUserData(m2dGradients);
				}
				else
				{
					linkedImporter.userData = GradientManager.GradientToUserData(mGradient);
				}
			}
			textureIsDirty = false;
		}

		private void DiscardEditedTexture()
		{
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(linkedTexture), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
			textureIsDirty = false;
		}
	}
}
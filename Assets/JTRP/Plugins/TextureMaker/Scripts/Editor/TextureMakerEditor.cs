using UnityEngine;
using UnityEditor;
using System.IO;

namespace TextureMaker
{
    public class TextureMakerEditor : EditorWindow
    {
        private GameObject previewObject;
        private Editor previewObjectEditor;
        private Material previewMat;
        private bool createdPreviewObject = false;

        [SerializeField] private bool previewSettingsFoldout = true;
        [SerializeField] private PreviewType previewType = PreviewType.DEFAULT; 
        [SerializeField] private PreviewMode previewMode = PreviewMode.TWO_D;
        private string[] previewModeStrings = {"None", "2D", "3D" };
       
        // Window.
        [SerializeField] private int currentWindowTab = 0;
        [SerializeField] private Color borderColor = Color.black;

        GUILayoutOption[] options = { GUILayout.MaxWidth(450), GUILayout.MinWidth(150), GUILayout.ExpandWidth(true) };
        
        private string[] windowTabsNames = { "Textures", "Tools" };
        private string[] previewTypeNames = { "Texture", "Grayscale", "Normal map", "Sobel operator" };
        
        private Vector2Int previewSize = Vector2Int.one;
        private Texture2D generatedTextureOutput = null;
        private Texture2D toolsTabTextureOutput = null;
        private Texture2D borderTexture = null;

        private static EditorWindow window;

        [SerializeField] private TextureModule textureModule = new TextureModule();
        [SerializeField] private ToolsModule toolsModule = new ToolsModule();

        [MenuItem("Tools/Texture Maker")]
        private static void Init()
        {            
            // Generate a random icon on startup.
            Gradient gradient = new Gradient();
            gradient.mode = GradientMode.Fixed;

            GradientColorKey[] cKeys = new GradientColorKey[3];
            cKeys[0].color = Random.ColorHSV();
            cKeys[0].time = 0.33f;
            cKeys[1].color = Random.ColorHSV();;
            cKeys[1].time = 0.66f;
            cKeys[2].color = Random.ColorHSV();;
            cKeys[2].time = 1.00f;

            GradientAlphaKey[] aKeys = new GradientAlphaKey[1];
            aKeys[0].alpha = 1f;
            aKeys[0].time = 0f;

            gradient.SetKeys(cKeys, aKeys);

            GUIContent content = new GUIContent();
            content.text = "Texture Maker";
            content.image = TextureMaker.FillHorizontal(16, 8, gradient, false);

            window = GetWindow<TextureMakerEditor>();
            window.titleContent = content;
            window.minSize = new Vector2(300, 450);
            window.maxSize = new Vector2(400, 600);
            window.Show();

        }

        void OnEnable()
        {
            // EditorPrefs.DeleteKey("TextureMaker");

            var data = EditorPrefs.GetString("TextureMaker", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            Undo.undoRedoPerformed += OnUndo;

            // TODO: Clean up.
            borderTexture = TextureMaker.FillSolid(8, 8, new Color(0.25f, 0.25f, 0.25f, 1f));
            
            if(currentWindowTab == 0)
                generatedTextureOutput = textureModule.GetTexture(previewType);
            else
                toolsTabTextureOutput = toolsModule.GetTexture(previewType);
        }

        void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("TextureMaker", data);
            
            Undo.undoRedoPerformed -= OnUndo;
        }

        void OnUndo()
        {
            // TODO: Clean up.
            borderTexture = TextureMaker.FillSolid(8, 8, new Color(0.25f, 0.25f, 0.25f, 1f));
            
            if(currentWindowTab == 0)
                generatedTextureOutput = textureModule.GetTexture(previewType);
            else
                toolsTabTextureOutput = toolsModule.GetTexture(previewType);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            Undo.RecordObject(this, "Texture maker settings");

            EditorGUIUtility.labelWidth = 120;

            currentWindowTab = GUILayout.Toolbar(currentWindowTab, windowTabsNames, new GUILayoutOption[] { GUILayout.Height(25) });

            switch(currentWindowTab)
            {
                case 0:
                    OnTexturesTabGUI();
                    break;

                case 1:
                    OnToolsTabGUI();
                    break;

                default:
                    OnTexturesTabGUI();
                    break;
            }

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
        }

        private void OnTexturesTabGUI()
        {            

            // if(GUI.changed)
            // {
            //     if(textureSize.x >= 1024 || textureSize.y >= 1024)
            //     {
            //         if(textureSize.x > textureSize.y)
            //         {
            //             previewSize.x = 1024;
            //             previewSize.y = Mathf.RoundToInt(previewSize.x * ((float)textureSize.y / (float)textureSize.x));
            //         }
            //         else
            //         {
            //             previewSize.y = 1024;
            //             previewSize.x = Mathf.RoundToInt(previewSize.y * ((float)textureSize.x / (float)textureSize.y));
            //         }
            //     }
            //     else
            //     {
            //         previewSize = textureSize;
            //     }

            // }
            
            EditorGUILayout.BeginVertical("box");

            textureModule.DrawTextureControls();
            DrawPreviewControls();
                                
            if(GUI.changed)
            {
                if(previewMode != PreviewMode.NONE)
                    generatedTextureOutput = textureModule.GetTexture(previewType);
            }

            GUILayout.BeginHorizontal();

            if(GUILayout.Button(new GUIContent("Reset", "Resets current settings to default."), GUILayout.Height(40)))
            {
                textureModule.Reset();
                
                if(previewMode != PreviewMode.NONE)
                {
                    generatedTextureOutput = textureModule.GetTexture(previewType);
                }
            }

            if(GUILayout.Button("Save " + previewTypeNames[(int)previewType], GUILayout.Height(40)))
            {
                generatedTextureOutput = textureModule.GetTexture(previewType);

                if(generatedTextureOutput)
                    WriteTextureToDisk(generatedTextureOutput, generatedTextureOutput.name);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if(previewMode != PreviewMode.NONE)
                ShowTexturePreview(generatedTextureOutput);

        }

        private void OnToolsTabGUI()
        {
            EditorGUILayout.BeginVertical("box");

            GUILayout.Space(2);
            EditorGUIUtility.fieldWidth = 45f;
            
            toolsModule.Draw();

            GUILayout.Space(5);

            DrawPreviewControls();
            
            if(GUI.changed)
            {
                if(previewMode != PreviewMode.NONE)
                    toolsTabTextureOutput = toolsModule.GetTexture(previewType);
            }

            GUILayout.Space(5);

            if(GUILayout.Button("Save " + previewTypeNames[(int)previewType], GUILayout.Height(40)))
            {
                if(toolsTabTextureOutput)
                    WriteTextureToDisk(toolsTabTextureOutput, toolsTabTextureOutput.name);
            }

            EditorGUILayout.EndVertical();

            ShowTexturePreview(toolsTabTextureOutput);
        }

        void DrawPreviewControls()
        {
            GUIStyle foldoutStyle = EditorStyles.foldout;
            foldoutStyle.fontStyle = FontStyle.Bold;

            // Preview settings.
            GUILayout.Space(5);
            
            previewSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(previewSettingsFoldout, "Preview settings", foldoutStyle);
            if(previewSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                previewType = (PreviewType)EditorGUILayout.EnumPopup("Preview type", previewType);

                EditorGUI.indentLevel++;
                if(currentWindowTab == 0)
                {
                    textureModule.DrawPreviewControls(previewType);
                }
                else
                {
                    toolsModule.DrawPreviewControls(previewType);
                }

                EditorGUI.indentLevel--;

                previewMode = (PreviewMode)EditorGUILayout.Popup("Live preview", (int)previewMode, previewModeStrings);
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            GUILayout.Space(5);
        }

        /// <summary>
        /// Draws a preview section for the generated texture.
        /// <param name="textureToPreview"> Texture to preview. </param>
        /// </summary>
        void ShowTexturePreview(Texture2D textureToPreview)
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.centeredGreyMiniLabel, options);

            Rect verticalGroupRect = GUILayoutUtility.GetLastRect();

            // Calculate a rect in which the preview texture is drawn in.
            float drawRectX = verticalGroupRect.x + 5;
            float drawRectY = verticalGroupRect.y + verticalGroupRect.height + 5;
            float drawRectMaxWidth = verticalGroupRect.width - 10;
            float drawRectMaxHeight = Mathf.Abs((position.height - (verticalGroupRect.y + verticalGroupRect.height)) - 15);

            Rect drawRect = new Rect(drawRectX, drawRectY, drawRectMaxWidth, drawRectMaxHeight);
            
            // Calculate a rect which acts as a border to the preview texture.
            Rect borderRect = drawRect;
            borderRect.x -= 3;
            borderRect.y -= 3;
            borderRect.width  += 6;
            borderRect.height += 6;
            

            if(previewMode == PreviewMode.THREE_D)
            {
                if(!createdPreviewObject)
                {
                    previewObject = GameObject.Find("_PreviewCube");
                    
                    if(!previewObject)
                    {
                        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        previewObject.name = "_PreviewCube";
                    }

                                        // Standard shader.

                    // previewMat = new Material(Shader.Find("Standard"));
                    // previewMat.EnableKeyword("_MainTex");
                    // previewMat.EnableKeyword("_BumpMap");

                                        // Unlit shader.
                    previewMat = new Material(Shader.Find("Unlit/Texture"));
                    previewMat.SetTexture("_MainTex", textureToPreview);
                    
                    previewObject.GetComponent<Renderer>().material = previewMat;
                    
                    if(previewObjectEditor)
                        DestroyImmediate(previewObjectEditor);
                    
                    previewObjectEditor = Editor.CreateEditor(previewObject);


                    createdPreviewObject = true;
                }

                if(GUI.changed)
                {
                    if(textureToPreview)
                    {                    

                        borderTexture = TextureMaker.FillSolid(8, 8, new Color(0.25f, 0.25f, 0.25f, 1f));

                        previewMat.SetTexture("_MainTex", textureToPreview);

                        DestroyImmediate(previewObjectEditor);
                        previewObjectEditor = Editor.CreateEditor(previewObject);
                    }
                }
    
                if (previewObject != null)
                {
                    GUIStyle bgColor = new GUIStyle();
                    bgColor.normal.background = borderTexture;

                    if(previewObjectEditor)
                        previewObjectEditor.OnInteractivePreviewGUI(drawRect, bgColor);
                }
            }
            else if(previewMode == PreviewMode.TWO_D)
            {
                if(GUI.changed)
                {
                    if(textureToPreview)
                        borderTexture = TextureMaker.FillSolid(textureToPreview.width, textureToPreview.height, new Color(0.25f, 0.25f, 0.25f, 1f));
                }

                if(textureToPreview)
                {
                    // Draws the border image.
                    EditorGUI.DrawTextureTransparent(borderRect, borderTexture, ScaleMode.ScaleToFit);

                    // Draws the preview of the generated image.
                    EditorGUI.DrawTextureTransparent(drawRect, textureToPreview, ScaleMode.ScaleToFit);
                }
            }
        }

        /// <summary>
        /// Saves the texture using dialog box.
        /// <param name="tex"> Texture to save. </param>
        /// <param name="name"> Texture name. </param>
        /// </summary>
        private void WriteTextureToDisk(Texture2D tex, string name)
        {
            string path = "";

            if(tex)
            {
                string directoryPath = Application.dataPath + "/Textures/Generated/";
                
                if(!Directory.Exists(directoryPath))
                    directoryPath = Application.dataPath;

                path = EditorUtility.SaveFilePanel("Save texture...", directoryPath, name, "png");

                if(path.Length > 0)
                {
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    
                    AssetDatabase.Refresh();

                    string[] splits = path.Split(new string[] { "Assets" }, System.StringSplitOptions.None);
                    
                    if(splits.Length > 1)
                        path = "Assets" + splits[1];

                    TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);
                    if(textureImporter)
                    {
                        bool isNormalMap = previewType == PreviewType.NORMAL_MAP;
                        textureImporter.filterMode = tex.filterMode;
                        textureImporter.wrapMode = tex.wrapMode;
                        textureImporter.mipmapEnabled = tex.mipmapCount > 1;
                        textureImporter.sRGBTexture = !isNormalMap;
                        textureImporter.alphaIsTransparency = !isNormalMap;
                        textureImporter.isReadable = true;
                        textureImporter.textureType = isNormalMap? TextureImporterType.NormalMap : TextureImporterType.Default;

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)));

                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TextureMaker
{
    public enum ToolsModuleType { GENERATE_MAPS, BLEND };

    [System.Serializable]
    public class ToolsModule
    {
        [SerializeField] private ToolsModuleType moduleType = ToolsModuleType.GENERATE_MAPS;

        [SerializeField] private IToolsModule blendModule = new BlendModule();
        [SerializeField] private IToolsModule generateMapsModule = new GenerateMapsModule();

        [SerializeField] private bool invert = false;
        [SerializeField] private float strength = 0.5f;

        private string[] toolsTabsNames = { "Generate maps", "Blend" };

        private IToolsModule GetModule()
        {
            switch(moduleType)
            {
                case ToolsModuleType.GENERATE_MAPS:
                    return generateMapsModule;
                case ToolsModuleType.BLEND:
                    return blendModule;
                default:
                    return generateMapsModule;
            }
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            moduleType = (ToolsModuleType)GUILayout.Toolbar((int)moduleType, toolsTabsNames, new GUILayoutOption[] { GUILayout.Height(25), GUILayout.Width(240) });
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GetModule().Draw();
        }

        public void DrawPreviewControls(PreviewType previewType)
        {
            if(previewType == PreviewType.NORMAL_MAP || previewType == PreviewType.SOBEL_OPERATOR)
            {
                invert = EditorGUILayout.Toggle("Invert", invert);
                strength = EditorGUILayout.Slider("Strength", strength, 0.0f, 5.0f);
            }
        }

        public Texture2D GetTexture(PreviewType previewType)
        {
            Texture2D tex = GetModule().GetTexture();
    
            switch(previewType)
            {
                case PreviewType.GRAYSCALE:
                    tex = TextureMaker.MakeGrayscale(tex);
                    break;

                case PreviewType.NORMAL_MAP:
                    tex = TextureMaker.MakeNormalMap(tex, strength, invert);
                    break;
                
                case PreviewType.SOBEL_OPERATOR:
                    tex = TextureMaker.SobelFilter(tex, strength, invert);
                    break;
            }

            return tex;
        }
    }

    [System.Serializable]
    public class BlendModule : IToolsModule
    {
        private List<float> blendingFactorsList = new List<float>();
        private List<Texture2D> texturesList = new List<Texture2D>();
        private List<Texture2D> alphaTexturesList = new List<Texture2D>();

        private bool useAlphaTextures = false;
        private Vector2 scrollPos;

        private GUIStyle boxStyle;

        public BlendModule()
        {
            texturesList.Add(null);
            texturesList.Add(null);
            alphaTexturesList.Add(null);

            blendingFactorsList.Add(0);
        }

        public void Draw()
        {
            boxStyle = new GUIStyle("box");
            boxStyle.normal.background = TextureMaker.FillSolid(8, 8, Color.gray);


            EditorGUILayout.BeginVertical(boxStyle);
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUIStyle.none, GUIStyle.none, EditorStyles.textArea, GUILayout.MinHeight(160));
                
                GUILayout.Space(3);


                for(int i = 0; i < texturesList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        // TODO: Revisit this in the future. enhance the blending functionality?

                        if(i == 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.HelpBox("Textures must be of equal size in order for blending to work.", MessageType.Info);
                                useAlphaTextures = EditorGUILayout.Toggle("Use alpha textures", useAlphaTextures);
                            }
                            EditorGUILayout.EndVertical();
                        }
                        
                        if(i > 0)
                        {
                                                                // Alpha textures.

                            if(useAlphaTextures)
                            {
                                EditorGUILayout.LabelField(string.Format("Mask {0}", (i)), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                                alphaTexturesList[i - 1] = (Texture2D)EditorGUILayout.ObjectField("", alphaTexturesList[i - 1], typeof(Texture2D), false, GUILayout.Width(80));
                            }
                            else
                            {
                                                                    // Sliders.
                                if(texturesList[i])
                                {
                                    GUILayout.FlexibleSpace();                            
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        EditorGUILayout.LabelField("Blend " + i, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(40));
                                        blendingFactorsList[i - 1] = EditorGUILayout.Slider("", blendingFactorsList[i - 1], 0f, 1f, GUILayout.MinWidth(100));
                                        GUILayout.FlexibleSpace();                            
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                        
                                                                // Actual textures to blend.
                        GUILayout.FlexibleSpace();                                     
                        EditorGUILayout.LabelField(string.Format("Texture {0}", i + 1), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                        texturesList[i] = (Texture2D)EditorGUILayout.ObjectField("", texturesList[i], typeof(Texture2D), false, GUILayout.Width(80));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("Reset"))
                {
                    Reset();
                }

                EditorGUILayout.Space();

                if(GUILayout.Button(" - ", GUILayout.Width(20), GUILayout.Height(20) ))
                {
                    if(texturesList.Count > 2)
                    {
                        texturesList.RemoveAt(texturesList.Count - 1);
                        alphaTexturesList.RemoveAt(alphaTexturesList.Count - 1);
                        blendingFactorsList.RemoveAt(blendingFactorsList.Count - 1);
                        scrollPos.y -= 60;
                    }
                }
                
                if(GUILayout.Button(" + ", GUILayout.Width(20), GUILayout.Height(20) ))
                {
                    texturesList.Add(null);
                    alphaTexturesList.Add(null);
                    blendingFactorsList.Add(0);
                    scrollPos.y += 70;
                }
            }
            EditorGUILayout.EndHorizontal();
            
        }        

        public Texture2D GetTexture()
        {
            if(useAlphaTextures)
                return TextureMaker.BlendUsingMaskTextures(texturesList.ToArray(), alphaTexturesList.ToArray());

            return TextureMaker.BlendTextures(texturesList.ToArray(), blendingFactorsList.ToArray());
        }

        // TODO: Clean up??
        public void Reset()
        {
            texturesList.Clear();
            texturesList.Add(null);
            texturesList.Add(null);

            alphaTexturesList.Clear();
            alphaTexturesList.Add(null);

            blendingFactorsList.Clear();
            blendingFactorsList.Add(0);

            useAlphaTextures = false;

            scrollPos = Vector2.zero;
        }
    }

    public class GenerateMapsModule : IToolsModule
    {        
        private Texture2D mapTextureInput = null;

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(string.Format("Texture"), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(40));
            mapTextureInput = (Texture2D)EditorGUILayout.ObjectField("", mapTextureInput, typeof(Texture2D), true, GUILayout.Width(80));
            
            EditorGUILayout.EndHorizontal();
            
            // Set texture's isReadable to true on import.
            if(mapTextureInput && !mapTextureInput.isReadable)
            {
                string path = AssetDatabase.GetAssetPath(mapTextureInput);
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
                if(ti)
                {
                    ti.isReadable = true;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        // TODO: Clean up??
        public Texture2D GetTexture()
        {
            return mapTextureInput? mapTextureInput : Texture2D.whiteTexture;
        }

        // TODO: Clean up??
        public void Reset()
        {
        }
    }
}

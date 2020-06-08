using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TextureMaker
{
    public enum GradientType          { HORIZONTAL, VERTICAL, RADIAL };
    public enum NoiseType             { RANDOM, PERLIN, VORONOI };
    public enum PatternType           { CHECKER, CIRCLES, TILE };
    public enum TextureModuleType     { GRADIENT, NOISE, PATTERN };
    public enum PreviewType           { DEFAULT, GRAYSCALE, NORMAL_MAP, SOBEL_OPERATOR };
    public enum PreviewMode           { NONE, TWO_D, THREE_D };

    // TODO: -- Move each fill function to its class.
    //       -- Move each class to its own file.
    //       -- Do invert colors inside each function instead of blitting it twice.

    [System.Serializable]
    public class TextureModule
    {
        [SerializeField] private TextureModuleType moduleType;
        
        [SerializeField] private NoiseModule noiseModule       = new NoiseModule();
        [SerializeField] private GradientModule gradientModule = new GradientModule();
        [SerializeField] private PatternModule patternModule   = new PatternModule();

        [SerializeField] private bool textureSettingsFoldout = true;

        [SerializeField] private bool invert = false;

        [SerializeField] private Vector2Int textureSize = new Vector2Int(256, 256);

        // Normal map.
        [SerializeField] private float strength = 0.5f;


        private ITextureModule GetModule()
        {
            switch(moduleType)
            {
                case TextureModuleType.GRADIENT:
                    return gradientModule;
                case TextureModuleType.NOISE:
                    return noiseModule;
                case TextureModuleType.PATTERN:
                    return patternModule;
                default:
                    return gradientModule;
            }
        }

        public void DrawTextureControls()
        {
            GUIStyle foldoutStyle = EditorStyles.foldout;
            foldoutStyle.fontStyle = FontStyle.Bold;
    
            textureSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(textureSettingsFoldout, "Settings", foldoutStyle);
            
            if(textureSettingsFoldout)
            {
                EditorGUI.indentLevel++;

                moduleType = (TextureModuleType)EditorGUILayout.EnumPopup("Type", moduleType);

                GetModule().Draw();

                invert = EditorGUILayout.Toggle("Invert", invert);

                textureSize = EditorGUILayout.Vector2IntField("Size", textureSize);

                if(GUI.changed)
                {
                    if(textureSize.x <= 0)
                        textureSize.x = 1;

                    if(textureSize.y <= 0)
                        textureSize.y = 1;
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

    

        public void DrawPreviewControls(PreviewType previewType)
        {
            if(previewType == PreviewType.NORMAL_MAP || previewType == PreviewType.SOBEL_OPERATOR)
            {
                strength = EditorGUILayout.Slider("Strength", strength, 0.0f, 5.0f);
            }
        }

        public Texture2D GetTexture(PreviewType previewType)
        {
            Texture2D tex = GetModule().GetTexture(textureSize);
            
            if(invert)
                TextureMaker.InvertColors(ref tex);

            switch(previewType)
            {
                case PreviewType.GRAYSCALE:
                    tex = TextureMaker.MakeGrayscale(tex);
                    break;

                case PreviewType.NORMAL_MAP:
                    tex = TextureMaker.MakeNormalMap(tex, strength);
                    break;
                
                case PreviewType.SOBEL_OPERATOR:
                    tex = TextureMaker.SobelFilter(tex, strength);
                    break;
            }

            return tex;
        }

        public void Reset()
        {
            invert = false;
            textureSize = new Vector2Int(256, 256);
            GetModule().Reset();
        }
    }

    [System.Serializable]
    public class GradientModule : ITextureModule
    {
        [SerializeField] GradientType gradientType = GradientType.HORIZONTAL;

        [SerializeField] Gradient gradient = new Gradient();

        // Radial.
        [SerializeField] float radialMaskThreshold = 2f;

        [SerializeField] private bool flip = false;

        public void Draw()
        {
            gradientType = (GradientType)EditorGUILayout.EnumPopup("Fill", gradientType);

            gradient = EditorGUILayout.GradientField("Color", gradient);

            if(gradientType == GradientType.RADIAL)
            {
                radialMaskThreshold = EditorGUILayout.FloatField("Mask threshold", radialMaskThreshold);

                if(GUI.changed)
                {
                    if(radialMaskThreshold < 0)
                        radialMaskThreshold = 0;
                }
            }

            flip = EditorGUILayout.Toggle("Flip", flip);

        }

        public Texture2D GetTexture(Vector2Int textureSize)
        {
            switch(gradientType)
            {
                case GradientType.HORIZONTAL:
                    return TextureMaker.FillHorizontal(textureSize.x, textureSize.y, gradient, flip);

                case GradientType.VERTICAL:
                    return TextureMaker.FillVertical(textureSize.x, textureSize.y, gradient, flip);

                case GradientType.RADIAL:
                    return TextureMaker.FillRadial(textureSize.x, textureSize.y, gradient, radialMaskThreshold, flip);

                default:
                    return Texture2D.whiteTexture;
            }
        }

        public void Reset()
        {
            gradient = new Gradient();

            GradientColorKey[] cKeys = new GradientColorKey[2];
            cKeys[0].color = Color.yellow;
            cKeys[0].time = 0.0f;
            cKeys[1].color = new Color(1.0f, 70.0f/255f, 0.0f, 1.0f);
            cKeys[1].time = 1.0f;

            GradientAlphaKey[] aKeys = new GradientAlphaKey[1];
            aKeys[0].alpha = 1.0f;
            aKeys[0].time = 0.0f;

            gradient.SetKeys(cKeys, aKeys);

            // Radial
            radialMaskThreshold = 2f;

            flip = false;
        }
    }

    [System.Serializable]
    public class NoiseModule : ITextureModule
    {

        [SerializeField] NoiseType noiseType = NoiseType.RANDOM;
        
        // Perlin noise.
        [SerializeField] private Gradient gradient = new Gradient();
        [SerializeField] private int octaves = 4;
        [SerializeField] private int seed = 1;
        [SerializeField] private float perlinNoiseScale = 150f;
        [SerializeField] private float perlinNoisePersistence = 0.5f;
        // [SerializeField] private float perlinNoiseFrequency = 2f;
        [SerializeField] private Vector2 perlinNoiseOffset = Vector2.zero;

        // Random noise.
        [SerializeField] private bool useColors = false;

        // Voronoi.
        [SerializeField] private int sites = 10;
        [SerializeField] private VoronoiDistanceMetric voronoiDstType = VoronoiDistanceMetric.Euclidean;
        [SerializeField] private bool renderFlat = false;

        // Utils.
        [SerializeField] private bool flip = false;

        public void Draw()
        {
            noiseType = (NoiseType)EditorGUILayout.EnumPopup("Fill", noiseType);

            if(noiseType == NoiseType.PERLIN)
            {
                gradient = EditorGUILayout.GradientField("Color", gradient);

                octaves = EditorGUILayout.IntSlider("Octaves", octaves, 1, 10);
                perlinNoiseScale = EditorGUILayout.Slider("Scale", perlinNoiseScale, 0.01f, 1000f);
                perlinNoisePersistence = EditorGUILayout.Slider("Persistence", perlinNoisePersistence, 0f, 1f);
                seed = EditorGUILayout.IntSlider("Seed", seed, 1, 100000);
                perlinNoiseOffset = EditorGUILayout.Vector2Field("Offset", perlinNoiseOffset);
                
                flip = EditorGUILayout.Toggle("Flip", flip);
            }
            else if(noiseType == NoiseType.RANDOM)
            {
                seed = EditorGUILayout.IntSlider("Seed", seed, 1, 100000);
                useColors = EditorGUILayout.Toggle("Use colors", useColors);
            }
            else if(noiseType == NoiseType.VORONOI)
            {
                voronoiDstType = (VoronoiDistanceMetric)EditorGUILayout.EnumPopup("Distance type", voronoiDstType);
                sites = EditorGUILayout.IntField("Sites", sites);
                seed = EditorGUILayout.IntSlider("Seed", seed, 1, 100000);
                useColors = EditorGUILayout.Toggle("Use colors", useColors);
                renderFlat = EditorGUILayout.Toggle("Render flat", renderFlat);

                flip = EditorGUILayout.Toggle("Flip", flip);
            }
            
        }

        public Texture2D GetTexture(Vector2Int textureSize)
        {
            switch(noiseType)
            {
                case NoiseType.PERLIN:
                    return TextureMaker.FillPerlinNoise(textureSize.x, textureSize.y, octaves, perlinNoisePersistence, 
                                                                    perlinNoiseScale, perlinNoiseOffset, gradient, seed, flip);
                case NoiseType.RANDOM:
                    return TextureMaker.FillRandomNoise(textureSize.x, textureSize.y, seed, useColors);

                case NoiseType.VORONOI:
                    return TextureMaker.FillVoronoi(textureSize.x, textureSize.y, sites, seed, voronoiDstType, renderFlat, useColors, flip);

                default:
                    return Texture2D.whiteTexture;
            } 
        }

        public void Reset()
        {
            gradient = new Gradient();

            GradientColorKey[] cKeys = new GradientColorKey[2];
            cKeys[0].color = Color.white;
            cKeys[0].time = 0.0f;
            cKeys[1].color = Color.black;
            cKeys[1].time = 1.0f;

            GradientAlphaKey[] aKeys = new GradientAlphaKey[1];
            aKeys[0].alpha = 1.0f;
            aKeys[0].time = 0.0f;

            gradient.SetKeys(cKeys, aKeys);

            // Perlin noise.
            octaves = 4;
            seed = 1;
            perlinNoiseScale = 150f;
            perlinNoisePersistence = 0.5f;
            // perlinNoiseFrequency = 2f;
            perlinNoiseOffset = Vector2.zero;

            // Random noise.
            useColors = false;

            // Voronoi.
            sites = 10;
            voronoiDstType = VoronoiDistanceMetric.Euclidean;
            renderFlat = false;

            flip = false;
        }
    }

    [System.Serializable]
    public class PatternModule : ITextureModule
    {
        // TODO: Add circles pattern with radius option?
        [SerializeField] private PatternType patternType = PatternType.CHECKER;

        [SerializeField] private Color color1     = Color.black;
        [SerializeField] private Color color2     = Color.white;
        [SerializeField] private Vector2Int count = new Vector2Int(4, 4);

        // Tile.
        [SerializeField] private Vector2 padding = new Vector2(4, 4);

        // Circles.
        [SerializeField] private float scale = 1f;

        // Utils.
        [SerializeField] private bool flip = false;

        public void Draw()
        {
            patternType = (PatternType)EditorGUILayout.EnumPopup("Type", patternType);

            color1 = EditorGUILayout.ColorField("Color 1", color1);
            color2 = EditorGUILayout.ColorField("Color 2", color2);

            count = EditorGUILayout.Vector2IntField("Count", count);


            if(GUI.changed)
            {
                if(count.x <= 0)
                    count.x = 1;
            
                if(count.y <= 0)
                    count.y = 1;
            }
            
            if(patternType == PatternType.TILE)
            {
                padding = EditorGUILayout.Vector2Field("Padding", padding);
            }
            else if(patternType == PatternType.CIRCLES)
            {
                scale = EditorGUILayout.Slider("Scale", scale, 1, 100);
            }

            flip = EditorGUILayout.Toggle("Flip", flip);
        }

        public Texture2D GetTexture(Vector2Int textureSize)
        {
            switch(patternType)
            {
                case PatternType.CHECKER:
                    return TextureMaker.FillChecker(textureSize.x, textureSize.y, count, color1, color2, flip);
                case PatternType.CIRCLES:
                    return TextureMaker.FillCircles(textureSize.x, textureSize.y, count, color1, color2, scale, flip);
                case PatternType.TILE:
                    return TextureMaker.FillTile(textureSize.x, textureSize.y,  count, padding, color1, color2, flip);
                default:
                    return Texture2D.whiteTexture;
            }
        }

        public void Reset()
        {
            color1 = Color.black;
            color2 = Color.white;
            count = new Vector2Int(4, 4);
            padding = new Vector2(4, 4);

            scale = 1f;

            flip = false;
        }
    }
}

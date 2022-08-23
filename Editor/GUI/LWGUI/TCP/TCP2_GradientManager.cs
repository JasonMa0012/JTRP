// Toony Colors Pro+Mobile 2
// (c) 2014-2020 Jean Moreno

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Gradient = UnityEngine.Gradient;

// Manages the Gradient Textures created with the Ramp Generator

namespace ToonyColorsPro
{
    namespace Utilities
    {
        public static class GradientManager
        {
            public static string LAST_SAVE_PATH
            {
                get { return EditorPrefs.GetString("TCP2_GradientSavePath", Application.dataPath); }
                set { EditorPrefs.SetString("TCP2_GradientSavePath", value); }
            }

            public static bool CreateAndSaveNewGradientTexture(int width, string unityPath)
            {
                var gradient = new Gradient();
                gradient.colorKeys = new[] { new GradientColorKey(Color.gray, 0.0f), new GradientColorKey(Color.white, 1.0f) };
                gradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

                return SaveGradientTexture(gradient, width, unityPath);
            }

            public static bool SaveGradientTexture(Gradient gradient, int width, string unityPath)
            {
                var ramp = CreateGradientTexture(gradient, width);
                var png = ramp.EncodeToPNG();
                Object.DestroyImmediate(ramp);

                var systemPath = Application.dataPath + "/" + unityPath.Substring(7);
                File.WriteAllBytes(systemPath, png);

                AssetDatabase.ImportAsset(unityPath);
                var ti = AssetImporter.GetAtPath(unityPath) as TextureImporter;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.isReadable = true;
#if UNITY_5_5_OR_NEWER
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.alphaSource = TextureImporterAlphaSource.FromInput;
#else
		ti.textureFormat = TextureImporterFormat.RGBA32;
#endif
                //Gradient data embedded in userData
                ti.userData = GradientToUserData(gradient);
                ti.SaveAndReimport();

                return true;
            }

            public static bool SaveGradientTexture2D(Gradient[] gradients, int width, int height, string unityPath)
            {
                // TODO:
                // - find gradient with highest key count
                // - add intermediate keys for other gradients so that all gradients have same key count
                // - support GradientMode.Fixed?

                Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, true, true);
                SetPixelsFromGradients(texture, gradients, width, height);

                var png = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);

                var systemPath = Application.dataPath + "/" + unityPath.Substring(7);
                File.WriteAllBytes(systemPath, png);

                AssetDatabase.ImportAsset(unityPath);
                var ti = AssetImporter.GetAtPath(unityPath) as TextureImporter;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.isReadable = true;
#if UNITY_5_5_OR_NEWER
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.alphaSource = TextureImporterAlphaSource.None;
#else
		ti.textureFormat = TextureImporterFormat.RGB24;
#endif
                //Gradient data embedded in userData
                ti.userData = GradientToUserData(gradients);
                ti.SaveAndReimport();

                return true;
            }

            public static string GradientToUserData(params Gradient[] gradients)
            {
                if (gradients == null || gradients.Length == 0)
                {
                    return null;
                }

                var output = "";
                foreach (var gradient in gradients)
                {
                    output += "gradient:";

                    for (var i = 0; i < gradient.colorKeys.Length; i++)
                    {
                        output += string.Format(CultureInfo.InvariantCulture, "{0},{1},", ColorToHex(gradient.colorKeys[i].color), gradient.colorKeys[i].time);
                    }
                    output = output.TrimEnd(',');
                    output += ";";
                    for (var i = 0; i < gradient.alphaKeys.Length; i++)
                    {
                        output += string.Format(CultureInfo.InvariantCulture, "{0},{1},", gradient.alphaKeys[i].alpha, gradient.alphaKeys[i].time);
                    }
                    output = output.TrimEnd(',');
#if UNITY_5_5_OR_NEWER
                    output += ";" + gradient.mode;
#endif
                    output += "\n";
                }
                output = output.TrimEnd('\n');

                return output;
            }

            // legacy version
            public static void LegacySetGradientFromUserData(string userData, Gradient gradient)
            {
                var keys = userData.Split('\n');
                if (keys == null || keys.Length < 3 || keys[0] != "GRADIENT")
                {
                    EditorApplication.Beep();
                    Debug.LogError("[TCP2_GradientManager] Invalid Gradient Texture\nMake sure the texture was created with the Ramp Generator.");
                    return;
                }

                var ckData = keys[1].Split('#');
                var colorsKeys = new GradientColorKey[ckData.Length];
                for (var i = 0; i < ckData.Length; i++)
                {
                    var data = ckData[i].Split(',');
                    colorsKeys[i] = new GradientColorKey(HexToColor(data[0]), float.Parse(data[1], CultureInfo.InvariantCulture));
                }
                var akData = keys[2].Split('#');
                var alphaKeys = new GradientAlphaKey[akData.Length];
                for (var i = 0; i < akData.Length; i++)
                {
                    var data = akData[i].Split(',');
                    alphaKeys[i] = new GradientAlphaKey(float.Parse(data[0], CultureInfo.InvariantCulture), float.Parse(data[1], CultureInfo.InvariantCulture));
                }
                gradient.SetKeys(colorsKeys, alphaKeys);

#if UNITY_5_5_OR_NEWER
                if (keys.Length >= 4)
                {
                    gradient.mode = (GradientMode)Enum.Parse(typeof(GradientMode), keys[3]);
                }
#endif
            }

            // new version handling multiple gradients
            public static List<Gradient> GetGradientsFromUserData(string userData)
            {
                if (!userData.StartsWith("gradient:") && !userData.StartsWith("GRADIENT"))
                {
                    EditorApplication.Beep();
                    Debug.LogError("[TCP2_GradientManager] Invalid Gradient Texture\nMake sure the texture was created with the Ramp Generator.");
                    return null;
                }

                try
                {
                    var list = new List<Gradient>();
                    bool legacy = userData.StartsWith("GRADIENT");

                    if (legacy)
                    {
                        var gradient = new Gradient();
                        LegacySetGradientFromUserData(userData, gradient);
                        list.Add(gradient);
                    }
                    else
                    {
                        var split = userData.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var gradientString in split)
                        {
                            list.Add(Deserialize(gradientString));
                        }
                    }

                    return list;
                }
                catch (Exception e)
                {
                    EditorApplication.Beep();
                    Debug.LogError("[TCP2_GradientManager] Couldn't parse gradients from texture\nMake sure the texture was created with the Ramp Generator.\nError:\n" + e.ToString());
                    return null;
                }
            }

            public static Gradient DeserializeGradient(string serializedGradient)
            {
                if (!serializedGradient.StartsWith("gradient:"))
                {
                    EditorApplication.Beep();
                    Debug.LogError("[TCP2_GradientManager] Invalid Gradient Texture\nMake sure the texture was created with the Ramp Generator.");
                    return null;
                }

                return null;
            }

            private static Texture2D CreateGradientTexture(Gradient gradient, int width)
            {
                var height = 4;
                var ramp = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
                var colors = GetPixelsFromGradient(gradient, width, height);
                ramp.SetPixels(colors);
                ramp.Apply(true);
                return ramp;
            }

            public static Color[] GetPixelsFromGradient(Gradient gradient, int width, int height)
            {
                var pixels = new Color[width * height];
                return GetPixelsFromGradient(gradient, width, height, pixels);
            }

            public static Color[] GetPixelsFromGradient(Gradient gradient, int width, int height, Color[] pixels)
            {
                for (var x = 0; x < width; x++)
                {
                    var delta = x / (float)width;
                    if (delta < 0) delta = 0;
                    if (delta > 1) delta = 1;
                    var col = gradient.Evaluate(delta);
                    for (int i = 0; i < height; i++)
                    {
                        pixels[x + i * width] = col;
                    }
                }
                return pixels;
            }

            public static void SetPixelsFromGradients(Texture2D texture, Gradient[] gradients, int width, int height)
            {
                // check color/alpha keys count across gradients
                int colorCount = gradients[0].colorKeys.Length;
                int alphaCount = gradients[0].alphaKeys.Length;

                for (int i = 1; i < gradients.Length; i++)
                {
                    if (gradients[i].colorKeys.Length != colorCount)
                    {
                        Debug.LogError("[TCP2 Ramp Generator] Invalid Gradients: gradients need to have the same number of color/alpha keys to be interpolated properly.");
                        return;
                    }

                    if (gradients[i].alphaKeys.Length != alphaCount)
                    {
                        Debug.LogError("[TCP2 Ramp Generator] Invalid Gradients: gradients need to have the same number of color/alpha keys to be interpolated properly.");
                        return;
                    }
                }

                int blockHeight = Mathf.FloorToInt(height / (float)(gradients.Length - 1));
                var lerpGradient = new Gradient();
                Color[] pixelsBuffer = new Color[width];
                for (int y = 0; y < height; y++)
                {
                    float l = (y % blockHeight) / (float)blockHeight;

                    int rampIndex = Mathf.FloorToInt((y / (float)height) * (gradients.Length - 1));
                    int nextRampIndex = rampIndex + 1;

                    var g1 = gradients[rampIndex];
                    var g2 = gradients[nextRampIndex];

                    var colorKeys = new GradientColorKey[g1.colorKeys.Length];
                    for (int i = 0; i < g1.colorKeys.Length; i++)
                    {
                        colorKeys[i] = new GradientColorKey(
                            Color.Lerp(g1.colorKeys[i].color, g2.colorKeys[i].color, l),
                            Mathf.Lerp(g1.colorKeys[i].time, g2.colorKeys[i].time, l)
                            );
                    }

                    var alphaKeys = new GradientAlphaKey[g1.alphaKeys.Length];
                    for (int i = 0; i < g1.alphaKeys.Length; i++)
                    {
                        alphaKeys[i] = new GradientAlphaKey(
                            Mathf.Lerp(g1.alphaKeys[i].alpha, g2.alphaKeys[i].alpha, l),
                            Mathf.Lerp(g1.alphaKeys[i].time, g2.alphaKeys[i].time, l)
                            );
                    }
                    lerpGradient.SetKeys(colorKeys, alphaKeys);

                    var pixels = GetPixelsFromGradient(lerpGradient, width, 1, pixelsBuffer);
                    texture.SetPixels(0, height - y - 1, width, 1, pixels);
                }
                texture.Apply(false);
            }

            public static string ColorToHex(Color32 color)
            {
                var hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
                return hex;
            }

            public static Color HexToColor(string hex)
            {
                var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }

            //--------------------------------------------------------------------------------------------------
            // Gradient Serialization

            static Gradient Deserialize(string serializedGradient)
            {
                // format:
                // gradient:colorKey0,colorTime0,colorKey1,colorTime1,...;alphaKey0,alphaTime0,alphaKey1,alphaTime1...;gradientMode

                var split = serializedGradient.Substring("gradient:".Length).Split(';');
                var colorKeys = new List<GradientColorKey>();
                var alphaKeys = new List<GradientAlphaKey>();

                // parse color keys:
                var colorKeysStr = split[0].Split(',');
                for (int i = 0; i < colorKeysStr.Length; i += 2)
                {
                    colorKeys.Add(new GradientColorKey(HexToColor(colorKeysStr[i]), float.Parse(colorKeysStr[i + 1], CultureInfo.InvariantCulture)));
                }

                // parse alpha keys:
                var alphaKeysStr = split[1].Split(',');
                for (int i = 0; i < alphaKeysStr.Length; i += 2)
                {
                    alphaKeys.Add(new GradientAlphaKey(float.Parse(alphaKeysStr[i], CultureInfo.InvariantCulture), float.Parse(alphaKeysStr[i + 1], CultureInfo.InvariantCulture)));
                }

                // parse gradient mode:
                GradientMode mode = (GradientMode)Enum.Parse(typeof(GradientMode), split[2]);

                var gradient = new Gradient()
                {
                    colorKeys = colorKeys.ToArray(),
                    alphaKeys = alphaKeys.ToArray(),
                    mode = mode
                };

                return gradient;
            }
        }
    }
}
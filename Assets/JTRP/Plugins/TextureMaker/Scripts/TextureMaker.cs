using System.IO;
using UnityEngine;

namespace TextureMaker
{
    public enum VoronoiDistanceMetric { Euclidean, Manhattan };

    // TODO: Convert all to GetPixel32(), faster and 4x less ram.
    public class TextureMaker
    {
        /// <summary>
        /// Creates a texture and draws a rectangle in it.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="rectWidth"> The width of the rectangle. </param>
        /// <param name="rectHeight"> The height of the rectangle. </param>
        /// <param name="offset"> Offset of the rectangle from the texture's origin. </param>
        /// <param name="gradient"> Color gradient to fill the rectangle with. </param>
        /// <param name="flip"> If true, swaps c1 with c2. </param>
        /// </summary>
        public static Texture2D FillRectangle(int width, int height, int rectWidth, int rectHeight, Vector2Int offset, Gradient gradient, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_rect_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[width * height];

            for(int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[x + y * width] = flip? gradient.Evaluate(x / (float)width) : Color.clear;       
                }
            }

            if(offset.x < 0)
                offset.x = 0;

            if(offset.y < 0)
                offset.y = 0;
            
            int offsetMaxX = offset.x + rectWidth;
            int offsetMaxY = offset.y + rectHeight;

            if(offsetMaxX > width)
            {
                // rectWidth = width - offset.x;
                offset.x = width - rectWidth;
                offsetMaxX = offset.x + rectWidth;
            }
           

            if(offsetMaxY > height)
            {
                // rectHeight = height - offset.y;
                offset.y = height - rectHeight;
                offsetMaxY = offset.y + rectHeight;
            }
          

            for(int y = offset.y; y < offsetMaxY; y++)
            {
                for(int x = offset.x ; x < offsetMaxX; x++)
                {
                    colors[x + y * width] = flip? Color.clear : gradient.Evaluate((x - offset.x) / (float)(rectWidth));
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }
        
        

        /// <summary>
        /// Creates a texture and fills it with colored checker tiles.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="count"> number of tiles in each direction. </param>
        /// <param name="c1"> Color 1. </param>
        /// <param name="c2"> Color 2. </param>
        /// <param name="flip"> If true, swaps c1 with c2. </param>
        /// </summary>
        public static Texture2D FillChecker(int width, int height, Vector2Int count, Color c1, Color c2, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_checker_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            
            Color c = c1;

            if(flip)
            {
                c1 = c2;
                c2 = c;
            }
            
            Color[] colors = new Color[width * height];

            for(int y = 0; y < height; ++y)
            {
                for(int x = 0; x < width; ++x)
                {

                    // TODO: Spread remaining pixels.
                    // TODO: Fix at very low resolution, tiles size aren't equal.
                    
                    int idx = Mathf.FloorToInt((((float)x / width)) * count.x);
                    int idy = Mathf.FloorToInt((((float)y / height)) * count.y);
                    c = ((idx + idy) % 2 == 0)? c2 : c1;

                    colors[x + y * width] = c;   
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with colored circles tiles.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="count"> number of tiles in each direction. </param>
        /// <param name="c1"> Color 1. </param>
        /// <param name="c2"> Color 2. </param>
        /// <param name="flip"> If true, swaps c1 with c2. </param>
        /// </summary>
        public static Texture2D FillCircles(int width, int height, Vector2Int count, Color c1, Color c2, float scale, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_circles_.png";
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            
            if(flip)
            {
                Color c = c1;
                c1 = c2;
                c2 = c;
            }

            Color[] colors = new Color[width * height];            

            float dX = width / (float)count.x;
            float dY = height / (float)count.y;

            float maxCount = (count.x > count.y)? count.x : count.y;

            for(int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xx = (x % dX - dX / 2.0f);
                    float yy = (y % dY - dY / 2.0f);

                    float root = Mathf.Sqrt(xx * xx + yy * yy);
                    
                    colors[x + y * tex.width] = Color.Lerp(c1, c2, Mathf.Abs(Mathf.Sin((root / maxCount) / scale)));
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with colored tiles.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="count"> number of tiles in each direction. </param>
        /// <param name="padding"> padding thickness between tiles in each direction. </param>
        /// <param name="c1"> Color 1. </param>
        /// <param name="c2"> Color 2. </param>
        /// <param name="flip"> If true, swaps c1 with c2. </param>
        /// </summary>
        public static Texture2D FillTile(int width, int height, Vector2Int count, Vector2 padding, Color c1, Color c2, bool flip = false)
        {
            // TODO: Fix tile's padding.
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_tile_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[width * height];
            
            padding.x = Mathf.Round(padding.x);
            padding.y = Mathf.Round(padding.y);

            // TODO: Simplify it.
            float tileWidth =  (float)(width - ((count.x + 1) * padding.x)) / count.x;
            float tileHeight = (float)(height - ((count.y + 1) * padding.y)) / count.y;

            // TODO: Enhance it in the low sized textures.
            int extraPixelsX = width - (count.x * (int)(tileWidth  + ((count.x + 1) * padding.x) / count.x));
            int extraPixelsY = height - (count.y * (int)(tileHeight + ((count.y + 1) * padding.y) / count.y));

            float paddingRatioX = (padding.x) / (tileWidth  + padding.x);
            float paddingRatioY = (padding.y) / (tileHeight + padding.y);

            Color c = c1;
            
            if(flip)
            {
                c1 = c2;
                c2 = c;
            }
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float modX = (x % (tileWidth  + padding.x)) / (tileWidth  + padding.x);
                    float modY = (y % (tileHeight + padding.y)) / (tileHeight + padding.y);

                    if(modX >= paddingRatioX && modY >= paddingRatioY)
                    {
                        c = c1;
                    }
                    else
                    {
                        c = c2;
                    }

                    colors[x + y * width] = c;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with solid circle tiles.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="count"> number of circle tiles in each direction. </param>
        /// <param name="padding"> padding thickness between circle tiles in each direction. </param>
        /// <param name="c1"> Color 1. </param>
        /// <param name="c2"> Color 2. </param>
        /// <param name="flip"> If true, swaps c1 with c2. </param>
        /// </summary>
        public static Texture2D FillCircleTiles(int width, int height, Vector2Int count, Vector2 padding, Color c1, Color c2, bool flip = false)
        {
            // TODO: Fix tile's padding.
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_tile_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[width * height];
            
            padding.x = Mathf.Round(padding.x);
            padding.y = Mathf.Round(padding.y);

            // TODO: Simplify it.
            float tileWidth =  (float)(width - ((count.x + 1) * padding.x)) / count.x;
            float tileHeight = (float)(height - ((count.y + 1) * padding.y)) / count.y;

            // TODO: Enhance it in the low sized textures.
            int extraPixelsX = width - (count.x * (int)(tileWidth  + ((count.x + 1) * padding.x) / count.x));
            int extraPixelsY = height - (count.y * (int)(tileHeight + ((count.y + 1) * padding.y) / count.y));

            float paddingRatioX = (padding.x) / (tileWidth  + padding.x);
            float paddingRatioY = (padding.y) / (tileHeight + padding.y);

            Color c = c1;
            
            if(flip)
            {
                c1 = c2;
                c2 = c;
            }
                        
            float r = 0.98f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float modX = ((x % (tileWidth )) / (tileWidth  )) * 2.0f - 1.0f;
                    float modY = ((y % (tileHeight)) / (tileHeight )) * 2.0f - 1.0f;

                    int idx = Mathf.FloorToInt((((float)x / width)) * count.x);
                    int idy = Mathf.FloorToInt((((float)y / height)) * count.y);

                    if(Vector2.Distance(Vector2.zero, new Vector2(modX, modY)) <= r)
                    {
                        c = c1;
                    }
                    else
                    {
                        c = c2;
                    }

                    colors[x + y * width] = c;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with radial gradient.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="gradient"> Color gradient to fill the texture with. </param>
        /// <param name="maskThreshold"> Value to mask the radial shape. </param>
        /// <param name="flip"> If true, flips texture's gradient colors. </param>
        /// </summary>
        public static Texture2D FillRadial(int width, int height, Gradient gradient, float maskThresholdValue, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_radial_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            
            Vector2 texCenter = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);

            Color[] colors = new Color[width * height];

            // Radial gradient.
            for(int y = 0; y < height; ++y)
            {
                for(int x = 0; x < width; ++x)
                {
                    float distFromCenter = Vector2.Distance(texCenter, new Vector2(x, y));
                    // float maskPixel = (0.5f - (distFromCenter / (width - 1))) * maskThresholdValue;
                    float maskPixel = (0.5f - (distFromCenter / width)) * maskThresholdValue;
                    // float blend = ((maskPixel * 2) / maskThresholdValue); // Normalize the value. (0 -> 1)
                    colors[x + y * width] = flip? gradient.Evaluate(1 - maskPixel) : gradient.Evaluate(maskPixel);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            
            return tex;
        }
        
        /// <summary>
        /// Creates a texture and fills it with horizontal gradient.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="gradient"> Color gradient to fill the texture with. </param>
        /// <param name="flip"> If true, flips texture horizontally. </param>
        /// </summary>
        public static Texture2D FillHorizontal(int width, int height, Gradient gradient, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_horizontal_.png";
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] colors = new Color[width * height];
            
            // Horizontal gradient.
            if(flip)
            {
                for(int y = 0; y < height; ++y)
                {
                    for(int x = 0; x < width; ++x)
                    {
                        float blend = (float)(x + 0.5f) / (float)width;
                        colors[x + y * width] = gradient.Evaluate(1 - blend);
                    }
                }
            }
            else 
            {
                for(int y = 0; y < height; ++y)
                {
                    for(int x = 0; x < width; ++x)
                    {
                        float blend = (float)(x + 0.5f) / (float)width;
                        colors[x + y * width] = gradient.Evaluate(blend);
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with vertical gradient.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="gradient"> Color gradient to fill the texture with. </param>
        /// <param name="flip"> If true, flips texture vertically. </param>
        /// </summary>
        public static Texture2D FillVertical(int width, int height, Gradient gradient, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_vertical_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            
            Color[] colors = new Color[width * height];

            // Vertical gradient.
            if(flip)
            {
                for(int y = 0; y < height; ++y)
                {
                    for(int x = 0; x < width; ++x)
                    {
                        float blend = (float)y / ((float)height - 1);
                        colors[x + y * width] = gradient.Evaluate(blend);
                    }
                }
            }
            else
            {
                for(int y = 0; y < height; ++y)
                {
                    for(int x = 0; x < width; ++x)
                    {
                        float blend = (float)y / ((float)height - 1);
                        colors[x + y * width] = gradient.Evaluate(1 - blend);
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with solid color.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="color"> Color to fill the texture with. </param>
        /// </summary>
        public static Texture2D FillSolid(int width, int height, Color color)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = "tex_solid_.png";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[width * height];

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    colors[x + y * width] = color;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with random values.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="seed"> The seed values used to init the random number generator. </param>
        /// <param name="useColors"> If true, Fills textures with colors. </param>
        /// </summary>
        public static Texture2D FillRandomNoise(int width, int height, int seed = 1, bool useColors = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = "tex_random_noise_.png";

            Color[] colors = new Color[width * height];

            Random.InitState(seed);

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if(useColors)
                        colors[x + y * width] = new Color(Random.value, Random.value, Random.value, 1.0f);
                    else
                    {
                        float v = Random.value;
                        colors[x + y * width] = new Color(v, v, v, 1.0f);
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with perlin noise values.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="octaves"> Number of octaves to sample the current point. </param>
        /// <param name="persistence"> Value from 0 to 1 of how much to preserve details of the current point. </param>
        /// <param name="noiseScale"> Scale of the current noise map. </param>
        /// <param name="offset"> Offset of the current noise map. </param>
        /// <param name="gradient"> Color gradient to fill the texture with. </param>
        /// <param name="seed"> The seed values used to init the random number generator. </param>
        /// <param name="flip"> If true, flips texture's gradient colors. </param>
        /// </summary>
        public static Texture2D FillPerlinNoise(int width, int height, int octaves, float persistence, float noiseScale, Vector2 offset, Gradient gradient, int seed = 1, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_perlin_.png";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            
            Color[] colors = new Color[width * height];
            float[] noiseValues = new float[width * height];

            System.Random rn = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for(int i = 0; i < octaves; i++)
            {
                float offsetX = rn.Next(-100000, 100000) + offset.x;
                float offsetY = rn.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxNoiseValue = float.MinValue;
            float minNoiseValue = float.MaxValue;

            // Perlin noise.
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseValue = 0f;

                    for(int i = 0; i < octaves; i++)
                    {
                        float noiseX = (((x - width / 2) / noiseScale) + octaveOffsets[i].x) * frequency;
                        float noiseY = (((y - height / 2) / noiseScale) + octaveOffsets[i].y) * frequency;

                        float perlinValue = Mathf.PerlinNoise(noiseX, noiseY);
                        noiseValue += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= 2;
                    }
                    
                    if(noiseValue > maxNoiseValue)
                        maxNoiseValue = noiseValue;
                    else if(noiseValue < minNoiseValue)
                        minNoiseValue = noiseValue;

                    noiseValues[x + y * width] = noiseValue;

                }
            }
            
            for(int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noiseValue = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseValues[x + y * width]);
                    colors[x + y * width] = flip? gradient.Evaluate(1 - noiseValue) : gradient.Evaluate(noiseValue);                    
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Creates a texture and fills it with voronoi diagram.
        /// <param name="width"> The width of the texture. </param>
        /// <param name="height"> The height of the texture. </param>
        /// <param name="sites"> Number of cell sites. </param>
        /// <param name="seed"> The seed values used to init the random number generator. </param>
        /// <param name="dstMetric"> Distance metric used to calculate distance to each cell. </param>
        /// <param name="renderFlat"> If true, renders the cells as flat colors if useColors is used otherwise grayscale. </param>
        /// <param name="useColors"> Appoints a random color to each cell. </param>
        /// <param name="flip"> If true, flips texture's gradient colors. </param>
        /// </summary>
        public static Texture2D FillVoronoi(int width, int height, int sites, int seed, VoronoiDistanceMetric dstMetric, bool renderFlat, bool useColors, bool flip = false)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "tex_voronoi_.png";
            tex.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[width * height];
            Color c = Color.white;

            Random.InitState(seed);

            Vector2[] points = new Vector2[sites];
            Color[] cellColors = new Color[sites];
            for(int i = 0; i < sites; i++)
            {
                float x = Random.Range(-1.0f, 1.0f);
                float y = Random.Range(-1.0f, 1.0f);
                points[i] = new Vector2(x, y);
                cellColors[i] = Random.ColorHSV();
            }

            Vector2 operand = Vector2.zero;

            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    float dist = float.MaxValue;
                    int cellIndex = 0;

                    for(int i = 0; i < sites; i++)
                    {
                        float xOperand = flip? 1.0f - (x / (width  - 1.0f)) : (x / (width  - 1.0f));
                        float yOperand = flip? 1.0f - (y / (height - 1.0f)) : (y / (height - 1.0f));
                        operand.x = xOperand * 2.0f - 1.0f;
                        operand.y = yOperand * 2.0f - 1.0f;

                        float tDist = 100;

                        switch(dstMetric)
                        {
                            case VoronoiDistanceMetric.Euclidean:
                                tDist = Vector2.Distance(operand, points[i]);
                                break;
                            case VoronoiDistanceMetric.Manhattan:
                                tDist = Mathf.Abs(operand.x - points[i].x) + Mathf.Abs(operand.y - points[i].y);
                                break;

                            default:
                                tDist = Vector2.Distance(operand, points[i]);
                                break;
                        }

                        if(tDist < dist)
                        {

                            cellIndex = i;

                            dist = tDist;
                        }
                    }
                    
                    if(renderFlat)
                    {
                        if(useColors)
                            c = cellColors[cellIndex];
                        else
                            c.r = c.g = c.b = (float)cellIndex / sites;                        
                    }
                    else
                    {
                        Color dc = dist * Color.white;
                        dc.a = 1.0f;
                        c.r = useColors? Color.Lerp(dc, cellColors[cellIndex], dist).r : dist;
                        c.g = useColors? Color.Lerp(dc, cellColors[cellIndex], dist).g : dist;
                        c.b = useColors? Color.Lerp(dc, cellColors[cellIndex], dist).b : dist;
                    }
                
                    colors[x + y * width] = c;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            return tex;
        }

        /// <summary>
        /// Inverts the colors of the given texture paramter.
        /// <param name="t"> The texture to be inverted. </param>
        /// </summary>
        public static void InvertColors(ref Texture2D t)
        {
            Color[] colors = t.GetPixels();

            for(int i = 0; i < colors.Length; i++)
            {
                colors[i].r = (1 - colors[i].r);                  
                colors[i].g = (1 - colors[i].g);                  
                colors[i].b = (1 - colors[i].b);
            }

            t.SetPixels(colors);
            t.Apply();
        }

        /// <summary>
        /// Creates a grayscale out of the input texture.
        /// <param name="t"> Texture to create a grayscale from. </param>
        /// <returns> Returns the grayscale texture. </returns>
        /// </summary>
        public static Texture2D MakeGrayscale(Texture2D t)
        {
            Texture2D gTex = new Texture2D(t.width, t.height, TextureFormat.RGBA32, t.mipmapCount > 1);
            Color[] colors = t.GetPixels();

            for(int i = 0; i < colors.Length; i++)
            {
                float a = colors[i].a;
                colors[i] = Color.white * colors[i].grayscale;
                colors[i].a = a;
            }
            
            gTex.SetPixels(colors);
            gTex.Apply();
            
            return gTex;
        }

        /// <summary>
        /// Creates a normal map out of the input texture.
        /// <param name="t"> Texture to create a normal map from. </param>
        /// <param name="strength"> Normal map strength. </param>
        /// <returns> Returns the normal map texture. </returns>
        /// </summary>
        public static Texture2D MakeNormalMap(Texture2D t, float strength = 0.5f, bool invert = false)
        {
            // TODO: Blur first so we get rid of noise.
            Texture2D nTex = new Texture2D(t.width, t.height, TextureFormat.RGBA32, t.mipmapCount > 1);
            nTex.alphaIsTransparency = false;

            Color[] origColors = t.GetPixels();
            Color[] colors = new Color[origColors.Length];
            Color c = Color.white;

            Vector3 tangent = new Vector3(1, 0, 0);
            Vector3 biTangent = new Vector3(0, 1, 0);

            for (int y = 0; y < nTex.height; y++) 
            {
                for (int x = 0; x < nTex.width; x++) 
                {
                                        // Slow version.

                    // float rightTopPixel = origTex.GetPixel(x + 1, y - 1).grayscale;
                    // float rightMidPixel = origTex.GetPixel(x + 1, y + 0).grayscale;
                    // float rightBotPixel = origTex.GetPixel(x + 1, y + 1).grayscale;
                    
                    // float midTopPixel   = origTex.GetPixel(x + 0, y - 1).grayscale;
                    // float midMidPixel   = origTex.GetPixel(x + 0, y + 0).grayscale;
                    // float midBotPixel   = origTex.GetPixel(x + 0, y + 1).grayscale;

                    // float leftTopPixel  = origTex.GetPixel(x - 1, y - 1).grayscale;
                    // float leftMidPixel  = origTex.GetPixel(x - 1, y + 0).grayscale;
                    // float leftBotPixel  = origTex.GetPixel(x - 1, y + 1).grayscale;
                                        
                                        // Fast version.                    
                    
                    // TODO: Look into the wrap around issue.

                    int xx = Mathf.Clamp(x, 1, nTex.width  - 2);
                    int yy = Mathf.Clamp(y, 1, nTex.height - 2);

                    int index = xx + yy * nTex.width;

                    float rightTopPixel = origColors[index - nTex.width + 1].grayscale;
                    float rightMidPixel = origColors[index              + 1].grayscale;
                    float rightBotPixel = origColors[index + nTex.width + 1].grayscale;
                    
                    float midTopPixel   = origColors[index - nTex.width + 0].grayscale;
                    float midMidPixel   = origColors[index              + 0].grayscale;
                    float midBotPixel   = origColors[index + nTex.width + 0].grayscale;

                    float leftTopPixel  = origColors[index - nTex.width - 1].grayscale;
                    float leftMidPixel  = origColors[index              - 1].grayscale;
                    float leftBotPixel  = origColors[index + nTex.width - 1].grayscale;

                    float dx = (rightTopPixel + 2 * rightMidPixel + rightBotPixel - leftTopPixel - 2 * leftMidPixel - leftBotPixel)  * strength;
                    float dy = (leftTopPixel  + 2 * midTopPixel   + rightTopPixel - leftBotPixel - 2 * midBotPixel  - rightBotPixel) * strength;

                    tangent.z  =  dx;
                    biTangent.z = -dy;

                    if(invert)
                    {
                        tangent.z *= -1;
                        biTangent.z *= -1;
                    }

                    Vector3 n  = Vector3.Cross(tangent, biTangent).normalized;
                    
                    c.r = (0.5f + n.x * 0.5f);
                    c.g = (0.5f + n.y * 0.5f);
                    c.b = (0.5f + n.z * 0.5f);

                    colors[x + y * nTex.width] = c;           
                }
            }

            nTex.SetPixels(colors);
            nTex.Apply();

            return nTex;
        }

        /// <summary>
        /// Creates a texture with highlighted edges using the sobel filter.
        /// <param name="t"> Texture to run the sobel filter on. </param>
        /// <param name="strength"> Edge highlighter strength. </param>
        /// <returns> Returns the generated texture. </returns>
        /// </summary>
        public static Texture2D SobelFilter(Texture2D t, float strength = 0.5f, bool invert = false)
        {
            // TODO: Add direction and colors???
            Texture2D to = new Texture2D(t.width, t.height, TextureFormat.RGBA32, t.mipmapCount > 1);

            Color[] origColors = t.GetPixels();
            Color[] colors = new Color[t.width * t.height];
            Color c = Color.white;

            for (int y = 0; y < t.height; y++) 
            {
                for (int x = 0; x < t.width; x++) 
                {                                
                    int xx = Mathf.Clamp(x, 1, t.width  - 2);
                    int yy = Mathf.Clamp(y, 1, t.height - 2);

                    int index = xx + yy * t.width;

                    float rightTopPixel = origColors[index - t.width + 1].grayscale;
                    float rightMidPixel = origColors[index           + 1].grayscale;
                    float rightBotPixel = origColors[index + t.width + 1].grayscale;
                    
                    float midTopPixel   = origColors[index - t.width + 0].grayscale;
                    float midMidPixel   = origColors[index           + 0].grayscale;
                    float midBotPixel   = origColors[index + t.width + 0].grayscale;

                    float leftTopPixel  = origColors[index - t.width - 1].grayscale;
                    float leftMidPixel  = origColors[index           - 1].grayscale;
                    float leftBotPixel  = origColors[index + t.width - 1].grayscale;

                    float dx = (rightTopPixel + 2 * rightMidPixel + rightBotPixel - leftTopPixel - 2 * leftMidPixel - leftBotPixel)  * strength;
                    float dy = (leftTopPixel  + 2 * midTopPixel   + rightTopPixel - leftBotPixel - 2 * midBotPixel  - rightBotPixel) * strength;

                    float magnitude = Mathf.Sqrt(dx * dx + dy * dy);

                    if(invert)
                    {
                        c.r = 1 - magnitude;
                        c.g = 1 - magnitude;
                        c.b = 1 - magnitude;
                    }
                    else
                    {
                        c.r = magnitude;
                        c.g = magnitude;
                        c.b = magnitude;
                    }


                    colors[x + y * t.width] = c;           
                }
            }

            to.SetPixels(colors);
            to.Apply();

            return to;
        }

        /// <summary>
        /// Blends given textures together.
        /// <param name="textures"> Textures to blend. </param>
        /// <param name="blendFactors"> float array of the blend values from 0 to 1 to use on the textures, its size is equal to the total size of the textures - 1. </param>
        /// <returns> Returns a texture of the final blend. </returns>
        /// </summary>
        public static Texture2D BlendTextures(Texture2D[] textures, float[] blendFactors)
        {
            if(textures != null && textures.Length > 0 && textures[0])
            {
                Texture2D tex = new Texture2D(textures[0].width, textures[0].height, TextureFormat.RGBA32, true);
                tex.name = "tex_blended_.png";
                    
                Color[] newColors = textures[0].GetPixels();
                
                for(int i = 1; i < textures.Length; i++)
                {
                    if(textures[i])
                    {
                        Color[] nextLayerColors = textures[i].GetPixels();
                        
                        if(newColors.Length == nextLayerColors.Length)
                        {
                            for(int j = 0; j < newColors.Length; j++)
                            {
                                // TODO: Fix blending values, don't use Color.Lerp.
                                newColors[j] = Color.Lerp(newColors[j], nextLayerColors[j], blendFactors[i - 1] * nextLayerColors[j].a);
                            }
                        }
                    }
                }

                tex.SetPixels(newColors);
                tex.Apply();

                return tex;
            }

            return Texture2D.whiteTexture;
        }

        /// <summary>
        /// Blends given textures together.
        /// <param name="textures"> Textures to blend together. </param>
        /// <param name="alphaTextures"> Textures used as an alpha map. </param>
        /// <returns> Returns the result as a texture. </returns>
        /// </summary>
        public static Texture2D BlendTextures(Texture2D[] textures, Texture2D[] alphaTextures)
        {
            if(textures != null && textures.Length > 0 && textures[0])
            {
                Texture2D tex = new Texture2D(textures[0].width, textures[0].height, TextureFormat.RGBA32, true);
                tex.name = "tex_blended_.png";
                    
                Color[] newColors = textures[0].GetPixels();

                    
                
                for(int i = 1; i < textures.Length; i++)
                {
                    if(textures[i])
                    {
                        if(alphaTextures[i - 1])
                        {
                            // alphaTextures[i - 1] = MakeGrayscale(alphaTextures[i - 1]);
                            Color[] alphaPixels = alphaTextures[i - 1].GetPixels();

                            Color[] nextLayerColors = textures[i].GetPixels();
                            
                            if(newColors.Length == nextLayerColors.Length)
                            {
                                for(int j = 0; j < newColors.Length; j++)
                                {
                                    // TODO: Fix blending values, don't use Color.Lerp.
                                    newColors[j] = Color.Lerp(newColors[j], nextLayerColors[j], alphaPixels[j].a /* * nextLayerColors[j].a */);
                                }
                            }
                        }
                    }
                }

                tex.SetPixels(newColors);
                tex.Apply();

                return tex;
            }

            return Texture2D.whiteTexture;
        }

        /// <summary>
        /// Blends given textures together.
        /// <param name="textures"> Textures to blend together. </param>
        /// <param name="maskTextures"> Textures used as an alpha map. </param>
        /// <returns> Returns the result as a texture. </returns>
        /// </summary>
        public static Texture2D BlendUsingMaskTextures(Texture2D[] textures, Texture2D[] maskTextures)
        {
            if(textures != null && textures.Length > 0 && textures[0])
            {
                Texture2D tex = new Texture2D(textures[0].width, textures[0].height, TextureFormat.RGBA32, true);
                tex.name = "tex_blended_.png";
                    
                Color[] newColors = textures[0].GetPixels();

                    
                
                for(int i = 1; i < textures.Length; i++)
                {
                    if(textures[i])
                    {
                        if(maskTextures[i - 1])
                        {
                            Color[] alphaPixels = MakeGrayscale(maskTextures[i - 1]).GetPixels();

                            Color[] nextLayerColors = textures[i].GetPixels();
                            
                            if(newColors.Length == nextLayerColors.Length)
                            {
                                for(int j = 0; j < newColors.Length; j++)
                                {
                                    // TODO: Fix blending values, don't use Color.Lerp.
                                    newColors[j] = Color.Lerp(newColors[j], nextLayerColors[j], alphaPixels[j].r * nextLayerColors[j].a);
                                }
                            }
                        }
                    }
                }

                tex.SetPixels(newColors);
                tex.Apply();

                return tex;
            }

            return Texture2D.whiteTexture;
        }
    }
}

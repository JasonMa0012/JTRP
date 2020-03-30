using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace JTRP.ShaderDrawer
{
    public class GUIData
    {
        public static Dictionary<string, bool> group = new Dictionary<string, bool>();
    }
    public class LWGUI : ShaderGUI
    {
        public MaterialProperty[] props;
        public MaterialEditor materialEditor;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            this.props = props;
            this.materialEditor = materialEditor;
            base.OnGUI(materialEditor, props);
        }
        public static MaterialProperty FindProp(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = false)
        {
            return FindProperty(propertyName, properties, propertyIsMandatory);
        }
    }
    public class Func
    {
        public static void TurnColorDraw(Color useColor, UnityAction action)
        {
            var c = GUI.color;
            GUI.color = useColor;
            if (action != null)
                action();
            GUI.color = c;
        }

        public static string GetKeyWord(string keyWord, string propName)
        {
            string k;
            if (keyWord == "" || keyWord == "__")
            {
                k = propName.ToUpperInvariant() + "_ON";
            }
            else
            {
                k = keyWord.ToUpperInvariant();
            }
            return k;
        }

        public static void Foldout(ref bool display, ref bool value, bool hasToggle, string title, UnityEngine.Object[] materials, string keyWord)
        {
            var style = new GUIStyle("ShurikenModuleTitle");// 背景
            style.font = EditorStyles.boldLabel.font;
            style.fontSize = EditorStyles.boldLabel.fontSize + 3;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 30;
            style.contentOffset = new Vector2(50f, 0f);

            var rect = GUILayoutUtility.GetRect(16f, 25f, style);// 范围
            rect.yMin -= 10;
            rect.yMax += 10;
            GUI.Box(rect, "", style);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);// 字体
            titleStyle.fontSize += 2;

            EditorGUI.PrefixLabel(
                new Rect(
                    hasToggle ? rect.x + 50f : rect.x + 25f,
                    rect.y + 6f, 13f, 13f),// title位置
                new GUIContent(title),
                titleStyle);

            var triangleRect = new Rect(rect.x + 4f, rect.y + 8f, 13f, 13f);// 三角

            var toggleRect = new Rect(triangleRect.x + 20f, triangleRect.y + 0f, 13f, 13f);

            var e = Event.current;
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(triangleRect, false, false, display, false);
                if (hasToggle)
                    GUI.Toggle(toggleRect, value, "");
            }

            if (hasToggle && e.type == EventType.MouseDown && toggleRect.Contains(e.mousePosition))
            {
                value = !value;
                foreach (Material item in materials)
                {
                    if (value) item.EnableKeyword(keyWord);
                    else item.DisableKeyword(keyWord);
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
        }

        public static MaterialProperty[] GetProperties(MaterialEditor editor)
        {
            if (editor.customShaderGUI != null && editor.customShaderGUI is LWGUI)
            {
                LWGUI gui = editor.customShaderGUI as LWGUI;
                return gui.props;
            }
            else
            {
                Debug.LogWarning("Please add \"CustomEditor \"JTRP.ShaderDrawer.LWGUI\"\" in your shader!");
                return null;
            }
        }

        public static float PowPreserveSign(float f, float p)
        {
            float num = Mathf.Pow(Mathf.Abs(f), p);
            if ((double)f < 0.0)
                return -num;
            return num;
        }
        /*
                public static Color RGBToHSV(Color c)
                {
                    Vector4 K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
                    Vector4 p, q;

                    if (c.b > c.g)
                    {
                        p = new Vector4(c.b, c.g, K.w, K.z);
                    }
                    else
                    {
                        p = new Vector4(c.g, c.b, K.x, K.y);
                    }

                    if (p.x > c.r)
                    {
                        q = new Vector4(p.x, p.y, p.w, c.r);
                    }
                    else
                    {
                        q = new Vector4(c.r, p.y, p.z, p.x);
                    }

                    float d = q.x - Mathf.Min(q.w, q.y);
                    float e = 0.0001f;
                    return new Color(Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x, c.a);
                }
                public static Color HSVToRGB(Color c)
                {
                    Vector4 K = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
                    Vector3 value = new Vector3(c.r, c.r, c.r) + (Vector3)K;
                    value = value - new Vector3((int)value.x, (int)value.y, (int)value.z);
                    Vector3 p = value * 6.0f - new Vector3(K.w, K.w, K.w);
                    p = new Vector3(Math.Abs(p.x), Math.Abs(p.y), Math.Abs(p.z));
                    var Kx = new Vector3(K.x, K.x, K.x);
                    var Kx01 = p - Kx;
                    Kx01 = new Vector3(Mathf.Clamp01(Kx01.x), Mathf.Clamp01(Kx01.y), Mathf.Clamp01(Kx01.z));
                    var result = c.b * Vector3.Lerp(Kx, Kx01, c.g);
                    return new Color(result.x, result.y, result.z, c.a);
                }
        */

        public static Color RGBToHSV(Color color)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            return new Color(h, s, v, color.a);
        }
        public static Color HSVToRGB(Color color)
        {
            var c = Color.HSVToRGB(color.r, color.g, color.b);
            c.a = color.a;
            return c;
        }

        public static void SetShaderKeyWord(UnityEngine.Object[] materials, string keyWord, bool isEnable)
        {
            foreach (Material m in materials)
            {
                if (m.IsKeywordEnabled(keyWord))
                {
                    if (!isEnable) m.DisableKeyword(keyWord);
                }
                else
                {
                    if (isEnable) m.EnableKeyword(keyWord);
                }
            }
        }

    }

}//namespace ShaderDrawer
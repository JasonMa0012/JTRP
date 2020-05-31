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
        public static Dictionary<string, bool> keyWord = new Dictionary<string, bool>();
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

        public static bool Foldout(ref bool display, bool value, bool hasToggle, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");// BG
            style.font = EditorStyles.boldLabel.font;
            style.fontSize = EditorStyles.boldLabel.fontSize + 3;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 30;
            style.contentOffset = new Vector2(50f, 0f);

            var rect = GUILayoutUtility.GetRect(16f, 25f, style);// Box
            rect.yMin -= 10;
            rect.yMax += 10;
            GUI.Box(rect, "", style);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);// Font
            titleStyle.fontSize += 2;

            EditorGUI.PrefixLabel(
                new Rect(
                    hasToggle ? rect.x + 50f : rect.x + 25f,
                    rect.y + 6f, 13f, 13f),// title pos
                new GUIContent(title),
                titleStyle);

            var triangleRect = new Rect(rect.x + 4f, rect.y + 8f, 13f, 13f);// triangle

            var clickRect = new Rect(rect);// click
            clickRect.height -= 15f;

            var toggleRect = new Rect(triangleRect.x + 20f, triangleRect.y + 0f, 13f, 13f);

            var e = Event.current;
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(triangleRect, false, false, display, false);
                if (hasToggle)
                {
                    if (EditorGUI.showMixedValue)
                        GUI.Toggle(toggleRect, false, "", new GUIStyle("ToggleMixed"));
                    else
                        GUI.Toggle(toggleRect, value, "");
                }
            }

            if (hasToggle && e.type == EventType.MouseDown && toggleRect.Contains(e.mousePosition))
            {
                value = !value;
                e.Use();
            }
            else if (e.type == EventType.MouseDown && clickRect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            return value;
        }

        public static void PowerSlider(MaterialProperty prop, float power, Rect position, GUIContent label)
        {
            int controlId = GUIUtility.GetControlID("EditorSliderKnob".GetHashCode(), FocusType.Passive, position);
            float left = prop.rangeLimits.x;
            float right = prop.rangeLimits.y;
            float start = left;
            float end = right;
            float value = prop.floatValue;
            float originValue = prop.floatValue;

            if ((double)power != 1.0)
            {
                start = Func.PowPreserveSign(start, 1f / power);
                end = Func.PowPreserveSign(end, 1f / power);
                value = Func.PowPreserveSign(value, 1f / power);
            }

            EditorGUI.BeginChangeCheck();

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;

            Rect position2 = EditorGUI.PrefixLabel(position, label);
            position2 = new Rect(position2.x, position2.y, position2.width - EditorGUIUtility.fieldWidth - 5, position2.height);

            if (position2.width >= 50f)
                value = GUI.Slider(position2, value, 0.0f, start, end, GUI.skin.horizontalSlider, !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed", true, controlId);

            if ((double)power != 1.0)
                value = Func.PowPreserveSign(value, power);

            position.xMin += position.width - SubDrawer.propRight;
            value = EditorGUI.FloatField(position, value);

            EditorGUIUtility.labelWidth = labelWidth;
            if (value != originValue)
                prop.floatValue = Mathf.Clamp(value, Mathf.Min(left, right), Mathf.Max(left, right));
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
                Debug.LogWarning("Please add \"CustomEditor \"JTRP.ShaderDrawer.LWGUI\"\" to the end of your shader!");
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

        public static void SetShaderKeyWord(UnityEngine.Object[] materials, string[] keyWords, int index)
        {
            Debug.Assert(keyWords.Length >= 1 && index < keyWords.Length && index >= 0, $"KeyWords:{keyWords} or Index:{index} Error! ");
            for (int i = 0; i < keyWords.Length; i++)
            {
                SetShaderKeyWord(materials, keyWords[i], index == i);
                if (GUIData.keyWord.ContainsKey(keyWords[i]))
                {
                    GUIData.keyWord[keyWords[i]] = index == i;
                }
                else
                {
                    Debug.LogError("KeyWord not exist! Throw a shader error to refresh the instance.");
                }
            }
        }

        public static bool NeedShow(string group)
        {
            if (group == "" || group == "_")
                return true;
            if (GUIData.group.ContainsKey(group))
            {// 一般sub
                return GUIData.group[group];
            }
            else
            {// 存在后缀，可能是依据枚举的条件sub
                foreach (var prefix in GUIData.group.Keys)
                {
                    if (group.Contains(prefix))
                    {
                        string suffix = group.Substring(prefix.Length, group.Length - prefix.Length).ToUpperInvariant();
                        if (GUIData.keyWord.ContainsKey(suffix))
                        {
                            return GUIData.keyWord[suffix] && GUIData.group[prefix];
                        }
                    }
                }
                return false;
            }
        }
    }

}//namespace ShaderDrawer
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace JTRP.ShaderDrawer
{
    /// <summary>
    /// 创建一个折叠组
    /// group：折叠组，不提供则使用属性名称（非显示名称）
    /// keyword：_为忽略，不填和__为属性名大写 + _ON
    /// style：0 默认关闭；1 默认打开；2 默认关闭无toggle；3 默认打开无toggle
    /// </summary>
    public class MainDrawer : MaterialPropertyDrawer
    {
        bool show = false;
        float height;
        string group;
        string keyWord;
        int style;
        public MainDrawer() : this("") { }
        public MainDrawer(string group) : this(group, "", 0) { }
        public MainDrawer(string group, string keyword) : this(group, keyword, 0) { }
        public MainDrawer(string group, string keyWord, float style)
        {
            this.group = group;
            this.keyWord = keyWord;
            this.style = (int)style;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var value = prop.floatValue == 1.0f;
            string g = group != "" ? group : prop.name;
            show = ((style == 1 || style == 3) && !GUIData.group.ContainsKey(g)) ? true : show;

            Func.Foldout(ref show, ref value,
             style == 0 || style == 1,
             label.text,
             editor.targets,
             Func.GetKeyWord(keyWord, prop.name));

            prop.floatValue = value ? 1.0f : 0.0f;

            if (GUIData.group.ContainsKey(g))
            {
                GUIData.group[g] = show;
            }
            else
            {
                GUIData.group.Add(g, show);
            }
        }
    }

    /// <summary>
    /// 在折叠组内以默认形式绘制属性
    /// group：对应折叠组的title
    /// </summary>
    public class SubDrawer : MaterialPropertyDrawer
    {
        public const int propRight = 80;
        public const int propHeight = 20;
        protected string group = "";
        protected float height;
        protected bool needShow => GUIData.group.ContainsKey(group) && GUIData.group[group];
        protected virtual bool matchPropType => true;
        protected MaterialProperty prop;
        protected MaterialProperty[] props;

        public SubDrawer() { }
        public SubDrawer(string group)
        {
            this.group = group;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            height = position.height;
            this.prop = prop;
            props = Func.GetProperties(editor);
            if (group != "" && group != "_")
            {
                EditorGUI.indentLevel++;
                if (needShow)
                {
                    if (matchPropType)
                        DrawProp(position, prop, label, editor);
                    else
                    {
                        Debug.LogWarning($"{this.GetType()} does not support this MaterialProperty type:'{prop.type}'!");
                        editor.DefaultShaderProperty(prop, label.text);
                    }
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                if (matchPropType)
                    DrawProp(position, prop, label, editor);
                else
                {
                    Debug.LogWarning($"{this.GetType()} does not support this MaterialProperty type:'{prop.type}'!");
                    editor.DefaultShaderProperty(prop, label.text);
                }
            }
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return needShow ? height : -2;
        }
        // 绘制自定义样式属性
        public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            editor.DefaultShaderProperty(prop, label.text);
        }
    }

    /// <summary>
    /// 以单行显示Texture，支持额外属性
    /// group为折叠组title，不填则不加入折叠组
    /// extraPropName为需要显示的额外属性名称
    /// </summary>
    public class TexDrawer : SubDrawer
    {
        public TexDrawer() : this("", "") { }
        public TexDrawer(string group) : this(group, "") { }
        public TexDrawer(string group, string extraPropName)
        {
            this.group = group;
            this.extraPropName = extraPropName;
        }
        protected override bool matchPropType => prop.type == MaterialProperty.PropType.Texture;
        string extraPropName;

        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var r = EditorGUILayout.GetControlRect(true);
            if (extraPropName != "" && extraPropName != "_")
            {
                var p = LWGUI.FindProp(extraPropName, props, true);
                if (p != null)
                {
                    EditorGUI.showMixedValue = p.hasMixedValue;
                    editor.ShaderProperty(r, p, " ");
                }
                EditorGUI.showMixedValue = prop.hasMixedValue;
                editor.TexturePropertyMiniThumbnail(r, prop, label.text, label.tooltip);
            }
            else
            {
                EditorGUI.ColorField(new Rect(-999, 0, 0, 0), new Color(0, 0, 0, 0));
                EditorGUI.showMixedValue = prop.hasMixedValue;
                editor.TexturePropertyMiniThumbnail(r, prop, label.text, label.tooltip);
            }
            EditorGUI.showMixedValue = false;
        }
    }
    /// <summary>
    /// 支持并排最多4个颜色，支持HSV
    /// !!!注意：更改参数需要手动刷新Drawer实例，在shader中随意输入字符引发报错再撤销以刷新Drawer实例
    /// </summary>
    public class ColorDrawer : SubDrawer
    {
        public ColorDrawer(string group, string parameter) : this(group, parameter, "", "", "") { }
        public ColorDrawer(string group, string parameter, string color2) : this(group, parameter, color2, "", "") { }
        public ColorDrawer(string group, string parameter, string color2, string color3) : this(group, parameter, color2, color3, "") { }
        public ColorDrawer(string group, string parameter, string color2, string color3, string color4)
        {
            this.group = group;
            this.parameter = parameter.ToUpperInvariant();
            this.colorStr[0] = color2;
            this.colorStr[1] = color3;
            this.colorStr[2] = color4;
        }
        const string preHSVKeyWord = "_HSV_OTColor";
        protected override bool matchPropType => prop.type == MaterialProperty.PropType.Color;
        bool isHSV => parameter.Contains("HSV");
        bool lastHSV;
        string parameter;
        string[] colorStr = new string[3];
        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Stack<MaterialProperty> cProps = new Stack<MaterialProperty>();
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    cProps.Push(prop);
                    continue;
                }
                var p = LWGUI.FindProp(colorStr[i - 1], props);
                if (p != null && p.type == MaterialProperty.PropType.Color)
                    cProps.Push(p);
            }
            int count = cProps.Count;

            var rect = EditorGUILayout.GetControlRect();

            var p1 = cProps.Pop();
            EditorGUI.showMixedValue = p1.hasMixedValue;
            editor.ColorProperty(rect, p1, label.text);

            for (int i = 1; i < count; i++)
            {
                var cProp = cProps.Pop();
                EditorGUI.showMixedValue = cProp.hasMixedValue;
                Rect r = new Rect(rect);
                var interval = 13 * i * (-0.25f + EditorGUI.indentLevel * 1.25f);
                float w = propRight * (0.8f + EditorGUI.indentLevel * 0.2f);
                r.xMin += r.width - w * (i + 1) + interval;
                r.xMax -= w * i - interval;

                EditorGUI.BeginChangeCheck();
                Color src, dst;
                if (isHSV)
                    src = Func.HSVToRGB(cProp.colorValue.linear).gamma;
                else
                    src = cProp.colorValue;
                var hdr = (prop.flags & MaterialProperty.PropFlags.HDR) != MaterialProperty.PropFlags.None;
                dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, hdr);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isHSV)
                        cProp.colorValue = Func.RGBToHSV(dst.linear).gamma;
                    else
                        cProp.colorValue = dst;
                }
            }
            EditorGUI.showMixedValue = false;
            Func.SetShaderKeyWord(editor.targets, preHSVKeyWord, isHSV);
        }
    }

    /// <summary>
    /// 以SubToggle形式显示float，其他行为与内置Toggle一致
    /// keyword：_为忽略，不填和__为属性名大写 + _ON
    /// </summary>
    public class SubToggleDrawer : SubDrawer
    {
        public SubToggleDrawer(string group) : this(group, "") { }
        public SubToggleDrawer(string group, string keyWord)
        {
            this.group = group;
            this.keyWord = keyWord;
        }
        protected override bool matchPropType => prop.type == MaterialProperty.PropType.Float;
        string keyWord;
        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Toggle(label, prop.floatValue > 0.0f);
            string k = Func.GetKeyWord(keyWord, prop.name);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value ? 1.0f : 0.0f;
                foreach (Material m in editor.targets)
                {
                    if (value)
                        m.EnableKeyword(k);
                    else
                        m.DisableKeyword(k);
                }
            }
            EditorGUI.showMixedValue = false;
        }
    }

    /// <summary>
    /// 同内置PowerSlider
    /// </summary>
    public class SubPowerSliderDrawer : SubDrawer
    {
        public SubPowerSliderDrawer(string group) : this(group, 1) { }
        public SubPowerSliderDrawer(string group, float power)
        {
            this.group = group;
            this.power = Mathf.Clamp(power, 0, float.MaxValue);
        }
        protected override bool matchPropType => prop.type == MaterialProperty.PropType.Range;
        private static int s_SliderKnobHash = "EditorSliderKnob".GetHashCode();
        float power;

        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            int controlId = GUIUtility.GetControlID(s_SliderKnobHash, FocusType.Passive, position);

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

            float l = EditorGUIUtility.labelWidth * 0.47f;// 宽度
            Rect position1 = EditorGUILayout.GetControlRect();
            Rect position2 = new Rect(position1);

            position2.xMin += l;
            position2.xMax -= propRight - 10;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            EditorGUI.PrefixLabel(position1, label);
            value = GUI.Slider(position2, value, 0.0f, start, end, GUI.skin.horizontalSlider, !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed", true, controlId);

            if ((double)power != 1.0)
                value = Func.PowPreserveSign(value, power);

            position1.xMin += position1.width - propRight;
            value = EditorGUI.FloatField(position1, value);

            if (value != originValue)
                prop.floatValue = Mathf.Clamp(value, Mathf.Min(left, right), Mathf.Max(left, right));
            EditorGUI.showMixedValue = false;
        }
    }

    public class TitleDecorator : SubDrawer
    {
        private readonly string header;

        public TitleDecorator(string group, string header)
        {
            this.group = group;
            this.header = header;
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (needShow)
                return 24f;
            else
                return 0;
        }

        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
            s.fontSize += 1;
            var r = EditorGUILayout.GetControlRect(true, 24);
            r.yMin += 5;
            EditorGUI.PrefixLabel(r, new GUIContent(header), s);
        }

    }

}//namespace ShaderDrawer

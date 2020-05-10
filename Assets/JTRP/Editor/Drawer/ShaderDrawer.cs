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
            EditorGUI.showMixedValue = prop.hasMixedValue;
            string g = group != "" ? group : prop.name;
            var lastShow = GUIData.group.ContainsKey(g) ? GUIData.group[g] : true;
            show = ((style == 1 || style == 3) && lastShow) ? true : show;

            bool result = Func.Foldout(ref show, value, style == 0 || style == 1, label.text);
            EditorGUI.showMixedValue = false;

            if (result != value)
            {
                prop.floatValue = result ? 1.0f : 0.0f;
                Func.SetShaderKeyWord(editor.targets, Func.GetKeyWord(keyWord, prop.name), result);
            }
            else// 有时会出现toggle激活key却未激活的情况
            {
                if (!prop.hasMixedValue)
                    Func.SetShaderKeyWord(editor.targets, Func.GetKeyWord(keyWord, prop.name), result);
            }

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
    /// group：父折叠组的group key，支持后缀KWEnum或SubToggle的KeyWord以根据enum显示
    /// </summary>
    public class SubDrawer : MaterialPropertyDrawer
    {
        public static readonly int propRight = 80;
        public static readonly int propHeight = 20;
        protected string group = "";
        protected float height;
        protected bool needShow => Func.NeedShow(group);
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
    /// n为显示的name，k为对应KeyWord，float值为当前激活的数组index
    /// </summary>
    public class KWEnumDrawer : SubDrawer
    {
        #region 
        public KWEnumDrawer(string group, string n1, string k1)
        : this(group, new string[1] { n1 }, new string[1] { k1 }) { }
        public KWEnumDrawer(string group, string n1, string k1, string n2, string k2)
        : this(group, new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }
        public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3)
        : this(group, new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }
        public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
        : this(group, new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }
        public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
        : this(group, new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }
        public KWEnumDrawer(string group, string[] names, string[] keyWords)
        {
            this.group = group;
            this.names = names;
            for (int i = 0; i < keyWords.Length; i++)
            {
                keyWords[i] = keyWords[i].ToUpperInvariant();
                if (!GUIData.keyWord.ContainsKey(keyWords[i]))
                {
                    GUIData.keyWord.Add(keyWords[i], false);
                }
            }
            this.keyWords = keyWords;
            this.values = new int[keyWords.Length];
            for (int index = 0; index < keyWords.Length; ++index)
                this.values[index] = index;
        }
        #endregion
        protected override bool matchPropType => prop.type == MaterialProperty.PropType.Float;
        string[] names, keyWords;
        int[] values;
        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var rect = EditorGUILayout.GetControlRect();
            int index = (int)prop.floatValue;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            int num = EditorGUI.IntPopup(rect, label.text, index, this.names, this.values);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = (float)num;
            }
            Func.SetShaderKeyWord(editor.targets, keyWords, num);
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
            EditorGUI.ColorField(new Rect(-999, 0, 0, 0), new Color(0, 0, 0, 0));

            var r = EditorGUILayout.GetControlRect();
            MaterialProperty p = null;
            if (extraPropName != "" && extraPropName != "_")
                p = LWGUI.FindProp(extraPropName, props, true);

            if (p != null)
            {
                Rect rect = Rect.zero;
                if (p.type == MaterialProperty.PropType.Range)
                {
                    var w = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 0;
                    rect = MaterialEditor.GetRectAfterLabelWidth(r);
                    EditorGUIUtility.labelWidth = w;
                }
                else
                    rect = MaterialEditor.GetRectAfterLabelWidth(r);

                editor.TexturePropertyMiniThumbnail(r, prop, label.text, label.tooltip);

                var i = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                editor.ShaderProperty(rect, p, string.Empty);
                EditorGUI.indentLevel = i;
            }
            else
            {
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
    /// 以SubToggle形式显示float，KeyWord行为与内置Toggle一致，
    /// keyword：_为忽略，不填和__为属性名大写 + _ON，将KeyWord后缀于group可根据toggle是否显示
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
                Func.SetShaderKeyWord(editor.targets, k, value);
            }
            else
            {
                if (!prop.hasMixedValue)
                    Func.SetShaderKeyWord(editor.targets, k, value);
            }
            if (GUIData.keyWord.ContainsKey(k))
            {
                GUIData.keyWord[k] = value;
            }
            else
            {
                GUIData.keyWord.Add(k, value);
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
        float power;

        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Func.PowerSlider(prop, power, EditorGUILayout.GetControlRect(), label);
            EditorGUI.showMixedValue = false;
        }
    }

    /// <summary>
    /// 绘制float以更改Render Queue
    /// </summary>
    public class QueueDrawer : MaterialPropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            editor.FloatProperty(prop, label.text);
            int queue = (int)prop.floatValue;
            if (EditorGUI.EndChangeCheck())
            {
                queue = Mathf.Clamp(queue, 1000, 5000);
                prop.floatValue = queue;
                foreach (Material m in editor.targets)
                {
                    m.renderQueue = queue;
                }
            }
        }
    }

    /// <summary>
    /// 与本插件共同使用，在不带Drawer的prop上请使用内置Header，否则会错位，
    /// </summary>
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

            EditorGUI.LabelField(r, new GUIContent(header), s);
        }

    }

}//namespace ShaderDrawer

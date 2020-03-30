using UnityEngine;
using UnityEditor;
using System;

namespace MyShaderDrawer
{
    /// <summary>
    /// 在shader Property前面加上[Disable]标签可以使这个Property在inspector中处于灰色禁用状态，禁止编辑；
    /// </summary>
    public class DisableDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            //EditorGUI.BeginDisabledGroup(true);
            //{
            //    editor.DefaultShaderProperty(position, prop, label);
            //}
            //EditorGUI.EndDisabledGroup();

            //unity建议使用DisabledScope会更安全（然而我并不知道为什么更安全）
            using (new EditorGUI.DisabledScope(true))
            {
                editor.DefaultShaderProperty(position, prop, label);
            }

        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return MaterialEditor.GetDefaultPropertyHeight(prop);
        }
    }

    /// <summary>
    /// 在shader Property前面加上[Int]标签可以使这个Property在inspector中显示为整型的输入框，输入的值都只能是整数(忽略小数取整)；
    /// </summary>
    public class IntDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();

            int newValue = EditorGUI.IntField(position, label, (int)prop.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = newValue;
            }

        }

    }


    /// <summary>
    /// 1.在shader Property前面加上[VectorField(name1,name2,name3,name4,labelWidth)]标签可对Vector类型Property每个分量名称自定义；
    /// 2.根据name的数量显示，如[VectorField(name1,name2,labelWidth)]则只会显示前面两个分量
    /// 3.最后一个参数是分量名的宽度，0则会根据name的长度自动设置
    /// </summary>
    internal class VectorFieldDrawer : MaterialPropertyDrawer
    {
        private readonly GUIContent[] labels;

        private float labelWidth = 13f;
        public VectorFieldDrawer(string x, float labelWidth) : this(labelWidth,new string[] { x }) { }
        public VectorFieldDrawer(string x, string y, float labelWidth) : this(labelWidth,new string[] { x, y }) { }
        public VectorFieldDrawer(string x, string y, string z, float labelWidth ) : this(labelWidth,new string[] { x, y, z }) { }
        public VectorFieldDrawer(string x, string y, string z, string w, float labelWidth ) : this(labelWidth,new string[] { x, y, z, w }) { }
        public VectorFieldDrawer(float labelWidth , params string[] labels)
        {
            this.labels = new GUIContent[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                this.labels[i] = new GUIContent(labels[i]);
                this.labelWidth = labelWidth;
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Vector)
            {
                position = EditorGUI.IndentedRect(position);


                var value = prop.vectorValue;

                EditorGUI.BeginChangeCheck();
                {
                    float interval = 2f;//每个分量之间的间距
                    int eCount = labels.Length;
                    float w = (position.width - (eCount - 1) * interval) / eCount;//计算每个分量的宽度
                    Rect nr = new Rect(position) { width = w };

                    //防止影响其他Properties的布局，把需要修改的参数存起来，绘制完自己的UI后复原
                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    int oldIndentLevel = EditorGUI.indentLevel;
                    EditorGUIUtility.labelWidth = labelWidth;
                    EditorGUI.indentLevel = 0;
                    for (int i = 0; i < eCount; i++)
                    {
                        if (labelWidth < 1)
                        {
                            char[] q = labels[i].text.ToCharArray();
                            EditorGUIUtility.labelWidth = q.Length * 7.5f + 10;
                        }

                        value[i] = EditorGUI.FloatField(nr, labels[i], value[i]);
                        nr.x += w + interval;
                    }
                    EditorGUIUtility.labelWidth = oldLabelWidth;
                    EditorGUI.indentLevel = oldIndentLevel;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    prop.vectorValue = value;

                }
                
            }
            else
            {
                editor.DefaultShaderProperty(prop, label.text);
            }
               
        }

    }
}


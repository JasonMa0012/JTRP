using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
namespace MyShaderGUI
{
    public class LitToonGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor is object o)
            {
                Debug.Log(o.GetType());
            }
        }
    }
}

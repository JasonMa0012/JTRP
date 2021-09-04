using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{
    
    [CustomEditor(typeof(TextureMapNode))]
    public class TextureMapNodeEditor : Editor
    {
        SerializedProperty propTextureUpdateMode;
        SerializedProperty propTexture;
        SerializedProperty propTextureUV;
        SerializedProperty propTiling;
        SerializedProperty propOffset;
        SerializedProperty propFlipScreenV;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            propTextureUpdateMode.enumValueIndex = (int)(TextureMapNode.TextureUpdateMode)
                EditorGUILayout.EnumPopup(
                    "Texture Update",
                    (TextureMapNode.TextureUpdateMode)propTextureUpdateMode.enumValueIndex);

            var textureToUndo = propTexture.objectReferenceValue;
            EditorGUILayout.ObjectField(propTexture, new GUIContent("Texture"));

            var texture = propTexture.objectReferenceValue as Texture2D;
            try
            {
                // MEMO: テクスチャのRead/Write Enabledが有効かどうか調べる手段が無いため
                // 試しにGetPixel()を呼んで例外が投げられたらメッセージボックスを表示する
                if (texture != null)
                {
                    texture.GetPixel(0, 0);
                }
            }
            catch(UnityException ex)
            {
                // TBD: エラーメッセージの内容を後で決める
                EditorUtility.DisplayDialog("Texture Load Failed", ex.Message, "OK");
                propTexture.objectReferenceValue = textureToUndo;
            }

            propTextureUV.enumValueIndex = (int)(TextureMapNode.TextureUVSource)
                EditorGUILayout.EnumPopup(
                    "Texture UV",
                    (TextureMapNode.TextureUVSource)propTextureUV.enumValueIndex);

            propTiling.vector2Value = EditorGUILayout.Vector2Field("Tiling", propTiling.vector2Value);
            propOffset.vector2Value = EditorGUILayout.Vector2Field("Offset", propOffset.vector2Value);

            using (new EditorGUI.DisabledGroupScope(propTextureUV.enumValueIndex != (int)TextureMapNode.TextureUVSource.Screen))
            {
                propFlipScreenV.boolValue =
                    EditorGUILayout.Toggle("Flip Screen V", propFlipScreenV.boolValue);
            }


            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            propTextureUpdateMode = serializedObject.FindProperty("TextureUpdate");
            propTexture = serializedObject.FindProperty("HoldingTexture");
            propTextureUV = serializedObject.FindProperty("TextureUV");
            propTiling = serializedObject.FindProperty("Tiling");
            propOffset = serializedObject.FindProperty("Offset");
            propFlipScreenV = serializedObject.FindProperty("FlipScreenV");
        }
                

        [MenuItem("GameObject/Pencil+ 4/Texture Map Node", priority = 20)]
        public static void OpenTextureMapNode(MenuCommand menuCommand)
        {
            EditorCommons.CreateNodeObjectFromMenu<TextureMapNode>(menuCommand);
        }
    }
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace JTRP.Utility
{
	public class UpdateMaterial : EditorWindow
	{
		[MenuItem("JTRP/Update Material Tool")]
		static void Init()
		{
			UpdateMaterial window = (UpdateMaterial) EditorWindow.GetWindow(typeof(UpdateMaterial));
			window.Show();
		}

		private SerializedProperty _dstShader;
		private SerializedProperty _srcProps;
		private SerializedProperty _dstProps;
		private PropPairs _preset;

		private SerializedObject _serializedObject;

		void OnGUI()
		{
			if (_srcProps == null || _dstProps == null || _dstShader == null)
			{
				var obj = ScriptableObject.CreateInstance<PropPairs>();
				var serObj = new UnityEditor.SerializedObject(obj);
				_dstShader = serObj.FindProperty("dstShader");
				_srcProps = serObj.FindProperty("src");
				_dstProps = serObj.FindProperty("dst");
			}

			var p = EditorGUILayout.ObjectField(_preset, typeof(PropPairs), allowSceneObjects: false);
			_preset = p == null ? null : p as PropPairs;

			if (GUILayout.Button("Load") && _preset != null)
			{
				_serializedObject = new UnityEditor.SerializedObject(ScriptableObject.Instantiate(_preset));
				_dstShader = _serializedObject.FindProperty("dstShader");
				_srcProps = _serializedObject.FindProperty("src");
				_dstProps = _serializedObject.FindProperty("dst");
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_dstShader);
			EditorGUILayout.PropertyField(_srcProps);
			EditorGUILayout.PropertyField(_dstProps);

			if (GUILayout.Button("Update"))
			{
				if (Selection.objects.Length == 0 ||
				    _srcProps.arraySize != _dstProps.arraySize ||
				    _dstShader.objectReferenceValue == null)
					return;
				var dstShader = _dstShader.objectReferenceValue as Shader;
				Undo.RecordObjects(Selection.objects, "Update Material");
				foreach (var o in Selection.objects)
				{
					var dstMaterial = (Material) o;
					var srcMaterial = Object.Instantiate(dstMaterial);
					dstMaterial.shader = dstShader;
					for (int i = 0; i < _srcProps.arraySize; i++)
					{
						var srcName = _srcProps.GetArrayElementAtIndex(i).stringValue;
						var dstName = _dstProps.GetArrayElementAtIndex(i).stringValue;
						var srcShader = srcMaterial.shader;
						var srcID = srcShader.FindPropertyIndex(srcName);
						var dstID = dstShader.FindPropertyIndex(dstName);
						if (srcID == -1 || dstID == -1)
							continue;
						var srcType = srcShader.GetPropertyType(srcID);
						var dstType = dstShader.GetPropertyType(dstID);
						if (srcType != dstType)
							continue;
						switch (dstType)
						{
							case ShaderPropertyType.Color:
								dstMaterial.SetColor(dstName, srcMaterial.GetColor(srcName));
								break;
							case ShaderPropertyType.Vector:
								dstMaterial.SetVector(dstName, srcMaterial.GetVector(srcName));
								break;
							case ShaderPropertyType.Range:
							case ShaderPropertyType.Float:
								dstMaterial.SetFloat(dstName, srcMaterial.GetFloat(srcName));
								break;
							case ShaderPropertyType.Texture:
								dstMaterial.SetTexture(dstName, srcMaterial.GetTexture(srcName));
								break;
						}
					}
				}
			}

			if (GUILayout.Button("Save"))
			{
				var preset = ScriptableObject.CreateInstance<PropPairs>();
				preset.dstShader = _dstShader.objectReferenceValue as Shader;
				preset.src = new List<string>();
				for (int i = 0; i < _srcProps.arraySize; i++)
					preset.src.Add(_srcProps.GetArrayElementAtIndex(i).stringValue);
				preset.dst = new List<string>();
				for (int i = 0; i < _dstProps.arraySize; i++)
					preset.dst.Add(_dstProps.GetArrayElementAtIndex(i).stringValue);
				var path = EditorUtility.SaveFilePanelInProject("Save Preset", "UpdateMaterialPreset", "asset",
					"File Path");
				if (!string.IsNullOrEmpty(path))
				{
					if (File.Exists(path))
						AssetDatabase.DeleteAsset(path);
					AssetDatabase.CreateAsset(preset, path);
					AssetDatabase.Refresh();
					_preset = preset;
				}
			}
		}
	}
}
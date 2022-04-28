using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using JTRP.Editor.Utilities;

namespace JTRP.Editor
{
	[CustomEditor(typeof(CameraSync))]
	public class CameraSyncEditor : UnityEditor.Editor
	{
		private static CameraSync _targetCameraSync;
		private static Transform _gameCamera;
		private static SceneView _viewCamera;


		private static Action _onInspectorChanged = () =>
		{
			_targetCameraSync = null;
			_gameCamera = null;
			_viewCamera = null;
		};

		private static bool _canUpdateKey =>
			TimelineEditor.selectedClip != null &&
			_targetCameraSync.root != null &&
			_targetCameraSync.timelinePlayableDirector != null;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			_targetCameraSync = target as CameraSync;
			_gameCamera = _targetCameraSync.transform;
			_viewCamera = SceneView.lastActiveSceneView;

			//Selection.selectionChanged -= _onInspectorChanged;
			//Selection.selectionChanged += _onInspectorChanged;

			GUILayout.Space(10f);
			if (GUILayout.Button("Game To Scene"))
			{
				Undo.RecordObject(_viewCamera, "Game To Scene");
				CameraSync.GameToScene(_gameCamera, _viewCamera);
			}

			GUILayout.Space(10f);
			if (GUILayout.Button("Scene TO Game"))
			{
				Undo.RecordObject(_viewCamera, "Scene To Game");
				CameraSync.SceneToGame(_gameCamera, _viewCamera.camera.transform);
			}

			GUILayout.Space(10f);
			EditorGUI.BeginDisabledGroup(!_canUpdateKey);
			if (GUILayout.Button("Update / Create Key (Shift+Q)"))
			{
				var clip = TimelineEditor.selectedClip.animationClip;
				Undo.RecordObject(clip, "Update / Create Key");

				string[] propertyNames = { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z",
						"m_LocalRotation.x", "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w" };
				float[] values = { _gameCamera.localPosition.x, _gameCamera.localPosition.y, _gameCamera.localPosition.z,
						_gameCamera.localRotation.x, _gameCamera.localRotation.y, _gameCamera.localRotation.z, _gameCamera.localRotation.w };

				var bindings = AnimationUtility.GetCurveBindings(clip).
					Where((binding) =>
					{
						return binding.path.EndsWith(_gameCamera.name) && propertyNames.Contains(binding.propertyName);
					}).ToArray();

				foreach (var binding in bindings)
				{
					float time = (float)(_targetCameraSync.timelinePlayableDirector.time - TimelineEditor.selectedClip.start);
					float value = values[Array.IndexOf(propertyNames, binding.propertyName)];
					var curve = AnimationUtility.GetEditorCurve(clip, binding);
					int index = -1;
					if (curve.keys.Any((k) =>
					{
						index++;
						return Mathf.Abs(k.time - time) < (1.0 / clip.frameRate * 0.5);
					}))
					{
						curve.RemoveKey(index);
					}
					curve.AddKey(new Keyframe(time, value));
					AnimationUtility.SetEditorCurve(clip, binding, curve);
				}

				EditorUtility.SetDirty(clip);
			}
			EditorGUI.EndDisabledGroup();
		}

		[MenuItem("JTRP/Timeline/Update Camera Key #Q")]
		private static void UpdateKey()
		{
			if (!_canUpdateKey) return;


			Debug.Log("Update Key Done！");
		}
	}
}
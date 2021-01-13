using JTRP.Utility;
using UnityEditor;
using UnityEngine;

namespace JTRP.Editor
{
	[CustomEditor(typeof(CameraSync))]
	public class CameraSyncEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Sync Scene From Game"))
			{
				var gameCamera = (target as CameraSync)?.transform;
				var view = SceneView.lastActiveSceneView;
				Undo.RecordObject(view, "Sync Scene From Game");
				view.AlignViewToObject(gameCamera);
			}
		}
	}
}
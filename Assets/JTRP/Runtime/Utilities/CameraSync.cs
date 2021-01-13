using UnityEditor;
using UnityEngine;

namespace JTRP.Utility
{
	[ExecuteInEditMode]
	public class CameraSync : MonoBehaviour
	{
		void LateUpdate()
		{
			var camera = SceneView.lastActiveSceneView.camera.transform;
			transform.position = camera.position;
			transform.rotation = camera.rotation;
		}
	}
}
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace JTRP.Utility
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class CameraSync : MonoBehaviour
	{
		public enum SyncMode
		{
			Automatic,
			SceneToGame,
			GameToScene,
		}

		public SyncMode mode;
		public PlayableDirector timelinePlayableDirector;
		public Transform root;

		private Vector3 _lastGamePos = Vector3.zero;
		private Vector3 _lastViewPos = Vector3.zero;
		private Quaternion _lastGameRot = Quaternion.identity;
		private Quaternion _lastViewRot = Quaternion.identity;
		private double _lastTime = 0;

		void LateUpdate()
		{
			var sceneView = SceneView.lastActiveSceneView;
			var viewCameraTransform = sceneView.camera.transform;

			switch (mode)
			{
				case SyncMode.Automatic:
					if (timelinePlayableDirector == null) break;
					// 时间轴移动时game2scene
					if (timelinePlayableDirector.time != _lastTime)
					{
						GameToScene(transform, sceneView);
					}
					// scene移动时scene2game
					else if (_lastViewRot != _lastGameRot || _lastViewPos != _lastGamePos)
					{
						SceneToGame(transform, viewCameraTransform);
					}

					_lastTime = timelinePlayableDirector.time;
					break;
				case SyncMode.SceneToGame:
					SceneToGame(transform, viewCameraTransform);
					break;
				case SyncMode.GameToScene:
					GameToScene(transform, sceneView);
					break;
			}

			_lastGamePos = transform.position;
			_lastViewPos = viewCameraTransform.position;
			_lastGameRot = transform.rotation;
			_lastViewRot = viewCameraTransform.rotation;

		}

		public static void SceneToGame(Transform gameCamera, Transform viewCamera)
		{
			gameCamera.position = viewCamera.position;
			gameCamera.rotation = viewCamera.rotation;
		}

		public static void GameToScene(Transform gameCamera, SceneView sceneView)
		{
			sceneView.FixNegativeSize();
			sceneView.pivot = gameCamera.position + gameCamera.forward *
				CameraSync.GetPerspectiveCameraDistance(sceneView.size, sceneView.cameraSettings.fieldOfView);
			sceneView.rotation = gameCamera.rotation;
		}

		public static float GetPerspectiveCameraDistance(float objectSize, float fov)
		{
			return objectSize / Mathf.Sin(fov * 0.5f * 0.017453292f);
		}

	}
}
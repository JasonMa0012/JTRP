using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP.RenderPipeline
{
	[ExecuteAlways]
	public class JTRPAntiPerspective : MonoBehaviour
	{
		public Transform centerPoint;

		private Renderer _renderer;

		private Vector3[]             _frustumCorners = new Vector3[4];
		private Rect                  _rect           = new Rect(0, 0, 1, 1);
		private MaterialPropertyBlock _materialPropertyBlock;

		void Init()
		{
			_renderer = GetComponent<Renderer>();
			_materialPropertyBlock = new MaterialPropertyBlock();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= Update;
			UnityEditor.EditorApplication.update += Update;
#endif
		}

		private void Start()
		{
			Init();
		}

		private void OnValidate()
		{
			Init();
		}

		private void OnEnable()
		{
			Init();
		}

		void Update()
		{
			if (centerPoint == null || _renderer == null)
				return;
			var camera = Camera.main;
			var materials = _renderer.sharedMaterials;

			var viewZ = camera.worldToCameraMatrix.MultiplyPoint(centerPoint.position).z;
			// view space, 0 = left-down 1 = left-up 2 = right-up 3 = right-down
			camera.CalculateFrustumCorners(_rect, viewZ, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);

			// var orthoMatrix = Matrix4x4.Ortho(_frustumCorners[0].x, _frustumCorners[2].x, _frustumCorners[0].y,
			//                                   _frustumCorners[1].y, camera.nearClipPlane, camera.farClipPlane);

			_renderer.GetPropertyBlock(_materialPropertyBlock);
			// _materialPropertyBlock.SetMatrix("_orthoMatrix", orthoMatrix);
			_materialPropertyBlock.SetFloat("_AntiPerspectiveW", Vector3.Distance(_frustumCorners[1], _frustumCorners[2]));
			_materialPropertyBlock.SetFloat("_AntiPerspectiveH", Vector3.Distance(_frustumCorners[0], _frustumCorners[1]));
			_renderer.SetPropertyBlock(_materialPropertyBlock);

			for (int i = 0; i < 4; i++)
			{
				var worldSpaceCorner = camera.transform.TransformVector(_frustumCorners[i]);
				Debug.DrawRay(camera.transform.position, worldSpaceCorner, new Color(i * 0.25f, 0f, 0f));
			}

			// foreach (var material in materials)
			// {
			// 	if (material)
			// 	{
			// 		material.SetMatrix("_orthoMatrix", orthoMatrix);
			// 	}
			// }
		}
	}
}
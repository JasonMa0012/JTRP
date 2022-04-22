using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP.Utility
{
	public enum Dir
	{
		forward,
		right,
		up
	}

	[ExecuteAlways]
	public class SphericalShadowHelper : MonoBehaviour
	{
		public Transform  faceCenter;
		public Renderer[] renderers;


		[Header("Projected Light Dir(Need Activate Built-in Light Direction)")]
		public bool enableProjectedLightDir;

		public                Transform      light;
		[Range(-1, 1)] public float          yAxisOffset;
		[Range(-1, 1)] public float          constantY;
		[Range(0, 1)]  public float          constantYBlend;
		public                Dir            forwardDir       = Dir.forward;
		public                bool           invertForwardDir = false;
		public                AnimationCurve dotPower         = AnimationCurve.Linear(-1, 1, 1, 1);


		private MaterialPropertyBlock _block;

		void LateUpdate()
		{
			if (faceCenter == null || renderers == null || renderers.Length == 0) return;
			if (_block == null) _block = new MaterialPropertyBlock();
			foreach (var renderer in renderers)
			{
				renderer.GetPropertyBlock(_block);
				_block.SetMatrix("_SphericalShadowCenter_Matrix_I_M", faceCenter.worldToLocalMatrix);
				_block.SetMatrix("_SphericalShadowCenter_Matrix_M", faceCenter.localToWorldMatrix);
				if (enableProjectedLightDir && light != null)
				{
					Vector3 forwardDirWS = Vector3.zero;
					switch (forwardDir)
					{
						case Dir.forward:
							forwardDirWS = faceCenter.forward;
							break;
						case Dir.right:
							forwardDirWS = faceCenter.right;
							break;
						case Dir.up:
							forwardDirWS = faceCenter.up;
							break;
					}

					forwardDirWS = forwardDirWS * (invertForwardDir ? -1 : 1);
					var forwardWS2D = forwardDirWS;
					forwardWS2D.y = 0;
					forwardWS2D = forwardWS2D.normalized;
					var lightDirWS2D = -light.forward;
					lightDirWS2D.y = 0;
					lightDirWS2D = lightDirWS2D.normalized;

					var dotValue = Mathf.Clamp(Vector3.Dot(forwardWS2D, lightDirWS2D) + yAxisOffset, -1, 1);
					dotValue = Mathf.Pow(Mathf.Abs(dotValue), dotPower.Evaluate(dotValue)) * (dotValue > 0 ? 1 : -1);
					_block.SetFloat("_Offset_Y_Axis_BLD", Mathf.Lerp(dotValue, constantY, constantYBlend));
					_block.SetFloat("_Inverse_Z_Axis_BLD", Vector3.Cross(forwardWS2D, lightDirWS2D).y + yAxisOffset > 0 ? 1 : 0);
				}

				renderer.SetPropertyBlock(_block);
			}
		}
	}
}
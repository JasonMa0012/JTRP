using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JTRP.Utilities
{
	[ExecuteAlways]
	public class SphericalHairHighLightHelper : MonoBehaviour
	{
		public Transform  hairCenter;
		public Renderer[] renderers;
		public string     centerShaderProp   = "_HairCenterWS";
		public string     upDirShaderProp    = "_HeadUpDirWS";
		public string     rightDirShaderProp = "_HeadRightDirWS";

		private MaterialPropertyBlock _block;

		void LateUpdate()
		{
			if (hairCenter == null || renderers == null || renderers.Length == 0) return;
			if (_block == null) _block = new MaterialPropertyBlock();
			foreach (var renderer in renderers)
			{
				renderer.GetPropertyBlock(_block);
				_block.SetVector(centerShaderProp, hairCenter.position);
				_block.SetVector(upDirShaderProp, hairCenter.up);
				_block.SetVector(rightDirShaderProp, hairCenter.right);
				renderer.SetPropertyBlock(_block);
			}
		}
	}
}
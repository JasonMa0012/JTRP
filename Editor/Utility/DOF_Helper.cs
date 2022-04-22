using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.HighDefinition;

namespace UnityEngine.Rendering.HighDefinition
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(Volume))]

	public class DOF_Helper : MonoBehaviour
	{
		public Transform camera;
		public Transform target;
		public float offset = 0;
		public bool isOnScreen = true;
		public float offScreenValue = 15;

		private DepthOfField _dof;
		void Update()
		{
			if (camera == null || target == null) return;
			if (_dof == null)
			{
				var volume = GetComponent<Volume>();
				if (volume == null) return;
				_dof = volume.sharedProfile.components.First((c) => c is DepthOfField) as DepthOfField;
			}
			if (_dof == null) return;
			_dof.focusDistance.value = (isOnScreen ? Vector3.Distance(camera.position, target.position) : offScreenValue) + offset;
		}
	}
}

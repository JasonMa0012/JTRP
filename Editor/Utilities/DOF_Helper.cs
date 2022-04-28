using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.HighDefinition
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(Volume))]

	public class DOF_Helper : MonoBehaviour
	{
		[FormerlySerializedAs("camera")] 
		public Transform mainCamera;
		public  Transform target;
		public  float     offset         = 0;
		public  bool      isOnScreen     = true;
		public  float     offScreenValue = 15;

		private DepthOfField _dof;
		void Update()
		{
			if (mainCamera == null || target == null) return;
			if (_dof == null)
			{
				var volume = GetComponent<Volume>();
				if (volume == null) return;
				_dof = volume.sharedProfile.components.First((c) => c is DepthOfField) as DepthOfField;
			}
			if (_dof == null) return;
			_dof.focusDistance.value = (isOnScreen ? Vector3.Distance(mainCamera.position, target.position) : offScreenValue) + offset;
		}
	}
}

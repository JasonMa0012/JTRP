using UnityEditor;
using UnityEngine;

namespace JTRP.Editor
{
	[CustomEditor(typeof(JTRP.GeometryOutline))]
	public class GeometryOutlineEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GUILayout.Space(20f);
			if (GUILayout.Button("Bake"))
			{
				foreach (var o in targets)
				{
					var item = (GeometryOutline)o;
					item.DoRebake();
				}
			}
		}

	}
}
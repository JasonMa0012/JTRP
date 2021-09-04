using UnityEditor;
using UnityEngine;

namespace JTRP.Editor
{
	[CustomEditor(typeof(GeometryOutline))]
	public class GeometryOutlineEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			GUILayout.Space(20f);
			if (GUILayout.Button("Bake"))
			{
				foreach (GeometryOutline item in targets)
				{
					item.DoRebake();
				}
			}
		}

	}
}
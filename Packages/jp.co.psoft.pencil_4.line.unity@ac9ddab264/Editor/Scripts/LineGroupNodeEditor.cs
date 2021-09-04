using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pencil_4;

namespace Pcl4Editor
{
	[CustomEditor(typeof(LineGroupNode))]
	public class LineGroupNodeEditor : Editor
	{
		private SerializedProperty _propObjects;

		private PencilReorderableList _reorderableObjects;

		private void OnEnable()
		{
			_propObjects = serializedObject.FindProperty("Objects");

			_reorderableObjects =
				EditorCommons.CreateObjectList(
					serializedObject,
					_propObjects,
					new List<GameObject>(),
					selectedObjects =>
					{
						serializedObject.Update();
						_propObjects.AppendObjects(selectedObjects);
						serializedObject.ApplyModifiedProperties();
						_reorderableObjects.index = _reorderableObjects.count - 1;
					});
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var currentNode = target as LineGroupNode;
			
			EditorGUILayout.LabelField("Objects");
			
			var style = new GUIStyle {margin = new RectOffset(4, 8, 0, 4)};
			
			var verticalLayout = new EditorGUILayout.VerticalScope(style);
			
			EditorCommons.DragAndDropObject<GameObject, LineGroupNode>(
				currentNode,
				null,
				_propObjects,
				verticalLayout.rect,
				(node, _) => ((LineGroupNode)node).Objects,
				false);

			using (verticalLayout)
			{
				_reorderableObjects.HandleInputEventAndLayoutList();
			}
			
			serializedObject.ApplyModifiedProperties();
		}
		
		/// <summary>
		/// MenuにLineGroupノードを追加する項目を追加
		/// </summary>
		[MenuItem("GameObject/Pencil+ 4/Line Group Node", priority = 20)]
		public static void OpenLineNode(MenuCommand menuCommand)
		{
			EditorCommons.CreateNodeObjectFromMenu<LineGroupNode>(menuCommand, typeof(LineListNode), "LineList");
		}

	}
}

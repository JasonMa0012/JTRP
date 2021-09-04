using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Pcl4Editor
{
    public class ObjectPickerWindow : PickerWindow<GameObject>
    {
        static ObjectPickerWindow pickerWindow;

        public static void Open(IEnumerable<GameObject> objectsToExclude, Action<List<GameObject>> onAddButtonPushed)
        {
            if (pickerWindow == null)
            {
                pickerWindow = CreateInstance<ObjectPickerWindow>();
                pickerWindow.titleContent = new GUIContent("Select Object");
            }
            pickerWindow.OnAddButtonPushed = onAddButtonPushed;
            pickerWindow.treeView = new PickerTreeView<GameObject>(
                new TreeViewState(),
                () => EnumerateMeshObjects().Where(x => !objectsToExclude.Contains(x)),
                EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D);
            pickerWindow.treeView.ItemDoubleClicked = (x) =>
            {
                onAddButtonPushed(new List<GameObject> { x });
                pickerWindow.Close();
            };
            pickerWindow.treeView.Reload();
            pickerWindow.ShowAuxWindow();
        }

        static IEnumerable<GameObject> EnumerateMeshObjects()
        {
            return Resources.FindObjectsOfTypeAll<Renderer>()
                .Where(x => x is MeshRenderer || x is SkinnedMeshRenderer)
                .Select(x => x.gameObject)
                .Where(x => x.scene.isLoaded);  // シーン中に無いオブジェクトを除く
        }

    }
}




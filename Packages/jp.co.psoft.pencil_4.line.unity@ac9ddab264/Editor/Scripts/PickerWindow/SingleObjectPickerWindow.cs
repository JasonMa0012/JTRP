using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Pcl4Editor
{
    public class SingleObjectPickerWindow : PickerWindow<GameObject>
    {
        static SingleObjectPickerWindow pickerWindow;

        public static void OpenAllObjectPicker(Action<GameObject> onAddButtonPushed)
        {
            Open(onAddButtonPushed, () => EnumerateAllObjects());
        }

        public static void OpenMeshObjectPicker(Action<GameObject> onAddButtonPushed)
        {
            Open(onAddButtonPushed, () => EnumerateMeshObjects());
        }


        static void Open(Action<GameObject> onAddButtonPushed, Func<IEnumerable<GameObject>> objectEnumerator)
        {

            if (pickerWindow == null)
            {
                pickerWindow = CreateInstance<SingleObjectPickerWindow>();
                pickerWindow.titleContent = new GUIContent("Select Object");

            }
            pickerWindow.OnAddButtonPushed = (x) => { onAddButtonPushed(x.FirstOrDefault()); };
            pickerWindow.treeView = new PickerTreeView<GameObject>(
                new TreeViewState(),
                objectEnumerator,
                EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D);
            pickerWindow.treeView.TreeViewCanMultiSelect = false;
            pickerWindow.treeView.ItemDoubleClicked = (x) =>
            {
                onAddButtonPushed(x);
                pickerWindow.Close();
            };
            pickerWindow.treeView.Reload();
            pickerWindow.ShowAuxWindow();
        }

        static IEnumerable<GameObject> EnumerateAllObjects()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(x => x.scene.isLoaded)
                .Where(x => x.GetComponent<Pencil_4.NodeBase>() == null)
                .Where(x => x.GetComponent<RectTransform>() == null)
                .Where(x => x.GetComponent<UnityEngine.EventSystems.EventSystem>() == null);
        }

        static IEnumerable<GameObject> EnumerateMeshObjects()
        {
            return Resources.FindObjectsOfTypeAll<Renderer>()
                .Where(x => x is MeshRenderer || x is SkinnedMeshRenderer)
                .Select(x => x.gameObject)
                .Where(x => x.scene.isLoaded)  // シーン中に無いオブジェクトを除く
                .Concat(new List<GameObject> { null });
        }
    }
}

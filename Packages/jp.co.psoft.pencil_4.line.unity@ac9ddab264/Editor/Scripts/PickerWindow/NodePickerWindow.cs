using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Pencil_4;

namespace Pcl4Editor
{
    // MEMO: ScriptableObject.CreateInstanceの型引数にはジェネリックな型を渡せないため、
    //       Nodeの型はこのクラスの型パラメータではなくOpenメソッドの引数に渡す実装にする。
    public class NodePickerWindow : PickerWindow<GameObject>
    {
        static NodePickerWindow pickerWindow;

        public static void Open(Type nodeType, Action<GameObject> onAddButtonPushed)
        {
            if(!nodeType.IsSubclassOf(typeof(NodeBase)))
            {
                throw new ArgumentException(nodeType.ToString() + " is not a subclass of NodeBase.");
            }

            if (pickerWindow == null)
            {
                pickerWindow = CreateInstance<NodePickerWindow>();
                pickerWindow.titleContent = new GUIContent("Select Node");
            }

            pickerWindow.OnAddButtonPushed = (x) => { onAddButtonPushed(x.FirstOrDefault()); };

            pickerWindow.treeView = new PickerTreeView<GameObject>(
                new TreeViewState(),
                () => EnumerateNodes(nodeType),
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

        static IEnumerable<GameObject> EnumerateNodes(Type type)
        {
            return Resources.FindObjectsOfTypeAll(type)
                .Select(x => (x as Component).gameObject)
                .Where(x => x.scene.isLoaded)
                .Concat(new List<GameObject> { null });
        }

    }
}

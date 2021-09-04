using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Pcl4Editor
{
    public class PickerWindow<T> : EditorWindow
        where T : UnityEngine.Object
    {
        public class PickerTreeView<U> : TreeView
            where U : UnityEngine.Object
        {
            public bool TreeViewCanMultiSelect = true;
            public Action<U> ItemDoubleClicked = (x) => { };

            Dictionary<int, U> treeElements;
            Func<IEnumerable<U>> enumerateTreeElements;
            Texture2D elementIcon;


            public PickerTreeView(
                TreeViewState state,
                Func<IEnumerable<U>> enumerateTreeElements,
                Texture2D elementIcon) : base(state)
            {
                this.enumerateTreeElements = enumerateTreeElements;
                this.elementIcon = elementIcon;
                this.showBorder = true;
            }


            public List<U> SelectedObjects
            {
                get
                {
                    return GetSelection().Select(x => treeElements[x]).ToList();
                }
            }


            protected override TreeViewItem BuildRoot()
            {
                treeElements = enumerateTreeElements()
                    .Select((x, i) => new KeyValuePair<int, U>(i, x))
                    .ToDictionary(x => x.Key, x => x.Value);

                // MEMO: rootオブジェクトは表示されない(rootの1層下にあるオブジェクトから表示対象になる)
                var root = new TreeViewItem(id: -1, depth: -1, displayName: "root");

                root.children = treeElements
                    .OrderBy(x => x.Value, new TreeViewItemComparer())
                    .Select(x => new TreeViewItem(
                        id: x.Key,
                        depth: 0,
                        displayName: (x.Value != null) ? x.Value.name : "None"))
                    .ToList();

                foreach(var element in root.children)
                {
                    element.icon = elementIcon;
                }

                return root;
            }


            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return TreeViewCanMultiSelect;
            }


            protected override void DoubleClickedItem(int id)
            {
                ItemDoubleClicked(treeElements[id]);
            }

            /// <summary>
            /// TreeViewのアイテムをソートするための比較用クラス
            /// (オブジェクトの名前でソートする。nullが含まれる場合は、nullが先頭になるようにソートする)
            /// </summary>
            class TreeViewItemComparer : IComparer<UnityEngine.Object>
            {
                public int Compare(UnityEngine.Object x, UnityEngine.Object y)
                {
                    return
                        x == null ? -1 :
                        y == null ? 1 :
                        x.name.CompareTo(y.name);
                }
            }

        }

        protected PickerTreeView<T> treeView;

        protected Action<List<T>> OnAddButtonPushed;


        void OnGUI()
        {
            const float topMargin = 4;
            const float leftMargin = 2;
            const float rightMargin = 2;
            const float bottomMargin = 4;

            const float searchTextFieldHeight = 15;
            const float searchCancelButtonWidth = 15;


            var searchTextFieldRect = new Rect
            {
                x = leftMargin,
                y = topMargin,
                width = position.width - leftMargin - rightMargin - searchCancelButtonWidth,
                height = searchTextFieldHeight
            };


            var searchCancelButtonRect = new Rect
            {
                x = position.width - searchCancelButtonWidth - rightMargin,
                y = topMargin,
                width = searchCancelButtonWidth,
                height = searchTextFieldRect.height
            };

            const float buttonTopMargin = 2;
            const float buttonHeight = 18;
            var addButtonRect = new Rect
            {
                x = position.width - 100,
                y = position.height - bottomMargin - buttonHeight,
                width = 98,
                height = buttonHeight
            };

            const float treeViewTopMargin = 2;
            const float treeViewY = topMargin + searchTextFieldHeight + treeViewTopMargin;
            var treeViewRect = new Rect
            {
                x = leftMargin,
                y = treeViewY,
                width = position.width - leftMargin - rightMargin,
                height = position.height - treeViewY - buttonTopMargin - buttonHeight - bottomMargin
            };



            GUI.Box(new Rect(0, 0, position.width, position.height), "");

            // 検索用テキストボックス
            treeView.searchString = EditorGUI.TextField(searchTextFieldRect, treeView.searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));

            // 検索用テキストボックスの右の"×"ボタン
            if (GUI.Button(searchCancelButtonRect, "", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                treeView.searchString = "";
                GUI.FocusControl(null);
            }

            // ツリービュー本体
            treeView.OnGUI(treeViewRect);

            // "Add"ボタン
            if (GUI.Button(addButtonRect, "Add"))
            {
                OnAddButtonPushed(treeView.SelectedObjects);
                this.Close();
            }
        }
    }
}
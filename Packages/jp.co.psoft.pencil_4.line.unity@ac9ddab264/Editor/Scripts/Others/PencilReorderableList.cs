using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Pcl4Editor
{
    public class PencilReorderableList : ReorderableList
    {
        /// <summary>
        /// Public Fields/Properties
        /// </summary>
        
        public readonly List<GameObject> ObjectsToDestroy = new List<GameObject>();
        
        public IEnumerable<int> SelectedIndices
        {
            get { return _selectedIndices.OrderBy(x => x); }
        }

        public new int index
        {
            get
            {
                if (count <= base.index)
                {
                    index = -1;
                }
                
                if (!IsSelectionLimitOneOrMore) return base.index;
                if (count < 1 || SelectedIndices.Any(x => 0 <= x && x < count)) return base.index;
                
                index = 0;
                _selectedIndices.Clear();
                _selectedIndices.Add(0);
                return base.index;
            }
            set 
            {
                base.index = value;
                _selectedIndices.Clear();
                _selectedIndices.Add(value);
            }
        }
        
        public Action<IEnumerable<GameObject>> WillDestroyGameObjectCallback { get; set; }
        
        public Action<int> OnDoubleClickCallback { get; set; }
        
        public Action<ReorderableList> OnSelectionChangeCallback { get; set; }

        public bool IsSelectionLimitOneOrMore { get; set; }

        /// <summary>
        /// Private Fields
        /// </summary>
        private int _previousIndex = -1;

        private bool _isRangeSelectionHandling = false;

        private bool _isToggleSelectionHandling = false;
        
        private HashSet<int> _selectedIndices = new HashSet<int>(); 
        
        private GUIStyle _activeElementFontStyle;
        
        private GUIStyle _elementBackgroundStyle;

        
        /// <summary>
        /// Public Methods
        /// </summary>

        public PencilReorderableList(SerializedObject serializedObject, SerializedProperty elements) 
            : base(serializedObject, elements)
        {
            RegisterCallbacks();
        }
        
        public void OnRepaint()
        {
            // オブジェクト削除用のキューにオブジェクトが溜まっている場合は削除する
            // (Repaint時に削除操作を行わないと、複数オブジェクトを削除する時に削除が完全に行われない)
            if (WillDestroyGameObjectCallback == null) return;
            WillDestroyGameObjectCallback(ObjectsToDestroy);
            WillDestroyGameObjectCallback = (_) => { };
            ObjectsToDestroy.Clear();
        }

        

        public void HandleInputEventAndLayoutList()
        {
            HandleInputEvent();            
            DoLayoutList();
        }


        /// <summary>
        /// Private Classes/Methods
        /// </summary>

        private struct SelectionRange
        {
            public int Lowerbound;
            public int Upperbound;
        }


        private SelectionRange CalcShiftSelectionRange(int newIndex, int oldIndex)
        {
            var selectedMin = SelectedIndices.Any() ? SelectedIndices.Min() : 0;
            var selectedMax = SelectedIndices.Any() ? SelectedIndices.Max() : 0;
            
            // 動かない
            if (newIndex == oldIndex)
            {
                return new SelectionRange
                {
                    Lowerbound = newIndex,
                    Upperbound = newIndex
                };
            }
            
            // 上に拡張
            if (newIndex < selectedMin)
            {
                return new SelectionRange
                {
                    Lowerbound = newIndex,
                    Upperbound = selectedMax
                };
            }
            
            // 下に拡張
            if (selectedMax < newIndex)
            {
                return new SelectionRange
                {
                    Lowerbound = selectedMin,
                    Upperbound = newIndex
                };
            }
            
            // 上に縮小
            if (newIndex < _previousIndex)
            {
                return new SelectionRange
                {
                    Lowerbound = selectedMin,
                    Upperbound = newIndex
                };
            }
            
            // 下に縮小
            return new SelectionRange
            {
                Lowerbound = newIndex,
                Upperbound = selectedMax
            };
            
        }
        
        
       
        private void HandleInputEvent()
        {
            var topRect = GUILayoutUtility.GetRect(0, 0);
 
            if (!HasKeyboardControl()) return;
            
            var ev = Event.current;

            // Control/Shift押下時の複数選択の準備
            if ((ev.shift || IsCommand(ev)) && (ev.type == EventType.MouseDown || ev.type == EventType.KeyDown))
            {
                if (IsCommand(ev) && ev.type == EventType.MouseDown)
                {
                    // トグル選択 : Control + マウス選択
                    _isToggleSelectionHandling = true;
                    EditorApplication.delayCall += () => { _isToggleSelectionHandling = false; };
                }
                else if (index >= 0)
                {
                    // 範囲選択 : Shift/Control + キー選択 または Shift + マウス選択
                    _previousIndex = index;
                    _isRangeSelectionHandling = true;
                    EditorApplication.delayCall += () =>
                    {
                        _previousIndex = -1;
                        _isRangeSelectionHandling = false;
                    };
                }
            }

            if (ev.type == EventType.MouseDown)
            {
                if (IsCommand(ev) ||
                    ev.button != 0 || 
                    ev.clickCount != 2 || 
                    OnDoubleClickCallback == null) return;

                if (index < 0 || count <= index) return;
                
                var currentYMin = topRect.yMin + headerHeight;
                var currentYMax = topRect.yMin + GetHeight() - footerHeight;
                var mouseY = ev.mousePosition.y;
                
                if (mouseY < currentYMin || currentYMax < mouseY) return;
                
                // HACK: リストの上端部・下端部(クリックしてもインデックスが移動しない部分)がダブルクリックされた時には
                //       リストのインデックスが端ならダブルクリックされたものと見なす
                var firstElemHeight = elementHeightCallback != null ? elementHeightCallback(0) : elementHeight;
                var lastElemHeight = elementHeightCallback != null ? elementHeightCallback(count - 1) : elementHeight;
                
                if (mouseY < currentYMin + firstElemHeight)
                {
                    if (index != 0) return;
                }

                if (mouseY > currentYMax - lastElemHeight)
                {
                    if (index != count - 1) return;
                }
                
                OnDoubleClickCallback(index);
                ev.Use();
                return;
            }

            // Ctrl + Aでの全選択 (Unity2018.3以降で動作する)
            if (ev.type == EventType.KeyDown && IsCommand(ev) && ev.keyCode == KeyCode.A)
            {
                _selectedIndices = new HashSet<int>(Enumerable.Range(0, count));
                ev.Use();
            }

        }

        private void RegisterGuiStyles()
        {
            
            if (_activeElementFontStyle == null)
            {
                _activeElementFontStyle = new GUIStyle
                {
                    normal = {textColor = EditorStyles.whiteLabel.normal.textColor}
                };
            }
            
            if (_elementBackgroundStyle == null)
            {
                _elementBackgroundStyle = new GUIStyle("RL Element");
            }
        }

        private void RegisterCallbacks()
        {
            drawElementCallback = (rect, idx, isActive, isFocused) =>
            {
                RegisterGuiStyles();
                
                if (serializedProperty.arraySize <= idx) return;

                var element = serializedProperty.GetArrayElementAtIndex(idx);
                
                if (element.objectReferenceValue == null)
                {
                    // serializedPropertyの実体が存在するかどうか確認して、無ければリストから削除する
                    // (ReorderbleListのUI以外から要素が削除された時に起こり得る)
                    serializedProperty.DeleteArrayElementAtIndex(idx);
                    return;
                }
                
                rect.height -= 4;
                rect.y += 2;

                if (IsDragging())
                {
                    if (isFocused)
                    {
                        rect.x += 2;
                        rect.y += 1;
                        EditorGUI.LabelField(rect, element.objectReferenceValue.name, _activeElementFontStyle);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, element.objectReferenceValue.name);
                    }
                }
                else
                {
                    if (idx == index)
                    {
                        rect.x += 2;
                        rect.y += 1;
                        EditorGUI.LabelField(rect, element.objectReferenceValue.name, _activeElementFontStyle);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, element.objectReferenceValue.name);
                    }
                }
 
            };
            
            
            drawElementBackgroundCallback = (rect, idx, isActive, isFocused) =>
            {
                RegisterGuiStyles();

                if (Event.current.type != EventType.Repaint) return;

                if (IsDragging())
                {
                    _elementBackgroundStyle.Draw(rect, true, isActive, isActive, isFocused);
                }
                else
                {
                    _elementBackgroundStyle.Draw(
                        rect,
                        false,
                        true,
                        idx >= 0 && SelectedIndices.Contains(idx),
                        idx >= 0 && SelectedIndices.Contains(idx) && HasKeyboardControl());
                }
            };

            onSelectCallback = newList =>
            {
                // 複数選択関連のイベントハンドリング
                if (_isToggleSelectionHandling)
                {
                    // トグル選択
                    var selectedIndex = newList.index;

                    if (!_selectedIndices.Contains(selectedIndex))
                    {
                        // 選択
                        _selectedIndices.Add(selectedIndex);
                    }
                    else
                    {
                        // 選択キャンセル
                        if (!IsSelectionLimitOneOrMore || SelectedIndices.Count() != 1)
                        {
                            _selectedIndices.Remove(selectedIndex);
                            base.index = SelectedIndices.Any() ? SelectedIndices.Min() : -1;
                        }
                        
                        GUIUtility.hotControl = 0;
                    }                   
                }
                else if (_isRangeSelectionHandling)
                {
                    // 範囲選択
                    if (newList.index != _previousIndex)
                    {
                        var selection = CalcShiftSelectionRange(newList.index, _previousIndex);
                        
                        _selectedIndices = new HashSet<int>(Enumerable.Range(
                            selection.Lowerbound,
                            selection.Upperbound - selection.Lowerbound + 1));
                    }
                }
                else
                {
                    // 通常の選択
                    _selectedIndices.Clear();
                    _selectedIndices.Add(newList.index);
                }
                
                if (OnSelectionChangeCallback != null)
                {
                    OnSelectionChangeCallback(this);
                }
            };
        }

        
        private bool IsDragging()
        {
            var field = GetType().BaseType.GetField("m_Dragging",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            var isDragging = (bool)field.GetValue(this);
            return isDragging;
        }

        
        private static bool IsCommand(Event ev)
        {
#if UNITY_EDITOR_OSX
            return ev.command;
#elif UNITY_EDITOR_WIN
            return ev.control;
#else
            return ev.control;
#endif
        }
    }

}


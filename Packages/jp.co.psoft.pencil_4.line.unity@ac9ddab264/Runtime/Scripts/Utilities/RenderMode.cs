using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Pencil_4
{
    [InitializeOnLoad]
    public class RenderMode
    {
        public enum Mode
        {
            On1000ms,
            On200ms,
            Off,
        }

        const string editorPrefsKey = "Pencil+4 Line / Rendering Mode";

        const string on1000msMenuPath = "Pencil+ 4/Rendering in Edit Mode/On (Time-out: 1000ms)";
        const string on200msMenuPath = "Pencil+ 4/Rendering in Edit Mode/On (Time-out: 200ms)";
        const string offMenuPath = "Pencil+ 4/Rendering in Edit Mode/Off";


        public static Mode GameViewRenderMode
        {
            get
            {
                if (EditorPrefs.HasKey(editorPrefsKey))
                {
                    return (Mode)EditorPrefs.GetInt(editorPrefsKey);
                }
                return Mode.On200ms;
            }

            set
            {
                EditorPrefs.SetInt(editorPrefsKey, (int)value);
#if UNITY_2017_2_OR_NEWER
                EditorApplication.QueuePlayerLoopUpdate();
#endif
            }
        }

        static RenderMode()
        {
            GameViewRenderMode = (Mode)EditorPrefs.GetInt(editorPrefsKey);
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(on1000msMenuPath, GameViewRenderMode == Mode.On1000ms);
                Menu.SetChecked(on200msMenuPath, GameViewRenderMode == Mode.On200ms);
                Menu.SetChecked(offMenuPath, GameViewRenderMode == Mode.Off);
            };
        }


        [MenuItem(on200msMenuPath, false, 1)]
        static void RenderOn200ms()
        {
            GameViewRenderMode = Mode.On200ms;
            Menu.SetChecked(on1000msMenuPath, false);
            Menu.SetChecked(on200msMenuPath, true);
            Menu.SetChecked(offMenuPath, false);
        }


        [MenuItem(on1000msMenuPath, false, 2)]
        static void RenderOn1000ms()
        {
            GameViewRenderMode = Mode.On1000ms;
            Menu.SetChecked(on1000msMenuPath, true);
            Menu.SetChecked(on200msMenuPath, false);
            Menu.SetChecked(offMenuPath, false);
        }


        [MenuItem(offMenuPath, false, 3)]
        static void RenderOff()
        {
            GameViewRenderMode = Mode.Off;
            Menu.SetChecked(on1000msMenuPath, false);
            Menu.SetChecked(on200msMenuPath, false);
            Menu.SetChecked(offMenuPath, true);
        }
    }
}
#endif

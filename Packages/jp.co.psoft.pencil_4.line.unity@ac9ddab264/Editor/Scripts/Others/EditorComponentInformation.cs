using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pencil_4;


//
// シーン再生中に変更したパラメータを一時保存する
// (再生中に生成したGameObjectを現状保存できないため、一時的に無効化している)
//
#if false
namespace Pcl4ParameterSerializer
{

    public static class Extensions
    {
        public static Dictionary<FieldInfo, object> Serialize(this MonoBehaviour component)
        {
            return component
                .GetType()
                .GetFields()
                .Where(field => field.GetCustomAttributes(typeof(Persistent), true).Length > 0)
                .ToDictionary(field => field, field =>
                {
                    var value = field.GetValue(component);
                    if(value.GetType() == typeof(List<GameObject>))
                    {
                        var list = value as List<GameObject>;
                        var i = list.Count;
                    }
                    return field.GetValue(component);
                });
        }

        public static void Deserialize(this MonoBehaviour component, Dictionary<FieldInfo, object> fields)
        {
            
            foreach(var field in fields)
            {
                field.Key.SetValue(component, field.Value);
            }
        }
    }


    [InitializeOnLoad]
    public class EditorComponentInformation
    {
        const string menuPath = "Pencil+4/Save changes in play mode";
        const string editorPrefsKey = "Pencil+4 Line / Save changes in play mode";

        static bool isEnabled = false;

        static bool isRecording = false;
        static Dictionary<int, Dictionary<FieldInfo, object>> savedObjects = new Dictionary<int, Dictionary<FieldInfo, object>>();

        static EditorComponentInformation()
        {
            EditorApplication.playmodeStateChanged += () => OnPlayModeStateChanged();


            isEnabled = EditorPrefs.GetBool(editorPrefsKey);
            EditorApplication.delayCall += () => Menu.SetChecked(menuPath, isEnabled);

        }


        static void NotifyToEditor(string text)
        {
            Resources.FindObjectsOfTypeAll<EditorWindow>()
                        .First(w => w.GetType().ToString() == "UnityEditor.GameView")
                        .ShowNotification(new GUIContent(text));
            Resources.FindObjectsOfTypeAll<EditorWindow>()
                .First(w => w.GetType().ToString() == "UnityEditor.SceneView")
                .ShowNotification(new GUIContent(text));
        }


        static void OnPlayModeStateChanged()
        {
            
            if (!isEnabled)
            {
                savedObjects.Clear();
                return;
            }

            if (isRecording)
            {
                if (EditorApplication.isPlaying)
                {
                    //Serialize
                    savedObjects = GameObject.FindObjectsOfType<MonoBehaviour>()
                        .ToDictionary(component => component.GetInstanceID(), component => component.Serialize());
                    
                }
                else
                {
                    //Deserialize
                    var components = GameObject.FindObjectsOfType<MonoBehaviour>()
                        .Where(x => savedObjects.ContainsKey(x.GetInstanceID()))
                        .ToArray();

                    Undo.RecordObjects(components, "Pencil+ Parameters Change");

                    foreach(var component in components)
                    {
                        component.Deserialize(savedObjects[component.GetInstanceID()]);
                    }
                    
                    NotifyToEditor("Pencil+ parameters saved.");
                }
            }

            isRecording = EditorApplication.isPlaying;

            if (!isRecording && !EditorApplication.isPlaying)
            {
                savedObjects.Clear();
            }

            
            EditorApplication.delayCall = () =>
            {
                if (!EditorApplication.isPlaying)
                {
                    Menu.SetChecked(menuPath, isEnabled);
                }
            };
            
        }

        [MenuItem(menuPath)]
        static void ChangePersistent()
        {
            isEnabled = !isEnabled;
            EditorPrefs.SetBool(editorPrefsKey, isEnabled);
            Menu.SetChecked(menuPath, isEnabled);
        }

    }
}

#endif
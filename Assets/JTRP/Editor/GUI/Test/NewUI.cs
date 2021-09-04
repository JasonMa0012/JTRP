using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class NewUI : EditorWindow
{
    [MenuItem("Window/UIElements/NewUI")]
    public static void ShowExample()
    {
        NewUI wnd = GetWindow<NewUI>();
        wnd.titleContent = new GUIContent("NewUI");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        root.Add(new Button());

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/JTRP/Editor/GUI/NewUI.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();

        (labelFromUXML[0] as Button).clicked += () => { Debug.Log(6666666); };
        // button.clicked += () => { Debug.Log(6666666); };

        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JTRP/Editor/GUI/NewUI.uss");
        VisualElement labelWithStyle = new Label("Hello World! With Style");
        labelWithStyle.styleSheets.Add(styleSheet);
        root.Add(labelWithStyle);
    }
}
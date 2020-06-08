using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UTJ.NormalPainter;

namespace UTJ.NormalPainterEditor
{
    public class NormalPainterWindow : EditorWindow
    {
        public static bool isOpen;

        Vector2 m_scrollPos;
        UTJ.NormalPainter.NormalPainter m_target;
        GameObject m_active;

        bool m_shift;
        bool m_ctrl;


        string tips = "";



        [MenuItem("Window/Normal Painter")]
        public static void Open()
        {
            var window = EditorWindow.GetWindow<NormalPainterWindow>();
            window.titleContent = new GUIContent("Normal Painter");
            window.Show();
            window.OnSelectionChange();
        }



        private void OnEnable()
        {
            isOpen = true;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            isOpen = false;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (m_target != null && m_target.editing)
            {
                Tools.current = Tool.None;

                if (HandleShortcutKeys() || HandleMouseAction())
                {
                    Event.current.Use();
                    RepaintAllViews();
                }
                else
                {
                    int ret = m_target.OnSceneGUI();
                    if ((ret & (int)SceneGUIState.Repaint) != 0)
                        RepaintAllViews();
                }
            }
        }

        private void OnGUI()
        {
            if (m_target != null)
            {
                if (!m_target.isActiveAndEnabled)
                {
                    EditorGUILayout.LabelField("(Enable " + m_target.name + " to show Normal Painter)");
                }
                else
                {
                    var tooltipHeight = 24;
                    var windowHeight = position.height;
                    bool repaint = false;

                    if (m_target.editing)
                    {
                        if (HandleMouseAction())
                        {
                            Event.current.Use();
                            repaint = true;
                        }
                    }


                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    m_target.editing = GUILayout.Toggle(m_target.editing, EditorGUIUtility.IconContent("EditCollider"),
                        "Button", GUILayout.Width(33), GUILayout.Height(23));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_target.editing)
                        {
                            Tools.current = Tool.None;
                        }
                        else
                        {
                            Tools.current = Tool.Move;
                        }
                    }
                    GUILayout.Label("Edit Normals");
                    EditorGUILayout.EndHorizontal();


                    if (m_target.editing)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Height(windowHeight - tooltipHeight));
                        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
                        DrawNormalPainter();
                        EditorGUILayout.EndScrollView();

                        EditorGUILayout.LabelField(tips);
                        EditorGUILayout.EndVertical();

                        if (HandleShortcutKeys())
                        {
                            Event.current.Use();
                            repaint = true;
                        }
                    }

                    if (repaint)
                        RepaintAllViews();
                }
            }
            else if (m_active != null)
            {
                if (GUILayout.Button("Add Normal Painter to " + m_active.name))
                {
                    m_active.AddComponent<UTJ.NormalPainter.NormalPainter>();
                    OnSelectionChange();
                }
            }
        }

        private void OnSelectionChange()
        {
            if (m_target != null)
            {
                m_target.edited = m_target.editing;
                m_target.editing = false;
            }

            m_target = null;
            m_active = null;
            if (Selection.activeGameObject != null)
            {
                m_target = Selection.activeGameObject.GetComponent<UTJ.NormalPainter.NormalPainter>();
                if (m_target)
                {
                    m_target.editing = m_target.edited;
                }
                else
                {
                    var activeGameObject = Selection.activeGameObject;
                    if ( Selection.activeGameObject.GetComponent<MeshRenderer>() != null ||
                         Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        m_active = activeGameObject;
                    }
                }
            }
            Repaint();
        }




        void RepaintAllViews()
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        static readonly int indentSize = 18;
        static readonly int spaceSize = 5;
        static readonly int c1Width = 100;

        static readonly string[] strCommands = new string[] {
            "Selection [F1]",
            "Brush [F2]",
            "Assign [F3]",
            "Move [F4]",
            "Rotate [F5]",
            "Scale [F6]",
            "Smooth [F7]",
            "Projection [F8]",
            "Reset [F9]",
        };
        static readonly string[] strSelectMode = new string[] {
            "Single [1]",
            "Rect [2]",
            "Lasso [3]",
            "Brush [4]",
        };
        static readonly string[] strBrushTypes = new string[] {
            "Paint",
            "Replace",
            "Smooth",
            "Project",
            "Reset",
            "Flow",
        };
        static readonly string[] strCoodinate = new string[] {
            "World",
            "Local",
            "Pivot",
        };
        static readonly string[] strProjectionMode = new string[] {
            "Directional",
            "Use Normals As Ray",
        };
        static readonly string[] strRaySourcee = new string[] {
            "Base Normals",
            "Current Normals",
        };
        static readonly string[] strSmoothMode = new string[] {
            "Smoothing",
            "Welding",
            "Welding2",
        };



        void DrawBrushPanel()
        {
            var settings = m_target.settings;

            var brushImages = new Texture[settings.brushData.Length];
            for (int i = 0; i < settings.brushData.Length; ++i)
            {
                brushImages[i] = settings.brushData[i].image;
            }
            settings.brushActiveSlot = GUILayout.SelectionGrid(settings.brushActiveSlot, brushImages, 5);

            EditorGUILayout.Space();

            var bd = settings.activeBrush;
            bd.maxRadius = EditorGUILayout.FloatField("Max Radius", bd.maxRadius);
            bd.radius = EditorGUILayout.Slider("Radius [Shift+Drag]", bd.radius, 0.0f, bd.maxRadius);
            bd.strength = EditorGUILayout.Slider("Strength [Ctrl+Drag]", bd.strength, -1.0f, 1.0f);
            EditorGUI.BeginChangeCheck();
            bd.curve = EditorGUILayout.CurveField("Brush Shape", bd.curve, GUILayout.Width(EditorGUIUtility.labelWidth + 32), GUILayout.Height(32));
            if (EditorGUI.EndChangeCheck())
            {
                bd.UpdateSamples();
            }
        }

        void DrawProjectionPanel()
        {
            var settings = m_target.settings;

            EditorGUILayout.LabelField("Projection Mode", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(indentSize));
            settings.projectionMode = GUILayout.SelectionGrid(settings.projectionMode, strProjectionMode, 2);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            settings.projectionNormalSource =
                (GameObject)EditorGUILayout.ObjectField("Normal Source", settings.projectionNormalSource, typeof(GameObject), true);

            if (settings.projectionMode == 0)
            {
                settings.projectionDir = EditorGUILayout.Vector3Field("Ray Direction", settings.projectionDir).normalized;
            }
            else if (settings.projectionMode == 1)
            {
                EditorGUILayout.LabelField("Ray Direction", GUILayout.Width(EditorGUIUtility.labelWidth));
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(indentSize));
                settings.projectionRayDir = GUILayout.SelectionGrid(settings.projectionRayDir, strRaySourcee, 2);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
        }

        void DrawEditPanel()
        {
            var settings = m_target.settings;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(indentSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(c1Width));
            {
                var prev = settings.editMode;
                settings.editMode = (EditMode)GUILayout.SelectionGrid((int)settings.editMode, strCommands, 1);
                if (settings.editMode != prev)
                {
                    switch (settings.editMode)
                    {
                        case EditMode.Select:
                        case EditMode.Assign:
                        case EditMode.Move:
                        case EditMode.Rotate:
                        case EditMode.Scale:
                        case EditMode.Smooth:
                        case EditMode.Projection:
                        case EditMode.Reset:
                            tips = "Shift+LB: Add selection, Ctrl+LB: Subtract selection";
                            break;
                        case EditMode.Brush:
                            tips = "Shift+Wheel: Change radius, Ctrl+Wheel: Change strength";
                            break;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(spaceSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            if (settings.editMode == EditMode.Select)
            {
                settings.selectMode = (SelectMode)GUILayout.SelectionGrid((int)settings.selectMode, strSelectMode, 4);
                EditorGUILayout.Space();
                if (settings.selectMode == SelectMode.Brush)
                {
                    DrawBrushPanel();
                }
                else
                {
                    if(settings.selectMode == SelectMode.Single)
                    {
                        GUILayout.BeginHorizontal();
                        settings.selectVertex = GUILayout.Toggle(settings.selectVertex, "Vertex", "Button");
                        settings.selectTriangle = GUILayout.Toggle(settings.selectTriangle, "Triangle", "Button");
                        GUILayout.EndHorizontal();
                    }
                    settings.selectFrontSideOnly = EditorGUILayout.Toggle("Front Side Only", settings.selectFrontSideOnly);
                }

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Edge [E]", GUILayout.Width(100)))
                {
                    m_target.SelectEdge(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                if (GUILayout.Button("Hole [H]", GUILayout.Width(100)))
                {
                    m_target.SelectHole(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                if (GUILayout.Button("Connected [C]", GUILayout.Width(100)))
                {
                    m_target.SelectConnected(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("All [A]", GUILayout.Width(100)))
                {
                    m_target.SelectAll();
                    m_target.UpdateSelection();
                }
                if (GUILayout.Button("Clear [D]", GUILayout.Width(100)))
                {
                    m_target.ClearSelection();
                    m_target.UpdateSelection();
                }
                if (GUILayout.Button("Invert [I]", GUILayout.Width(100)))
                {
                    m_target.InvertSelection();
                    m_target.UpdateSelection();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Save", GUILayout.Width(50));
                for (int i = 0; i < 5; ++i)
                {
                    if (GUILayout.Button((i + 1).ToString()))
                        settings.selectionSets[i].selection = m_target.selection;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Load", GUILayout.Width(50));
                for (int i = 0; i < 5; ++i)
                {
                    if (GUILayout.Button((i + 1).ToString()))
                        m_target.selection = settings.selectionSets[i].selection;
                }
                GUILayout.EndHorizontal();
            }
            else if (settings.editMode == EditMode.Brush)
            {
                settings.brushMode = (BrushMode)GUILayout.SelectionGrid((int)settings.brushMode, strBrushTypes, 5);
                EditorGUILayout.Space();

                settings.brushMaskWithSelection = EditorGUILayout.Toggle("Mask With Selection", settings.brushMaskWithSelection); EditorGUILayout.Space();
                DrawBrushPanel();

                if (settings.brushMode == BrushMode.Replace)
                {
                    GUILayout.BeginHorizontal();
                    settings.assignValue = EditorGUILayout.Vector3Field("Value", settings.assignValue);
                    settings.pickNormal = GUILayout.Toggle(settings.pickNormal, "Pick [P]", "Button", GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }
                else if (settings.brushMode == BrushMode.Projection)
                {
                    DrawProjectionPanel();
                }
            }
            else if (settings.editMode == EditMode.Assign)
            {
                settings.assignValue = EditorGUILayout.Vector3Field("Value", settings.assignValue);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Selected Normal [Shift+C]", GUILayout.Width(200)))
                    settings.assignValue = m_target.selectionNormal;
                settings.pickNormal = GUILayout.Toggle(settings.pickNormal, "Pick [P]", "Button", GUILayout.Width(90));
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coordinate", GUILayout.Width(EditorGUIUtility.labelWidth));
                settings.coordinate = (Coordinate)GUILayout.SelectionGrid((int)settings.coordinate, strCoodinate, strCoodinate.Length);
                GUILayout.EndHorizontal();
                if (settings.coordinate == Coordinate.Pivot)
                {
                    EditorGUILayout.Space();
                    settings.pivotRot = Quaternion.Euler(EditorGUILayout.Vector3Field("Pivot Rotation", settings.pivotRot.eulerAngles));
                }

                if (GUILayout.Button("Assign [Shift+V]"))
                {
                    m_target.ApplyAssign(settings.assignValue, settings.coordinate, true);
                }
            }
            else if (settings.editMode == EditMode.Move)
            {
                settings.moveAmount = EditorGUILayout.Vector3Field("Move Amount", settings.moveAmount);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coordinate", GUILayout.Width(EditorGUIUtility.labelWidth));
                settings.coordinate = (Coordinate)GUILayout.SelectionGrid((int)settings.coordinate, strCoodinate, strCoodinate.Length);
                GUILayout.EndHorizontal();
                if (settings.coordinate == Coordinate.Pivot)
                {
                    EditorGUILayout.Space();
                    settings.pivotPos = EditorGUILayout.Vector3Field("Pivot Position", settings.pivotPos);
                    settings.pivotRot = Quaternion.Euler(EditorGUILayout.Vector3Field("Pivot Rotation", settings.pivotRot.eulerAngles));
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply Move"))
                {
                    m_target.ApplyMove(settings.moveAmount, settings.coordinate, true);
                }
            }
            else if (settings.editMode == EditMode.Rotate)
            {
                settings.rotateAmount = EditorGUILayout.Vector3Field("Rotate Amount", settings.rotateAmount);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coordinate", GUILayout.Width(EditorGUIUtility.labelWidth));
                settings.coordinate = (Coordinate)GUILayout.SelectionGrid((int)settings.coordinate, strCoodinate, strCoodinate.Length);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
                settings.rotatePivot = EditorGUILayout.Toggle("Rotate Around Pivot", settings.rotatePivot);
                if (settings.coordinate == Coordinate.Pivot || settings.rotatePivot)
                {
                    settings.pivotPos = EditorGUILayout.Vector3Field("Pivot Position", settings.pivotPos);
                    settings.pivotRot = Quaternion.Euler(EditorGUILayout.Vector3Field("Pivot Rotation", settings.pivotRot.eulerAngles));
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply Rotate"))
                {
                    if (settings.rotatePivot)
                        m_target.ApplyRotatePivot(
                            Quaternion.Euler(settings.rotateAmount), settings.pivotPos, settings.pivotRot, settings.coordinate, true);
                    else
                        m_target.ApplyRotate(Quaternion.Euler(settings.rotateAmount), settings.pivotRot, settings.coordinate, true);
                }
            }
            else if (settings.editMode == EditMode.Scale)
            {
                settings.scaleAmount = EditorGUILayout.Vector3Field("Scale Amount", settings.scaleAmount);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coordinate", GUILayout.Width(EditorGUIUtility.labelWidth));
                settings.coordinate = (Coordinate)GUILayout.SelectionGrid((int)settings.coordinate, strCoodinate, strCoodinate.Length);
                GUILayout.EndHorizontal();
                if (settings.coordinate == Coordinate.Pivot)
                {
                    EditorGUILayout.Space();
                    settings.pivotPos = EditorGUILayout.Vector3Field("Pivot Position", settings.pivotPos);
                    settings.pivotRot = Quaternion.Euler(EditorGUILayout.Vector3Field("Pivot Rotation", settings.pivotRot.eulerAngles));
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Apply Scale"))
                {
                    m_target.ApplyScale(settings.scaleAmount, settings.pivotPos, settings.pivotRot, settings.coordinate, true);
                }
            }
            else if (settings.editMode == EditMode.Smooth)
            {
                settings.smoothMode = GUILayout.SelectionGrid(settings.smoothMode, strSmoothMode, 3);
                EditorGUILayout.Space();

                if (settings.smoothMode == 0)
                {
                    settings.smoothRadius = EditorGUILayout.FloatField("Smooth Radius", settings.smoothRadius);
                    settings.smoothAmount = EditorGUILayout.FloatField("Smooth Amount", settings.smoothAmount);
                    if (GUILayout.Button("Apply Smoothing [Shift+S]"))
                    {
                        m_target.ApplySmoothing(settings.smoothRadius, settings.smoothAmount, true);
                    }
                }
                else if (settings.smoothMode == 1)
                {
                    settings.weldWithSmoothing = EditorGUILayout.Toggle("Smoothing", settings.weldWithSmoothing);
                    settings.weldAngle = EditorGUILayout.FloatField("Weld Angle", settings.weldAngle);

                    if (GUILayout.Button("Apply Welding [Shift+W]"))
                    {
                        m_target.ApplyWelding(settings.weldWithSmoothing, settings.weldAngle, true);
                    }
                }
                else if (settings.smoothMode == 2)
                {
                    EditorGUILayout.LabelField("Weld Targets");
                    EditorGUI.indentLevel++;
                    int n = EditorGUILayout.IntField("Size", settings.weldTargets.Length);
                    if (n != settings.weldTargets.Length)
                        System.Array.Resize(ref settings.weldTargets, n);

                    for (int i = 0; i < settings.weldTargets.Length; ++i)
                        settings.weldTargets[i] = (GameObject)EditorGUILayout.ObjectField(settings.weldTargets[i], typeof(GameObject), true);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();

                    settings.weldTargetsMode = EditorGUILayout.IntPopup("Weld Mode", settings.weldTargetsMode,
                        new string[3] { "Copy To Targets", "Copy From Targets", "Smoothing" },
                        new int[3] { 0, 1, 2 });

                    settings.weldAngle = EditorGUILayout.FloatField("Weld Angle", settings.weldAngle);

                    if (GUILayout.Button("Apply Welding"))
                    {
                        m_target.ApplyWelding2(settings.weldTargets, settings.weldTargetsMode, settings.weldAngle, true);
                    }
                }
            }
            else if (settings.editMode == EditMode.Projection)
            {
                DrawProjectionPanel();
                EditorGUILayout.Space();

                if (GUILayout.Button("Apply Projection"))
                {
                    var normalSource = settings.projectionNormalSourceData;
                    if (normalSource == null || normalSource.empty)
                    {
                        Debug.LogError("\"Normal Source\" object is not set or has no readable Mesh or Terrain.");
                    }
                    else if (settings.projectionMode == 0)
                    {
                        m_target.ApplyProjection2(normalSource, settings.projectionDir, true);
                    }
                    else
                    {
                        var rayDirs = settings.projectionRayDir == 0 ?
                            m_target.normalsBase : m_target.normals;
                        m_target.ApplyProjection(normalSource, rayDirs, true);
                    }
                }
            }
            else if (settings.editMode == EditMode.Reset)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset (Selection)"))
                {
                    m_target.ResetNormals(true, true);
                }
                else if (GUILayout.Button("Reset (All)"))
                {
                    m_target.ResetNormals(false, true);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void DrawMiscPanel()
        {
            var settings = m_target.settings;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(indentSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(c1Width));
            EditorGUILayout.LabelField("", GUILayout.Width(c1Width));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(spaceSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                var mirrorMode = settings.mirrorMode;
                settings.mirrorMode = (MirrorMode)EditorGUILayout.EnumPopup("Mirroring", settings.mirrorMode);
                if (mirrorMode != settings.mirrorMode)
                {
                    m_target.ApplyMirroring(true);
                }

                EditorGUILayout.Space();
                settings.foldTangents = EditorGUILayout.Foldout(settings.foldTangents, "Tangents");
                if (settings.foldTangents)
                {
                    EditorGUI.indentLevel++;
                    settings.tangentsMode = (TangentsUpdateMode)EditorGUILayout.EnumPopup("Update Mode", settings.tangentsMode);
                    if (settings.tangentsMode == TangentsUpdateMode.Manual)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(indentSize));
                        if (GUILayout.Button("Recalculate [T]"))
                            m_target.RecalculateTangents();
                        EditorGUILayout.EndHorizontal();
                    }
                    settings.tangentsPrecision = (TangentsPrecision)EditorGUILayout.EnumPopup("Precision", settings.tangentsPrecision);
                    EditorGUI.indentLevel--;
                }

                if (m_target.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Reset To Bindpose"))
                        m_target.ResetToBindpose(true);
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Save Settings"))
                m_target.ExportSettings("Assets/UTJ/NormalPainter/Data/DefaultSettings.asset");

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


        static readonly string[] strInExport = new string[] {
            "Vertex Color",
            "Bake Texture",
            "Load Texture",
            "Export .asset",
            "Export .obj",
        };

        void DrawInExportPanel()
        {
            var settings = m_target.settings;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(indentSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(c1Width));
            settings.inexportIndex = GUILayout.SelectionGrid(settings.inexportIndex, strInExport, 1);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(spaceSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            if (settings.inexportIndex == 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Convert To Vertex Color"))
                    m_target.BakeToVertexColor(true);
                if (GUILayout.Button("Convert From Vertex Color"))
                    m_target.LoadVertexColor(true);
                GUILayout.EndHorizontal();
            }
            else if (settings.inexportIndex == 1)
            {
                settings.bakeFormat = (ImageFormat)EditorGUILayout.EnumPopup("Format", settings.bakeFormat);
                settings.bakeWidth = EditorGUILayout.IntField("Width", settings.bakeWidth);
                settings.bakeHeight = EditorGUILayout.IntField("Height", settings.bakeHeight);
                settings.bakeSeparateSubmeshes = EditorGUILayout.Toggle("Separate Submeshes", settings.bakeSeparateSubmeshes);

                if (GUILayout.Button("Bake"))
                {
                    string path = settings.bakeFormat == ImageFormat.PNG ?
                        EditorUtility.SaveFilePanel("Export .png file", "", SanitizeForFileName(m_target.name) + "_normal", "png") :
                        EditorUtility.SaveFilePanel("Export .exr file", "", SanitizeForFileName(m_target.name) + "_normal", "exr");
                    m_target.BakeToTexture(settings.bakeWidth, settings.bakeHeight, path, settings.bakeFormat, settings.bakeSeparateSubmeshes);
                }
            }
            else if (settings.inexportIndex == 2)
            {
                settings.bakeSource = EditorGUILayout.ObjectField("Source Texture", settings.bakeSource, typeof(Texture), true) as Texture;

                if (GUILayout.Button("Load"))
                    m_target.LoadTexture(settings.bakeSource, true);
            }
            else if (settings.inexportIndex == 3)
            {
                if (GUILayout.Button("Export .asset file"))
                {
                    string path = EditorUtility.SaveFilePanel("Export .asset file", "Assets", SanitizeForFileName(m_target.name), "asset");
                    if (path.Length > 0)
                    {
                        var dataPath = Application.dataPath;
                        if (!path.StartsWith(dataPath))
                        {
                            Debug.LogError("Invalid path: Path must be under " + dataPath);
                        }
                        else
                        {
                            path = path.Replace(dataPath, "Assets");
                            AssetDatabase.CreateAsset(Instantiate(m_target.mesh), path);
                            Debug.Log("Asset exported: " + path);
                        }
                    }
                }
            }
            else if (settings.inexportIndex == 4)
            {
                settings.objFlipHandedness = EditorGUILayout.Toggle("Flip Handedness", settings.objFlipHandedness);
                settings.objFlipFaces = EditorGUILayout.Toggle("Flip Faces", settings.objFlipFaces);
                settings.objMakeSubmeshes = EditorGUILayout.Toggle("Make Submeshes", settings.objMakeSubmeshes);
                settings.objApplyTransform = EditorGUILayout.Toggle("Apply Transform", settings.objApplyTransform);
                settings.objIncludeChildren = EditorGUILayout.Toggle("Include Children", settings.objIncludeChildren);

                if (GUILayout.Button("Export .obj file"))
                {
                    string path = EditorUtility.SaveFilePanel("Export .obj file", "", SanitizeForFileName(m_target.name), "obj");
                    ObjExporter.Export(m_target.gameObject, path, new ObjExporter.Settings
                    {
                        flipFaces = settings.objFlipFaces,
                        flipHandedness = settings.objFlipHandedness,
                        includeChildren = settings.objIncludeChildren,
                        makeSubmeshes = settings.objMakeSubmeshes,
                        applyTransform = settings.objApplyTransform,
                    });
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


        static readonly string[] strDisplay = new string[] {
            "Display",
            "Options",
        };


        void DrawDisplayPanel()
        {
            var settings = m_target.settings;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(indentSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(c1Width));
            settings.displayIndex = GUILayout.SelectionGrid(settings.displayIndex, strDisplay, 1);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(spaceSize));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (settings.displayIndex == 0)
            {
                settings.visualize = EditorGUILayout.Toggle("Visualize [Tab]", settings.visualize);
                if (settings.visualize)
                {
                    EditorGUI.indentLevel++;
                    settings.showVertices = EditorGUILayout.Toggle("Vertices", settings.showVertices);
                    settings.showNormals = EditorGUILayout.Toggle("Normals", settings.showNormals);
                    settings.showTangents = EditorGUILayout.Toggle("Tangents", settings.showTangents);
                    settings.showBinormals = EditorGUILayout.Toggle("Binormals", settings.showBinormals);
                    EditorGUILayout.Space();
                    settings.showSelectedOnly = EditorGUILayout.Toggle("Selection Only", settings.showSelectedOnly);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                settings.modelOverlay = (ModelOverlay)EditorGUILayout.EnumPopup("Overlay", settings.modelOverlay);
                settings.showBrushRange = EditorGUILayout.Toggle("Brush Range", settings.showBrushRange);
            }
            else if (settings.displayIndex == 1)
            {
                settings.vertexSize = EditorGUILayout.Slider("Vertex Size", settings.vertexSize, 0.0f, 0.05f);
                settings.normalSize = EditorGUILayout.Slider("Normal Size", settings.normalSize, 0.0f, 1.00f);
                settings.tangentSize = EditorGUILayout.Slider("Tangent Size", settings.tangentSize, 0.0f, 1.00f);
                settings.binormalSize = EditorGUILayout.Slider("Binormal Size", settings.binormalSize, 0.0f, 1.00f);

                EditorGUILayout.Space();

                settings.vertexColor = EditorGUILayout.ColorField("Vertex Color", settings.vertexColor);
                settings.vertexColor2 = EditorGUILayout.ColorField("Vertex Color (Selected)", settings.vertexColor2);
                settings.vertexColor3 = EditorGUILayout.ColorField("Vertex Color (Highlighted)", settings.vertexColor3);
                settings.normalColor = EditorGUILayout.ColorField("Normal Color", settings.normalColor);
                settings.tangentColor = EditorGUILayout.ColorField("Tangent Color", settings.tangentColor);
                settings.binormalColor = EditorGUILayout.ColorField("Binormal Color", settings.binormalColor);
                if (GUILayout.Button("Reset"))
                {
                    settings.ResetDisplayOptions();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


        void DrawNormalPainter()
        {
            var settings = m_target.settings;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("Box");
            settings.foldEdit = EditorGUILayout.Foldout(settings.foldEdit, "Edit");
            if (settings.foldEdit)
                DrawEditPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            settings.foldMisc = EditorGUILayout.Foldout(settings.foldMisc, "Misc");
            if (settings.foldMisc)
                DrawMiscPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            settings.foldInExport = EditorGUILayout.Foldout(settings.foldInExport, "Import / Export");
            if (settings.foldInExport)
                DrawInExportPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            settings.foldDisplay = EditorGUILayout.Foldout(settings.foldDisplay, "Display");
            if (settings.foldDisplay)
                DrawDisplayPanel();
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                RepaintAllViews();
        }


        bool HandleShortcutKeys()
        {
            bool handled = false;
            var settings = m_target.settings;
            var e = Event.current;

            m_shift = e.shift;
            m_ctrl = e.control;

            if (e.type == EventType.KeyDown)
            {
                var prevEditMode = settings.editMode;
                switch (e.keyCode)
                {
                    case KeyCode.F1: settings.editMode = EditMode.Select; break;
                    case KeyCode.F2: settings.editMode = EditMode.Brush; break;
                    case KeyCode.F3: settings.editMode = EditMode.Assign; break;
                    case KeyCode.F4: settings.editMode = EditMode.Move; break;
                    case KeyCode.F5: settings.editMode = EditMode.Rotate; break;
                    case KeyCode.F6: settings.editMode = EditMode.Scale; break;
                    case KeyCode.F7: settings.editMode = EditMode.Smooth; break;
                    case KeyCode.F8: settings.editMode = EditMode.Projection; break;
                    case KeyCode.F9: settings.editMode = EditMode.Reset; break;
                }
                if (settings.editMode != prevEditMode)
                    handled = true;

                if (settings.editMode == EditMode.Select)
                {
                    var prevSelectMode = settings.selectMode;
                    switch (e.keyCode)
                    {
                        case KeyCode.Alpha1: settings.selectMode = SelectMode.Single; break;
                        case KeyCode.Alpha2: settings.selectMode = SelectMode.Rect; break;
                        case KeyCode.Alpha3: settings.selectMode = SelectMode.Lasso; break;
                        case KeyCode.Alpha4: settings.selectMode = SelectMode.Brush; break;
                    }
                    if (settings.selectMode != prevSelectMode)
                        handled = true;
                }
                else if (settings.editMode == EditMode.Brush)
                {
                    var prevBrushMode = settings.brushMode;
                    switch (e.keyCode)
                    {
                        case KeyCode.Alpha1: settings.brushMode = BrushMode.Paint; break;
                        case KeyCode.Alpha2: settings.brushMode = BrushMode.Replace; break;
                        case KeyCode.Alpha3: settings.brushMode = BrushMode.Smooth; break;
                        case KeyCode.Alpha4: settings.brushMode = BrushMode.Projection; break;
                        case KeyCode.Alpha5: settings.brushMode = BrushMode.Reset; break;
                        case KeyCode.Alpha6: settings.brushMode = BrushMode.Flow; break;
                    }
                    if (settings.brushMode != prevBrushMode)
                        handled = true;
                }

                if (e.keyCode == KeyCode.C && e.shift)
                {
                    handled = true;
                    tips = "Copy";
                    settings.assignValue = m_target.selectionNormal;
                }
                else if (e.keyCode == KeyCode.V && e.shift)
                {
                    handled = true;
                    tips = "Paste";
                    m_target.ApplyAssign(settings.assignValue, settings.coordinate, true);
                }
                else if (e.keyCode == KeyCode.S && e.shift)
                {
                    handled = true;
                    tips = "Apply Smoothing";
                    m_target.ApplySmoothing(settings.smoothRadius, settings.smoothAmount, true);
                }
                else if (e.keyCode == KeyCode.W && e.shift)
                {
                    handled = true;
                    tips = "Apply Welding";
                    m_target.ApplyWelding(settings.weldWithSmoothing, settings.weldAngle, true);
                }
                else if (e.keyCode == KeyCode.Tab)
                {
                    handled = true;
                    tips = "Toggle Visualization";
                    settings.visualize = !settings.visualize;

                }
                else if (e.keyCode == KeyCode.A)
                {
                    handled = true;
                    tips = "Select All";
                    m_target.SelectAll();
                    m_target.UpdateSelection();
                }
                else if (e.keyCode == KeyCode.I)
                {
                    handled = true;
                    tips = "Invert Selection";
                    m_target.InvertSelection();
                    m_target.UpdateSelection();
                }
                else if (e.keyCode == KeyCode.E)
                {
                    handled = true;
                    tips = "Select Edge";
                    m_target.SelectEdge(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                else if(e.keyCode == KeyCode.H)
                {
                    handled = true;
                    tips = "Select Hole";
                    m_target.SelectHole(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                else if (e.keyCode == KeyCode.C)
                {
                    handled = true;
                    tips = "Select Connected";
                    m_target.SelectConnected(m_ctrl ? -1.0f : 1.0f, !m_shift && !m_ctrl);
                    m_target.UpdateSelection();
                }
                else if(e.keyCode == KeyCode.D)
                {
                    handled = true;
                    tips = "Clear Selection";
                    m_target.ClearSelection();
                    m_target.UpdateSelection();
                }
                else if (e.keyCode == KeyCode.T)
                {
                    handled = true;
                    tips = "Recalculate Tangents";
                    m_target.RecalculateTangents();
                }
                else if (e.keyCode == KeyCode.P)
                {
                    handled = true;
                    tips = "Pick Normal";
                    settings.pickNormal = !settings.pickNormal;
                }
            }

            return handled;
        }

        bool HandleMouseAction()
        {
            bool handled = false;
            var settings = m_target.settings;
            var e = Event.current;

            if (e.type == EventType.MouseDrag)
            {
                float amount = e.delta.x - e.delta.y;

                if (settings.editMode == EditMode.Brush ||
                    (settings.editMode == EditMode.Select && settings.selectMode == SelectMode.Brush))
                {
                    var bd = settings.activeBrush;
                    if (e.shift)
                    {
                        bd.radius = Mathf.Clamp(bd.radius + amount * (bd.maxRadius * 0.0025f), 0.0f, bd.maxRadius);
                        handled = true;
                    }
                    else if (e.control)
                    {
                        bd.strength = Mathf.Clamp(bd.strength + amount * 0.0025f, -1.0f, 1.0f);
                        handled = true;
                    }
                }
            }

            return handled;
        }

        public static string SanitizeForFileName(string name)
        {
            var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
            return reg.Replace(name, "_");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJ.NormalPainter
{

    [ExecuteInEditMode]
    public partial class NormalPainter : MonoBehaviour
    {
#if UNITY_EDITOR
        [Serializable]
        public class History
        {
            [Serializable]
            public class Record
            {
                public Mesh mesh;
                public Vector3[] normals;
                public Color[] colors;
            }

            public int index;
            public Mesh mesh;
            public Vector3[] normals;
            public Record[] records;
        }


        NormalPainterSettings m_settings;

        // internal resources
        [SerializeField] Mesh m_meshTarget;
        [SerializeField] Mesh m_meshPoint;
        [SerializeField] Mesh m_meshVector;
        [SerializeField] Mesh m_meshLasso;
        [SerializeField] Material m_matVisualize;
        [SerializeField] Material m_matOverlay;
        [SerializeField] Material m_matBake;
        [SerializeField] ComputeShader m_csBakeFromMap;

        ComputeBuffer m_cbArgPoints;
        ComputeBuffer m_cbArgVectors;
        ComputeBuffer m_cbPoints;
        ComputeBuffer m_cbNormals;
        ComputeBuffer m_cbTangents;
        ComputeBuffer m_cbSelection;
        ComputeBuffer m_cbBaseNormals;
        ComputeBuffer m_cbBaseTangents;
        Texture2D m_texBrushSamples;
        CommandBuffer m_cmdDraw;

        bool m_skinned;
        PinnedList<Vector3> m_points, m_pointsPredeformed;
        PinnedList<Vector3> m_normals, m_normalsPredeformed, m_normalsBase, m_normalsBasePredeformed;
        PinnedList<Vector4> m_tangents, m_tangentsPredeformed, m_tangentsBase, m_tangentsBasePredeformed;
        PinnedList<Vector2> m_uv;
        PinnedList<int> m_indices;
        PinnedList<int> m_mirrorRelation;
        PinnedList<float> m_selection;

        PinnedList<BoneWeight> m_boneWeights;
        PinnedList<Matrix4x4> m_bindposes;
        PinnedList<Matrix4x4> m_boneMatrices;

        bool m_editing;
        bool m_edited;
        int m_numSelected = 0;
        bool m_rayHit;
        int m_rayHitTriangle;
        Vector3 m_rayPos;
        Vector3 m_prevRayPos;
        Vector3 m_selectionPos;
        Vector3 m_selectionNormal;
        Quaternion m_selectionRot;
        bool m_rectDragging;
        Vector2 m_rectStartPoint;
        Vector2 m_rectEndPoint;
        List<Vector2> m_lassoPoints = new List<Vector2>();
        int m_brushNumPainted = 0;

        [SerializeField] History m_history = new History();
        int m_historyIndex = 0;

        npMeshData m_npModelData = new npMeshData();
        npSkinData m_npSkinData = new npSkinData();

        public bool editing
        {
            get { return m_editing; }
            set
            {
                if (value && !m_editing) BeginEdit();
                if (!value && m_editing) EndEdit();
            }
        }
        public bool edited
        {
            get { return m_edited; }
            set { m_edited = value; }
        }

        public NormalPainterSettings settings { get { return m_settings; } }
        public Mesh mesh { get { return m_meshTarget; } }
        public Vector3 selectionNormal { get { return m_selectionNormal; } }
        public PinnedList<Vector3> normals { get { return m_normals; } }
        public PinnedList<Vector3> normalsBase { get { return m_normalsBase; } }

        public float[] selection
        {
            get { return (float[])m_selection.Array.Clone(); }
            set
            {
                if (value != null && value.Length == m_selection.Count)
                {
                    Array.Copy(value, m_selection.Array, m_selection.Count);
                    UpdateSelection();
                }
            }
        }


        Mesh GetTargetMesh()
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr) { return smr.sharedMesh; }

            var mf = GetComponent<MeshFilter>();
            if (mf) { return mf.sharedMesh; }

            return null;
        }

        void BeginEdit()
        {
            var tmesh = GetTargetMesh();
            if (tmesh == null)
            {
                Debug.LogWarning("Target mesh is null.");
                return;
            }
            else if (!tmesh.isReadable)
            {
                Debug.LogWarning("Target mesh is not readable.");
                return;
            }

            if (m_settings == null)
            {
                var ds = AssetDatabase.LoadAssetAtPath<NormalPainterSettings>(AssetDatabase.GUIDToAssetPath("f9fa1a75054c38b439daaed96bc5b424"));
                if (ds != null)
                {
                    m_settings = Instantiate(ds);
                }
                if (m_settings == null)
                {
                    m_settings = ScriptableObject.CreateInstance<NormalPainterSettings>();
                }
            }

            if (m_meshPoint == null)
            {
                float l = 0.5f;
                var p = new Vector3[] {
                    new Vector3(-l,-l, l),
                    new Vector3( l,-l, l),
                    new Vector3( l,-l,-l),
                    new Vector3(-l,-l,-l),

                    new Vector3(-l, l, l),
                    new Vector3( l, l, l),
                    new Vector3( l, l,-l),
                    new Vector3(-l, l,-l),
                };

                m_meshPoint = new Mesh();
                m_meshPoint.vertices = new Vector3[] {
                    p[0], p[1], p[2], p[3],
                    p[7], p[4], p[0], p[3],
                    p[4], p[5], p[1], p[0],
                    p[6], p[7], p[3], p[2],
                    p[5], p[6], p[2], p[1],
                    p[7], p[6], p[5], p[4],
                };
                m_meshPoint.SetIndices(new int[] {
                    3, 1, 0, 3, 2, 1,
                    7, 5, 4, 7, 6, 5,
                    11, 9, 8, 11, 10, 9,
                    15, 13, 12, 15, 14, 13,
                    19, 17, 16, 19, 18, 17,
                    23, 21, 20, 23, 22, 21,
                }, MeshTopology.Triangles, 0);
                m_meshPoint.UploadMeshData(false);
            }

            if (m_meshVector == null)
            {
                m_meshVector = new Mesh();
                m_meshVector.vertices = new Vector3[2] { Vector3.zero, Vector3.zero };
                m_meshVector.uv = new Vector2[2] { Vector2.zero, Vector2.one };
                m_meshVector.SetIndices(new int[2] { 0, 1 }, MeshTopology.Lines, 0);
                m_meshVector.UploadMeshData(false);
            }

            if (m_meshLasso == null)
            {
                m_meshLasso = new Mesh();
            }

            if (m_matVisualize == null)
                m_matVisualize = new Material(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath("03871fa9be0375f4c91cb4842f15b890")));
            if (m_matOverlay == null)
                m_matOverlay = new Material(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath("b531c1011d0464740aa59c2809bbcbb2")));
            if (m_matBake == null)
                m_matBake = new Material(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath("4ddd0053dc720414b8afc76bf0a93f8e")));
            if (m_csBakeFromMap == null)
                m_csBakeFromMap = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("f6687b99e1b6bfc4f854f46669e84e31"));

            if (m_meshTarget == null ||
                m_meshTarget != tmesh ||
                (m_points != null && m_meshTarget.vertexCount != m_points.Count))
            {
                m_meshTarget = tmesh;
                m_points = null;
                m_normals = null;
                m_normalsBase = null;
                m_tangents = null;
                m_indices = null;
                m_mirrorRelation = null;
                m_selection = null;

                ReleaseComputeBuffers();
            }

            if (m_meshTarget != null)
            {
                m_points = new PinnedList<Vector3>(m_meshTarget.vertices);
                m_pointsPredeformed = m_points;

                m_uv = new PinnedList<Vector2>(m_meshTarget.uv);

                m_normals = new PinnedList<Vector3>(m_meshTarget.normals);
                if (m_normals.Count == 0)
                {
                    m_meshTarget.RecalculateNormals();
                    m_normalsBase = m_normals = new PinnedList<Vector3>(m_meshTarget.normals);
                }
                else
                {
                    m_meshTarget.RecalculateNormals();
                    m_normalsBase = new PinnedList<Vector3>(m_meshTarget.normals);
                    m_meshTarget.normals = m_normals.Array;
                }
                m_normalsPredeformed = m_normals;
                m_normalsBasePredeformed = m_normalsBase;

                m_tangents = new PinnedList<Vector4>(m_meshTarget.tangents);
                if (m_tangents.Count == 0)
                {
                    m_meshTarget.RecalculateTangents();
                    m_tangentsBase = m_tangents = new PinnedList<Vector4>(m_meshTarget.tangents);
                }
                else
                {
                    m_meshTarget.RecalculateTangents();
                    m_tangentsBase = new PinnedList<Vector4>(m_meshTarget.tangents);
                    m_meshTarget.tangents = m_tangents.Array;
                }
                m_tangentsPredeformed = m_tangents;
                m_tangentsBasePredeformed = m_tangentsBase;

                m_indices = new PinnedList<int>(m_meshTarget.triangles);
                m_selection = new PinnedList<float>(m_points.Count);

                m_npModelData.num_vertices = m_points.Count;
                m_npModelData.num_triangles = m_indices.Count / 3;
                m_npModelData.indices = m_indices;
                m_npModelData.vertices = m_points;
                m_npModelData.normals = m_normals;
                m_npModelData.tangents = m_tangents;
                m_npModelData.uv = m_uv;
                m_npModelData.selection = m_selection;

                var smr = GetComponent<SkinnedMeshRenderer>();
                if (smr != null && smr.bones.Length > 0)
                {
                    m_skinned = true;

                    m_boneWeights = new PinnedList<BoneWeight>(m_meshTarget.boneWeights);
                    m_bindposes = new PinnedList<Matrix4x4>(m_meshTarget.bindposes);
                    m_boneMatrices = new PinnedList<Matrix4x4>(m_bindposes.Count);

                    m_pointsPredeformed = m_points.Clone();
                    m_normalsPredeformed = m_normals.Clone();
                    m_normalsBasePredeformed = m_normalsBase.Clone();
                    m_tangentsPredeformed = m_tangents.Clone();
                    m_tangentsBasePredeformed = m_tangentsBase.Clone();

                    m_npSkinData.num_vertices = m_boneWeights.Count;
                    m_npSkinData.num_bones = m_bindposes.Count;
                    m_npSkinData.weights = m_boneWeights;
                    m_npSkinData.bindposes = m_bindposes;
                    m_npSkinData.bones = m_boneMatrices;
                }

            }

            if (m_cbPoints == null && m_points != null && m_points.Count > 0)
            {
                m_cbPoints = new ComputeBuffer(m_points.Count, 12);
                m_cbPoints.SetData(m_points.List);
            }
            if (m_cbNormals == null && m_normals != null && m_normals.Count > 0)
            {
                m_cbNormals = new ComputeBuffer(m_normals.Count, 12);
                m_cbNormals.SetData(m_normals.List);
                m_cbBaseNormals = new ComputeBuffer(m_normalsBase.Count, 12);
                m_cbBaseNormals.SetData(m_normalsBase.List);
            }
            if (m_cbTangents == null && m_tangents != null && m_tangents.Count > 0)
            {
                m_cbTangents = new ComputeBuffer(m_tangents.Count, 16);
                m_cbTangents.SetData(m_tangents.List);
                m_cbBaseTangents = new ComputeBuffer(m_tangentsBase.Count, 16);
                m_cbBaseTangents.SetData(m_tangentsBase.List);
            }
            if (m_cbSelection == null && m_selection != null && m_selection.Count > 0)
            {
                m_cbSelection = new ComputeBuffer(m_selection.Count, 4);
                m_cbSelection.SetData(m_selection.List);
            }

            if (m_cbArgPoints == null && m_points != null && m_points.Count > 0)
            {
                m_cbArgPoints = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                m_cbArgPoints.SetData(new uint[5] { m_meshPoint.GetIndexCount(0), (uint)m_points.Count, 0, 0, 0 });

                m_cbArgVectors = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                m_cbArgVectors.SetData(new uint[5] { m_meshVector.GetIndexCount(0), (uint)m_points.Count, 0, 0, 0 });
            }

            m_settings.InitializeBrushData();

            UpdateTransform();
            UpdateNormals();
            PushUndo();
            m_editing = true;
        }

        void EndEdit()
        {
            ReleaseComputeBuffers();
            if(m_settings) m_settings.projectionNormalSource = null;

            m_editing = false;
        }

        void ReleaseComputeBuffers()
        {
            if (m_cbArgPoints != null) { m_cbArgPoints.Release(); m_cbArgPoints = null; }
            if (m_cbArgVectors != null) { m_cbArgVectors.Release(); m_cbArgVectors = null; }
            if (m_cbPoints != null) { m_cbPoints.Release(); m_cbPoints = null; }
            if (m_cbNormals != null) { m_cbNormals.Release(); m_cbNormals = null; }
            if (m_cbTangents != null) { m_cbTangents.Release(); m_cbTangents = null; }
            if (m_cbSelection != null) { m_cbSelection.Release(); m_cbSelection = null; }
            if (m_cbBaseNormals != null) { m_cbBaseNormals.Release(); m_cbBaseNormals = null; }
            if (m_cbBaseTangents != null) { m_cbBaseTangents.Release(); m_cbBaseTangents = null; }
            if (m_texBrushSamples != null) { DestroyImmediate(m_texBrushSamples); m_texBrushSamples = null; }
            if (m_cmdDraw != null) { m_cmdDraw.Release(); m_cmdDraw = null; }
        }

        void Start()
        {
            npInitializePenInput();
        }

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            EndEdit();
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void LateUpdate()
        {
            if (m_editing)
            {
                UpdateTransform();
            }
        }

        public int OnSceneGUI()
        {
            if (!isActiveAndEnabled || m_points == null)
                return 0;

            int ret = 0;
            ret |= HandleEditTools();

            Event e = Event.current;
            var et = e.type;
            int id = GUIUtility.GetControlID(FocusType.Passive);
            et = e.GetTypeForControl(id);

            if ((et == EventType.MouseDown || et == EventType.MouseDrag || et == EventType.MouseUp) && e.button == 0)
            {
                ret |= HandleMouseEvent(e, et, id);
            }

            if (Event.current.type == EventType.Repaint)
                OnRepaint();
            return ret;
        }


        Vector3 m_prevMove;
        Quaternion m_prevRot;
        Vector3 m_prevScale;
        bool m_toolHanding = false;

        public int HandleEditTools()
        {
            // check if model has been changed
            if(m_meshTarget != GetTargetMesh())
            {
                BeginEdit();
            }

            Event e = Event.current;
            if (e.alt) return 0;

            var editMode = m_settings.editMode;
            var et = e.type;
            int ret = 0;
            bool handled = false;
            var t = GetComponent<Transform>();

            if (et == EventType.MouseMove || et == EventType.MouseDrag)
            {
                bool prevRayHit = m_rayHit;
                m_prevRayPos = m_rayPos;
                m_rayHit = Raycast(e, ref m_rayPos, ref m_rayHitTriangle);
                
                if (m_rayHit || prevRayHit)
                    ret |= (int)SceneGUIState.Repaint;
            }

            if (m_numSelected > 0 && editMode == EditMode.Assign)
            {
                var pivotRot = Quaternion.identity;
                switch (m_settings.coordinate)
                {
                    case Coordinate.Pivot:
                        pivotRot = m_settings.pivotRot;
                        break;
                    case Coordinate.Local:
                        pivotRot = t.rotation;
                        break;
                }

                Handles.PositionHandle(m_settings.pivotPos, pivotRot);
            }
            else if (m_numSelected > 0 && editMode == EditMode.Move)
            {
                var pivotRot = Quaternion.identity;
                switch (m_settings.coordinate)
                {
                    case Coordinate.Pivot:
                        pivotRot = m_settings.pivotRot;
                        break;
                    case Coordinate.Local:
                        pivotRot = t.rotation;
                        break;
                }

                if (et == EventType.MouseDown)
                    m_prevMove = m_settings.pivotPos;

                EditorGUI.BeginChangeCheck();
                var move = Handles.PositionHandle(m_settings.pivotPos, pivotRot);
                if (EditorGUI.EndChangeCheck())
                {
                    handled = true;
                    var diff = move - m_prevMove;
                    m_prevMove = move;

                    ApplyMove(diff * 3.0f, Coordinate.World, false);
                }
            }
            else if (m_numSelected > 0 && editMode == EditMode.Rotate)
            {
                var pivotRot = Quaternion.identity;
                switch (m_settings.coordinate)
                {
                    case Coordinate.Pivot:
                        pivotRot = m_settings.pivotRot;
                        break;
                    case Coordinate.Local:
                        pivotRot = t.rotation;
                        break;
                }

                if (et == EventType.MouseDown)
                    m_prevRot = pivotRot;

                EditorGUI.BeginChangeCheck();
                var rot = Handles.RotationHandle(pivotRot, m_settings.pivotPos);
                if (EditorGUI.EndChangeCheck())
                {
                    handled = true;
                    var diff = Quaternion.Inverse(m_prevRot) * rot;
                    m_prevRot = rot;

                    if (m_settings.rotatePivot)
                        ApplyRotatePivot(diff, m_settings.pivotPos, pivotRot, Coordinate.Pivot, false);
                    else
                        ApplyRotate(diff, pivotRot, Coordinate.Pivot, false);
                }
            }
            else if (m_numSelected > 0 && editMode == EditMode.Scale)
            {
                var pivotRot = Quaternion.identity;
                switch (m_settings.coordinate)
                {
                    case Coordinate.Pivot:
                        pivotRot = m_settings.pivotRot;
                        break;
                    case Coordinate.Local:
                        pivotRot = t.rotation;
                        break;
                }
                if (et == EventType.MouseDown)
                    m_prevScale = Vector3.one;

                EditorGUI.BeginChangeCheck();
                var scale = Handles.ScaleHandle(Vector3.one, m_settings.pivotPos,
                    pivotRot, HandleUtility.GetHandleSize(m_settings.pivotPos));
                if (EditorGUI.EndChangeCheck())
                {
                    handled = true;
                    var diff = scale - m_prevScale;
                    m_prevScale = scale;

                    ApplyScale(diff, m_settings.pivotPos, pivotRot, Coordinate.Pivot, false);
                }
            }

            if (handled)
            {
                m_toolHanding = true;
                ret |= (int)SceneGUIState.Repaint;
            }
            else if (m_toolHanding && et == EventType.MouseUp)
            {
                m_toolHanding = false;
                ret |= (int)SceneGUIState.Repaint;
                PushUndo();
            }

            return ret;
        }

        public static Color ToColor(Vector3 n)
        {
            return new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1.0f);
        }
        public static Vector3 ToVector(Color n)
        {
            return new Vector3(n.r * 2.0f - 1.0f, n.g * 2.0f - 1.0f, n.b * 2.0f - 1.0f);
        }

        int HandleMouseEvent(Event e, EventType et, int id)
        {
            if (e.alt) return 0;

            int ret = 0;
            var editMode = m_settings.editMode;
            bool handled = false;

            if (m_settings.pickNormal)
            {
                if (m_rayHit)
                {
                    m_settings.assignValue = PickNormal(m_rayPos, m_rayHitTriangle);
                    handled = true;
                }
                m_settings.pickNormal = false;
            }
            else if (editMode == EditMode.Brush)
            {
                if (m_rayHit && (et == EventType.MouseDown || et == EventType.MouseDrag) && (!e.shift && !e.control))
                {
                    var bd = m_settings.activeBrush;
                    switch (m_settings.brushMode)
                    {
                    case BrushMode.Flow:
                        if (ApplyFlowBrush(m_settings.brushMaskWithSelection, m_rayPos, m_prevRayPos, bd.radius, bd.strength, bd.samples,
                                PickBaseNormal(m_rayPos, m_rayHitTriangle)))
                                ++m_brushNumPainted;
                            break;
                        case BrushMode.Paint:
                            if (ApplyPaintBrush(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples,
                                PickBaseNormal(m_rayPos, m_rayHitTriangle), settings.brushBlendMode))
                                ++m_brushNumPainted;
                            break;
                        case BrushMode.Replace:
                            if (ApplyReplaceBrush(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples,
                                m_settings.assignValue))
                                ++m_brushNumPainted;
                            break;
                        case BrushMode.Smooth:
                            if (ApplySmoothBrush(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples))
                                ++m_brushNumPainted;
                            break;
                        case BrushMode.Projection:
                            if (m_settings.projectionNormalSourceData == null || m_settings.projectionNormalSourceData.empty)
                            {
                                if (et == EventType.MouseDown)
                                    Debug.LogError("\"Normal Source\" object is not set or has no readable Mesh or Terrain.");
                            }
                            else if (settings.projectionMode == 0)
                            {
                                if (ApplyProjectionBrush2(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples,
                                    m_settings.projectionNormalSourceData, settings.projectionDir))
                                    ++m_brushNumPainted;
                            }
                            else
                            {
                                var rayDirs = settings.projectionRayDir == 0 ? m_normalsBase : m_normals;
                                if (ApplyProjectionBrush(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples,
                                    m_settings.projectionNormalSourceData, rayDirs))
                                    ++m_brushNumPainted;
                            }
                            break;
                        case BrushMode.Reset:
                            if (ApplyResetBrush(m_settings.brushMaskWithSelection, m_rayPos, bd.radius, bd.strength, bd.samples))
                                ++m_brushNumPainted;
                            break;
                    }
                    handled = true;
                }

                if (et == EventType.MouseUp)
                {
                    if (m_brushNumPainted > 0)
                    {
                        PushUndo();
                        m_brushNumPainted = 0;
                        handled = true;
                    }
                }
            }
            else
            {
                var selectMode = m_settings.selectMode;
                float selectSign = e.control ? -1.0f : 1.0f;

                if (selectMode == SelectMode.Single)
                {
                    if (!e.shift && !e.control)
                        System.Array.Clear(m_selection.Array, 0, m_selection.Count);

                    if (settings.selectVertex && SelectVertex(e, selectSign, settings.selectFrontSideOnly))
                    {
                        handled = true;
                    }
                    else if(settings.selectTriangle && SelectTriangle(e, selectSign))
                    {
                        handled = true;
                    }
                    else if(m_rayHit)
                    {
                        handled = true;
                    }
                }
                else if (selectMode == SelectMode.Rect)
                {
                    if (et == EventType.MouseDown)
                    {
                        m_rectDragging = true;
                        m_rectStartPoint = m_rectEndPoint = e.mousePosition;
                        handled = true;
                    }
                    else if (et == EventType.MouseDrag)
                    {
                        m_rectEndPoint = e.mousePosition;
                        handled = true;
                    }
                    else if (et == EventType.MouseUp)
                    {
                        if (m_rectDragging)
                        {
                            m_rectDragging = false;
                            if (!e.shift && !e.control)
                                System.Array.Clear(m_selection.Array, 0, m_selection.Count);

                            m_rectEndPoint = e.mousePosition;
                            handled = true;

                            if (!SelectRect(m_rectStartPoint, m_rectEndPoint, selectSign, settings.selectFrontSideOnly) && !m_rayHit)
                            {
                            }
                            m_rectStartPoint = m_rectEndPoint = -Vector2.one;
                        }
                    }
                }
                else if (selectMode == SelectMode.Lasso)
                {
                    if (et == EventType.MouseDown || et == EventType.MouseDrag)
                    {
                        if(et == EventType.MouseDown)
                        {
                            m_lassoPoints.Clear();
                            m_meshLasso.Clear();
                        }

                        m_lassoPoints.Add(ScreenCoord11(e.mousePosition));
                        handled = true;

                        m_meshLasso.Clear();
                        if (m_lassoPoints.Count > 1)
                        {
                            var vertices = new Vector3[m_lassoPoints.Count];
                            var indices = new int[(vertices.Length - 1) * 2];
                            for (int i = 0; i < vertices.Length; ++i)
                            {
                                vertices[i].x = m_lassoPoints[i].x;
                                vertices[i].y = m_lassoPoints[i].y;
                            }
                            for (int i = 0; i < vertices.Length - 1; ++i)
                            {
                                indices[i * 2 + 0] = i;
                                indices[i * 2 + 1] = i + 1;
                            }
                            m_meshLasso.vertices = vertices;
                            m_meshLasso.SetIndices(indices, MeshTopology.Lines, 0);
                            m_meshLasso.UploadMeshData(false);
                        }
                    }
                    else if (et == EventType.MouseUp)
                    {
                        if (!e.shift && !e.control)
                            System.Array.Clear(m_selection.Array, 0, m_selection.Count);

                        handled = true;
                        if (!SelectLasso(m_lassoPoints.ToArray(), selectSign, settings.selectFrontSideOnly) && !m_rayHit)
                        {
                        }

                        m_lassoPoints.Clear();
                        m_meshLasso.Clear();
                    }
                }
                else if (selectMode == SelectMode.Brush)
                {
                    if (et == EventType.MouseDown && !e.shift && !e.control)
                        System.Array.Clear(m_selection.Array, 0, m_selection.Count);

                    if (et == EventType.MouseDown || et == EventType.MouseDrag)
                    {
                        var bd = m_settings.activeBrush;
                        if (m_rayHit && SelectBrush(m_rayPos, bd.radius, bd.strength * selectSign, bd.samples))
                            handled = true;
                    }
                }

                UpdateSelection();
            }

            if (et == EventType.MouseDown)
            {
                GUIUtility.hotControl = id;
            }
            else if (et == EventType.MouseUp)
            {
                if (GUIUtility.hotControl == id && e.button == 0)
                    GUIUtility.hotControl = 0;
            }
            e.Use();

            if (handled)
            {
                ret |= (int)SceneGUIState.Repaint;
            }
            return ret;
        }


        void OnDrawGizmosSelected()
        {
            if(!m_editing) { return; }

            if (m_matVisualize == null || m_meshPoint == null || m_meshVector == null)
            {
                Debug.LogWarning("NormalEditor: Some resources are missing.\n");
                return;
            }

            bool pickMode = m_settings.pickNormal;
            bool brushMode = !pickMode &&
                (m_settings.editMode == EditMode.Brush ||
                 (m_settings.editMode == EditMode.Select && m_settings.selectMode == SelectMode.Brush));
            bool brushReplace = brushMode && m_settings.brushMode == BrushMode.Replace;
            bool brushProjection = brushMode && m_settings.brushMode == BrushMode.Projection;

            var trans = GetComponent<Transform>();
            var matrix = trans.localToWorldMatrix;
            var renderer = GetComponent<Renderer>();

            m_matVisualize.SetMatrix("_Transform", matrix);
            m_matVisualize.SetFloat("_VertexSize", m_settings.vertexSize);
            m_matVisualize.SetFloat("_NormalSize", m_settings.normalSize);
            m_matVisualize.SetFloat("_TangentSize", m_settings.tangentSize);
            m_matVisualize.SetFloat("_BinormalSize", m_settings.binormalSize);
            m_matVisualize.SetColor("_VertexColor", m_settings.vertexColor);
            m_matVisualize.SetColor("_VertexColor2", m_settings.vertexColor2);
            m_matVisualize.SetColor("_NormalColor", m_settings.normalColor);
            m_matVisualize.SetColor("_TangentColor", m_settings.tangentColor);
            m_matVisualize.SetColor("_BinormalColor", m_settings.binormalColor);
            m_matVisualize.SetInt("_OnlySelected", m_settings.showSelectedOnly ? 1 : 0);

            if (m_rayHit)
            {
                m_matVisualize.SetColor("_VertexColor3", m_settings.vertexColor3);
                if (brushMode)
                {
                    var bd = m_settings.activeBrush;
                    if (m_texBrushSamples == null)
                    {
                        m_texBrushSamples = new Texture2D(bd.samples.Length, 1, TextureFormat.RFloat, false);
                    }
                    m_texBrushSamples.LoadRawTextureData(bd.samples, bd.samples.Length * 4);
                    m_texBrushSamples.Apply();
                    m_matVisualize.SetVector("_BrushPos", new Vector4(m_rayPos.x, m_rayPos.y, m_rayPos.z, bd.radius));
                    m_matVisualize.SetTexture("_BrushSamples", m_texBrushSamples);
                }
                else
                {
                    m_matVisualize.SetVector("_BrushPos", m_rayPos);
                }

                if (pickMode)
                    m_matVisualize.SetVector("_Direction", PickNormal(m_rayPos, m_rayHitTriangle));
                else if (brushReplace)
                    m_matVisualize.SetVector("_Direction", m_settings.assignValue);
                else if (brushProjection)
                    m_matVisualize.SetVector("_Direction", m_settings.projectionDir);
            }
            else
            {
                m_matVisualize.SetColor("_VertexColor3", Color.black);
            }

            if (m_cbPoints != null) m_matVisualize.SetBuffer("_Points", m_cbPoints);
            if (m_cbNormals != null) m_matVisualize.SetBuffer("_Normals", m_cbNormals);
            if (m_cbTangents != null) m_matVisualize.SetBuffer("_Tangents", m_cbTangents);
            if (m_cbSelection != null) m_matVisualize.SetBuffer("_Selection", m_cbSelection);
            if (m_cbBaseNormals != null) m_matVisualize.SetBuffer("_BaseNormals", m_cbBaseNormals);
            if (m_cbBaseTangents != null) m_matVisualize.SetBuffer("_BaseTangents", m_cbBaseTangents);

            if (m_cmdDraw == null)
            {
                m_cmdDraw = new CommandBuffer();
                m_cmdDraw.name = "NormalEditor";
            }
            m_cmdDraw.Clear();

            // overlay
            if(m_settings.modelOverlay != ModelOverlay.None)
            {
                if (m_cbPoints != null) m_matOverlay.SetBuffer("_Points", m_cbPoints);
                if (m_cbNormals != null) m_matOverlay.SetBuffer("_Normals", m_cbNormals);
                if (m_cbTangents != null) m_matOverlay.SetBuffer("_Tangents", m_cbTangents);
                if (m_cbSelection != null) m_matOverlay.SetBuffer("_Selection", m_cbSelection);
                if (m_cbBaseNormals != null) m_matOverlay.SetBuffer("_BaseNormals", m_cbBaseNormals);
                if (m_cbBaseTangents != null) m_matOverlay.SetBuffer("_BaseTangents", m_cbBaseTangents);

                int pass = (int)m_settings.modelOverlay - 1;
                for (int si = 0; si < m_meshTarget.subMeshCount; ++si)
                    m_cmdDraw.DrawRenderer(renderer, m_matOverlay, si, pass);
            }

            // visualize brush range
            if (m_settings.showBrushRange && m_rayHit && brushMode)
                m_cmdDraw.DrawRenderer(renderer, m_matVisualize, 0, (int)VisualizeType.BrushRange);

            if(m_settings.visualize)
            {
                // visualize vertices
                if (m_settings.showVertices && m_points != null)
                    m_cmdDraw.DrawMeshInstancedIndirect(m_meshPoint, 0, m_matVisualize, (int)VisualizeType.Vertices, m_cbArgPoints);

                // visualize binormals
                if (m_settings.showBinormals && m_tangents != null)
                    m_cmdDraw.DrawMeshInstancedIndirect(m_meshVector, 0, m_matVisualize, (int)VisualizeType.Binormals, m_cbArgVectors);

                // visualize tangents
                if (m_settings.showTangents && m_tangents != null)
                    m_cmdDraw.DrawMeshInstancedIndirect(m_meshVector, 0, m_matVisualize, (int)VisualizeType.Tangents, m_cbArgVectors);

                // visualize normals
                if (m_settings.showNormals && m_normals != null)
                    m_cmdDraw.DrawMeshInstancedIndirect(m_meshVector, 0, m_matVisualize, (int)VisualizeType.Normals, m_cbArgVectors);
            }

            if (m_settings.showBrushRange && m_rayHit)
            {
                // ray pos
                if (pickMode || brushMode)
                    m_cmdDraw.DrawMesh(m_meshPoint, Matrix4x4.identity, m_matVisualize, 0, (int)VisualizeType.RayPosition);

                // visualize direction
                if (pickMode || brushReplace)
                    m_cmdDraw.DrawMesh(m_meshVector, Matrix4x4.identity, m_matVisualize, 0, (int)VisualizeType.Direction);
            }
            if(brushProjection && m_settings.projectionMode == 0)
            {
                // visualize projection direction
                m_cmdDraw.DrawMesh(m_meshVector, Matrix4x4.identity, m_matVisualize, 0, (int)VisualizeType.Direction);
            }

            // lasso lines
            if (m_meshLasso.vertexCount > 1)
                m_cmdDraw.DrawMesh(m_meshLasso, Matrix4x4.identity, m_matVisualize, 0, (int)VisualizeType.Lasso);

            Graphics.ExecuteCommandBuffer(m_cmdDraw);
        }

        void OnRepaint()
        {
            if (m_settings.selectMode == SelectMode.Rect && m_rectDragging)
            {
                var selectionRect = typeof(EditorStyles).GetProperty("selectionRect", BindingFlags.NonPublic | BindingFlags.Static);
                if (selectionRect != null)
                {
                    var style = (GUIStyle)selectionRect.GetValue(null, null);
                    Handles.BeginGUI();
                    style.Draw(FromToRect(m_rectStartPoint, m_rectEndPoint), GUIContent.none, false, false, false, false);
                    Handles.EndGUI();
                }
            }
        }

#endif // UNITY_EDITOR
    }
}


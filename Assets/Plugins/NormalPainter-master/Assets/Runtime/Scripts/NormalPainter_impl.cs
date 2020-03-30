using System;
using System.Text.RegularExpressions;
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
#if UNITY_EDITOR
    public enum EditMode
    {
        Select,
        Brush,
        Assign,
        Move,
        Rotate,
        Scale,
        Smooth,
        Projection,
        Reset,
    }

    public enum BrushMode
    {
        Paint,
        Replace,
        Smooth,
        Projection,
        Reset,
        Flow,
    }

    public enum SelectMode
    {
        Single,
        Rect,
        Lasso,
        Brush,
    }

    public enum MirrorMode
    {
        None,
        RightToLeft,
        LeftToRight,
        ForwardToBack,
        BackToForward,
        UpToDown,
        DownToUp,
    }

    public enum TangentsUpdateMode
    {
        Manual,
        Auto,
        Realtime,
    }
    public enum TangentsPrecision
    {
        Fast,
        Precise,
    }

    public enum ImageFormat
    {
        PNG,
        EXR,
    }

    public enum ModelOverlay
    {
        None,
        LocalSpaceNormals,
        TangentSpaceNormals,
        Tangents,
        Binormals,
        UV,
        VertexColor,
    }

    public enum Coordinate
    {
        World,
        Local,
        Pivot,
    }

    public enum SceneGUIState
    {
        Repaint = 1 << 0,
        SelectionChanged = 1 << 1,
    }

    public enum VisualizeType
    {
        Vertices,
        Normals,
        Tangents,
        Binormals,
        Lasso,
        BrushRange,
        RayPosition,
        Direction,
    }

    public class MeshData
    {
        public PinnedList<Vector3> vertices = new PinnedList<Vector3>();
        public PinnedList<Vector3> normals = new PinnedList<Vector3>();
        public PinnedList<Vector2> uv = new PinnedList<Vector2>();
        public PinnedList<int> indices = new PinnedList<int>();
        public Matrix4x4 transform;

        public int vertexCount
        {
            get { return vertices.Count; }
            set
            {
                vertices.Resize(value);
                normals.Resize(value);
                uv.Resize(value);
            }
        }

        public int indexCount
        {
            get { return indices.Count; }
            set
            {
                indices.Resize(value);
            }
        }

        public bool empty { get { return vertices.Count == 0; } }

        public bool Extract(GameObject go)
        {
            if (!go) { return false; }

            var terrain = go.GetComponent<Terrain>();
            if (terrain)
            {
                // terrain doesn't support rotation and scale
                transform = Matrix4x4.TRS(go.GetComponent<Transform>().position, Quaternion.identity, Vector3.one);
                return Extract(terrain);
            }

            transform = go.GetComponent<Transform>().localToWorldMatrix;

            var smi = go.GetComponent<SkinnedMeshRenderer>();
            if (smi != null)
            {
                var mesh = new Mesh();
                smi.BakeMesh(mesh);
                return Extract(mesh);
            }

            var mf = go.GetComponent<MeshFilter>();
            if (mf != null)
            {
                return Extract(mf.sharedMesh);
            }
            return false;

        }

        public bool Extract(Mesh mesh)
        {
            if (!mesh || !mesh.isReadable) { return false; }

            vertexCount = mesh.vertexCount;
            mesh.GetVertices(vertices.List);
            mesh.GetNormals(normals.List);
            mesh.GetUVs(0, uv.List);
            indices = new PinnedList<int>(mesh.triangles);
            return true;
        }

        public bool Extract(Terrain terrain)
        {
            if (!terrain) { return false; }

            var tdata = terrain.terrainData;
            var w = tdata.heightmapResolution;
            var h = tdata.heightmapResolution;
            var heightmap = new PinnedArray2D<float>(tdata.GetHeights(0, 0, w, h));

            vertexCount = w * h;
            indexCount = (w - 1) * (h - 1) * 2 * 3;
            npGenerateTerrainMesh(heightmap, w, h, tdata.size,
                vertices, normals, uv, indices);
            return true;
        }
        [DllImport("NormalPainterCore")]
        static extern void npGenerateTerrainMesh(
            IntPtr heightmap, int width, int height, Vector3 size,
            IntPtr dst_vertices, IntPtr dst_normals, IntPtr dst_uv, IntPtr dst_indices);

        public static implicit operator npMeshData(MeshData v)
        {
            return new npMeshData
            {
                vertices = v.vertices,
                normals = v.normals,
                uv = v.uv,
                indices = v.indices,
                num_vertices = v.vertices.Count,
                num_triangles = v.indices.Count / 3,
                transform = v.transform,
            };
        }
    }

    public struct npMeshData
    {
        public IntPtr indices;
        public IntPtr vertices;
        public IntPtr normals;
        public IntPtr tangents;
        public IntPtr uv;
        public IntPtr selection;
        public int num_vertices;
        public int num_triangles;
        public Matrix4x4 transform;
    }
    public struct npSkinData
    {
        public IntPtr weights;
        public IntPtr bones;
        public IntPtr bindposes;
        public int num_vertices;
        public int num_bones;
        public Matrix4x4 root;
    }
#endif // UNITY_EDITOR



    public partial class NormalPainter : MonoBehaviour
    {
#if UNITY_EDITOR

        public Vector3 ToWorldVector(Vector3 v, Coordinate c)
        {
            switch (c)
            {
                case Coordinate.Local: return GetComponent<Transform>().localToWorldMatrix.MultiplyVector(v);
                case Coordinate.Pivot: return m_settings.pivotRot * v;
            }
            return v;
        }


        public void ApplyAssign(Vector3 v, Coordinate c, bool pushUndo)
        {
            v = ToWorldVector(v, c).normalized;

            npAssign(ref m_npModelData, v);
            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public void ApplyMove(Vector3 v, Coordinate c, bool pushUndo)
        {
            v = ToWorldVector(v, c);

            npMove(ref m_npModelData, v);
            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public void ApplyRotate(Quaternion amount, Quaternion pivotRot, Coordinate c, bool pushUndo)
        {
            var backup = m_npModelData.transform;
            var t = GetComponent<Transform>();
            switch (c)
            {
                case Coordinate.World:
                    m_npModelData.transform = t.localToWorldMatrix;
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Local:
                    m_npModelData.transform = Matrix4x4.identity;
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Pivot:
                    m_npModelData.transform = t.localToWorldMatrix;
                    break;
                default: return;
            }

            npRotate(ref m_npModelData, amount, pivotRot);
            m_npModelData.transform = backup;

            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public void ApplyRotatePivot(Quaternion amount, Vector3 pivotPos, Quaternion pivotRot, Coordinate c, bool pushUndo)
        {
            var backup = m_npModelData.transform;
            var t = GetComponent<Transform>();
            switch (c)
            {
                case Coordinate.World:
                    m_npModelData.transform = t.localToWorldMatrix;
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Local:
                    m_npModelData.transform = Matrix4x4.identity;
                    pivotPos = t.worldToLocalMatrix.MultiplyPoint(pivotPos);
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Pivot:
                    m_npModelData.transform = t.localToWorldMatrix;
                    break;
                default: return;
            }

            npRotatePivot(ref m_npModelData, amount, pivotPos, pivotRot);
            m_npModelData.transform = backup;

            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public void ApplyScale(Vector3 amount, Vector3 pivotPos, Quaternion pivotRot, Coordinate c, bool pushUndo)
        {
            var backup = m_npModelData.transform;
            var t = GetComponent<Transform>();
            switch (c)
            {
                case Coordinate.World:
                    m_npModelData.transform = t.localToWorldMatrix;
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Local:
                    m_npModelData.transform = Matrix4x4.identity;
                    pivotPos = t.worldToLocalMatrix.MultiplyPoint(pivotPos);
                    pivotRot = Quaternion.identity;
                    break;
                case Coordinate.Pivot:
                    m_npModelData.transform = t.localToWorldMatrix;
                    break;
                default: return;
            }

            npScale(ref m_npModelData, amount, pivotPos, pivotRot);
            m_npModelData.transform = backup;

            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public bool ApplyFlowBrush(bool useSelection, Vector3 pos, Vector3 previousPos, float radius, float strength, PinnedArray<float> bsamples, Vector3 baseDir)
        {
            useSelection = useSelection && m_numSelected > 0;
            if (npBrushFlow(ref m_npModelData, pos, previousPos, radius, strength, bsamples.Length, bsamples, baseDir, useSelection) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public bool ApplyPaintBrush(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples, Vector3 baseDir, int blendMode)
        {
            useSelection = useSelection && m_numSelected > 0;
            if (npBrushPaint(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, baseDir, blendMode, useSelection) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public bool ApplyReplaceBrush(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples, Vector3 amount)
        {
            useSelection = useSelection && m_numSelected > 0;
            amount = GetComponent<Transform>().worldToLocalMatrix.MultiplyVector(amount).normalized;

            if (npBrushReplace(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, amount, useSelection) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public bool ApplySmoothBrush(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples)
        {
            useSelection = useSelection && m_numSelected > 0;
            if (npBrushSmooth(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, useSelection) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public bool ApplyProjectionBrush(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples,
            MeshData normalSource, PinnedList<Vector3> rayDirs)
        {
            useSelection = useSelection && m_numSelected > 0;
            var np = (npMeshData)normalSource;
            if (npBrushProjection(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, useSelection, ref np, rayDirs) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }
        public bool ApplyProjectionBrush2(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples,
            MeshData normalSource, Vector3 rayDir)
        {
            useSelection = useSelection && m_numSelected > 0;
            var np = (npMeshData)normalSource;
            if (npBrushProjection2(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, useSelection, ref np, rayDir) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public bool ApplyResetBrush(bool useSelection, Vector3 pos, float radius, float strength, PinnedArray<float> bsamples)
        {
            useSelection = useSelection && m_numSelected > 0;
            if (npBrushLerp(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples, m_normalsBase, m_normals, useSelection) > 0)
            {
                UpdateNormals();
                return true;
            }
            return false;
        }

        public void ResetNormals(bool useSelection, bool pushUndo)
        {
            if (!useSelection)
            {
                Array.Copy(m_normalsBase.Array, m_normals.Array, m_normals.Count);
            }
            else
            {
                for (int i = 0; i < m_normals.Count; ++i)
                    m_normals[i] = Vector3.Lerp(m_normals[i], m_normalsBase[i], m_selection[i]).normalized;
            }
            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        void PushUndo()
        {
            PushUndo(m_normals.Array, null);
        }

        void PushUndo(Vector3[] normals, History.Record[] records)
        {
            Undo.IncrementCurrentGroup();
            Undo.RecordObject(this, "NormalEditor [" + m_history.index + "]");
            m_historyIndex = ++m_history.index;

            if (normals == null)
            {
                m_history.normals = null;
            }
            else
            {
                if (m_history.normals != null && m_history.normals.Length == normals.Length)
                    Array.Copy(normals, m_history.normals, normals.Length);
                else
                    m_history.normals = (Vector3[])normals.Clone();

                if (m_settings.tangentsMode == TangentsUpdateMode.Auto)
                    RecalculateTangents();
            }
            m_history.mesh = m_meshTarget;
            m_history.records = records != null ? (History.Record[])records.Clone() : null;

            Undo.FlushUndoRecordObjects();
        }

        public void OnUndoRedo()
        {
            if (m_historyIndex != m_history.index)
            {
                m_historyIndex = m_history.index;

                if (m_history.mesh != m_meshTarget)
                {
                    m_meshTarget = m_history.mesh;
                    BeginEdit();
                }
                UpdateTransform();
                if (m_history.normals != null && m_normals != null && m_history.normals.Length == m_normals.Count)
                {
                    Array.Copy(m_history.normals, m_normals.Array, m_normals.Count);
                    UpdateNormals(false);

                    if (m_settings.tangentsMode == TangentsUpdateMode.Auto)
                        RecalculateTangents();
                }

                if (m_history.records != null)
                {
                    foreach (var r in m_history.records)
                    {
                        if (r.normals != null) r.mesh.normals = r.normals;
                        if (r.colors != null) r.mesh.colors = r.colors;
                        r.mesh.UploadMeshData(false);
                    }
                }
            }
        }


        bool UpdateBoneMatrices()
        {
            bool ret = false;

            var rootMatrix = GetComponent<Transform>().localToWorldMatrix;
            if (m_npSkinData.root != rootMatrix)
            {
                m_npSkinData.root = rootMatrix;
                ret = true;
            }

            var bones = GetComponent<SkinnedMeshRenderer>().bones;
            for (int i = 0; i < m_boneMatrices.Count; ++i)
            {
                var l2w = bones[i].localToWorldMatrix;
                if (m_boneMatrices[i] != l2w)
                {
                    m_boneMatrices[i] = l2w;
                    ret = true;
                }
            }
            return ret;
        }

        void UpdateTransform()
        {
            m_npModelData.transform = GetComponent<Transform>().localToWorldMatrix;

            if (m_skinned && UpdateBoneMatrices())
            {
                npApplySkinning(ref m_npSkinData,
                    m_pointsPredeformed, m_normalsPredeformed, m_tangentsPredeformed,
                    m_points, m_normals, m_tangents);
                npApplySkinning(ref m_npSkinData,
                    IntPtr.Zero, m_normalsBasePredeformed, m_tangentsBasePredeformed,
                    IntPtr.Zero, m_normalsBase, m_tangentsBase);

                if (m_cbPoints != null) m_cbPoints.SetData(m_points.List);
                if (m_cbNormals != null) m_cbNormals.SetData(m_normals.List);
                if (m_cbBaseNormals != null) m_cbBaseNormals.SetData(m_normalsBase.List);
                if (m_cbTangents != null) m_cbTangents.SetData(m_tangents.List);
                if (m_cbBaseTangents != null) m_cbBaseTangents.SetData(m_tangentsBase.List);
            }
        }

        public void UpdateNormals(bool mirror = true)
        {
            if (m_meshTarget == null) return;

            if (m_skinned)
            {
                UpdateBoneMatrices();
                npApplyReverseSkinning(ref m_npSkinData,
                    IntPtr.Zero, m_normals, IntPtr.Zero,
                    IntPtr.Zero, m_normalsPredeformed, IntPtr.Zero);
                if (mirror)
                {
                    ApplyMirroringInternal();
                    npApplySkinning(ref m_npSkinData,
                        IntPtr.Zero, m_normalsPredeformed, IntPtr.Zero,
                        IntPtr.Zero, m_normals, IntPtr.Zero);
                }
                m_meshTarget.SetNormals(m_normalsPredeformed.List);
            }
            else
            {
                if (mirror)
                    ApplyMirroringInternal();
                m_meshTarget.SetNormals(m_normals.List);
            }

            if (m_settings.tangentsMode == TangentsUpdateMode.Realtime)
                RecalculateTangents();

            m_meshTarget.UploadMeshData(false);
            if (m_cbNormals != null)
                m_cbNormals.SetData(m_normals.List);
        }

        public void UpdateSelection()
        {
            int prevSelected = m_numSelected;

            m_numSelected = npUpdateSelection(ref m_npModelData, ref m_selectionPos, ref m_selectionNormal);

            m_selectionRot = Quaternion.identity;
            if (m_numSelected > 0)
            {
                m_selectionRot = Quaternion.LookRotation(m_selectionNormal);
                m_settings.pivotPos = m_selectionPos;
                m_settings.pivotRot = m_selectionRot;
            }
            else
            {
                m_selectionPos = GetComponent<Transform>().position;
            }

            if (prevSelected == 0 && m_numSelected == 0)
            {
                // no need to upload
            }
            else
            {
                m_cbSelection.SetData(m_selection.List);
            }
        }

        public void RecalculateTangents(bool updateMesh = true)
        {
            RecalculateTangents(m_settings.tangentsPrecision, updateMesh);
        }
        public void RecalculateTangents(TangentsPrecision precision, bool updateMesh = true)
        {
            if (precision == TangentsPrecision.Precise)
            {
                m_meshTarget.RecalculateTangents();
                m_tangentsPredeformed.LockList(l =>
                {
                    m_meshTarget.GetTangents(l);
                });

                if (m_skinned)
                    npApplySkinning(ref m_npSkinData,
                        IntPtr.Zero, IntPtr.Zero, m_tangentsPredeformed,
                        IntPtr.Zero, IntPtr.Zero, m_tangents);
            }
            else
            {
                if (m_skinned)
                {
                    npMeshData tmp = m_npModelData;
                    tmp.vertices = m_pointsPredeformed;
                    tmp.normals = m_normalsPredeformed;
                    npGenerateTangents(ref tmp, m_tangentsPredeformed);
                    npApplySkinning(ref m_npSkinData,
                        IntPtr.Zero, IntPtr.Zero, m_tangentsPredeformed,
                        IntPtr.Zero, IntPtr.Zero, m_tangents);
                }
                else
                {
                    npGenerateTangents(ref m_npModelData, m_tangents);
                }
            }

            if (updateMesh)
                m_meshTarget.SetTangents(m_tangentsPredeformed.List);

            if (m_cbTangents != null)
                m_cbTangents.SetData(m_tangents.List);
        }


        public bool Raycast(Event e, ref Vector3 pos, ref int ti)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            float d = 0.0f;
            if (Raycast(ray, ref ti, ref d))
            {
                pos = ray.origin + ray.direction * d;
                return true;
            }
            return false;
        }

        public bool Raycast(Ray ray, ref int ti, ref float distance)
        {
            bool ret = npRaycast(ref m_npModelData, ray.origin, ray.direction, ref ti, ref distance) > 0;
            return ret;
        }

        public Vector3 PickNormal(Vector3 pos, int ti)
        {
            return npPickNormal(ref m_npModelData, pos, ti);
        }

        public Vector3 PickBaseNormal(Vector3 pos, int ti)
        {
            m_npModelData.normals = m_normalsBase;
            var ret = npPickNormal(ref m_npModelData, pos, ti);
            m_npModelData.normals = m_normals;
            return ret;
        }


        public bool SelectEdge(float strength, bool clear)
        {
            bool mask = m_numSelected > 0;
            return npSelectEdge(ref m_npModelData, strength, clear, mask) > 0;
        }
        public bool SelectHole(float strength, bool clear)
        {
            bool mask = m_numSelected > 0;
            return npSelectHole(ref m_npModelData, strength, clear, mask) > 0;
        }

        public bool SelectConnected(float strength, bool clear)
        {
            if (m_numSelected == 0)
                return SelectAll();
            else
                return npSelectConnected(ref m_npModelData, strength, clear) > 0;
        }

        public bool SelectAll()
        {
            for (int i = 0; i < m_selection.Count; ++i)
                m_selection[i] = 1.0f;
            return m_selection.Count > 0;
        }

        public bool InvertSelection()
        {
            for (int i = 0; i < m_selection.Count; ++i)
                m_selection[i] = 1.0f - m_selection[i];
            return m_selection.Count > 0;
        }

        public bool ClearSelection()
        {
            System.Array.Clear(m_selection.Array, 0, m_selection.Count);
            return m_selection.Count > 0;
        }

        public static Vector2 ScreenCoord11(Vector2 v)
        {
            var cam = SceneView.lastActiveSceneView.camera;
            var pixelRect = cam.pixelRect;
            var rect = cam.rect;
            return new Vector2(
                    v.x / pixelRect.width * rect.width * 2.0f - 1.0f,
                    (v.y / pixelRect.height * rect.height * 2.0f - 1.0f) * -1.0f);
        }

        public bool SelectVertex(Event e, float strength, bool frontFaceOnly)
        {
            var center = e.mousePosition;
            var size = new Vector2(15.0f, 15.0f);
            var r1 = center - size;
            var r2 = center + size;
            return SelectVertex(r1, r2, strength, frontFaceOnly);
        }
        public bool SelectVertex(Vector2 r1, Vector2 r2, float strength, bool frontFaceOnly)
        {
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) { return false; }

            var campos = cam.GetComponent<Transform>().position;
            var trans = GetComponent<Transform>().localToWorldMatrix;
            var mvp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix * trans;
            r1 = ScreenCoord11(r1);
            r2 = ScreenCoord11(r2);
            var rmin = new Vector2(Math.Min(r1.x, r2.x), Math.Min(r1.y, r2.y));
            var rmax = new Vector2(Math.Max(r1.x, r2.x), Math.Max(r1.y, r2.y));

            return npSelectSingle(ref m_npModelData, ref mvp, rmin, rmax, campos, strength, frontFaceOnly) > 0;
        }

        public bool SelectTriangle(Event e, float strength)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            return SelectTriangle(ray, strength);
        }
        public bool SelectTriangle(Ray ray, float strength)
        {
            Matrix4x4 trans = GetComponent<Transform>().localToWorldMatrix;
            return npSelectTriangle(ref m_npModelData, ray.origin, ray.direction, strength) > 0;
        }


        public bool SelectRect(Vector2 r1, Vector2 r2, float strength, bool frontFaceOnly)
        {
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) { return false; }

            var campos = cam.GetComponent<Transform>().position;
            var trans = GetComponent<Transform>().localToWorldMatrix;
            var mvp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix * trans;
            r1 = ScreenCoord11(r1);
            r2 = ScreenCoord11(r2);
            var rmin = new Vector2(Math.Min(r1.x, r2.x), Math.Min(r1.y, r2.y));
            var rmax = new Vector2(Math.Max(r1.x, r2.x), Math.Max(r1.y, r2.y));

            return npSelectRect(ref m_npModelData,
                ref mvp, rmin, rmax, campos, strength, frontFaceOnly) > 0;
        }

        public bool SelectLasso(Vector2[] lasso, float strength, bool frontFaceOnly)
        {
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) { return false; }

            var campos = cam.GetComponent<Transform>().position;
            var trans = GetComponent<Transform>().localToWorldMatrix;
            var mvp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix * trans;

            return npSelectLasso(ref m_npModelData,
                ref mvp, lasso, lasso.Length, campos, strength, frontFaceOnly) > 0;
        }

        public bool SelectBrush(Vector3 pos, float radius, float strength, PinnedArray<float> bsamples)
        {
            Matrix4x4 trans = GetComponent<Transform>().localToWorldMatrix;
            return npSelectBrush(ref m_npModelData, pos, radius, strength, bsamples.Length, bsamples) > 0;
        }

        public static Vector3 GetMirrorPlane(MirrorMode mirrorMode)
        {
            switch (mirrorMode)
            {
                case MirrorMode.RightToLeft: return Vector3.left;
                case MirrorMode.LeftToRight: return Vector3.right;
                case MirrorMode.ForwardToBack: return Vector3.back;
                case MirrorMode.BackToForward: return Vector3.forward;
                case MirrorMode.UpToDown: return Vector3.down;
                case MirrorMode.DownToUp: return Vector3.up;
            }
            return Vector3.up;
        }

        MirrorMode m_prevMirrorMode;

        bool ApplyMirroringInternal()
        {
            if (m_settings.mirrorMode == MirrorMode.None) return false;

            bool needsSetup = false;
            if (m_mirrorRelation == null || m_mirrorRelation.Count != m_points.Count)
            {
                m_mirrorRelation = new PinnedList<int>(m_points.Count);
                needsSetup = true;
            }
            else if (m_prevMirrorMode != m_settings.mirrorMode)
            {
                m_prevMirrorMode = m_settings.mirrorMode;
                needsSetup = true;
            }

            Vector3 planeNormal = GetMirrorPlane(m_settings.mirrorMode);
            if (needsSetup)
            {
                npMeshData tmp = m_npModelData;
                tmp.vertices = m_pointsPredeformed;
                tmp.normals = m_normalsBasePredeformed;
                if (npBuildMirroringRelation(ref tmp, planeNormal, 0.0001f, m_mirrorRelation) == 0)
                {
                    Debug.LogWarning("NormalEditor: this mesh seems not symmetric");
                    m_mirrorRelation = null;
                    m_settings.mirrorMode = MirrorMode.None;
                    return false;
                }
            }

            npApplyMirroring(m_normals.Count, m_mirrorRelation, planeNormal, m_normalsPredeformed);
            return true;
        }
        public bool ApplyMirroring(bool pushUndo)
        {
            ApplyMirroringInternal();
            UpdateNormals();
            if (pushUndo) PushUndo();
            return true;
        }


        static Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect r = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (r.width < 0)
            {
                r.x += r.width;
                r.width = -r.width;
            }
            if (r.height < 0)
            {
                r.y += r.height;
                r.height = -r.height;
            }
            return r;
        }

        public bool BakeToTexture(int width, int height, string pathBase, ImageFormat format, bool separateSubmesh)
        {
            if (pathBase == null || pathBase.Length == 0)
                return false;

            m_matBake.SetBuffer("_BaseNormals", m_cbBaseNormals);
            m_matBake.SetBuffer("_BaseTangents", m_cbBaseTangents);

            int numSubmeshes = m_meshTarget.subMeshCount;

            if (separateSubmesh && numSubmeshes > 1)
            {
                for (int si = 0; si < numSubmeshes; ++si)
                {
                    var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
                    var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
                    rt.Create();

                    m_cmdDraw.Clear();
                    m_cmdDraw.SetRenderTarget(rt);
                    m_cmdDraw.DrawMesh(m_meshTarget, Matrix4x4.identity, m_matBake, si, 0);
                    Graphics.ExecuteCommandBuffer(m_cmdDraw);

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                    tex.Apply();
                    RenderTexture.active = null;

                    var regex = new Regex("\\..+$");
                    var path = regex.Replace(pathBase, "");
                    path += string.Format("_submesh{0}", si);
                    switch (format)
                    {
                        case ImageFormat.PNG: path += ".png"; break;
                        case ImageFormat.EXR: path += ".exr"; break;
                    }

                    switch (format)
                    {
                        case ImageFormat.PNG: System.IO.File.WriteAllBytes(path, tex.EncodeToPNG()); break;
                        case ImageFormat.EXR: System.IO.File.WriteAllBytes(path, tex.EncodeToEXR()); break;
                        default: Debug.LogError("Unknown format"); break;
                    }

                    DestroyImmediate(tex);
                    DestroyImmediate(rt);
                }
            }
            else
            {
                var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
                rt.Create();

                m_cmdDraw.Clear();
                m_cmdDraw.SetRenderTarget(rt);
                for (int si = 0; si < numSubmeshes; ++si)
                    m_cmdDraw.DrawMesh(m_meshTarget, Matrix4x4.identity, m_matBake, si, 0);
                Graphics.ExecuteCommandBuffer(m_cmdDraw);

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                tex.Apply();
                RenderTexture.active = null;

                switch (format)
                {
                    case ImageFormat.PNG: System.IO.File.WriteAllBytes(pathBase, tex.EncodeToPNG()); break;
                    case ImageFormat.EXR: System.IO.File.WriteAllBytes(pathBase, tex.EncodeToEXR()); break;
                    default: Debug.LogError("Unknown format"); break;
                }

                DestroyImmediate(tex);
                DestroyImmediate(rt);
            }
            return true;
        }

        public bool BakeToVertexColor(bool pushUndo)
        {
            if (pushUndo)
            {
                var record = new History.Record { mesh = m_meshTarget, colors = m_meshTarget.colors };
                PushUndo(null, new History.Record[1] { record });
            }

            var colors = new Color[m_normals.Count];
            for (int i = 0; i < colors.Length; ++i)
            {
                var base_tangent = m_tangentsBase[i];
                var base_normal = m_normalsBase[i];
                Vector3 base_binormal = Vector3.Normalize(Vector3.Cross(base_normal, base_tangent) * base_tangent.w);
                var tbn = new Matrix4x4(new Vector4(base_tangent.x, base_tangent.y, base_tangent.z),
                                        new Vector4(base_binormal.x, base_binormal.y, base_binormal.z),
                                        new Vector4(base_normal.x, base_normal.y, base_normal.z),
                                        Vector4.zero).transpose;
                var new_normal = tbn.MultiplyVector(m_normals[i]).normalized;

                colors[i].r = new_normal.x * 0.5f + 0.5f;
                colors[i].g = new_normal.y * 0.5f + 0.5f;
                colors[i].b = new_normal.z * 0.5f + 0.5f;
                colors[i].a = 1;// m_meshTarget.colors[i].a; // ?????????
            }
            m_meshTarget.colors = colors;

            if (pushUndo)
            {
                var record = new History.Record { mesh = m_meshTarget, colors = colors };
                PushUndo(null, new History.Record[1] { record });
            }
            return true;
        }

        public bool LoadTexture(Texture tex, bool pushUndo)
        {
            if (tex == null)
                return false;

            bool packed = false;
            {
                var path = AssetDatabase.GetAssetPath(tex);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                    packed = importer.textureType == TextureImporterType.NormalMap;
            }

            var cbUV = new ComputeBuffer(m_normals.Count, 8);
            cbUV.SetData(m_meshTarget.uv);

            m_csBakeFromMap.SetInt("_Packed", packed ? 1 : 0);
            m_csBakeFromMap.SetTexture(0, "_NormalMap", tex);
            m_csBakeFromMap.SetBuffer(0, "_UV", cbUV);
            m_csBakeFromMap.SetBuffer(0, "_Normals", m_cbBaseNormals);
            m_csBakeFromMap.SetBuffer(0, "_Tangents", m_cbBaseTangents);
            m_csBakeFromMap.SetBuffer(0, "_Dst", m_cbNormals);
            m_csBakeFromMap.Dispatch(0, m_normals.Count, 1, 1);
            m_cbNormals.GetData(m_normals.Array);
            cbUV.Dispose();

            UpdateNormals();
            if (pushUndo) PushUndo();

            return true;
        }

        public bool LoadVertexColor(bool pushUndo)
        {
            var color = m_meshTarget.colors;
            if (color.Length != m_normals.Count)
                return false;

            for (int i = 0; i < color.Length; ++i)
            {
                var c = color[i];

                var base_tangent = m_tangentsBase[i];
                var base_normal = m_normalsBase[i];
                Vector3 base_binormal = Vector3.Normalize(Vector3.Cross(base_normal, base_tangent) * base_tangent.w);
                var tbn = new Matrix4x4(new Vector4(base_tangent.x, base_tangent.y, base_tangent.z),
                                        new Vector4(base_binormal.x, base_binormal.y, base_binormal.z),
                                        new Vector4(base_normal.x, base_normal.y, base_normal.z),
                                        Vector4.zero);

                m_normals[i] = tbn.MultiplyVector(new Vector3(
                                c.r * 2 - 1,
                                c.g * 2 - 1,
                                c.b * 2 - 1
                                )).normalized;
            }
            UpdateNormals();
            if (pushUndo) PushUndo();
            return true;
        }

        public void ApplySmoothing(float radius, float strength, bool pushUndo)
        {
            bool mask = m_numSelected > 0;
            npSmooth(ref m_npModelData, radius, strength, mask);

            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public bool ApplyWelding(bool smoothing, float weldAngle, bool pushUndo)
        {
            bool mask = m_numSelected > 0;
            if (npWeld(ref m_npModelData, smoothing, weldAngle, mask) > 0)
            {
                UpdateNormals();
                if (pushUndo) PushUndo();
                return true;
            }
            return false;
        }


        class WeldData
        {
            public bool skinned;
            public Mesh mesh;
            public Matrix4x4 transform;
            public PinnedList<Vector3> vertices;
            public PinnedList<Vector3> normals;

            public PinnedList<BoneWeight> weights;
            public PinnedList<Matrix4x4> bones;
            public PinnedList<Matrix4x4> bindposes;
            public npSkinData skinData;
        }

        public static bool IsValidMesh(Mesh mesh)
        {
            if (!mesh || mesh.vertexCount == 0) return false;
            if (!mesh.isReadable)
            {
                Debug.LogWarning("Mesh " + mesh.name + " is not readable.");
                return false;
            }
            return true;
        }

        public bool ApplyWelding2(GameObject[] targets, int weldMode, float weldAngle, bool pushUndo)
        {
            var data = new List<WeldData>();

            // build weld data
            foreach (var t in targets)
            {
                if (!t || t == this.gameObject) { continue; }

                var smr = t.GetComponent<SkinnedMeshRenderer>();
                if (smr)
                {
                    var mesh = smr.sharedMesh;
                    if (!IsValidMesh(mesh)) continue;

                    var d = new WeldData();
                    d.skinned = true;
                    d.mesh = mesh;
                    d.transform = t.GetComponent<Transform>().localToWorldMatrix;
                    d.vertices = new PinnedList<Vector3>(mesh.vertices);
                    d.normals = new PinnedList<Vector3>(mesh.normals);

                    d.weights = new PinnedList<BoneWeight>(mesh.boneWeights);
                    d.bindposes = new PinnedList<Matrix4x4>(mesh.bindposes);
                    d.bones = new PinnedList<Matrix4x4>(d.bindposes.Count);

                    var bones = smr.bones;
                    int n = System.Math.Min(d.bindposes.Count, smr.bones.Length);
                    for (int bi = 0; bi < n; ++bi)
                        d.bones[bi] = smr.bones[bi].localToWorldMatrix;

                    d.skinData.num_vertices = d.vertices.Count;
                    d.skinData.num_bones = d.bindposes.Count;
                    d.skinData.weights = d.weights;
                    d.skinData.bindposes = d.bindposes;
                    d.skinData.bones = d.bones;
                    d.skinData.root = d.transform;

                    npApplySkinning(ref d.skinData,
                        d.vertices, d.normals, IntPtr.Zero,
                        d.vertices, d.normals, IntPtr.Zero);

                    data.Add(d);
                }

                var mr = t.GetComponent<MeshRenderer>();
                if (mr)
                {
                    var mesh = t.GetComponent<MeshFilter>().sharedMesh;
                    if (!IsValidMesh(mesh)) continue;

                    var d = new WeldData();
                    d.mesh = mesh;
                    d.transform = t.GetComponent<Transform>().localToWorldMatrix;
                    d.vertices = new PinnedList<Vector3>(mesh.vertices);
                    d.normals = new PinnedList<Vector3>(mesh.normals);
                    data.Add(d);
                }
            }

            if (data.Count == 0)
            {
                Debug.LogWarning("Nothing to weld.");
                return false;
            }

            // create data to pass to C++
            bool mask = m_numSelected > 0;
            npMeshData[] tdata = new npMeshData[data.Count];
            for (int i = 0; i < data.Count; ++i)
            {
                tdata[i].num_vertices = data[i].vertices.Count;
                tdata[i].vertices = data[i].vertices;
                tdata[i].normals = data[i].normals;
                tdata[i].transform = data[i].transform;
            }

            Vector3[] normalsPreWeld = null;
            if (pushUndo)
                normalsPreWeld = m_normals.Clone().Array;

            // do weld
            bool ret = false;
            if (npWeld2(ref m_npModelData, tdata.Length, tdata, weldMode, weldAngle, mask) > 0)
            {
                // data to undo
                History.Record[] records = null;

                if (pushUndo && (weldMode == 0 || weldMode == 2))
                {
                    records = new History.Record[data.Count];

                    for (int i = 0; i < data.Count; ++i)
                    {
                        records[i] = new History.Record();
                        records[i].mesh = data[i].mesh;
                        records[i].normals = data[i].mesh.normals;
                    }
                    PushUndo(normalsPreWeld, records);
                }

                // update normals
                if (weldMode == 1 || weldMode == 2)
                {
                    UpdateNormals();
                }
                if (weldMode == 0 || weldMode == 2)
                {
                    foreach (var d in data)
                    {
                        if (d.skinned)
                        {
                            npApplyReverseSkinning(ref d.skinData,
                                IntPtr.Zero, d.normals, IntPtr.Zero,
                                IntPtr.Zero, d.normals, IntPtr.Zero);
                        }
                        d.mesh.normals = d.normals.Array;
                        d.mesh.UploadMeshData(false);
                    }

                    if (pushUndo)
                    {
                        for (int i = 0; i < data.Count; ++i)
                        {
                            records[i] = new History.Record();
                            records[i].mesh = data[i].mesh;
                            records[i].normals = data[i].normals.Array;
                        }
                    }
                }

                if (pushUndo)
                    PushUndo(m_normals.Array, records);

                ret = true;
            }

            return ret;
        }

        public void ApplyProjection(GameObject go, PinnedList<Vector3> rayDirs, bool pushUndo)
        {
            var mdata = new MeshData();
            if (mdata.Extract(go))
                ApplyProjection(mdata, rayDirs, pushUndo);
        }
        public void ApplyProjection(MeshData normalSource, PinnedList<Vector3> rayDirs, bool pushUndo)
        {
            bool mask = m_numSelected > 0;
            var np = (npMeshData)normalSource;
            npProjectNormals(ref m_npModelData, ref np, rayDirs, mask);

            UpdateNormals();
            if (pushUndo) PushUndo();
        }

        public void ApplyProjection2(GameObject go, Vector3 rayDir, bool pushUndo)
        {
            var mdata = new MeshData();
            if (mdata.Extract(go))
                ApplyProjection2(mdata, rayDir, pushUndo);
        }
        public void ApplyProjection2(MeshData normalSource, Vector3 rayDir, bool pushUndo)
        {
            bool mask = m_numSelected > 0;
            var np = (npMeshData)normalSource;
            npProjectNormals2(ref m_npModelData, ref np, rayDir, mask);

            UpdateNormals();
            if (pushUndo) PushUndo();
        }


        public void ResetToBindpose(bool pushUndo)
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr == null || smr.bones == null || smr.sharedMesh == null) { return; }

            var bones = smr.bones;
            var bindposes = smr.sharedMesh.bindposes;
            var bindposeMap = new Dictionary<Transform, Matrix4x4>();

            for (int i = 0; i < bones.Length; i++)
            {
                if (!bindposeMap.ContainsKey(bones[i]))
                {
                    bindposeMap.Add(bones[i], bindposes[i]);
                }
            }

            if (pushUndo)
                Undo.RecordObjects(bones, "NormalPainter: ResetToBindpose");

            foreach (var kvp in bindposeMap)
            {
                var bone = kvp.Key;
                var imatrix = kvp.Value;
                var localMatrix =
                    bindposeMap.ContainsKey(bone.parent) ? (imatrix * bindposeMap[bone.parent].inverse).inverse : imatrix.inverse;

                bone.localPosition = localMatrix.MultiplyPoint(Vector3.zero);
                bone.localRotation = Quaternion.LookRotation(localMatrix.GetColumn(2), localMatrix.GetColumn(1));
                bone.localScale = new Vector3(localMatrix.GetColumn(0).magnitude, localMatrix.GetColumn(1).magnitude, localMatrix.GetColumn(2).magnitude);
            }

            if (pushUndo)
                Undo.FlushUndoRecordObjects();
        }

        public void ExportSettings(string path)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(Instantiate(m_settings), path);
        }

        [DllImport("NormalPainterCore")]
        static extern int npRaycast(
            ref npMeshData model, Vector3 pos, Vector3 dir, ref int tindex, ref float distance);

        [DllImport("NormalPainterCore")]
        static extern Vector3 npPickNormal(
            ref npMeshData model, Vector3 pos, int ti);

        [DllImport("NormalPainterCore")]
        static extern int npSelectSingle(
            ref npMeshData model, ref Matrix4x4 viewproj, Vector2 rmin, Vector2 rmax, Vector3 campos, float strength, bool frontfaceOnly);

        [DllImport("NormalPainterCore")]
        static extern int npSelectTriangle(
            ref npMeshData model, Vector3 pos, Vector3 dir, float strength);

        [DllImport("NormalPainterCore")]
        static extern int npSelectEdge(
            ref npMeshData model, float strength, bool clear, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npSelectHole(
            ref npMeshData model, float strength, bool clear, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npSelectConnected(
            ref npMeshData model, float strength, bool clear);

        [DllImport("NormalPainterCore")]
        static extern int npSelectRect(
            ref npMeshData model, ref Matrix4x4 viewproj, Vector2 rmin, Vector2 rmax, Vector3 campos, float strength, bool frontfaceOnly);

        [DllImport("NormalPainterCore")]
        static extern int npSelectLasso(
            ref npMeshData model, ref Matrix4x4 viewproj, Vector2[] lasso, int numLassoPoints, Vector3 campos, float strength, bool frontfaceOnly);

        [DllImport("NormalPainterCore")]
        static extern int npSelectBrush(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples);

        [DllImport("NormalPainterCore")]
        static extern int npUpdateSelection(
            ref npMeshData model,
            ref Vector3 selection_pos, ref Vector3 selection_normal);


        [DllImport("NormalPainterCore")]
        static extern int npBrushReplace(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, Vector3 amount, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npBrushFlow(
            ref npMeshData model,
            Vector3 pos, Vector3 prevPos, float radius, float strength, int num_bsamples, IntPtr bsamples, Vector3 baseNormal, bool mask);


        [DllImport("NormalPainterCore")]
        static extern int npBrushPaint(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, Vector3 baseNormal, int blend_mode, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npBrushSmooth(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npBrushProjection(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, bool mask,
            ref npMeshData normal_source, IntPtr ray_dirs);
        [DllImport("NormalPainterCore")]
        static extern int npBrushProjection2(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, bool mask,
            ref npMeshData normal_source, Vector3 ray_dir);

        [DllImport("NormalPainterCore")]
        static extern int npBrushLerp(
            ref npMeshData model,
            Vector3 pos, float radius, float strength, int num_bsamples, IntPtr bsamples, IntPtr baseNormals, IntPtr normals, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npAssign(
            ref npMeshData model, Vector3 value);

        [DllImport("NormalPainterCore")]
        static extern int npMove(
            ref npMeshData model, Vector3 amount);

        [DllImport("NormalPainterCore")]
        static extern int npRotate(
            ref npMeshData model, Quaternion amount, Quaternion pivotRot);

        [DllImport("NormalPainterCore")]
        static extern int npRotatePivot(
            ref npMeshData model, Quaternion amount, Vector3 pivotPos, Quaternion pivotRot);

        [DllImport("NormalPainterCore")]
        static extern int npScale(
            ref npMeshData model, Vector3 amount, Vector3 pivotPos, Quaternion pivotRot);

        [DllImport("NormalPainterCore")]
        static extern int npSmooth(
            ref npMeshData model, float radius, float strength, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npWeld(
            ref npMeshData model, bool smoothing, float weldAngle, bool mask);
        [DllImport("NormalPainterCore")]
        static extern int npWeld2(
            ref npMeshData model, int num_targets, npMeshData[] targets,
            int weldMode, float weldAngle, bool mask);

        [DllImport("NormalPainterCore")]
        static extern int npBuildMirroringRelation(
            ref npMeshData model, Vector3 plane_normal, float epsilon, IntPtr relation);

        [DllImport("NormalPainterCore")]
        static extern void npApplyMirroring(
            int num_vertices, IntPtr relation, Vector3 plane_normal, IntPtr normals);

        [DllImport("NormalPainterCore")]
        static extern void npProjectNormals(
            ref npMeshData model, ref npMeshData target, IntPtr ray_dir, bool mask);
        [DllImport("NormalPainterCore")]
        static extern void npProjectNormals2(
            ref npMeshData model, ref npMeshData target, Vector3 ray_dir, bool mask);

        [DllImport("NormalPainterCore")]
        static extern void npApplySkinning(
            ref npSkinData skin,
            IntPtr ipoints, IntPtr inormals, IntPtr itangents,
            IntPtr opoints, IntPtr onormals, IntPtr otangents);

        [DllImport("NormalPainterCore")]
        static extern void npApplyReverseSkinning(
            ref npSkinData skin,
            IntPtr ipoints, IntPtr inormals, IntPtr itangents,
            IntPtr opoints, IntPtr onormals, IntPtr otangents);

        [DllImport("NormalPainterCore")]
        static extern int npGenerateNormals(
            ref npMeshData model, IntPtr dst);
        [DllImport("NormalPainterCore")]
        static extern int npGenerateTangents(
            ref npMeshData model, IntPtr dst);

        [DllImport("NormalPainterCore")] static extern void npInitializePenInput();
#endif // UNITY_EDITOR
    }
}

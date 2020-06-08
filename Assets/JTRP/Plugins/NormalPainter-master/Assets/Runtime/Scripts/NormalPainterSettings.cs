using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTJ.NormalPainter
{
#if UNITY_EDITOR
    [Serializable]
    public class BrushData
    {
        public static Mesh s_quad;
        public static Material s_mat;

        public float maxRadius = 3.0f;
        public float radius = 0.2f;
        public float strength = 0.2f;
        public AnimationCurve curve = new AnimationCurve();

        [NonSerialized] public PinnedArray<float> samples = new PinnedArray<float>(256);
        [NonSerialized] public RenderTexture image;

        public void UpdateSamples()
        {
            // update samples
            float unit = 1.0f / (samples.Length - 1);
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = Mathf.Clamp01(curve.Evaluate(unit * i));
            }

            // update image
            if (s_mat == null)
            {
                s_mat = new Material(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath("a19852f0736178441b093ba995baff4a")));
            }
            if (s_quad == null)
            {
                float l = 1.0f;
                s_quad = new Mesh();
                s_quad.vertices = new Vector3[] {
                    new Vector3(-l,-l, 0.0f),
                    new Vector3( l,-l, 0.0f),
                    new Vector3( l, l, 0.0f),
                    new Vector3(-l, l, 0.0f),
                };
                s_quad.SetIndices(new int[] {
                    0, 1, 2, 0, 2, 3,
                }, MeshTopology.Triangles, 0);
            }

            var cb = new ComputeBuffer(samples.Length, 4);
            cb.SetData(samples.Array);
            s_mat.SetInt("_NumBrushSamples", samples.Length);
            s_mat.SetBuffer("_BrushSamples", cb);

            if (image == null)
            {
                image = new RenderTexture(48, 24, 0, RenderTextureFormat.ARGB32);
                image.Create();
            }

            var rtBackup = RenderTexture.active;
            RenderTexture.active = image;
            s_mat.SetPass(0);
            Graphics.DrawMeshNow(s_quad, Matrix4x4.identity, 0);
            cb.Release();
            RenderTexture.active = rtBackup;
        }
    }

    public class NormalPainterSettings : ScriptableObject
    {
        [Serializable]
        public class SelectionSet
        {
            public float[] selection;
        }

        public TangentsUpdateMode tangentsMode = TangentsUpdateMode.Auto;
        public TangentsPrecision tangentsPrecision = TangentsPrecision.Fast;

        // edit options
        public EditMode editMode = EditMode.Select;
        public BrushMode brushMode = BrushMode.Paint;
        public SelectMode selectMode = SelectMode.Single;
        public MirrorMode mirrorMode = MirrorMode.None;
        public bool selectFrontSideOnly = true;
        public bool selectVertex = true;
        public bool selectTriangle = true;
        public bool rotatePivot = false;
        public bool brushMaskWithSelection = true;
        public int brushBlendMode = 0;

        public BrushData[] brushData = new BrushData[5] {
            new BrushData(),
            new BrushData(),
            new BrushData(),
            new BrushData(),
            new BrushData(),
        };
        [NonSerialized] public int brushActiveSlot = 0;

        [NonSerialized] public bool pickNormal = false;

        public int projectionMode;
        public Vector3 projectionDir = new Vector3(0, -1, 0);
        public int projectionRayDir;
        GameObject _projectionNormalSource;
        MeshData _projectionNormalSourceData;

        public GameObject projectionNormalSource
        {
            get { return _projectionNormalSource; }
            set
            {
                if(value != _projectionNormalSource)
                {
                    _projectionNormalSource = value;
                    _projectionNormalSourceData = null;
                }
            }
        }
        public MeshData projectionNormalSourceData
        {
            get
            {
                if(_projectionNormalSourceData == null && _projectionNormalSource != null)
                {
                    var md = new MeshData();
                    if (md.Extract(_projectionNormalSource))
                        _projectionNormalSourceData = md;
                }
                return _projectionNormalSourceData;
            }
        }

        // display options
        public bool showVertices = true;
        public bool showNormals = true;
        public bool showTangents = false;
        public bool showBinormals = false;
        public bool visualize = true;
        public bool showSelectedOnly = false;
        public bool showBrushRange = true;
        public ModelOverlay modelOverlay = ModelOverlay.None;
        public float vertexSize;
        public float normalSize;
        public float tangentSize;
        public float binormalSize;
        public Color vertexColor;
        public Color vertexColor2;
        public Color vertexColor3;
        public Color normalColor;
        public Color tangentColor;
        public Color binormalColor;


        // inspector states

        [NonSerialized] public Vector3 pivotPos;
        [NonSerialized] public Quaternion pivotRot = Quaternion.identity;

        [NonSerialized] public bool foldEdit = true;
        [NonSerialized] public bool foldMisc = true;
        [NonSerialized] public bool foldInExport = false;
        [NonSerialized] public bool foldDisplay = true;
        [NonSerialized] public int displayIndex;
        [NonSerialized] public int inexportIndex;
        [NonSerialized] public bool foldTangents = true;

        [NonSerialized] public Coordinate coordinate = Coordinate.World;
        [NonSerialized] public Vector3  assignValue = Vector3.up;
        [NonSerialized] public Vector3  moveAmount;
        [NonSerialized] public Vector3  rotateAmount;
        [NonSerialized] public Vector3 scaleAmount;

        [NonSerialized] public int smoothMode = 0;
        [NonSerialized] public float smoothRadius = 0.5f;
        [NonSerialized] public float smoothAmount = 1.0f;
        [NonSerialized] public float weldAngle = 60.0f;
        [NonSerialized] public bool weldWithSmoothing = true;
        [NonSerialized] public int weldTargetsMode = 2;
        [NonSerialized] public GameObject[] weldTargets = new GameObject[1];

        [NonSerialized] public ImageFormat bakeFormat = ImageFormat.PNG;
        [NonSerialized] public int bakeWidth = 1024;
        [NonSerialized] public int bakeHeight = 1024;
        [NonSerialized] public bool bakeSeparateSubmeshes = true;
        [NonSerialized] public bool bakeVertexColor01 = true;

        [NonSerialized] public Texture bakeSource;

        [NonSerialized] public bool objFlipHandedness = true;
        [NonSerialized] public bool objFlipFaces = false;
        [NonSerialized] public bool objApplyTransform = false;
        [NonSerialized] public bool objMakeSubmeshes = true;
        [NonSerialized] public bool objIncludeChildren = false;

        [NonSerialized] public SelectionSet[] selectionSets = new SelectionSet[5] {
            new SelectionSet(),
            new SelectionSet(),
            new SelectionSet(),
            new SelectionSet(),
            new SelectionSet(),
        };


        public BrushData activeBrush { get { return brushData[brushActiveSlot]; } }


        public NormalPainterSettings()
        {
            ResetDisplayOptions();
        }

        public void ResetDisplayOptions()
        {
            vertexSize = 0.0075f;
            normalSize = 0.10f;
            tangentSize = 0.075f;
            binormalSize = 0.06f;
            vertexColor = new Color(0.15f, 0.15f, 0.3f, 0.75f);
            vertexColor2 = new Color(1.0f, 0.0f, 0.0f, 0.75f);
            vertexColor3 = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            normalColor = Color.yellow;
            tangentColor = Color.cyan;
            binormalColor = Color.green;
        }

        public void InitializeBrushData()
        {
            for (int i = 0; i < brushData.Length; ++i)
            {
                var bd = brushData[i];
                if(bd.curve == null || bd.curve.length == 0)
                {
                    bd.curve = new AnimationCurve();
                    bd.curve.AddKey(0.0f, 0.0f);
                    bd.curve.AddKey(1.0f, 1.0f);
                }
                bd.UpdateSamples();
            }
        }
    }
#endif // UNITY_EDITOR
}

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace JTRP
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class GeometryOutline : MonoBehaviour
	{
		public bool enable = true;
		public bool enableBorderEdge = true;
		public bool enableSilhouetteEdge = true;
		public bool enableCreaseEdge = true;
		public Color edgeColor = Color.black;
		[Range(0, 0.1f)] public float edgeWidth = 0.01f;
		public bool edgeWidthVC = false;
		[Range(0, 1f)] public float normalBias = 0.0f;
		public float zOffset = 0.0f;
		public CompareFunction zTest = CompareFunction.LessEqual;

		[SerializeField] private List<DegradedRectangle> _bakedDegradedRectangles;

		private bool _enable = true;
		private static Shader _geometryOutlineShader;
		private Material _material;
		private SkinnedMeshRenderer _renderer;

		private OutlineBufferManager _bufferManager;
		private Mesh _bakedMesh;

		#region PropID

		private static readonly int _enableBorderEdge = Shader.PropertyToID("_enableBorderEdge");
		private static readonly int _enableSilhouetteEdge = Shader.PropertyToID("_enableSilhouetteEdge");
		private static readonly int _enableCreaseEdge = Shader.PropertyToID("_enableCreaseEdge");
		private static readonly int _edgeWidth = Shader.PropertyToID("_edgeWidth");
		private static readonly int _edgeWidthVc = Shader.PropertyToID("_edgeWidthVC");
		private static readonly int _normalBias = Shader.PropertyToID("_normalBias");
		private static readonly int _zOffset = Shader.PropertyToID("_zOffset");
		private static readonly int _zTest = Shader.PropertyToID("_zTest");
		private static readonly int _edgeColor = Shader.PropertyToID("_edgeColor");

		#endregion

		private void Setup()
		{
			if (_geometryOutlineShader == null) _geometryOutlineShader = Shader.Find("Hidden/GeometryOutline");
			if (_material == null) _material = new Material(_geometryOutlineShader);
			if (_renderer == null) _renderer = GetComponent<SkinnedMeshRenderer>();
			if (_bakedMesh == null) _bakedMesh = new Mesh();
			_renderer.BakeMesh(_bakedMesh);
			_material.SetFloat(_enableBorderEdge, enableBorderEdge ? 1 : 0);
			_material.SetFloat(_enableSilhouetteEdge, enableSilhouetteEdge ? 1 : 0);
			_material.SetFloat(_enableCreaseEdge, enableCreaseEdge ? 1 : 0);
			_material.SetFloat(_edgeWidth, edgeWidth);
			_material.SetFloat(_edgeWidthVc, edgeWidthVC ? 1.0f : 0.0f);
			_material.SetFloat(_normalBias, normalBias);
			_material.SetFloat(_zOffset, zOffset);
			_material.SetFloat(_zTest, (int) zTest);
			_material.SetColor(_edgeColor, edgeColor);
		}

		public void DoRebake()
		{
			Setup();
			var mesh = _renderer.sharedMesh;
			if(mesh == null)
			{
				Debug.LogError("Missing Mesh : " + _renderer.name);
				return;
			} 
			var vertices = mesh.vertices;
			var triangles = mesh.triangles;
			var rectangleDic = new Dictionary<(Vector3, Vector3), DegradedRectangle>();
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				var v1 = triangles[i * 3];
				var v2 = triangles[i * 3 + 1];
				var v3 = triangles[i * 3 + 2];
				AddLine(v1, v2, v3, vertices, rectangleDic, mesh.name);
				AddLine(v2, v3, v1, vertices, rectangleDic, mesh.name);
				AddLine(v3, v1, v2, vertices, rectangleDic, mesh.name);
			}

			_bakedDegradedRectangles = rectangleDic.Values.ToList();
			Debug.Log(
				$"Bake done. Name:{mesh.name}, Vertex Count:{mesh.vertexCount}, Triangles Count:{triangles.Length / 3}, Rectangles Count:{_bakedDegradedRectangles.Count}");
		}

		private void AddLine(int index1, int index2, int index3, Vector3[] vertices,
			Dictionary<(Vector3, Vector3), DegradedRectangle> rectangleDic, string name)
		{
			// 改进灵刃的算法，将List.Contains()改为hash map, 要求对同一条边不同顶点顺序的输入作为同一边处理
			var key1 = (vertices[index1], vertices[index2]);
			var key2 = (vertices[index2], vertices[index1]);
			int flag = 0;
			flag = rectangleDic.ContainsKey(key1) ? 1 : flag;
			flag = rectangleDic.ContainsKey(key2) ? 2 : flag;
			var key = flag == 2 ? key2 : key1;
			if (flag == 0)
			{
				var degRec = new DegradedRectangle();
				degRec.vertex1 = index1;
				degRec.vertex2 = index2;
				degRec.triangle1_vertex3 = index3;
				degRec.triangle2_vertex3 = -1;
				rectangleDic.Add(key, degRec);
			}
			else
			{
				var degRec = rectangleDic[key];
				if (degRec.triangle2_vertex3 == -1)
				{
					degRec.triangle2_vertex3 = index3;
					rectangleDic[key] = degRec;
				}
				else
					Debug.LogError("Mesh:'" + name + "' has more than two faces share an edge!!!");
			}
		}

		public void Draw(CommandBuffer cmd)
		{
			if (!enable || !_enable) return;
			Setup();
			
			if(_bakedMesh.vertexCount == 0 || _bakedMesh.vertices.Length == 0) return;
			if (_bakedDegradedRectangles == null || _bakedDegradedRectangles.Count == 0) DoRebake();
			if (_bufferManager == null)
				_bufferManager = new OutlineBufferManager(_bakedMesh, _bakedDegradedRectangles, _material);
			_bufferManager.Update(_bakedMesh, _bakedDegradedRectangles);
			cmd.DrawProcedural(transform.localToWorldMatrix, _material, 0, MeshTopology.Points,
				_bakedDegradedRectangles.Count);
		}

		private void OnEnable() => _enable = true;
		private void OnDisable() => _enable = false;
		private void OnBecameVisible() => _enable = true;
		private void OnBecameInvisible() => _enable = false;

		private void OnDestroy()
		{
			_bufferManager?.Release();
			_bakedDegradedRectangles = null;
			_bakedMesh = null;
		}
	}

	public class OutlineBufferManager
	{
		public Material material;
		public ComputeBuffer vertices;
		public ComputeBuffer normals;
		public ComputeBuffer uvs;
		public ComputeBuffer colors;
		public ComputeBuffer degradedRectangles;

		private static readonly int _normals = Shader.PropertyToID("_normals");
		private static readonly int _uvs = Shader.PropertyToID("_uvs");
		private static readonly int _colors = Shader.PropertyToID("_colors");
		private static readonly int _degradedRectangles = Shader.PropertyToID("_degradedRectangles");
		private static readonly int _vertices = Shader.PropertyToID("_vertices");

		public OutlineBufferManager(Mesh mesh, List<DegradedRectangle> degraded_rectangles, Material material)
		{
			this.normals = new ComputeBuffer(mesh.normals.Length, sizeof(float) * 3, ComputeBufferType.Default);
			this.uvs = new ComputeBuffer(mesh.uv.Length, sizeof(float) * 2, ComputeBufferType.Default);
			if (mesh.colors.Length > 0)
				this.colors = new ComputeBuffer(mesh.colors.Length, sizeof(float) * 4, ComputeBufferType.Default);
			this.degradedRectangles = new ComputeBuffer(degraded_rectangles.Count,
				Marshal.SizeOf(typeof(DegradedRectangle)), ComputeBufferType.Default);
			this.vertices = new ComputeBuffer(mesh.vertexCount, 12, ComputeBufferType.Default);
			this.material = material;
		}

		public void Update(Mesh mesh, List<DegradedRectangle> degraded_rectangles)
		{
			vertices.SetData(mesh.vertices);
			normals.SetData(mesh.normals);
			uvs.SetData(mesh.uv);
			if (colors != null)
				colors.SetData(mesh.colors);
			degradedRectangles.SetData(degraded_rectangles);
			SetBuffer();
		}

		public void SetBuffer()
		{
			material.SetBuffer(_normals, normals);
			material.SetBuffer(_uvs, uvs);
			if (colors != null)
				material.SetBuffer(_colors, colors);
			material.SetBuffer(_degradedRectangles, degradedRectangles);
			material.SetBuffer(_vertices, vertices);
		}

		~OutlineBufferManager()
		{
			Release();
		}

		public void Release()
		{
			vertices?.Release();
			normals?.Release();
			uvs?.Release();
			colors?.Release();
			degradedRectangles?.Release();

			vertices = null;
			normals = null;
			uvs = null;
			colors = null;
			degradedRectangles = null;
		}
	}


	//退化四边形，一条边和其相邻面的顶点，默认有1或2个相邻面
	[System.Serializable]
	public struct DegradedRectangle
	{
		public int vertex1; // 构成边的顶点1的索引
		public int vertex2; // 构成边的顶点2的索引
		public int triangle1_vertex3; // 边所在三角面1的顶点3索引
		public int triangle2_vertex3; // 边所在三角面2的顶点3索引
	}
}
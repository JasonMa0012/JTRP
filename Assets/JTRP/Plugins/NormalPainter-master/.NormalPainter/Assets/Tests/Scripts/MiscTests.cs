#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UTJ.NormalPainter
{
    public static class MiscTests
    {

        [MenuItem("Test/Generate Mesh From Terratin")]
        public static bool GenerateMeshDataFromTerrain()
        {
            var go = Selection.activeGameObject;
            if (go != null)
            {
                var terrain = go.GetComponent<Terrain>();
                if (terrain != null)
                {
                    var data = new MeshData();
                    data.Extract(terrain);

                    var mesh = new Mesh();
                    mesh.SetVertices(data.vertices.List);
                    mesh.SetUVs(0, data.uv.List);
                    mesh.SetNormals(data.normals.List);
                    mesh.SetIndices(data.indices.Array, MeshTopology.Triangles, 0);

                    var dataPath = Application.dataPath;
                    var path = EditorUtility.SaveFilePanel("Export .asset file", "Assets", terrain.name, "asset");
                    if (!path.StartsWith(dataPath))
                    {
                        Debug.LogError("Invalid path: Path must be under " + dataPath);
                        return false;
                    }
                    else
                    {
                        path = path.Replace(dataPath, "Assets");
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.CreateAsset(mesh, path);
                        return true;
                    }

                }
            }
            return false;
        }
    }
}

#endif

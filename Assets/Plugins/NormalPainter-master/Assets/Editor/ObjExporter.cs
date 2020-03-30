// based on http://wiki.unity3d.com/index.php?title=ExportOBJ (thanks!)

using UnityEngine;
using System.IO;
using System.Text;

namespace UTJ.NormalPainter
{
    public class ObjExporter
    {
        public class Settings
        {
            public bool includeChildren = true;
            public bool makeSubmeshes = true;
            public bool applyTransform = true;
            public bool flipHandedness = true;
            public bool flipFaces = false;

            public bool normals = true;
            public bool uv = true;
        }

        public static bool Export(GameObject go, string path, Settings settings)
        {
            if (path == null || path.Length == 0 || go == null)
                return false;

            var inst = new ObjExporter();
            inst.DoExport(go, path, settings);
            return true;
        }


        Settings m_settings;
        int m_startIndex;

        void DoExport(GameObject go, string path, Settings settings)
        {
            m_settings = settings;

            StringBuilder meshString = new StringBuilder();
            meshString.Append("#" + go.name + ".obj"
                                + "\n#" + System.DateTime.Now.ToLongDateString()
                                + "\n#" + System.DateTime.Now.ToLongTimeString()
                                + "\n#-------"
                                + "\n\n");

            Transform t = go.transform;
            Vector3 originalPosition = t.position;
            t.position = Vector3.zero;

            if (!m_settings.makeSubmeshes)
                meshString.Append("g ").Append(t.name).Append("\n");
            meshString.Append(ProcessTransform(t, m_settings.makeSubmeshes));

            WriteToFile(meshString.ToString(), path);

            t.position = originalPosition;

            Debug.Log("Exported Mesh: " + path);
        }


        string ProcessTransform(Transform t, bool makeSubmeshes)
        {
            StringBuilder meshString = new StringBuilder();

            meshString.Append("#" + t.name
                            + "\n#-------"
                            + "\n");

            if (makeSubmeshes)
                meshString.Append("g ").Append(t.name).Append("\n");

            Mesh mesh = null;
            Material[] materials = null;
            {
                var mf = t.GetComponent<MeshFilter>();
                if (mf != null)
                    mesh = mf.sharedMesh;
                else
                {
                    var smi = t.GetComponent<SkinnedMeshRenderer>();
                    if (smi != null)
                        mesh = smi.sharedMesh;
                }

                var renderer = t.GetComponent<Renderer>();
                if (renderer != null)
                    materials = renderer.sharedMaterials;
            }

            if (mesh != null)
                meshString.Append(MeshToString(mesh, materials, t));

            if (m_settings.includeChildren)
            {
                for (int i = 0; i < t.childCount; i++)
                    meshString.Append(ProcessTransform(t.GetChild(i), makeSubmeshes));
            }

            return meshString.ToString();
        }

        string MeshToString(Mesh mesh, Material[] mats, Transform t)
        {
            if (!mesh)
                return "####Error####";

            Vector3[] points = mesh.vertices;
            Vector3[] normals = m_settings.normals ? mesh.normals : null;
            Vector2[] uv = m_settings.uv ? mesh.uv : null;

            if (m_settings.applyTransform && t != null)
            {
                if (points != null)
                {
                    for (int i = 0; i < points.Length; ++i)
                        points[i] = t.TransformPoint(points[i]);
                }
                if (normals != null)
                {
                    for (int i = 0; i < normals.Length; ++i)
                        normals[i] = t.TransformVector(normals[i]);
                }

            }
            if (m_settings.flipHandedness)
            {
                if (points != null)
                {
                    for (int i = 0; i < points.Length; ++i)
                        points[i].x *= -1.0f;
                }
                if (normals != null)
                {
                    for (int i = 0; i < normals.Length; ++i)
                        normals[i].x *= -1.0f;
                }
            }

            StringBuilder sb = new StringBuilder();

            if (points != null)
            {
                foreach (Vector3 v in points)
                    sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
                sb.Append("\n");
            }
            if (normals != null)
            {
                foreach (Vector3 n in normals)
                    sb.Append(string.Format("vn {0} {1} {2}\n", n.x, n.y, n.z));
                sb.Append("\n");
            }
            if (uv != null)
            {
                foreach (Vector3 u in uv)
                    sb.Append(string.Format("vt {0} {1}\n", u.x, u.y));
            }

            int i1 = m_settings.flipFaces ? 2 : 1;
            int i2 = m_settings.flipFaces ? 1 : 2;
            string format = "";
            {
                int numComponents = 0;
                if (points != null && points.Length > 0) ++numComponents;
                if (normals != null && normals.Length > 0) ++numComponents;
                if (uv != null && uv.Length > 0) ++numComponents;

                switch (numComponents)
                {
                    case 1: format = "f {0} {1} {2}\n"; break;
                    case 2: format = "f {0}/{0} {1}/{1} {2}/{2}\n"; break;
                    case 3: format = "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n"; break;
                }
            }

            for (int sm = 0; sm < mesh.subMeshCount; sm++)
            {
                sb.Append("\n");
                if (mats != null && sm < mats.Length)
                {
                    sb.Append("usemtl ").Append(mats[sm].name).Append("\n");
                    sb.Append("usemap ").Append(mats[sm].name).Append("\n");
                }

                int[] triangles = mesh.GetTriangles(sm);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format(format,
                        triangles[i] + 1 + m_startIndex, triangles[i + i1] + 1 + m_startIndex, triangles[i + i2] + 1 + m_startIndex));
                }
            }

            m_startIndex += points.Length;
            return sb.ToString();
        }

        void WriteToFile(string s, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
                sw.Write(s);
        }
    }
}
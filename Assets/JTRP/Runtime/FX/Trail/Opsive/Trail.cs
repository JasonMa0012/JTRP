/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;
using Opsive.UltimateCharacterController.Game;
using System.Collections.Generic;

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    /// <summary>
    /// Builds a mesh which can show a trail following a melee weapon. This is typically used when the melee weapon is slashed.
    /// </summary>
    public class Trail : MonoBehaviour
    {
        [Tooltip("The minimum distance between the position of the last slice and the current position.")]
        [SerializeField] protected float m_MinDistance = 0.01f;
        [Tooltip("The vertical length of the trail.")]
        [SerializeField] protected float m_Length = 1f;
        [Tooltip("The maximum number of slices within the trail. A larger value will cause the trail to be longer.")]
        [SerializeField] protected int m_MaxSliceCount = 50;
        [Tooltip("The smoothing value of the curve. A larger value will have a smoother curve compared to a smaller value.")]
        [SerializeField] protected int m_CurveSmoothness = 10;
        [Tooltip("The steepness of the curve. A value closer to 1 will increase the steepness.")]
        [Range(0, 1)] [SerializeField] protected float m_CurveSteepness = 0.5f;
        [Tooltip("The start color of the trail, near the melee weapon object.")]
        [SerializeField] protected Color m_StartColor = Color.white;
        [Tooltip("The end color of the trail.")]
        [SerializeField] protected Color m_EndColor = Color.white;
        [Tooltip("The amount of time the slice should be visible.")]
        [SerializeField] protected float m_VisibilityTime = 1;

        private GameObject m_GameObject;
        private Transform m_Transform;
        private Mesh m_Mesh;

        private float m_MinDistanceSquared;
        private TrailSlice[] m_TrailSlices = new TrailSlice[4];
        private int m_TrailSlicesIndex = -1;
        private int m_TrailSlicesCount;

        private TrailSlice[] m_SmoothedTrailSlices;
        private int m_SmoothedTrailSlicesIndex = -1;
        private int m_SmoothedTrailSlicesCount;

        private int m_SmoothedTrailSlicesPrevCount;
        private int m_SmoothedTrailSlicesPrevIndex;

        private List<Vector3> m_Vertices;
        private List<Vector2> m_UVs;
        private List<Color> m_Colors;
        private List<int> m_Triangles;

        private bool m_GenerateSlices;

        /// <summary>
        /// A small container class for each object that represents a slice from the melee trail.
        /// </summary>
        private struct TrailSlice
        {
            private Vector3 m_Point;
            private Vector3 m_Up;
            private float m_Time;

            public Vector3 Point { get { return m_Point; } }
            public Vector3 Up { get { return m_Up; } }
            public float Time { get { return m_Time; } }

            /// <summary>
            /// Initializes the slice.
            /// </summary>
            /// <param name="point">The position of the slice.</param>
            /// <param name="up">The up direction of the slice.</param>
            public void Initialize(Vector3 point, Vector3 up)
            {
                m_Point = point;
                m_Up = up;
                m_Time = UnityEngine.Time.time;
            }
        }

        /// <summary>
        /// Initializes the trail.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;

            var meshFilter = GetComponent<MeshFilter>();
            m_Mesh = meshFilter.mesh;

            m_MinDistanceSquared = m_MinDistance * m_MinDistance;
            var count = m_MaxSliceCount * m_CurveSmoothness;
            m_SmoothedTrailSlices = new TrailSlice[count];

            m_Vertices = new List<Vector3>(count * 2);
            m_UVs = new List<Vector2>(count * 2);
            m_Colors = new List<Color>(count * 2);
            m_Triangles = new List<int>((count - 1) * 6); // 3 indices per triangle, 2 triangles per slice.
        }

        /// <summary>
        /// Start to generate the trail slices.
        /// </summary>
        private void OnEnable()
        {
            m_GenerateSlices = true;
        }

        /// <summary>
        /// Samples the position of the melee object.
        /// </summary>
        private void FixedUpdate()
        {
            if (m_GenerateSlices) {
                SampleTrail();
            }
        }

        /// <summary>
        /// Displays the trail.
        /// </summary>
        private void LateUpdate()
        {
            RemoveOldSlices();
            BuildMesh();
        }

        /// <summary>
        /// Stores a new sample of the trail slice.
        /// </summary>
        private void SampleTrail()
        {
            // Add a new slice if the last sample position is too far away.
            if (m_TrailSlicesCount == 0 || (m_TrailSlices[m_TrailSlicesIndex].Point - m_Transform.position).sqrMagnitude > m_MinDistanceSquared) {
                // Revert the trail slice index and smoothed index/count values so the extra slice can be removed. A more accurate slice value will replace the prediction.
                if (m_TrailSlicesCount > 3) {
                    m_TrailSlicesIndex = m_TrailSlicesIndex - 1;
                    if (m_TrailSlicesIndex < 0) {
                        m_TrailSlicesIndex += m_TrailSlicesCount;
                    }

                    m_SmoothedTrailSlicesIndex = m_SmoothedTrailSlicesPrevIndex;
                    m_SmoothedTrailSlicesCount = m_SmoothedTrailSlicesPrevCount;
                }

                // Add the new slice at the current position.
                AddTrailSlice(m_Transform.position, m_Transform.up);

                // A catmull-rom curve smooths the middle two verticies rather then all four vertices. Add one more slice near the beginning of the trail so the start of the
                // curve will intersect with the melee object.
                if (m_TrailSlicesCount > 3) {
                    var prevIndex = m_TrailSlicesIndex - 1;
                    if (prevIndex < 0) {
                        prevIndex = m_TrailSlicesCount - 1;
                    }
                    var prevTrailSlice = m_TrailSlices[prevIndex];
                    var trailSlice = m_TrailSlices[m_TrailSlicesIndex];

                    // Remember the previous smoothed values so the extra slice can be removed.
                    m_SmoothedTrailSlicesPrevIndex = m_SmoothedTrailSlicesIndex;
                    m_SmoothedTrailSlicesPrevCount = m_SmoothedTrailSlicesCount;
                    // The new slice should be in the previous to the current slice position. This value probably won't be correct the next frame unless
                    // the object is moving in a linear path but it is a good prediction.
                    AddTrailSlice(trailSlice.Point + (trailSlice.Point - prevTrailSlice.Point).normalized, (trailSlice.Up + prevTrailSlice.Up) / 2);
                }
            }
        }

        /// <summary>
        /// Adds the point and up vertex to the trail slices array. The values will also be smoothed.
        /// </summary>
        /// <param name="point">The point of the slice.</param>
        /// <param name="up">The up direction of the slice.</param>
        private void AddTrailSlice(Vector3 point, Vector3 up)
        {
            // Catmull-rom curves do not like repeated points.
            if (m_TrailSlicesCount > 0 && m_TrailSlices[m_TrailSlicesIndex].Point == point) {
                return;
            }

            m_TrailSlicesIndex = (m_TrailSlicesIndex + 1) % m_TrailSlices.Length;
            m_TrailSlices[m_TrailSlicesIndex].Initialize(point, up);
            if (m_TrailSlicesIndex + 1 > m_TrailSlicesCount) {
                m_TrailSlicesCount++;
            }

            SmoothTrailSlice();
        }

        /// <summary>
        /// Smooths the trail slices with a catmull-rom curve.
        /// </summary>
        private void SmoothTrailSlice()
        {
            // A catmull-rom curve requires at least four points.
            if (m_TrailSlicesCount < 4) {
                return;
            }

            // A fixed size array is used to store the vertex values. The starting index may not be at the beginning of the array.
            var startIndex = m_TrailSlicesIndex - m_TrailSlicesCount + 1;
            if (startIndex < 0) {
                startIndex = m_TrailSlices.Length + startIndex;
            }

            // Determine a smoothed value for both the point and up vertex.
            var p0 = m_TrailSlices[startIndex].Point;
            var p1 = m_TrailSlices[(startIndex + 1) % 4].Point;
            var p2 = m_TrailSlices[(startIndex + 2) % 4].Point;
            var p3 = m_TrailSlices[(startIndex + 3) % 4].Point;

            var u0 = m_TrailSlices[startIndex].Up;
            var u1 = m_TrailSlices[(startIndex + 1) % 4].Up;
            var u2 = m_TrailSlices[(startIndex + 2) % 4].Up;
            var u3 = m_TrailSlices[(startIndex + 3) % 4].Up;

            var t1 = CentripetralCatmullRomTime(0, p0, p1);
            var t2 = CentripetralCatmullRomTime(t1, p1, p2);
            var t3 = CentripetralCatmullRomTime(t2, p2, p3);

            // Iterate based on the number of sample values.
            var iterAmount = ((t2 - t1) / m_CurveSmoothness);
            for (float t = t1; t < t2; t += iterAmount) {
                var point = CentripetralCatmullRomValue(p0, p1, p2, p3, 0, t1, t2, t3, t);
                var up = CentripetralCatmullRomValue(u0, u1, u2, u3, 0, t1, t2, t3, t);

                // The value has been determined. Add it to the smoothed array.
                m_SmoothedTrailSlicesIndex = (m_SmoothedTrailSlicesIndex + 1) % m_SmoothedTrailSlices.Length;
                m_SmoothedTrailSlices[m_SmoothedTrailSlicesIndex].Initialize(point, up);
                if (m_SmoothedTrailSlicesIndex + 1 > m_SmoothedTrailSlicesCount) {
                    m_SmoothedTrailSlicesCount++;
                }
            }
        }

        /// <summary>
        /// Returns the time of the centripetral catmull-rom curve, defined in https://en.wikipedia.org/wiki/Centripetal_Catmull–Rom_spline.
        /// </summary>
        /// <param name="t">The sample time.</param>
        /// <param name="v0">The first vertex.</param>
        /// <param name="v1">The second vertex.</param>
        /// <returns>The time of the centripetral catmull-rom curve.</returns>
        private float CentripetralCatmullRomTime(float t, Vector3 v0, Vector3 v1)
        {
            var a = Mathf.Pow((v1.x - v0.x), 2f) + Mathf.Pow((v1.y - v0.y), 2f) + Mathf.Pow((v1.z - v0.z), 2f);
            var b = Mathf.Pow(a, 0.5f);
            var c = Mathf.Pow(b, m_CurveSteepness);

            return c + t;
        }

        /// <summary>
        /// Returns the vertex of the centripetral catmull-rom curve, defined in https://en.wikipedia.org/wiki/Centripetal_Catmull–Rom_spline.
        /// </summary>
        /// <returns>The vertex of the centripetral catmull-rom curve.</returns>
        private Vector3 CentripetralCatmullRomValue(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float t0, float t1, float t2, float t3, float t)
        {
            var a1 = (t1 - t) / (t1 - t0) * v0 + (t - t0) / (t1 - t0) * v1;
            var a2 = (t2 - t) / (t2 - t1) * v1 + (t - t1) / (t2 - t1) * v2;
            var a3 = (t3 - t) / (t3 - t2) * v2 + (t - t2) / (t3 - t2) * v3;
            var b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
            var b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;
            return (t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2;
        }

        /// <summary>
        /// Removes any slices which have existed for more than the visible time.
        /// </summary>
        private void RemoveOldSlices()
        {
            if (m_TrailSlicesCount == 0) {
                return;
            }

            var startIndex = m_TrailSlicesIndex - m_TrailSlicesCount + 1;
            if (startIndex < 0) {
                startIndex = m_TrailSlices.Length + startIndex;
            }
            var count = m_TrailSlicesCount;
            for (int i = 0; i < count; ++i) {
                var trailSlice = m_TrailSlices[(startIndex + i) % m_TrailSlices.Length];
                if (trailSlice.Time + m_VisibilityTime > Time.time) {
                    break;
                }
                
                // The slice has existed for more than the visiblity time - remove it by decreasing the count.
                m_TrailSlicesCount--;
            }

            if (m_SmoothedTrailSlicesCount == 0) {
                return;
            }

            startIndex = m_SmoothedTrailSlicesIndex - m_SmoothedTrailSlicesCount + 1;
            if (startIndex < 0) {
                startIndex = m_SmoothedTrailSlices.Length + startIndex;
            }
            count = m_SmoothedTrailSlicesCount;
            for (int i = 0; i < count; ++i) {
                var trailSlice = m_SmoothedTrailSlices[(startIndex + i) % m_SmoothedTrailSlices.Length];
                if (trailSlice.Time + m_VisibilityTime > Time.time) {
                    break;
                }

                // The slice has existed for more than the visiblity time - remove it by decreasing the count.
                m_SmoothedTrailSlicesCount--;
                m_SmoothedTrailSlicesPrevCount--;
            }

            if (m_TrailSlicesCount == 0 && m_SmoothedTrailSlicesCount == 0) {
                m_GenerateSlices = false;
                if (ObjectPool.InstantiatedWithPool(m_GameObject)) {
                    ObjectPool.Destroy(m_GameObject);
                } else {
                    m_GameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Creates the mesh from the catmull rom verticies.
        /// </summary>
        private void BuildMesh()
        {
            m_Mesh.Clear();

            if (m_TrailSlicesCount < 4) {
                return;
            }

            // A fixed size array is used to store the vertex values. The starting index may not be at the beginning of the array.
            var startIndex = m_SmoothedTrailSlicesIndex - m_SmoothedTrailSlicesCount + 1;
            if (startIndex < 0) {
                startIndex = m_SmoothedTrailSlices.Length + startIndex;
            }

            m_Vertices.Clear();
            m_UVs.Clear();
            m_Colors.Clear();
            m_Triangles.Clear();

            for (int i = 0; i < m_SmoothedTrailSlicesCount; ++i) {
                var trailSlice = m_SmoothedTrailSlices[(startIndex + i) % m_SmoothedTrailSlices.Length];
                // The vertex position is the local position of the slice point. This will allow the trail to stay in the same position while the melee object is moving..
                m_Vertices.Add(m_Transform.InverseTransformPoint(trailSlice.Point));
                m_Vertices.Add(m_Transform.InverseTransformPoint(trailSlice.Point + trailSlice.Up * m_Length));
                // Set the UV value so a texture can be applied to the material.
                var u = Mathf.Max(i / (float)(m_SmoothedTrailSlicesCount - 1), 0.01f);
                m_UVs.Add(new Vector2(u, 0));
                m_UVs.Add(new Vector2(u, 1));
                // Optionally lerp between the start and end color.
                m_Colors.Add(Color.Lerp(m_EndColor, m_StartColor, u));
                // A clear color will fade the trail at the bottom.
                m_Colors.Add(Color.clear);

                // Map the triangle indices to the vertex element.
                if (i < m_SmoothedTrailSlicesCount - 1) {
                    // First triangle.
                    m_Triangles.Add((i * 2));
                    m_Triangles.Add((i * 2) + 1);
                    m_Triangles.Add((i * 2) + 2);

                    // Second triangle.
                    m_Triangles.Add((i * 2) + 2);
                    m_Triangles.Add((i * 2) + 1);
                    m_Triangles.Add((i * 2) + 3);
                }
            }

            // Assign the values so the mesh will be displayed on the screen. The list version is used to prevent allocations when the mesh changes size.
            m_Mesh.SetVertices(m_Vertices);
            m_Mesh.SetUVs(0, m_UVs);
            m_Mesh.SetColors(m_Colors);
            m_Mesh.SetTriangles(m_Triangles, 0);
        }

        /// <summary>
        /// Stops generating the trail.
        /// </summary>
        public void StopGeneration()
        {
            m_Transform.parent = null;
            m_GenerateSlices = false;
        }
    }
}
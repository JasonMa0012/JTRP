using System;
using UnityEngine;
using System.Runtime.CompilerServices;

public class RayTrianglesIntersectionTest : MonoBehaviour
{
    const float Epsilon = 1e-6f;

    // note: "ref" for Vector3 parameters makes this faster a little
    // AggressiveInlining require .NET 4.5 or later :(
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RayTriangleIntersection(
        ref Vector3 pos, ref Vector3 dir, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, ref float distance)
    {
        Vector3 e1 = p2 - p1;
        Vector3 e2 = p3 - p1;
        Vector3 p = Vector3.Cross(dir, e2);
        float det = Vector3.Dot(e1, p);
        if (Math.Abs(det) < Epsilon) return false;
        float inv_det = 1.0f / det;
        Vector3 t = pos - p1;
        float u = Vector3.Dot(t, p) * inv_det;
        if (u < 0 || u > 1) return false;
        Vector3 q = Vector3.Cross(t, e1);
        float v = Vector3.Dot(dir, q) * inv_det;
        if (v < 0 || u + v > 1) return false;

        distance = Vector3.Dot(e2, q) * inv_det;
        return true;
    }



    public void Start()
    {
        const int seg = 70;
        const int numRays = 4000;

        int numTriangles = (seg - 1) * (seg - 1) * 2;
        var indices = new int[numTriangles * 3];
        var vertices = new Vector3[seg * seg];
        var verticesFlattened = new Vector3[indices.Length];

        for (int yi = 0; yi < seg - 1; ++yi)
        {
            for (int xi = 0; xi < seg - 1; ++xi)
            {
                int i = yi * (seg - 1) + xi;
                indices[i * 6 + 0] = seg * yi + xi;
                indices[i * 6 + 1] = seg * (yi + 1) + xi;
                indices[i * 6 + 2] = seg * (yi + 1) + (xi + 1);
                indices[i * 6 + 3] = seg * yi + xi;
                indices[i * 6 + 4] = seg * (yi + 1) + (xi + 1);
                indices[i * 6 + 5] = seg * yi + (xi + 1);
            }
        }

        for (int yi = 0; yi < seg; ++yi)
            for (int xi = 0; xi < seg; ++xi)
                vertices[yi * seg + xi] = new Vector3(xi - seg * 0.5f, 0.0f, yi - seg * 0.5f);

        for (int i = 0; i < indices.Length; ++i)
            verticesFlattened[i] = vertices[indices[i]];



        var rayPos = new Vector3(0.0f, 10.0f, 0.0f);
        var rayDir = new Vector3(0.0f, -1.0f, 0.0f);

        // test1: RayTriangleIntersection with indexed triangles
        {
            int numHits = 0;
            float nearest = float.MaxValue;
            float distance = 0.0f;

            float tbegin = Time.realtimeSinceStartup;
            for (int ri = 0; ri < numRays; ++ri)
            {
                numHits = 0;
                nearest = float.MaxValue;
                for (int ti = 0; ti < numTriangles; ++ti)
                {
                    if (RayTriangleIntersection(ref rayPos, ref rayDir,
                        ref vertices[indices[ti * 3 + 0]], ref vertices[indices[ti * 3 + 1]], ref vertices[indices[ti * 3 + 2]], ref distance))
                    {
                        ++numHits;
                        if (distance < nearest)
                            nearest = distance;
                    }
                }
            }
            float tend = Time.realtimeSinceStartup;

            Debug.Log("RayTriangleIntersection with indexed triangles\n" +
                "  time: " + (tend - tbegin) * 1000.0f +
                "  numHits: " + numHits +
                "  nearest: " + nearest);
        }

        // test2: RayTriangleIntersection with flattened triangles
        {
            float distance = 0.0f;
            float nearest = float.MaxValue;
            int numHits = 0;

            float tbegin = Time.realtimeSinceStartup;
            for (int ri = 0; ri < numRays; ++ri)
            {
                numHits = 0;
                nearest = float.MaxValue;
                for (int ti = 0; ti < numTriangles; ++ti)
                {
                    if (RayTriangleIntersection(ref rayPos, ref rayDir,
                        ref verticesFlattened[ti * 3 + 0], ref verticesFlattened[ti * 3 + 1], ref verticesFlattened[ti * 3 + 2], ref distance))
                    {
                        ++numHits;
                        if (distance < nearest)
                            nearest = distance;
                    }
                }
            }
            float tend = Time.realtimeSinceStartup;

            Debug.Log("RayTriangleIntersection with flattened triangles\n" +
                "  time: " + (tend - tbegin) * 1000.0f +
                "  numHits: " + numHits +
                "  nearest: " + nearest);
        }
    }
}

#include "pch.h"
#include "muMath.h"
#include "muSIMD.h"
#include "muRawVector.h"

namespace mu {

const float PI = 3.14159265358979323846264338327950288419716939937510f;
const float Deg2Rad = PI / 180.0f;
const float Rad2Deg = 1.0f / (PI / 180.0f);

#ifdef muEnableHalf
void FloatToHalf_Generic(half *dst, const float *src, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i] = src[i];
    }
}
void HalfToFloat_Generic(float *dst, const half *src, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i] = src[i];
    }
}
#endif // muEnableHalf

void InvertX_Generic(float3 *dst, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i].x *= -1.0f;
    }
}
void InvertX_Generic(float4 *dst, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i].x *= -1.0f;
    }
}

void InvertV(float2 *dst, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i].y = 1.0f - dst[i].y;
    }
}

void Scale_Generic(float *dst, float s, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i] *= s;
    }
}
void Scale_Generic(float3 *dst, float s, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i] *= s;
    }
}

void Normalize_Generic(float3 *dst, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        dst[i] = normalize(dst[i]);
    }
}

void Lerp_Generic(float *dst, const float *src1, const float *src2, size_t num, float w)
{
    const float iw = 1.0f - w;
    for (size_t i = 0; i < num; ++i) {
        dst[i] = src1[i] * w + src2[i] * iw;
    }
}

void MinMax_Generic(const float2 *src, size_t num, float2& dst_min, float2& dst_max)
{
    if (num == 0) { return; }
    float2 rmin = src[0];
    float2 rmax = src[0];
    for (size_t i = 1; i < num; ++i) {
        rmin = min(rmin, src[i]);
        rmax = max(rmax, src[i]);
    }
    dst_min = rmin;
    dst_max = rmax;
}

void MinMax_Generic(const float3 *src, size_t num, float3& dst_min, float3& dst_max)
{
    if (num == 0) { return; }
    float3 rmin = src[0];
    float3 rmax = src[0];
    for (size_t i = 1; i < num; ++i) {
        rmin = min(rmin, src[i]);
        rmax = max(rmax, src[i]);
    }
    dst_min = rmin;
    dst_max = rmax;
}

bool NearEqual_Generic(const float *src1, const float *src2, size_t num, float eps)
{
    for (size_t i = 0; i < num; ++i) {
        if (!near_equal(src1[i], src2[i], eps)) {
            return false;
        }
    }
    return true;
}

void MulPoints_Generic(const float4x4& m, const float3 src[], float3 dst[], size_t num_data)
{
    for (int i = 0; i < (int)num_data; ++i) {
        dst[i] = mul_p(m, src[i]);
    }
}
void MulVectors_Generic(const float4x4& m, const float3 src[], float3 dst[], size_t num_data)
{
    for (int i = 0; i < (int)num_data; ++i) {
        dst[i] = mul_v(m, src[i]);
    }
}

int RayTrianglesIntersectionIndexed_Generic(float3 pos, float3 dir, const float3 *vertices, const int *indices, int num_triangles, int& tindex, float& distance)
{
    int num_hits = 0;
    distance = FLT_MAX;

    for (int i = 0; i < num_triangles; ++i) {
        float d;
        if (ray_triangle_intersection(pos, dir, vertices[indices[i * 3 + 0]], vertices[indices[i * 3 + 1]], vertices[indices[i * 3 + 2]], d)) {
            ++num_hits;
            if (d < distance) {
                distance = d;
                tindex = i;
            }
        }
    }
    return num_hits;
}
int RayTrianglesIntersectionFlattened_Generic(float3 pos, float3 dir, const float3 *vertices, int num_triangles, int& tindex, float& distance)
{
    int num_hits = 0;
    distance = FLT_MAX;

    for (int i = 0; i < num_triangles; ++i) {
        float d;
        if (ray_triangle_intersection(pos, dir, vertices[i * 3 + 0], vertices[i * 3 + 1], vertices[i * 3 + 2], d)) {
            ++num_hits;
            if (d < distance) {
                distance = d;
                tindex = i;
            }
        }
    }
    return num_hits;
}

int RayTrianglesIntersectionSoA_Generic(float3 pos, float3 dir,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    int num_triangles, int& tindex, float& distance)
{
    int num_hits = 0;
    distance = FLT_MAX;

    for (int i = 0; i < num_triangles; ++i) {
        float d;
        if (ray_triangle_intersection(pos, dir,
        { v1x[i], v1y[i], v1z[i] },
        { v2x[i], v2y[i], v2z[i] },
        { v3x[i], v3y[i], v3z[i] }, d))
        {
            ++num_hits;
            if (d < distance) {
                distance = d;
                tindex = i;
            }
        }
    }
    return num_hits;
}


bool PolyInside_Generic(const float2 points[], int num_points, const float2 minp, const float2 maxp, const float2 pos)
{
    // an implementation of even-odd rule algorithm ( https://en.wikipedia.org/wiki/Even%E2%80%93odd_rule )

    if (pos.x < minp.x || pos.x > maxp.x ||
        pos.y < minp.y || pos.y > maxp.y)
    {
        return false;
    }

    // max x-intersections. this should be enough for most cases
    const int MaxIntersections = 64;
    float xc[MaxIntersections];
    int c = 0;
    for (int i = 0; i < num_points; i++) {
        int j = i + 1;
        if (j == num_points) { j = 0; }

        float2 p1 = points[i];
        float2 p2 = points[j];
        if (p1.y == p2.y) { continue; }
        else if (p1.y > p2.y) { std::swap(p1, p2); }

        if ((pos.y >= p1.y && pos.y < p2.y) ||
            (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y))
        {
            xc[c++] = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
            if (c == MaxIntersections) break;
        }
    }
    std::sort(xc, xc + c);

    for (int i = 0; i < c; i += 2) {
        if (pos.x >= xc[i] && pos.x < xc[i + 1]) {
            return true;
        }
    }
    return false;
}

// SoA variant
bool PolyInside_Generic(const float px[], const float py[], int num_points, const float2 minp, const float2 maxp, const float2 pos)
{
    if (pos.x < minp.x || pos.x > maxp.x ||
        pos.y < minp.y || pos.y > maxp.y)
    {
        return false;
    }

    const int MaxIntersections = 64;
    float xc[MaxIntersections];
    int c = 0;
    for (int i = 0; i < num_points; i++) {
        int j = i + 1;
        if (j == num_points) { j = 0; }

        float2 p1 = { px[i], py[i] };
        float2 p2 = { px[j], py[j] };
        if (p1.y == p2.y) { continue; }
        else if (p1.y > p2.y) { std::swap(p1, p2); }

        if ((pos.y >= p1.y && pos.y < p2.y) ||
            (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y))
        {
            xc[c++] = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
            if (c == MaxIntersections) break;
        }
    }
    std::sort(xc, xc + c);

    for (int i = 0; i < c; i += 2) {
        if (pos.x >= xc[i] && pos.x < xc[i + 1]) {
            return true;
        }
    }
    return false;
}

bool PolyInside_Generic(const float2 points[], int num_points, const float2 pos)
{
    if (num_points < 3) { return false; }
    float2 minp, maxp;
    MinMax_Generic(points, num_points, minp, maxp);
    return PolyInside_Generic(points, num_points, minp, maxp, pos);
}

void GenerateNormalsTriangleIndexed_Generic(float3 *dst,
    const float3 *vertices, const int *indices, int num_triangles, int num_vertices)
{
    memset(dst, 0, sizeof(float3)*num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        float3 p0 = vertices[indices[ti3 + 0]];
        float3 p1 = vertices[indices[ti3 + 1]];
        float3 p2 = vertices[indices[ti3 + 2]];
        float3 n = cross(p1 - p0, p2 - p0);

        for (int i = 0; i < 3; ++i) {
            dst[indices[ti3 + i]] += n;
        }
    }
    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = normalize(dst[vi]);
    }
}

void GenerateNormalsTriangleFlattened_Generic(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices)
{
    memset(dst, 0, sizeof(float3)*num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        float3 p0 = vertices[ti3 + 0];
        float3 p1 = vertices[ti3 + 1];
        float3 p2 = vertices[ti3 + 2];
        float3 n = cross(p1 - p0, p2 - p0);

        for (int ci = 0; ci < 3; ++ci) {
            dst[indices[ti3 + ci]] += n;
        }
    }
    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = normalize(dst[vi]);
    }

}

void GenerateNormalsTriangleSoA_Generic(float3 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const int *indices, int num_triangles, int num_vertices)
{
    memset(dst, 0, sizeof(float3)*num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        float3 p0 = { v1x[ti], v1y[ti], v1z[ti] };
        float3 p1 = { v2x[ti], v2y[ti], v2z[ti] };
        float3 p2 = { v3x[ti], v3y[ti], v3z[ti] };
        float3 n = cross(p1 - p0, p2 - p0);

        for (int ci = 0; ci < 3; ++ci) {
            dst[indices[ti3 + ci]] += n;
        }
    }
    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = normalize(dst[vi]);
    }
}


// tangent calculation


void GenerateTangentsTriangleIndexed_Generic(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices)
{
    RawVector<float3> tangents, binormals;
    tangents.resize_zeroclear(num_vertices);
    binormals.resize_zeroclear(num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        int idx[] = { indices[ti3 + 0], indices[ti3 + 1], indices[ti3 + 2] };
        float3 v[3] = { vertices[idx[0]], vertices[idx[1]], vertices[idx[2]] };
        float2 u[3] = { uv[idx[0]], uv[idx[1]], uv[idx[2]] };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangent(v, u, t, b);

        for (int i = 0; i < 3; ++i) {
            tangents[idx[i]] += t[i];
            binormals[idx[i]] += b[i];
        }
    }

    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = orthogonalize_tangent(tangents[vi], binormals[vi], normals[vi]);
    }
}

void GenerateTangentsTriangleFlattened_Generic(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices)
{
    RawVector<float3> tangents, binormals;
    tangents.resize_zeroclear(num_vertices);
    binormals.resize_zeroclear(num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        float3 v[3] = { vertices[ti3 + 0], vertices[ti3 + 1], vertices[ti3 + 2] };
        float2 u[3] = { uv[ti3 + 0], uv[ti3 + 1], uv[ti3 + 2] };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangent(v, u, t, b);

        for (int i = 0; i < 3; ++i) {
            int i0 = indices[ti3 + i];
            tangents[i0] += t[i];
            binormals[i0] += b[i];
        }
    }

    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = orthogonalize_tangent(tangents[vi], binormals[vi], normals[vi]);
    }
}

void GenerateTangentsTriangleSoA_Generic(float4 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const float *u1x, const float *u1y,
    const float *u2x, const float *u2y,
    const float *u3x, const float *u3y,
    const float3 *normals, const int *indices,
    int num_triangles, int num_vertices)
{
    RawVector<float3> tangents, binormals;
    tangents.resize_zeroclear(num_vertices);
    binormals.resize_zeroclear(num_vertices);

    for (int ti = 0; ti < num_triangles; ++ti) {
        int ti3 = ti * 3;
        int idx[] = { indices[ti3 + 0], indices[ti3 + 1], indices[ti3 + 2] };
        float3 v[3] = {
            { v1x[ti], v1y[ti], v1z[ti] },
            { v2x[ti], v2y[ti], v2z[ti] },
            { v3x[ti], v3y[ti], v3z[ti] },
        };
        float2 u[3] = {
            { u1x[ti], u1y[ti] },
            { u2x[ti], u2y[ti] },
            { u3x[ti], u3y[ti] },
        };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangent(v, u, t, b);

        for (int i = 0; i < 3; ++i) {
            tangents[idx[i]] += t[i];
            binormals[idx[i]] += b[i];
        }
    }

    for (int vi = 0; vi < num_vertices; ++vi) {
        dst[vi] = orthogonalize_tangent(tangents[vi], binormals[vi], normals[vi]);
    }
}


bool GenerateNormalsPoly(
    float3 *dst, const float3 *points, const int *counts, const int *offsets, const int *indices,
    int num_faces, int num_vertices)
{
    memset(dst, 0, sizeof(float3)*num_vertices);

    for (int fi = 0; fi < num_faces; ++fi)
    {
        const int *face = &indices[offsets[fi]];
        float3 p0 = points[face[0]];
        float3 p1 = points[face[1]];
        float3 p2 = points[face[2]];
        float3 n = cross(p1 - p0, p2 - p0);

        int count = counts[fi];
        for (int ci = 0; ci < count; ++ci) {
            dst[face[ci]] += n;
        }
    }
    Normalize(dst, num_vertices);
    return true;
}

} // namespace mu

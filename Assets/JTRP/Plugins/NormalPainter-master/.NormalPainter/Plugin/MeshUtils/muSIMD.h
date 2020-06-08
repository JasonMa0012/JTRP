#pragma once
#include "muSIMDConfig.h"

namespace mu {

#ifdef muEnableHalf
void FloatToHalf(half *dst, const float *src, size_t num);
void HalfToFloat(float *dst, const half *src, size_t num);
#endif // muEnableHalf

void InvertX(float3 *dst, size_t num);
void InvertX(float4 *dst, size_t num);
void InvertV(float2 *dst, size_t num);
void Scale(float *dst, float s, size_t num);
void Scale(float3 *dst, float s, size_t num);
void Normalize(float3 *dst, size_t num);
void Lerp(float *dst, const float *src1, const float *src2, size_t num, float w);
void Lerp(float2 *dst, const float2 *src1, const float2 *src2, size_t num, float w);
void Lerp(float3 *dst, const float3 *src1, const float3 *src2, size_t num, float w);
void MinMax(const float3 *src, size_t num, float3& dst_min, float3& dst_max);
void MinMax(const float2 *src, size_t num, float2& dst_min, float2& dst_max);
bool NearEqual(const float *src1, const float *src2, size_t num, float eps = muEpsilon);
bool NearEqual(const float2 *src1, const float2 *src2, size_t num, float eps = muEpsilon);
bool NearEqual(const float3 *src1, const float3 *src2, size_t num, float eps = muEpsilon);
bool NearEqual(const float4 *src1, const float4 *src2, size_t num, float eps = muEpsilon);

void MulPoints(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);
void MulVectors(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);

int RayTrianglesIntersectionIndexed(float3 pos, float3 dir, const float3 *vertices, const int *indices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionFlattened(float3 pos, float3 dir, const float3 *vertices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionSoA(float3 pos, float3 dir,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionSoA(float3 pos, float3 dir,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    int num_triangles, int& tindex, float& distance);

bool PolyInside(const float px[], const float py[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside(const float2 poly[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside(const float2 poly[], int ngon, const float2 pos);

void GenerateNormalsTriangleIndexed(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleFlattened(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleSoA(float3 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const int *indices, int num_triangles, int num_vertices);

void GenerateTangentsTriangleIndexed(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
// vertices and uv must be flattened, * normals must not be flattened *
void GenerateTangentsTriangleFlattened(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleSoA(float4 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const float *u1x, const float *u1y,
    const float *u2x, const float *u2y,
    const float *u3x, const float *u3y,
    const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);


// ------------------------------------------------------------
// internal (for test)
// ------------------------------------------------------------
#ifdef muEnableHalf
void FloatToHalf_Generic(half *dst, const float *src, size_t num);
void FloatToHalf_ISPC(half *dst, const float *src, size_t num);
void HalfToFloat_Generic(float *dst, const half *src, size_t num);
void HalfToFloat_ISPC(float *dst, const half *src, size_t num);
#endif // muEnableHalf

void InvertX_Generic(float3 *dst, size_t num);
void InvertX_ISPC(float3 *dst, size_t num);
void InvertX_Generic(float4 *dst, size_t num);
void InvertX_ISPC(float4 *dst, size_t num);

void Scale_Generic(float *dst, float s, size_t num);
void Scale_Generic(float3 *dst, float s, size_t num);
void Scale_ISPC(float *dst, float s, size_t num);
void Scale_ISPC(float3 *dst, float s, size_t num);

void Normalize_Generic(float3 *dst, size_t num);
void Normalize_ISPC(float3 *dst, size_t num);

void Lerp_Generic(float *dst, const float *src1, const float *src2, size_t num, float w);
void Lerp_ISPC(float *dst, const float *src1, const float *src2, size_t num, float w);

void MinMax_Generic(const float2 *src, size_t num, float2& dst_min, float2& dst_max);
void MinMax_ISPC(const float2 *src, size_t num, float2& dst_min, float2& dst_max);
void MinMax_Generic(const float3 *src, size_t num, float3& dst_min, float3& dst_max);
void MinMax_ISPC(const float3 *src, size_t num, float3& dst_min, float3& dst_max);

bool NearEqual_Generic(const float *src1, const float *src2, size_t num, float eps);
bool NearEqual_ISPC(const float *src1, const float *src2, size_t num, float eps);

void MulPoints_Generic(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);
void MulPoints_ISPC(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);
void MulVectors_Generic(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);
void MulVectors_ISPC(const float4x4& m, const float3 src[], float3 dst[], size_t num_data);

int RayTrianglesIntersectionIndexed_Generic(float3 pos, float3 dir, const float3 *vertices, const int *indices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionIndexed_ISPC(float3 pos, float3 dir, const float3 *vertices, const int *indices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionFlattened_Generic(float3 pos, float3 dir, const float3 *vertices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionFlattened_ISPC(float3 pos, float3 dir, const float3 *vertices, int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionSoA_Generic(float3 pos, float3 dir,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    int num_triangles, int& tindex, float& distance);
int RayTrianglesIntersectionSoA_ISPC(float3 pos, float3 dir,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    int num_triangles, int& tindex, float& distance);

bool PolyInside_Generic(const float px[], const float py[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside_ISPC(const float px[], const float py[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside_Generic(const float2 poly[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside_ISPC(const float2 poly[], int ngon, const float2 minp, const float2 maxp, const float2 pos);
bool PolyInside_Generic(const float2 poly[], int ngon, const float2 pos);
bool PolyInside_ISPC(const float2 poly[], int ngon, const float2 pos);

void GenerateNormalsTriangleIndexed_Generic(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleIndexed_ISPC(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleFlattened_Generic(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleFlattened_ISPC(float3 *dst,
    const float3 *vertices, const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleSoA_Generic(float3 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const int *indices,
    int num_triangles, int num_vertices);
void GenerateNormalsTriangleSoA_ISPC(float3 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const int *indices,
    int num_triangles, int num_vertices);

void GenerateTangentsTriangleIndexed_Generic(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleIndexed_ISPC(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleFlattened_Generic(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleFlattened_ISPC(float4 *dst,
    const float3 *vertices, const float2 *uv, const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleSoA_Generic(float4 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const float *u1x, const float *u1y,
    const float *u2x, const float *u2y,
    const float *u3x, const float *u3y,
    const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);
void GenerateTangentsTriangleSoA_ISPC(float4 *dst,
    const float *v1x, const float *v1y, const float *v1z,
    const float *v2x, const float *v2y, const float *v2z,
    const float *v3x, const float *v3y, const float *v3z,
    const float *u1x, const float *u1y,
    const float *u2x, const float *u2y,
    const float *u3x, const float *u3y,
    const float3 *normals, const int *indices,
    int num_triangles, int num_vertices);

} // namespace mu

#pragma once

namespace mu {

struct MeshRefiner
{
    struct Submesh
    {
        int num_indices_tri = 0;
        int materialID = 0;
        int* faces_to_write = nullptr;
    };

    struct Split
    {
        int offset_faces = 0;
        int offset_indices = 0;
        int offset_vertices = 0;
        int num_faces = 0;
        int num_vertices = 0;
        int num_indices = 0;
        int num_indices_triangulated = 0;
        int num_submeshes = 0;
    };

    int split_unit = 0; // 0 == no split
    bool triangulate = true;
    bool swap_faces = false;

    IArray<int> counts;
    IArray<int> indices;
    IArray<float3> points;
    IArray<float3> normals;
    IArray<float2> uv;
    IArray<float4> colors;
    RawVector<Submesh> submeshes;
    RawVector<Split> splits;

    RawVector<int> old2new_indices; // indices to new indices
    RawVector<int> new2old_vertices; // indices to old vertices

private:
    RawVector<int> counts_tmp;
    RawVector<int> offsets;
    ConnectionData connection;
    RawVector<float3> face_normals;
    RawVector<float3> normals_tmp;
    RawVector<float4> tangents_tmp;

    RawVector<float3> new_points;
    RawVector<float3> new_normals;
    RawVector<float4> new_tangents;
    RawVector<float2> new_uv;
    RawVector<float4> new_colors;
    RawVector<int>    new_indices;
    RawVector<int>    new_indices_triangulated;
    RawVector<int>    new_indices_submeshes;
    RawVector<int>    dummy_materialIDs;
    int num_indices_tri = 0;

public:
    void prepare(const IArray<int>& counts, const IArray<int>& indices, const IArray<float3>& points);
    void genNormals(bool flip);
    void genNormalsWithSmoothAngle(float smooth_angle, bool flip);
    void genTangents();

    bool refine(bool optimize);

    // should be called after refine(), and only valid for triangulated meshes
    bool genSubmesh(IArray<int> materialIDs);

    void swapNewData(
        RawVector<float3>& p,
        RawVector<float3>& n,
        RawVector<float4>& t,
        RawVector<float2>& u,
        RawVector<float4>& c,
        RawVector<int>& idx);

private:
    bool refineDumb();
    bool refineWithOptimization();
    void buildConnection();

    template<class Body> void doRefine(const Body& body);
    int findOrAddVertexPNTUC(int vi, const float3& p, const float3& n, const float4& t, const float2& u, const float4& c);
    int findOrAddVertexPNTU(int vi, const float3& p, const float3& n, const float4& t, const float2& u);
    int findOrAddVertexPNU(int vi, const float3& p, const float3& n, const float2& u);
    int findOrAddVertexPN(int vi, const float3& p, const float3& n);
    int findOrAddVertexPU(int vi, const float3& p, const float2& u);
};

} // namespace mu

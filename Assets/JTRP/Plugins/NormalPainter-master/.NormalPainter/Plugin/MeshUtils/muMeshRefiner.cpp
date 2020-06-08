#include "pch.h"
#include "MeshUtils.h"

namespace mu {

void MeshRefiner::prepare(
    const IArray<int>& counts_, const IArray<int>& indices_, const IArray<float3>& points_)
{
    counts = counts_;
    indices = indices_;
    points = points_;
    normals.reset(nullptr, 0);
    uv.reset(nullptr, 0);
    colors.reset(nullptr, 0);

    submeshes.clear();
    splits.clear();

    counts_tmp.clear();
    offsets.clear();
    connection.clear();

    face_normals.clear();
    normals_tmp.clear();
    tangents_tmp.clear();

    new_points.clear();
    new_normals.clear();
    new_uv.clear();
    new_colors.clear();
    new_indices.clear();
    new_indices_triangulated.clear();
    new_indices_submeshes.clear();

    old2new_indices.clear();
    new2old_vertices.clear();

    num_indices_tri = 0;

    int num_indices = 0;
    if (counts.empty()) {
        // assume all faces are triangle
        num_indices = num_indices_tri = (int)indices.size();
        int num_faces = num_indices / 3;
        counts_tmp.resize(num_faces, 3);
        offsets.resize(num_faces);
        for (int i = 0; i < num_faces; ++i) {
            offsets[i] = i * 3;
        }
        counts = counts_tmp;
    }
    else {
        mu::CountIndices(counts, offsets, num_indices, num_indices_tri);
    }
}

void MeshRefiner::genNormals(bool flip)
{
    auto& p = points;
    normals_tmp.resize_zeroclear(p.size());

    size_t num_faces = counts.size();
    int i1 = flip ? 2 : 1;
    int i2 = flip ? 1 : 2;
    for (size_t fi = 0; fi < num_faces; ++fi)
    {
        int count = counts[fi];
        const int *face = &indices[offsets[fi]];
        float3 p0 = p[face[0]];
        float3 p1 = p[face[i1]];
        float3 p2 = p[face[i2]];
        float3 n = cross(p1 - p0, p2 - p0);
        for (int ci = 0; ci < count; ++ci) {
            normals_tmp[face[ci]] += n;
        }
    }
    Normalize(normals_tmp.data(), normals_tmp.size());

    normals = normals_tmp;
}

void MeshRefiner::genNormalsWithSmoothAngle(float smooth_angle, bool flip)
{
    buildConnection();

    auto& p = points;
    size_t num_indices = indices.size();
    size_t num_faces = counts.size();
    normals_tmp.resize(num_indices);

    // gen face normals
    face_normals.resize_zeroclear(num_faces);
    int i1 = flip ? 2 : 1;
    int i2 = flip ? 1 : 2;
    for (size_t fi = 0; fi < num_faces; ++fi)
    {
        int offset = offsets[fi];
        const int *face = &indices[offset];
        float3 p0 = p[face[0]];
        float3 p1 = p[face[i1]];
        float3 p2 = p[face[i2]];
        float3 n = cross(p1 - p0, p2 - p0);
        face_normals[fi] = n;
    }
    Normalize(face_normals.data(), face_normals.size());

    // gen vertex normals
    const float angle = std::cos(smooth_angle * Deg2Rad) - 0.001f;
    for (size_t fi = 0; fi < num_faces; ++fi)
    {
        int count = counts[fi];
        int offset = offsets[fi];
        const int *face = &indices[offset];
        auto& face_normal = face_normals[fi];
        for (int ci = 0; ci < count; ++ci) {
            int vi = face[ci];
            auto normal = float3::zero();
            connection.eachConnectedFaces(vi, [&](int fi2, int) {
                float3 n = face_normals[fi2];
                if (dot(face_normal, n) > angle) {
                    normal += n;
                }
            });
            normals_tmp[offset + ci] = normal;
        }
    }

    // normalize
    Normalize(normals_tmp.data(), normals_tmp.size());
    normals = normals_tmp;
}

void MeshRefiner::genTangents()
{
    tangents_tmp.resize_discard(std::max<size_t>(normals.size(), uv.size()));
    mu::GenerateTangentsPoly(tangents_tmp, points, normals, uv, counts, offsets, indices);
}

bool MeshRefiner::refine(bool optimize)
{
    return optimize ? refineWithOptimization() : refineDumb();
}



bool MeshRefiner::genSubmesh(IArray<int> materialIDs)
{
    submeshes.clear();

    if (materialIDs.empty()) {
        dummy_materialIDs.resize_zeroclear(counts.size());
        materialIDs = IArray<int>(dummy_materialIDs);
    }
    else if (materialIDs.size() != counts.size()) {
        return false;
    }

    new_indices_submeshes.resize(new_indices_triangulated.size());
    const int *faces_to_read = new_indices_triangulated.data();
    int *faces_to_write = new_indices_submeshes.data();

    int num_splits = (int)splits.size();
    int offset_faces = 0;

    RawVector<Submesh> sm;

    for (int si = 0; si < num_splits; ++si) {
        auto& split = splits[si];

        // count triangle indices
        for (int fi = 0; fi < split.num_faces; ++fi) {
            int mid = materialIDs[offset_faces + fi] + 1; // -1 == no material. adjust to it
            while (mid >= (int)sm.size()) {
                int id = (int)sm.size();
                sm.push_back({});
                sm.back().materialID = id - 1;
            }
            sm[mid].num_indices_tri += (counts[fi] - 2) * 3;
        }

        for (int mi = 0; mi < (int)sm.size(); ++mi) {
            sm[mi].faces_to_write = faces_to_write;
            faces_to_write += sm[mi].num_indices_tri;
        }

        // copy triangles
        for (int fi = 0; fi < split.num_faces; ++fi) {
            int mid = materialIDs[offset_faces + fi] + 1;
            int count = counts[offset_faces + fi];
            int nidx = (count - 2) * 3;
            for (int i = 0; i < nidx; ++i) {
                *(sm[mid].faces_to_write++) = *(faces_to_read++);
            }
        }

        for (int mi = 0; mi < (int)sm.size(); ++mi) {
            if (sm[mi].num_indices_tri > 0) {
                ++split.num_submeshes;
                submeshes.push_back(sm[mi]);
            }
        }

        offset_faces += split.num_faces;
        sm.clear();
    }
    return true;
}

bool MeshRefiner::refineDumb()
{
    int num_indices = (int)indices.size();
    bool flattened = false;

    // flatten
    if ((int)points.size() > split_unit ||
        (int)normals.size() == num_indices ||
        (int)uv.size() == num_indices)
    {
        {
            new_points.resize(num_indices);
            mu::CopyWithIndices(new_points.data(), points.data(), indices);
            points = new_points;
        }
        if (!normals.empty() && (int)normals.size() != num_indices) {
            new_normals.resize(num_indices);
            mu::CopyWithIndices(new_normals.data(), normals.data(), indices);
            normals = new_normals;
        }
        if (!uv.empty() && (int)uv.size() != num_indices) {
            new_uv.resize(num_indices);
            mu::CopyWithIndices(new_uv.data(), uv.data(), indices);
            uv = new_uv;
        }
        if (!colors.empty() && (int)colors.size() != num_indices) {
            new_colors.resize(num_indices);
            mu::CopyWithIndices(new_colors.data(), colors.data(), indices);
            colors = colors;
        }
        flattened = true;
    }


    // split & triangulate
    splits.clear();
    new_indices_triangulated.resize(num_indices_tri);
    if ((int)points.size() > split_unit) {
        int *sub_indices = new_indices_triangulated.data();
        int offset_faces = 0;
        int offset_indices = 0;
        int offset_vertices = 0;
        mu::Split(counts, split_unit, [&](int num_faces, int num_vertices, int num_indices_triangulated) {
            mu::Triangulate(sub_indices, IntrusiveArray<int>(&counts[offset_faces], num_faces), swap_faces);
            sub_indices += num_indices_triangulated;
            offset_faces += num_faces;
            offset_indices += num_indices_triangulated;
            offset_vertices += num_vertices;

            auto split = Split{};
            split.offset_faces = offset_faces;
            split.offset_indices = offset_indices;
            split.offset_vertices = offset_vertices;
            split.num_faces = num_faces;
            split.num_vertices = num_vertices;
            split.num_indices = num_vertices; // in this case num_vertex == num_indices
            split.num_indices_triangulated = num_indices_triangulated;
            splits.push_back(split);
        });
    }
    else if (triangulate) {
        if (flattened) {
            mu::Triangulate(new_indices_triangulated, counts, swap_faces);
        }
        else {
            mu::TriangulateWithIndices(new_indices_triangulated, counts, indices, swap_faces);
        }
        auto split = Split{};
        split.num_faces = (int)counts.size();
        split.num_vertices = (int)points.size();
        split.num_indices = (int)indices.size();
        split.num_indices_triangulated = (int)new_indices_triangulated.size();
        splits.push_back(split);
    }
    return true;
}


template<class Body>
void MeshRefiner::doRefine(const Body& body)
{
    buildConnection();

    int num_indices = (int)indices.size();
    new_points.reserve(num_indices);
    new_normals.reserve(num_indices);
    if (!uv.empty()) { new_uv.reserve(num_indices); }
    new_indices.reserve(num_indices);

    old2new_indices.resize(num_indices, -1);

    int num_faces_total = (int)counts.size();
    int offset_faces = 0;
    int offset_indices = 0;
    int offset_vertices = 0;
    int num_faces = 0;
    int num_indices_triangulated = 0;

    auto add_new_split = [&]() {
        auto split = Split{};
        split.offset_faces = offset_faces;
        split.offset_indices = offset_indices;
        split.offset_vertices = offset_vertices;
        split.num_faces = num_faces;
        split.num_indices_triangulated = num_indices_triangulated;
        split.num_vertices = (int)new_points.size() - offset_vertices;
        split.num_indices = (int)new_indices.size() - offset_indices;
        splits.push_back(split);

        offset_faces += split.num_faces;
        offset_indices += split.num_indices;
        offset_vertices += split.num_vertices;
        num_faces = 0;
        num_indices_triangulated = 0;
    };

    for (int fi = 0; fi < num_faces_total; ++fi) {
        int offset = offsets[fi];
        int count = counts[fi];

        if (split_unit > 0 && (int)new_points.size() - offset_vertices + count > split_unit) {
            add_new_split();

            // clear vertex cache
            std::fill(old2new_indices.begin(), old2new_indices.end(), -1);
        }

        for (int ci = 0; ci < count; ++ci) {
            int i = offset + ci;
            int vi = indices[i];
            int ni = body(vi, i);
            new_indices.push_back(ni - offset_vertices);
        }
        ++num_faces;
        num_indices_triangulated += (count - 2) * 3;
    }
    add_new_split();

    if (triangulate) {
        int nindices = 0;
        for (auto& split : splits) {
            nindices += split.num_indices_triangulated;
        }

        new_indices_triangulated.resize(nindices);
        int *sub_indices = new_indices_triangulated.data();
        int *n_counts = counts.data();
        int *n_indices = new_indices.data();
        for (auto& split : splits) {
            mu::TriangulateWithIndices(sub_indices,
                IntrusiveArray<int>(n_counts, split.num_faces),
                IntrusiveArray<int>(n_indices, split.num_indices),
                swap_faces);
            sub_indices += split.num_indices_triangulated;
            n_counts += split.num_faces;
            n_indices += split.num_indices;
        }
    }
    else if (swap_faces) {
        // todo
    }
}

bool MeshRefiner::refineWithOptimization()
{
    int num_points = (int)points.size();
    int num_indices = (int)indices.size();
    int num_normals = (int)normals.size();
    int num_uv = (int)uv.size();
    int num_colors = (int)colors.size();

    if (!uv.empty()) {
        if (!normals.empty()) {
            if (!tangents_tmp.empty()) {
                if (!colors.empty()) {
                    if (num_normals == num_indices && num_uv == num_indices && num_colors == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[i], tangents_tmp[i], uv[i], colors[i]);
                        });
                    }
                    else if (num_normals == num_indices && num_uv == num_indices && num_colors == num_points) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[i], tangents_tmp[i], uv[i], colors[vi]);
                        });
                    }
                    else if (num_normals == num_indices && num_uv == num_points && num_colors == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[i], tangents_tmp[i], uv[vi], colors[i]);
                        });
                    }
                    else if (num_normals == num_indices && num_uv == num_points && num_colors == num_points) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[i], tangents_tmp[i], uv[vi], colors[vi]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_indices && num_colors == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[vi], tangents_tmp[i], uv[i], colors[i]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_indices && num_colors == num_points) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[vi], tangents_tmp[i], uv[i], colors[vi]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_points && num_colors == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[vi], tangents_tmp[vi], uv[vi], colors[i]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_points && num_colors == num_points) {
                        doRefine([this](int vi, int) {
                            return findOrAddVertexPNTUC(vi, points[vi], normals[vi], tangents_tmp[vi], uv[vi], colors[vi]);
                        });
                    }
                }
                else {
                    if (num_normals == num_indices && num_uv == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTU(vi, points[vi], normals[i], tangents_tmp[i], uv[i]);
                        });
                    }
                    else if (num_normals == num_indices && num_uv == num_points) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTU(vi, points[vi], normals[i], tangents_tmp[i], uv[vi]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_indices) {
                        doRefine([this](int vi, int i) {
                            return findOrAddVertexPNTU(vi, points[vi], normals[vi], tangents_tmp[i], uv[i]);
                        });
                    }
                    else if (num_normals == num_points && num_uv == num_points) {
                        doRefine([this](int vi, int) {
                            return findOrAddVertexPNTU(vi, points[vi], normals[vi], tangents_tmp[vi], uv[vi]);
                        });
                    }
                }
            }
            else {
                if (num_normals == num_indices && num_uv == num_indices) {
                    doRefine([this](int vi, int i) {
                        return findOrAddVertexPNU(vi, points[vi], normals[i], uv[i]);
                    });
                }
                else if (num_normals == num_indices && num_uv == num_points) {
                    doRefine([this](int vi, int i) {
                        return findOrAddVertexPNU(vi, points[vi], normals[i], uv[vi]);
                    });
                }
                else if (num_normals == num_points && num_uv == num_indices) {
                    doRefine([this](int vi, int i) {
                        return findOrAddVertexPNU(vi, points[vi], normals[vi], uv[i]);
                    });
                }
                else if (num_normals == num_points && num_uv == num_points) {
                    doRefine([this](int vi, int) {
                        return findOrAddVertexPNU(vi, points[vi], normals[vi], uv[vi]);
                    });
                }
            }
        }
        else {
            if (num_uv == num_indices) {
                doRefine([this](int vi, int i) {
                    return findOrAddVertexPU(vi, points[vi], uv[i]);
                });
            }
            else if (num_uv == num_points) {
                doRefine([this](int vi, int) {
                    return findOrAddVertexPU(vi, points[vi], uv[vi]);
                });
            }
        }
    }
    else {
        if (num_normals == num_indices) {
            doRefine([this](int vi, int i) {
                return findOrAddVertexPN(vi, points[vi], normals[i]);
            });
        }
        else if (num_normals == num_points) {
            doRefine([this](int vi, int) {
                return findOrAddVertexPN(vi, points[vi], normals[vi]);
            });
        }
    }

    return true;
}

void MeshRefiner::swapNewData(
    RawVector<float3>& p,
    RawVector<float3>& n,
    RawVector<float4>& t,
    RawVector<float2>& u,
    RawVector<float4>& c,
    RawVector<int>& idx)
{
    if (!new_points.empty()) { p.swap(new_points); }

    if (!new_normals.empty()) { n.swap(new_normals); }
    else if (!normals_tmp.empty()) { n.swap(normals_tmp); }

    if (!new_tangents.empty()) { t.swap(new_tangents); }
    else if (!tangents_tmp.empty()) { t.swap(tangents_tmp); }

    if (!new_uv.empty()) { u.swap(new_uv); }
    if (!new_colors.empty()) { c.swap(new_colors); }

    if (!new_indices_submeshes.empty()) { idx.swap(new_indices_submeshes); }
    else if (!new_indices_triangulated.empty()) { idx.swap(new_indices_triangulated); }
}

void MeshRefiner::buildConnection()
{
    // skip if already built
    if (connection.v2f_counts.size() == points.size()) { return; }

    connection.buildConnection(indices, counts, offsets, points);
}

int MeshRefiner::findOrAddVertexPNTUC(int vi, const float3& p, const float3& n, const float4& t, const float2& u, const float4& c)
{
    int offset = connection.v2f_offsets[vi];
    int count = connection.v2f_counts[vi];
    for (int ci = 0; ci < count; ++ci) {
        int& ni = old2new_indices[connection.v2f_indices[offset + ci]];
        // tangent can be omitted as it is generated by point, normal and uv
        if (ni != -1 && near_equal(new_points[ni], p) && near_equal(new_normals[ni], n) && near_equal(new_uv[ni], u) && near_equal(new_colors[ni], c)) {
            return ni;
        }
        else if (ni == -1) {
            new2old_vertices.push_back(vi);
            ni = (int)new_points.size();
            new_points.push_back(p);
            new_normals.push_back(n);
            new_tangents.push_back(t);
            new_uv.push_back(u);
            new_colors.push_back(c);
            return ni;
        }
    }
    return 0;
}

int MeshRefiner::findOrAddVertexPNTU(int vi, const float3& p, const float3& n, const float4& t, const float2& u)
{
    int offset = connection.v2f_offsets[vi];
    int count = connection.v2f_counts[vi];
    for (int ci = 0; ci < count; ++ci) {
        int& ni = old2new_indices[connection.v2f_indices[offset + ci]];
        if (ni != -1 && near_equal(new_points[ni], p) && near_equal(new_normals[ni], n) && near_equal(new_uv[ni], u)) {
            return ni;
        }
        else if (ni == -1) {
            new2old_vertices.push_back(vi);
            ni = (int)new_points.size();
            new_points.push_back(p);
            new_normals.push_back(n);
            new_tangents.push_back(t);
            new_uv.push_back(u);
            return ni;
        }
    }
    return 0;
}

int MeshRefiner::findOrAddVertexPNU(int vi, const float3& p, const float3& n, const float2& u)
{
    int offset = connection.v2f_offsets[vi];
    int count = connection.v2f_counts[vi];
    for (int ci = 0; ci < count; ++ci) {
        int& ni = old2new_indices[connection.v2f_indices[offset + ci]];
        if (ni != -1 && near_equal(new_points[ni], p) && near_equal(new_normals[ni], n) && near_equal(new_uv[ni], u)) {
            return ni;
        }
        else if (ni == -1) {
            new2old_vertices.push_back(vi);
            ni = (int)new_points.size();
            new_points.push_back(p);
            new_normals.push_back(n);
            new_uv.push_back(u);
            return ni;
        }
    }
    return 0;
}

int MeshRefiner::findOrAddVertexPN(int vi, const float3& p, const float3& n)
{
    int offset = connection.v2f_offsets[vi];
    int count = connection.v2f_counts[vi];
    for (int ci = 0; ci < count; ++ci) {
        int& ni = old2new_indices[connection.v2f_indices[offset + ci]];
        if (ni != -1 && near_equal(new_points[ni], p) && near_equal(new_normals[ni], n)) {
            return ni;
        }
        else if (ni == -1) {
            new2old_vertices.push_back(vi);
            ni = (int)new_points.size();
            new_points.push_back(p);
            new_normals.push_back(n);
            return ni;
        }
    }
    return 0;
}

int MeshRefiner::findOrAddVertexPU(int vi, const float3& p, const float2& u)
{
    int offset = connection.v2f_offsets[vi];
    int count = connection.v2f_counts[vi];
    for (int ci = 0; ci < count; ++ci) {
        int& ni = old2new_indices[connection.v2f_indices[offset + ci]];
        if (ni != -1 && near_equal(new_points[ni], p) && near_equal(new_uv[ni], u)) {
            return ni;
        }
        else if (ni == -1) {
            new2old_vertices.push_back(vi);
            ni = (int)new_points.size();
            new_points.push_back(p);
            new_uv.push_back(u);
            return ni;
        }
    }
    return 0;
}

} // namespace mu

#include "pch.h"
#include "MeshGenerator.h"

void GenerateWaveMesh(
    RawVector<int>& counts,
    RawVector<int>& indices,
    RawVector<float3> &points,
    RawVector<float2> &uv,
    float size, float height,
    const int resolution,
    float angle,
    bool triangulate)
{
    const int num_vertices = resolution * resolution;

    // vertices
    points.resize(num_vertices);
    uv.resize(num_vertices);
    for (int iy = 0; iy < resolution; ++iy) {
        for (int ix = 0; ix < resolution; ++ix) {
            int i = resolution*iy + ix;
            float2 pos = {
                float(ix) / float(resolution - 1) - 0.5f,
                float(iy) / float(resolution - 1) - 0.5f
            };
            float d = std::sqrt(pos.x*pos.x + pos.y*pos.y);

            float3& v = points[i];
            v.x = pos.x * size;
            v.y = std::sin(d * 10.0f + angle) * std::max<float>(1.0 - d, 0.0f) * height;
            v.z = pos.y * size;

            float2& t = uv[i];
            t.x = pos.x * 0.5 + 0.5;
            t.y = pos.y * 0.5 + 0.5;
        }
    }

    // topology
    if (triangulate) {
        int num_faces = (resolution - 1) * (resolution - 1) * 2;
        int num_indices = num_faces * 3;

        counts.resize(num_faces);
        indices.resize(num_indices);
        for (int iy = 0; iy < resolution - 1; ++iy) {
            for (int ix = 0; ix < resolution - 1; ++ix) {
                int i = (resolution - 1)*iy + ix;
                counts[i * 2 + 0] = 3;
                counts[i * 2 + 1] = 3;
                indices[i * 6 + 0] = resolution*iy + ix;
                indices[i * 6 + 1] = resolution*(iy + 1) + ix;
                indices[i * 6 + 2] = resolution*(iy + 1) + (ix + 1);
                indices[i * 6 + 3] = resolution*iy + ix;
                indices[i * 6 + 4] = resolution*(iy + 1) + (ix + 1);
                indices[i * 6 + 5] = resolution*iy + (ix + 1);
            }
        }
    }
    else {
        int num_faces = (resolution - 1) * (resolution - 1);
        int num_indices = num_faces * 4;

        counts.resize(num_faces);
        indices.resize(num_indices);
        for (int iy = 0; iy < resolution - 1; ++iy) {
            for (int ix = 0; ix < resolution - 1; ++ix) {
                int i = (resolution - 1)*iy + ix;
                counts[i] = 4;
                indices[i * 4 + 0] = resolution*iy + ix;
                indices[i * 4 + 1] = resolution*(iy + 1) + ix;
                indices[i * 4 + 2] = resolution*(iy + 1) + (ix + 1);
                indices[i * 4 + 3] = resolution*iy + (ix + 1);
            }
        }
    }
}

#include "pch.h"
#include "muMath.h"
#include "muVertex.h"

namespace mu {

template<class VertexT> static inline void InterleaveImpl(VertexT *dst, const typename VertexT::arrays_t& src, size_t i);

template<> inline void InterleaveImpl(vertex_v3n3 *dst, const vertex_v3n3::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
}
template<> inline void InterleaveImpl(vertex_v3n3c4 *dst, const vertex_v3n3c4::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
    dst[i].c = src.colors[i];
}
template<> inline void InterleaveImpl(vertex_v3n3u2 *dst, const vertex_v3n3u2::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
    dst[i].u = src.uvs[i];
}
template<> inline void InterleaveImpl(vertex_v3n3c4u2 *dst, const vertex_v3n3c4u2::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
    dst[i].c = src.colors[i];
    dst[i].u = src.uvs[i];
}
template<> inline void InterleaveImpl(vertex_v3n3u2t4 *dst, const vertex_v3n3u2t4::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
    dst[i].u = src.uvs[i];
    dst[i].t = src.tangents[i];
}
template<> inline void InterleaveImpl(vertex_v3n3c4u2t4 *dst, const vertex_v3n3c4u2t4::arrays_t& src, size_t i)
{
    dst[i].p = src.points[i];
    dst[i].n = src.normals[i];
    dst[i].c = src.colors[i];
    dst[i].u = src.uvs[i];
    dst[i].t = src.tangents[i];
}

template<class VertexT>
void TInterleave(VertexT *dst, const typename VertexT::arrays_t& src, size_t num)
{
    for (size_t i = 0; i < num; ++i) {
        InterleaveImpl(dst, src, i);
    }
}

VertexFormat GuessVertexFormat(
    const float3 *points,
    const float3 *normals,
    const float4 *colors,
    const float2 *uvs,
    const float4 *tangents
)
{
    if (points && normals) {
        if (colors && uvs && tangents) { return VertexFormat::V3N3C4U2T4; }
        if (colors && uvs) { return VertexFormat::V3N3C4U2; }
        if (uvs && tangents) { return VertexFormat::V3N3U2T4; }
        if (uvs) { return VertexFormat::V3N3U2; }
        if (colors) { return VertexFormat::V3N3C4; }
        return VertexFormat::V3N3;
    }
    return VertexFormat::Unknown;
}

size_t GetVertexSize(VertexFormat format)
{
    switch (format) {
    case VertexFormat::V3N3: return sizeof(vertex_v3n3);
    case VertexFormat::V3N3C4: return sizeof(vertex_v3n3c4);
    case VertexFormat::V3N3U2: return sizeof(vertex_v3n3u2);
    case VertexFormat::V3N3C4U2: return sizeof(vertex_v3n3c4u2);
    case VertexFormat::V3N3U2T4: return sizeof(vertex_v3n3u2t4);
    case VertexFormat::V3N3C4U2T4: return sizeof(vertex_v3n3c4u2t4);
    default: return 0;
    }
}

void Interleave(void *dst, VertexFormat format, size_t num,
    const float3 *points,
    const float3 *normals,
    const float4 *colors,
    const float2 *uvs,
    const float4 *tangents
)
{
    switch (format) {
    case VertexFormat::V3N3: TInterleave((vertex_v3n3*)dst, {points, normals}, num); break;
    case VertexFormat::V3N3C4: TInterleave((vertex_v3n3c4*)dst, { points, normals, colors }, num); break;
    case VertexFormat::V3N3U2: TInterleave((vertex_v3n3u2*)dst, { points, normals, uvs }, num); break;
    case VertexFormat::V3N3C4U2: TInterleave((vertex_v3n3c4u2*)dst, { points, normals, colors, uvs }, num); break;
    case VertexFormat::V3N3U2T4: TInterleave((vertex_v3n3u2t4*)dst, { points, normals, uvs, tangents }, num); break;
    case VertexFormat::V3N3C4U2T4: TInterleave((vertex_v3n3c4u2t4*)dst, { points, normals, colors, uvs, tangents }, num); break;
    default: break;
    }
}

} // namespace mu

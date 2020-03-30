#include "ispcmath.h"

#ifdef muSIMD_FloatToHalf
export void FloatToHalf(
    uniform half dst[],
    uniform const float src[],
    uniform const int num)
{
    foreach(i=0 ... num) {
        dst[i] = float_to_half(src[i]);
    }
}
#endif

#ifdef muSIMD_HalfToFloat
export void HalfToFloat(
    uniform float dst[],
    uniform const half src[],
    uniform const int num)
{
    foreach(i=0 ... num) {
        dst[i] = half_to_float(src[i]);
    }
}
#endif

#ifdef muSIMD_NearEqual
export uniform bool NearEqual(
    uniform const float src1[], uniform const float src2[], uniform const int num, uniform const float eps)
{
    float tdiff = 0.0f;
    foreach(i=0 ... num) {
        tdiff = max(tdiff, abs(src1[i] - src2[i]));
    }
    return reduce_max(tdiff) < eps;
}
#endif

#ifdef muSIMD_MulVectors3
export void MulVectors3(uniform const float4x4& m_, uniform const float3 src[], uniform float3 dst[], uniform int num_data)
{
    uniform float4x4 m = m_;

    uniform int num_data_simd = num_data & ~(C - 1);
    for (uniform int bi = 0; bi < num_data_simd; bi += C) {
        float3 v;
        aos_to_soa3((uniform float*)&src[bi], &v.x, &v.y, &v.z);

        float3 r = {
            m.m[0].x * v.x + m.m[1].x * v.y + m.m[2].x * v.z,
            m.m[0].y * v.x + m.m[1].y * v.y + m.m[2].y * v.z,
            m.m[0].z * v.x + m.m[1].z * v.y + m.m[2].z * v.z,
        };
        soa_to_aos3(r.x, r.y, r.z, (uniform float*)&dst[bi]);
    }

    for(uniform int i = num_data_simd; i < num_data; ++i) {
        uniform float3 v = src[i];
        uniform float3 r = {
            m.m[0].x * v.x + m.m[1].x * v.y + m.m[2].x * v.z,
            m.m[0].y * v.x + m.m[1].y * v.y + m.m[2].y * v.z,
            m.m[0].z * v.x + m.m[1].z * v.y + m.m[2].z * v.z,
        };
        dst[i] = r;
    }
}
#endif

#ifdef muSIMD_MulPoints3
export void MulPoints3(uniform const float4x4& m_, uniform const float3 src[], uniform float3 dst[], uniform int num_data)
{
    uniform float4x4 m = m_;

    uniform int num_data_simd = num_data & ~(C - 1);
    for (uniform int bi = 0; bi < num_data_simd; bi += C) {
        float3 v;
        aos_to_soa3((uniform float*)&src[bi], &v.x, &v.y, &v.z);

        float3 r = {
            m.m[0].x * v.x + m.m[1].x * v.y + m.m[2].x * v.z + m.m[3].x,
            m.m[0].y * v.x + m.m[1].y * v.y + m.m[2].y * v.z + m.m[3].y,
            m.m[0].z * v.x + m.m[1].z * v.y + m.m[2].z * v.z + m.m[3].z,
        };
        soa_to_aos3(r.x, r.y, r.z, (uniform float*)&dst[bi]);
    }

    for(uniform int i = num_data_simd; i < num_data; ++i) {
        uniform float3 v = src[i];
        uniform float3 r = {
            m.m[0].x * v.x + m.m[1].x * v.y + m.m[2].x * v.z + m.m[3].x,
            m.m[0].y * v.x + m.m[1].y * v.y + m.m[2].y * v.z + m.m[3].y,
            m.m[0].z * v.x + m.m[1].z * v.y + m.m[2].z * v.z + m.m[3].z,
        };
        dst[i] = r;
    }
}
#endif

#ifdef muSIMD_MinMax3
export void MinMax3(
    uniform const float3 src[], uniform const int num,
    uniform float3& dst_min, uniform float3& dst_max)
{
    if(num == 0) { return; }

    uniform float3 rmin = src[0], rmax = src[0];

    const uniform int block_size = C;
    const uniform int num_loops = num / block_size;
    if(num_loops > 1) {
        const uniform float * uniform fv = (const uniform float * uniform)src;
        uniform float tmin[3][C];
        uniform float tmax[3][C];
        tmin[0][I] = tmax[0][I] = fv[C*0 + I];
        tmin[1][I] = tmax[1][I] = fv[C*1 + I];
        tmin[2][I] = tmax[2][I] = fv[C*2 + I];

        for(uniform int i=1; i < num_loops; ++i) {
            uniform const int i3 = i*3;

            float _0 = fv[C*(i3+0) + I];
            tmin[0][I] = min(tmin[0][I], _0);
            tmax[0][I] = max(tmax[0][I], _0);

            float _1 = fv[C*(i3+1) + I];
            tmin[1][I] = min(tmin[1][I], _1);
            tmax[1][I] = max(tmax[1][I], _1);

            float _2 = fv[C*(i3+2) + I];
            tmin[2][I] = min(tmin[2][I], _2);
            tmax[2][I] = max(tmax[2][I], _2);
        }

        float x,y,z;
        aos_to_soa3((uniform float*)&tmin[0], &x, &y, &z);
        rmin.x = reduce_min(x);
        rmin.y = reduce_min(y);
        rmin.z = reduce_min(z);

        aos_to_soa3((uniform float*)&tmax[0], &x, &y, &z);
        rmax.x = reduce_max(x);
        rmax.y = reduce_max(y);
        rmax.z = reduce_max(z);
    }

    for(uniform int i=num_loops*block_size; i < num; ++i) {
        uniform float3 t = src[i];
        rmin.x = min(rmin.x, t.x);
        rmin.y = min(rmin.y, t.y);
        rmin.z = min(rmin.z, t.z);
        rmax.x = max(rmax.x, t.x);
        rmax.y = max(rmax.y, t.y);
        rmax.z = max(rmax.z, t.z);
    }

    dst_min = rmin;
    dst_max = rmax;
}
#endif

#ifdef muSIMD_MinMax2
export void MinMax2(
    uniform const float2 src[], uniform const int num,
    uniform float2& dst_min, uniform float2& dst_max)
{
    if(num == 0) { return; }

    uniform float2 rmin = src[0];
    uniform float2 rmax = src[0];

    const uniform int block_size = C;
    const uniform int num_loops = num / block_size;
    if(num_loops > 1) {
        const uniform float * uniform fv = (const uniform float * uniform)src;

        float tmin[2], tmax[2];
        tmin[0] = tmax[0] = fv[  I];
        tmin[1] = tmax[1] = fv[C+I];
        for(uniform int i=1; i < num_loops; ++i) {
            uniform const int i2 = i*2;
            tmin[0] = min(tmin[0], fv[C*(i2+0) + I]);
            tmax[0] = max(tmax[0], fv[C*(i2+0) + I]);
            tmin[1] = min(tmin[1], fv[C*(i2+1) + I]);
            tmax[1] = max(tmax[1], fv[C*(i2+1) + I]);
        }

        rmin.x = reduce_min(shuffle(tmin[0], tmin[1], I*2 + 0));
        rmin.y = reduce_min(shuffle(tmin[0], tmin[1], I*2 + 1));
        rmax.x = reduce_max(shuffle(tmax[0], tmax[1], I*2 + 0));
        rmax.y = reduce_max(shuffle(tmax[0], tmax[1], I*2 + 1));
    }

    for(uniform int i=num_loops*block_size; i < num; ++i) {
        uniform float2 t = src[i];
        rmin.x = min(rmin.x, t.x);
        rmin.y = min(rmin.y, t.y);
        rmax.x = max(rmax.x, t.x);
        rmax.y = max(rmax.y, t.y);
    }

    dst_min = rmin;
    dst_max = rmax;
}
#endif

#ifdef muSIMD_InvertX3
export void InvertX3(uniform float3 dst[], uniform const int num)
{
    const uniform int num_loops = num / C;

    {
        uniform float _c[3][C];
        _c[0][I] = select((C*0 + I)%3==0, -1.0f, 1.0f);
        _c[1][I] = select((C*1 + I)%3==0, -1.0f, 1.0f);
        _c[2][I] = select((C*2 + I)%3==0, -1.0f, 1.0f);

        uniform float * uniform fv = (uniform float * uniform)dst;
        for(uniform int i=0; i < num_loops; ++i) {
            uniform int i3 = i*3;
            fv[C*(i3+0) + I] = fv[C*(i3+0) + I] * _c[0][I];
            fv[C*(i3+1) + I] = fv[C*(i3+1) + I] * _c[1][I];
            fv[C*(i3+2) + I] = fv[C*(i3+2) + I] * _c[2][I];
        }
    }

    for(uniform int i=num_loops*C; i < num; ++i) {
        dst[i].x *= -1.0f;
    }
}
#endif

#ifdef muSIMD_InvertX4
export void InvertX4(uniform float4 dst[], uniform const int num)
{
    const uniform int num_loops = num / (C/4);

    {
        uniform float _c[C];
        _c[I] = select(I%4==0, -1.0f, 1.0f);

        uniform float * uniform fv = (uniform float * uniform)dst;
        for(uniform int i=0; i < num_loops; ++i) {
            fv[C*i + I] = fv[C*i + I] * _c[I];
        }
    }

    for(uniform int i=num_loops*C; i < num; ++i) {
        dst[i].x *= -1.0f;
    }
}
#endif


#ifdef muSIMD_Scale
export void Scale(uniform float dst[], uniform const float scale, uniform const int num)
{
    const uniform int num_loops = num / (C*4);

    {
        uniform float * uniform fv = (uniform float * uniform)dst;
        for(uniform int i=0; i < num_loops; ++i) {
            uniform int i4 = i*4;
            fv[C*(i4+0) + I] = fv[C*(i4+0) + I] * scale;
            fv[C*(i4+1) + I] = fv[C*(i4+1) + I] * scale;
            fv[C*(i4+2) + I] = fv[C*(i4+2) + I] * scale;
            fv[C*(i4+3) + I] = fv[C*(i4+3) + I] * scale;
        }
    }

    for(uniform int i=num_loops*(C*4); i < num; ++i) {
        dst[i] *= scale;
    }
}
#endif

#ifdef muSIMD_Normalize
export void Normalize(
    uniform float3 dst[],
    uniform const int num)
{
    uniform int num_simd = num & ~(C - 1);
    for(uniform int bi=0; bi < num_simd; bi+=C) {
        float3 n;
        aos_to_soa3((uniform float*)&dst[bi], &n.x, &n.y, &n.z);
        n = normalize(n);
        soa_to_aos3(n.x, n.y, n.z, (uniform float*)&dst[bi]);
    }

    for(uniform int i=num_simd; i < num; ++i) {
        dst[i] = normalize(dst[i]);
    }
}
#endif


#ifdef muSIMD_Lerp
export void Lerp(uniform float dst[], uniform const float src1[], uniform const float src2[], uniform const int num, uniform float w)
{
    uniform float iw = 1.0f - w;
    foreach(i=0 ... num) {
        dst[i] = src1[i]*w + src2[i]*iw;
    }
}
#endif


#ifdef muSIMD_RayTrianglesIntersectionIndexed
export uniform int RayTrianglesIntersectionIndexed(
    uniform const float3& pos, uniform const float3& dir,
    uniform const float3 vertices[], uniform const int indices[], uniform const int num_triangles,
    uniform int& tindex, uniform float& distance)
{
    uniform int total_hit = 0;
    distance = FLT_MAX;

    // SIMD pass
    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi + I;
        int ti3 = ti * 3;
        float3 p1, p2, p3;
        // this emits warnings but performace is acceptable.
        p1 = vertices[indices[ti3  + 0]];
        p2 = vertices[indices[ti3  + 1]];
        p3 = vertices[indices[ti3  + 2]];

        float d;
        bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(any(hit)) {
            uniform int hita[C]; hita[I] = hit;
            uniform float da[C]; da[I] = d;
            for(uniform int i = 0; i < C; ++i) {
                if(hita[i]) {
                    total_hit++;
                    if(da[i] < distance) {
                        tindex = bi + i;
                        distance = da[i];
                    }
                }
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti = num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti * 3;
        uniform float3 p1 = vertices[indices[ti3 + 0]];
        uniform float3 p2 = vertices[indices[ti3 + 1]];
        uniform float3 p3 = vertices[indices[ti3 + 2]];

        uniform float d;
        uniform bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(hit) {
            total_hit++;
            if(d < distance) {
                tindex = ti;
                distance = d;
            }
        }
    }

    return total_hit;
}
#endif

#ifdef muSIMD_RayTrianglesIntersectionFlattened
export uniform int RayTrianglesIntersectionFlattened(
    uniform const float3& pos, uniform const float3& dir,
    uniform const float3 vertices[], uniform const int num_triangles,
    uniform int& tindex, uniform float& distance)
{
    uniform int total_hit = 0;
    distance = FLT_MAX;

    // SIMD pass
    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi + I;
        int ti3 = ti * 3;
        float3 p1, p2, p3;
        // this emits warnings but performace is acceptable.
        p1 = vertices[ti3  + 0];
        p2 = vertices[ti3  + 1];
        p3 = vertices[ti3  + 2];

        float d;
        bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(any(hit)) {
            uniform int hita[C]; hita[I] = hit;
            uniform float da[C]; da[I] = d;
            for(uniform int i = 0; i < C; ++i) {
                if(hita[i]) {
                    total_hit++;
                    if(da[i] < distance) {
                        tindex = bi + i;
                        distance = da[i];
                    }
                }
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti = num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti * 3;
        uniform float3 p1 = vertices[ti + 0];
        uniform float3 p2 = vertices[ti + 1];
        uniform float3 p3 = vertices[ti + 2];

        uniform float d;
        uniform bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(hit) {
            total_hit++;
            if(d < distance) {
                tindex = ti;
                distance = d;
            }
        }
    }

    return total_hit;
}
#endif

#ifdef muSIMD_RayTrianglesIntersectionSoA
export uniform int RayTrianglesIntersectionSoA(
    uniform const float3& pos, uniform const float3& dir,
    uniform const float v1x[], uniform const float v1y[], uniform const float v1z[],
    uniform const float v2x[], uniform const float v2y[], uniform const float v2z[],
    uniform const float v3x[], uniform const float v3y[], uniform const float v3z[],
    uniform const int num_triangles,
    uniform int& tindex, uniform float& distance)
{
    uniform int total_hit = 0;
    distance = FLT_MAX;

    // SIMD pass
    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        float3 p1 = {v1x[bi+I], v1y[bi+I], v1z[bi+I]};
        float3 p2 = {v2x[bi+I], v2y[bi+I], v2z[bi+I]};
        float3 p3 = {v3x[bi+I], v3y[bi+I], v3z[bi+I]};

        float d;
        bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(any(hit)) {
            uniform int hita[C]; hita[I] = hit;
            uniform float da[C]; da[I] = d;
            for(uniform int i = 0; i < C; ++i) {
                if(hita[i]) {
                    total_hit++;
                    if(da[i] < distance) {
                        tindex = bi + i;
                        distance = da[i];
                    }
                }
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti = num_triangles_simd; ti < num_triangles; ++ti) {
        uniform float3 p1 = {v1x[ti], v1y[ti], v1z[ti]};
        uniform float3 p2 = {v2x[ti], v2y[ti], v2z[ti]};
        uniform float3 p3 = {v3x[ti], v3y[ti], v3z[ti]};

        uniform float d;
        uniform bool hit = ray_triangle_intersection(pos, dir, p1, p2, p3, d);
        if(hit) {
            total_hit++;
            if(d < distance) {
                tindex = ti;
                distance = d;
            }
        }
    }

    return total_hit;
}
#endif

#ifdef muSIMD_PolyInside
export uniform int PolyInsideImpl(
    uniform const float2 points[], uniform int ngon, uniform float2& minp, uniform float2& maxp, uniform float2& pos,
    uniform float xc[], uniform int maxxc)
{
    if (pos.x < minp.x || pos.x > maxp.x ||
        pos.y < minp.y || pos.y > maxp.y)
    {
        return 0;
    }

    uniform int c = 0;
    uniform int ngon_simd = (ngon - 1) & ~(C - 1);

    // SIMD pass
    for (uniform int bi = 0; bi < ngon_simd; bi += C) {
        float2 p1, p2;
        {
            float t1 = ((uniform const float*)points)[bi*2 + I    ];
            float t2 = ((uniform const float*)points)[bi*2 + C+I  ];
            float t3 = ((uniform const float*)points)[bi*2 + I  +2];
            float t4 = ((uniform const float*)points)[bi*2 + C+I+2];

            float2 tp1 = {
                shuffle(t1, t2, I*2 + 0),
                shuffle(t1, t2, I*2 + 1) };
            float2 tp2 = {
                shuffle(t3, t4, I*2 + 0),
                shuffle(t3, t4, I*2 + 1) };

            bool needs_swap = tp1.y > tp2.y;
            p1.x = select(needs_swap, tp2.x, tp1.x);
            p1.y = select(needs_swap, tp2.y, tp1.y);
            p2.x = select(needs_swap, tp1.x, tp2.x);
            p2.y = select(needs_swap, tp1.y, tp2.y);
        }

        bool intersect =
            (p1.y != p2.y) &&
            ((pos.y >= p1.y && pos.y < p2.y) ||
             (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y));

        if (any(intersect)) {
            float x = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;

            uniform int intersecta[C]; intersecta[I] = intersect;
            uniform float xa[C]; xa[I] = x;
            for (uniform int ci = 0; ci < C; ++ci) {
                if (intersecta[ci]) {
                    xc[c++] = xa[ci];
                    if (c == maxxc) return c;
                }
            }
        }
    }

    // non-SIMD pass
    for (uniform int i = ngon_simd; i < ngon; ++i) {
        uniform int j = i + 1;
        if (j == ngon) { j = 0; }

        uniform float2 p1 = points[i];
        uniform float2 p2 = points[j];
        if(p1.y == p2.y) { continue; }
        else if(p1.y > p2.y) {
            uniform float2 tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

        uniform bool intersect =
            (pos.y >= p1.y && pos.y < p2.y) ||
            (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y);

        if (intersect) {
            xc[c++] = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
            if (c == maxxc) return c;
        }
    }
    return c;
}
#endif


#ifdef muSIMD_PolyInsideSoA
export uniform int PolyInsideSoAImpl(
    uniform const float px[], uniform const float py[], uniform int ngon, uniform float2& minp, uniform float2& maxp, uniform float2& pos,
    uniform float xc[], uniform int maxxc)
{
    if (pos.x < minp.x || pos.x > maxp.x ||
        pos.y < minp.y || pos.y > maxp.y)
    {
        return 0;
    }

    uniform int c = 0;
    uniform int ngon_simd = (ngon - 1) & ~(C - 1);

    // SIMD pass
    for (uniform int bi = 0; bi < ngon_simd; bi += C) {
        float2 p1, p2;
        {
            float2 tp1 = { px[bi+I+0], py[bi+I+0] };
            float2 tp2 = { px[bi+I+1], py[bi+I+1] };

            bool needs_swap = tp1.y > tp2.y;
            p1.x = select(needs_swap, tp2.x, tp1.x);
            p1.y = select(needs_swap, tp2.y, tp1.y);
            p2.x = select(needs_swap, tp1.x, tp2.x);
            p2.y = select(needs_swap, tp1.y, tp2.y);
        }

        bool intersect =
            (p1.y != p2.y) &&
            ((pos.y >= p1.y && pos.y < p2.y) ||
             (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y));

        if (any(intersect)) {
            float x = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;

            uniform int intersecta[C]; intersecta[I] = intersect;
            uniform float xa[C]; xa[I] = x;
            for (uniform int ci = 0; ci < C; ++ci) {
                if (intersecta[ci]) {
                    xc[c++] = xa[ci];
                    if (c == maxxc) return c;
                }
            }
        }
    }

    // non-SIMD pass
    for (uniform int i = ngon_simd; i < ngon; ++i) {
        uniform int j = i + 1;
        if (j == ngon) { j = 0; }

        uniform float2 p1 = {px[i], py[i]};
        uniform float2 p2 = {px[j], py[j]};
        if(p1.y == p2.y) { continue; }
        else if(p1.y > p2.y) {
            uniform float2 tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

        uniform bool intersect =
            (pos.y >= p1.y && pos.y < p2.y) ||
            (pos.y == maxp.y && pos.y > p1.y && pos.y <= p2.y);

        if (intersect) {
            xc[c++] = (pos.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
            if (c == maxxc) return c;
        }
    }
    return c;
}
#endif


static inline void NormalizeSoAToAoS(uniform float3 dst[],
    uniform float srcx[], uniform float srcy[], uniform float srcz[], uniform const int num)
{
    uniform int num_simd = num & ~(C - 1);

    for (uniform int bi = 0; bi < num_simd; bi += C) {
        int i = bi + I;
        float3 n = { srcx[i], srcy[i], srcz[i] };
        n = normalize(n);
        soa_to_aos3(n.x, n.y, n.z, (uniform float*)&dst[bi]);
    }

    for (uniform int i = num_simd; i < num; ++i) {
        uniform float3 n = { srcx[i], srcy[i], srcz[i] };
        dst[i] = normalize(n);
    }
}

#ifdef muSIMD_GenerateNormalsTriangleIndexed
export void GenerateNormalsTriangleIndexed(uniform float3 dst[],
    uniform const float3 vertices[], uniform const int indices[],
    uniform const int num_triangles, uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 3];
    zeroclear(mem_tmp, num_vertices_aligned * 3);
    float * uniform tnx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tny = mem_tmp + num_vertices_aligned * 1;
    float * uniform tnz = mem_tmp + num_vertices_aligned * 2;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        int ti3 = ti * 3;
        float3 p0 = vertices[indices[ti3  + 0]];
        float3 p1 = vertices[indices[ti3  + 1]];
        float3 p2 = vertices[indices[ti3  + 2]];
        float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int ci=0; ci<C; ++ci) {
            uniform float3 cn = {extract(n.x, ci), extract(n.y, ci), extract(n.z, ci)};
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi + ci) * 3 + i];
                tnx[ix] += cn.x;
                tny[ix] += cn.y;
                tnz[ix] += cn.z;
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti * 3;
        uniform int i0 = indices[ti3 + 0];
        uniform int i1 = indices[ti3 + 1];
        uniform int i2 = indices[ti3 + 2];
        uniform float3 p0 = vertices[i0];
        uniform float3 p1 = vertices[i1];
        uniform float3 p2 = vertices[i2];
        uniform float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            tnx[ix] += n.x;
            tny[ix] += n.y;
            tnz[ix] += n.z;
        }
    }

    NormalizeSoAToAoS(dst, tnx, tny, tnz, num_vertices);
    delete[] mem_tmp;
}
#endif

#ifdef muSIMD_GenerateNormalsTriangleFlattened
export void GenerateNormalsTriangleFlattened(uniform float3 dst[],
    uniform const float3 vertices[], uniform const int indices[],
    uniform const int num_triangles, uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 3];
    zeroclear(mem_tmp, num_vertices_aligned * 3);
    float * uniform tnx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tny = mem_tmp + num_vertices_aligned * 1;
    float * uniform tnz = mem_tmp + num_vertices_aligned * 2;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        int ti3 = ti * 3;
        float3 p0 = vertices[ti3  + 0];
        float3 p1 = vertices[ti3  + 1];
        float3 p2 = vertices[ti3  + 2];
        float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int ci=0; ci<C; ++ci) {
            uniform float3 cn = {extract(n.x, ci), extract(n.y, ci), extract(n.z, ci)};
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi + ci) * 3 + i];
                tnx[ix] += cn.x;
                tny[ix] += cn.y;
                tnz[ix] += cn.z;
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti * 3;
        uniform float3 p0 = vertices[ti3 + 0];
        uniform float3 p1 = vertices[ti3 + 1];
        uniform float3 p2 = vertices[ti3 + 2];
        uniform float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            tnx[ix] += n.x;
            tny[ix] += n.y;
            tnz[ix] += n.z;
        }
    }

    NormalizeSoAToAoS(dst, tnx, tny, tnz, num_vertices);
    delete[] mem_tmp;
}
#endif

#ifdef muSIMD_GenerateNormalsTriangleSoA
export void GenerateNormalsTriangleSoA(uniform float3 dst[],
    uniform const float v1x[], uniform const float v1y[], uniform const float v1z[],
    uniform const float v2x[], uniform const float v2y[], uniform const float v2z[],
    uniform const float v3x[], uniform const float v3y[], uniform const float v3z[],
    uniform const int indices[],
    uniform const int num_triangles, uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 3];
    zeroclear(mem_tmp, num_vertices_aligned * 3);
    float * uniform tnx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tny = mem_tmp + num_vertices_aligned * 1;
    float * uniform tnz = mem_tmp + num_vertices_aligned * 2;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        float3 p0 = {v1x[ti], v1y[ti], v1z[ti]};
        float3 p1 = {v2x[ti], v2y[ti], v2z[ti]};
        float3 p2 = {v3x[ti], v3y[ti], v3z[ti]};
        float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int ci=0; ci<C; ++ci) {
            uniform float3 cn = {extract(n.x, ci), extract(n.y, ci), extract(n.z, ci)};
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi + ci) * 3 + i];
                tnx[ix] += cn.x;
                tny[ix] += cn.y;
                tnz[ix] += cn.z;
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti * 3;
        uniform float3 p0 = {v1x[ti], v1y[ti], v1z[ti]};
        uniform float3 p1 = {v2x[ti], v2y[ti], v2z[ti]};
        uniform float3 p2 = {v3x[ti], v3y[ti], v3z[ti]};
        uniform float3 n = cross(p1 - p0, p2 - p0);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            tnx[ix] += n.x;
            tny[ix] += n.y;
            tnz[ix] += n.z;
        }
    }

    NormalizeSoAToAoS(dst, tnx, tny, tnz, num_vertices);
    delete[] mem_tmp;
}
#endif


#ifdef muSIMD_GenerateTangentsTriangleIndexed
export void GenerateTangentsTriangleIndexed(uniform float4 dst[],
    uniform const float3 vertices[], uniform const float2 uv[], uniform const float3 normals[], uniform const int indices[],
    uniform const int num_triangles, uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 6];
    zeroclear(mem_tmp, num_vertices_aligned * 6);
    float * uniform ttx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tty = mem_tmp + num_vertices_aligned * 1;
    float * uniform ttz = mem_tmp + num_vertices_aligned * 2;
    float * uniform tbx = mem_tmp + num_vertices_aligned * 3;
    float * uniform tby = mem_tmp + num_vertices_aligned * 4;
    float * uniform tbz = mem_tmp + num_vertices_aligned * 5;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    uniform int num_vertices_simd = num_vertices & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        int ti3 = ti*3;
        float3 v[3] = {
             vertices[indices[ti3  + 0]],
             vertices[indices[ti3  + 1]],
             vertices[indices[ti3  + 2]],
        };
        float2 u[3] = {
             uv[indices[ti3  + 0]],
             uv[indices[ti3  + 1]],
             uv[indices[ti3  + 2]],
        };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int ci=0; ci<C; ++ci) {
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi+ci)*3 + i];
                ttx[ix]+=extract(t[i].x, ci); tty[ix]+=extract(t[i].y, ci); ttz[ix]+=extract(t[i].z, ci);
                tbx[ix]+=extract(b[i].x, ci); tby[ix]+=extract(b[i].y, ci); tbz[ix]+=extract(b[i].z, ci);
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti*3;
        uniform float3 v[3] = {
             vertices[indices[ti3  + 0]],
             vertices[indices[ti3  + 1]],
             vertices[indices[ti3  + 2]],
        };
        uniform float2 u[3] = {
             uv[indices[ti3  + 0]],
             uv[indices[ti3  + 1]],
             uv[indices[ti3  + 2]],
        };
        uniform float3 t[3];
        uniform float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            ttx[ix]+=t[i].x; tty[ix]+=t[i].y; ttz[ix]+=t[i].z;
            tbx[ix]+=b[i].x; tby[ix]+=b[i].y; tbz[ix]+=b[i].z;
        }
    }


    // SIMD pass
    for(uniform int bi=0; bi < num_vertices_simd; bi += C) {
        int vi = bi+I;
        float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        float3 n; aos_to_soa3((uniform float*)&normals[bi], &n.x, &n.y, &n.z);

        float4 result = orthogonalize_tangent(t, b, n);
        soa_to_aos4(result.x, result.y, result.z, result.w, (uniform float*)&dst[bi]);
    }

    // non-SIMD pass
    for(uniform int vi=num_vertices_simd; vi < num_vertices; ++vi) {
        uniform float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        uniform float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        uniform float3 n = normals[vi];
        dst[vi] = orthogonalize_tangent(t, b, n);
    }

    delete[] mem_tmp;
}
#endif

#ifdef muSIMD_GenerateTangentsTriangleFlattened
export void GenerateTangentsTriangleFlattened(uniform float4 dst[],
    uniform const float3 vertices[], uniform const float2 uv[], uniform const float3 normals[], uniform const int indices[],
    uniform const int num_triangles, uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 6];
    foreach(i=0 ... num_vertices_aligned * 6) { mem_tmp[i]=0.0f; }
    zeroclear(mem_tmp, num_vertices_aligned * 6);
    float * uniform ttx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tty = mem_tmp + num_vertices_aligned * 1;
    float * uniform ttz = mem_tmp + num_vertices_aligned * 2;
    float * uniform tbx = mem_tmp + num_vertices_aligned * 3;
    float * uniform tby = mem_tmp + num_vertices_aligned * 4;
    float * uniform tbz = mem_tmp + num_vertices_aligned * 5;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    uniform int num_vertices_simd = num_vertices & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        int ti3 = ti*3;
        float3 v[3] = {
             vertices[ti3  + 0],
             vertices[ti3  + 1],
             vertices[ti3  + 2],
        };
        float2 u[3] = {
             uv[ti3  + 0],
             uv[ti3  + 1],
             uv[ti3  + 2],
        };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int ci=0; ci<C; ++ci) {
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi+ci)*3 + i];
                ttx[ix]+=extract(t[i].x, ci); tty[ix]+=extract(t[i].y, ci); ttz[ix]+=extract(t[i].z, ci);
                tbx[ix]+=extract(b[i].x, ci); tby[ix]+=extract(b[i].y, ci); tbz[ix]+=extract(b[i].z, ci);
            }
        }
    }

    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti*3;
        uniform float3 v[3] = {
             vertices[ti3  + 0],
             vertices[ti3  + 1],
             vertices[ti3  + 2],
        };
        uniform float2 u[3] = {
             uv[ti3  + 0],
             uv[ti3  + 1],
             uv[ti3  + 2],
        };
        uniform float3 t[3];
        uniform float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            ttx[ix]+=t[i].x; tty[ix]+=t[i].y; ttz[ix]+=t[i].z;
            tbx[ix]+=b[i].x; tby[ix]+=b[i].y; tbz[ix]+=b[i].z;
        }
    }


    // SIMD pass
    for(uniform int bi=0; bi < num_vertices_simd; bi += C) {
        int vi = bi+I;
        float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        float3 n; aos_to_soa3((uniform float*)&normals[bi], &n.x, &n.y, &n.z);

        float4 result = orthogonalize_tangent(t, b, n);
        soa_to_aos4(result.x, result.y, result.z, result.w, (uniform float*)&dst[bi]);
    }

    // non-SIMD pass
    for(uniform int vi=num_vertices_simd; vi < num_vertices; ++vi) {
        uniform float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        uniform float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        uniform float3 n = normals[vi];
        dst[vi] = orthogonalize_tangent(t, b, n);
    }

    delete[] mem_tmp;
}
#endif

#ifdef muSIMD_GenerateTangentsTriangleSoA
export void GenerateTangentsTriangleSoA(
    uniform float4 dst[],
    uniform const float v1x[], uniform const float v1y[], uniform const float v1z[],
    uniform const float v2x[], uniform const float v2y[], uniform const float v2z[],
    uniform const float v3x[], uniform const float v3y[], uniform const float v3z[],

    uniform const float u1x[], uniform const float u1y[],
    uniform const float u2x[], uniform const float u2y[],
    uniform const float u3x[], uniform const float u3y[],

    uniform const float3 normals[],
    uniform const int indices[],
    uniform const int num_triangles,
    uniform const int num_vertices)
{
    uniform int num_vertices_aligned = (num_vertices + (C - 1)) & ~(C - 1);
    float * uniform mem_tmp = uniform new float[num_vertices_aligned * 6];
    zeroclear(mem_tmp, num_vertices_aligned * 6);
    float * uniform ttx = mem_tmp + num_vertices_aligned * 0;
    float * uniform tty = mem_tmp + num_vertices_aligned * 1;
    float * uniform ttz = mem_tmp + num_vertices_aligned * 2;
    float * uniform tbx = mem_tmp + num_vertices_aligned * 3;
    float * uniform tby = mem_tmp + num_vertices_aligned * 4;
    float * uniform tbz = mem_tmp + num_vertices_aligned * 5;

    uniform int num_triangles_simd = num_triangles & ~(C - 1);
    uniform int num_vertices_simd = num_vertices & ~(C - 1);

    // SIMD pass
    for(uniform int bi=0; bi < num_triangles_simd; bi += C) {
        int ti = bi+I;
        float3 v[3] = {
            {v1x[ti], v1y[ti], v1z[ti]},
            {v2x[ti], v2y[ti], v2z[ti]},
            {v3x[ti], v3y[ti], v3z[ti]}
        };
        float2 u[3] = {
            {u1x[ti], u1y[ti]},
            {u2x[ti], u2y[ti]},
            {u3x[ti], u3y[ti]}
        };
        float3 t[3];
        float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int ci=0; ci<C; ++ci) {
            for(uniform int i=0; i<3; ++i) {
                uniform int ix = indices[(bi+ci)*3 + i];
                ttx[ix]+=extract(t[i].x, ci); tty[ix]+=extract(t[i].y, ci); ttz[ix]+=extract(t[i].z, ci);
                tbx[ix]+=extract(b[i].x, ci); tby[ix]+=extract(b[i].y, ci); tbz[ix]+=extract(b[i].z, ci);
            }
        }
    }
    
    // non-SIMD pass
    for(uniform int ti=num_triangles_simd; ti < num_triangles; ++ti) {
        uniform int ti3 = ti*3;
        uniform float3 v[3] = {
            {v1x[ti], v1y[ti], v1z[ti]},
            {v2x[ti], v2y[ti], v2z[ti]},
            {v3x[ti], v3y[ti], v3z[ti]}
        };
        uniform float2 u[3] = {
            {u1x[ti], u1y[ti]},
            {u2x[ti], u2y[ti]},
            {u3x[ti], u3y[ti]}
        };
        uniform float3 t[3];
        uniform float3 b[3];
        compute_triangle_tangents(v, u, t, b);

        for(uniform int i=0; i<3; ++i) {
            uniform int ix = indices[ti3 + i];
            ttx[ix]+=t[i].x; tty[ix]+=t[i].y; ttz[ix]+=t[i].z;
            tbx[ix]+=b[i].x; tby[ix]+=b[i].y; tbz[ix]+=b[i].z;
        }
    }

    // SIMD pass
    for(uniform int bi=0; bi < num_vertices_simd; bi += C) {
        int vi = bi+I;
        float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        float3 n; aos_to_soa3((uniform float*)&normals[bi], &n.x, &n.y, &n.z);

        float4 result = orthogonalize_tangent(t, b, n);
        soa_to_aos4(result.x, result.y, result.z, result.w, (uniform float*)&dst[bi]);
    }

    // non-SIMD pass
    for(uniform int vi=num_vertices_simd; vi < num_vertices; ++vi) {
        uniform float3 t = float3_(ttx[vi], tty[vi], ttz[vi]);
        uniform float3 b = float3_(tbx[vi], tby[vi], tbz[vi]);
        uniform float3 n = normals[vi];
        dst[vi] = orthogonalize_tangent(t, b, n);
    }

    delete[] mem_tmp;
}
#endif

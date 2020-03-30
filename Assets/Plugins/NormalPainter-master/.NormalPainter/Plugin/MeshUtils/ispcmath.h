#define C programCount
#define I programIndex
#define FLT_MAX 3.402823466e+38F

#include "muSIMDConfig.h"
//#define ispcmathEstimate

typedef unsigned int16 half;

struct half2  { half x, y; };
struct half3  { half x, y, z; };
struct half4  { half x, y, z, w; };
struct quath  { half x, y, z, w; };
struct float2 { float x, y; };
struct float3 { float x, y, z; };
struct float4 { float x, y, z, w; };
struct quatf  { float x, y, z, w; };
struct float4x4 { float4 m[4]; };

static inline float2 float2_(float x, float y) { float2 r = { x,y }; return r; }
static inline float3 float3_(float x, float y, float z) { float3 r = { x,y,z }; return r; }
static inline float4 float4_(float x, float y, float z, float w) { float4 r = { x,y,z,w }; return r; }
static inline uniform float2 float2_(uniform float x, uniform float y) { uniform float2 r = { x,y }; return r; }
static inline uniform float3 float3_(uniform float x, uniform float y, uniform float z) { uniform float3 r = { x,y,z }; return r; }
static inline uniform float4 float4_(uniform float x, uniform float y, uniform float z, uniform float w) { uniform float4 r = { x,y,z,w }; return r; }


static inline float2 half_to_float(half2 h)
{
    float2 ret = {half_to_float(h.x), half_to_float(h.y)};
    return ret;
}
static inline float3 half_to_float(half3 h)
{
    float3 ret = {half_to_float(h.x), half_to_float(h.y), half_to_float(h.z)};
    return ret;
}
static inline float4 half_to_float(half4 h)
{
    float4 ret = {half_to_float(h.x), half_to_float(h.y), half_to_float(h.z), half_to_float(h.w)};
    return ret;
}


static inline float2 operator+(float2 a, float2 b) { float2 r = { a.x + b.x, a.y + b.y }; return r; }
static inline float2 operator-(float2 a, float2 b) { float2 r = { a.x - b.x, a.y - b.y }; return r; }
static inline float2 operator*(float2 a, float2 b) { float2 r = { a.x * b.x, a.y * b.y }; return r; }
static inline float2 operator/(float2 a, float2 b) { float2 r = { a.x / b.x, a.y / b.y }; return r; }
static inline float2 operator+(float2 a, float b) { float2 r = { a.x + b, a.y + b }; return r; }
static inline float2 operator-(float2 a, float b) { float2 r = { a.x - b, a.y - b }; return r; }
static inline float2 operator*(float2 a, float b) { float2 r = { a.x * b, a.y * b }; return r; }
static inline float2 operator/(float2 a, float b) { float2 r = { a.x / b, a.y / b }; return r; }
static inline float2 operator+(float a, float2 b) { float2 r = { a + b.x, a + b.y }; return r; }
static inline float2 operator-(float a, float2 b) { float2 r = { a - b.x, a - b.y }; return r; }
static inline float2 operator*(float a, float2 b) { float2 r = { a * b.x, a * b.y }; return r; }
static inline float2 operator/(float a, float2 b) { float2 r = { a / b.x, a / b.y }; return r; }

static inline float3 operator+(float3 a, float3 b) { float3 r = { a.x + b.x, a.y + b.y, a.z + b.z }; return r; }
static inline float3 operator-(float3 a, float3 b) { float3 r = { a.x - b.x, a.y - b.y, a.z - b.z }; return r; }
static inline float3 operator*(float3 a, float3 b) { float3 r = { a.x * b.x, a.y * b.y, a.z * b.z }; return r; }
static inline float3 operator/(float3 a, float3 b) { float3 r = { a.x / b.x, a.y / b.y, a.z / b.z }; return r; }
static inline float3 operator+(float3 a, float b) { float3 r = { a.x + b, a.y + b, a.z + b }; return r; }
static inline float3 operator-(float3 a, float b) { float3 r = { a.x - b, a.y - b, a.z - b }; return r; }
static inline float3 operator*(float3 a, float b) { float3 r = { a.x * b, a.y * b, a.z * b }; return r; }
static inline float3 operator/(float3 a, float b) { float3 r = { a.x / b, a.y / b, a.z / b }; return r; }
static inline float3 operator+(float a, float3 b) { float3 r = { a + b.x, a + b.y, a + b.z }; return r; }
static inline float3 operator-(float a, float3 b) { float3 r = { a - b.x, a - b.y, a - b.z }; return r; }
static inline float3 operator*(float a, float3 b) { float3 r = { a * b.x, a * b.y, a * b.z }; return r; }
static inline float3 operator/(float a, float3 b) { float3 r = { a / b.x, a / b.y, a / b.z }; return r; }

static inline uniform float2 operator+(uniform float2 a, uniform float2 b) { uniform float2 r = { a.x + b.x, a.y + b.y }; return r; }
static inline uniform float2 operator-(uniform float2 a, uniform float2 b) { uniform float2 r = { a.x - b.x, a.y - b.y }; return r; }
static inline uniform float2 operator*(uniform float2 a, uniform float2 b) { uniform float2 r = { a.x * b.x, a.y * b.y }; return r; }
static inline uniform float2 operator/(uniform float2 a, uniform float2 b) { uniform float2 r = { a.x / b.x, a.y / b.y }; return r; }
static inline uniform float2 operator+(uniform float2 a, uniform float b) { uniform float2 r = { a.x + b, a.y + b }; return r; }
static inline uniform float2 operator-(uniform float2 a, uniform float b) { uniform float2 r = { a.x - b, a.y - b }; return r; }
static inline uniform float2 operator*(uniform float2 a, uniform float b) { uniform float2 r = { a.x * b, a.y * b }; return r; }
static inline uniform float2 operator/(uniform float2 a, uniform float b) { uniform float2 r = { a.x / b, a.y / b }; return r; }
static inline uniform float2 operator+(uniform float a, uniform float2 b) { uniform float2 r = { a + b.x, a + b.y }; return r; }
static inline uniform float2 operator-(uniform float a, uniform float2 b) { uniform float2 r = { a - b.x, a - b.y }; return r; }
static inline uniform float2 operator*(uniform float a, uniform float2 b) { uniform float2 r = { a * b.x, a * b.y }; return r; }
static inline uniform float2 operator/(uniform float a, uniform float2 b) { uniform float2 r = { a / b.x, a / b.y }; return r; }

static inline uniform float3 operator+(uniform float3 a, uniform float3 b) { uniform float3 r = { a.x + b.x, a.y + b.y, a.z + b.z }; return r; }
static inline uniform float3 operator-(uniform float3 a, uniform float3 b) { uniform float3 r = { a.x - b.x, a.y - b.y, a.z - b.z }; return r; }
static inline uniform float3 operator*(uniform float3 a, uniform float3 b) { uniform float3 r = { a.x * b.x, a.y * b.y, a.z * b.z }; return r; }
static inline uniform float3 operator/(uniform float3 a, uniform float3 b) { uniform float3 r = { a.x / b.x, a.y / b.y, a.z / b.z }; return r; }
static inline uniform float3 operator+(uniform float3 a, uniform float b) { uniform float3 r = { a.x + b, a.y + b, a.z + b }; return r; }
static inline uniform float3 operator-(uniform float3 a, uniform float b) { uniform float3 r = { a.x - b, a.y - b, a.z - b }; return r; }
static inline uniform float3 operator*(uniform float3 a, uniform float b) { uniform float3 r = { a.x * b, a.y * b, a.z * b }; return r; }
static inline uniform float3 operator/(uniform float3 a, uniform float b) { uniform float3 r = { a.x / b, a.y / b, a.z / b }; return r; }
static inline uniform float3 operator+(uniform float a, uniform float3 b) { uniform float3 r = { a + b.x, a + b.y, a + b.z }; return r; }
static inline uniform float3 operator-(uniform float a, uniform float3 b) { uniform float3 r = { a - b.x, a - b.y, a - b.z }; return r; }
static inline uniform float3 operator*(uniform float a, uniform float3 b) { uniform float3 r = { a * b.x, a * b.y, a * b.z }; return r; }
static inline uniform float3 operator/(uniform float a, uniform float3 b) { uniform float3 r = { a / b.x, a / b.y, a / b.z }; return r; }

static inline uniform float2 reduce_add(float2 v)
{
    uniform float2 r = { reduce_add(v.x), reduce_add(v.y) };
    return r;
}
static inline uniform float3 reduce_add(float3 v)
{
    uniform float3 r = {reduce_add(v.x), reduce_add(v.y), reduce_add(v.z)};
    return r;
}

static inline float mod(float a, float b)
{
    return a - b * floor(a / b);
}
static inline uniform float mod(uniform float a, uniform float b)
{
    return a - b * floor(a / b);
}

static inline float frac(float a)
{
    return mod(a, 1.0);
}
static inline uniform float frac(uniform float a)
{
    return mod(a, 1.0);
}


#define VF1(f)\
    static inline float2 f(float2 a) { return float2_(f(a.x), f(a.y)); }\
    static inline float3 f(float3 a) { return float3_(f(a.x), f(a.y), f(a.z)); }\
    static inline float4 f(float4 a) { return float4_(f(a.x), f(a.y), f(a.z), f(a.w)); }\
    static inline uniform float2 f(uniform float2 a) { return float2_(f(a.x), f(a.y)); }\
    static inline uniform float3 f(uniform float3 a) { return float3_(f(a.x), f(a.y), f(a.z)); }\
    static inline uniform float4 f(uniform float4 a) { return float4_(f(a.x), f(a.y), f(a.z), f(a.w)); }\

#define VF2(f)\
    static inline float2 f(float2 a, float2 b) { return float2_(f(a.x, b.x), f(a.y, b.y)); }\
    static inline float3 f(float3 a, float3 b) { return float3_(f(a.x, b.x), f(a.y, b.y), f(a.z, b.z)); }\
    static inline float4 f(float4 a, float4 b) { return float4_(f(a.x, b.x), f(a.y, b.y), f(a.z, b.z), f(a.w, b.w)); }\
    static inline uniform float2 f(uniform float2 a, uniform float2 b) { return float2_(f(a.x, b.x), f(a.y, b.y)); }\
    static inline uniform float3 f(uniform float3 a, uniform float3 b) { return float3_(f(a.x, b.x), f(a.y, b.y), f(a.z, b.z)); }\
    static inline uniform float4 f(uniform float4 a, uniform float4 b) { return float4_(f(a.x, b.x), f(a.y, b.y), f(a.z, b.z), f(a.w, b.w)); }\

VF1(abs)
VF1(round)
VF1(floor)
VF1(ceil)
VF2(min)
VF2(max)
VF1(rcp)
VF1(sqrt)
VF1(rsqrt)
VF1(sin)
VF1(cos)
VF1(tan)
VF1(asin)
VF1(acos)
VF1(atan)
VF2(atan2)
VF1(exp)
VF1(log)
VF2(pow)
VF2(mod)
VF1(frac)

#undef VF1
#undef VF2


static inline float dot(float2 a, float2 b)
{
    return a.x*b.x + a.y*b.y;
}
static inline uniform float dot(uniform float2 a, uniform float2 b)
{
    return a.x*b.x + a.y*b.y;
}
static inline float dot(float3 a, float3 b)
{
    return a.x*b.x + a.y*b.y + a.z*b.z;
}
static inline uniform float dot(uniform float3 a, uniform float3 b)
{
    return a.x*b.x + a.y*b.y + a.z*b.z;
}


static inline float3 cross(float3 v0, float3 v1)
{
    float3 ret;
    ret.x = v0.y*v1.z - v0.z*v1.y;
    ret.y = v0.z*v1.x - v0.x*v1.z;
    ret.z = v0.x*v1.y - v0.y*v1.x;
    return ret;
}
static inline uniform float3 cross(uniform float3 v0, uniform float3 v1)
{
    uniform float3 ret;
    ret.x = v0.y*v1.z - v0.z*v1.y;
    ret.y = v0.z*v1.x - v0.x*v1.z;
    ret.z = v0.x*v1.y - v0.y*v1.x;
    return ret;
}


static inline float  rcp_precise(float  v) { return 1.0f / v; }
static inline float2 rcp_precise(float2 v) { return 1.0f / v; }
static inline float3 rcp_precise(float3 v) { return 1.0f / v; }
static inline uniform float  rcp_precise(uniform float  v) { return 1.0f / v; }
static inline uniform float2 rcp_precise(uniform float2 v) { return 1.0f / v; }
static inline uniform float3 rcp_precise(uniform float3 v) { return 1.0f / v; }
static inline float  rcp_estimate(float  v) { return rcp(v); }
static inline float2 rcp_estimate(float2 v) { return rcp(v); }
static inline float3 rcp_estimate(float3 v) { return rcp(v); }
static inline uniform float  rcp_estimate(uniform float  v) { return rcp(v); }
static inline uniform float2 rcp_estimate(uniform float2 v) { return rcp(v); }
static inline uniform float3 rcp_estimate(uniform float3 v) { return rcp(v); }

static inline float length_sq(float2 v) { return dot(v, v); }
static inline float length_sq(float3 v) { return dot(v, v); }
static inline uniform float length_sq(uniform float2 v) { return dot(v, v); }
static inline uniform float length_sq(uniform float3 v) { return dot(v, v); }

static inline float length(float2 v) { return sqrt(length_sq(v)); }
static inline float length(float3 v) { return sqrt(length_sq(v)); }
static inline uniform float length(uniform float2 v) { return sqrt(length_sq(v)); }
static inline uniform float length(uniform float3 v) { return sqrt(length_sq(v)); }

static inline float rlength_precise(float2 v) { return 1.0f / sqrt(length_sq(v)); }
static inline float rlength_precise(float3 v) { return 1.0f / sqrt(length_sq(v)); }
static inline uniform float rlength_precise(uniform float2 v) { return 1.0f / sqrt(length_sq(v)); }
static inline uniform float rlength_precise(uniform float3 v) { return 1.0f / sqrt(length_sq(v)); }
static inline float rlength_estimate(float2 v) { return rsqrt(length_sq(v)); }
static inline float rlength_estimate(float3 v) { return rsqrt(length_sq(v)); }
static inline uniform float rlength_estimate(uniform float2 v) { return rsqrt(length_sq(v)); }
static inline uniform float rlength_estimate(uniform float3 v) { return rsqrt(length_sq(v)); }

static inline float2 normalize_precise(float2 v) { return v * rlength_precise(v); }
static inline float3 normalize_precise(float3 v) { return v * rlength_precise(v); }
static inline uniform float2 normalize_precise(uniform float2 v) { return v * rlength_precise(v); }
static inline uniform float3 normalize_precise(uniform float3 v) { return v * rlength_precise(v); }
static inline float2 normalize_estimate(float2 v) { return v * rlength_estimate(v); }
static inline float3 normalize_estimate(float3 v) { return v * rlength_estimate(v); }
static inline uniform float2 normalize_estimate(uniform float2 v) { return v * rlength_estimate(v); }
static inline uniform float3 normalize_estimate(uniform float3 v) { return v * rlength_estimate(v); }

static inline float angle_between(float3 a, float3 b) { return acos(dot(a, b)); }
static inline uniform float angle_between(uniform float3 a, uniform float3 b) { return acos(dot(a, b)); }

static inline float angle_between2_precise(float3 pos1, float3 pos2, float3 center)
{
    return angle_between(
        normalize_precise(pos1 - center),
        normalize_precise(pos2 - center));
}
static inline uniform float angle_between2_precise(uniform float3 pos1, uniform float3 pos2, uniform float3 center)
{
    return angle_between(
        normalize_precise(pos1 - center),
        normalize_precise(pos2 - center));
}
static inline float angle_between2_estimate(float3 pos1, float3 pos2, float3 center)
{
    return angle_between(
        normalize_estimate(pos1 - center),
        normalize_estimate(pos2 - center));
}
static inline uniform float angle_between2_estimate(uniform float3 pos1, uniform float3 pos2, uniform float3 center)
{
    return angle_between(
        normalize_estimate(pos1 - center),
        normalize_estimate(pos2 - center));
}

static inline float lerp(float a, float b, float t) {
    return (1.0f-t)*a + t*b;
}
static inline uniform float lerp(uniform float a, uniform float b, uniform float t) {
    return (1.0f-t)*a + t*b;
}

static inline float3 lerp(float3 a, float3 b, float t) {
    return (1.0f-t)*a + t*b;
}
static inline uniform float3 lerp(uniform float3 a, uniform float3 b, uniform float t) {
    return (1.0f-t)*a + t*b;
}

#ifdef ispcmathEstimate
    #define rcp rcp_estimate
    #define rlength rlength_estimate
    #define normalize normalize_estimate
    #define angle_between2 angle_between2_estimate
#else
    #define rcp rcp_precise
    #define rlength rlength_precise
    #define normalize normalize_precise
    #define angle_between2 angle_between2_precise
#endif



static inline bool ray_triangle_intersection(uniform float3 pos, uniform float3 dir, float3 p1, float3 p2, float3 p3, float& distance)
{
    const float epsdet = 1e-10f;
    const float eps = 1e-4f;

    float3 e1 = p2 - p1;
    float3 e2 = p3 - p1;
    float3 p = cross(dir, e2);
    float det = dot(e1, p);
    float inv_det = rcp(det);
    float3 t = pos - p1;
    float u = dot(t, p) * inv_det;
    float3 q = cross(t, e1);
    float v = dot(dir, q) * inv_det;

    distance = dot(e2, q) * inv_det;
    return
        abs(det) > 1e-10f &&
        u > -eps && u < 1 + eps &&
        v > -eps && u + v < 1 + eps &&
        distance >= 0.0f;
}

// uniform variant
static inline uniform bool ray_triangle_intersection(uniform float3 pos, uniform float3 dir, uniform float3 p1, uniform float3 p2, uniform float3 p3, uniform float& distance)
{
    uniform const float epsdet = 1e-10f;
    uniform const float eps = 1e-4f;

    uniform float3 e1 = p2 - p1;
    uniform float3 e2 = p3 - p1;
    uniform float3 p = cross(dir, e2);
    uniform float det = dot(e1, p);
    uniform float inv_det = rcp(det);
    uniform float3 t = pos - p1;
    uniform float u = dot(t, p) * inv_det;
    uniform float3 q = cross(t, e1);
    uniform float v = dot(dir, q) * inv_det;

    distance = dot(e2, q) * inv_det;
    return
        abs(det) > epsdet &&
        u > -eps && u < 1 + eps &&
        v > -eps && u + v < 1 + eps &&
        distance >= 0.0f;
}


static inline float ray_point_distance(float3 pos, float3 dir, float3 p)
{
    return length(cross(dir, p - pos));
}

static inline uniform float ray_point_distance(uniform float3 pos, uniform float3 dir, uniform float3 p)
{
    return length(cross(dir, p - pos));
}


static inline void compute_triangle_tangents(
    const float3(&vertices)[3], const float2(&uv)[3],
    float3(&dst_tangent)[3], float3(&dst_binormal)[3])
{
    float3 p = vertices[1] - vertices[0];
    float3 q = vertices[2] - vertices[0];
    float2 s = { uv[1].x - uv[0].x, uv[2].x - uv[0].x };
    float2 t = { uv[1].y - uv[0].y, uv[2].y - uv[0].y };

    float3 tangent, binormal;
    float d = s.x * t.y - s.y * t.x;
    float area = abs(d);

    if (area > 1e-8f) {
        float rd = rcp(d);
        s = s * rd;
        t = t * rd;
        tangent = normalize_estimate(float3_(
            t.y * p.x - t.x * q.x,
            t.y * p.y - t.x * q.y,
            t.y * p.z - t.x * q.z
        )) * area;
        binormal = normalize_estimate(float3_(
            s.x * q.x - s.y * p.x,
            s.x * q.y - s.y * p.y,
            s.x * q.z - s.y * p.z
        )) * area;
    }
    else {
        tangent = binormal = float3_(0, 0, 0);
    }


    float angles[3] = {
        angle_between2_estimate(vertices[2], vertices[1], vertices[0]),
        angle_between2_estimate(vertices[0], vertices[2], vertices[1]),
        angle_between2_estimate(vertices[1], vertices[0], vertices[2]),
    };
    for (int v = 0; v < 3; ++v) {
        dst_tangent[v] = tangent * angles[v];
        dst_binormal[v] = binormal * angles[v];
    }
}
static inline void compute_triangle_tangents(
    uniform const float3(&vertices)[3], uniform const float2(&uv)[3],
    uniform float3(&dst_tangent)[3], uniform float3(&dst_binormal)[3])
{
    uniform float3 p = vertices[1] - vertices[0];
    uniform float3 q = vertices[2] - vertices[0];
    uniform float2 s = { uv[1].x - uv[0].x, uv[2].x - uv[0].x };
    uniform float2 t = { uv[1].y - uv[0].y, uv[2].y - uv[0].y };

    uniform float3 tangent, binormal;
    uniform float d = s.x * t.y - s.y * t.x;
    uniform float area = abs(d);

    if (area > 1e-8f) {
        uniform float rd = rcp(d);
        s = s * rd;
        t = t * rd;
        tangent = normalize_estimate(float3_(
            t.y * p.x - t.x * q.x,
            t.y * p.y - t.x * q.y,
            t.y * p.z - t.x * q.z
        )) * area;
        binormal = normalize_estimate(float3_(
            s.x * q.x - s.y * p.x,
            s.x * q.y - s.y * p.y,
            s.x * q.z - s.y * p.z
        )) * area;
    }
    else {
        tangent = binormal = float3_(0, 0, 0);
    }

    uniform float angles[3] = {
        angle_between2_estimate(vertices[2], vertices[1], vertices[0]),
        angle_between2_estimate(vertices[0], vertices[2], vertices[1]),
        angle_between2_estimate(vertices[1], vertices[0], vertices[2]),
    };
    for (uniform int v = 0; v < 3; ++v) {
        dst_tangent[v] = tangent * angles[v];
        dst_binormal[v] = binormal * angles[v];
    }
}

static inline float4 orthogonalize_tangent(float3 tangent, float3 binormal, float3 normal)
{
    float NdotT = dot(normal, tangent);
    tangent = tangent - normal * NdotT;
    float magT = length(tangent);
    tangent = tangent / magT;

    float NdotB = dot(normal, binormal);
    float TdotB = dot(tangent, binormal) * magT;
    binormal = binormal - normal * NdotB - tangent * TdotB;
    float magB = length(binormal);
    binormal = binormal / magB;

    const float epsilon = 1e-6f;
    if (magT <= epsilon || magB <= epsilon)
    {
        float dpXN = abs(dot(float3_(1.0f, 0.0f, 0.0f), normal));
        float dpYN = abs(dot(float3_(0.0f, 1.0f, 0.0f), normal));
        float dpZN = abs(dot(float3_(0.0f, 0.0f, 1.0f), normal));

        bool a1x = dpXN <= dpYN && dpXN <= dpZN;
        bool a1y = !a1x && (dpYN <= dpXN && dpYN <= dpZN);
        bool a1z = !a1x && !a1y;
        bool a2x = (a1y && dpXN <= dpZN) || (a1z && dpXN <= dpYN);
        bool a2y = (a1x && dpYN <= dpZN) || !a1z;
        bool a2z = !a1x || !a1y;

        float3 axis1 = {
            select(a1x, 1.0f, 0.0f),
            select(a1y, 1.0f, 0.0f),
            select(a1z, 1.0f, 0.0f),
        };
        float3 axis2 = {
            select(a2x, 1.0f, 0.0f),
            select(a2y, 1.0f, 0.0f),
            select(a2z, 1.0f, 0.0f),
        };
        tangent = normalize_estimate(axis1 - normal * dot(normal, axis1));
        binormal = normalize_estimate(axis2 - normal * dot(normal, axis2) - normalize_estimate(tangent) * dot(tangent, axis2));
    }
    return float4_(tangent.x, tangent.y, tangent.z,
        select(dot(cross(normal, tangent), binormal) > 0.0f, 1.0f, -1.0f) );
}

static inline uniform float4 orthogonalize_tangent(
    uniform float3 tangent, uniform float3 binormal, uniform float3 normal)
{
    uniform float NdotT = dot(normal, tangent);
    tangent = tangent - normal * NdotT;
    uniform float magT = length(tangent);
    tangent = tangent / magT;

    uniform float NdotB = dot(normal, binormal);
    uniform float TdotB = dot(tangent, binormal) * magT;
    binormal = binormal - normal * NdotB - tangent * TdotB;
    uniform float magB = length(binormal);
    binormal = binormal / magB;

    uniform const float epsilon = 1e-6f;
    if (magT <= epsilon || magB <= epsilon)
    {
        uniform float dpXN = abs(dot(float3_(1.0f, 0.0f, 0.0f), normal));
        uniform float dpYN = abs(dot(float3_(0.0f, 1.0f, 0.0f), normal));
        uniform float dpZN = abs(dot(float3_(0.0f, 0.0f, 1.0f), normal));

        uniform bool a1x = dpXN <= dpYN && dpXN <= dpZN;
        uniform bool a1y = !a1x && (dpYN <= dpXN && dpYN <= dpZN);
        uniform bool a1z = !a1x && !a1y;
        uniform bool a2x = (a1y && dpXN <= dpZN) || (a1z && dpXN <= dpYN);
        uniform bool a2y = (a1x && dpYN <= dpZN) || !a1z;
        uniform bool a2z = !a1x || !a1y;

        uniform float3 axis1 = {
            select(a1x, 1.0f, 0.0f),
            select(a1y, 1.0f, 0.0f),
            select(a1z, 1.0f, 0.0f),
        };
        uniform float3 axis2 = {
            select(a2x, 1.0f, 0.0f),
            select(a2y, 1.0f, 0.0f),
            select(a2z, 1.0f, 0.0f),
        };
        tangent = normalize_estimate(axis1 - normal * dot(normal, axis1));
        binormal = normalize_estimate(axis2 - normal * dot(normal, axis2) - normalize_estimate(tangent) * dot(tangent, axis2));
    }
    return float4_(tangent.x, tangent.y, tangent.z,
        select(dot(cross(normal, tangent), binormal) > 0.0f, 1.0f, -1.0f));
}


static inline void NormalizeImpl(uniform float3 dst[], uniform const int num)
{
    uniform int num_simd = num & ~(C - 1);

    for (uniform int bi = 0; bi < num_simd; bi += C) {
        float3 n;
        aos_to_soa3((uniform float*)(&dst[bi]), &n.x, &n.y, &n.z);
        n = normalize(n);
        soa_to_aos3(n.x, n.y, n.z, (uniform float*)(&dst[bi]));
    }

    for (uniform int i = num_simd; i < num; ++i) {
        dst[i] = normalize(dst[i]);
    }
}
static inline void NormalizeSoAImpl(uniform float dstx[], uniform float dsty[], uniform float dstz[], uniform const int num)
{
    foreach(i=0 ... num) {
        float3 n = {dstx[i], dsty[i], dstz[i]};
        n = normalize(n);
        dstx[i] = n.x;
        dsty[i] = n.y;
        dstz[i] = n.z;
    }
}

static inline void SoAToAoS(uniform float3 dst[], uniform float sx[], uniform float sy[], uniform float sz[], uniform const int num)
{
    uniform int num_simd = num & ~(C-1);
    for (uniform int bi = 0; bi < num_simd; bi += C) {
        int i = bi + I;
        uniform float3 aos[C];
        soa_to_aos3(sx[i], sy[i], sz[i], (uniform float*)&aos[0]);

        for (uniform int ci = 0; ci < C; ++ci) {
            dst[bi+ci] = aos[ci];
        }
    }

    for (uniform int i = num_simd; i < num; ++i) {
        dst[i] = float3_(sx[i], sy[i], sz[i]);
    }
}

static inline void zeroclear(uniform float dst[], uniform int size)
{
    memset(dst, 0, size*4);
    //foreach(i = 0 ... size) { dst[i] = 0.0f; }
    //int size = size + (C -1) & ~(C - 1);
    //for (uniform int bi = 0; bi < size; bi += C) { dst[bi+I] = 0.0f; }
}

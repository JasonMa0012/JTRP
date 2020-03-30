
inline bool device_available()
{
    static bool value = !Concurrency::accelerator().get_is_emulated();
    return value;
}


#define Def1A(A,F)                                                                  \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 2>* = nullptr>\
inline T A(T v) restrict(amp) {                                                     \
    return { F(v.x), F(v.y) };                                                      \
}                                                                                   \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 3>* = nullptr>\
inline T A(T l, T r) restrict(amp) {                                                \
    return { F(v.x), F(v.y), F(v.z) };                                              \
}                                                                                   \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 4>* = nullptr>\
inline T A(T l, T r) restrict(amp) {                                                \
    return { F(v.x), F(v.y), F(v.z), F(v.w) };                                      \
}

#define Def2A(A,F)                                                                  \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 2>* = nullptr>\
inline T A(T a, T b) restrict(amp) {                                                \
    return { F(a.x, b.x), F(a.y, b.y )};                                            \
}                                                                                   \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 3>* = nullptr>\
inline T A(T a, T b) restrict(amp) {                                                \
    return { F(a.x, b.x), F(a.y, b.y), F(a.z, b.z) };                               \
}                                                                                   \
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 4>* = nullptr>\
inline T A(T a, T b) restrict(amp) {                                                \
    return { F(a.x, b.x), F(a.y, b.y), F(a.z, b.z), F(a.w, b.w) };                  \
}

#define Def1(F) Def1A(F,F)
#define Def2(F) Def2A(F,F)

Def1A(abs, fabs)
Def1(round)
Def1(floor)
Def1(ceil)
Def2(min)
Def2(max)
Def1(rcp)
Def1A(sqrt, sqrtf)
Def1A(rsqrt, rsqrtf)
Def1(sin)
Def1(cos)
Def1(tan)
Def1(asin)
Def1(acos)
Def1(atan)
Def2(atan2)
Def1(exp)
Def1(log)
Def2(pow)
Def2(mod)
Def1(frac)

#undef Def2
#undef Def1

inline float abs(float v) restrict(amp) { return fabs(v); }
inline float sqrt(float v) restrict(amp) { return sqrtf(v); }
inline float rsqrt(float v) restrict(amp) { return rsqrtf(v); }

template<typename T, std::enable_if_t<short_vector_traits<T>::size == 2>* = nullptr>
inline typename short_vector_traits<T>::value_type dot(T l, T r) restrict(amp) {
    return l.x * r.x + l.y * r.y;
}
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 3>* = nullptr>
inline typename short_vector_traits<T>::value_type dot(T l, T r) restrict(amp) {
    return l.x * r.x + l.y * r.y + l.z * r.z;
}
template<typename T, std::enable_if_t<short_vector_traits<T>::size == 4>* = nullptr>
inline typename short_vector_traits<T>::value_type dot(T l, T r) restrict(amp) {
    return l.x * r.x + l.y * r.y + l.z * r.z + l.w * r.w;
}

template<typename T, std::enable_if_t<short_vector_traits<T>::size == 3>* = nullptr>
inline T cross(T l, T r) restrict(amp) {
    return {
        l.y * r.z - l.z * r.y,
        l.z * r.x - l.x * r.z,
        l.x * r.y - l.y * r.x };
}

template<typename T>
inline typename short_vector_traits<T>::value_type length_sq(T v) restrict(amp) {
    return dot(v, v);
}

template<typename T>
inline typename short_vector_traits<T>::value_type length(T v) restrict(amp) {
    return sqrt(dot(v, v));
}

template<typename T>
inline T normalize(T v) restrict(amp) {
    return v * rsqrt(dot(v, v));
}


inline bool ray_triangle_intersection(float_3 pos, float_3 dir, float_3 p1, float_3 p2, float_3 p3, float& distance) restrict(amp)
{
    const float epsdet = 1e-10f;
    const float eps = 1e-4f;

    float_3 e1 = p2 - p1;
    float_3 e2 = p3 - p1;
    float_3 p = cross(dir, e2);
    float det = dot(e1, p);
    if (abs(det) < epsdet) return false;
    float inv_det = 1.0f / det;
    float_3 t = pos - p1;
    float u = dot(t, p) * inv_det;
    if (u < -eps || u  > 1+ eps) return false;
    float_3 q = cross(t, e1);
    float v = dot(dir, q) * inv_det;
    if (v < -eps || u + v > 1+ eps) return false;

    distance = dot(e2, q) * inv_det;
    return distance >= 0.0f;
}
inline bool ray_triangle_intersection(float_3 pos, float_3 dir, float_4 p1, float_4 p2, float_4 p3, float& distance) restrict(amp)
{
    return ray_triangle_intersection(pos, dir,
        { p1.x, p1.y, p1.z },
        { p2.x, p2.y, p2.z },
        { p3.x, p3.y, p3.z }, distance);
}

template<typename T>
inline T triangle_interpolation(float_3 pos, float_3 p1, float_3 p2, float_3 p3, T x1, T x2, T x3) restrict(amp)
{
    float_3 f1 = p1 - pos;
    float_3 f2 = p2 - pos;
    float_3 f3 = p3 - pos;
    float a = 1.0f / length(cross(p1 - p2, p1 - p3));
    float a1 = length(cross(f2, f3)) * a;
    float a2 = length(cross(f3, f1)) * a;
    float a3 = length(cross(f1, f2)) * a;
    return x1 * a1 + x2 * a2 + x3 * a3;
}
template<typename T>
inline T triangle_interpolation(float_3 pos, float_4 p1, float_4 p2, float_4 p3, T x1, T x2, T x3) restrict(amp)
{
    return triangle_interpolation(pos,
        { p1.x, p1.y, p1.z },
        { p2.x, p2.y, p2.z },
        { p3.x, p3.y, p3.z },
        x1, x2, x3);
}

inline float ray_point_distance(float_3 pos, float_3 dir, float_3 p) restrict(amp)
{
    return length(cross(dir, p - pos));
}

#pragma once

#define Boilerplate(I, V)                                                       \
    using this_t            = I;                                                \
    using difference_type   = std::ptrdiff_t;                                   \
    using value_type        = typename std::iterator_traits<V>::value_type;     \
    using reference         = typename std::iterator_traits<V>::reference;      \
    using pointer           = typename std::iterator_traits<V>::pointer;        \
    using iterator_category = std::random_access_iterator_tag;                  \


template<class T, int Stride = 0>
struct strided_iterator
{
    Boilerplate(strided_iterator, T);
    static const int stride = Stride;

    uint8_t *data;

    reference operator*()  { return *(T*)data; }
    pointer   operator->() { return &(T*)data; }
    this_t  operator+(size_t v)  { return data + stride*v; }
    this_t  operator-(size_t v)  { return data + stride*v; }
    this_t& operator+=(size_t v) { data += stride*v; return *this; }
    this_t& operator-=(size_t v) { data -= stride*v; return *this; }
    this_t& operator++()         { data += stride; return *this; }
    this_t& operator++(int)      { data += stride; return *this; }
    this_t& operator--()         { data -= stride; return *this; }
    this_t& operator--(int)      { data -= stride; return *this; }
    bool operator==(const this_t& v) const { return data == data; }
    bool operator!=(const this_t& v) const { return data != data; }

};

template<class T>
struct strided_iterator<T, 0>
{
    Boilerplate(strided_iterator, T)

    uint8_t *data;
    size_t stride;

    reference operator*()  { return *(T*)data; }
    pointer   operator->() { return &(T*)data; }
    this_t  operator+(size_t v)  { return data + stride*v; }
    this_t  operator-(size_t v)  { return data + stride*v; }
    this_t& operator+=(size_t v) { data += stride*v; return *this; }
    this_t& operator-=(size_t v) { data -= stride*v; return *this; }
    this_t& operator++()         { data += stride; return *this; }
    this_t& operator++(int)      { data += stride; return *this; }
    this_t& operator--()         { data -= stride; return *this; }
    this_t& operator--(int)      { data -= stride; return *this; }
    bool operator==(const this_t& v) const { return data == data; }
    bool operator!=(const this_t& v) const { return data != data; }
};


template<class VIter, class IIter>
struct indexed_iterator
{
    Boilerplate(indexed_iterator, VIter)

    VIter data;
    IIter index;
    
    reference operator*()       { return data[*index]; }
    pointer   operator->()      { return &data[*index]; }
    this_t  operator+(size_t v) { return { data, index + v }; }
    this_t  operator-(size_t v) { return { data, index - v }; }
    this_t& operator+=(size_t v){ index += v; return *this; }
    this_t& operator-=(size_t v){ index -= v; return *this; }
    this_t& operator++()        { ++index; return *this; }
    this_t& operator++(int)     { ++index; return *this; }
    this_t& operator--()        { --index; return *this; }
    this_t& operator--(int)     { --index; return *this; }
    bool operator==(const this_t& v) const { return v.data == data && v.index == index; }
    bool operator!=(const this_t& v) const { return v.data != data || v.index != index; }
};

template<class VIter, class IIter>
struct indexed_iterator_s
{
    Boilerplate(indexed_iterator_s, VIter)

    VIter data;
    IIter index;

    reference operator*()       { return index ? data[*index] : *data; }
    pointer   operator->()      { return index ? &data[*index] : data; }
    this_t  operator+(size_t v) { return index ? this_t{ data, index + v } : this_t{ data + v, nullptr }; }
    this_t  operator-(size_t v) { return index ? this_t{ data, index - v } : this_t{ data - v, nullptr }; }
    this_t& operator+=(size_t v){ if (index) index += v; else data += v; return *this; }
    this_t& operator-=(size_t v){ if (index) index -= v; else data -= v; return *this; }
    this_t& operator++()        { if (index) ++index; else ++data; return *this; }
    this_t& operator++(int)     { if (index) ++index; else ++data; return *this; }
    this_t& operator--()        { if (index) --index; else --data; return *this; }
    this_t& operator--(int)     { if (index) --index; else --data; return *this; }
    bool operator==(const this_t& v) const { return v.data == data && v.index == index; }
    bool operator!=(const this_t& v) const { return v.data != data || v.index != index; }
};

#undef Boilerplate

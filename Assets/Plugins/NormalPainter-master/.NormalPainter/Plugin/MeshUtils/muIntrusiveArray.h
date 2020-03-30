#pragma once

#include "muIterator.h"

template<class T>
class IntrusiveArray
{
public:
    using value_type = T;
    using reference = T&;
    using const_reference = const T&;
    using pointer = T*;
    using const_pointer = const T*;
    using iterator = pointer;
    using const_iterator = const_pointer;

    IntrusiveArray() {}
    IntrusiveArray(const T *d, size_t s) : m_data(const_cast<T*>(d)), m_size(s) {}
    IntrusiveArray(const IntrusiveArray& v) : m_data(const_cast<T*>(v.m_data)), m_size(v.m_size) {}
    template<int N>
    IntrusiveArray(const T(&v)[N]) : m_data(const_cast<T*>(v)), m_size(N) {}
    template<class Container>
    IntrusiveArray(const Container& v) : m_data(const_cast<T*>(v.data())), m_size(v.size()) {}
    IntrusiveArray& operator=(const IntrusiveArray& v) { m_data = const_cast<T*>(v.m_data); m_size = v.m_size; return *this; }

    void reset(T *d, size_t s)
    {
        m_data = d;
        m_size = s;
    }

    bool empty() const { return m_size == 0; }
    size_t size() const { return m_size; }

    T* data() { return m_data; }
    const T* data() const { return m_data; }

    T& operator[](size_t i) { return m_data[i]; }
    const T& operator[](size_t i) const { return m_data[i]; }

    iterator begin() { return m_data; }
    const_iterator begin() const { return m_data; }
    iterator end() { return m_data + m_size; }
    const_iterator end() const { return m_data + m_size; }

    void zeroclear()
    {
        memset(m_data, 0, sizeof(T)*m_size);
    }
    void copy_to(pointer dst)
    {
        memcpy(dst, m_data, sizeof(value_type) * m_size);
    }
    void copy_to(pointer dst, size_t num_elements)
    {
        memcpy(dst, m_data, sizeof(value_type) * num_elements);
    }

private:
    T *m_data = nullptr;
    size_t m_size = 0;
};


template<class I, class T>
class IntrusiveIndexedArray
{
public:
    using value_type = T;
    using reference = T&;
    using const_reference = const T&;
    using pointer = T*;
    using const_pointer = const T*;
    using iterator = indexed_iterator<T*, const I*>;
    using const_iterator = indexed_iterator<const T*, const I*>;

    IntrusiveIndexedArray() {}
    IntrusiveIndexedArray(const I *i, const T *d, size_t s) : m_index(const_cast<I*>(i)), m_data(const_cast<T*>(d)), m_size(s) {}
    IntrusiveIndexedArray(const IntrusiveIndexedArray& v) : m_index(const_cast<I*>(v.m_index)), m_data(const_cast<T*>(v.m_data)), m_size(v.m_size) {}
    template<class IContainer, class VContainer>
    IntrusiveIndexedArray(const IContainer& i, const VContainer& v) : m_index(const_cast<I*>(i.data())), m_data(const_cast<T*>(v.data())), m_size(i.size()) {}
    IntrusiveIndexedArray& operator=(const IntrusiveIndexedArray& v)
    {
        m_index = const_cast<I*>(v.m_index);
        m_data = const_cast<T*>(v.m_data);
        m_size = v.m_size;
        return *this;
    }

    void reset(I *i, T *d, size_t s)
    {
        m_index = i;
        m_data = d;
        m_size = s;
    }

    bool empty() const { return m_size == 0; }
    size_t size() const { return m_size; }

    I* index() { return m_index; }
    const I* index() const { return m_index; }

    T* data() { return m_data; }
    const T* data() const { return m_data; }

    T& operator[](size_t i) { return m_data[m_index[i]]; }
    const T& operator[](size_t i) const { return m_data[m_index[i]]; }

    iterator begin() { return { m_data, m_index }; }
    const_iterator begin() const { return { m_data, m_index }; }
    iterator end() { return { m_data, m_index + m_size }; }
    const_iterator end() const { return { m_data, m_index + m_size }; }

private:
    I *m_index = nullptr;
    T *m_data = nullptr;
    size_t m_size = 0;
};

template<class T> using IArray = IntrusiveArray<T>;
template<class I, class T> using IIArray = IntrusiveIndexedArray<I, T>;

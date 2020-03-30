#pragma once

#include <algorithm>
#include <initializer_list>
#include "muAllocator.h"

template<class T, int Align = 0x20>
class RawVector
{
public:
    using value_type      = T;
    using reference       = T&;
    using const_reference = const T&;
    using pointer         = T*;
    using const_pointer   = const T*;
    using iterator        = pointer;
    using const_iterator  = const_pointer;
    static const int alignment = Align;

    RawVector() {}
    RawVector(const RawVector& v)
    {
        operator=(v);
    }
    RawVector(RawVector&& v)
    {
        v.swap(*this);
    }
    RawVector(std::initializer_list<T> v)
    {
        operator=(v);
    }
    explicit RawVector(size_t initial_size) { resize(initial_size); }
    RawVector& operator=(const RawVector& v)
    {
        assign(v.begin(), v.end());
        return *this;
    }
    RawVector& operator=(RawVector&& v)
    {
        v.swap(*this);
        return *this;
    }
    RawVector& operator=(std::initializer_list<T> v)
    {
        assign(v.begin(), v.end());
        return *this;
    }

    ~RawVector()
    {
        clear();
        shrink_to_fit();
    }

    bool empty() const { return m_size == 0; }
    size_t size() const { return m_size; }
    size_t capacity() const { return m_capacity; }

    T* data() { return m_data; }
    const T* data() const { return m_data; }
    const T* cdata() const { return m_data; }

    T& at(size_t i) { return m_data[i]; }
    const T& at(size_t i) const { return m_data[i]; }
    T& operator[](size_t i) { return at(i); }
    const T& operator[](size_t i) const { return at(i); }

    T& front() { return m_data[0]; }
    const T& front() const { return m_data[0]; }
    T& back() { return m_data[m_size - 1]; }
    const T& back() const { return m_data[m_size - 1]; }

    iterator begin() { return m_data; }
    const_iterator begin() const { return m_data; }
    iterator end() { return m_data + m_size; }
    const_iterator end() const { return m_data + m_size; }

    static void* allocate(size_t size) { return AlignedMalloc(size, alignment); }
    static void deallocate(void *addr, size_t /*size*/) { AlignedFree(addr); }

    void reserve(size_t s)
    {
        if (s > m_capacity) {
            s = std::max<size_t>(s, m_size * 2);
            size_t newsize = sizeof(T) * s;
            size_t oldsize = sizeof(T) * m_size;

            T *newdata = (T*)allocate(newsize);
            memcpy(newdata, m_data, oldsize);
            deallocate(m_data, oldsize);
            m_data = newdata;
            m_capacity = s;
        }
    }

    void reserve_discard(size_t s)
    {
        if (s > m_capacity) {
            s = std::max<size_t>(s, m_size * 2);
            size_t newsize = sizeof(T) * s;
            size_t oldsize = sizeof(T) * m_size;

            deallocate(m_data, oldsize);
            m_data = (T*)allocate(newsize);
            m_capacity = s;
        }
    }

    void shrink_to_fit()
    {
        if (m_size == 0) {
            deallocate(m_data, m_size);
            m_size = m_capacity = 0;
        }
        else if (m_size == m_capacity) {
            // nothing to do
            return;
        }
        else {
            size_t newsize = sizeof(T) * m_size;
            size_t oldsize = sizeof(T) * m_capacity;
            T *newdata = (T*)allocate(newsize);
            memcpy(newdata, m_data, newsize);
            deallocate(m_data, oldsize);
            m_data = newdata;
            m_capacity = m_size;
        }
    }

    void resize(size_t s)
    {
        reserve(s);
        m_size = s;
    }

    void resize_discard(size_t s)
    {
        reserve_discard(s);
        m_size = s;
    }

    void resize_zeroclear(size_t s)
    {
        resize_discard(s);
        zeroclear();
    }

    void resize(size_t s, const T& v)
    {
        size_t pos = size();
        resize(s);
        // std::fill() can be significantly slower than plain copy
        for (size_t i = pos; i < s; ++i) {
            m_data[i] = v;
        }
    }

    void clear()
    {
        m_size = 0;
    }

    void swap(RawVector &other)
    {
        std::swap(m_data, other.m_data);
        std::swap(m_size, other.m_size);
        std::swap(m_capacity, other.m_capacity);
    }

    template<class FwdIter>
    void assign(FwdIter first, FwdIter last)
    {
        resize(std::distance(first, last));
        std::copy(first, last, begin());
    }
    void assign(const_pointer first, const_pointer last)
    {
        resize(std::distance(first, last));
        // sadly, memcpy() can way faster than std::copy()
        memcpy(m_data, first, sizeof(value_type) * m_size);
    }

    template<class ForwardIter>
    void insert(iterator pos, ForwardIter first, ForwardIter last)
    {
        size_t d = std::distance(begin(), pos);
        size_t s = std::distance(first, last);
        resize(d + s);
        std::copy(first, last, begin() + pos);
    }
    void insert(iterator pos, const_pointer first, const_pointer last)
    {
        size_t d = std::distance(begin(), pos);
        size_t s = std::distance(first, last);
        resize(d + s);
        memcpy(m_data + d, first, sizeof(value_type) * s);
    }

    void insert(iterator pos, const_reference v)
    {
        insert(pos, &v, &v + 1);
    }

    void erase(iterator first, iterator last)
    {
        size_t s = std::distance(first, last);
        std::copy(last, end(), first);
        m_size -= s;
    }

    void erase(iterator pos)
    {
        erase(pos, pos + 1);
    }

    void push_back(const T& v)
    {
        resize(m_size + 1);
        back() = v;
    }
    void push_back(T&& v)
    {
        resize(m_size + 1);
        back() = v;
    }


    void pop_back()
    {
        --m_size;
    }

    bool operator == (const RawVector& other) const
    {
        return m_size == other.m_size && memcmp(m_data, other.m_data, sizeof(T)*m_size) == 0;
    }

    bool operator != (const RawVector& other) const
    {
        return !(*this == other);
    }

    void zeroclear()
    {
        memset(m_data, 0, sizeof(T)*m_size);
    }

    void copy_to(pointer dst)
    {
        memcpy(dst, m_data, sizeof(value_type) * m_size);
    }
    void copy_to(pointer dst, size_t length, size_t offset = 0)
    {
        memcpy(dst, m_data + offset, sizeof(value_type) * length);
    }

private:
    T *m_data = nullptr;
    size_t m_size = 0;
    size_t m_capacity = 0;
};

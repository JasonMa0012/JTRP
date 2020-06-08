#pragma once

#include <vector>
#include <mutex>
#if _WIN32
    #define NOMINMAX
    #include <windows.h>
#else
    #include <pthread.h>
#endif


class tls_base
{
public:
#if _WIN32
    using tls_key_t = DWORD;
#if WindowsStoreApp
    tls_base() { m_key = ::FlsAlloc(nullptr); }
    ~tls_base() { ::FlsFree(m_key); }
    void set_value(void *value) { ::FlsSetValue(m_key, value); }
    void* get_value() { return (void *)::FlsGetValue(m_key); }
#else
    tls_base() { m_key = ::TlsAlloc(); }
    ~tls_base() { ::TlsFree(m_key); }
    void set_value(void *value) { ::TlsSetValue(m_key, value); }
    void* get_value() const { return (void *)::TlsGetValue(m_key); }
#endif
#else
    using tls_key_t = pthread_key_t;
    tls_base() { pthread_key_create(&m_key, nullptr); }
    ~tls_base() { pthread_key_delete(m_key); }
    void set_value(void *value) { pthread_setspecific(m_key, value); }
    void* get_value() const { return pthread_getspecific(m_key); }
#endif
private:
    tls_key_t m_key;
};

template<class T>
class tls : private tls_base
{
public:
    tls() {}

    ~tls()
    {
        std::unique_lock<std::mutex> lock(m_mutex);
        for (auto p : m_locals) { delete p; }
        m_locals.clear();
    }

    T& local()
    {
        return local([](T&) {});
    }

    template<class OnFirst>
    T& local(const OnFirst& on_first)
    {
        void *value = get_value();
        if (value == nullptr) {
            T *v = new T();
            set_value(v);
            value = v;

            {
                std::unique_lock<std::mutex> lock(m_mutex);
                m_locals.push_back(v);
            }
            on_first(*v);
        }
        return *(T*)value;
    }

    template<class Body>
    void each(const Body& body)
    {
        std::unique_lock<std::mutex> lock(m_mutex);
        for (auto p : m_locals) { body(*p); }
    }

protected:
    std::mutex m_mutex;
    std::vector<T*> m_locals;
};

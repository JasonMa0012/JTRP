#include "pch.h"
#include "muAllocator.h"

void* AlignedMalloc(size_t size, size_t alignment)
{
    size_t mask = alignment - 1;
    size = (size + mask) & (~mask);
#ifdef _WIN32
    return _mm_malloc(size, alignment);
#else
    void *ret = nullptr;
    posix_memalign(&ret, alignment, size);
    return ret;
#endif
}

void AlignedFree(void *addr)
{
#ifdef _WIN32
    _mm_free(addr);
#else
    free(addr);
#endif
}

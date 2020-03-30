#pragma once

#include "muConfig.h"
#if defined(muEnablePPL)
    #include <ppl.h>
#elif defined(muEnableTBB)
    #include <tbb/tbb.h>
#endif

namespace mu {

template<class Index, class Body>
inline void parallel_for(Index begin, Index end, const Body& body)
{
#if defined(muEnablePPL)
    concurrency::parallel_for(begin, end, body);
#elif defined(muEnableTBB)
    tbb::parallel_for(begin, end, body);
#else
    for (; begin != end; ++begin) { body(begin); }
#endif
}

#if defined(muEnablePPL) || defined(muEnableTBB)
template<class Body>
inline void parallel_for_blocked(int begin, int end, int granularity, const Body& body)
{
    int num_elements = end - begin;
    int num_blocks = ceildiv(num_elements, granularity);
    parallel_for(0, num_blocks, [&](int i) {
        int begin = granularity * i;
        int end = std::min<int>(granularity * (i + 1), num_elements);
        body(begin, end);
    });
}
#else
template<class Body>
inline void parallel_for_blocked(int begin, int end, int /*granularity*/, const Body& body)
{
    body(begin, end);
}
#endif

template<class Iter, class Body>
inline void parallel_for_each(Iter begin, Iter end, const Body& body)
{
#if defined(muEnablePPL)
    concurrency::parallel_for_each(begin, end, body);
#elif defined(muEnableTBB)
    tbb::parallel_for_each(begin, end, body);
#else
    for (; begin != end; ++begin) { body(*begin); }
#endif
}


#if defined(muEnablePPL)

template <class... Bodies>
inline void parallel_invoke(Bodies... bodies) { concurrency::parallel_invoke(bodies...); }

#elif defined(muEnableTBB)

template <class... Bodies>
inline void parallel_invoke(Bodies... bodies) { tbb::parallel_invoke(bodies...); }

#else

template <class Body>
inline void parallel_invoke(const Body& body) { body(); }

template<typename Body, typename... Args>
void parallel_invoke(const Body& first, Args... args) {
    first();
    parallel_invoke(args...);
}

#endif

} // namespace ms


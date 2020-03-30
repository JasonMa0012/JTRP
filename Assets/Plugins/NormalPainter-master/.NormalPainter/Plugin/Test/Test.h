#pragma once

#include "MeshUtils/MeshUtils.h"
using namespace mu;

#ifdef _WIN32
    #define testExport extern "C" __declspec(dllexport)
#else
    #define testExport extern "C"
#endif
void RegisterTestEntryImpl(const char *name, const std::function<void()>& body);
void PrintImpl(const char *format, ...);


#define Print(...) PrintImpl(__VA_ARGS__)

#define RegisterTestEntry(Name)\
    struct Register##Name {\
        Register##Name() { RegisterTestEntryImpl(#Name, Name); }\
    } g_Register##Name;


#define TestCase(Name) testExport void Name(); RegisterTestEntry(Name); testExport void Name()

template<class Body>
inline void TestScope(const char *name, const Body& body, int num_try = 1)
{
    auto begin = Now();
    for (int i = 0; i < num_try; ++i)
        body();
    auto end = Now();

    float elapsed = NS2MS(end - begin);
    Print("    %s: %.2fms", name, elapsed / num_try);
    if (num_try > 1) {
        Print(" (%.2fms in total)", elapsed);
    }
    Print("\n");
}
